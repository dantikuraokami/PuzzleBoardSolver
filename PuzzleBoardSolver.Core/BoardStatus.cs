namespace PuzzleBoardSolver;

/// <summary>Outcome of analyzing a board's patterns and empty cells.</summary>
public enum BoardStatus
{
    /// <summary>No winner and at least one empty cell.</summary>
    InProgress,
    /// <summary>Exactly one distinct winner.</summary>
    Win,
    /// <summary>No winner and no empty cells.</summary>
    Draw,
    /// <summary>Multiple distinct winners.</summary>
    Invalid
}
