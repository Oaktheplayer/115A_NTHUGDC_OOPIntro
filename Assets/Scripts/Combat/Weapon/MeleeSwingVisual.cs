using System.Collections;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Teacher-provided swing visual. Students do not need to implement this.
    /// Assumes the weapon is already faced by WeaponPivot; only applies a local Z arc.
    /// When the sprite is Y-flipped (aiming left), the swing direction is reversed.
    /// </summary>
    public class MeleeSwingVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualTarget;
        [SerializeField] private float swingAngle = 70f;
        [SerializeField] private float swingDuration = 0.2f;

        private Coroutine swingRoutine;
        private Quaternion restRotation;

        private void Awake()
        {
            if (visualTarget == null)
                visualTarget = transform;
            restRotation = visualTarget.localRotation;
        }

        public void PlaySwing(Vector2 direction)
        {
            if (visualTarget == null)
                return;

            if (swingRoutine != null)
                StopCoroutine(swingRoutine);

            swingRoutine = StartCoroutine(SwingRoutine());
        }

        public void PlaySwing()
        {
            PlaySwing(Vector2.right);
        }

        private IEnumerator SwingRoutine()
        {
            // WeaponPivot flips localScale.y when aiming left (90..270).
            bool flipped = visualTarget.localScale.y < 0f;
            float signedSwing = flipped ? -swingAngle : swingAngle;

            Quaternion from = restRotation * Quaternion.Euler(0f, 0f, signedSwing * 0.5f);
            Quaternion to = restRotation * Quaternion.Euler(0f, 0f, -signedSwing * 0.5f);

            float half = Mathf.Max(0.01f, swingDuration * 0.5f);
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                visualTarget.localRotation = Quaternion.Slerp(from, to, t / half);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                visualTarget.localRotation = Quaternion.Slerp(to, restRotation, t / half);
                yield return null;
            }

            visualTarget.localRotation = restRotation;
            swingRoutine = null;
        }
    }
}
