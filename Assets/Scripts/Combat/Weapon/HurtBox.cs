using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Melee hit volume. Enable the GameObject only during active swing frames.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HurtBox : MonoBehaviour
    {
        [SerializeField] private Damager damager;
        [SerializeField] private WeaponOwner owner;

        private readonly HashSet<Health> hitThisSwing = new();

        private void Awake()
        {
            if (damager == null)
                damager = GetComponent<Damager>();
            if (owner == null)
                owner = GetComponentInParent<WeaponOwner>();

            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        public void ApplyDamageValue(int damage)
        {
            if (damager == null)
                damager = GetComponent<Damager>();
            if (owner == null)
                owner = GetComponentInParent<WeaponOwner>();

            damager?.SetDamage(damage);
        }

        public void BeginSwing()
        {
            if (damager == null)
                damager = GetComponent<Damager>();
            if (owner == null)
                owner = GetComponentInParent<WeaponOwner>();

            hitThisSwing.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == null || string.IsNullOrEmpty(owner.ValidTargetTag))
                return;

            if (!other.CompareTag(owner.ValidTargetTag))
                return;

#if LAB_SOLUTION
            Health health = other.GetComponent<Health>();
            if (health == null)
                health = other.GetComponentInParent<Health>();
#else
            // TODO [LAB-H4] (1/2) 找出被砍到的物件身上的 Health 元件（血量是「組合」在 GameObject 上的元件）。
            // 提示：other.GetComponent<Health>()；Collider 可能在子物件上，找不到時再試 GetComponentInParent<Health>()
            Health health = null;
#endif

            if (health == null || !health.IsAlive)
                return;

            if (!hitThisSwing.Add(health))
                return;

            if (damager == null)
                damager = GetComponent<Damager>();

#if LAB_SOLUTION
            damager?.DealDamage(health);
#else
            // TODO [LAB-H4] (2/2) 把找到的 Health 交給這個 HurtBox 擁有的 Damager 元件，讓它造成傷害。
            // 提示：damager 可能是 null，記得檢查；Damager 有一個接收 Health 的方法
#endif
        }
    }
}
