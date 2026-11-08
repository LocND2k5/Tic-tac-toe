namespace Khang.Core
{
    public enum GameMode
    {
        Local,
        AI,
        Multiplayer
    }

    public enum ReturnTargetPanel
    {
        None,
        Main,
        GridSize,
        Multiplayer
    }

    // Lớp tĩnh để lưu trữ cấu hình trước khi chuyển Scene
    public static class GameConfig
    {
        public static GameMode SelectedMode = GameMode.Local;
        public static int GridSize = 3; // 3x3, 5x5, 7x7
        public static ReturnTargetPanel ReturnTarget = ReturnTargetPanel.None;

        public static bool ReturnToMultiplayer
        {
            get => ReturnTarget == ReturnTargetPanel.Multiplayer;
            set => ReturnTarget = value ? ReturnTargetPanel.Multiplayer : ReturnTargetPanel.None;
        }
    }
}
