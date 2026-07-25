namespace MathVerse.Math.AI.Optimization;

/// <summary>Gradient descent optimizer using numerical central-difference gradients.</summary>
public sealed class GradientDescentOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "GradientDescent";

    /// <summary>Runs gradient descent optimization on the given objective function.</summary>
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
            double[] grad = ComputeGradient(objective, x);
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
                x[i] -= opts.LearningRate * grad[i];
            }

            ClampToBounds(x, opts.LowerBounds, opts.UpperBounds);

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

    /// <summary>Computes the numerical gradient using central differences.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">The point at which to evaluate the gradient.</param>
    /// <returns>The gradient vector.</returns>
    internal static double[] ComputeGradient(Func<double[], double> objective, double[] x)
    {
        int n = x.Length;
        double h = 1e-7;
        double[] grad = new double[n];
        for (int i = 0; i < n; i++)
        {
            double xi = x[i];
            x[i] = xi + h;
            double fph = objective(x);
            x[i] = xi - h;
            double fmh = objective(x);
            x[i] = xi;
            grad[i] = (fph - fmh) / (2.0 * h);
        }
        return grad;
    }

    /// <summary>Clamps each parameter to its corresponding bounds.</summary>
    /// <param name="x">The parameter vector to clamp in-place.</param>
    /// <param name="lower">Lower bounds, or null for no lower bound.</param>
    /// <param name="upper">Upper bounds, or null for no upper bound.</param>
    internal static void ClampToBounds(double[] x, double[]? lower, double[]? upper)
    {
        for (int i = 0; i < x.Length; i++)
        {
            if (lower != null && i < lower.Length && x[i] < lower[i])
            {
                x[i] = lower[i];
            }
            if (upper != null && i < upper.Length && x[i] > upper[i])
            {
                x[i] = upper[i];
            }
        }
    }
}
