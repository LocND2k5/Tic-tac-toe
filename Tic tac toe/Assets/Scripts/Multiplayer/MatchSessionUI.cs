using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

namespace TicTacToe.Multiplayer
{
    public class MatchSessionUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button _leaveButton;

        [Header("Disconnection Dialog")]
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TextMeshProUGUI _dialogTitleText;
        [SerializeField] private TextMeshProUGUI _dialogMessageText;
        [SerializeField] private Button _dialogReturnButton;

        private void Start()
        {
            if (_leaveButton != null)
            {
                _leaveButton.onClick.AddListener(LeaveMatch);
            }

            if (_dialogReturnButton != null)
            {
                _dialogReturnButton.onClick.AddListener(LeaveMatch);
            }

            if (_dialogPanel != null)
            {
                _dialogPanel.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMatchInterrupted += GameManager_OnMatchInterrupted;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMatchInterrupted -= GameManager_OnMatchInterrupted;
            }
        }

        private void GameManager_OnMatchInterrupted(object sender, GameManager.OnMatchInterruptedEventArgs e)
        {
            ShowDisconnectionDialog(e.reason);
        }

        public void ShowDisconnectionDialog(string message)
        {
            if (_dialogPanel != null)
            {
                if (_dialogMessageText != null)
                {
                    _dialogMessageText.text = message;
                }
                _dialogPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[MatchSessionUI] Disconnect Dialog triggered: {message}");
            }
        }

        public void LeaveMatch()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            SceneManager.LoadScene(MultiplayerConstants.SCENE_MAIN_MENU);
        }
    }
}
