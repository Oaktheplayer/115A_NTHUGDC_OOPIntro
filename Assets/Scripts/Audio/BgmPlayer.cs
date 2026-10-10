using Settings;
using UnityEngine;

namespace Audio
{
    /// <summary>Loops a background music clip and follows the volume stored in AudioSettingsData.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class BgmPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSettingsData settings;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (settings != null)
                settings.OnBgmVolumeChanged += HandleVolumeChanged;
        }

        private void OnDisable()
        {
            if (settings != null)
                settings.OnBgmVolumeChanged -= HandleVolumeChanged;
        }

        private void Start()
        {
            if (settings == null)
                Debug.LogWarning($"{name}: BgmPlayer has no AudioSettingsData assigned, using volume 1.", this);

            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.volume = settings != null ? settings.BgmVolume : 1f;

            if (playOnStart && bgmClip != null)
                audioSource.Play();
        }

        private void HandleVolumeChanged(float volume)
        {
            // TODO [LAB-H6] 設定檔的音量改變時，讓正在播放的 BGM 跟著變大聲／變小聲。（投影片「ScriptableObject 設定／Observer」）
            // 提示：這個方法已在 OnEnable 訂閱了 settings.OnBgmVolumeChanged；參數 volume 就是新音量。
            // 提示：把它設給 audioSource.volume。
        }
    }
}
