using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Weapon";

        [Header("Combat Stats")]
        public int damage = 1;
        [Tooltip("Minimum time between attacks (fire interval).")]
        [Min(0.05f)] public float attackCooldown = 0.35f;
        [Tooltip("How long the melee HurtBox stays active during a swing.")]
        [Min(0.01f)] public float hurtboxActiveDuration = 0.15f;

        [Header("Ammo")]
        [Min(1)] public int maxAmmo = 3;
        [Tooltip("Seconds to regenerate one ammo while not full.")]
        [Min(0.05f)] public float reloadTimePerAmmo = 1.2f;
    }
}
