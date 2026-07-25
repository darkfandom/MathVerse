namespace MathVerse.Math.AI.Optimization;

/// <summary>Trust region optimizer using Cauchy point computation and radius adaptation.</summary>
public sealed class TrustRegionOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "TrustRegion";

    /// <summary>Initial trust region radius.</summary>
    private double _radius = 1.0;

    /// <summary>Runs trust region optimization.</summary>
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
        _radius = 1.0;
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
            double[] step = ComputeCauchyPoint(hessian, grad, n, _radius);
            double f0 = objective(x);
            double[] xNew = new double[n];
            for (int i = 0; i < n; i++)
            {
                xNew[i] = x[i] + step[i];
            }
            double fNew = objective(xNew);
            double actualReduction = f0 - fNew;
            double predictedReduction = -Dot(grad, step);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    predictedReduction -= 0.5 * step[i] * hessian[i, j] * step[j];
                }
            }

            double ratio = System.Math.Abs(predictedReduction) > 1e-15
                ? actualReduction / predictedReduction
                : 0.0;

            if (ratio > 0.75)
            {
                _radius = System.Math.Min(2.0 * _radius, 10.0);
            }
            else if (ratio < 0.25)
            {
                _radius *= 0.25;
            }

            if (actualReduction > 0.0 && ratio > 0.1)
            {
                for (int i = 0; i < n; i++)
                {
                    x[i] = xNew[i];
                }
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

    /// <summary>Computes the Cauchy point within the trust region.</summary>
    /// <param name="H">The Hessian matrix.</param>
    /// <param name="grad">The gradient vector.</param>
    /// <param name="n">The dimension.</param>
    /// <param name="radius">The trust region radius.</param>
    /// <returns>The step vector to the Cauchy point.</returns>
    private static double[] ComputeCauchyPoint(double[,] H, double[] grad, int n, double radius)
    {
        double[] dir = new double[n];
        double gNorm = 0.0;
        for (int i = 0; i < n; i++)
        {
            dir[i] = -grad[i];
            gNorm += grad[i] * grad[i];
        }
        gNorm = System.Math.Sqrt(gNorm);
        if (gNorm < 1e-15)
        {
            return dir;
        }

        double gHg = 0.0;
        for (int i = 0; i < n; i++)
        {
            double tmp = 0.0;
            for (int j = 0; j < n; j++)
            {
                tmp += H[i, j] * grad[j];
            }
            gHg += grad[i] * tmp;
        }

        double tau;
        if (gHg <= 0.0)
        {
            tau = radius / gNorm;
        }
        else
        {
            tau = System.Math.Min(gNorm * gNorm / gHg, radius / gNorm);
        }

        for (int i = 0; i < n; i++)
        {
            dir[i] = -tau * grad[i];
        }

        double stepNorm = 0.0;
        for (int i = 0; i < n; i++)
        {
            stepNorm += dir[i] * dir[i];
        }
        stepNorm = System.Math.Sqrt(stepNorm);
        if (stepNorm > radius)
        {
            double scale = radius / stepNorm;
            for (int i = 0; i < n; i++)
            {
                dir[i] *= scale;
            }
        }

        return dir;
    }

    /// <summary>Computes the numerical Hessian matrix.</summary>
    /// <param name="objective">The objective function.</param>
    /// <param name="x">Current parameters.</param>
    /// <returns>The Hessian matrix.</returns>
    private static double[,] ComputeHessian(Func<double[], double> objective, double[] x)
    {
        int n = x.Length;
        double h = 1e-5;
        double[,] hessian = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double[] xpp = (double[])x.Clone();
                double[] xpm = (double[])x.Clone();
                double[] xmp = (double[])x.Clone();
                double[] xmm = (double[])x.Clone();

                xpp[i] += h; xpp[j] += h;
                xpm[i] += h; xpm[j] -= h;
                xmp[i] -= h; xmp[j] += h;
                xmm[i] -= h; xmm[j] -= h;

                hessian[i, j] = (objective(xpp) - objective(xpm) - objective(xmp) + objective(xmm)) / (4.0 * h * h);
                hessian[j, i] = hessian[i, j];
            }
        }

        return hessian;
    }

    /// <summary>Computes the dot product of two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>The dot product.</returns>
    private static double Dot(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
}
