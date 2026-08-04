using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Damages the first valid tagged Health, then destroys the projectile.
    /// Target tag is copied from the firing weapon's WeaponOwner.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileHit : MonoBehaviour
    {
        [SerializeField] private Damager damager;

        private string validTargetTag = WeaponOwner.EnemyTag;

        private void Awake()
        {
            if (damager == null)
                damager = GetComponent<Damager>();

            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        public void Initialize(string targetTag, int damage)
        {
            validTargetTag = targetTag;
            if (damager != null)
                damager.SetDamage(damage);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (string.IsNullOrEmpty(validTargetTag))
                return;

            if (!other.CompareTag(validTargetTag))
                return;

            Health health = other.GetComponent<Health>();
            if (health == null)
                health = other.GetComponentInParent<Health>();

            if (health == null || !health.IsAlive)
                return;

            damager.DealDamage(health);
            Destroy(gameObject);
        }
    }
}
