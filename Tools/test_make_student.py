"""make_student.py 的單元測試。

執行：python -I -m unittest discover -s Tools
"""

import contextlib
import importlib.util
import io
import sys
import tempfile
import unittest
from pathlib import Path

_spec = importlib.util.spec_from_file_location(
    "make_student", Path(__file__).resolve().parent / "make_student.py")
ms = importlib.util.module_from_spec(_spec)
sys.modules.setdefault("make_student", ms)
_spec.loader.exec_module(ms)


def t(src: str) -> str:
    return ms.transform(src.encode("utf-8"), "X.cs").data.decode("utf-8")


class TransformTests(unittest.TestCase):
    def test_basic_block_keeps_else(self):
        src = (
            "void A() {\n"
            "#if LAB_SOLUTION\n"
            "    Solve();\n"
            "#else\n"
            "    // TODO [LAB-H1] do it\n"
            "#endif\n"
            "}\n"
        )
        res = ms.transform(src.encode(), "X.cs")
        self.assertEqual(res.blocks, 1)
        self.assertEqual(res.data.decode(), "void A() {\n    // TODO [LAB-H1] do it\n}\n")

    def test_no_else_removes_whole_block(self):
        src = "class C {\n#if LAB_SOLUTION\n  void M() {}\n#endif\n  int x;\n}\n"
        self.assertEqual(t(src), "class C {\n  int x;\n}\n")

    def test_indented_directives_and_trailing_comments(self):
        src = (
            "  a\n"
            "    #if LAB_SOLUTION // 解答\n"
            "  sol\n"
            "\t#  else   // 學生\n"
            "  stub\n"
            "    #endif // LAB_SOLUTION\n"
            "  b\n"
        )
        self.assertEqual(t(src), "  a\n  stub\n  b\n")

    def test_multiple_blocks(self):
        src = "#if LAB_SOLUTION\n1\n#else\n2\n#endif\nx\n#if LAB_SOLUTION\n3\n#endif\n"
        res = ms.transform(src.encode(), "X.cs")
        self.assertEqual(res.blocks, 2)
        self.assertEqual(res.data.decode(), "2\nx\n")

    def test_crlf_preserved(self):
        src = "a\r\n#if LAB_SOLUTION\r\nsol\r\n#else\r\nstub\r\n#endif\r\nb\r\n"
        out = ms.transform(src.encode(), "X.cs").data
        self.assertEqual(out, b"a\r\nstub\r\nb\r\n")

    def test_mixed_line_endings_preserved(self):
        src = b"a\r\n#if LAB_SOLUTION\nsol\r\n#else\r\nstub\nmore\r\n#endif\nb"
        out = ms.transform(src, "X.cs").data
        self.assertEqual(out, b"a\r\nstub\nmore\r\nb")

    def test_endif_at_eof_without_newline(self):
        src = b"a\n#if LAB_SOLUTION\nsol\n#endif"
        self.assertEqual(ms.transform(src, "X.cs").data, b"a\n")

    def test_bom_preserved(self):
        src = ms.BOM + "a\n#if LAB_SOLUTION\n解\n#else\n樁\n#endif\n".encode()
        out = ms.transform(src, "X.cs").data
        self.assertTrue(out.startswith(ms.BOM))
        self.assertEqual(out, ms.BOM + "a\n樁\n".encode())

    def test_no_bom_not_added(self):
        out = ms.transform(b"#if LAB_SOLUTION\nx\n#endif\ny\n", "X.cs").data
        self.assertEqual(out, b"y\n")

    def test_nested_unity_editor_in_else(self):
        src = (
            "#if LAB_SOLUTION\n"
            "sol\n"
            "#else\n"
            "stub\n"
            "#if UNITY_EDITOR\n"
            "ed\n"
            "#else\n"
            "rt\n"
            "#endif\n"
            "#endif\n"
            "tail\n"
        )
        self.assertEqual(t(src), "stub\n#if UNITY_EDITOR\ned\n#else\nrt\n#endif\ntail\n")

    def test_nested_unity_editor_in_solution(self):
        src = (
            "#if LAB_SOLUTION\n"
            "sol\n"
            "#if UNITY_EDITOR\n"
            "ed\n"
            "#elif UNITY_ANDROID\n"
            "an\n"
            "#else\n"
            "rt\n"
            "#endif\n"
            "#else\n"
            "stub\n"
            "#endif\n"
        )
        self.assertEqual(t(src), "stub\n")

    def test_lab_inside_unity_editor(self):
        src = (
            "#if UNITY_EDITOR\n"
            "#if LAB_SOLUTION\n"
            "sol\n"
            "#else\n"
            "stub\n"
            "#endif\n"
            "#else\n"
            "rt\n"
            "#endif\n"
        )
        self.assertEqual(t(src), "#if UNITY_EDITOR\nstub\n#else\nrt\n#endif\n")

    def test_unrelated_directives_untouched(self):
        src = "#if UNITY_EDITOR\na\n#elif DEBUG\nb\n#else\nc\n#endif\n"
        res = ms.transform(src.encode(), "X.cs")
        self.assertEqual(res.blocks, 0)
        self.assertEqual(res.data.decode(), src)


class TransformErrorTests(unittest.TestCase):
    def assertLabError(self, src: str, *fragments: str):
        with self.assertRaises(ms.LabError) as cm:
            ms.transform(src.encode(), "Foo.cs")
        msg = str(cm.exception)
        for f in fragments:
            self.assertIn(f, msg)

    def test_nested_lab(self):
        self.assertLabError(
            "#if LAB_SOLUTION\n#if LAB_SOLUTION\n#endif\n#endif\n", "Foo.cs:2", "巢狀")

    def test_nested_lab_in_else(self):
        self.assertLabError(
            "#if LAB_SOLUTION\n#else\n#if UNITY_EDITOR\n#if LAB_SOLUTION\n#endif\n#endif\n#endif\n",
            "Foo.cs:4", "巢狀")

    def test_unclosed(self):
        self.assertLabError("x\n#if LAB_SOLUTION\na\n#else\nb\n", "Foo.cs:2", "#endif")

    def test_unclosed_with_inner_if(self):
        self.assertLabError("#if LAB_SOLUTION\n#if A\n#endif\n", "Foo.cs:1")

    def test_stray_endif(self):
        self.assertLabError("#endif\n", "Foo.cs:1")

    def test_stray_else(self):
        self.assertLabError("a\n#else\n", "Foo.cs:2")

    def test_elif_on_lab(self):
        self.assertLabError("#if LAB_SOLUTION\na\n#elif FOO\nb\n#endif\n", "Foo.cs:3", "#elif")

    def test_double_else(self):
        self.assertLabError("#if LAB_SOLUTION\n#else\n#else\n#endif\n", "Foo.cs:3")

    def test_unsupported_condition(self):
        self.assertLabError("#if !LAB_SOLUTION\n#endif\n", "Foo.cs:1")
        self.assertLabError("#if LAB_SOLUTION && UNITY_EDITOR\n#endif\n", "Foo.cs:1")

    def test_not_utf8(self):
        with self.assertRaises(ms.LabError):
            ms.transform(b"#if LAB_SOLUTION\n\xff\xfe\n#endif\n", "Foo.cs")


class EndToEndTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.src = self.root / "repo"
        self.out = self.root / "student"
        s = self.src
        (s / "Assets/Scripts").mkdir(parents=True)
        (s / "Packages").mkdir()
        (s / "ProjectSettings").mkdir()
        (s / "Library").mkdir()
        (s / ".gitignore").write_text("/Library/\n")
        (s / "Library/junk.txt").write_text("junk")
        (s / "Packages/manifest.json").write_text("{}")
        (s / "ProjectSettings/ProjectVersion.txt").write_text("6000.4.1f1")
        (s / "Assets/csc.rsp").write_text("-define:LAB_SOLUTION")
        (s / "Assets/csc.rsp.meta").write_text("guid: x")
        (s / "Assets/Scripts.meta").write_text("guid: s")
        (s / "Assets/Scripts/A.cs").write_bytes(
            ms.BOM + b"class A {\r\n#if LAB_SOLUTION\r\n  sol();\r\n#else\r\n"
                     b"  // TODO [LAB-H2] x\r\n#endif\r\n}\r\n")
        (s / "Assets/Scripts/A.cs.meta").write_text("guid: a")
        (s / "Assets/Scripts/B.cs").write_text(
            "class B {\n  // TODO [LAB-H1] write OnTriggerEnter2D\n"
            "#if LAB_SOLUTION\n  void M() {}\n#endif\n}\n")
        (s / "Assets/Scripts/C.cs").write_text("class C {}\n")

    def tearDown(self):
        self._tmp.cleanup()

    def run_main(self, *args):
        so, se = io.StringIO(), io.StringIO()
        with contextlib.redirect_stdout(so), contextlib.redirect_stderr(se):
            code = ms.main(["--src", str(self.src), *args])
        return code, so.getvalue(), se.getvalue()

    def test_full_run(self):
        self.out.mkdir()
        (self.out / ".git").write_text("gitdir: elsewhere")
        (self.out / "stale.txt").write_text("old")
        (self.out / "Assets/Old").mkdir(parents=True)
        (self.out / "Assets/Old/x.cs").write_text("old")

        code, so, se = self.run_main("--out", str(self.out))
        self.assertEqual(code, 0, se)
        o = self.out
        self.assertEqual((o / ".git").read_text(), "gitdir: elsewhere")
        self.assertFalse((o / "stale.txt").exists())
        self.assertFalse((o / "Assets/Old").exists())
        self.assertFalse((o / "Assets/csc.rsp").exists())
        self.assertFalse((o / "Assets/csc.rsp.meta").exists())
        self.assertFalse((o / "Library").exists())
        self.assertTrue((o / "Assets/Scripts/A.cs.meta").exists())
        self.assertTrue((o / "Assets/Scripts.meta").exists())
        self.assertTrue((o / "Packages/manifest.json").exists())
        self.assertTrue((o / "ProjectSettings/ProjectVersion.txt").exists())
        self.assertTrue((o / ".gitignore").exists())
        self.assertEqual((o / "Assets/Scripts/A.cs").read_bytes(),
                         ms.BOM + b"class A {\r\n  // TODO [LAB-H2] x\r\n}\r\n")
        self.assertEqual((o / "Assets/Scripts/B.cs").read_text(),
                         "class B {\n  // TODO [LAB-H1] write OnTriggerEnter2D\n}\n")
        self.assertEqual((o / "Assets/Scripts/C.cs").read_text(), "class C {}\n")
        self.assertIn("LAB-H1, LAB-H2", so)
        self.assertIn("處理 2 個 .cs 檔", so)

    def test_preserves_git_directory(self):
        (self.out / ".git/objects").mkdir(parents=True)
        (self.out / ".git/HEAD").write_text("ref: refs/heads/student")
        code, _, se = self.run_main("--out", str(self.out))
        self.assertEqual(code, 0, se)
        self.assertEqual((self.out / ".git/HEAD").read_text(), "ref: refs/heads/student")

    def test_dry_run_writes_nothing(self):
        code, so, se = self.run_main("--out", str(self.out), "--dry-run")
        self.assertEqual(code, 0, se)
        self.assertFalse(self.out.exists())
        self.assertIn("Assets/Scripts/A.cs：移除 1 個區塊", so)
        self.assertIn("Assets/Scripts/B.cs：移除 1 個區塊", so)

    def test_rejects_out_inside_repo(self):
        for bad in (self.src, self.src / "Build", self.root):
            code, _, se = self.run_main("--out", str(bad))
            self.assertEqual(code, 2, bad)
            self.assertIn("錯誤", se)
        self.assertTrue((self.src / "Assets/Scripts/A.cs").exists())

    def test_syntax_error_aborts_before_writing(self):
        (self.src / "Assets/Scripts/D.cs").write_text("#if LAB_SOLUTION\nx\n")
        self.out.mkdir()
        (self.out / "keep.txt").write_text("k")
        code, _, se = self.run_main("--out", str(self.out))
        self.assertNotEqual(code, 0)
        self.assertIn("Assets/Scripts/D.cs:1", se)
        self.assertTrue((self.out / "keep.txt").exists())

    def test_leftover_symbol_is_error(self):
        (self.src / "Assets/Readme.md").write_text("define LAB_SOLUTION to see answers")
        code, _, se = self.run_main("--out", str(self.out))
        self.assertNotEqual(code, 0)
        self.assertIn("Assets/Readme.md", se)

    def test_leftover_symbol_in_kept_else_is_error(self):
        (self.src / "Assets/Scripts/E.cs").write_text(
            "#if LAB_SOLUTION\na\n#else\n// LAB_SOLUTION\n#endif\n")
        code, _, se = self.run_main("--out", str(self.out))
        self.assertNotEqual(code, 0)
        self.assertIn("Assets/Scripts/E.cs", se)

    def test_scan_leftovers_after_write(self):
        self.out.mkdir()
        (self.out / ".git").write_text("LAB_SOLUTION")  # .git 不檢查
        (self.out / "sub").mkdir()
        (self.out / "sub/x.txt").write_text("LAB_SOLUTION")
        self.assertEqual(ms.scan_leftovers(self.out), ["sub/x.txt"])


if __name__ == "__main__":
    unittest.main()
