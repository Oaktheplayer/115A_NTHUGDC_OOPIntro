using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// One bar cell: nine-sliced frame + inner fill Image (Filled).
    /// </summary>
    public class SegmentView : MonoBehaviour
    {
        [SerializeField] private Image frame;
        [SerializeField] private Image fill;

        private void Awake()
        {
            if (frame == null)
                frame = GetComponent<Image>();
            if (fill == null && transform.childCount > 0)
                fill = transform.GetChild(0).GetComponent<Image>();
        }

        public void SetFillColor(Color color)
        {
            if (fill == null)
                return;
            fill.color = color;
        }

        public void SetFill(float amount01)
        {
            if (fill == null)
                return;

            fill.fillAmount = Mathf.Clamp01(amount01);
            fill.enabled = amount01 > 0.001f;
        }
    }
}
