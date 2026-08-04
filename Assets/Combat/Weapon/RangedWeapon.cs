using UnityEngine;
using UnityEngine.Events;

namespace Combat
{
    public class RangedWeapon : Weapon
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 spawnOffset;

        [SerializeField] ShootMode shootMode = ShootMode.FaceTarget;
        [SerializeField] Vector2 shootDir = Vector2.up;

        public UnityEvent OnDidAttack;
        enum ShootMode
        {
            FaceTarget,
            UpAsDirection,
            OffsetAsDirection,
            StaticDirection,
            StaticDirectionInLocal,
        }

        /////
        public void SetDirection(Vector2 direction)
        {
            shootDir = direction.normalized;
        }
        /////

        public override void Attack(Transform target)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[{name}] No projectile prefab assigned.");
                return;
            }



            if (target == null && shootMode == ShootMode.FaceTarget)
            {
                Debug.LogWarning($"[{name}] No target assigned for FaceTarget shoot mode.");
                return;
            }

            var position = spawnPoint != null ?
                spawnPoint.position :
                transform.position;
            position += spawnOffset;

            var projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            Debug.Log($"[{name}] Spawned projectile at {position} with shoot mode {shootMode}.");
            switch (shootMode)
            {
                case ShootMode.FaceTarget:
                    projectile.Initialize(target);
                    break;
                case ShootMode.UpAsDirection:
                    projectile.Initialize(Vector2.up, target);
                    break;
                case ShootMode.OffsetAsDirection:
                    projectile.Initialize(spawnOffset.normalized, target);
                    break;
                case ShootMode.StaticDirection:
                    projectile.Initialize(shootDir, target);
                    break;
                case ShootMode.StaticDirectionInLocal:
                    {
                        Vector2 direction = transform.rotation * shootDir;
                        projectile.Initialize(direction, target);
                    }
                    break;
                default:
                    Debug.LogWarning($"[{name}] Invalid shoot mode.");
                    projectile.enabled = false;
                    break;

            }

            OnDidAttack?.Invoke();
        }

        private void OnDrawGizmos()
        {
            // draw a cross at the spawn point

            var position = spawnPoint != null ?
                spawnPoint.position :
                transform.position;
            position += spawnOffset;

            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(position + new Vector3(0.5f, 0.5f), position + new Vector3(-0.5f, -0.5f));
            Gizmos.DrawLine(position + new Vector3(-0.5f, 0.5f), position + new Vector3(0.5f, -0.5f));
        }
    }
}
