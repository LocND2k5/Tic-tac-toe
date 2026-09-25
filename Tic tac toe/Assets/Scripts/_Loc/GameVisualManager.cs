using System;
using System.Collections.Generic;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
{
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList = new List<GameObject>();

    private void Awake()
    {
        if (visualGameObjectList == null)
        {
            visualGameObjectList = new List<GameObject>();
        }
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClickedOnGridPosition -= GameManager_OnClickedOnGridPosition;
            GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
        }
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Transform prefab = (e.playerType == GameManager.PlayerType.Cross) ? crossPrefab : circlePrefab;
        Transform spawnedTransform = Instantiate(prefab, GetWorldPosition(e.x, e.y), Quaternion.identity);
        
        float scaleMultiplier = 3f / GameManager.Instance.boardSize;
        spawnedTransform.localScale = Vector3.one * scaleMultiplier;

        visualGameObjectList.Add(spawnedTransform.gameObject);
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (lineCompletePrefab == null) return;

        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }

        Transform lineCompleteTransform = Instantiate(
            lineCompletePrefab,
            GetWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
            Quaternion.Euler(0f, 0f, eulerZ)
        );

        // Calculate custom scale based on board size and win condition length
        float scaleMultiplier = 3f / GameManager.Instance.boardSize;
        float winConditionRatio = (float)GameManager.Instance.winCondition / 3f;
        lineCompleteTransform.localScale = new Vector3(scaleMultiplier * winConditionRatio, scaleMultiplier, 1f);

        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (visualGameObjectList != null)
        {
            foreach (GameObject visualGo in visualGameObjectList)
            {
                if (visualGo != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) DestroyImmediate(visualGo);
                    else Destroy(visualGo);
#else
                    Destroy(visualGo);
#endif
                }
            }
            visualGameObjectList.Clear();
        }
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        int size = GameManager.Instance.boardSize;
        float baseSpacing = 3.1f;
        float scaleMultiplier = 3f / size;
        float spacing = baseSpacing * scaleMultiplier;

        float posX = -((size - 1) * spacing / 2f) + x * spacing;
        float posY = -((size - 1) * spacing / 2f) + y * spacing;
        
        return new Vector2(posX, posY);
    }
}
