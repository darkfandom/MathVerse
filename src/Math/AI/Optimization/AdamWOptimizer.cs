namespace MathVerse.Math.AI.Optimization;

/// <summary>AdamW optimizer with decoupled weight decay.</summary>
public sealed class AdamWOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "AdamW";

    /// <summary>Runs AdamW optimization with decoupled weight decay.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector.</param>
    /// <param name="options">Optional optimization options. The LearningRate field also controls weight decay scale.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var x = (double[])initial.Clone();
        int n = x.Length;
        double beta1 = 0.9;
        double beta2 = 0.999;
        double eps = 1e-8;
        double weightDecay = 0.01;
        double[] m = new double[n];
        double[] v = new double[n];
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        bool converged = false;
        int iter;

        for (iter = 1; iter <= opts.MaxIterations; iter++)
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

            double bc1 = 1.0 - System.Math.Pow(beta1, iter);
            double bc2 = 1.0 - System.Math.Pow(beta2, iter);

            for (int i = 0; i < n; i++)
            {
                m[i] = beta1 * m[i] + (1.0 - beta1) * grad[i];
                v[i] = beta2 * v[i] + (1.0 - beta2) * grad[i] * grad[i];
                double mHat = m[i] / bc1;
                double vHat = v[i] / bc2;
                x[i] -= opts.LearningRate * (mHat / (System.Math.Sqrt(vHat) + eps) + weightDecay * x[i]);
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
