using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TicTacToe.Multiplayer;

public class RematchUI : MonoBehaviour
{
    [SerializeField] private Button rematchButton;
    [SerializeField] private TextMeshProUGUI rematchText;

    private void Awake()
    {
        if (rematchButton == null)
        {
            rematchButton = GetComponentInChildren<Button>(true);
        }

        if (rematchText == null && rematchButton != null)
        {
            rematchText = rematchButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (rematchButton != null)
        {
            rematchButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Rematch();

                    if (GameManager.Instance.IsNetworkActive())
                    {
                        // In online mode, wait for opponent to also accept
                        rematchButton.interactable = false;
                        if (rematchText != null)
                        {
                            rematchText.text = MultiplayerConstants.REMATCH_WAITING;
                        }
                    }
                }
            });
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin += GameManager_OnGameWin;
            GameManager.Instance.OnGameTied += GameManager_OnGameTied;
            GameManager.Instance.OnRematch += GameManager_OnRematch;
            GameManager.Instance.OnRematchRequested += GameManager_OnRematchRequested;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
            GameManager.Instance.OnGameTied -= GameManager_OnGameTied;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
            GameManager.Instance.OnRematchRequested -= GameManager_OnRematchRequested;
        }
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        Show();
    }

    private void GameManager_OnGameTied(object sender, EventArgs e)
    {
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
        ResetButtonState();
    }

    private void GameManager_OnRematchRequested(object sender, GameManager.OnRematchRequestedEventArgs e)
    {
        if (GameManager.Instance == null) return;

        // If the opponent requested a rematch and we haven't clicked yet
        if (e.requestingPlayer != GameManager.Instance.GetLocalPlayerType())
        {
            if (rematchButton != null && rematchButton.interactable)
            {
                if (rematchText != null)
                {
                    rematchText.text = MultiplayerConstants.REMATCH_OPPONENT_REQUESTED;
                }
            }
        }
    }

    private void Show()
    {
        ResetButtonState();
        if (rematchButton != null)
        {
            rematchButton.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (rematchButton != null)
        {
            rematchButton.gameObject.SetActive(false);
        }
    }

    private void ResetButtonState()
    {
        if (rematchButton != null)
        {
            rematchButton.interactable = true;
        }

        if (rematchText != null)
        {
            rematchText.text = MultiplayerConstants.REMATCH_DEFAULT;
        }
    }
}
