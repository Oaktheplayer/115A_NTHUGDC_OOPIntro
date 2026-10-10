using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Rigidbody2D rb;

        private Vector2 moveInput;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }

        private void FixedUpdate()
        {
            Vector2 desired = moveInput;
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            rb.linearVelocity = desired * moveSpeed;
        }

        public void ApplyMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }
    }
}
