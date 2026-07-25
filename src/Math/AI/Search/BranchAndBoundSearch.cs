using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements branch-and-bound search with pruning when lower bound exceeds best solution.</summary>
public sealed class BranchAndBoundSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes branch-and-bound search on the given problem starting from the initial state.</summary>
    public SearchResult Search(SearchProblem problem, double[] initialState)
    {
        Stopwatch sw = Stopwatch.StartNew();

        SortedList<double, (double[] state, double cost, double lowerBound, List<double[]> path)> priorityQueue = new();
        HashSet<string> visited = [];

        double initialH = problem.Heuristic(initialState);
        List<double[]> startPath = [initialState];
        priorityQueue.Add(initialH, (initialState, 0, initialH, startPath));

        double bestCost = double.MaxValue;
        List<double[]> bestPath = [];
        int nodesExplored = 0;

        while (priorityQueue.Count > 0)
        {
            double lowestBound = priorityQueue.Keys[0];
            (double[] currentState, double currentCost, double currentLowerBound, List<double[]> currentPath) = priorityQueue.Values[0];
            priorityQueue.RemoveAt(0);

            if (currentLowerBound >= bestCost)
            {
                continue;
            }

            string key = StateKey(currentState);
            if (visited.Contains(key))
            {
                continue;
            }

            visited.Add(key);
            nodesExplored++;

            if (problem.IsGoal(currentState))
            {
                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
                    bestPath = currentPath;
                }
                continue;
            }

            foreach ((double[] successorState, double transitionCost) in problem.GetSuccessors(currentState))
            {
                string successorKey = StateKey(successorState);
                if (!visited.Contains(successorKey))
                {
                    double newCost = currentCost + transitionCost;
                    double newLowerBound = newCost + problem.Heuristic(successorState);

                    if (newLowerBound < bestCost)
                    {
                        List<double[]> newPath = [.. currentPath, successorState];

                        double sortKey = newLowerBound;
                        while (priorityQueue.ContainsKey(sortKey))
                        {
                            sortKey += 1e-10;
                        }

                        priorityQueue.Add(sortKey, (successorState, newCost, newLowerBound, newPath));
                    }
                }
            }
        }

        sw.Stop();
        bool found = bestPath.Count > 0;
        return new SearchResult
        {
            Found = found,
            Path = found ? bestPath : [],
            TotalCost = found ? bestCost : 0,
            NodesExplored = nodesExplored,
            ElapsedTime = sw.Elapsed
        };
    }
}
