namespace MathVerse.Math.AI.Optimization;
using System.Collections.Immutable;

/// <summary>Result of an optimization run.</summary>
public sealed class OptimizationResult
{
    /// <summary>Whether the optimization completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>The best parameter vector found during optimization.</summary>
    public double[] BestParameters { get; init; } = [];

    /// <summary>The objective function value at the best parameters.</summary>
    public double BestValue { get; init; }

    /// <summary>The number of iterations actually executed.</summary>
    public int IterationsExecuted { get; init; }

    /// <summary>Whether the optimizer converged within tolerance.</summary>
    public bool Converged { get; init; }

    /// <summary>The total elapsed time for the optimization run.</summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>Additional metrics collected during optimization.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;
}
