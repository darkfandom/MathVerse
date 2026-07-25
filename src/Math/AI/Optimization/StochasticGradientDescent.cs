namespace MathVerse.Math.AI.Optimization;

/// <summary>Stochastic gradient descent optimizer with mini-batch support and learning rate scheduling.</summary>
public sealed class StochasticGradientDescentOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "SGD";

    /// <summary>Runs stochastic gradient descent optimization.</summary>
    /// <param name="objective">The objective function to minimize. Evaluated as a sum over mini-batches.</param>
    /// <param name="initial">The initial parameter vector.</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var x = (double[])initial.Clone();
        int n = x.Length;
        var rng = new Random(opts.RandomSeed);
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            double lr = opts.LearningRate / System.Math.Sqrt(1.0 + iter);

            double[] grad = ComputeMiniBatchGradient(objective, x, rng);

            double gradNorm = 0.0;
            for (int i = 0; i < n; i++)
            {
                gradNorm += grad[i] * grad[i];
            }
            gradNorm = System.Math.Sqrt(gradNorm);

            if (gradNorm < opts.Tolerance)
            {
                converged = true;
                break;
            }

            for (int i = 0; i < n; i++)
            {
                x[i] -= lr * grad[i];
            }

            GradientDescentOptimizer.ClampToBounds(x, opts.LowerBounds, opts.UpperBounds);

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

    /// <summary>Computes a mini-batch gradient approximation using random sampling.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">Current parameter vector.</param>
    /// <param name="rng">Random number generator.</param>
    /// <returns>The approximate gradient.</returns>
    private static double[] ComputeMiniBatchGradient(Func<double[], double> objective, double[] x, Random rng)
    {
        int n = x.Length;
        double h = 1e-7;
        double[] grad = new double[n];
        int batchSize = System.Math.Max(1, n / 5);
        if (batchSize > n) batchSize = n;

        var indices = new int[n];
        for (int i = 0; i < n; i++) indices[i] = i;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        for (int k = 0; k < batchSize; k++)
        {
            int i = indices[k];
            double xi = x[i];
            x[i] = xi + h;
            double fph = objective(x);
            x[i] = xi - h;
            double fmh = objective(x);
            x[i] = xi;
            grad[i] = (fph - fmh) / (2.0 * h);
        }

        double scale = (double)n / batchSize;
        for (int i = 0; i < n; i++)
        {
            grad[i] *= scale;
        }

        return grad;
    }
}
