namespace MathVerse.Math.AI.ScientificOptimization;

/// <summary>Represents a Pareto front of non-dominated solutions.</summary>
public sealed class ParetoFront
{
    /// <summary>Gets the set of non-dominated solution vectors.</summary>
    public List<double[]> Solutions { get; init; } = [];

    /// <summary>Gets the objective values for each solution.</summary>
    public List<double[]> ObjectiveValues { get; init; } = [];

    /// <summary>Gets the crowding distances for each solution.</summary>
    public double[] CrowdingDistances { get; init; } = [];

    /// <summary>Gets the number of solutions on the front.</summary>
    public int Count => Solutions.Count;
}
