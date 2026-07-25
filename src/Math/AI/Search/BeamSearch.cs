using System.Diagnostics;
using System.Linq;

namespace MathVerse.Math.AI.Search;

/// <summary>Implements beam search that keeps only the top-k nodes at each level sorted by heuristic.</summary>
public sealed class BeamSearch
{
    private static string StateKey(double[] state)
    {
        return string.Join(",", state.Select(x => x.ToString("G")));
    }

    /// <summary>Executes beam search on the given problem starting from the initial state.</summary>
    /// <param name="problem">The search problem to solve.</param>
    /// <param name="initialState">The starting state.</param>
    /// <param name="beamWidth">Maximum number of nodes to keep at each level.</param>
    public SearchResult Search(SearchProblem problem, double[] initialState, int beamWidth = 10)
    {
        Stopwatch sw = Stopwatch.StartNew();

        HashSet<string> visited = [];

        List<double[]> startPath = [initialState];
        List<(double[] state, double cost, List<double[]> path)> currentBeam = [(initialState, 0, startPath)];
        visited.Add(StateKey(initialState));

        int nodesExplored = 0;

        while (currentBeam.Count > 0)
        {
            List<(double[] state, double cost, List<double[]> path)> nextBeam = [];

            foreach ((double[] currentState, double currentCost, List<double[]> currentPath) in currentBeam)
            {
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

                        double newCost = currentCost + transitionCost;
                        List<double[]> newPath = [.. currentPath, successorState];
                        nextBeam.Add((successorState, newCost, newPath));
                    }
                }
            }

            currentBeam = [.. nextBeam
                .OrderBy(node => problem.Heuristic(node.state))
                .Take(beamWidth)];
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
