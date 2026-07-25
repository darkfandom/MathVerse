namespace MathVerse.Math.AI.Search;

/// <summary>Defines a search problem with states, actions, and goal test.</summary>
public sealed class SearchProblem
{
    /// <summary>Tests whether a given state is the goal state.</summary>
    public Func<double[], bool> IsGoal { get; init; } = _ => false;

    /// <summary>Returns successor states and their transition costs from the given state.</summary>
    public Func<double[], IEnumerable<(double[] state, double cost)>> GetSuccessors { get; init; } = _ => [];

    /// <summary>Estimates the cost from a state to the goal.</summary>
    public Func<double[], double> Heuristic { get; init; } = _ => 0;

    /// <summary>Computes the actual transition cost between two states.</summary>
    public Func<double[], double[], double> TransitionCost { get; init; } = (_, _) => 1;
}

/// <summary>Result of a search operation.</summary>
public sealed class SearchResult
{
    /// <summary>Whether the goal state was found.</summary>
    public bool Found { get; init; }

    /// <summary>The path from the start state to the goal state.</summary>
    public List<double[]> Path { get; init; } = [];

    /// <summary>The total cost of the path.</summary>
    public double TotalCost { get; init; }

    /// <summary>The number of nodes explored during the search.</summary>
    public int NodesExplored { get; init; }

    /// <summary>The time elapsed during the search.</summary>
    public TimeSpan ElapsedTime { get; init; }
}
