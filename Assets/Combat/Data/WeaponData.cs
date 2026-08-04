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
        [Min(0.05f)] public float attackCooldown = 0.35f;
        [Tooltip("How long the melee HurtBox stays active during a swing.")]
        [Min(0.01f)] public float hurtboxActiveDuration = 0.15f;

        [Header("Ranged Only")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 12f;
        public float projectileLifetime = 3f;
    }
}
