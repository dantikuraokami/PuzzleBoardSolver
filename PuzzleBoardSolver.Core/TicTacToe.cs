namespace PuzzleBoardSolver;

/// <summary>Authoritative analyzer for configurable Tic-Tac-Toe-style board patterns.</summary>
public sealed class TicTacToe
{
    /// <summary>Analyzes an immutable generic board snapshot.</summary>
    public GameAnalysis<T> Analyze<T>(Board<T> board)
    {
        ArgumentNullException.ThrowIfNull(board);
        var rules = board.Rules;
        var winners = new List<T>();
        var conditions = new HashSet<WinCondition>();
        var anyMovesLeft = HasEmptyCell(board);

        foreach (var condition in rules.EnabledConditions)
        {
            EvaluateCondition(board, rules, winners, conditions, condition);
        }

        return new GameAnalysis<T>(anyMovesLeft, DetermineStatus(winners.Count, anyMovesLeft), winners, conditions.OrderBy(condition => condition));
    }

    /// <summary>Analyzes a rectangular character array using the supplied empty value.</summary>
    public GameAnalysis<char> Analyze(char[,] cells, char emptyValue = AnalysisDefaults.CharacterEmptyValue)
    {
        ArgumentNullException.ThrowIfNull(cells);
        return Analyze(Board<char>.FromArray(cells, new BoardRules<char>(emptyValue)));
    }

    /// <summary>Returns the single winner, or the default value for no winner or multiple winners.</summary>
    /// <remarks>For value types, use <see cref="TryCheckWinner{T}"/> when default-value ambiguity matters.</remarks>
    public T? CheckWinner<T>(Board<T> board)
    {
        return TryCheckWinner(board, out var winner) ? winner : default;
    }

    /// <summary>Attempts to return the single winner without default-value ambiguity.</summary>
    /// <typeparam name="T">The board cell type.</typeparam>
    /// <param name="board">The board snapshot to analyze.</param>
    /// <param name="winner">The single winner when the method returns <see langword="true"/>; otherwise the default value.</param>
    /// <returns><see langword="true"/> only when exactly one distinct winner exists.</returns>
    public bool TryCheckWinner<T>(Board<T> board, out T winner)
    {
        var analysis = Analyze(board);
        if (analysis.Winners.Count == 1)
        {
            winner = analysis.Winners[0];
            return true;
        }

        winner = default!;
        return false;
    }

    /// <summary>Returns the character winner, or null for no winner or multiple winners.</summary>
    public char? CheckWinner(char[,] cells, char emptyValue = AnalysisDefaults.CharacterEmptyValue)
    {
        return TryCheckWinner(cells, out var winner, emptyValue) ? winner : null;
    }

    /// <summary>Attempts to return the character winner without using a sentinel value.</summary>
    /// <param name="cells">The rectangular board to analyze.</param>
    /// <param name="winner">The single winner when the method returns <see langword="true"/>; otherwise the default character.</param>
    /// <param name="emptyValue">The value representing an empty cell.</param>
    /// <returns><see langword="true"/> only when exactly one distinct winner exists.</returns>
    public bool TryCheckWinner(char[,] cells, out char winner, char emptyValue = AnalysisDefaults.CharacterEmptyValue)
    {
        var analysis = Analyze(cells, emptyValue);
        if (analysis.Winners.Count == 1)
        {
            winner = analysis.Winners[0];
            return true;
        }

        winner = default;
        return false;
    }

    /// <summary>Returns whether the board contains at least one empty cell.</summary>
    public bool AnyMovesLeft<T>(Board<T> board) => Analyze(board).AnyMovesLeft;

    /// <summary>Returns whether the rectangular character board contains an empty cell.</summary>
    public bool AnyMovesLeft(char[,] cells, char emptyValue = AnalysisDefaults.CharacterEmptyValue) => Analyze(cells, emptyValue).AnyMovesLeft;

    /// <summary>Returns whether the board is won, drawn, or invalid.</summary>
    public bool IsGameOver<T>(Board<T> board) => Analyze(board).IsGameOver;

    /// <summary>Returns whether the rectangular character board is over.</summary>
    public bool IsGameOver(char[,] cells, char emptyValue = AnalysisDefaults.CharacterEmptyValue) => Analyze(cells, emptyValue).IsGameOver;

    private static bool HasEmptyCell<T>(Board<T> board)
    {
        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                if (board.Rules.Comparer.Equals(board[row, column], board.Rules.EmptyValue))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static BoardStatus DetermineStatus(int winnerCount, bool anyMovesLeft) => winnerCount switch
    {
        0 when anyMovesLeft => BoardStatus.InProgress,
        0 => BoardStatus.Draw,
        1 => BoardStatus.Win,
        _ => BoardStatus.Invalid
    };

    private static void EvaluateCondition<T>(Board<T> board, BoardRules<T> rules, List<T> winners, HashSet<WinCondition> conditions, WinCondition condition)
    {
        switch (condition)
        {
            case WinCondition.Horizontal:
                EvaluateLines(board, rules, winners, conditions, 0, 1, condition);
                break;
            case WinCondition.Vertical:
                EvaluateLines(board, rules, winners, conditions, 1, 0, condition);
                break;
            case WinCondition.Diagonal:
                EvaluateLines(board, rules, winners, conditions, 1, 1, condition);
                EvaluateLines(board, rules, winners, conditions, 1, -1, condition);
                break;
            case WinCondition.AllFourCorners:
                EvaluateCorners(board, rules, winners, conditions, condition);
                break;
            case WinCondition.Box2x2:
                EvaluateBoxes(board, rules, winners, conditions, condition);
                break;
        }
    }

    private static void EvaluateLines<T>(Board<T> board, BoardRules<T> rules, List<T> winners, HashSet<WinCondition> conditions, int rowStep, int columnStep, WinCondition condition)
    {
        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                var endRow = row + ((rules.WinLength - 1) * rowStep);
                var endColumn = column + ((rules.WinLength - 1) * columnStep);
                if ((uint)endRow >= (uint)board.Rows || (uint)endColumn >= (uint)board.Columns)
                {
                    continue;
                }

                var value = board[row, column];
                if (rules.Comparer.Equals(value, rules.EmptyValue))
                {
                    continue;
                }

                var matches = true;
                for (var offset = 1; offset < rules.WinLength; offset++)
                {
                    if (!rules.Comparer.Equals(value, board[row + (offset * rowStep), column + (offset * columnStep)]))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    AddWinner(value, winners, rules.Comparer, conditions, condition);
                }
            }
        }
    }

    private static void EvaluateCorners<T>(Board<T> board, BoardRules<T> rules, List<T> winners, HashSet<WinCondition> conditions, WinCondition condition)
    {
        if (board.Rows < 2 || board.Columns < 2)
        {
            return;
        }

        var value = board[0, 0];
        if (rules.Comparer.Equals(value, rules.EmptyValue) ||
            !rules.Comparer.Equals(value, board[0, board.Columns - 1]) ||
            !rules.Comparer.Equals(value, board[board.Rows - 1, 0]) ||
            !rules.Comparer.Equals(value, board[board.Rows - 1, board.Columns - 1]))
        {
            return;
        }

        AddWinner(value, winners, rules.Comparer, conditions, condition);
    }

    private static void EvaluateBoxes<T>(Board<T> board, BoardRules<T> rules, List<T> winners, HashSet<WinCondition> conditions, WinCondition condition)
    {
        for (var row = 0; row <= board.Rows - AnalysisDefaults.BoxDimension; row++)
        {
            for (var column = 0; column <= board.Columns - AnalysisDefaults.BoxDimension; column++)
            {
                var value = board[row, column];
                if (rules.Comparer.Equals(value, rules.EmptyValue) ||
                    !rules.Comparer.Equals(value, board[row + AnalysisDefaults.BoxDimension - 1, column]) ||
                    !rules.Comparer.Equals(value, board[row, column + AnalysisDefaults.BoxDimension - 1]) ||
                    !rules.Comparer.Equals(value, board[row + AnalysisDefaults.BoxDimension - 1, column + AnalysisDefaults.BoxDimension - 1]))
                {
                    continue;
                }

                AddWinner(value, winners, rules.Comparer, conditions, condition);
            }
        }
    }

    private static void AddWinner<T>(T value, List<T> winners, IEqualityComparer<T> comparer, HashSet<WinCondition> conditions, WinCondition condition)
    {
        if (!winners.Any(existing => comparer.Equals(existing, value)))
        {
            winners.Add(value);
        }

        conditions.Add(condition);
    }
}
