namespace MathVerse.Math.AI.Optimization;

/// <summary>L-BFGS optimizer with limited-memory Broyden-Fletcher-Goldfarb-Shanno algorithm.</summary>
public sealed class LBFGSOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "LBFGS";

    /// <summary>Runs L-BFGS optimization with Wolfe line search approximation.</summary>
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
        int m = 10;
        var sHistory = new List<double[]>(m);
        var yHistory = new List<double[]>(m);
        double[] grad = GradientDescentOptimizer.ComputeGradient(objective, x);
        double bestValue = objective(x);
        var bestParams = (double[])x.Clone();
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
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

            double[] dir = ComputeLBFGSDirection(grad, sHistory, yHistory, m);
            double alpha = WolfeLineSearch(objective, x, grad, dir, opts.LearningRate);
            double[] s = new double[n];
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                s[i] = alpha * dir[i];
                x[i] += s[i];
            }

            GradientDescentOptimizer.ClampToBounds(x, opts.LowerBounds, opts.UpperBounds);

            double[] newGrad = GradientDescentOptimizer.ComputeGradient(objective, x);
            for (int i = 0; i < n; i++)
            {
                y[i] = newGrad[i] - grad[i];
            }
            double sy = Dot(s, y);
            if (sy > 1e-10)
            {
                sHistory.Add(s);
                yHistory.Add(y);
                if (sHistory.Count > m)
                {
                    sHistory.RemoveAt(0);
                    yHistory.RemoveAt(0);
                }
            }

            grad = newGrad;
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

    /// <summary>Computes the L-BFGS two-loop recursion search direction.</summary>
    /// <param name="grad">Current gradient.</param>
    /// <param name="sHistory">History of step vectors.</param>
    /// <param name="yHistory">History of gradient difference vectors.</param>
    /// <param name="m">Maximum history size.</param>
    /// <returns>The search direction vector.</returns>
    private static double[] ComputeLBFGSDirection(double[] grad, List<double[]> sHistory, List<double[]> yHistory, int m)
    {
        int n = grad.Length;
        int histCount = System.Math.Min(sHistory.Count, m);
        double[] q = (double[])grad.Clone();
        var alphaArr = new double[histCount];
        var rhoArr = new double[histCount];

        for (int i = histCount - 1; i >= 0; i--)
        {
            double sy = Dot(sHistory[i], yHistory[i]);
            if (System.Math.Abs(sy) < 1e-10) continue;
            rhoArr[i] = 1.0 / sy;
            alphaArr[i] = rhoArr[i] * Dot(sHistory[i], q);
            for (int j = 0; j < n; j++)
            {
                q[j] -= alphaArr[i] * yHistory[i][j];
            }
        }

        if (histCount > 0)
        {
            double yy = Dot(yHistory[histCount - 1], yHistory[histCount - 1]);
            double gamma = System.Math.Abs(yy) > 1e-10 ? 1.0 / yy : 1.0;
            for (int j = 0; j < n; j++)
            {
                q[j] *= gamma;
            }
        }

        for (int i = 0; i < histCount; i++)
        {
            double sy = Dot(sHistory[i], yHistory[i]);
            if (System.Math.Abs(sy) < 1e-10) continue;
            double beta = Dot(yHistory[i], q) / sy;
            for (int j = 0; j < n; j++)
            {
                q[j] += (alphaArr[i] - beta) * sHistory[i][j];
            }
        }

        double[] dir = new double[n];
        for (int i = 0; i < n; i++)
        {
            dir[i] = -q[i];
        }
        return dir;
    }

    /// <summary>Approximate Wolfe line search with backtracking.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">Current parameters.</param>
    /// <param name="grad">Current gradient.</param>
    /// <param name="dir">Search direction.</param>
    /// <param name="initialAlpha">Initial step size.</param>
    /// <returns>The step size alpha.</returns>
    private static double WolfeLineSearch(Func<double[], double> objective, double[] x, double[] grad, double[] dir, double initialAlpha)
    {
        int n = x.Length;
        double c1 = 1e-4;
        double c2 = 0.9;
        double alpha = initialAlpha;
        double f0 = objective(x);
        double dirGrad = Dot(grad, dir);

        for (int i = 0; i < 20; i++)
        {
            double[] xNew = new double[n];
            for (int j = 0; j < n; j++)
            {
                xNew[j] = x[j] + alpha * dir[j];
            }
            double fNew = objective(x);
            double[] gradNew = GradientDescentOptimizer.ComputeGradient(objective, xNew);

            if (fNew > f0 + c1 * alpha * dirGrad)
            {
                alpha *= 0.5;
            }
            else if (Dot(gradNew, dir) < c2 * dirGrad)
            {
                alpha *= 2.0;
            }
            else
            {
                break;
            }
        }

        return alpha;
    }

    /// <summary>Computes the dot product of two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>The dot product.</returns>
    private static double Dot(double[] a, double[] b)
    {
        double sum = 0.0;
        int len = System.Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
}
