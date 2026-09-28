using UnityEngine;
using UnityEngine.UI;
using Khang.Core;
using Khang.Commands;
using Khang.Strategies;
using Khang.States;

namespace Khang
{
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

        [Header("Grid Size Buttons (Play Local)")]
        [SerializeField] private Button size3x3Button;
        [SerializeField] private Button size5x5Button;
        [SerializeField] private Button size7x7Button;

        [Header("Navigation Buttons")]
        [SerializeField] private Button[] backToMainButtons; // Nút Back/Close ở các màn hình phụ

        private IMenuState currentState;
        private IUIAnimationStrategy defaultAnimStrategy;

        // Các State
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
            
            defaultAnimStrategy = new InstantAnimationStrategy();

            // Khởi tạo các State cho từng Panel
            stateMain = new GenericPanelState(mainPanel, defaultAnimStrategy);
            stateMultiplayer = new GenericPanelState(multiplayerPanel, defaultAnimStrategy);
            stateGridSize = new GenericPanelState(gridSizePanel, defaultAnimStrategy);
            stateSettings = new GenericPanelState(settingsPanel, defaultAnimStrategy);

            // Ẩn tất cả các panel phụ lúc mới bật
            if (multiplayerPanel) multiplayerPanel.SetActive(false);
            if (gridSizePanel) gridSizePanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
        }

        private void Start()
        {
            SetupButtons();
            
            // Vào State chính của Main Menu
            ChangeState(stateMain);
        }

        private void SetupButtons()
        {
            // 1. Nút Play Local: Mở bảng chọn kích thước bàn cờ (Grid Size)
            if (playLocalButton)
            {
                playLocalButton.onClick.AddListener(() => new ChangeStateCommand(this, stateGridSize).Execute());
            }

            // 2. Nút Play AI: Hiện tại chưa có AI, tạm thời báo log
            if (playAIButton)
            {
                playAIButton.onClick.AddListener(() => 
                {
                    Debug.Log("Chế độ chơi với Máy (AI) đang được phát triển!");
                });
            }

            // 3. Nút Play Online: Mở bảng Multiplayer
            if (playOnlineButton)
                playOnlineButton.onClick.AddListener(() => new ChangeStateCommand(this, stateMultiplayer).Execute());
            
            // 4. Nút Settings
            if (openSettingsButton)
                openSettingsButton.onClick.AddListener(() => new ChangeStateCommand(this, stateSettings).Execute());

            // 5. Nút chọn Grid Size (Dành cho sau này nếu cần)
            if (size3x3Button)
                size3x3Button.onClick.AddListener(() => new StartMatchCommand(GameMode.Local, 3).Execute());
            
            if (size5x5Button)
                size5x5Button.onClick.AddListener(() => Debug.Log("Tính năng bàn cờ 5x5 đang được phát triển!"));
                
            if (size7x7Button)
                size7x7Button.onClick.AddListener(() => Debug.Log("Tính năng bàn cờ 7x7 đang được phát triển!"));

            // 6. Các nút Back để quay về Main Menu
            foreach (var btn in backToMainButtons)
            {
                if (btn != null)
                {
                    btn.onClick.AddListener(() => new ChangeStateCommand(this, stateMain).Execute());
                }
            }
        }

        public void ChangeState(IMenuState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }
    }
}
