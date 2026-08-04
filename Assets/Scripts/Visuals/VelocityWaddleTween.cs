using UnityEngine;

namespace Visuals
{
    /// <summary>
    /// Teacher-provided walk waddle. Observes Rigidbody2D.velocity:
    /// flips sprite on X based on velocity.x, waddles while moving, stops when too slow.
    /// Hang on the Sprite child so only visuals move.
    /// </summary>
    public class VelocityWaddleTween : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float moveThreshold = 0.1f;
        [SerializeField] private float angleAmplitude = 8f;
        [SerializeField] private float frequency = 10f;

        private Quaternion restRotation;
        private Vector3 restScale;
        private float phase;
        private bool waddling;
        private float facingSign = 1f;

        private void Awake()
        {
            restRotation = transform.localRotation;
            restScale = transform.localScale;

            if (body == null)
                body = GetComponentInParent<Rigidbody2D>();

            if (Mathf.Abs(restScale.x) > 0.0001f)
                facingSign = Mathf.Sign(restScale.x);
        }

        private void LateUpdate()
        {
            if (body == null)
                return;

            Vector2 velocity = body.linearVelocity;
            float speed = velocity.magnitude;

            if (Mathf.Abs(velocity.x) >= moveThreshold)
                facingSign = Mathf.Sign(velocity.x);

            Vector3 scale = restScale;
            scale.x = Mathf.Abs(restScale.x) * facingSign;
            transform.localScale = scale;

            if (speed >= moveThreshold)
            {
                waddling = true;
                phase += Time.deltaTime * frequency;
                float waddle = Mathf.Sin(phase) * angleAmplitude;
                transform.localRotation = restRotation * Quaternion.Euler(0f, 0f, waddle);
            }
            else if (waddling)
            {
                waddling = false;
                phase = 0f;
                transform.localRotation = restRotation;
            }
        }
    }
}
