using Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAttacker : MonoBehaviour
    {
        [SerializeField] private PlayerAim aim;
        [SerializeField] private PlayerWeaponHolder weaponHolder;
        [SerializeField] private PlayerInput playerInput;

        private InputAction attackAction;

        private void Awake()
        {
            if (aim == null)
                aim = GetComponent<PlayerAim>();
            if (weaponHolder == null)
                weaponHolder = GetComponent<PlayerWeaponHolder>();
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();

            if (playerInput != null && playerInput.actions != null)
                attackAction = playerInput.actions.FindAction("Attack", throwIfNotFound: false);
        }

        private void OnEnable()
        {
            attackAction?.Enable();
        }

        private void OnDisable()
        {
            attackAction?.Disable();
        }

        private void Update()
        {
            if (!IsAttackHeld())
                return;

            TryAttack();
        }

        private bool IsAttackHeld()
        {
            if (attackAction != null)
                return attackAction.IsPressed();

            // Fallback if PlayerInput / action map is missing.
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
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
