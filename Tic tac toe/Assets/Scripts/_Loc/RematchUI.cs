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
                GameManager.Instance.Rematch();
            });
        }
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;
        GameManager.Instance.OnRematch += GameManager_OnRematch;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
            GameManager.Instance.OnGameTied -= GameManager_OnGameTied;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
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
