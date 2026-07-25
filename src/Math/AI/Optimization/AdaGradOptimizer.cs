namespace MathVerse.Math.AI.Optimization;

/// <summary>AdaGrad optimizer with accumulated squared gradient learning rate adaptation.</summary>
public sealed class AdaGradOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "AdaGrad";

    /// <summary>Runs AdaGrad optimization.</summary>
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
        double eps = 1e-8;
        double[] sumSq = new double[n];
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
                sumSq[i] += grad[i] * grad[i];
                x[i] -= opts.LearningRate * grad[i] / (System.Math.Sqrt(sumSq[i]) + eps);
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
