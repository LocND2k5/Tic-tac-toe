using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Khang
{
    /// <summary>
    /// In-game Pause Menu overlay for SampleScene.
    ///
    /// Setup:
    ///   1. Create a full-screen Panel called "PauseMenuPanel" (child of Canvas).
    ///   2. Inside it add:
    ///        • Resume Button       → wire to PauseMenuUI.Resume()
    ///        • Settings Button     → wire to PauseMenuUI.OpenSettings()
    ///   3. Create a child Panel called "SettingsPanel" and attach SettingsUI to it.
    ///   4. Attach this script to any persistent GameObject in SampleScene.
    ///   5. Assign all serialized fields in the Inspector.
    ///   6. Press Escape (or the configured pauseKey) to toggle the pause menu.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private SettingsUI settingsUI;        // SettingsPanel nested inside PauseMenuPanel

        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;

        [Header("Configuration")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        // ── State ──────────────────────────────────────────────────────
        private bool isPaused;

        // ── Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton  != null) restartButton.onClick.AddListener(Restart);
            if (settingsButton  != null) settingsButton.onClick.AddListener(OpenSettings);

            HidePauseMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (isPaused) Resume();
                else          Pause();
            }
        }

        // ── Public API (called by buttons and from Update) ─────────────

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            ShowPauseMenu();
        }

        public void Resume()
        {
            // If settings panel is open, close it first instead of fully resuming
            if (settingsUI != null && settingsUI.gameObject.activeSelf)
            {
                settingsUI.Hide();
                return;
            }

            isPaused = false;
            Time.timeScale = 1f;
            HidePauseMenu();
        }

        public void Restart()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (GameManager.Instance != null && GameManager.Instance.IsNetworkActive())
            {
                GameManager.Instance.Rematch();
                HidePauseMenu();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void OpenSettings()
        {
            if (settingsUI != null) settingsUI.Show();
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (settingsUI     != null) settingsUI.Hide(); // always start with settings closed
        }

        private void HidePauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsUI     != null) settingsUI.Hide();
        }
    }
}
