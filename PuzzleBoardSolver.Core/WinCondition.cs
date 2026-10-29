namespace PuzzleBoardSolver;

/// <summary>Winning patterns supported by the analyzer.</summary>
public enum WinCondition
{
    /// <summary>Contiguous matching cells in a row.</summary>
    Horizontal,
    /// <summary>Contiguous matching cells in a column.</summary>
    Vertical,
    /// <summary>Contiguous matching cells on either diagonal.</summary>
    Diagonal,
    /// <summary>Matching values in all four outer corners.</summary>
    AllFourCorners,
    /// <summary>Matching values in a contiguous 2x2 block.</summary>
    Box2x2
}
