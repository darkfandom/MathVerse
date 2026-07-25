namespace MathVerse.Math.AI.Optimization;

/// <summary>Simulated annealing optimizer with temperature scheduling and Metropolis acceptance.</summary>
public sealed class SimulatedAnnealingOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "SimulatedAnnealing";

    /// <summary>Runs simulated annealing optimization.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector.</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var rng = new Random(opts.RandomSeed);
        int n = initial.Length;
        var x = (double[])initial.Clone();
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        double temp = 1.0;
        double coolingRate = 0.995;
        double minTemp = 1e-8;
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            double[] neighbor = Perturb(x, rng, temp);
            GradientDescentOptimizer.ClampToBounds(neighbor, opts.LowerBounds, opts.UpperBounds);
            double currentVal = objective(x);
            double neighborVal = objective(neighbor);
            double delta = neighborVal - currentVal;

            if (delta < 0.0)
            {
                for (int i = 0; i < n; i++)
                {
                    x[i] = neighbor[i];
                }
            }
            else
            {
                double prob = System.Math.Exp(-delta / temp);
                if (rng.NextDouble() < prob)
                {
                    for (int i = 0; i < n; i++)
                    {
                        x[i] = neighbor[i];
                    }
                }
            }

            double val = objective(x);
            if (val < bestValue)
            {
                bestValue = val;
                bestParams = (double[])x.Clone();
            }

            temp *= coolingRate;
            if (temp < minTemp)
            {
                converged = true;
                break;
            }
        }

        sw.Stop();
        return new OptimizationResult
        {
            Success = true,
            BestParameters = bestParams,
            BestValue = bestValue,
            IterationsExecuted = iter,
            Converged = converged,
            ElapsedTime = sw.Elapsed,
            Metrics = System.Collections.Immutable.ImmutableDictionary<string, double>.Empty
                .Add("FinalTemperature", temp)
        };
    }

    /// <summary>Generates a neighboring solution by random perturbation scaled by temperature.</summary>
    /// <param name="x">Current solution.</param>
    /// <param name="rng">Random number generator.</param>
    /// <param name="temperature">Current temperature.</param>
    /// <returns>A new candidate solution.</returns>
    private static double[] Perturb(double[] x, Random rng, double temperature)
    {
        int n = x.Length;
        double[] neighbor = new double[n];
        double scale = temperature;
        for (int i = 0; i < n; i++)
        {
            neighbor[i] = x[i] + scale * (rng.NextDouble() * 2.0 - 1.0);
        }
        return neighbor;
    }
}
