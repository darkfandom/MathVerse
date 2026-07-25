namespace MathVerse.Math.AI.Optimization;

/// <summary>Momentum-based gradient descent optimizer.</summary>
public sealed class MomentumOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "Momentum";

    /// <summary>Runs momentum gradient descent optimization.</summary>
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
        double beta = 0.9;
        double[] velocity = new double[n];
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
                velocity[i] = beta * velocity[i] + grad[i];
                x[i] -= opts.LearningRate * velocity[i];
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
