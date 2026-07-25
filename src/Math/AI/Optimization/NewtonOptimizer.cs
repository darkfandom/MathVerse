namespace MathVerse.Math.AI.Optimization;

/// <summary>Newton's method optimizer using numerical Hessian approximation.</summary>
public sealed class NewtonOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "Newton";

    /// <summary>Runs Newton's method optimization with numerical Hessian.</summary>
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

            double[,] hessian = ComputeHessian(objective, x);
            double[] step = SolveLinearSystem(hessian, grad, n);

            for (int i = 0; i < n; i++)
            {
                x[i] -= step[i];
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

    /// <summary>Computes the numerical Hessian matrix using central differences.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">The point at which to evaluate the Hessian.</param>
    /// <returns>The Hessian matrix.</returns>
    private static double[,] ComputeHessian(Func<double[], double> objective, double[] x)
    {
        int n = x.Length;
        double h = 1e-5;
        double[,] hessian = new double[n, n];
        double f0 = objective(x);

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double[] xpp = (double[])x.Clone();
                double[] xpm = (double[])x.Clone();
                double[] xmp = (double[])x.Clone();
                double[] xmm = (double[])x.Clone();

                xpp[i] += h;
                xpp[j] += h;
                xpm[i] += h;
                xpm[j] -= h;
                xmp[i] -= h;
                xmp[j] += h;
                xmm[i] -= h;
                xmm[j] -= h;

                double fpp = objective(xpp);
                double fpm = objective(xpm);
                double fmp = objective(xmp);
                double fmm = objective(xmm);

                hessian[i, j] = (fpp - fpm - fmp + fmm) / (4.0 * h * h);
                hessian[j, i] = hessian[i, j];
            }
        }

        return hessian;
    }

    /// <summary>Solves the linear system Hx = g using Gaussian elimination with partial pivoting.</summary>
    /// <param name="H">The Hessian matrix.</param>
    /// <param name="g">The gradient vector.</param>
    /// <param name="n">The system dimension.</param>
    /// <returns>The solution vector x.</returns>
    private static double[] SolveLinearSystem(double[,] H, double[] g, int n)
    {
        double[,] aug = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                aug[i, j] = H[i, j];
            }
            aug[i, n] = g[i];
        }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(aug[col, col]);
            for (int row = col + 1; row < n; row++)
            {
                if (System.Math.Abs(aug[row, col]) > maxVal)
                {
                    maxVal = System.Math.Abs(aug[row, col]);
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                for (int j = 0; j <= n; j++)
                {
                    (aug[col, j], aug[maxRow, j]) = (aug[maxRow, j], aug[col, j]);
                }
            }

            double diag = aug[col, col];
            if (System.Math.Abs(diag) < 1e-12)
            {
                diag = 1e-12;
            }

            for (int j = col; j <= n; j++)
            {
                aug[col, j] /= diag;
            }

            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = aug[row, col];
                for (int j = col; j <= n; j++)
                {
                    aug[row, j] -= factor * aug[col, j];
                }
            }
        }

        double[] result = new double[n];
        for (int i = 0; i < n; i++)
        {
            result[i] = aug[i, n];
        }
        return result;
    }
}
