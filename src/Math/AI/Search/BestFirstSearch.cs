using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements greedy best-first search ordered by heuristic value h(n) only.</summary>
public sealed class BestFirstSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes greedy best-first search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        SortedList<double, (double[] state, double cost, List<double[]> path)> priorityQueue = new();
        HashSet<string> visited = [];

        double initialH = problem.Heuristic(initialState);
        List<double[]> startPath = [initialState];
        priorityQueue.Add(initialH, (initialState, 0, startPath));

        int nodesExplored = 0;

        while (priorityQueue.Count > 0)
        {
            double lowestH = priorityQueue.Keys[0];
            (double[] currentState, double currentCost, List<double[]> currentPath) = priorityQueue.Values[0];
            priorityQueue.RemoveAt(0);

            string key = StateKey(currentState);
            if (visited.Contains(key))
            {
                continue;
            }

            visited.Add(key);
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
                string successorKey = StateKey(successorState);
                if (!visited.Contains(successorKey))
                {
                    double h = problem.Heuristic(successorState);
                    double newCost = currentCost + transitionCost;
                    List<double[]> newPath = [.. currentPath, successorState];

                    double sortKey = h;
                    while (priorityQueue.ContainsKey(sortKey))
                    {
                        sortKey += 1e-10;
                    }

                    priorityQueue.Add(sortKey, (successorState, newCost, newPath));
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
