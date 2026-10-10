using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>Binds a UI Slider to AudioSettingsData.BgmVolume.</summary>
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private AudioSettingsData settings;
        [SerializeField] private Slider slider;
        [Tooltip("Optional. Shows the volume as a percentage.")]
        [SerializeField] private TMP_Text valueLabel;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;

            if (settings != null)
                slider.SetValueWithoutNotify(settings.BgmVolume);
            else
                Debug.LogWarning($"{name}: VolumeSlider has no AudioSettingsData assigned.", this);

            RefreshLabel(slider.value);
            slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private void HandleValueChanged(float value)
        {
            RefreshLabel(value);

            if (settings == null)
                return;

            // TODO [LAB-H6] 玩家拖動 Slider 時，把新數值寫進 ScriptableObject 設定檔。（投影片「ScriptableObject 設定／Observer」）
            // 提示：使用屬性 settings.BgmVolume（不是直接改欄位），它會通知所有訂閱者（例如 BgmPlayer）。
        }

        private void RefreshLabel(float value)
        {
            if (valueLabel != null)
                valueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
