using PuzzleBoardSolver;

var analyzer = new TicTacToe();
Display("4x4 horizontal win", new[,]
{
    { 'X', 'X', 'X', 'X' },
    { '.', 'O', '.', '.' },
    { '.', '.', 'O', '.' },
    { '.', '.', '.', '.' }
}, cells => analyzer.Analyze(cells));

Display("4x4 draw", new[,]
{
    { 'X', 'X', 'O', 'O' },
    { 'O', 'O', 'X', 'X' },
    { 'X', 'O', 'X', 'O' },
    { 'O', 'X', 'O', 'X' }
}, cells => analyzer.Analyze(cells));

Display("4x4 multiple distinct winners", new[,]
{
    { 'X', 'X', 'X', 'X' },
    { 'O', 'O', 'O', 'O' },
    { '.', '.', '.', '.' },
    { '.', '.', '.', '.' }
}, cells => analyzer.Analyze(cells));

var nonSquareRules = new BoardRules<char>('.', 3,
    [WinCondition.Horizontal, WinCondition.Vertical, WinCondition.Diagonal]);
var nonSquare = Board<char>.FromArray(new[,]
{
    { 'A', '.', '.', '.', '.' },
    { '.', 'A', '.', '.', '.' },
    { '.', '.', 'A', '.', '.' }
}, nonSquareRules);
DisplayBoard("3x5 non-square with custom rules", nonSquare, board => analyzer.Analyze(board));

static void Display(string name, char[,] cells, Func<char[,], GameAnalysis<char>> analyze)
{
    var board = Board<char>.FromArray(cells, new BoardRules<char>('.'));
    DisplayResult(name, board, analyze(cells));
}

static void DisplayBoard(string name, Board<char> board, Func<Board<char>, GameAnalysis<char>> analyze)
{
    DisplayResult(name, board, analyze(board));
}

static void DisplayResult(string name, Board<char> board, GameAnalysis<char> result)
{
    Console.WriteLine($"{name} ({board.Rows}x{board.Columns})");
    for (var row = 0; row < board.Rows; row++)
    {
        Console.WriteLine(string.Concat(Enumerable.Range(0, board.Columns).Select(column => board[row, column])));
    }

    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"Winners: {(result.Winners.Count == 0 ? "(none)" : string.Join(", ", result.Winners))}");
    Console.WriteLine($"Winning conditions: {(result.WinningConditions.Count == 0 ? "(none)" : string.Join(", ", result.WinningConditions))}");
    Console.WriteLine($"Moves remain: {result.AnyMovesLeft}");
    Console.WriteLine($"Game over: {result.IsGameOver}");
    Console.WriteLine();
}
