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
        spawnedTransform.localScale = prefab.localScale * scaleMultiplier;

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

        // Preserve the prefab's original thickness and length proportions
        Vector3 originalScale = lineCompletePrefab.localScale;

        // Calculate custom scale based on board size and win condition length
        float scaleMultiplier = 3f / GameManager.Instance.boardSize;
        float winConditionRatio = (float)GameManager.Instance.winCondition / 3f;
        
        // Tùy chỉnh độ dài và độ rõ (dày) theo yêu cầu
        float lengthBoost = 1.15f;    // Dài hơn 15%
        float thicknessBoost = 1.4f;  // Dày hơn 40% để nhìn rõ nét hơn

        // Xử lý sự khác biệt hình học: Cạnh góc vuông luôn ngắn hơn cạnh huyền (đường chéo)
        // Prefab gốc của bạn có vẻ được thiết kế cho đường chéo, nên khi dùng cho đường ngang/dọc nó bị dư ra.
        float orientationMultiplier = 1f;
        if (e.line.orientation == GameManager.Orientation.Horizontal || e.line.orientation == GameManager.Orientation.Vertical)
        {
            orientationMultiplier = 1f / Mathf.Sqrt(2f); // Thu ngắn lại theo tỷ lệ cạnh góc vuông / cạnh huyền (~0.707)
        }

        lineCompleteTransform.localScale = new Vector3(
            originalScale.x * scaleMultiplier * winConditionRatio * lengthBoost * orientationMultiplier, 
            originalScale.y * scaleMultiplier * thicknessBoost, 
            originalScale.z
        );

        // Kỹ thuật ép Layer: Đảm bảo vạch chiến thắng luôn đè lên X và O
        SpriteRenderer[] lineSprites = lineCompleteTransform.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer crossSprite = crossPrefab.GetComponentInChildren<SpriteRenderer>();
        string targetLayer = crossSprite != null ? crossSprite.sortingLayerName : "Default";

        foreach (SpriteRenderer sr in lineSprites)
        {
            sr.sortingLayerName = targetLayer; // Dùng chung Layer với chữ X
            sr.sortingOrder = 100; // Đặt số Order cực lớn để luôn đè lên trên cùng
        }

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

    private Vector2 GetWorldPosition(float x, float y)
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
