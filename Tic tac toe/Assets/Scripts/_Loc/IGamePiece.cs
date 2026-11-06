using UnityEngine;

public interface IGamePiece
{
    /// <summary>
    /// Được gọi ngay sau khi quân cờ được khởi tạo trên bàn cờ.
    /// </summary>
    /// <param name="scaleMultiplier">Tỷ lệ thu phóng dựa trên kích thước bàn cờ</param>
    void OnPlaced(float scaleMultiplier);
}
