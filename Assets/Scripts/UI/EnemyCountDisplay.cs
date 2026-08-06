using Enemy;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EnemyCountDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text remainingText;
        [SerializeField] private TMP_Text slashText;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private string label = "Enemies";

        private void OnEnable()
        {
            EnemyRegistry.OnCountChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            EnemyRegistry.OnCountChanged -= Refresh;
        }

        private void Refresh()
        {
            if (labelText != null)
                labelText.text = label;

            if (remainingText != null)
                remainingText.text = EnemyRegistry.RemainingCount.ToString();

            if (slashText != null)
                slashText.text = "/";

            if (totalText != null)
                totalText.text = EnemyRegistry.TotalCount.ToString();
        }
    }
}
