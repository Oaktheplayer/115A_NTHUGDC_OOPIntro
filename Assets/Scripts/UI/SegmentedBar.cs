using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Dynamically instantiates segment cells to match max health / max ammo.
    /// </summary>
    public class SegmentedBar : MonoBehaviour
    {
        [SerializeField] private SegmentView segmentPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private Color fillColor = Color.white;
        [SerializeField] private bool overrideFillColor;

        private readonly List<SegmentView> segments = new();

        private void Awake()
        {
            if (container == null)
                container = transform;
        }

        public void SetFillColor(Color color)
        {
            fillColor = color;
            overrideFillColor = true;
            for (int i = 0; i < segments.Count; i++)
                segments[i].SetFillColor(fillColor);
        }

        public void SetSegmentCount(int count)
        {
            count = Mathf.Max(0, count);

            while (segments.Count < count)
            {
                if (segmentPrefab == null)
                {
                    Debug.LogWarning($"[{name}] SegmentedBar missing segmentPrefab.");
                    return;
                }

                SegmentView instance = Instantiate(segmentPrefab, container);
                instance.gameObject.SetActive(true);
                if (overrideFillColor)
                    instance.SetFillColor(fillColor);
                segments.Add(instance);
            }

            while (segments.Count > count)
            {
                int last = segments.Count - 1;
                SegmentView extra = segments[last];
                segments.RemoveAt(last);
                if (extra != null)
                    Destroy(extra.gameObject);
            }
        }

        /// <summary>
        /// Fills the first <paramref name="filledCount"/> segments completely.
        /// The next segment (if any) uses <paramref name="partialFill01"/> (e.g. ammo reload).
        /// Remaining segments are empty.
        /// </summary>
        public void SetFilled(int filledCount, float partialFill01 = 0f)
        {
            filledCount = Mathf.Clamp(filledCount, 0, segments.Count);
            partialFill01 = Mathf.Clamp01(partialFill01);

            for (int i = 0; i < segments.Count; i++)
            {
                if (i < filledCount)
                    segments[i].SetFill(1f);
                else if (i == filledCount)
                    segments[i].SetFill(partialFill01);
                else
                    segments[i].SetFill(0f);
            }
        }

        public void Clear()
        {
            SetSegmentCount(0);
        }
    }
}
