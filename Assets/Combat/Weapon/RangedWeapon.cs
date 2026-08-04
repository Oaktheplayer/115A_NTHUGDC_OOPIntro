using UnityEngine;

namespace Combat
{
    public class RangedWeapon : Weapon
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 spawnOffset;

        protected override void PerformAttack(Vector2 direction, Transform target)
        {
            if (data == null || data.projectilePrefab == null)
            {
                Debug.LogWarning($"[{name}] RangedWeapon missing WeaponData projectilePrefab.");
                return;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            position += spawnOffset;

            GameObject instance = Instantiate(data.projectilePrefab, position, Quaternion.identity);
            var projectile = instance.GetComponent<ProjectileMover>();
            var hit = instance.GetComponent<ProjectileHit>();

            float speed = data.projectileSpeed;
            float lifetime = data.projectileLifetime;

            if (hit != null)
            {
                hit.Initialize(owner.ValidTargetTag, data.damage);
            }

            if (projectile != null)
            {
                projectile.Initialize(direction, speed, lifetime);
            }
            else
            {
                // Fallback: push with Rigidbody2D if present.
                if (instance.TryGetComponent(out Rigidbody2D rb))
                    rb.linearVelocity = direction * speed;
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            position += spawnOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(position + new Vector3(0.25f, 0.25f), position + new Vector3(-0.25f, -0.25f));
            Gizmos.DrawLine(position + new Vector3(-0.25f, 0.25f), position + new Vector3(0.25f, -0.25f));
        }
    }
}
