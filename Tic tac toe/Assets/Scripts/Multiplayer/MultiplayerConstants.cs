namespace TicTacToe.Multiplayer
{
    public static class MultiplayerConstants
    {
        // Network Transport Configuration
        public const ushort DEFAULT_PORT = 7777;
        public const string DEFAULT_IP = "127.0.0.1";
        public const int MAX_PLAYERS = 2;
        public const float CONNECTION_TIMEOUT_SECONDS = 10.0f;

        // Player Role Identification
        public const ulong HOST_CLIENT_ID = 0;

        // Status Messages
        public const string STATUS_OFFLINE = "Status: Offline (Local Hotseat)";
        public const string STATUS_HOSTING_WAITING = "Hosting ({0}) - Waiting for Opponent...";
        public const string STATUS_HOST_CONNECTED = "Host Connected! Playing as X";
        public const string STATUS_CLIENT_CONNECTED = "Connected! Playing as O";
        public const string STATUS_CONNECTING = "Connecting to {0}:{1}...";
        public const string STATUS_DISCONNECTED = "Disconnected from session.";

        // Scene Identifiers
        public const string SCENE_MAIN_MENU = "MainMenuScene";
        public const string SCENE_GAMEPLAY = "SampleScene";

        // Rematch & Disconnection Strings
        public const string REMATCH_DEFAULT = "REMATCH";
        public const string REMATCH_WAITING = "WAITING FOR OPPONENT...";
        public const string REMATCH_OPPONENT_REQUESTED = "OPPONENT WANTS REMATCH! (CLICK TO ACCEPT)";
        public const string MSG_OPPONENT_DISCONNECTED = "Opponent disconnected from the match.";
        public const string MSG_HOST_DISCONNECTED = "Host disconnected. Session closed.";
    }
}
