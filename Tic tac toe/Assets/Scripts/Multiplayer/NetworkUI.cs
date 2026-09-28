using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using Khang.Core;

namespace TicTacToe.Multiplayer
{
    public class NetworkUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _buttonsContainer;

        [Header("Grid Size Selection (Host)")]
        [SerializeField] private GameObject _gridSelectionContainer;
        [SerializeField] private Button _size3x3Button;
        [SerializeField] private Button _size5x5Button;
        [SerializeField] private Button _size7x7Button;
        [SerializeField] private TextMeshProUGUI _selectedGridText;

        private int _selectedGridSize = 3;

        private void Awake()
        {
            SetupGridButtons();
        }

        private void Start()
        {
            if (_hostButton != null)
            {
                _hostButton.onClick.AddListener(StartHost);
            }

            if (_clientButton != null)
            {
                _clientButton.onClick.AddListener(StartClient);
            }

            if (_disconnectButton != null)
            {
                _disconnectButton.onClick.AddListener(Disconnect);
                _disconnectButton.gameObject.SetActive(false);
            }

            if (_startGameButton != null)
            {
                _startGameButton.onClick.AddListener(StartGameSession);
                _startGameButton.gameObject.SetActive(false);
            }

            if (_gridSelectionContainer != null)
            {
                _gridSelectionContainer.SetActive(false);
            }

            SetStatus(MultiplayerConstants.STATUS_OFFLINE);
            SubscribeToNetworkEvents();
        }

        private void SetupGridButtons()
        {
            if (_size3x3Button != null) _size3x3Button.onClick.AddListener(() => SelectGridSize(3));
            if (_size5x5Button != null) _size5x5Button.onClick.AddListener(() => SelectGridSize(5));
            if (_size7x7Button != null) _size7x7Button.onClick.AddListener(() => SelectGridSize(7));

            SelectGridSize(3);
        }

        private void SelectGridSize(int size)
        {
            _selectedGridSize = size;
            GameConfig.GridSize = size;

            if (_selectedGridText != null)
            {
                _selectedGridText.text = $"Board Size: {size}x{size}";
            }

            UpdateButtonVisual(_size3x3Button, size == 3);
            UpdateButtonVisual(_size5x5Button, size == 5);
            UpdateButtonVisual(_size7x7Button, size == 7);
        }

        private void UpdateButtonVisual(Button btn, bool isSelected)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected 
                    ? new Color(0.95f, 0.65f, 0.15f, 1f)  // Gold/Orange (Selected)
                    : new Color(0.25f, 0.30f, 0.40f, 0.8f); // Dark Slate (Unselected)
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnect;
            NetworkManager.Singleton.OnServerStarted += NetworkManager_OnServerStarted;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnect;
            NetworkManager.Singleton.OnServerStarted -= NetworkManager_OnServerStarted;
        }

        private void StartHost()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[NetworkUI] NetworkManager.Singleton is null!");
                return;
            }

            ConfigureTransport();
            ConfigureConnectionApproval();

            if (NetworkManager.Singleton.StartHost())
            {
                SetButtonsVisible(false);
                SetDisconnectButtonVisible(true);
                if (_gridSelectionContainer != null) _gridSelectionContainer.SetActive(true);
                SetStatus(string.Format(MultiplayerConstants.STATUS_HOSTING_WAITING, MultiplayerConstants.DEFAULT_PORT));
            }
            else
            {
                SetStatus("Failed to Start Host.");
            }
        }

        private void StartClient()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[NetworkUI] NetworkManager.Singleton is null!");
                return;
            }

            ConfigureTransport();

            SetButtonsVisible(false);
            SetDisconnectButtonVisible(true);
            if (_gridSelectionContainer != null) _gridSelectionContainer.SetActive(false);
            SetStatus(string.Format(MultiplayerConstants.STATUS_CONNECTING, MultiplayerConstants.DEFAULT_IP, MultiplayerConstants.DEFAULT_PORT));

            if (!NetworkManager.Singleton.StartClient())
            {
                SetStatus("Failed to initialize Client.");
                SetButtonsVisible(true);
                SetDisconnectButtonVisible(false);
            }
        }

        private void Disconnect()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            SetButtonsVisible(true);
            SetDisconnectButtonVisible(false);
            if (_gridSelectionContainer != null) _gridSelectionContainer.SetActive(false);
            if (_startGameButton != null) _startGameButton.gameObject.SetActive(false);
            SetStatus(MultiplayerConstants.STATUS_OFFLINE);
        }

        private void StartGameSession()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                GameConfig.GridSize = _selectedGridSize;
                NetworkManager.Singleton.SceneManager.LoadScene(MultiplayerConstants.SCENE_GAMEPLAY, LoadSceneMode.Single);
            }
        }

        private void ConfigureTransport()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(MultiplayerConstants.DEFAULT_IP, MultiplayerConstants.DEFAULT_PORT);
            }
        }

        private void ConfigureConnectionApproval()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MultiplayerConstants.MAX_PLAYERS)
            {
                response.Approved = false;
                response.Reason = "Session is full (Maximum 2 players allowed).";
                Debug.LogWarning("[NetworkUI] Connection rejected: Session is full.");
            }
            else
            {
                response.Approved = true;
                response.CreatePlayerObject = false;
                response.Pending = false;
            }
        }

        private void NetworkManager_OnServerStarted()
        {
            SetStatus(string.Format(MultiplayerConstants.STATUS_HOSTING_WAITING, MultiplayerConstants.DEFAULT_PORT));
        }

        private void NetworkManager_OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    SetStatus("Opponent Joined! Select board size and start match.");
                    if (_startGameButton != null) _startGameButton.gameObject.SetActive(true);
                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                SetStatus("Connected to Host! Waiting for Host to start match...");
            }
        }

        private void NetworkManager_OnClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;

            if (NetworkManager.Singleton.IsHost)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    SetStatus("Opponent left lobby. Waiting for new player...");
                    if (_startGameButton != null) _startGameButton.gameObject.SetActive(false);
                }
            }
            else
            {
                SetStatus(MultiplayerConstants.STATUS_DISCONNECTED);
                SetButtonsVisible(true);
                SetDisconnectButtonVisible(false);
                if (_gridSelectionContainer != null) _gridSelectionContainer.SetActive(false);
                if (_startGameButton != null) _startGameButton.gameObject.SetActive(false);
            }
        }

        private void SetStatus(string text)
        {
            if (_statusText != null)
            {
                _statusText.text = text;
            }
        }

        private void SetButtonsVisible(bool isVisible)
        {
            if (_buttonsContainer != null)
            {
                _buttonsContainer.SetActive(isVisible);
            }
            else
            {
                if (_hostButton != null) _hostButton.gameObject.SetActive(isVisible);
                if (_clientButton != null) _clientButton.gameObject.SetActive(isVisible);
            }
        }

        private void SetDisconnectButtonVisible(bool isVisible)
        {
            if (_disconnectButton != null)
            {
                _disconnectButton.gameObject.SetActive(isVisible);
            }
        }
    }
}
