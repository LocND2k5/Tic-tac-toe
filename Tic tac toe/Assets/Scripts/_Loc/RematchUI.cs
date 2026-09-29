using System;
using UnityEngine;
using UnityEngine.UI;

public class RematchUI : MonoBehaviour
{
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        if (rematchButton == null)
        {
            rematchButton = GetComponentInChildren<Button>(true);
        }

        if (rematchButton != null)
        {
            rematchButton.onClick.AddListener(() =>
            {
                GameLocator.GetGameStateProvider().Rematch();
            });
        }
    }

    private void Start()
    {
        GameLocator.GetGameStateProvider().OnGameWin += GameManager_OnGameWin;
        GameLocator.GetGameStateProvider().OnGameTied += GameManager_OnGameTied;
        GameLocator.GetGameStateProvider().OnRematch += GameManager_OnRematch;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameLocator.GetGameStateProvider() != null)
        {
            GameLocator.GetGameStateProvider().OnGameWin -= GameManager_OnGameWin;
            GameLocator.GetGameStateProvider().OnGameTied -= GameManager_OnGameTied;
            GameLocator.GetGameStateProvider().OnRematch -= GameManager_OnRematch;
        }
    }

    private void GameManager_OnGameWin(object sender, OnGameWinEventArgs e)
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
    }

    private void Show()
    {
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
}
