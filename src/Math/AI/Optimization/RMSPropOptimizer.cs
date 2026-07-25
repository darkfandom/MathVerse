namespace MathVerse.Math.AI.Optimization;

/// <summary>RMSProp optimizer using exponential moving average of squared gradients.</summary>
public sealed class RMSPropOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "RMSProp";

    /// <summary>Runs RMSProp optimization.</summary>
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
        double decay = 0.9;
        double eps = 1e-8;
        double[] cache = new double[n];
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            double[] grad = GradientDescentOptimizer.ComputeGradient(objective, x);

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
                cache[i] = decay * cache[i] + (1.0 - decay) * grad[i] * grad[i];
                x[i] -= opts.LearningRate * grad[i] / (System.Math.Sqrt(cache[i]) + eps);
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
}
