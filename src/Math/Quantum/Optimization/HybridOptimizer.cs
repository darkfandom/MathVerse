namespace MathVerse.Math.Quantum.Optimization;

using System;

/// <summary>
/// Result of a hybrid classical-quantum optimization run.
/// </summary>
public sealed class HybridResult
{
    /// <summary>Gets the optimal cost value found.</summary>
    public double OptimalValue { get; init; }

    /// <summary>Gets the optimal parameter vector.</summary>
    public double[] OptimalParameters { get; init; } = Array.Empty<double>();

    /// <summary>Gets the number of classical optimization iterations performed.</summary>
    public int ClassicalIterations { get; init; }

    /// <summary>Gets the number of quantum evaluations performed.</summary>
    public int QuantumIterations { get; init; }
}

/// <summary>
/// Classical-quantum hybrid optimizer that alternates between classical gradient-free
/// optimization and quantum cost function evaluation for variational algorithms.
/// </summary>
public sealed class HybridOptimizer
{
    private readonly Func<double[], double> _classicalCost;
    private readonly Func<double[], double> _quantumCost;
    private readonly Random _rng;

    /// <summary>Creates a hybrid classical-quantum optimizer.</summary>
    /// <param name="classicalCost">The classical cost function.</param>
    /// <param name="quantumCost">The quantum cost function evaluated on a quantum processor/simulator.</param>
    public HybridOptimizer(Func<double[], double> classicalCost, Func<double[], double> quantumCost)
    {
        _classicalCost = classicalCost ?? throw new ArgumentNullException(nameof(classicalCost));
        _quantumCost = quantumCost ?? throw new ArgumentNullException(nameof(quantumCost));
        _rng = new Random(42);
    }

    /// <summary>Runs the hybrid optimization starting from the given initial parameters.</summary>
    /// <param name="initialParams">The initial parameter vector.</param>
    /// <param name="maxIterations">The maximum number of iterations.</param>
    /// <returns>A <see cref="HybridResult"/> containing the optimal parameters and cost.</returns>
    public HybridResult Optimize(double[] initialParams, int maxIterations)
    {
        if (initialParams == null) throw new ArgumentNullException(nameof(initialParams));
        if (maxIterations < 1) throw new ArgumentOutOfRangeException(nameof(maxIterations));

        int n = initialParams.Length;
        var bestParams = (double[])initialParams.Clone();
        double bestCost = _quantumCost(bestParams) + _classicalCost(bestParams);

        int classicalIter = 0;
        int quantumIter = 1;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var trialParams = (double[])bestParams.Clone();
            double stepSize = 0.1 / System.Math.Sqrt(iter + 1);

            for (int i = 0; i < n; i++)
                trialParams[i] += (_rng.NextDouble() - 0.5) * 2.0 * stepSize;

            double classicalPart = _classicalCost(trialParams);
            classicalIter++;

            double quantumPart = _quantumCost(trialParams);
            quantumIter++;

            double trialCost = classicalPart + quantumPart;

            if (trialCost < bestCost)
            {
                bestCost = trialCost;
                bestParams = trialParams;
            }
        }

        return new HybridResult
        {
            OptimalValue = bestCost,
            OptimalParameters = bestParams,
            ClassicalIterations = classicalIter,
            QuantumIterations = quantumIter
        };
    }
}
