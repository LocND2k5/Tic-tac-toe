using UnityEngine;

public class CirclePiece : MonoBehaviour, IGamePiece
{
    public void OnPlaced(float scaleMultiplier)
    {
        // Điều chỉnh kích thước quân cờ dựa trên kích thước bàn cờ
        transform.localScale = transform.localScale * scaleMultiplier;
        
        // TODO: Sau này bạn có thể thêm logic/animation/âm thanh RIÊNG biệt cho O ở đây
        // Ví dụ: GetComponent<Animator>().Play("CircleSpawn");
    }
}
