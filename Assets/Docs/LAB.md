# OOP Lab — Prefab, ScriptableObject, Inheritance

## Goal
Clear the field: find and defeat every enemy. Controls: **WASD** move, **hold Left Mouse** to attack (ammo-limited), **E** swap weapon with one on the ground, **Space** pause / resume.

## Project layout

```
Assets/
├── Scripts/                  ← Teacher-provided game code (do not rewrite for the lab)
│   ├── Player/
│   ├── Enemy/
│   ├── Combat/               ← Health, Weapon, Projectile, Data class definitions
│   ├── Visuals/
│   └── Optional/             ← Legacy / advanced (not required)
├── EducationalContent/       ← Prefab templates + SO assets to copy / assign
│   ├── Prefabs/
│   └── Data/
├── Docs/
├── Editor/
└── Scenes/
```

## Teacher setup (once)
1. Open Unity and wait for scripts to compile.
2. Menu: **NTHUGDC → Setup Educational Content**
3. This creates `Assets/EducationalContent/` with:
   - `WeaponData_Pistol`, `WeaponData_Axe`, `ProjectileData_Bullet`, `EnemyData_Basic`
   - Prefabs: `Player`, `EnemyTemplate`, `WeaponTemplate_Ranged`, `WeaponTemplate_Melee`, `Projectile_Bullet`, `UISegment`, `UIs`, `GameMenus`
4. Drag `Player` into `BaseScene`. Place a few `EnemyTemplate` instances and a few weapon prefabs on the ground.
5. Replace placeholder sprites with Kenney desert-shooter art on each Prefab’s `Sprite` child.
6. Setup also places `GameMenus` + `EventSystem` into `BaseScene` (pause / game over Overlay). Or run **NTHUGDC → Setup Game Menus In BaseScene**.

## Hierarchy rules

### Enemy
```
Enemy
├── UIs             (shared HUD Prefab: Health + Ammo bars)
├── Sprite          (+ VelocityWaddleTween)
├── AggroZone       (CircleCollider2D trigger)
└── WeaponPivot     (+ WeaponPivot; rotates toward player when Aggro)
    └── Weapon      (your chosen weapon prefab instance)
        ├── Sprite
        └── HurtBox
```

### Player
```
Player
├── UIs             (same shared HUD Prefab)
├── Sprite          (+ VelocityWaddleTween)
└── WeaponPivot     (+ WeaponPivot; rotates toward mouse)
    └── Weapon
        ├── Sprite
        └── HurtBox
```

### Weapon
```
Weapon
├── Sprite
└── HurtBox         (melee: enabled only during swing; ranged: leave disabled)
```

`WeaponPivot` owns rotation; the weapon keeps a local `holdOffset` under the pivot.

## Combat system — data flow

This section explains **who calls whom** and **where numbers come from**. You do not need to rewrite these scripts for the lab, but understanding the flow helps you place Prefabs and ScriptableObjects correctly.

### Big picture

```mermaid
flowchart TB
  subgraph input [Input_and_AI]
    PlayerAttacker[PlayerAttacker left click]
    EnemyController[EnemyController Aggro]
  end

  subgraph weapons [Weapon_layer]
    WeaponBase["Weapon.Attack direction"]
    Ranged[RangedWeapon]
    Melee[MeleeWeapon]
  end

  subgraph data [ScriptableObjects]
    WD[WeaponData]
    PD[ProjectileData]
    ED[EnemyData]
  end

  subgraph hit [Hit_and_Health]
    Proj[ProjectileMover plus ProjectileHit]
    HurtBox[HurtBox]
    Damager[Damager]
    HealthComp[Health]
  end

  PlayerAttacker --> WeaponBase
  EnemyController --> WeaponBase
  WeaponBase --> Ranged
  WeaponBase --> Melee
  WD -->|damage cooldown hurtboxDuration| WeaponBase
  PD -->|prefab speed lifetime| Ranged
  ED -->|maxHealth moveSpeed aggroRange| EnemyController
  Ranged -->|Instantiate| Proj
  Melee -->|enable briefly| HurtBox
  Proj --> Damager
  HurtBox --> Damager
  Damager --> HealthComp
```

**Read it this way:**
1. Something decides to attack (player click, or enemy AI).
2. It calls the same API: `Weapon.Attack(direction, target)`.
3. `Weapon` checks **ammo + fire interval** from `WeaponData`, then calls the subclass.
4. Subclass either spawns a bullet or opens a melee HurtBox.
5. Damage finally reaches `Health` through `Damager` (and a **target tag** filter).

### Who owns which numbers?

```mermaid
flowchart LR
  subgraph so [ScriptableObject_assets]
    WD2[WeaponData]
    PD2[ProjectileData]
    ED2[EnemyData]
  end

  subgraph comps [Runtime_components]
    WComp[Weapon]
    RW[RangedWeapon]
    EC[EnemyController]
    HP[Health]
    AZ[AggroZone]
    EM[EnemyMovement]
  end

  WD2 -->|damage cooldown| WComp
  WD2 -->|hurtboxActiveDuration| MeleeW[MeleeWeapon]
  PD2 -->|only here| RW
  ED2 -->|maxHealth| HP
  ED2 -->|aggroRange| AZ
  ED2 -->|moveSpeed| EM
```

| Data | Lives on | Used by |
|------|----------|---------|
| damage, attackCooldown, maxAmmo, reloadTimePerAmmo | `WeaponData` | any `Weapon` |
| hurtboxActiveDuration | `WeaponData` | `MeleeWeapon` only |
| prefab, speed, lifetime | `ProjectileData` | `RangedWeapon` component field (not on WeaponData) |
| maxHealth, moveSpeed, aggroRange | `EnemyData` | `EnemyController` → Health / Movement / AggroZone |

Sprites and hierarchy stay on **Prefabs**. SO assets only store **numbers and references**.

### Ranged attack sequence

```mermaid
sequenceDiagram
  participant Click as PlayerAttacker
  participant Aim as PlayerAim
  participant W as RangedWeapon
  participant WD as WeaponData
  participant PD as ProjectileData
  participant Owner as WeaponOwner
  participant Bullet as ProjectileHit
  participant HP as Health

  Click->>Aim: read AimDirection
  Click->>W: Attack(direction)
  W->>WD: check ammo and attackCooldown
  W->>W: consume 1 ammo and reset reload timer
  W->>PD: read prefab speed lifetime
  W->>Owner: read ValidTargetTag
  W->>Bullet: Instantiate prefab
  W->>Bullet: Initialize(tag, damage from WD)
  Note over Bullet: flies via ProjectileMover
  Bullet->>HP: OnTriggerEnter2D if tag matches
```

Same path for enemies: `EnemyController` calls `Attack` toward the player; `WeaponOwner` has already set `ValidTargetTag = "Player"`, so enemy bullets hit the player.

### Melee attack sequence

```mermaid
sequenceDiagram
  participant Caller as Player_or_Enemy
  participant W as MeleeWeapon
  participant WD as WeaponData
  participant Visual as MeleeSwingVisual
  participant HB as HurtBox
  participant HP as Health

  Caller->>W: Attack(direction)
  W->>WD: check ammo and cooldown
  W->>W: consume 1 ammo and reset reload timer
  W->>Visual: PlaySwing local arc
  W->>HB: SetActive true for hurtboxActiveDuration
  Note over HB: swing animation may whiff
  alt HurtBox overlaps valid tag
    HB->>HP: Damager.DealDamage
  else no overlap
    Note over HB,HP: no damage
  end
  W->>HB: SetActive false
```

Important teaching point: **animation ≠ hit**. The swing always plays; damage only happens if the active HurtBox touches a valid target.

### Enemy aggro and chase

```mermaid
stateDiagram-v2
  [*] --> Idle
  Idle --> Aggro: AggroZone detects Player
  Aggro --> Aggro: MoveTowards plus Attack on cooldown
```

```mermaid
flowchart LR
  AZ[AggroZone Trigger] -->|NotifyPlayerDetected| EC[EnemyController]
  EC -->|MoveTowards| EM[EnemyMovement]
  EM -->|linearVelocity| RB[Rigidbody2D]
  EC -->|FaceDirection| Pivot[WeaponPivot]
  EC -->|Attack when IsReady| WeaponNode[Weapon]
```

- `EnemyData.aggroRange` sets the AggroZone circle radius.
- `EnemyData.moveSpeed` is passed into `EnemyMovement.MoveTowards`.
- Once Aggro, the enemy keeps chasing (no de-aggro in this lab).

### Friendly fire filter (`WeaponOwner`)

```mermaid
flowchart TB
  Equip[Equip by Player or Enemy]
  Equip --> Owner[WeaponOwner.ConfigureForHolder]
  Owner -->|holder Player| TagEnemy[ValidTargetTag equals Enemy]
  Owner -->|holder Enemy| TagPlayer[ValidTargetTag equals Player]
  TagEnemy --> HurtOrBullet[HurtBox or ProjectileHit]
  TagPlayer --> HurtOrBullet
  HurtOrBullet -->|CompareTag| Deal[DealDamage]
```

When you pick up a weapon with **E**, the same GameObject instance is reparented under the player’s `WeaponPivot` and retagged to hit `"Enemy"`. Dropped weapons become OnGround (non-functioning) until picked up again.

### Inheritance (weapon family)

```mermaid
classDiagram
  class Weapon {
    WeaponData data
    Attack(direction, target)
    IsReady
  }
  class RangedWeapon {
    ProjectileData projectileData
    Instantiate bullet
  }
  class MeleeWeapon {
    enable HurtBox window
    MeleeSwingVisual
  }
  Weapon <|-- RangedWeapon
  Weapon <|-- MeleeWeapon
```

Shared behavior (ammo, fire interval, equip/drop, owner tag) lives in `Weapon`. Different attack behavior lives in subclasses — that is the OOP inheritance lesson of this lab.

### Ammo and hold-to-fire (Brawl Stars style)

Both melee and ranged use the same magazine on `WeaponData`:

| Field | Meaning |
|-------|---------|
| `maxAmmo` | Magazine size (e.g. 3) |
| `reloadTimePerAmmo` | Seconds to regain **one** ammo while not full |
| `attackCooldown` | Minimum time between two attacks |

Runtime state lives on the `Weapon` instance (not in the SO):

- `CurrentAmmo`, `MaxAmmo`, `ReloadProgress01`, `IsAmmoFull`, `DisplayName` — read by the player HUD
- **Equip starts at 0 ammo**; reload regenerates one-by-one while equipped
- Successful `Attack` spends 1 ammo and **resets** the reload timer
- While equipped and not full, `Update` regenerates ammo one-by-one
- **Hold left mouse** to keep attacking whenever ammo and fire interval allow
- Enemies use the same `IsReady` gate: they burst while they have ammo, keep chasing while reloading, then fire again

### Segmented health / ammo HUD

Shared Prefab `EducationalContent/Prefabs/UIs` (World-space Canvas with `Health` + `Ammo` `SegmentedBar` rows) is nested under **Player** and **EnemyTemplate**:

- Each cell is an instance of `UISegment` (nine-sliced frame + inner `Image` fill)
- Segment count matches `Health.MaxValue` / `Weapon.MaxAmmo` and rebuilds when those change (e.g. weapon swap)
- Full segments use `fillAmount = 1`; empty use `0`
- For ammo, the next empty segment uses `ReloadProgress01` so the fill animates while regenerating
- Scripts: `SegmentView`, `SegmentedBar`, `HealthBarDisplay`, `AmmoBarDisplay` under `Assets/Scripts/UI/`
- `HealthBarDisplay` binds the parent character’s `Health`
- `AmmoBarDisplay` binds the character’s `WeaponPivot.CurrentWeapon` (works for both Player and Enemy)
- `Health.OnChanged` notifies the health bar; the ammo bar polls the equipped weapon each frame

Run **NTHUGDC → Setup Educational Content** if `UIs` / `UISegment` wiring is missing.

### Pause / Game Over

Screen-space Overlay Prefab `EducationalContent/Prefabs/GameMenus` (wired by Setup into `BaseScene`):

- **Space** toggles pause (`Time.timeScale` 0/1); Pause panel shows/hides immediately
- **Win**: all enemies defeated (`EnemyRegistry.OnAllEnemiesDefeated`) → Game Over panel fades in with “You Win!”
- **Lose**: player `Health.OnKilledEvent` → same panel with “You Lose...”
- Game Over fade uses `CanvasGroup` + `Time.unscaledDeltaTime` so it still animates at `timeScale = 0`
- Both panels have a **Restart** button (`SceneManager.LoadScene` current scene)
- Top-left **enemy counter** (`EnemyCountDisplay`): label + remaining + `/` + total (from `EnemyRegistry`)
- `GameManager` debug flags (default off): `skipGameOverPanel`, `skipPauseOnGameOver`
- Needs **EventSystem** + Input System UI module for button clicks
- Clone includes TextMesh Pro Essential Resources under `Assets/TextMesh Pro/` (no extra Import step for HUD/menu text)

## Student assignments

### 1. Enemy Prefab variants (≥2)
- Duplicate `EnemyTemplate` under `EducationalContent/Prefabs/`.
- Change the `Sprite` child look.
- Create new **Enemy Data** assets (`Create → Combat → Enemy Data`) and assign different `maxHealth`, `moveSpeed`, `aggroRange`.
- Change which **Weapon** child is attached (melee vs ranged) as part of the enemy design.
- Place them in the scene. Confirm: enemies idle until you enter `AggroZone`, then chase via `EnemyMovement` (Rigidbody2D velocity).

### 2. New Weapon Prefab (≥1)
- Duplicate `WeaponTemplate_Ranged` or `WeaponTemplate_Melee`.
- Create a new **Weapon Data** asset (`Create → Combat → Weapon Data`) — shared stats only (damage, cooldown). Do **not** put sprites in the SO.
- For ranged weapons: also create **Projectile Data** and assign it on the **`RangedWeapon` component** (not on WeaponData). Melee weapons never see this field.
- Assign Weapon Data on the weapon component; tweak `Sprite` / `HurtBox` size on the Prefab.
- Put the weapon in the scene (OnGround). Press **E** near it to swap (same instance becomes non-functioning on the ground).

### 3. Optional inheritance
- Subclass `Weapon` / `RangedWeapon` / `MeleeWeapon` for a new firing pattern, then wrap it in a Prefab students can pick up.

## What ScriptableObjects change
| Asset | Affects |
|-------|---------|
| `WeaponData` | damage, attackCooldown, maxAmmo, reloadTimePerAmmo, hurtbox duration |
| `ProjectileData` | bullet prefab, speed, lifetime — referenced by `RangedWeapon` only |
| `EnemyData` | maxHealth, moveSpeed, aggroRange |

Sprites and hierarchy live on Prefabs. Class definitions live in `Scripts/Combat/Data/`; asset instances live in `EducationalContent/Data/`.

## Key scripts (teacher-provided, under `Assets/Scripts/`)
| Script | Role |
|--------|------|
| `Weapon` / `RangedWeapon` / `MeleeWeapon` | OOP inheritance + ammo / fire interval from `WeaponData` |
| `WeaponOwner` | Sets target tag (Player→Enemy, Enemy→Player); bullets inherit tag |
| `HurtBox` | Melee hit detection while active |
| `EnemyMovement` | Rigidbody2D velocity only (stun/knockback hooks later) |
| `EnemyController` | Idle / Aggro AI |
| `VelocityWaddleTween` | Walk wiggle from velocity magnitude |
| `MeleeSwingVisual` | Coroutine rotation tween (later student task) |
| `SegmentedBar` / `HealthBarDisplay` / `AmmoBarDisplay` | Segmented world-space health & ammo HUD |
| `EnemyCountDisplay` | Screen-space remaining / total enemy counter |
| `GameManager` | Pause (Space), win/lose Game Over UI, Restart |
| `Player*` | Ready-to-use player Prefab |

Legacy RangeDetect / old projectile strategies live under `Assets/Scripts/Optional/` and are not required for the lab.

---
中文版：[`Assets/Docs/LAB_zh-TW.md`](Assets/Docs/LAB_zh-TW.md)
