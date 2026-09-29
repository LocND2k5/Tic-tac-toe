using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, ITurnProvider, IGameStateProvider, IBoardVisualProvider, IGridInteractionProvider
{
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler OnGameTied;
    public event EventHandler OnRematch;

    [System.Serializable]
    public struct BoardConfig
    {
        public string configName; 
        public int size;          
        public int winCondition;  
    }

    [Header("Cài đặt các loại Bàn cờ (Board Setups)")]
    public BoardConfig[] availableConfigs;

    [HideInInspector] public int boardSize = 3;
    [HideInInspector] public int winCondition = 3;

    public int BoardSize => boardSize;
    public int WinCondition => winCondition;

    private PlayerType[,] playerTypeArray;
    private PlayerType currentPlayablePlayerType;
    private List<WinLine> lineList;
    private bool isGameOver;

    private void Awake()
    {
        // XÓA BỎ SINGLETON (Instance)
        // Áp dụng Đảo ngược Phụ thuộc (DIP) và Phân tách Giao diện (ISP)
        GameLocator.Register(this);
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        int targetSize = MainMenuUI.SelectedBoardSize;
        bool foundConfig = false;

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

        if (!foundConfig)
        {
            boardSize = targetSize < 3 ? 3 : targetSize;
            winCondition = boardSize == 3 ? 3 : (boardSize == 5 ? 4 : 5);
        }

        playerTypeArray = new PlayerType[boardSize, boardSize];
        currentPlayablePlayerType = PlayerType.Cross;
        isGameOver = false;

        InitLineList();
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        if (isGameOver) return;
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return;
        if (playerTypeArray[x, y] != PlayerType.None) return;

        playerTypeArray[x, y] = currentPlayablePlayerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = currentPlayablePlayerType,
        });

        if (TestWinner(out WinLine winningLine))
        {
            isGameOver = true;
            OnGameWin?.Invoke(this, new OnGameWinEventArgs
            {
                line = winningLine,
                winPlayerType = currentPlayablePlayerType,
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
        }
    }

    public void Rematch()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }

        currentPlayablePlayerType = PlayerType.Cross;
        isGameOver = false;

        OnRematch?.Invoke(this, EventArgs.Empty);
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType;
    }

    private bool TestWinner(out WinLine winningLine)
    {
        foreach (WinLine line in lineList)
        {
            if (TestWinnerLine(line))
            {
                winningLine = line;
                return true;
            }
        }

        winningLine = default;
        return false;
    }

    private bool TestWinnerLine(WinLine line)
    {
        foreach (Vector2Int pos in line.gridVector2IntList)
        {
            if (playerTypeArray[pos.x, pos.y] != currentPlayablePlayerType) return false;
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

    private void InitLineList()
    {
        lineList = new List<WinLine>();

        // Horizontal
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x <= boardSize - winCondition; x++)
            {
                WinLine line = new WinLine { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.Horizontal };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Vertical
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                WinLine line = new WinLine { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.Vertical };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x, y + i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Diagonal A
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                WinLine line = new WinLine { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.DiagonalA };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y + i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }

        // Diagonal B
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = winCondition - 1; y < boardSize; y++)
            {
                WinLine line = new WinLine { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.DiagonalB };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y - i));
                Vector2Int first = line.gridVector2IntList[0];
                Vector2Int last = line.gridVector2IntList[winCondition - 1];
                line.centerGridPosition = new Vector2((first.x + last.x) / 2f, (first.y + last.y) / 2f);
                lineList.Add(line);
            }
        }
    }
}
