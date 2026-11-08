using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Khang.Core;
using TicTacToe.Multiplayer;

namespace Khang
{
    /// <summary>
    /// In-game Pause Menu overlay for SampleScene.
    ///
    /// Setup:
    ///   1. Create a full-screen Panel called "PauseMenuPanel" (child of Canvas).
    ///   2. Inside it add:
    ///        • Resume Button       → wire to PauseMenuUI.Resume()
    ///        • Restart Button      → wire to PauseMenuUI.Restart()
    ///        • Menu Button        → wire to PauseMenuUI.ReturnToMenu()
    ///   3. Keep the Settings button outside PauseMenuPanel and wire it to OpenSettings().
    ///   4. Create a SettingsPanel and attach SettingsUI to it.
    ///   5. Attach this script to any persistent GameObject in SampleScene.
    ///   6. Assign all serialized fields in the Inspector.
    ///   7. Press Escape (or the configured pauseKey) to toggle the pause menu.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private SettingsUI settingsUI;

        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Gameplay Buttons")]
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
            if (menuButton     != null) menuButton.onClick.AddListener(ReturnToMenu);
            if (settingsButton  != null) settingsButton.onClick.AddListener(OpenSettings);

            HidePauseMenu();
        }

        private void Start()
        {
            HookMultiplayerLeaveButtons();
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

        public void ReturnToMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (settingsUI != null) settingsUI.Hide();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            GameConfig.ReturnToMultiplayer = false;
            SceneManager.LoadScene(MultiplayerConstants.SCENE_MAIN_MENU);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void HookMultiplayerLeaveButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                if (btn.gameObject.name == "LeaveMatchButton" || btn.gameObject.name == "ReturnButton")
                {
                    btn.onClick.AddListener(OnMultiplayerLeaveClicked);
                }
            }
        }

        private void OnMultiplayerLeaveClicked()
        {
            bool isMultiplayer = (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                                 || GameConfig.SelectedMode == GameMode.Multiplayer;

            if (isMultiplayer)
            {
                GameConfig.ReturnToMultiplayer = true;
                PlayerProfile.OpponentName = "";
            }
        }

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
