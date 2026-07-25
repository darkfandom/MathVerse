using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements Dijkstra's shortest-path search using a priority queue.</summary>
public sealed class DijkstraSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes Dijkstra's search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        SortedList<double, (double[] state, double cost, List<double[]> path)> priorityQueue = new();
        HashSet<string> visited = [];

        List<double[]> startPath = [initialState];
        priorityQueue.Add(0, (initialState, 0, startPath));

        int nodesExplored = 0;

        while (priorityQueue.Count > 0)
        {
            double lowestCost = priorityQueue.Keys[0];
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
                    double newCost = currentCost + transitionCost;
                    List<double[]> newPath = [.. currentPath, successorState];

                    double sortKey = newCost;
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
