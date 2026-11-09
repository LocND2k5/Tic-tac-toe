using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;
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

        [Header("Restart Confirmation Dialog (Multiplayer)")]
        [SerializeField] private GameObject restartDialogPanel;
        [SerializeField] private TextMeshProUGUI restartDialogText;
        [SerializeField] private Button restartAcceptButton;
        [SerializeField] private Button restartDeclineButton;
        [SerializeField] private Button restartCancelButton;

        // Custom Messaging Identifiers
        private const string MSG_RESTART_REQUEST = "TTT_RestartRequest";
        private const string MSG_RESTART_ACCEPT  = "TTT_RestartAccept";
        private const string MSG_RESTART_DECLINE = "TTT_RestartDecline";
        private const string MSG_RESTART_CANCEL  = "TTT_RestartCancel";

        // ── State ──────────────────────────────────────────────────────
        private bool isPaused;
        private Coroutine autoCloseNoticeCoroutine;

        // ── Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton  != null) restartButton.onClick.AddListener(Restart);
            if (menuButton     != null) menuButton.onClick.AddListener(ReturnToMenu);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);

            HidePauseMenu();
        }

        private void Start()
        {
            HookLeaveMatchButtons();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRematch += GameManager_OnRematch;
                GameManager.Instance.OnMatchInterrupted += GameManager_OnMatchInterrupted;
            }

            RegisterNetworkMessages();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRematch -= GameManager_OnRematch;
                GameManager.Instance.OnMatchInterrupted -= GameManager_OnMatchInterrupted;
            }

            UnregisterNetworkMessages();
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
                HidePauseMenu();
                StartInGameRestartRequest();
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

            GameConfig.ReturnTarget = ReturnTargetPanel.None;
            SceneManager.LoadScene(MultiplayerConstants.SCENE_MAIN_MENU);
        }

        // ── In-Game Restart Handshake (Multiplayer) ────────────────────

        private void StartInGameRestartRequest()
        {
            EnsureRestartDialog();
            ShowWaitingDialog();

            string myName = string.IsNullOrEmpty(PlayerProfile.Name) ? "Opponent" : PlayerProfile.Name;
            SendNetworkMessage(MSG_RESTART_REQUEST, myName);
        }

        private void OnRestartRequestReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string requesterName);
            if (string.IsNullOrEmpty(requesterName)) requesterName = "Opponent";

            // Đóng Pause Menu nếu người nhận đang mở
            isPaused = false;
            Time.timeScale = 1f;
            HidePauseMenu();

            EnsureRestartDialog();
            ShowRequestDialog($"{requesterName} wants to restart");
        }

        private void OnAcceptRestartClicked()
        {
            HideRestartDialog();
            SendNetworkMessage(MSG_RESTART_ACCEPT);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Rematch();
            }
        }

        private void OnRestartAcceptReceived(ulong senderClientId, FastBufferReader reader)
        {
            HideRestartDialog();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Rematch();
            }
        }

        private void OnDeclineRestartClicked()
        {
            HideRestartDialog();
            SendNetworkMessage(MSG_RESTART_DECLINE);
            // Tiếp tục chơi ván đang dở
        }

        private void OnRestartDeclineReceived(ulong senderClientId, FastBufferReader reader)
        {
            ShowNoticeDialog("Opponent declined the restart request.");
        }

        private void OnCancelRestartClicked()
        {
            HideRestartDialog();
            SendNetworkMessage(MSG_RESTART_CANCEL);
        }

        private void OnRestartCancelReceived(ulong senderClientId, FastBufferReader reader)
        {
            HideRestartDialog();
        }

        private void GameManager_OnRematch(object sender, EventArgs e)
        {
            HideRestartDialog();
        }

        private void GameManager_OnMatchInterrupted(object sender, GameManager.OnMatchInterruptedEventArgs e)
        {
            HideRestartDialog();
        }

        // ── Custom Messaging ──────────────────────────────────────────

        private void RegisterNetworkMessages()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.CustomMessagingManager == null) return;

            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                MSG_RESTART_REQUEST, OnRestartRequestReceived);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                MSG_RESTART_ACCEPT, OnRestartAcceptReceived);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                MSG_RESTART_DECLINE, OnRestartDeclineReceived);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                MSG_RESTART_CANCEL, OnRestartCancelReceived);
        }

        private void UnregisterNetworkMessages()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.CustomMessagingManager == null) return;

            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MSG_RESTART_REQUEST);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MSG_RESTART_ACCEPT);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MSG_RESTART_DECLINE);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MSG_RESTART_CANCEL);
        }

        private void SendNetworkMessage(string messageName, string payload = "")
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

            using (var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(payload), Allocator.Temp))
            {
                writer.WriteValueSafe(payload);

                if (NetworkManager.Singleton.IsServer)
                {
                    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        if (clientId != NetworkManager.ServerClientId)
                        {
                            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageName, clientId, writer);
                        }
                    }
                }
                else
                {
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                        messageName, NetworkManager.ServerClientId, writer);
                }
            }
        }

        // ── Dialog UI Building & Management ───────────────────────────

        private void EnsureRestartDialog()
        {
            if (restartDialogPanel != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Root panel phủ toàn màn hình làm mờ nền
            GameObject overlay = new GameObject("InGameRestartDialog", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas.rootCanvas.transform, false);

            RectTransform overlayRt = (RectTransform)overlay.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.sizeDelta = Vector2.zero;
            overlayRt.anchoredPosition = Vector2.zero;

            Image overlayImg = overlay.GetComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.7f);

            Canvas overlayCanvas = overlay.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 1100;
            overlay.AddComponent<GraphicRaycaster>();

            // Box hộp thoại ở giữa màn hình
            GameObject box = new GameObject("DialogBox", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(overlay.transform, false);

            RectTransform boxRt = (RectTransform)box.transform;
            boxRt.anchorMin = boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(620, 300);
            boxRt.anchoredPosition = Vector2.zero;

            Image boxImg = box.GetComponent<Image>();
            boxImg.color = new Color(0.08f, 0.08f, 0.22f, 0.98f);

            // Text thông báo
            GameObject textGo = new GameObject("MessageText", typeof(RectTransform));
            textGo.transform.SetParent(box.transform, false);
            RectTransform textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 0.5f);
            textRt.anchoredPosition = new Vector2(0, 55);
            textRt.sizeDelta = new Vector2(560, 120);

            restartDialogText = textGo.AddComponent<TextMeshProUGUI>();
            restartDialogText.text = "";
            restartDialogText.fontSize = 32;
            restartDialogText.enableAutoSizing = true;
            restartDialogText.fontSizeMin = 20;
            restartDialogText.fontSizeMax = 34;
            restartDialogText.fontStyle = FontStyles.Bold;
            restartDialogText.alignment = TextAlignmentOptions.Center;
            restartDialogText.color = Color.white;
            restartDialogText.raycastTarget = false;

            // Nút bấm: Accept / Decline / Cancel
            restartAcceptButton = CreateDialogButton(box.transform, "ACCEPT", new Color(0.18f, 0.72f, 0.44f), new Vector2(-135, -70));
            restartDeclineButton = CreateDialogButton(box.transform, "DECLINE", new Color(0.85f, 0.28f, 0.28f), new Vector2(135, -70));
            restartCancelButton = CreateDialogButton(box.transform, "CANCEL", new Color(0.45f, 0.50f, 0.58f), new Vector2(0, -70));

            restartAcceptButton.onClick.AddListener(OnAcceptRestartClicked);
            restartDeclineButton.onClick.AddListener(OnDeclineRestartClicked);
            restartCancelButton.onClick.AddListener(OnCancelRestartClicked);

            restartDialogPanel = overlay;
            restartDialogPanel.SetActive(false);
        }

        private Button CreateDialogButton(Transform parent, string label, Color color, Vector2 pos)
        {
            GameObject go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            go.name = label + "Button";
            go.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(200, 65);

            go.GetComponent<Image>().color = color;

            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return go.GetComponent<Button>();
        }

        private void ShowWaitingDialog()
        {
            if (autoCloseNoticeCoroutine != null) StopCoroutine(autoCloseNoticeCoroutine);

            EnsureRestartDialog();
            restartDialogText.text = "Waiting for opponent...";
            restartAcceptButton.gameObject.SetActive(false);
            restartDeclineButton.gameObject.SetActive(false);
            restartCancelButton.gameObject.SetActive(true);

            restartDialogPanel.transform.SetAsLastSibling();
            restartDialogPanel.SetActive(true);
        }

        private void ShowRequestDialog(string message)
        {
            if (autoCloseNoticeCoroutine != null) StopCoroutine(autoCloseNoticeCoroutine);

            EnsureRestartDialog();
            restartDialogText.text = message;
            restartAcceptButton.gameObject.SetActive(true);
            restartDeclineButton.gameObject.SetActive(true);
            restartCancelButton.gameObject.SetActive(false);

            restartDialogPanel.transform.SetAsLastSibling();
            restartDialogPanel.SetActive(true);
        }

        private void ShowNoticeDialog(string message)
        {
            EnsureRestartDialog();
            restartDialogText.text = message;
            restartAcceptButton.gameObject.SetActive(false);
            restartDeclineButton.gameObject.SetActive(false);
            restartCancelButton.gameObject.SetActive(false);

            restartDialogPanel.transform.SetAsLastSibling();
            restartDialogPanel.SetActive(true);

            if (autoCloseNoticeCoroutine != null) StopCoroutine(autoCloseNoticeCoroutine);
            autoCloseNoticeCoroutine = StartCoroutine(AutoCloseNoticeRoutine());
        }

        private IEnumerator AutoCloseNoticeRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            HideRestartDialog();
        }

        private void HideRestartDialog()
        {
            if (autoCloseNoticeCoroutine != null)
            {
                StopCoroutine(autoCloseNoticeCoroutine);
                autoCloseNoticeCoroutine = null;
            }

            if (restartDialogPanel != null)
            {
                restartDialogPanel.SetActive(false);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void HookLeaveMatchButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                if (btn.gameObject.name == "LeaveMatchButton" || btn.gameObject.name == "ReturnButton")
                {
                    btn.onClick.AddListener(OnLeaveMatchClicked);
                }
            }
        }

        private void OnLeaveMatchClicked()
        {
            bool isMultiplayer = (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                                 || GameConfig.SelectedMode == GameMode.Multiplayer;

            if (isMultiplayer)
            {
                GameConfig.ReturnTarget = ReturnTargetPanel.Multiplayer;
                PlayerProfile.OpponentName = "";
            }
            else
            {
                // Chế độ Local hoặc đánh với máy (AI) quay về màn hình GridSizePanel
                GameConfig.ReturnTarget = ReturnTargetPanel.GridSize;
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
