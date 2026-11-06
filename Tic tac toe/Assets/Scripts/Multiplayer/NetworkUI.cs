using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Khang.Core;

namespace TicTacToe.Multiplayer
{
    // Giao diện Multiplayer: Tạo phòng (nhận mã) / Tham gia bằng mã / Quay lại menu.
    // Có thể tự thiết kế trong Canvas rồi gán vào các ô bên dưới (xem menu Editor "TicTacToe/Build Multiplayer UI").
    // Nếu để trống, UI sẽ được dựng bằng code lúc chạy game.
    public class NetworkUI : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.95f, 0.65f, 0.15f);
        private static readonly Color UnselectedColor = new Color(0.25f, 0.30f, 0.40f, 0.9f);

        // Chỉ dùng khi UI được dựng bằng code lúc chạy game
        private const float UiScale = 0.7f;

        [Header("Views")]
        [SerializeField] private GameObject _menuView;
        [SerializeField] private GameObject _hostView;
        [SerializeField] private GameObject _joinView;

        [Header("Menu View")]
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinGameButton;
        [SerializeField] private Button _backToMenuButton;

        [Header("Host View")]
        [SerializeField] private TextMeshProUGUI _roomCodeText;
        [SerializeField] private TextMeshProUGUI _hostStatusText;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private TextMeshProUGUI _selectedGridText;
        [Tooltip("Thứ tự: 3x3, 5x5, 7x7")]
        [SerializeField] private Button[] _gridButtons = new Button[3];
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _closeRoomButton;

        [Header("Join View")]
        [SerializeField] private TMP_InputField _codeInput;
        [SerializeField] private Button _joinConfirmButton;
        [SerializeField] private TextMeshProUGUI _joinStatusText;
        [SerializeField] private Button _joinBackButton;

        private readonly int[] _gridSizes = { 3, 5, 7 };

        private int _selectedGridSize = 3;
        private string _roomCode = "";
        private bool _busy;
        private bool _initialized;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            SubscribeToNetworkEvents();
        }

        private void OnEnable()
        {
            Initialize();
            if (!IsSessionActive()) ShowView(_menuView);
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        // ------------------------------------------------------------
        // Setup
        // ------------------------------------------------------------
        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            if (_menuView == null || _hostView == null || _joinView == null)
            {
                BuildRuntimeUI();
            }

            WireUI();
        }

        private void BuildRuntimeUI()
        {
            // Ẩn toàn bộ UI cũ (Host/Join/Grid selection...) trong panel
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            MultiplayerMenuBuilder.Refs r = MultiplayerMenuBuilder.Build(transform, UiScale);
            _menuView = r.menuView; _hostView = r.hostView; _joinView = r.joinView;
            _nameInput = r.nameInput; _createRoomButton = r.createRoomButton; _joinGameButton = r.joinGameButton; _backToMenuButton = r.backToMenuButton;
            _roomCodeText = r.roomCodeText; _hostStatusText = r.hostStatusText; _copyCodeButton = r.copyCodeButton;
            _selectedGridText = r.selectedGridText; _gridButtons = r.gridButtons;
            _startGameButton = r.startGameButton; _closeRoomButton = r.closeRoomButton;
            _codeInput = r.codeInput; _joinConfirmButton = r.joinConfirmButton; _joinStatusText = r.joinStatusText; _joinBackButton = r.joinBackButton;
        }

        private void WireUI()
        {
            if (_nameInput != null)
            {
                _nameInput.characterLimit = PlayerProfile.MaxNameLength;
                if (PlayerProfile.HasSavedName) _nameInput.text = PlayerProfile.Name;
            }

            if (_codeInput != null)
            {
                _codeInput.onValidateInput += (text, index, ch) => char.ToUpperInvariant(ch);
            }

            Bind(_createRoomButton, OnCreateRoomClicked);
            Bind(_joinGameButton, () => ShowView(_joinView));
            Bind(_backToMenuButton, BackToMainMenu);
            Bind(_copyCodeButton, CopyRoomCode);
            Bind(_startGameButton, StartGameSession);
            Bind(_closeRoomButton, LeaveRoom);
            Bind(_joinConfirmButton, OnJoinClicked);
            Bind(_joinBackButton, LeaveRoom);

            for (int i = 0; i < _gridButtons.Length && i < _gridSizes.Length; i++)
            {
                int size = _gridSizes[i];
                Bind(_gridButtons[i], () => SelectGridSize(size));
            }

            SelectGridSize(3);
            if (_startGameButton != null) _startGameButton.gameObject.SetActive(false);
            ShowView(_menuView);
        }

        private static void Bind(Button button, Action onClick)
        {
            if (button != null) button.onClick.AddListener(() => onClick());
        }

        private void ShowView(GameObject view)
        {
            if (_menuView != null) _menuView.SetActive(view == _menuView);
            if (_hostView != null) _hostView.SetActive(view == _hostView);
            if (_joinView != null) _joinView.SetActive(view == _joinView);

            if (view == _joinView && _joinStatusText != null) _joinStatusText.text = "";
        }

        // ------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------
        private async void OnCreateRoomClicked()
        {
            if (_busy || NetworkManager.Singleton == null) return;
            _busy = true;
            SaveLocalName();
            PlayerProfile.OpponentName = "";

            ShowView(_hostView);
            _roomCodeText.text = "...";
            _hostStatusText.text = "Creating room...";
            _startGameButton.gameObject.SetActive(false);

            try
            {
                _roomCode = await RelayRoomService.CreateRoomAsync();

                ConfigureConnectionApproval();
                if (!NetworkManager.Singleton.StartHost())
                {
                    throw new Exception("Failed to start host.");
                }

                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ClientNameMessage, OnClientNameReceived);

                _roomCodeText.text = _roomCode;
                _hostStatusText.text = "Share this code with your friend. Waiting for opponent...";
            }
            catch (Exception e)
            {
                Debug.LogError("[NetworkUI] Create room failed: " + e);
                _roomCodeText.text = "------";
                _hostStatusText.text = "Could not create room. Check your internet / Unity Cloud setup.";
            }
            finally
            {
                _busy = false;
            }
        }

        private async void OnJoinClicked()
        {
            if (_busy || NetworkManager.Singleton == null) return;

            string code = (_codeInput.text ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code))
            {
                _joinStatusText.text = "Please enter a room code.";
                return;
            }

            _busy = true;
            SaveLocalName();
            PlayerProfile.OpponentName = "";
            _joinConfirmButton.interactable = false;
            _joinStatusText.text = "Joining room...";

            try
            {
                await RelayRoomService.JoinRoomAsync(code);

                // NetworkConfig của client phải trùng host (ConnectionApproval), nếu không host sẽ từ chối kết nối
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                if (!NetworkManager.Singleton.StartClient())
                {
                    throw new Exception("Failed to start client.");
                }
                _joinStatusText.text = "Connecting...";
            }
            catch (Exception e)
            {
                Debug.LogWarning("[NetworkUI] Join room failed: " + e.Message);
                _joinStatusText.text = "Room not found or unavailable. Check the code and try again.";
                _joinConfirmButton.interactable = true;
            }
            finally
            {
                _busy = false;
            }
        }

        private void SaveLocalName()
        {
            PlayerProfile.Name = _nameInput != null ? _nameInput.text : PlayerProfile.DefaultName;
        }

        // Trao đổi tên: client gửi tên của mình cho host, host trả lời bằng tên của host
        private const string ClientNameMessage = "TTT_ClientName";
        private const string HostNameMessage = "TTT_HostName";

        private static void SendName(string messageName, ulong targetClientId)
        {
            string name = PlayerProfile.Name;
            using (var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(name), Allocator.Temp))
            {
                writer.WriteValueSafe(name);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageName, targetClientId, writer);
            }
        }

        private void OnClientNameReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string name);
            PlayerProfile.OpponentName = PlayerProfile.Sanitize(name);
            SendName(HostNameMessage, senderClientId);

            if (_hostStatusText != null)
            {
                _hostStatusText.text = $"{PlayerProfile.OpponentName} joined! Select board size and start the match.";
            }
        }

        private void OnHostNameReceived(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string name);
            PlayerProfile.OpponentName = PlayerProfile.Sanitize(name);

            if (_joinStatusText != null)
            {
                _joinStatusText.text = $"Connected to {PlayerProfile.OpponentName}! Waiting for the host to start the match...";
            }
        }

        private void CopyRoomCode()
        {
            if (string.IsNullOrEmpty(_roomCode)) return;
            GUIUtility.systemCopyBuffer = _roomCode;
            _hostStatusText.text = "Code copied to clipboard!";
        }

        private void SelectGridSize(int size)
        {
            _selectedGridSize = size;
            GameConfig.GridSize = size;

            if (_selectedGridText != null)
            {
                _selectedGridText.text = $"Board Size: {size}x{size}";
            }

            for (int i = 0; i < _gridButtons.Length; i++)
            {
                if (_gridButtons[i] == null) continue;
                _gridButtons[i].GetComponent<Image>().color = _gridSizes[i] == size ? SelectedColor : UnselectedColor;
            }
        }

        private void StartGameSession()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                GameConfig.GridSize = _selectedGridSize;
                NetworkManager.Singleton.SceneManager.LoadScene(MultiplayerConstants.SCENE_GAMEPLAY, LoadSceneMode.Single);
            }
        }

        // Đóng phòng / rời phòng và quay lại màn hình chọn Tạo phòng - Join
        private void LeaveRoom()
        {
            ShutdownNetwork();
            PlayerProfile.OpponentName = "";
            _roomCode = "";
            _busy = false;
            if (_joinConfirmButton != null) _joinConfirmButton.interactable = true;
            if (_codeInput != null) _codeInput.text = "";
            ShowView(_menuView);
        }

        private void BackToMainMenu()
        {
            ShutdownNetwork();
            if (Khang.MainMenuSystem.Instance != null)
            {
                Khang.MainMenuSystem.Instance.ShowMainMenu();
            }
        }

        private static void ShutdownNetwork()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        private static bool IsSessionActive()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        }

        // ------------------------------------------------------------
        // Network events
        // ------------------------------------------------------------
        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnect;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnect;
        }

        private void ConfigureConnectionApproval()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MultiplayerConstants.MAX_PLAYERS)
            {
                response.Approved = false;
                response.Reason = "Session is full (Maximum 2 players allowed).";
            }
            else
            {
                response.Approved = true;
                response.CreatePlayerObject = false;
                response.Pending = false;
            }
        }

        private void NetworkManager_OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    if (string.IsNullOrEmpty(PlayerProfile.OpponentName))
                    {
                        _hostStatusText.text = "Opponent joined! Select board size and start the match.";
                    }
                    _startGameButton.gameObject.SetActive(true);
                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                _joinStatusText.text = "Connected! Waiting for the host to start the match...";
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(HostNameMessage, OnHostNameReceived);
                SendName(ClientNameMessage, NetworkManager.ServerClientId);
            }
        }

        private void NetworkManager_OnClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;

            if (NetworkManager.Singleton.IsHost)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    PlayerProfile.OpponentName = "";
                    _hostStatusText.text = "Opponent left. Waiting for a new player...";
                    _startGameButton.gameObject.SetActive(false);
                }
            }
            else
            {
                // Client mất kết nối với host
                if (_joinView != null && _joinView.activeInHierarchy)
                {
                    _joinStatusText.text = MultiplayerConstants.STATUS_DISCONNECTED;
                    _joinConfirmButton.interactable = true;
                }
            }
        }
    }
}
