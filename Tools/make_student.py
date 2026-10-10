#!/usr/bin/env python3
"""產生學生版（題目版）專案。

用法：
    python Tools/make_student.py --out <dir> [--dry-run]

做的事：
  1. 從 repo 根目錄複製 Assets/（含 .meta）、Packages/、ProjectSettings/、.gitignore 到 <dir>。
     複製前會清空 <dir>，但保留 <dir>/.git（可能是 git worktree 的 .git 檔案）。
  2. Assets/**/*.cs 裡的
         #if LAB_SOLUTION
             解答
         #else
             TODO 樁
         #endif
     只保留 #else 段；沒有 #else 就整段刪掉。
     巢狀 #if LAB_SOLUTION、沒配對、在 LAB_SOLUTION 那層用 #elif 都會報錯。
  3. 不複製 Assets/csc.rsp 與 Assets/csc.rsp.meta（教師版用它定義 LAB_SOLUTION）。
  4. 輸出完成後若任何檔案仍含有 "LAB_SOLUTION" 字串就報錯。

只使用標準函式庫，需要 Python 3.10+。
"""

from __future__ import annotations

import argparse
import os
import re
import shutil
import stat
import sys
from dataclasses import dataclass, field
from pathlib import Path

SYMBOL = "LAB_SOLUTION"
BOM = b"\xef\xbb\xbf"

COPY_DIRS = ("Assets", "Packages", "ProjectSettings")
COPY_FILES = (".gitignore",)
EXCLUDED = {"Assets/csc.rsp", "Assets/csc.rsp.meta"}
PRESERVED_IN_OUT = {".git"}

DIRECTIVE_RE = re.compile(r"^[ \t]*#[ \t]*(if|elif|else|endif)\b(.*)$")
HOLE_RE = re.compile(r"TODO\s*\[LAB-(H[0-9A-Za-z]+)\]")


class LabError(Exception):
    """原始碼裡的 LAB_SOLUTION 區塊格式錯誤。"""


@dataclass
class TransformResult:
    data: bytes
    blocks: int


@dataclass
class Plan:
    copies: list[tuple[Path, str]] = field(default_factory=list)  # (來源, 相對路徑)
    outputs: dict[str, bytes] = field(default_factory=dict)  # 改寫過的 .cs
    blocks: dict[str, int] = field(default_factory=dict)  # 相對路徑 -> 移除的區塊數
    holes: set[str] = field(default_factory=set)
    errors: list[str] = field(default_factory=list)


def _condition(rest: str) -> str:
    """去掉指令後面的行尾註解與空白，留下條件本身。"""
    idx = rest.find("//")
    if idx >= 0:
        rest = rest[:idx]
    return rest.strip()


def transform(data: bytes, name: str = "<input>") -> TransformResult:
    """把 #if LAB_SOLUTION 區塊換成 #else 段。保留 BOM 與每行原本的換行符號。"""
    has_bom = data.startswith(BOM)
    body = data[len(BOM):] if has_bom else data
    try:
        text = body.decode("utf-8")
    except UnicodeDecodeError as e:
        raise LabError(f"{name}: 不是 UTF-8 編碼（{e}）") from None

    # 每段都保留自己的 '\n'（以及前面的 '\r'），原樣接回去就不會改到換行符號。
    lines = re.split(r"(?<=\n)", text)

    out: list[str] = []
    # 堆疊中每一層是 [is_lab, 開始行號, 是否已經到 #else]
    stack: list[list] = []
    lab_frame: list | None = None
    blocks = 0

    def dropping() -> bool:
        return lab_frame is not None and not lab_frame[2]

    for lineno, line in enumerate(lines, start=1):
        m = DIRECTIVE_RE.match(line.rstrip("\r\n"))
        if m is None:
            if not dropping():
                out.append(line)
            continue

        kind, cond = m.group(1), _condition(m.group(2))
        where = f"{name}:{lineno}"

        if kind == "if":
            if cond == SYMBOL:
                if lab_frame is not None:
                    raise LabError(
                        f"{where}: 巢狀的 #if {SYMBOL}（外層在第 {lab_frame[1]} 行）")
                lab_frame = [True, lineno, False]
                stack.append(lab_frame)
                continue
            if SYMBOL in cond:
                raise LabError(f"{where}: 不支援的條件 '#if {cond}'，只能寫 '#if {SYMBOL}'")
            stack.append([False, lineno, False])
        elif kind == "elif":
            if not stack:
                raise LabError(f"{where}: #elif 沒有對應的 #if")
            if stack[-1][0]:
                raise LabError(f"{where}: {SYMBOL} 區塊不能用 #elif（區塊從第 {stack[-1][1]} 行開始）")
            if SYMBOL in cond:
                raise LabError(f"{where}: 不支援的條件 '#elif {cond}'")
        elif kind == "else":
            if not stack:
                raise LabError(f"{where}: #else 沒有對應的 #if")
            top = stack[-1]
            if top[0]:
                if top[2]:
                    raise LabError(f"{where}: {SYMBOL} 區塊有兩個 #else")
                top[2] = True
                continue
            top[2] = True
        else:  # endif
            if not stack:
                raise LabError(f"{where}: #endif 沒有對應的 #if")
            top = stack.pop()
            if top[0]:
                lab_frame = None
                blocks += 1
                continue

        # 與 LAB_SOLUTION 無關的指令：在解答段內就跟著刪，否則保留。
        if not dropping():
            out.append(line)

    if lab_frame is not None:
        raise LabError(f"{name}:{lab_frame[1]}: #if {SYMBOL} 沒有對應的 #endif")

    result = "".join(out).encode("utf-8")
    return TransformResult((BOM + result) if has_bom else result, blocks)


def _iter_files(root: Path):
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames.sort()
        for fn in sorted(filenames):
            yield Path(dirpath) / fn


def build_plan(src: Path) -> Plan:
    plan = Plan()
    for d in COPY_DIRS:
        base = src / d
        if not base.is_dir():
            plan.errors.append(f"找不到資料夾：{base}")
            continue
        for p in _iter_files(base):
            rel = p.relative_to(src).as_posix()
            if rel in EXCLUDED:
                continue
            plan.copies.append((p, rel))
    for f in COPY_FILES:
        p = src / f
        if p.is_file():
            plan.copies.append((p, f))

    for p, rel in plan.copies:
        if not (rel.startswith("Assets/") and rel.endswith(".cs")):
            continue
        data = p.read_bytes()
        if SYMBOL.encode() in data:
            try:
                res = transform(data, rel)
            except LabError as e:
                plan.errors.append(str(e))
                continue
            if res.blocks:
                plan.blocks[rel] = res.blocks
            plan.outputs[rel] = res.data
            data = res.data
        try:
            plan.holes.update(HOLE_RE.findall(data.decode("utf-8-sig", errors="replace")))
        except Exception:
            pass
    return plan


def leftover_in_plan(plan: Plan) -> list[str]:
    """事先檢查：輸出後會不會還有 LAB_SOLUTION。"""
    bad = []
    needle = SYMBOL.encode()
    for p, rel in plan.copies:
        data = plan.outputs.get(rel)
        if data is None:
            data = p.read_bytes()
        if needle in data:
            bad.append(rel)
    return bad


def scan_leftovers(out: Path) -> list[str]:
    bad = []
    needle = SYMBOL.encode()
    for dirpath, dirnames, filenames in os.walk(out):
        if Path(dirpath) == out:
            dirnames[:] = [d for d in dirnames if d not in PRESERVED_IN_OUT]
        for fn in filenames:
            p = Path(dirpath) / fn
            if Path(dirpath) == out and fn in PRESERVED_IN_OUT:
                continue
            if needle in p.read_bytes():
                bad.append(p.relative_to(out).as_posix())
    return sorted(bad)


def _force_remove(func, path, _exc):
    # Windows 上唯讀檔（例如 git 物件）要先拿掉唯讀屬性才能刪。
    os.chmod(path, stat.S_IWRITE)
    func(path)


def _rmtree(path: Path) -> None:
    if sys.version_info >= (3, 12):
        shutil.rmtree(path, onexc=_force_remove)
    else:
        shutil.rmtree(path, onerror=_force_remove)


def clear_out_dir(out: Path) -> None:
    for child in out.iterdir():
        if child.name in PRESERVED_IN_OUT:
            continue
        if child.is_dir() and not child.is_symlink():
            _rmtree(child)
        else:
            try:
                child.unlink()
            except PermissionError:
                os.chmod(child, stat.S_IWRITE)
                child.unlink()


def check_out_dir(src: Path, out: Path) -> str | None:
    if out == src:
        return f"--out 不能是 repo 本身：{out}"
    if out.is_relative_to(src):
        return f"--out 不能在 repo 裡面：{out}"
    if src.is_relative_to(out):
        return f"--out 不能是 repo 的上層資料夾（會把 repo 清掉）：{out}"
    if out.exists() and not out.is_dir():
        return f"--out 已存在但不是資料夾：{out}"
    return None


def _hole_key(h: str):
    m = re.match(r"H(\d+)(.*)", h)
    return (int(m.group(1)), m.group(2)) if m else (10**9, h)


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description="產生學生版（移除 #if LAB_SOLUTION 解答段）")
    ap.add_argument("--out", required=True, help="輸出資料夾（例如 student 分支的 worktree）")
    ap.add_argument("--dry-run", action="store_true", help="只列出會處理的檔案，不寫入")
    ap.add_argument("--src", default=str(Path(__file__).resolve().parent.parent),
                    help=argparse.SUPPRESS)  # 測試用；預設是這個檔案所在的 repo
    args = ap.parse_args(argv)

    src = Path(args.src).resolve()
    out = Path(args.out).resolve()

    err = check_out_dir(src, out)
    if err:
        print(f"錯誤：{err}", file=sys.stderr)
        return 2

    plan = build_plan(src)
    if plan.errors:
        for e in plan.errors:
            print(f"錯誤：{e}", file=sys.stderr)
        return 1

    bad = leftover_in_plan(plan)
    if bad:
        for rel in bad:
            print(f"錯誤：輸出後仍會含有 '{SYMBOL}'：{rel}", file=sys.stderr)
        return 1

    holes = sorted(plan.holes, key=_hole_key)
    total_blocks = sum(plan.blocks.values())

    if args.dry_run:
        print(f"[dry-run] 來源：{src}")
        print(f"[dry-run] 輸出：{out}（不會寫入）")
        for rel in sorted(plan.blocks):
            print(f"  {rel}：移除 {plan.blocks[rel]} 個區塊")
    else:
        out.mkdir(parents=True, exist_ok=True)
        clear_out_dir(out)
        for p, rel in plan.copies:
            dst = out / rel
            dst.parent.mkdir(parents=True, exist_ok=True)
            data = plan.outputs.get(rel)
            if data is None:
                shutil.copy2(p, dst)
            else:
                dst.write_bytes(data)
                shutil.copystat(p, dst)
        bad = scan_leftovers(out)
        if bad:
            for rel in bad:
                print(f"錯誤：輸出中仍含有 '{SYMBOL}'：{rel}", file=sys.stderr)
            return 1
        print(f"已輸出到：{out}")
        for rel in sorted(plan.blocks):
            print(f"  {rel}：移除 {plan.blocks[rel]} 個區塊")

    print(f"摘要：複製 {len(plan.copies)} 個檔案；處理 {len(plan.blocks)} 個 .cs 檔，"
          f"移除 {total_blocks} 個解答區塊；洞 {len(holes)} 個："
          f"{', '.join('LAB-' + h for h in holes) if holes else '（無）'}")
    return 0


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
        sys.stderr.reconfigure(encoding="utf-8")
    sys.exit(main())
