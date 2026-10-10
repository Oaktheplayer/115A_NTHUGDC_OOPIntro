using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 散彈槍：繼承 RangedWeapon，改寫 PerformAttack，一次朝扇形方向射出多顆子彈。
    /// 注意：父類別 Weapon.Attack 每次只扣一發彈藥，所以一次散射（pelletCount 顆子彈）只消耗 1 發，這是預期行為。
    /// </summary>
    public class SpreadShotWeapon : RangedWeapon
    {
        [SerializeField] private int pelletCount = 3;
        [SerializeField] private float spreadAngle = 30f;

        protected override void PerformAttack(Vector2 direction, Transform target)
        {
            // TODO [LAB-H5] 改寫父類別的發射方法：把 direction 平均旋轉成 pelletCount 個方向（總角度 spreadAngle），每個方向各射一發。
            // 提示：用 for 迴圈；Quaternion.Euler(0f, 0f, 角度) * direction 可以旋轉方向
            // 提示：每一發都呼叫 base.PerformAttack(新方向, target)，讓父類別幫你生成子彈
            base.PerformAttack(direction, target);
        }

        private void OnValidate()
        {
            pelletCount = Mathf.Max(1, pelletCount);
            spreadAngle = Mathf.Clamp(spreadAngle, 0f, 360f);
        }
    }
}
