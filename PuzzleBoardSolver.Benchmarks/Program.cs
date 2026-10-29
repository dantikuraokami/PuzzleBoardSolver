using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PuzzleBoardSolver;

BenchmarkRunner.Run<AnalyzerBenchmarks>();

[MemoryDiagnoser]
public class AnalyzerBenchmarks
{
    private readonly TicTacToe analyzer = new();
    private Board<char> default4x4 = null!;
    private Board<char> full4x4 = null!;
    private Board<char> earlyEmpty4x4 = null!;
    private Board<char> lateEmpty4x4 = null!;
    private Board<char> board8x8 = null!;
    private Board<char> board16x16 = null!;
    private Board<char> nonSquare = null!;
    private Board<char> multiplePatterns = null!;

    [GlobalSetup]
    public void Setup()
    {
        default4x4 = CreateBoard(4, 4, '.');
        full4x4 = CreateBoard(4, 4, 'X');
        earlyEmpty4x4 = CreateBoard(4, 4, '.');
        lateEmpty4x4 = CreateBoard(4, 4, 'X');
        lateEmpty4x4 = new Board<char>(4, 4, lateEmpty4x4.Cells.Select((value, index) => index == 15 ? '.' : value), new BoardRules<char>('.'));
        board8x8 = CreateBoard(8, 8, '.');
        board16x16 = CreateBoard(16, 16, '.');
        nonSquare = CreateBoard(3, 5, '.');
        multiplePatterns = new Board<char>(4, 4, ['X', 'X', 'X', 'X', 'X', '.', '.', 'X', 'X', '.', '.', 'X', 'X', 'X', 'X', 'X'], new BoardRules<char>('.'));
    }

    [Benchmark(Baseline = true)] public GameAnalysis<char> AnalyzeDefault4x4() => analyzer.Analyze(default4x4);
    [Benchmark] public GameAnalysis<char> AnalyzeFull4x4() => analyzer.Analyze(full4x4);
    [Benchmark] public GameAnalysis<char> AnalyzeEarlyEmpty4x4() => analyzer.Analyze(earlyEmpty4x4);
    [Benchmark] public GameAnalysis<char> AnalyzeLateEmpty4x4() => analyzer.Analyze(lateEmpty4x4);
    [Benchmark] public GameAnalysis<char> Analyze8x8() => analyzer.Analyze(board8x8);
    [Benchmark] public GameAnalysis<char> Analyze16x16() => analyzer.Analyze(board16x16);
    [Benchmark] public GameAnalysis<char> AnalyzeNonSquareBoard() => analyzer.Analyze(nonSquare);
    [Benchmark] public GameAnalysis<char> AnalyzeMultipleWinningPatterns() => analyzer.Analyze(multiplePatterns);

    private static Board<char> CreateBoard(int rows, int columns, char value) => new(rows, columns, Enumerable.Repeat(value, rows * columns), new BoardRules<char>('.'));
}
