using Combat;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Binds a <see cref="SegmentedBar"/> to the equipped weapon under a parent
    /// <see cref="WeaponPivot"/> (works for Player and Enemy).
    /// </summary>
    public class AmmoBarDisplay : MonoBehaviour
    {
        [SerializeField] private WeaponPivot weaponPivot;
        [SerializeField] private SegmentedBar bar;

        private int lastMaxAmmo = -1;
        private Weapon lastWeapon;

        private void Awake()
        {
            if (weaponPivot == null)
            {
                Health ownerHealth = GetComponentInParent<Health>();
                if (ownerHealth != null)
                    weaponPivot = ownerHealth.GetComponentInChildren<WeaponPivot>(true);
            }

            if (bar == null)
                bar = GetComponent<SegmentedBar>();
        }

        private void Update()
        {
            if (bar == null)
                return;

            Weapon weapon = weaponPivot != null ? weaponPivot.CurrentWeapon : null;
            if (weapon == null)
            {
                if (lastWeapon != null || lastMaxAmmo != 0)
                {
                    bar.Clear();
                    lastMaxAmmo = 0;
                    lastWeapon = null;
                }
                return;
            }

            int maxAmmo = weapon.MaxAmmo;
            if (weapon != lastWeapon || maxAmmo != lastMaxAmmo)
            {
                bar.SetSegmentCount(maxAmmo);
                lastMaxAmmo = maxAmmo;
                lastWeapon = weapon;
            }

            // Next empty cell shows reload progress toward the next ammo.
            float partial = weapon.IsAmmoFull ? 0f : weapon.ReloadProgress01;
            bar.SetFilled(weapon.CurrentAmmo, partial);
        }
    }
}
