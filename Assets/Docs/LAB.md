# OOP Lab — Prefab, ScriptableObject, Inheritance

## Goal
Clear the field: find and defeat every enemy. Controls: **WASD** move, **Left Mouse** attack, **E** swap weapon with one on the ground.

## Teacher setup (once)
1. Open Unity and wait for scripts to compile.
2. Menu: **NTHUGDC → Setup Educational Content**
3. This creates `Assets/EducationalContent/` with:
   - `WeaponData_Pistol`, `WeaponData_Axe`, `EnemyData_Basic`
   - Prefabs: `Player`, `EnemyTemplate`, `WeaponTemplate_Ranged`, `WeaponTemplate_Melee`, `Projectile_Bullet`
4. Drag `Player` into `BaseScene`. Place a few `EnemyTemplate` instances and a few weapon prefabs on the ground.
5. Replace placeholder sprites with Kenney desert-shooter art on each Prefab’s `Sprite` child.

## Hierarchy rules

### Enemy
```
Enemy
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
## Student assignments

### 1. Enemy Prefab variants (≥2)
- Duplicate `EnemyTemplate`.
- Change the `Sprite` child look.
- Create new **Enemy Data** assets (`Create → Combat → Enemy Data`) and assign different `maxHealth`, `moveSpeed`, `aggroRange`.
- Change which **Weapon** child is attached (melee vs ranged) as part of the enemy design.
- Place them in the scene. Confirm: enemies idle until you enter `AggroZone`, then chase via `EnemyMovement` (Rigidbody2D velocity).

### 2. New Weapon Prefab (≥1)
- Duplicate `WeaponTemplate_Ranged` or `WeaponTemplate_Melee`.
- Create a new **Weapon Data** asset (`Create → Combat → Weapon Data`) — stats only (damage, cooldown, projectile refs). Do **not** put sprites in the SO.
- Assign the data on the weapon component; tweak `Sprite` / `HurtBox` size on the Prefab.
- Put the weapon in the scene (OnGround). Press **E** near it to swap (same instance becomes non-functioning on the ground).

### 3. Optional inheritance
- Subclass `Weapon` / `RangedWeapon` / `MeleeWeapon` for a new firing pattern, then wrap it in a Prefab students can pick up.

## What ScriptableObjects change
| Asset | Affects |
|-------|---------|
| `WeaponData` | damage, attackCooldown, hurtbox duration, projectile prefab/speed |
| `EnemyData` | maxHealth, moveSpeed, aggroRange |

Sprites and hierarchy live on Prefabs.

## Key scripts (teacher-provided)
| Script | Role |
|--------|------|
| `Weapon` / `RangedWeapon` / `MeleeWeapon` | OOP inheritance + cooldown from `WeaponData` |
| `WeaponOwner` | Sets target tag (Player→Enemy, Enemy→Player); bullets inherit tag |
| `HurtBox` | Melee hit detection while active |
| `EnemyMovement` | Rigidbody2D velocity only (stun/knockback hooks later) |
| `EnemyController` | Idle / Aggro AI |
| `VelocityWaddleTween` | Walk wiggle from velocity magnitude |
| `MeleeSwingVisual` | Coroutine rotation tween (later student task) |
| `Player*` | Ready-to-use player Prefab |

Legacy RangeDetect / old projectile strategies live under `Assets/Combat/Optional/` and are not required for the lab.
