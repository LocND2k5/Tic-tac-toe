using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Khang
{
    /// <summary>
    /// Reusable Settings panel UI.
    /// Wire all fields in the Inspector. Every slider change is
    /// applied live and saved when the settings panel closes.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            SettingsManager.EnsureInstance();

            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            RefreshUI();
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMasterVolumeChanged += SyncMaster;
                SettingsManager.Instance.OnSFXVolumeChanged    += SyncSFX;
                SettingsManager.Instance.OnMusicVolumeChanged  += SyncMusic;
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SaveSettings();
                SettingsManager.Instance.OnMasterVolumeChanged -= SyncMaster;
                SettingsManager.Instance.OnSFXVolumeChanged    -= SyncSFX;
                SettingsManager.Instance.OnMusicVolumeChanged  -= SyncMusic;
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnMasterSliderChanged(float value)
        {
            SettingsManager.Instance?.SetMasterVolume(value);
        }

        private void OnSFXSliderChanged(float value)
        {
            SettingsManager.Instance?.SetSFXVolume(value);
        }

        private void OnMusicSliderChanged(float value)
        {
            SettingsManager.Instance?.SetMusicVolume(value);
        }

        private void OnCloseClicked()
        {
            Hide();
        }

        private void SyncMaster(float v)    => SetSliderSilently(masterVolumeSlider, v);
        private void SyncSFX(float v)       => SetSliderSilently(sfxVolumeSlider, v);
        private void SyncMusic(float v)     => SetSliderSilently(musicVolumeSlider, v);

        private void RefreshUI()
        {
            if (SettingsManager.Instance == null) return;

            SetSliderSilently(masterVolumeSlider, SettingsManager.Instance.MasterVolume);
            SetSliderSilently(sfxVolumeSlider,    SettingsManager.Instance.SFXVolume);
            SetSliderSilently(musicVolumeSlider,  SettingsManager.Instance.MusicVolume);
        }

        private static void SetSliderSilently(Slider slider, float value)
        {
            if (slider != null) slider.SetValueWithoutNotify(value);
        }

    }

    /// <summary>
    /// Gộp SettingsManager vào đây để quản lý lưu trữ PlayerPrefs và âm lượng.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public static SettingsManager EnsureInstance()
        {
            if (Instance != null) return Instance;

            var settingsObject = new GameObject(nameof(SettingsManager));
            return settingsObject.AddComponent<SettingsManager>();
        }

        private const string KEY_MASTER_VOLUME = "MasterVolume";
        private const string KEY_SFX_VOLUME    = "SFXVolume";
        private const string KEY_MUSIC_VOLUME  = "MusicVolume";
        private const float DEFAULT_MASTER = 1f;
        private const float DEFAULT_SFX    = 1f;
        private const float DEFAULT_MUSIC  = 0.5f;

        [Header("Audio Mixer (optional)")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterParam = "MasterVolume";
        [SerializeField] private string sfxParam    = "SFXVolume";
        [SerializeField] private string musicParam  = "MusicVolume";

        public float MasterVolume { get; private set; }
        public float SFXVolume    { get; private set; }
        public float MusicVolume  { get; private set; }

        public event System.Action<float> OnMasterVolumeChanged;
        public event System.Action<float> OnSFXVolumeChanged;
        public event System.Action<float> OnMusicVolumeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            ApplyAll();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            ApplyMasterVolume();
            OnMasterVolumeChanged?.Invoke(MasterVolume);
        }

        public void SetSFXVolume(float value)
        {
            SFXVolume = Mathf.Clamp01(value);
            ApplySFXVolume();
            OnSFXVolumeChanged?.Invoke(SFXVolume);
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            ApplyMusicVolume();
            OnMusicVolumeChanged?.Invoke(MusicVolume);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, MasterVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME,    SFXVolume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME,  MusicVolume);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, DEFAULT_MASTER);
            SFXVolume    = PlayerPrefs.GetFloat(KEY_SFX_VOLUME,    DEFAULT_SFX);
            MusicVolume  = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME,  DEFAULT_MUSIC);
        }

        private void ApplyAll()
        {
            ApplyMasterVolume();
            ApplySFXVolume();
            ApplyMusicVolume();
        }

        private void ApplyMasterVolume()
        {
            if (audioMixer != null)
                audioMixer.SetFloat(masterParam, LinearToDecibel(MasterVolume));
            else
                AudioListener.volume = MasterVolume;
        }

        private void ApplySFXVolume()
        {
            if (audioMixer != null)
                audioMixer.SetFloat(sfxParam, LinearToDecibel(SFXVolume));
        }

        private void ApplyMusicVolume()
        {
            if (audioMixer != null)
                audioMixer.SetFloat(musicParam, LinearToDecibel(MusicVolume));
        }

        private static float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        }
    }
}
