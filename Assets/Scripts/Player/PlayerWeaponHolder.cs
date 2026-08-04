using Combat;
using UnityEngine;

namespace Player
{
    public class PlayerWeaponHolder : MonoBehaviour
    {
        [SerializeField] private WeaponPivot weaponPivot;
        [SerializeField] private Weapon startingWeapon;
        [SerializeField] private PlayerAim aim;

        public Weapon CurrentWeapon => weaponPivot != null ? weaponPivot.CurrentWeapon : null;

        private void Awake()
        {
            if (weaponPivot == null)
                weaponPivot = GetComponentInChildren<WeaponPivot>(true);
            if (aim == null)
                aim = GetComponent<PlayerAim>();

            if (weaponPivot == null)
            {
                Debug.LogWarning($"[{name}] PlayerWeaponHolder needs a child WeaponPivot.");
                return;
            }

            if (startingWeapon != null)
                Equip(startingWeapon);
            else
            {
                Weapon childWeapon = weaponPivot.GetComponentInChildren<Weapon>(true);
                if (childWeapon != null)
                    Equip(childWeapon);
            }
        }

        private void LateUpdate()
        {
            if (weaponPivot == null || aim == null || CurrentWeapon == null)
                return;

            weaponPivot.FaceDirection(aim.AimDirection);
        }

        public void Equip(Weapon weapon)
        {
            if (weaponPivot == null || weapon == null)
                return;

            weaponPivot.Equip(weapon, WeaponOwner.PlayerTag);
        }

        public Weapon UnequipToGround(Vector3 dropPosition)
        {
            if (weaponPivot == null)
                return null;

            return weaponPivot.UnequipToGround(dropPosition);
        }
    }
}
