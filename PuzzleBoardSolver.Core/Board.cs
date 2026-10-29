using System.Collections.ObjectModel;

namespace PuzzleBoardSolver;

/// <summary>Immutable row-major snapshot of a rectangular board.</summary>
public sealed class Board<T>
{
    private readonly T[] cells;

    /// <summary>Creates a board snapshot from row-major cells.</summary>
    public Board(int rows, int columns, IEnumerable<T> cells, BoardRules<T> rules)
    {
        if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
        if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(rules);

        var snapshot = cells.ToArray();
        if (snapshot.Length != checked(rows * columns))
        {
            throw new ArgumentException("The cell count must equal rows multiplied by columns.", nameof(cells));
        }

        for (var index = 0; index < snapshot.Length; index++)
        {
            if (snapshot[index] is null)
            {
                throw new ArgumentException("Cells cannot contain null values.", nameof(cells));
            }
        }

        Rows = rows;
        Columns = columns;
        Rules = rules;
        this.cells = snapshot;
        Cells = new ReadOnlyCollection<T>(this.cells);
    }

    /// <summary>Number of rows.</summary>
    public int Rows { get; }

    /// <summary>Number of columns.</summary>
    public int Columns { get; }

    /// <summary>Total number of cells.</summary>
    public int CellCount => cells.Length;

    /// <summary>Rules associated with this snapshot.</summary>
    public BoardRules<T> Rules { get; }

    /// <summary>Read-only row-major cell view.</summary>
    public IReadOnlyList<T> Cells { get; }

    /// <summary>Gets a cell with bounds-safe indexing.</summary>
    public T this[int row, int column]
    {
        get
        {
            if ((uint)row >= (uint)Rows) throw new ArgumentOutOfRangeException(nameof(row));
            if ((uint)column >= (uint)Columns) throw new ArgumentOutOfRangeException(nameof(column));
            return cells[(row * Columns) + column];
        }
    }

    /// <summary>Creates a snapshot from a rectangular array.</summary>
    public static Board<T> FromArray(T[,] source, BoardRules<T> rules)
    {
        ArgumentNullException.ThrowIfNull(source);
        var values = new T[source.GetLength(0) * source.GetLength(1)];
        var index = 0;
        for (var row = 0; row < source.GetLength(0); row++)
        {
            for (var column = 0; column < source.GetLength(1); column++)
            {
                values[index++] = source[row, column];
            }
        }

        return new Board<T>(source.GetLength(0), source.GetLength(1), values, rules);
    }
}
