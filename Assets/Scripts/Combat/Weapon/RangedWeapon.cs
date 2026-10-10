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

            // TODO [LAB-H3] 開槍：在 position 複製一顆子彈 Prefab，並設定它的傷害與飛行方向。
            // 提示：Instantiate(projectileData.prefab, ...)；再用 GetComponent 取得 ProjectileHit / ProjectileMover 呼叫 Initialize(...)
            // 提示：傷害來自 data.damage、目標 Tag 來自 owner.ValidTargetTag；速度與壽命來自 projectileData
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
