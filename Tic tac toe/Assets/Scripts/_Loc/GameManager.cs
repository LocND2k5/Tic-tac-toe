using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TicTacToe.Multiplayer;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }

    public event EventHandler<OnRematchRequestedEventArgs> OnRematchRequested;
    public class OnRematchRequestedEventArgs : EventArgs
    {
        public PlayerType requestingPlayer;
    }

    public event EventHandler<OnMatchInterruptedEventArgs> OnMatchInterrupted;
    public class OnMatchInterruptedEventArgs : EventArgs
    {
        public string reason;
    }

    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler OnGameTied;
    public event EventHandler OnRematch;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2 centerGridPosition;
        public float rotationAngle;
        public float lengthMultiplier;
    }

    [System.Serializable]
    public struct BoardConfig
    {
        public string configName; // Tên hiển thị cho dễ nhìn (VD: "Mức 3x3")
        public int size;          // Kích thước thật (VD: 3)
        public int winCondition;  // Cần bao nhiêu ô để thắng (VD: 3)
    }

    [Header("Cài đặt các loại Bàn cờ (Board Setups)")]
    public BoardConfig[] availableConfigs;

    [HideInInspector] public int boardSize = 3;
    [HideInInspector] public int winCondition = 3;

    // Network synchronized board configuration
    private NetworkVariable<int> networkBoardSize = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> networkWinCondition = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private PlayerType[,] playerTypeArray;
    private PlayerType currentPlayablePlayerType;
    private List<Line> lineList;
    private bool isGameOver;

    // Two-player rematch handshake tracking (Server authoritative)
    private bool hostWantsRematch;
    private bool clientWantsRematch;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one GameManager instance! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Init();
    }

    private void Start()
    {
        SubscribeToNetworkCallbacks();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            networkBoardSize.Value = boardSize;
            networkWinCondition.Value = winCondition;
        }
        else
        {
            networkBoardSize.OnValueChanged += HandleNetworkBoardSizeChanged;
            if (networkBoardSize.Value != boardSize)
            {
                ApplySyncedBoardSize(networkBoardSize.Value, networkWinCondition.Value);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        networkBoardSize.OnValueChanged -= HandleNetworkBoardSizeChanged;
    }

    private void HandleNetworkBoardSizeChanged(int oldVal, int newVal)
    {
        ApplySyncedBoardSize(newVal, networkWinCondition.Value);
    }

    private void ApplySyncedBoardSize(int newSize, int newWinCond)
    {
        boardSize = newSize;
        winCondition = newWinCond;
        playerTypeArray = new PlayerType[boardSize, boardSize];
        InitLineList();

        if (BoardGenerator.Instance != null)
        {
            BoardGenerator.Instance.GenerateGrid();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromNetworkCallbacks();
    }

    public void Init()
    {
        int targetSize = MainMenuUI.SelectedBoardSize;
        if (Khang.Core.GameConfig.GridSize > 0)
        {
            targetSize = Khang.Core.GameConfig.GridSize;
        }

        bool foundConfig = false;

        // Quét danh sách cài đặt trên Inspector để tìm cấu hình tương ứng
        if (availableConfigs != null)
        {
            foreach (BoardConfig config in availableConfigs)
            {
                if (config.size == targetSize)
                {
                    boardSize = config.size;
                    winCondition = config.winCondition;
                    foundConfig = true;
                    break;
                }
            }
        }

        // Nếu chưa cài đặt trên Inspector, tự động tính toán (Dự phòng an toàn)
        if (!foundConfig)
        {
            boardSize = targetSize < 3 ? 3 : targetSize;
            winCondition = boardSize == 3 ? 3 : (boardSize == 5 ? 4 : 5);
        }

        playerTypeArray = new PlayerType[boardSize, boardSize];
        currentPlayablePlayerType = PlayerType.Cross;
        isGameOver = false;
        hostWantsRematch = false;
        clientWantsRematch = false;

        InitLineList();
    }

    private void InitLineList()
    {
        lineList = new List<Line>();

        // Generate Horizontal Lines
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x <= boardSize - winCondition; x++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), rotationAngle = 0f, lengthMultiplier = 1f / Mathf.Sqrt(2f) };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Generate Vertical Lines
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), rotationAngle = 90f, lengthMultiplier = 1f / Mathf.Sqrt(2f) };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x, y + i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Generate Diagonal A (Bottom-Left to Top-Right)
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), rotationAngle = 45f, lengthMultiplier = 1f };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y + i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Generate Diagonal B (Top-Left to Bottom-Right)
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = winCondition - 1; y < boardSize; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), rotationAngle = -45f, lengthMultiplier = 1f };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y - i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }
    }

    public bool IsNetworkActive()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    public PlayerType GetLocalPlayerType()
    {
        if (!IsNetworkActive())
        {
            return currentPlayablePlayerType;
        }

        return NetworkManager.Singleton.IsServer ? PlayerType.Cross : PlayerType.Circle;
    }

    private PlayerType GetPlayerTypeForClientId(ulong clientId)
    {
        return clientId == NetworkManager.ServerClientId ? PlayerType.Cross : PlayerType.Circle;
    }

    private void SubscribeToNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnect;
        }
    }

    private void UnsubscribeFromNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnect;
        }
    }

    private void NetworkManager_OnClientDisconnect(ulong clientId)
    {
        if (!IsNetworkActive()) return;

        if (NetworkManager.Singleton.IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                // Client disconnected
                OnMatchInterrupted?.Invoke(this, new OnMatchInterruptedEventArgs
                {
                    reason = MultiplayerConstants.MSG_OPPONENT_DISCONNECTED
                });
            }
        }
        else
        {
            // Client lost host
            OnMatchInterrupted?.Invoke(this, new OnMatchInterruptedEventArgs
            {
                reason = MultiplayerConstants.MSG_HOST_DISCONNECTED
            });
        }
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        if (playerTypeArray == null)
        {
            Init();
        }

        if (isGameOver) return;
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return;
        if (playerTypeArray[x, y] != PlayerType.None) return;

        if (!IsNetworkActive())
        {
            // OFFLINE / LOCAL HOTSEAT MODE (hoặc đấu với máy)
            if (IsAIMode() && currentPlayablePlayerType == AIPlayerType) return; // Đang là lượt của máy
            ExecuteMoveLocally(x, y, currentPlayablePlayerType);
        }
        else
        {
            // ONLINE MULTIPLAYER MODE
            PlayerType localPlayer = GetLocalPlayerType();
            if (localPlayer != currentPlayablePlayerType)
            {
                // Not your turn! Cannot move opponent's pieces.
                return;
            }

            SubmitMoveServerRpc(x, y);
        }
    }

    private void ExecuteMoveLocally(int x, int y, PlayerType playerType)
    {
        playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType,
        });

        if (TestWinner(playerType, out Line winningLine))
        {
            isGameOver = true;
            OnGameWin?.Invoke(this, new OnGameWinEventArgs
            {
                line = winningLine,
                winPlayerType = playerType,
            });
        }
        else if (TestTie())
        {
            isGameOver = true;
            OnGameTied?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            currentPlayablePlayerType = (currentPlayablePlayerType == PlayerType.Cross) ? PlayerType.Circle : PlayerType.Cross;
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);

            if (IsAIMode() && currentPlayablePlayerType == AIPlayerType)
            {
                StartCoroutine(AIMoveRoutine());
            }
        }
    }

    // ----------------------------------------------------
    // TRAINING MODE (đấu với máy) - người chơi là Cross, máy là Circle
    // ----------------------------------------------------
    private const PlayerType AIPlayerType = PlayerType.Circle;
    private const float AIThinkDelay = 0.6f;

    private bool IsAIMode()
    {
        return !IsNetworkActive() && Khang.Core.GameConfig.SelectedMode == Khang.Core.GameMode.AI;
    }

    private System.Collections.IEnumerator AIMoveRoutine()
    {
        yield return new WaitForSeconds(AIThinkDelay);

        if (isGameOver || !IsAIMode() || currentPlayablePlayerType != AIPlayerType) yield break;

        if (TryGetAIMove(out Vector2Int move))
        {
            ExecuteMoveLocally(move.x, move.y, AIPlayerType);
        }
    }

    private bool TryGetAIMove(out Vector2Int move)
    {
        PlayerType human = PlayerType.Cross;
        move = default;

        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int x = 0; x < boardSize; x++)
            for (int y = 0; y < boardSize; y++)
                if (playerTypeArray[x, y] == PlayerType.None) emptyCells.Add(new Vector2Int(x, y));

        if (emptyCells.Count == 0) return false;

        // 1. Có nước thắng ngay -> đánh
        foreach (Vector2Int cell in emptyCells)
        {
            if (WouldWin(cell, AIPlayerType)) { move = cell; return true; }
        }

        // 2. Chặn nước thắng của người chơi
        foreach (Vector2Int cell in emptyCells)
        {
            if (WouldWin(cell, human)) { move = cell; return true; }
        }

        // 3. Bàn 3x3: minimax (không thể thắng máy)
        if (boardSize == 3)
        {
            int bestScore = int.MinValue;
            foreach (Vector2Int cell in emptyCells)
            {
                playerTypeArray[cell.x, cell.y] = AIPlayerType;
                int score = Minimax(false, 1, int.MinValue, int.MaxValue);
                playerTypeArray[cell.x, cell.y] = PlayerType.None;

                if (score > bestScore) { bestScore = score; move = cell; }
            }
            return true;
        }

        // 4. Bàn lớn: chấm điểm theo các đường thắng đi qua ô đó
        float bestHeuristic = float.MinValue;
        float center = (boardSize - 1) / 2f;
        foreach (Vector2Int cell in emptyCells)
        {
            float score = ScoreCell(cell, AIPlayerType, human);
            score -= (Mathf.Abs(cell.x - center) + Mathf.Abs(cell.y - center)) * 0.1f;
            score += UnityEngine.Random.value * 0.05f; // phá thế đối xứng

            if (score > bestHeuristic) { bestHeuristic = score; move = cell; }
        }
        return true;
    }

    private bool WouldWin(Vector2Int cell, PlayerType player)
    {
        playerTypeArray[cell.x, cell.y] = player;
        bool win = TestWinner(player, out _);
        playerTypeArray[cell.x, cell.y] = PlayerType.None;
        return win;
    }

    private int Minimax(bool aiTurn, int depth, int alpha, int beta)
    {
        if (TestWinner(AIPlayerType, out _)) return 10 - depth;
        if (TestWinner(PlayerType.Cross, out _)) return depth - 10;
        if (TestTie()) return 0;

        PlayerType mover = aiTurn ? AIPlayerType : PlayerType.Cross;
        int best = aiTurn ? int.MinValue : int.MaxValue;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (playerTypeArray[x, y] != PlayerType.None) continue;

                playerTypeArray[x, y] = mover;
                int score = Minimax(!aiTurn, depth + 1, alpha, beta);
                playerTypeArray[x, y] = PlayerType.None;

                if (aiTurn)
                {
                    best = Mathf.Max(best, score);
                    alpha = Mathf.Max(alpha, best);
                }
                else
                {
                    best = Mathf.Min(best, score);
                    beta = Mathf.Min(beta, best);
                }
                if (beta <= alpha) return best;
            }
        }
        return best;
    }

    private float ScoreCell(Vector2Int cell, PlayerType self, PlayerType opponent)
    {
        float total = 0f;
        foreach (Line line in lineList)
        {
            if (!line.gridVector2IntList.Contains(cell)) continue;

            int selfCount = 0, oppCount = 0;
            foreach (Vector2Int pos in line.gridVector2IntList)
            {
                PlayerType t = playerTypeArray[pos.x, pos.y];
                if (t == self) selfCount++;
                else if (t == opponent) oppCount++;
            }

            if (oppCount == 0) total += Mathf.Pow(10f, selfCount);       // đường còn mở cho máy
            if (selfCount == 0) total += Mathf.Pow(10f, oppCount) * 0.9f; // chặn đường của người chơi
        }
        return total;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitMoveServerRpc(int x, int y, ServerRpcParams rpcParams = default)
    {
        if (isGameOver) return;
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return;
        if (playerTypeArray[x, y] != PlayerType.None) return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;
        PlayerType senderPlayerType = GetPlayerTypeForClientId(senderClientId);

        if (senderPlayerType != currentPlayablePlayerType)
        {
            // Unauthorized or wrong turn
            return;
        }

        playerTypeArray[x, y] = currentPlayablePlayerType;

        TriggerOnClickedOnGridPositionClientRpc(x, y, currentPlayablePlayerType);

        if (TestWinner(currentPlayablePlayerType, out Line winningLine))
        {
            isGameOver = true;
            int lineIndex = lineList.IndexOf(winningLine);
            TriggerOnGameWinClientRpc(lineIndex, currentPlayablePlayerType);
        }
        else if (TestTie())
        {
            isGameOver = true;
            TriggerOnGameTiedClientRpc();
        }
        else
        {
            currentPlayablePlayerType = (currentPlayablePlayerType == PlayerType.Cross) ? PlayerType.Circle : PlayerType.Cross;
            TriggerOnCurrentPlayablePlayerTypeChangedClientRpc(currentPlayablePlayerType);
        }
    }

    [ClientRpc]
    private void TriggerOnClickedOnGridPositionClientRpc(int x, int y, PlayerType playerType)
    {
        if (playerTypeArray != null)
        {
            playerTypeArray[x, y] = playerType;
        }

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType,
        });
    }

    [ClientRpc]
    private void TriggerOnGameWinClientRpc(int winningLineIndex, PlayerType winPlayerType)
    {
        isGameOver = true;
        Line winLine = default;
        if (lineList != null && winningLineIndex >= 0 && winningLineIndex < lineList.Count)
        {
            winLine = lineList[winningLineIndex];
        }

        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = winLine,
            winPlayerType = winPlayerType,
        });
    }

    [ClientRpc]
    private void TriggerOnGameTiedClientRpc()
    {
        isGameOver = true;
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void TriggerOnCurrentPlayablePlayerTypeChangedClientRpc(PlayerType nextPlayerType)
    {
        currentPlayablePlayerType = nextPlayerType;
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    // ----------------------------------------------------
    // REMATCH HANDSHAKE (Two-player agreement protocol)
    // ----------------------------------------------------
    public void Rematch()
    {
        if (!IsNetworkActive())
        {
            // Offline: Single-click instant rematch
            ResetBoardData();
            OnRematch?.Invoke(this, EventArgs.Empty);
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Online: Send handshake intent to server
            RequestRematchServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRematchServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        PlayerType senderType = GetPlayerTypeForClientId(senderClientId);

        if (senderType == PlayerType.Cross)
        {
            hostWantsRematch = true;
        }
        else if (senderType == PlayerType.Circle)
        {
            clientWantsRematch = true;
        }

        // Notify both peers that a rematch has been requested by senderType
        NotifyRematchRequestedClientRpc(senderType);

        // Check if both players have agreed to rematch
        if (hostWantsRematch && clientWantsRematch)
        {
            hostWantsRematch = false;
            clientWantsRematch = false;

            ResetBoardData();
            TriggerOnRematchClientRpc();
        }
    }

    [ClientRpc]
    private void NotifyRematchRequestedClientRpc(PlayerType requestingPlayer)
    {
        OnRematchRequested?.Invoke(this, new OnRematchRequestedEventArgs
        {
            requestingPlayer = requestingPlayer
        });
    }

    [ClientRpc]
    private void TriggerOnRematchClientRpc()
    {
        ResetBoardData();
        OnRematch?.Invoke(this, EventArgs.Empty);
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ResetBoardData()
    {
        if (playerTypeArray == null)
        {
            Init();
            return;
        }

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }

        currentPlayablePlayerType = PlayerType.Cross;
        isGameOver = false;
        hostWantsRematch = false;
        clientWantsRematch = false;
    }

    private bool TestWinner(PlayerType playerType, out Line winningLine)
    {
        foreach (Line line in lineList)
        {
            if (TestWinnerLine(line, playerType))
            {
                winningLine = line;
                return true;
            }
        }

        winningLine = default;
        return false;
    }

    private bool TestWinnerLine(Line line, PlayerType playerType)
    {
        foreach (Vector2Int pos in line.gridVector2IntList)
        {
            if (playerTypeArray[pos.x, pos.y] != playerType) return false;
        }
        return true;
    }

    private bool TestTie()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType;
    }

    public PlayerType[,] GetPlayerTypeArray()
    {
        return playerTypeArray;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
