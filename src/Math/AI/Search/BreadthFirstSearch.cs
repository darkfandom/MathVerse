using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements breadth-first search using a queue-based approach.</summary>
public sealed class BreadthFirstSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes breadth-first search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        HashSet<string> visited = [];
        Queue<(double[] state, List<double[]> path, double cost)> frontier = [];

        List<double[]> startPath = [initialState];
        visited.Add(StateKey(initialState));
        frontier.Enqueue((initialState, startPath, 0));

        int nodesExplored = 0;

        while (frontier.Count > 0)
        {
            (double[] currentState, List<double[]> currentPath, double currentCost) = frontier.Dequeue();
            nodesExplored++;

            if (problem.IsGoal(currentState))
            {
                sw.Stop();
                return new SearchResult
                {
                    Found = true,
                    Path = currentPath,
                    TotalCost = currentCost,
                    NodesExplored = nodesExplored,
                    ElapsedTime = sw.Elapsed
                };
            }

            foreach ((double[] successorState, double transitionCost) in problem.GetSuccessors(currentState))
            {
                string key = StateKey(successorState);
                if (!visited.Contains(key))
                {
                    visited.Add(key);

                    List<double[]> newPath = [.. currentPath, successorState];
                    double newCost = currentCost + transitionCost;
                    frontier.Enqueue((successorState, newPath, newCost));
                }
            }
        }

        sw.Stop();
        return new SearchResult
        {
            Found = false,
            Path = [],
            TotalCost = 0,
            NodesExplored = nodesExplored,
            ElapsedTime = sw.Elapsed
        };
    }
}
