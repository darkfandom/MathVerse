using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements iterative deepening search combining BFS completeness with DFS space efficiency.</summary>
public sealed class IterativeDeepeningSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes iterative deepening search on the given problem starting from the initial state.</summary>
    /// <param name="problem">The search problem to solve.</param>
    /// <param name="initialState">The starting state.</param>
    /// <param name="maxDepth">The maximum depth limit to search.</param>
    public SearchResult Search(SearchProblem problem, double[] initialState, int maxDepth = 100)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int totalNodesExplored = 0;

        for (int depthLimit = 0; depthLimit <= maxDepth; depthLimit++)
        {
            (bool found, List<double[]> path, double cost, int nodesExplored) =
                DepthLimitedSearch(problem, initialState, depthLimit);

            totalNodesExplored += nodesExplored;

            if (found)
            {
                sw.Stop();
                return new SearchResult
                {
                    Found = true,
                    Path = path,
                    TotalCost = cost,
                    NodesExplored = totalNodesExplored,
                    ElapsedTime = sw.Elapsed
                };
            }
        }

        sw.Stop();
        return new SearchResult
        {
            Found = false,
            Path = [],
            TotalCost = 0,
            NodesExplored = totalNodesExplored,
            ElapsedTime = sw.Elapsed
        };
    }

    private static (bool found, List<double[]> path, double cost, int nodesExplored) DepthLimitedSearch(
        SearchProblem problem, double[] initialState, int depthLimit)
    {
        Stack<(double[] state, List<double[]> path, double cost, int depth)> stack = [];
        HashSet<string> visitedAtDepth = [];

        List<double[]> startPath = [initialState];
        stack.Push((initialState, startPath, 0, 0));

        int nodesExplored = 0;

        while (stack.Count > 0)
        {
            (double[] currentState, List<double[]> currentPath, double currentCost, int currentDepth) = stack.Pop();
            nodesExplored++;

            if (problem.IsGoal(currentState))
            {
                return (true, currentPath, currentCost, nodesExplored);
            }

            if (currentDepth < depthLimit)
            {
                foreach ((double[] successorState, double transitionCost) in problem.GetSuccessors(currentState))
                {
                    List<double[]> newPath = [.. currentPath, successorState];
                    double newCost = currentCost + transitionCost;
                    stack.Push((successorState, newPath, newCost, currentDepth + 1));
                }
            }
        }

        return (false, [], 0, nodesExplored);
    }
}
