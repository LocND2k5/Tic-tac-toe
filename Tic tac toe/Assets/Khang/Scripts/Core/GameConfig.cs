namespace Khang.Core
{
    public enum GameMode
    {
        Local,
        AI,
        Multiplayer
    }

    // Lớp tĩnh để lưu trữ cấu hình trước khi chuyển Scene
    public static class GameConfig
    {
        public static GameMode SelectedMode = GameMode.Local;
        public static int GridSize = 3; // 3x3, 5x5, 7x7
    }
}
