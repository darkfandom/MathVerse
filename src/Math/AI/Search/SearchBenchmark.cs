using System.Diagnostics;

namespace MathVerse.Math.AI.Search;

/// <summary>Provides benchmarking capabilities for search algorithms.</summary>
public sealed class SearchBenchmark
{
    /// <summary>Represents the benchmark result for a search algorithm run.</summary>
    public sealed class BenchmarkResult
    {
        /// <summary>The name of the search algorithm benchmarked.</summary>
        public string AlgorithmName { get; init; } = string.Empty;

        /// <summary>Whether the search found the goal.</summary>
        public bool Found { get; init; }

        /// <summary>The number of nodes explored during the search.</summary>
        public int NodesExplored { get; init; }

        /// <summary>The time elapsed during the search.</summary>
        public TimeSpan TimeTaken { get; init; }

        /// <summary>The total cost of the found path.</summary>
        public double PathCost { get; init; }

        /// <summary>The length of the found path (number of states).</summary>
        public int PathLength { get; init; }

        /// <summary>The time elapsed in milliseconds.</summary>
        public double TimeMilliseconds => TimeTaken.TotalMilliseconds;

        /// <summary>Returns a string representation of the benchmark result.</summary>
        public override string ToString()
        {
            return $"{AlgorithmName}: Found={Found}, Nodes={NodesExplored}, Time={TimeMilliseconds:F3}ms, Cost={PathCost:G}, PathLen={PathLength}";
        }
    }

    /// <summary>Benchmarks a search algorithm on the given problem.</summary>
    /// <param name="algorithmName">The name of the search algorithm to benchmark.</param>
    /// <param name="problem">The search problem to solve.</param>
    /// <param name="initialState">The starting state for the search.</param>
    /// <returns>A benchmark result containing performance metrics.</returns>
    public static BenchmarkResult Run(string algorithmName, SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();
        SearchResult result = RunSearch(algorithmName, problem, initialState);
        sw.Stop();

        return new BenchmarkResult
        {
            AlgorithmName = algorithmName,
            Found = result.Found,
            NodesExplored = result.NodesExplored,
            TimeTaken = result.ElapsedTime,
            PathCost = result.TotalCost,
            PathLength = result.Path.Count
        };
    }

    /// <summary>Benchmarks a search algorithm multiple times and returns aggregated results.</summary>
    /// <param name="algorithmName">The name of the search algorithm to benchmark.</param>
    /// <param name="problem">The search problem to solve.</param>
    /// <param name="initialState">The starting state for the search.</param>
    /// <param name="iterations">The number of benchmark iterations to run.</param>
    /// <returns>An array of benchmark results, one per iteration.</returns>
    public static BenchmarkResult[] RunMultiple(string algorithmName, SearchProblem problem, double[] initialState, int iterations)
    {
        BenchmarkResult[] results = new BenchmarkResult[iterations];

        for (int i = 0; i < iterations; i++)
        {
            results[i] = Run(algorithmName, problem, initialState);
        }

        return results;
    }

    /// <summary>Benchmarks and compares multiple search algorithms on the same problem.</summary>
    /// <param name="algorithmNames">The names of the search algorithms to compare.</param>
    /// <param name="problem">The search problem to solve.</param>
    /// <param name="initialState">The starting state for the search.</param>
    /// <returns>An array of benchmark results, one per algorithm.</returns>
    public static BenchmarkResult[] Compare(string[] algorithmNames, SearchProblem problem, double[] initialState)
    {
        BenchmarkResult[] results = new BenchmarkResult[algorithmNames.Length];

        for (int i = 0; i < algorithmNames.Length; i++)
        {
            results[i] = Run(algorithmNames[i], problem, initialState);
        }

        return results;
    }

    private static SearchResult RunSearch(string algorithmName, SearchProblem problem, double[] initialState)
    {
        switch (algorithmName)
        {
            case "BFS":
            case "BreadthFirst":
            case "BreadthFirstSearch":
                return new BreadthFirstSearch().Search(problem, initialState);

            case "DFS":
            case "DepthFirst":
            case "DepthFirstSearch":
                return new DepthFirstSearch().Search(problem, initialState);

            case "Dijkstra":
            case "DijkstraSearch":
                return new DijkstraSearch().Search(problem, initialState);

            case "A*":
            case "AStar":
            case "AStarSearch":
                return new AStarSearch().Search(problem, initialState);

            case "Beam":
            case "BeamSearch":
                return new BeamSearch().Search(problem, initialState);

            case "BestFirst":
            case "GreedyBestFirst":
            case "BestFirstSearch":
                return new BestFirstSearch().Search(problem, initialState);

            case "BranchAndBound":
            case "BranchAndBoundSearch":
                return new BranchAndBoundSearch().Search(problem, initialState);

            case "IterativeDeepening":
            case "IDS":
            case "IterativeDeepeningSearch":
                return new IterativeDeepeningSearch().Search(problem, initialState);

            default:
                throw new ArgumentException($"Unknown search algorithm: '{algorithmName}'.", nameof(algorithmName));
        }
    }
}
