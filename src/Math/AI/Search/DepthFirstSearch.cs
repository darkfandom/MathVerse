using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements depth-first search using a stack-based approach with cycle detection.</summary>
public sealed class DepthFirstSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes depth-first search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        HashSet<string> visited = [];
        Stack<(double[] state, List<double[]> path, double cost)> stack = [];

        List<double[]> startPath = [initialState];
        stack.Push((initialState, startPath, 0));
        visited.Add(StateKey(initialState));

        int nodesExplored = 0;

        while (stack.Count > 0)
        {
            (double[] currentState, List<double[]> currentPath, double currentCost) = stack.Pop();
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
                    stack.Push((successorState, newPath, newCost));
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
