using System.Collections.ObjectModel;

namespace PuzzleBoardSolver;

/// <summary>Immutable configuration for board analysis.</summary>
public sealed class BoardRules<T>
{
    private static readonly WinCondition[] AllConditions = Enum.GetValues<WinCondition>();

    /// <summary>Creates rules with all supported conditions enabled.</summary>
    public BoardRules(T emptyValue, int winLength = AnalysisDefaults.LineWinLength, IEnumerable<WinCondition>? enabledConditions = null, IEqualityComparer<T>? comparer = null)
    {
        if (emptyValue is null)
        {
            throw new ArgumentNullException(nameof(emptyValue));
        }

        if (winLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(winLength), "Win length must be positive.");
        }

        var conditions = (enabledConditions ?? AllConditions).ToArray();
        if (conditions.Length == 0)
        {
            throw new ArgumentException("At least one winning condition is required.", nameof(enabledConditions));
        }

        if (conditions.Any(condition => !Enum.IsDefined(condition)))
        {
            throw new ArgumentException("Every winning condition must be defined.", nameof(enabledConditions));
        }

        EmptyValue = emptyValue;
        WinLength = winLength;
        Comparer = comparer ?? EqualityComparer<T>.Default;
        EnabledConditions = new ReadOnlyCollection<WinCondition>(conditions.Distinct().OrderBy(condition => condition).ToArray());
    }

    /// <summary>Empty cell value.</summary>
    public T EmptyValue { get; }

    /// <summary>Line length required by horizontal, vertical, and diagonal conditions.</summary>
    public int WinLength { get; }

    /// <summary>Enabled winning conditions in deterministic order.</summary>
    public IReadOnlyList<WinCondition> EnabledConditions { get; }

    /// <summary>Equality comparer used for cell values.</summary>
    public IEqualityComparer<T> Comparer { get; }

    /// <summary>Creates default character rules for the assessment API.</summary>
    public static BoardRules<char> DefaultCharacter { get; } = new(AnalysisDefaults.CharacterEmptyValue, AnalysisDefaults.LineWinLength);
}
