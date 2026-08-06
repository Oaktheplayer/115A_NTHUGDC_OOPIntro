using UnityEngine;

namespace Combat
{
    public class RangedWeapon : Weapon
    {
        [SerializeField] private ProjectileData projectileData;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 spawnOffset;

        protected override void PerformAttack(Vector2 direction, Transform target)
        {
            if (projectileData == null || projectileData.prefab == null)
            {
                Debug.LogWarning($"[{name}] RangedWeapon missing ProjectileData or prefab.");
                return;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            position += spawnOffset;

            GameObject instance = Instantiate(projectileData.prefab, position, Quaternion.identity);
            var projectile = instance.GetComponent<ProjectileMover>();
            var hit = instance.GetComponent<ProjectileHit>();

            if (hit != null)
                hit.Initialize(owner.ValidTargetTag, data != null ? data.damage : 1);

            if (projectile != null)
            {
                projectile.Initialize(
                    direction: direction,
                    speed: projectileData.speed,
                    lifetime: projectileData.lifetime
                );
            }
            else if (instance.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = direction * projectileData.speed;
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
