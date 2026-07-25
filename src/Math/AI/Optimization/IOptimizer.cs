namespace MathVerse.Math.AI.Optimization;

/// <summary>Interface for mathematical optimization algorithms.</summary>
public interface IOptimizer
{
    /// <summary>Gets the name of the optimizer.</summary>
    string Name { get; }

    /// <summary>Runs optimization on the given objective function.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector.</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result containing best parameters and metrics.</returns>
    OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null);
}
