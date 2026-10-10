using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Combat
{
    public class RangeDetect : MonoBehaviour
    {
        [SerializeField] private List<Transform> targets = new();

        // TODO: make multiple modes to handle inclusion and exclusion logic
        [SerializeField] private bool doUseIncludeTags = true;
        [SerializeField] private List<string> includeTags = new();

        public bool HasTargets => targets.Count > 0;
        public int TargetCount => targets.Count;
        public IReadOnlyList<Transform> Targets => targets;
        public Transform FirstTarget => targets[0];

        public event Action OnTargetDetected;
        public event Action OnTargetLost;

        private void Awake()
        {
            targets.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Transform target = collision.transform;
            if (!IsValidTarget(target))
                return;

            if (targets.Contains(target))
                return;

            targets.Add(target);

            OnTargetDetected?.Invoke();
        }

        public void OnValidate()
        {
            if (doUseIncludeTags && includeTags.Count == 0)
            {
                Debug.LogWarning(
                    $"[{name}:cannot detect] Set to include tags, but no include tags specified.\n" +
                    $"This detector will not detect anything."
                    );
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Transform target = collision.transform;

            // try removing from the list (if it doesn't exist, nothing happens
            targets.Remove(target);

            if (targets.Count == 0)
            {
                OnTargetLost?.Invoke();
            }
        }

        public Transform GetTarget()
        {
            if (targets.Count == 0) return null;

            // TODO: Sort targets by given priority (distance, health, etc.)
            // targets.Sort();

            return targets[0];
        }

        /// <summary>
        /// Detects whether the currentTarget is valid based on its tag.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsValidTarget(Transform target)
        {
            if (target == null) return false;

            // if not using tags, then all targets are valid
            if (!doUseIncludeTags) return true;

            // check if *Any* tag in the list matches the currentTarget's tag
            return includeTags.Any(tag => target.CompareTag(tag));
        }

        public IEnumerator WaitForTarget()
        {
            yield return new WaitUntil(() => HasTargets);
        }

        public void SetTags(IReadOnlyList<string> tags)
        {
            this.doUseIncludeTags = true;
            this.includeTags = new(tags);
        }
    }
}