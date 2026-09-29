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
        GameLocator.GetTurnProvider().OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameLocator.GetTurnProvider().OnRematch += GameManager_OnRematch;

        UpdateTurnUI();
    }

    private void OnDestroy()
    {
        if (GameLocator.GetTurnProvider() != null)
        {
            GameLocator.GetTurnProvider().OnCurrentPlayablePlayerTypeChanged -= GameManager_OnCurrentPlayablePlayerTypeChanged;
            GameLocator.GetTurnProvider().OnRematch -= GameManager_OnRematch;
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
        PlayerType currentPlayerType = GameLocator.GetTurnProvider().GetCurrentPlayablePlayerType();

        // Toggling Active State of two arrows
        if (crossArrow != null && circleArrow != null)
        {
            crossArrow.SetActive(currentPlayerType == PlayerType.Cross);
            circleArrow.SetActive(currentPlayerType == PlayerType.Circle);
        }

        // Move the "YOU" text
        if (youTextRect != null)
        {
            Vector2 currentPos = youTextRect.anchoredPosition;
            if (currentPlayerType == PlayerType.Cross)
            {
                currentPos.x = crossYouPosX;
            }
            else if (currentPlayerType == PlayerType.Circle)
            {
                currentPos.x = circleYouPosX;
            }
            youTextRect.anchoredPosition = currentPos;
        }
    }
}
