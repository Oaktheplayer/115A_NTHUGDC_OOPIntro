using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Sole owner of Rigidbody2D velocity for an enemy.
    /// Future stun / knockback should extend this component.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;

        private Vector2 desiredVelocity;
        private bool hasDesiredVelocity;

        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void MoveTowards(Vector2 worldTarget, float speed)
        {
            Vector2 delta = worldTarget - (Vector2)transform.position;
            if (delta.sqrMagnitude < 0.0001f)
            {
                desiredVelocity = Vector2.zero;
            }
            else
            {
                desiredVelocity = delta.normalized * speed;
            }

            hasDesiredVelocity = true;
        }

        public void Stop()
        {
            desiredVelocity = Vector2.zero;
            hasDesiredVelocity = true;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Reserved for a future knockback system. Mainline course does not use this yet.
        /// </summary>
        public void ApplyKnockback(Vector2 force)
        {
            if (rb == null)
                return;

            rb.AddForce(force, ForceMode2D.Impulse);
            hasDesiredVelocity = false;
        }

        private void FixedUpdate()
        {
            if (!CanMove)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (hasDesiredVelocity)
                rb.linearVelocity = desiredVelocity;
        }
    }
}
