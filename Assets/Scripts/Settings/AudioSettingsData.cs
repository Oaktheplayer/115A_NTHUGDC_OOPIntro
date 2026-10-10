using System;
using UnityEngine;

namespace Settings
{
    /// <summary>Shared audio settings asset. Changing BgmVolume notifies every listener (Observer).</summary>
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Settings/Audio Settings")]
    public class AudioSettingsData : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.8f;

        /// <summary>Fires with the new volume (0..1) whenever BgmVolume changes.</summary>
        public event Action<float> OnBgmVolumeChanged;

        public float BgmVolume
        {
            get => bgmVolume;
            set
            {
                float next = Mathf.Clamp01(value);
                if (Mathf.Approximately(next, bgmVolume))
                    return;

                bgmVolume = next;
                OnBgmVolumeChanged?.Invoke(bgmVolume);
            }
        }

        private void OnValidate()
        {
            // Inspector edits bypass the setter, so notify here to apply them live.
            bgmVolume = Mathf.Clamp01(bgmVolume);
            OnBgmVolumeChanged?.Invoke(bgmVolume);
        }
    }
}
