namespace MathVerse.Math.AI.Optimization;

/// <summary>Options for optimization algorithms.</summary>
public sealed class OptimizationOptions
{
    /// <summary>Maximum number of iterations allowed.</summary>
    public int MaxIterations { get; init; } = 1000;

    /// <summary>Learning rate (step size) for gradient-based methods.</summary>
    public double LearningRate { get; init; } = 0.01;

    /// <summary>Convergence tolerance for gradient norm or parameter change.</summary>
    public double Tolerance { get; init; } = 1e-8;

    /// <summary>Random seed for stochastic methods.</summary>
    public int RandomSeed { get; init; } = 42;

    /// <summary>Lower bounds for each parameter dimension. Null means unbounded.</summary>
    public double[]? LowerBounds { get; init; }

    /// <summary>Upper bounds for each parameter dimension. Null means unbounded.</summary>
    public double[]? UpperBounds { get; init; }

    /// <summary>Optional constraint function that the solution must satisfy.</summary>
    public Func<double[], double>? Constraint { get; init; }

    /// <summary>Gets the default optimization options.</summary>
    public static OptimizationOptions Default => new();
}
