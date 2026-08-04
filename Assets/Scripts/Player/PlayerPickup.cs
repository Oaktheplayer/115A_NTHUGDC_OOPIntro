using Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private float pickupRadius = 1.25f;
        [SerializeField] private PlayerWeaponHolder weaponHolder;
        [SerializeField] private LayerMask pickupMask = ~0;

        private void Awake()
        {
            if (weaponHolder == null)
                weaponHolder = GetComponent<PlayerWeaponHolder>();
        }

        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
                return;

            TrySwapWeapon();
        }

        public void TrySwapWeapon()
        {
            WeaponPickup nearest = FindNearestPickup();
            if (nearest == null || nearest.Weapon == null)
                return;

            Weapon groundWeapon = nearest.Weapon;
            Vector3 dropPos = transform.position;

            // Drop current (same instance, OnGround / non-functioning).
            weaponHolder.UnequipToGround(dropPos);

            // Equip the ground weapon (same instance).
            weaponHolder.Equip(groundWeapon);
        }

        private WeaponPickup FindNearestPickup()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius, pickupMask);
            WeaponPickup best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                WeaponPickup pickup = hits[i].GetComponentInParent<WeaponPickup>();
                if (pickup == null || !pickup.enabled || pickup.Weapon == null)
                    continue;

                // Ignore the weapon currently held by the player.
                if (weaponHolder.CurrentWeapon != null && pickup.Weapon == weaponHolder.CurrentWeapon)
                    continue;

                float dist = ((Vector2)pickup.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = pickup;
                }
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
