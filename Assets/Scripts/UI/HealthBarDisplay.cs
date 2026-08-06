using Combat;
using UnityEngine;

namespace UI
{
    public class HealthBarDisplay : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private SegmentedBar bar;

        private void Awake()
        {
            if (health == null)
                health = GetComponentInParent<Health>();
            if (bar == null)
                bar = GetComponent<SegmentedBar>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            if (bar == null || health == null)
                return;

            bar.SetSegmentCount(health.MaxValue);
            bar.SetFilled(health.Value);
        }
    }
}
