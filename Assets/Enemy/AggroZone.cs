using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Trigger zone that notifies the EnemyController when the player enters.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class AggroZone : MonoBehaviour
    {
        [SerializeField] private EnemyController controller;
        [SerializeField] private CircleCollider2D circle;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponentInParent<EnemyController>();
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();

            circle.isTrigger = true;
        }

        public void SetRadius(float radius)
        {
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();
            circle.radius = Mathf.Max(0.1f, radius);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Combat.WeaponOwner.PlayerTag))
                return;

            controller?.NotifyPlayerDetected(other.transform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();
            if (circle == null)
                return;

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, circle.radius * Mathf.Abs(transform.lossyScale.x));
        }
#endif
    }
}
