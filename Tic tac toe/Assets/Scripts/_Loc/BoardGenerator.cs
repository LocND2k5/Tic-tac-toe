using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator Instance { get; private set; }

    [SerializeField] private Sprite lineSprite; // Assign your Line.png here

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Clean up previous children if any to support dynamic regeneration
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        int size = GameManager.Instance != null ? GameManager.Instance.boardSize : 3;
        float baseSpacing = 3.1f;
        float scaleMultiplier = 3f / size; // Scale down for larger boards so it fits screen
        float spacing = baseSpacing * scaleMultiplier;

        // 1. Generate invisible hitboxes (GridPositions)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject gridGo = new GameObject($"GridPosition_{x}_{y}");
                gridGo.transform.SetParent(this.transform);
                
                float posX = -((size - 1) * spacing / 2f) + x * spacing;
                float posY = -((size - 1) * spacing / 2f) + y * spacing;
                gridGo.transform.position = new Vector2(posX, posY);

                BoxCollider2D col = gridGo.AddComponent<BoxCollider2D>();
                col.size = new Vector2(spacing, spacing); // Fit cell perfectly

                GridPosition gp = gridGo.AddComponent<GridPosition>();
                gp.SetPosition(x, y); 
            }
        }

        // 2. Generate Visual Grid Lines
        if (lineSprite == null) 
        {
            Debug.LogWarning("Line Sprite is missing in BoardGenerator!");
            return;
        }
        
        float lineLength = size * spacing;
        float lineThickness = 0.5f * scaleMultiplier; // Make it a bit thicker so it's clearly visible

        float spriteWidth = lineSprite.bounds.size.x;
        float spriteHeight = lineSprite.bounds.size.y;

        // Vertical lines
        for (int i = 1; i < size; i++)
        {
            GameObject lineGo = new GameObject($"Line_V_{i}");
            lineGo.transform.SetParent(this.transform);
            
            float posX = -((size - 1) * spacing / 2f) + (i - 0.5f) * spacing;
            lineGo.transform.position = new Vector2(posX, 0);
            
            SpriteRenderer sr = lineGo.AddComponent<SpriteRenderer>();
            sr.sprite = lineSprite;
            sr.sortingLayerName = "Line";
            
            // Sprite is horizontal. To make it vertical, rotate 90 degrees.
            lineGo.transform.rotation = Quaternion.Euler(0, 0, 90);
            // Scale X affects length (because it's horizontally drawn), Scale Y affects thickness
            lineGo.transform.localScale = new Vector3(lineLength / spriteWidth, lineThickness / spriteHeight, 1); 
        }

        // Horizontal lines
        for (int i = 1; i < size; i++)
        {
            GameObject lineGo = new GameObject($"Line_H_{i}");
            lineGo.transform.SetParent(this.transform);
            
            float posY = -((size - 1) * spacing / 2f) + (i - 0.5f) * spacing;
            lineGo.transform.position = new Vector2(0, posY);
            
            SpriteRenderer sr = lineGo.AddComponent<SpriteRenderer>();
            sr.sprite = lineSprite;
            sr.sortingLayerName = "Line";
            
            // Sprite is already horizontal. No rotation needed.
            lineGo.transform.rotation = Quaternion.Euler(0, 0, 0);
            lineGo.transform.localScale = new Vector3(lineLength / spriteWidth, lineThickness / spriteHeight, 1); 
        }
    }
}
