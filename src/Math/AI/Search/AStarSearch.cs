using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements A* search using f(n) = g(n) + h(n) as the priority ordering.</summary>
public sealed class AStarSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes A* search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        SortedList<double, (double[] state, double gCost, List<double[]> path)> openSet = new();
        HashSet<string> closedSet = [];

        double h = problem.Heuristic(initialState);
        List<double[]> startPath = [initialState];
        openSet.Add(h, (initialState, 0, startPath));

        int nodesExplored = 0;

        while (openSet.Count > 0)
        {
            double lowestF = openSet.Keys[0];
            (double[] currentState, double gCost, List<double[]> currentPath) = openSet.Values[0];
            openSet.RemoveAt(0);

            string key = StateKey(currentState);
            if (closedSet.Contains(key))
            {
                continue;
            }

            closedSet.Add(key);
            nodesExplored++;

            if (problem.IsGoal(currentState))
            {
                sw.Stop();
                return new SearchResult
                {
                    Found = true,
                    Path = currentPath,
                    TotalCost = gCost,
                    NodesExplored = nodesExplored,
                    ElapsedTime = sw.Elapsed
                };
            }

            foreach ((double[] successorState, double transitionCost) in problem.GetSuccessors(currentState))
            {
                string successorKey = StateKey(successorState);
                if (!closedSet.Contains(successorKey))
                {
                    double newG = gCost + transitionCost;
                    double newF = newG + problem.Heuristic(successorState);
                    List<double[]> newPath = [.. currentPath, successorState];

                    double sortKey = newF;
                    while (openSet.ContainsKey(sortKey))
                    {
                        sortKey += 1e-10;
                    }

                    openSet.Add(sortKey, (successorState, newG, newPath));
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
