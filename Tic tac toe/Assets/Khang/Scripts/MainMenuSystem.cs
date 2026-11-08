using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Khang.Core;

namespace Khang
{
    // =========================================================================
    // DEPENDENCY INVERSION PRINCIPLE (DIP)
    // 1. Abstraction (Giao diện trừu tượng)
    // =========================================================================
    public interface IMenuState
    {
        void EnterState();
        void ExitState();
    }

    // =========================================================================
    // 2. Low-Level Modules (Các Panel cụ thể tuân thủ hợp đồng IMenuState)
    // =========================================================================
    public class GenericPanelState : IMenuState
    {
        private readonly GameObject panel;

        public GenericPanelState(GameObject panel)
        {
            this.panel = panel;
        }

        public void EnterState()
        {
            if (panel != null) panel.SetActive(true);
        }

        public void ExitState()
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    public class SettingsPanelState : IMenuState
    {
        private readonly GameObject settingsPanel;

        public SettingsPanelState(GameObject settingsPanel)
        {
            this.settingsPanel = settingsPanel;
        }

        public void EnterState()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void ExitState()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }

    // =========================================================================
    // 3. High-Level Module (Hệ thống Menu chỉ phụ thuộc vào IMenuState)
    // =========================================================================
    public class MainMenuSystem : MonoBehaviour
    {
        public static MainMenuSystem Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject multiplayerPanel;
        [SerializeField] private GameObject gridSizePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playLocalButton;
        [SerializeField] private Button playAIButton;
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button exitButton;

        [Header("Grid Size Buttons (Play Local)")]
        [SerializeField] private Button size3x3Button;
        [SerializeField] private Button size5x5Button;
        [SerializeField] private Button size7x7Button;

        [Header("Navigation Buttons")]
        [SerializeField] private Button[] backToMainButtons;

        // Dependency Inversion: Chỉ phụ thuộc vào Abstraction IMenuState
        private IMenuState currentState;

        private IMenuState stateMain;
        private IMenuState stateMultiplayer;
        private IMenuState stateGridSize;
        private IMenuState stateSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Khởi tạo các State cho từng panel (Low-level Modules)
            stateMain = new GenericPanelState(mainPanel);
            stateMultiplayer = new GenericPanelState(multiplayerPanel);
            stateGridSize = new GenericPanelState(gridSizePanel);
            stateSettings = new SettingsPanelState(settingsPanel);

            // Ẩn các panel phụ lúc đầu
            if (multiplayerPanel) multiplayerPanel.SetActive(false);
            if (gridSizePanel) gridSizePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
        }

        private void Start()
        {
            SetupButtons();
            ChangeState(stateMain);
        }

        private void SetupButtons()
        {
            // Các nút chuyển giao diện thông thường
            if (playLocalButton)
                playLocalButton.onClick.AddListener(() => ChangeState(stateGridSize));

            if (playAIButton)
                playAIButton.onClick.AddListener(() => Debug.Log("Chế độ chơi với Máy (AI) đang được phát triển!"));

            if (playOnlineButton)
                playOnlineButton.onClick.AddListener(() => ChangeState(stateMultiplayer));

            if (openSettingsButton)
                openSettingsButton.onClick.AddListener(() => ChangeState(stateSettings));

            if (exitButton)
                exitButton.onClick.AddListener(QuitApp);

            // Nút chọn kích thước bàn cờ và vào trận trực tiếp
            if (size3x3Button)
                size3x3Button.onClick.AddListener(() => StartMatch(GameMode.Local, 3));

            if (size5x5Button)
                size5x5Button.onClick.AddListener(() => StartMatch(GameMode.Local, 5));

            if (size7x7Button)
                size7x7Button.onClick.AddListener(() => StartMatch(GameMode.Local, 7));

            // Các nút Back quay về Main Panel
            foreach (var btn in backToMainButtons)
            {
                if (btn != null)
                    btn.onClick.AddListener(() => ChangeState(stateMain));
            }
        }

        public void StartMatch(GameMode mode, int gridSize)
        {
            GameConfig.SelectedMode = mode;
            GameConfig.GridSize = gridSize;
            SceneManager.LoadScene("SampleScene");
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitApp()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // =========================================================================
        // DEPENDENCY INVERSION: Đổi panel thông qua IMenuState trừu tượng
        // =========================================================================
        public void ChangeState(IMenuState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }
    }
}
