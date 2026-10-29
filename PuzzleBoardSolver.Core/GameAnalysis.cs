using System.Collections.ObjectModel;

namespace PuzzleBoardSolver;

/// <summary>Immutable result of one board analysis.</summary>
public sealed class GameAnalysis<T>
{
    internal GameAnalysis(bool anyMovesLeft, BoardStatus status, IEnumerable<T> winners, IEnumerable<WinCondition> winningConditions)
    {
        AnyMovesLeft = anyMovesLeft;
        Status = status;
        IsValid = status != BoardStatus.Invalid;
        IsGameOver = status != BoardStatus.InProgress;
        Winners = new ReadOnlyCollection<T>(winners.ToArray());
        WinningConditions = new ReadOnlyCollection<WinCondition>(winningConditions.ToArray());
    }

    /// <summary>Whether at least one cell contains the configured empty value.</summary>
    public bool AnyMovesLeft { get; }

    /// <summary>Computed board status.</summary>
    public BoardStatus Status { get; }

    /// <summary>False only when multiple distinct winners were detected.</summary>
    public bool IsValid { get; }

    /// <summary>Whether the board is won, drawn, or invalid.</summary>
    public bool IsGameOver { get; }

    /// <summary>Distinct winning values in deterministic encounter order.</summary>
    public IReadOnlyList<T> Winners { get; }

    /// <summary>Conditions that detected one or more winning patterns.</summary>
    public IReadOnlyList<WinCondition> WinningConditions { get; }
}
