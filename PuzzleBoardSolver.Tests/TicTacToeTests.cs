using PuzzleBoardSolver;

namespace PuzzleBoardSolver.Tests;

public sealed class TicTacToeTests
{
    private readonly TicTacToe analyzer = new();

    [Fact]
    public void DetectsEachLineDirection()
    {
        AssertCondition(new[,] { { 'X', 'X', 'X', 'X' }, { '.', 'O', '.', '.' }, { '.', '.', 'O', '.' }, { '.', '.', '.', '.' } }, WinCondition.Horizontal);
        AssertCondition(new[,] { { 'X', '.', '.', '.' }, { 'X', 'O', '.', '.' }, { 'X', '.', 'O', '.' }, { 'X', '.', '.', '.' } }, WinCondition.Vertical);
        AssertCondition(new[,] { { 'X', '.', '.', '.' }, { '.', 'X', '.', '.' }, { '.', '.', 'X', '.' }, { '.', '.', '.', 'X' } }, WinCondition.Diagonal);
        AssertCondition(new[,] { { '.', '.', '.', 'X' }, { '.', '.', 'X', '.' }, { '.', 'X', '.', '.' }, { 'X', '.', '.', '.' } }, WinCondition.Diagonal);
    }

    [Fact]
    public void DetectsCornersAndEvery2x2Position()
    {
        var corners = new[,] { { 'X', '.', '.', 'X' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { 'X', '.', '.', 'X' } };
        var cornerRules = new BoardRules<char>('.', 4, [WinCondition.AllFourCorners]);
        Assert.Equal(BoardStatus.Win, analyzer.Analyze(Board<char>.FromArray(corners, cornerRules)).Status);

        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                var cells = Enumerable.Repeat('.', 16).ToArray();
                cells[(row * 4) + column] = 'Z';
                cells[((row + 1) * 4) + column] = 'Z';
                cells[(row * 4) + column + 1] = 'Z';
                cells[((row + 1) * 4) + column + 1] = 'Z';
                var board = new Board<char>(4, 4, cells, new BoardRules<char>('.', 4, [WinCondition.Box2x2]));
                Assert.Equal('Z', analyzer.CheckWinner(board));
            }
        }
    }

    [Fact]
    public void EmptyPatternsDoNotWinAndPartialWinIsGameOver()
    {
        var empty = new char[4, 4];
        for (var row = 0; row < 4; row++) for (var column = 0; column < 4; column++) empty[row, column] = '.';
        var emptyResult = analyzer.Analyze(empty);
        Assert.Equal(BoardStatus.InProgress, emptyResult.Status);
        Assert.Empty(emptyResult.Winners);

        var partial = new[,] { { 'X', 'X', 'X', 'X' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } };
        var partialResult = analyzer.Analyze(partial);
        Assert.Equal(BoardStatus.Win, partialResult.Status);
        Assert.True(partialResult.AnyMovesLeft);
        Assert.True(partialResult.IsGameOver);
    }

    [Fact]
    public void DrawAndMultipleWinnersHaveCorrectSemantics()
    {
        var draw = new[,] { { 'X', 'X', 'O', 'O' }, { 'O', 'O', 'X', 'X' }, { 'X', 'O', 'X', 'O' }, { 'O', 'X', 'O', 'X' } };
        var drawResult = analyzer.Analyze(draw);
        Assert.Equal(BoardStatus.Draw, drawResult.Status);
        Assert.False(drawResult.AnyMovesLeft);
        Assert.True(drawResult.IsGameOver);

        var multiple = new[,] { { 'X', 'X', 'X', 'X' }, { 'O', 'O', 'O', 'O' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } };
        var multipleResult = analyzer.Analyze(multiple);
        Assert.Equal(BoardStatus.Invalid, multipleResult.Status);
        Assert.False(multipleResult.IsValid);
        Assert.Null(analyzer.CheckWinner(multiple));
        Assert.True(analyzer.IsGameOver(multiple));
        Assert.Equal(['X', 'O'], multipleResult.Winners);
    }

    [Fact]
    public void MultiplePatternsForOneWinnerAreOneWinner()
    {
        var cells = new[,] { { 'X', 'X', 'X', 'X' }, { 'X', '.', '.', 'X' }, { 'X', '.', '.', 'X' }, { 'X', 'X', 'X', 'X' } };
        var result = analyzer.Analyze(Board<char>.FromArray(cells, new BoardRules<char>('.', 4, [WinCondition.Horizontal])));
        Assert.Equal(BoardStatus.Win, result.Status);
        Assert.Single(result.Winners);
        Assert.Single(result.WinningConditions);
        Assert.Equal('X', result.Winners[0]);
    }

    [Fact]
    public void SupportsRectangularCustomLengthAndNonCharacterCells()
    {
        var rules = new BoardRules<int>(0, 3, [WinCondition.Horizontal, WinCondition.Vertical, WinCondition.Diagonal]);
        var board = new Board<int>(3, 5, [7, 7, 7, 0, 0, 0, 1, 0, 0, 0, 0, 0, 2, 0, 0], rules);
        var result = analyzer.Analyze(board);
        Assert.Equal(BoardStatus.Win, result.Status);
        Assert.Equal(7, analyzer.CheckWinner(board));
        Assert.True(analyzer.TryCheckWinner(board, out var integerWinner));
        Assert.Equal(7, integerWinner);

        var noCorners = new Board<int>(1, 5, [1, 0, 1, 1, 1], new BoardRules<int>(0, 3, [WinCondition.AllFourCorners]));
        Assert.Equal(BoardStatus.InProgress, analyzer.Analyze(noCorners).Status);
    }

    [Fact]
    public void ValidatesInputsAndRules()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Board<char>(0, 4, [], new BoardRules<char>('.')));
        Assert.Throws<ArgumentException>(() => new Board<char>(2, 2, ['.', '.', '.'], new BoardRules<char>('.')));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoardRules<char>('.', 0));
        Assert.Throws<ArgumentException>(() => new BoardRules<char>('.', 4, []));
        Assert.Throws<ArgumentException>(() => new BoardRules<char>('.', 4, [(WinCondition)99]));
        Assert.Throws<ArgumentNullException>(() => analyzer.Analyze((Board<char>)null!));
        Assert.Throws<ArgumentException>(() => new Board<string>(1, 1, new string?[] { null }!, new BoardRules<string>(string.Empty)));
    }

    [Fact]
    public void RejectsNullInputsAcrossAnalysisAndWinnerApis()
    {
        Board<char> nullBoard = null!;
        char[,] nullCells = null!;

        Assert.Throws<ArgumentNullException>(() => analyzer.Analyze(nullBoard));
        Assert.Throws<ArgumentNullException>(() => analyzer.Analyze(nullCells));
        Assert.Throws<ArgumentNullException>(() => analyzer.CheckWinner(nullBoard));
        Assert.Throws<ArgumentNullException>(() => analyzer.CheckWinner(nullCells));
        Assert.Throws<ArgumentNullException>(() => analyzer.AnyMovesLeft(nullBoard));
        Assert.Throws<ArgumentNullException>(() => analyzer.AnyMovesLeft(nullCells));
        Assert.Throws<ArgumentNullException>(() => analyzer.IsGameOver(nullBoard));
        Assert.Throws<ArgumentNullException>(() => analyzer.IsGameOver(nullCells));
    }

    [Fact]
    public void RejectsNullBoardDependenciesAndReferenceCells()
    {
        BoardRules<string> nullRules = null!;
        IEnumerable<string> nullCells = null!;
        string[,] nullArray = null!;
        var arrayWithNullCell = new string[1, 1];

        Assert.Throws<ArgumentNullException>(() => new Board<string>(1, 1, ["X"], nullRules));
        Assert.Throws<ArgumentNullException>(() => new Board<string>(1, 1, nullCells, new BoardRules<string>(string.Empty)));
        Assert.Throws<ArgumentNullException>(() => Board<string>.FromArray(nullArray, new BoardRules<string>(string.Empty)));
        Assert.Throws<ArgumentNullException>(() => Board<string>.FromArray(new[,] { { "X" } }, nullRules));
        Assert.Throws<ArgumentNullException>(() => new BoardRules<string>(null!));
        Assert.Throws<ArgumentException>(() => Board<string>.FromArray(arrayWithNullCell, new BoardRules<string>(string.Empty)));
    }

    [Fact]
    public void AllowsNullableValueTypeOnlyWhenTheCellIsNonNull()
    {
        var rules = new BoardRules<int?>(-1, 1, [WinCondition.Horizontal]);
        var validBoard = new Board<int?>(1, 1, [0], rules);

        Assert.Equal(0, analyzer.CheckWinner(validBoard));
        Assert.Throws<ArgumentException>(() => new Board<int?>(1, 1, [null], rules));
    }

    [Fact]
    public void BoardSnapshotAndAnalysisDoNotMutateInput()
    {
        var input = new[] { 'X', '.', '.', '.' };
        var board = new Board<char>(2, 2, input, new BoardRules<char>('.'));
        input[0] = 'O';
        Assert.Equal('X', board[0, 0]);

        var array = new[,] { { 'X', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } };
        var before = (char[,])array.Clone();
        _ = analyzer.Analyze(array);
        Assert.Equal(before, array);
    }

    [Fact]
    public async Task IndependentAnalysesAreThreadSafe()
    {
        var boards = Enumerable.Range(0, 32).Select(_ => new[,] { { 'X', 'X', 'X', 'X' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } });
        var results = await Task.WhenAll(boards.Select(board => Task.Run(() => analyzer.Analyze(board))));
        Assert.All(results, result => Assert.Equal(BoardStatus.Win, result.Status));
    }

    [Fact]
    public void ConvenienceMethodsMatchCentralAnalysis()
    {
        var cells = new[,] { { 'A', 'A', 'A', '.', }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } };
        var rules = new BoardRules<char>('.', 3, [WinCondition.Horizontal]);
        var board = Board<char>.FromArray(cells, rules);
        Assert.Equal(analyzer.Analyze(board).AnyMovesLeft, analyzer.AnyMovesLeft(new[,] { { 'A', 'A', 'A', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' }, { '.', '.', '.', '.' } }));
        Assert.Equal('A', analyzer.CheckWinner(board));
    }

    [Fact]
    public void SupportsCustomEmptyValuesAndComparers()
    {
        var rules = new BoardRules<string>("-", 3, [WinCondition.Horizontal], StringComparer.OrdinalIgnoreCase);
        var board = new Board<string>(1, 3, ["a", "A", "a"], rules);

        var result = analyzer.Analyze(board);

        Assert.Equal(BoardStatus.Win, result.Status);
        Assert.Equal("a", Assert.Single(result.Winners));
        Assert.False(result.AnyMovesLeft);
    }

    [Fact]
    public void SupportsLineLengthOneAndDefaultValuedWinner()
    {
        var rules = new BoardRules<char>('.', 1, [WinCondition.Horizontal]);
        var board = new Board<char>(1, 1, ['\0'], rules);

        var result = analyzer.Analyze(board);

        Assert.Equal(BoardStatus.Win, result.Status);
        Assert.Equal('\0', Assert.Single(result.Winners));
        Assert.Equal('\0', analyzer.CheckWinner(board));
    }

    [Fact]
    public void TryCheckWinnerDisambiguatesValueTypeDefaultAndNoWinner()
    {
        var winningRules = new BoardRules<int>(-1, 3, [WinCondition.Horizontal]);
        var zeroWinner = new Board<int>(1, 3, [0, 0, 0], winningRules);
        var noWinner = new Board<int>(1, 3, [1, 2, 1], winningRules);
        var multipleWinners = new Board<int>(2, 3, [1, 1, 1, 2, 2, 2], new BoardRules<int>(0, 3, [WinCondition.Horizontal]));

        Assert.True(analyzer.TryCheckWinner(zeroWinner, out var winner));
        Assert.Equal(0, winner);
        Assert.False(analyzer.TryCheckWinner(noWinner, out var noWinnerValue));
        Assert.Equal(0, noWinnerValue);
        Assert.False(analyzer.TryCheckWinner(multipleWinners, out _));
    }

    [Fact]
    public void FixedPatternsDoNotWinOnSmallestBoard()
    {
        var cells = new[,] { { 'X' } };
        var cornerRules = new BoardRules<char>('.', 1, [WinCondition.AllFourCorners]);
        var boxRules = new BoardRules<char>('.', 1, [WinCondition.Box2x2]);

        Assert.Equal(BoardStatus.Draw, analyzer.Analyze(Board<char>.FromArray(cells, cornerRules)).Status);
        Assert.Equal(BoardStatus.Draw, analyzer.Analyze(Board<char>.FromArray(cells, boxRules)).Status);
    }

    [Fact]
    public void RulesAndIndexingAreSafeToCallThroughReadOnlyApis()
    {
        var rules = new BoardRules<char>('.', 4, [WinCondition.Horizontal, WinCondition.Vertical]);
        var board = new Board<char>(2, 2, ['.', '.', '.', '.'], rules);

        Assert.Throws<NotSupportedException>(() => ((IList<WinCondition>)rules.EnabledConditions)[0] = WinCondition.Diagonal);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = board[-1, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = board[0, 2]);
    }

    private void AssertCondition(char[,] cells, WinCondition condition)
    {
        var rules = new BoardRules<char>('.', 4, [condition]);
        var result = analyzer.Analyze(Board<char>.FromArray(cells, rules));
        Assert.Equal(BoardStatus.Win, result.Status);
        Assert.Equal(condition, Assert.Single(result.WinningConditions));
    }
}
