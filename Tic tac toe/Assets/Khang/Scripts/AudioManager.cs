using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Khang
{
    /// <summary>
    /// Singleton quản lý toàn bộ âm thanh trong game (Nhạc nền và Hiệu ứng SFX).
    /// Tự động đồng bộ âm lượng với SettingsManager và phát âm thanh khi chơi cờ.
    /// Tồn tại xuyên suốt các Scene nhờ DontDestroyOnLoad.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources (Để trống sẽ tự tạo)")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips (Kéo file nhạc/âm thanh vào đây)")]
        [SerializeField] private AudioClip bgmMusic;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip placePieceSFX;
        [SerializeField] private AudioClip winSFX;
        [SerializeField] private AudioClip tieSFX;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tự khởi tạo AudioSource nếu chưa kéo trong Inspector
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnsubscribeSettingsEvents();
            UnsubscribeGameEvents();
        }

        private void Start()
        {
            SubscribeSettingsEvents();

            // Phát nhạc nền lặp lại
            if (bgmMusic != null)
            {
                PlayMusic(bgmMusic);
            }

            // Đăng ký sự kiện nếu GameManager đã có sẵn trong Scene
            SubscribeGameEvents();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SubscribeGameEvents();
        }

        // ── KẾT NỐI VỚI SETTINGSMANAGER ─────────────────────────────────

        private void SubscribeSettingsEvents()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMusicVolumeChanged += UpdateMusicVolume;
                SettingsManager.Instance.OnSFXVolumeChanged    += UpdateSFXVolume;

                // Áp dụng mức âm lượng đã lưu
                UpdateMusicVolume(SettingsManager.Instance.MusicVolume);
                UpdateSFXVolume(SettingsManager.Instance.SFXVolume);
            }
        }

        private void UnsubscribeSettingsEvents()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnMusicVolumeChanged -= UpdateMusicVolume;
                SettingsManager.Instance.OnSFXVolumeChanged    -= UpdateSFXVolume;
            }
        }

        private void UpdateMusicVolume(float volume)
        {
            if (musicSource != null) musicSource.volume = Mathf.Clamp01(volume);
        }

        private void UpdateSFXVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
        }

        // ── KẾT NỐI VỚI TRẬN ĐẤU (GAMEMANAGER) ─────────────────────────

        private void SubscribeGameEvents()
        {
            if (GameManager.Instance == null) return;

            UnsubscribeGameEvents(); // Tránh trùng lặp đăng ký
            GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
            GameManager.Instance.OnGameWin               += GameManager_OnGameWin;
            GameManager.Instance.OnGameTied              += GameManager_OnGameTied;
        }

        private void UnsubscribeGameEvents()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnClickedOnGridPosition -= GameManager_OnClickedOnGridPosition;
            GameManager.Instance.OnGameWin               -= GameManager_OnGameWin;
            GameManager.Instance.OnGameTied              -= GameManager_OnGameTied;
        }

        private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
        {
            PlayPlacePiece();
        }

        private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
        {
            PlayWin();
        }

        private void GameManager_OnGameTied(object sender, EventArgs e)
        {
            PlayTie();
        }

        // ── HÀM PHÁT ÂM THANH PUBLIC ───────────────────────────────────

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayButtonClick() => PlaySFX(buttonClickSFX);
        public void PlayPlacePiece()  => PlaySFX(placePieceSFX);
        public void PlayWin()         => PlaySFX(winSFX);
        public void PlayTie()         => PlaySFX(tieSFX);
    }
}
