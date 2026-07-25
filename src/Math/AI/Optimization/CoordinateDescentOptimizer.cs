namespace MathVerse.Math.AI.Optimization;

/// <summary>Coordinate descent optimizer that optimizes one coordinate at a time in cyclic order.</summary>
public sealed class CoordinateDescentOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "CoordinateDescent";

    /// <summary>Runs coordinate descent optimization.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector.</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var x = (double[])initial.Clone();
        int n = x.Length;
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            double maxDelta = 0.0;
            for (int coord = 0; coord < n; coord++)
            {
                double f0 = objective(x);
                double dir = ComputePartialDerivative(objective, x, coord);
                double step = opts.LearningRate;

                double xNew = x[coord] - step * dir;
                if (opts.LowerBounds != null && coord < opts.LowerBounds.Length && xNew < opts.LowerBounds[coord])
                {
                    xNew = opts.LowerBounds[coord];
                }
                if (opts.UpperBounds != null && coord < opts.UpperBounds.Length && xNew > opts.UpperBounds[coord])
                {
                    xNew = opts.UpperBounds[coord];
                }

                x[coord] = xNew;
                double fNew = objective(x);

                if (fNew < f0)
                {
                    double delta = System.Math.Abs(f0 - fNew);
                    if (delta > maxDelta) maxDelta = delta;
                }
                else
                {
                    x[coord] = x[coord] + step * dir;
                }
            }

            if (maxDelta < opts.Tolerance)
            {
                converged = true;
                break;
            }

            double val = objective(x);
            if (val < bestValue)
            {
                bestValue = val;
                bestParams = (double[])x.Clone();
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
            ElapsedTime = sw.Elapsed
        };
    }

    /// <summary>Computes the partial derivative with respect to a single coordinate.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">Current parameter vector.</param>
    /// <param name="coord">The coordinate index.</param>
    /// <returns>The partial derivative value.</returns>
    private static double ComputePartialDerivative(Func<double[], double> objective, double[] x, int coord)
    {
        double h = 1e-7;
        double xi = x[coord];
        x[coord] = xi + h;
        double fph = objective(x);
        x[coord] = xi - h;
        double fmh = objective(x);
        x[coord] = xi;
        return (fph - fmh) / (2.0 * h);
    }
}
