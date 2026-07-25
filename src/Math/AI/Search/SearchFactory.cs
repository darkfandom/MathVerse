using System.Collections.Generic;

namespace MathVerse.Math.AI.Search;

/// <summary>Factory class that creates search algorithm instances by name.</summary>
public sealed class SearchFactory
{
    private static readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BFS"] = "BFS",
        ["BreadthFirst"] = "BFS",
        ["BreadthFirstSearch"] = "BFS",
        ["DFS"] = "DFS",
        ["DepthFirst"] = "DFS",
        ["DepthFirstSearch"] = "DFS",
        ["Dijkstra"] = "Dijkstra",
        ["DijkstraSearch"] = "Dijkstra",
        ["A*"] = "AStar",
        ["AStar"] = "AStar",
        ["AStarSearch"] = "AStar",
        ["Beam"] = "Beam",
        ["BeamSearch"] = "Beam",
        ["BestFirst"] = "BestFirst",
        ["GreedyBestFirst"] = "BestFirst",
        ["BestFirstSearch"] = "BestFirst",
        ["BranchAndBound"] = "BranchAndBound",
        ["BranchAndBoundSearch"] = "BranchAndBound",
        ["IterativeDeepening"] = "IterativeDeepening",
        ["IDS"] = "IterativeDeepening",
        ["IterativeDeepeningSearch"] = "IterativeDeepening",
    };

    /// <summary>Creates a search algorithm instance by name.</summary>
    /// <param name="name">The name or alias of the search algorithm.</param>
    /// <returns>A new instance of the requested search algorithm.</returns>
    /// <exception cref="ArgumentException">Thrown when the algorithm name is not recognized.</exception>
    public static object Create(string name)
    {
        string key = _aliases.TryGetValue(name, out string? canonical) ? canonical : name;

        return key switch
        {
            "BFS" => new BreadthFirstSearch(),
            "DFS" => new DepthFirstSearch(),
            "Dijkstra" => new DijkstraSearch(),
            "AStar" => new AStarSearch(),
            "Beam" => new BeamSearch(),
            "BestFirst" => new BestFirstSearch(),
            "BranchAndBound" => new BranchAndBoundSearch(),
            "IterativeDeepening" => new IterativeDeepeningSearch(),
            _ => throw new ArgumentException($"Unknown search algorithm: '{name}'. Valid names: BFS, DFS, Dijkstra, A*, Beam, BestFirst, BranchAndBound, IterativeDeepening.", nameof(name))
        };
    }

    /// <summary>Returns all supported algorithm names.</summary>
    /// <returns>An array of supported algorithm names.</returns>
    public static string[] GetSupportedAlgorithms()
    {
        return ["BFS", "DFS", "Dijkstra", "AStar", "Beam", "BestFirst", "BranchAndBound", "IterativeDeepening"];
    }
}
