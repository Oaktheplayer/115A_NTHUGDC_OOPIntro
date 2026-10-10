using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Straight projectile movement driven by velocity.
    /// </summary>
    public class ProjectileMover : MonoBehaviour
    {
        private Rigidbody2D rb;
        private float despawnAt;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 direction, float speed, float lifetime)
        {
            if (direction.sqrMagnitude > 0.0001f)
                transform.up = direction;

            despawnAt = Time.time + lifetime;

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = direction.normalized * speed;
            }
        }

        private void Update()
        {
            if (Time.time >= despawnAt)
                Destroy(gameObject);
        }
    }
}
