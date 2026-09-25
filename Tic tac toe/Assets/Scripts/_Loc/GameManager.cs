using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
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

    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler OnGameTied;
    public event EventHandler OnRematch;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    public int boardSize = 3;
    public int winCondition = 3;

    private PlayerType[,] playerTypeArray;
    private PlayerType currentPlayablePlayerType;
    private List<Line> lineList;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance!");
        }
        Instance = this;

        // Try to read from MainMenuUI if set
        if (MainMenuUI.SelectedBoardSize >= 3)
        {
            boardSize = MainMenuUI.SelectedBoardSize;
        }

        Init();
    }

    public void Init()
    {
        if (boardSize < 3) boardSize = 3;
        winCondition = boardSize == 3 ? 3 : (boardSize == 5 ? 4 : 5); // Example: 3x3=3, 5x5=4, 7x7=5

        playerTypeArray = new PlayerType[boardSize, boardSize];
        currentPlayablePlayerType = PlayerType.Cross;
        isGameOver = false;

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
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.Horizontal };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y));
                line.centerGridPosition = line.gridVector2IntList[winCondition / 2];
                lineList.Add(line);
            }
        }

        // Generate Vertical Lines
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.Vertical };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x, y + i));
                line.centerGridPosition = line.gridVector2IntList[winCondition / 2];
                lineList.Add(line);
            }
        }

        // Generate Diagonal A (Bottom-Left to Top-Right)
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = 0; y <= boardSize - winCondition; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.DiagonalA };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y + i));
                line.centerGridPosition = line.gridVector2IntList[winCondition / 2];
                lineList.Add(line);
            }
        }

        // Generate Diagonal B (Top-Left to Bottom-Right)
        for (int x = 0; x <= boardSize - winCondition; x++)
        {
            for (int y = winCondition - 1; y < boardSize; y++)
            {
                Line line = new Line { gridVector2IntList = new List<Vector2Int>(), orientation = Orientation.DiagonalB };
                for (int i = 0; i < winCondition; i++) line.gridVector2IntList.Add(new Vector2Int(x + i, y - i));
                line.centerGridPosition = line.gridVector2IntList[winCondition / 2];
                lineList.Add(line);
            }
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

        playerTypeArray[x, y] = currentPlayablePlayerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = currentPlayablePlayerType,
        });

        if (TestWinner(currentPlayablePlayerType, out Line winningLine))
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

        OnRematch?.Invoke(this, EventArgs.Empty);
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
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
