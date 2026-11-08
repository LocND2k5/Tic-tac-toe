using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace Khang
{
    /// <summary>
    /// Reusable Settings panel UI.
    /// Wire all fields in the Inspector. Every slider/toggle change is
    /// applied live; the Save button persists to PlayerPrefs.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        [Header("Volume Labels (optional)")]
        [SerializeField] private TextMeshProUGUI masterLabel;
        [SerializeField] private TextMeshProUGUI sfxLabel;
        [SerializeField] private TextMeshProUGUI musicLabel;

        [Header("Fullscreen Toggle")]
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button resetButton;
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
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

            if (saveButton  != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
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
                SettingsManager.Instance.OnFullscreenChanged   += SyncFullscreen;
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMasterVolumeChanged -= SyncMaster;
                SettingsManager.Instance.OnSFXVolumeChanged    -= SyncSFX;
                SettingsManager.Instance.OnMusicVolumeChanged  -= SyncMusic;
                SettingsManager.Instance.OnFullscreenChanged   -= SyncFullscreen;
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnMasterSliderChanged(float value)
        {
            SettingsManager.Instance?.SetMasterVolume(value);
            SetLabel(masterLabel, value);
        }

        private void OnSFXSliderChanged(float value)
        {
            SettingsManager.Instance?.SetSFXVolume(value);
            SetLabel(sfxLabel, value);
        }

        private void OnMusicSliderChanged(float value)
        {
            SettingsManager.Instance?.SetMusicVolume(value);
            SetLabel(musicLabel, value);
        }

        private void OnFullscreenToggleChanged(bool value)
        {
            SettingsManager.Instance?.SetFullscreen(value);
        }

        private void OnSaveClicked()
        {
            SettingsManager.Instance?.SaveSettings();
            Hide();
        }

        private void OnResetClicked()
        {
            SettingsManager.Instance?.ResetToDefaults();
            RefreshUI();
        }

        private void OnCloseClicked()
        {
            RefreshUI();
            Hide();
        }

        private void SyncMaster(float v)    { SetSliderSilently(masterVolumeSlider, v); SetLabel(masterLabel, v); }
        private void SyncSFX(float v)       { SetSliderSilently(sfxVolumeSlider, v);    SetLabel(sfxLabel, v); }
        private void SyncMusic(float v)     { SetSliderSilently(musicVolumeSlider, v);  SetLabel(musicLabel, v); }
        private void SyncFullscreen(bool v) { if (fullscreenToggle != null) fullscreenToggle.isOn = v; }

        private void RefreshUI()
        {
            if (SettingsManager.Instance == null) return;

            SetSliderSilently(masterVolumeSlider, SettingsManager.Instance.MasterVolume);
            SetSliderSilently(sfxVolumeSlider,    SettingsManager.Instance.SFXVolume);
            SetSliderSilently(musicVolumeSlider,  SettingsManager.Instance.MusicVolume);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen;

            SetLabel(masterLabel, SettingsManager.Instance.MasterVolume);
            SetLabel(sfxLabel,    SettingsManager.Instance.SFXVolume);
            SetLabel(musicLabel,  SettingsManager.Instance.MusicVolume);
        }

        private static void SetSliderSilently(Slider slider, float value)
        {
            if (slider != null) slider.SetValueWithoutNotify(value);
        }

        private static void SetLabel(TextMeshProUGUI label, float value)
        {
            if (label != null) label.text = Mathf.RoundToInt(value * 100f) + "%";
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
        private const string KEY_FULLSCREEN    = "Fullscreen";

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
        public bool  IsFullscreen { get; private set; }

        public event System.Action<float> OnMasterVolumeChanged;
        public event System.Action<float> OnSFXVolumeChanged;
        public event System.Action<float> OnMusicVolumeChanged;
        public event System.Action<bool>  OnFullscreenChanged;

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

        public void SetFullscreen(bool value)
        {
            IsFullscreen = value;
            Screen.fullScreen = IsFullscreen;
            OnFullscreenChanged?.Invoke(IsFullscreen);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, MasterVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME,    SFXVolume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME,  MusicVolume);
            PlayerPrefs.SetInt(KEY_FULLSCREEN, IsFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            SetMasterVolume(DEFAULT_MASTER);
            SetSFXVolume(DEFAULT_SFX);
            SetMusicVolume(DEFAULT_MUSIC);
            SetFullscreen(!Application.isMobilePlatform);
            SaveSettings();
        }

        private void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, DEFAULT_MASTER);
            SFXVolume    = PlayerPrefs.GetFloat(KEY_SFX_VOLUME,    DEFAULT_SFX);
            MusicVolume  = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME,  DEFAULT_MUSIC);
            IsFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        }

        private void ApplyAll()
        {
            ApplyMasterVolume();
            ApplySFXVolume();
            ApplyMusicVolume();
            Screen.fullScreen = IsFullscreen;
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
