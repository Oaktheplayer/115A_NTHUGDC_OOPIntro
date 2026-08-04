using Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAttacker : MonoBehaviour
    {
        [SerializeField] private PlayerAim aim;
        [SerializeField] private PlayerWeaponHolder weaponHolder;

        private void Awake()
        {
            if (aim == null)
                aim = GetComponent<PlayerAim>();
            if (weaponHolder == null)
                weaponHolder = GetComponent<PlayerWeaponHolder>();
        }

        public void OnAttack(InputValue value)
        {
            if (!value.isPressed)
                return;

            TryAttack();
        }

        public void TryAttack()
        {
            Weapon weapon = weaponHolder != null ? weaponHolder.CurrentWeapon : null;
            if (weapon == null)
                return;

            Vector2 direction = aim != null ? aim.AimDirection : Vector2.up;
            weapon.Attack(direction);
        }
    }
}
