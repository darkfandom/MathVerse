namespace MathVerse.Math.AI.ScientificOptimization;
using System;
using System.Collections.Immutable;

/// <summary>Result of a constrained optimization solve.</summary>
public sealed class ConstraintSolverResult
{
    /// <summary>Whether the solver converged successfully.</summary>
    public bool Success { get; init; }

    /// <summary>The optimal parameter vector found.</summary>
    public double[] BestParameters { get; init; } = [];

    /// <summary>The objective function value at the best parameters.</summary>
    public double BestValue { get; init; }

    /// <summary>The final Lagrange multiplier estimates.</summary>
    public double[] LagrangeMultipliers { get; init; } = [];

    /// <summary>The final constraint violation values.</summary>
    public double[] ConstraintViolations { get; init; } = [];

    /// <summary>The number of iterations executed.</summary>
    public int IterationsExecuted { get; init; }

    /// <summary>Whether the solution is feasible.</summary>
    public bool IsFeasible { get; init; }

    /// <summary>The total elapsed time for the solve.</summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>Additional metrics from the solve.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;
}
