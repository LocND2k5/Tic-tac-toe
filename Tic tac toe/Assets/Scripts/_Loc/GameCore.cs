using System;
using System.Collections.Generic;
using UnityEngine;

// --- DỮ LIỆU DÙNG CHUNG ---
public enum PlayerType { None, Cross, Circle }
public enum Orientation { Horizontal, Vertical, DiagonalA, DiagonalB }

public struct WinLine
{
    public List<Vector2Int> gridVector2IntList;
    public Vector2 centerGridPosition;
    public Orientation orientation;
}

public class OnClickedOnGridPositionEventArgs : EventArgs
{
    public int x;
    public int y;
    public PlayerType playerType;
}

public class OnGameWinEventArgs : EventArgs
{
    public WinLine line;
    public PlayerType winPlayerType;
}

// --- CÁC INTERFACES ĐÃ ĐƯỢC PHÂN TÁCH (ISP) ---

// 1. Dành cho UI hiển thị lượt đi (GameTurnUI)
public interface ITurnProvider
{
    PlayerType GetCurrentPlayablePlayerType();
    event EventHandler OnCurrentPlayablePlayerTypeChanged;
    event EventHandler OnRematch;
}

// 2. Dành cho UI kết thúc game (RematchUI)
public interface IGameStateProvider
{
    event EventHandler<OnGameWinEventArgs> OnGameWin;
    event EventHandler OnGameTied;
    event EventHandler OnRematch;
    void Rematch();
}

// 3. Dành cho Sinh lưới và Vẽ hình (BoardGenerator & GameVisualManager)
public interface IBoardVisualProvider
{
    int BoardSize { get; }
    int WinCondition { get; }
    event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    event EventHandler<OnGameWinEventArgs> OnGameWin;
    event EventHandler OnRematch;
}

// 4. Dành cho tương tác người chơi (GridPosition)
public interface IGridInteractionProvider
{
    void ClickedOnGridPosition(int x, int y);
}

// --- TRẠM PHÂN PHỐI (SERVICE LOCATOR) ĐỂ GIẢI QUYẾT DIP ---
public static class GameLocator
{
    private static ITurnProvider turnProvider;
    private static IGameStateProvider gameStateProvider;
    private static IBoardVisualProvider boardVisualProvider;
    private static IGridInteractionProvider gridInteractionProvider;

    // GameManager sẽ gọi hàm này để tự nộp mình vào hệ thống
    public static void Register(object service)
    {
        if (service is ITurnProvider tp) turnProvider = tp;
        if (service is IGameStateProvider gsp) gameStateProvider = gsp;
        if (service is IBoardVisualProvider bvp) boardVisualProvider = bvp;
        if (service is IGridInteractionProvider gip) gridInteractionProvider = gip;
    }

    // Các Script UI sẽ gọi các hàm này để lấy đúng cái "remote" chúng cần
    public static ITurnProvider GetTurnProvider() => turnProvider;
    public static IGameStateProvider GetGameStateProvider() => gameStateProvider;
    public static IBoardVisualProvider GetBoardVisualProvider() => boardVisualProvider;
    public static IGridInteractionProvider GetGridInteractionProvider() => gridInteractionProvider;
}
