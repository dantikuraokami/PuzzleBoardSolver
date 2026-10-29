# PuzzleBoardSolver

A .NET 10 solution for analyzing a configurable rectangular Tic-Tac-Toe / Connect Four-style board. It detects patterns only; it does not manage gameplay, generate moves, validate legal history, search positions, or implement an AI.

## Projects

- `PuzzleBoardSolver.Core` contains immutable `Board<T>` snapshots, `BoardRules<T>`, `GameAnalysis<T>`, status semantics, and the single authoritative `TicTacToe` analyzer.
- `PuzzleBoardSolver` demonstrates four representative scenarios.
- `PuzzleBoardSolver.Tests` contains xUnit tests for required behavior and extensibility.
- `PuzzleBoardSolver.Benchmarks` contains BenchmarkDotNet workloads.

Required SDK: .NET 10 SDK. The repository currently targets `net10.0` and enables nullable reference types.

## Commands

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project .\PuzzleBoardSolver\PuzzleBoardSolver.csproj
dotnet run -c Release --project .\PuzzleBoardSolver.Benchmarks\PuzzleBoardSolver.Benchmarks.csproj
```

The benchmark command runs the suite and reports mean, error, standard deviation, allocation, and Gen0 data where BenchmarkDotNet supports them. `AnalyzeDefault4x4` is the declared baseline; workloads with different dimensions or board contents should be compared only as separately described scenarios.

## API

```csharp
var analyzer = new TicTacToe();
char? winner = analyzer.CheckWinner(cells); // rectangular char[,] and '.' empty by default
GameAnalysis<char> result = analyzer.Analyze(cells);
```

For generic boards, prefer the unambiguous `TryCheckWinner<T>(board, out winner)` method. It returns `true` only for exactly one distinct winner, so a valid value such as integer `0` cannot be confused with “no winner.” The generic `CheckWinner<T>` method is retained for compatibility, but returns `default(T)` when there is no single winner; use `GameAnalysis<T>` or `TryCheckWinner` when that distinction matters.

For reusable analysis, construct a snapshot with `Board<T>(rows, columns, rowMajorCells, rules)` or `Board<T>.FromArray(array, rules)`. The board copies its input into stable row-major storage. `BoardRules<T>` configures the empty value, positive line win length, enabled conditions, and optional equality comparer. All five conditions are enabled by default; the character convenience API uses `'.'` and a line length of four.

Supported conditions:

- `Horizontal`, `Vertical`, and `Diagonal` detect contiguous matching non-empty runs of the configured line length.
- `AllFourCorners` checks the actual four outer corners, independent of line length.
- `Box2x2` checks every contiguous 2x2 block, independent of line length.

Rules that cannot fit a board produce no pattern. Rectangular, square, larger, and smaller positive dimensions are supported; fixed corner and 2x2 rules naturally require at least 2 rows and 2 columns.

## Results and semantics

`GameAnalysis<T>` contains immutable `Winners`, `WinningConditions`, `AnyMovesLeft`, `Status`, `IsValid`, and `IsGameOver` values. Winners are distinct and returned in deterministic discovery order; conditions are deterministic enum order. Empty values never win. Multiple patterns for one value are one winner. Multiple distinct winners produce `Invalid`, no single winner from `CheckWinner`, and `IsGameOver == true`.

- `InProgress`: no pattern and at least one empty cell.
- `Win`: exactly one distinct winner, even if empty cells remain.
- `Draw`: no pattern and no empty cells.
- `Invalid`: multiple distinct winning values. This means conflicting detected patterns, not an assertion that move history is illegal.

`AnyMovesLeft` is independent of winner detection. `IsGameOver` is true for `Win`, `Draw`, and `Invalid`, and false only for `InProgress`.

## Safety and performance

`Board<T>` validates dimensions and cell counts, rejects null reference cells, copies caller-provided enumerables, and exposes read-only cells plus bounds-checked indexing. Analysis does not mutate input. Independent analyses are safe to run concurrently because the analyzer has no mutable per-analysis state. Copying a mutable caller collection is not an atomic snapshot if another thread mutates that collection during the copy; callers must provide stable input while constructing a board. Rectangular-array convenience analysis copies the array during board construction and has the same concurrent-mutation limitation during that copy.

Line checks are proportional to the number of board positions and configured line length; corner checking is constant time and 2x2 checking is proportional to board area. The implementation favors straightforward repeated checks and immutable result allocation over speculative caching. Benchmark setup prepares production boards, and every benchmark calls `TicTacToe.Analyze`.

## Tests and limitations

The tests cover all five conditions independently, empty cells, draws, wins with remaining moves, repeated and conflicting winners, validation, null inputs and cells, rectangular/custom configurations, non-character cells, immutability, convenience consistency, value-type winner ambiguity, and concurrent independent analysis. No arbitrary performance threshold is imposed on tests.

The analyzer intentionally does not perform legal move-history validation, move generation, minimax, future-move analysis, a UI, or plugin-based rule expressions. Future enhancements could include richer winning-pattern coordinates or optional caller-controlled diagnostic detail without changing the existing status model.

## AI-assisted transcript

`AI-ASSISTED-TRANSCRIPT.md` contains the exported AI-assisted development transcript.
