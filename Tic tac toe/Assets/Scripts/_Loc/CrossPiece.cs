using UnityEngine;

public class CrossPiece : MonoBehaviour, IGamePiece
{
    public void OnPlaced(float scaleMultiplier)
    {
        // Điều chỉnh kích thước quân cờ dựa trên kích thước bàn cờ
        transform.localScale = transform.localScale * scaleMultiplier;
        
        // TODO: Sau này bạn có thể thêm logic/animation/âm thanh RIÊNG biệt cho X ở đây
        // Ví dụ: GetComponent<Animator>().Play("CrossSpawn");
    }
}
