using System;
using UnityEngine;

public class GameTurnUI : MonoBehaviour
{
    [Header("Turn Arrow Indicators")]
    [SerializeField] private GameObject crossArrow;
    [SerializeField] private GameObject circleArrow;

    [Header("You Text Indicator")]
    [SerializeField] private RectTransform youTextRect;
    [SerializeField] private float crossYouPosX = 60f;
    [SerializeField] private float circleYouPosX = -40f;

    private void Start()
    {
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnRematch += GameManager_OnRematch;

        UpdateTurnUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrentPlayablePlayerTypeChanged -= GameManager_OnCurrentPlayablePlayerTypeChanged;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
        }
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateTurnUI();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        UpdateTurnUI();
    }

    private void UpdateTurnUI()
    {
        GameManager.PlayerType currentPlayerType = GameManager.Instance.GetCurrentPlayablePlayerType();

        // Toggling Active State of two arrows
        if (crossArrow != null && circleArrow != null)
        {
            crossArrow.SetActive(currentPlayerType == GameManager.PlayerType.Cross);
            circleArrow.SetActive(currentPlayerType == GameManager.PlayerType.Circle);
        }

        // Move the "YOU" text
        if (youTextRect != null)
        {
            Vector2 currentPos = youTextRect.anchoredPosition;
            if (currentPlayerType == GameManager.PlayerType.Cross)
            {
                currentPos.x = crossYouPosX;
            }
            else if (currentPlayerType == GameManager.PlayerType.Circle)
            {
                currentPos.x = circleYouPosX;
            }
            youTextRect.anchoredPosition = currentPos;
        }
    }
}
