using UnityEngine;

namespace TicTacToe.Multiplayer
{
    // Tên người chơi: lưu trên máy (PlayerPrefs), tên đối thủ được trao đổi khi vào phòng
    public static class PlayerProfile
    {
        public const int MaxNameLength = 12;
        public const string DefaultName = "Player";

        // Khóa lưu theo đường dẫn project: các cửa sổ Multiplayer Play Mode (và bản build khác nhau)
        // dùng chung PlayerPrefs nhưng sẽ không ghi đè / tự điền tên của nhau
        private static string prefsKey;
        private static string PrefsKey => prefsKey ??= "PlayerName_" + StableHash(Application.dataPath);

        private static string StableHash(string s)
        {
            uint h = 2166136261;
            foreach (char c in s) h = (h ^ c) * 16777619;
            return h.ToString("x8");
        }

        public static string OpponentName = "";

        public static bool HasSavedName => PlayerPrefs.HasKey(PrefsKey);

        // Giữ tên trong bộ nhớ của tiến trình này: PlayerPrefs có thể dùng chung giữa nhiều bản chạy
        // trên cùng một máy (Multiplayer Play Mode, nhiều bản build), nên không đọc lại từ đó sau khi đã đặt tên.
        private static string currentName;

        public static string Name
        {
            get => currentName ?? PlayerPrefs.GetString(PrefsKey, DefaultName);
            set
            {
                currentName = Sanitize(value);
                PlayerPrefs.SetString(PrefsKey, currentName);
                PlayerPrefs.Save();
            }
        }

        public static string Sanitize(string raw)
        {
            string name = (raw ?? "").Trim();
            if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);
            return string.IsNullOrEmpty(name) ? DefaultName : name;
        }
    }
}
