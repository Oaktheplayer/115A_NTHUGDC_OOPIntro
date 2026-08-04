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

            Health health = other.GetComponent<Health>();
            if (health == null)
                health = other.GetComponentInParent<Health>();

            if (health == null || !health.IsAlive)
                return;

            if (!hitThisSwing.Add(health))
                return;

            if (damager == null)
                damager = GetComponent<Damager>();

            damager?.DealDamage(health);
        }
    }
}
