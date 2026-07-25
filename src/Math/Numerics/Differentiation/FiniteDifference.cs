namespace MathVerse.Math.Numerics.Differentiation;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum FiniteDifference
{
    Forward,
    Backward,
    Central,
    FivePoint,
    SevenPoint
}

public sealed record DerivativeOptions
{
    public FiniteDifference Method { get; init; } = FiniteDifference.Central;
    public double StepSize { get; init; } = 1e-6;
    public bool UseAdaptiveStep { get; init; } = true;
    public double StepSizeGrowth { get; init; } = 1.5;
    public int MaxOrder { get; init; } = 5;

    public static DerivativeOptions Default { get; } = new();
}

public sealed record DerivativeResult
{
    public double Value { get; init; }
    public double ErrorEstimate { get; init; }
    public int FunctionEvaluations { get; init; }
    public bool Success { get; init; }
    public ImmutableArray<double> History { get; init; }
    public DerivativeResult(double value, double errorEstimate, int functionEvaluations, bool success, ImmutableArray<double> history)
    {
        Value = value;
        ErrorEstimate = errorEstimate;
        FunctionEvaluations = functionEvaluations;
        Success = success;
        History = history;
    }
}

public static class Differentiator
{
    public static DerivativeResult Derivative(Func<double, double> f, double x, DerivativeOptions? options = null)
    {
        options ??= DerivativeOptions.Default;
        var history = ImmutableArray.CreateBuilder<double>();
        int evals = 0;
        double bestValue = 0;
        double bestError = double.MaxValue;

        double h = options.StepSize;

        for (int order = 1; order <= options.MaxOrder; order++)
        {
            if (!options.UseAdaptiveStep && order > 1) break;

            double value = ComputeDerivative(f, x, h, options.Method, order, ref evals);
            history.Add(value);

            double error = EstimateError(f, x, h, options.Method, order, value, ref evals);
            if (error < bestError)
            {
                bestError = error;
                bestValue = value;
            }

            if (options.UseAdaptiveStep) h /= options.StepSizeGrowth;
        }

        return new DerivativeResult(bestValue, bestError, evals, bestError < 1e-10, history.ToImmutable());
    }

    private static double ComputeDerivative(Func<double, double> f, double x, double h, FiniteDifference method, int order, ref int evals)
    {
        return method switch
        {
            FiniteDifference.Central => CentralDifference(f, x, h, order, ref evals),
            FiniteDifference.Forward => ForwardDifference(f, x, h, order, ref evals),
            FiniteDifference.Backward => BackwardDifference(f, x, h, order, ref evals),
            FiniteDifference.FivePoint => FivePointDifference(f, x, h, ref evals),
            FiniteDifference.SevenPoint => SevenPointDifference(f, x, h, ref evals),
            _ => throw new ArgumentException("Unknown method")
        };
    }

    private static double CentralDifference(Func<double, double> f, double x, double h, int order, ref int evals)
    {
        return order switch
        {
            1 => (f(x + h) - f(x - h)) / (2 * h),
            2 => (f(x - 2 * h) - 8 * f(x - h) + 8 * f(x + h) - f(x + 2 * h)) / (12 * h),
            3 => (-f(x - 3 * h) + 9 * f(x - 2 * h) - 45 * f(x - h) + 45 * f(x + h) - 9 * f(x + 2 * h) + f(x + 3 * h)) / (60 * h),
            4 => (f(x - 4 * h) - 12 * f(x - 3 * h) + 54 * f(x - 2 * h) - 204 * f(x - h) + 204 * f(x + h) - 54 * f(x + 2 * h) + 12 * f(x + 3 * h) - f(x + 4 * h)) / (480 * h),
            _ => throw new ArgumentException("Order not supported for central difference")
        };
    }

    private static double ForwardDifference(Func<double, double> f, double x, double h, int order, ref int evals)
    {
        return order switch
        {
            1 => (f(x + h) - f(x)) / h,
            2 => (-3 * f(x) + 4 * f(x + h) - f(x + 2 * h)) / (2 * h),
            3 => (2 * f(x) - 9 * f(x + h) + 18 * f(x + 2 * h) - 11 * f(x + 3 * h)) / (6 * h),
            4 => (-3 * f(x) - 20 * f(x + h) + 90 * f(x + 2 * h) - 120 * f(x + 3 * h) + 65 * f(x + 4 * h) - 12 * f(x + 5 * h)) / (60 * h),
            _ => throw new ArgumentException("Order not supported for forward difference")
        };
    }

    private static double BackwardDifference(Func<double, double> f, double x, double h, int order, ref int evals)
    {
        return order switch
        {
            1 => (f(x) - f(x - h)) / h,
            2 => (3 * f(x) - 4 * f(x - h) + f(x - 2 * h)) / (2 * h),
            3 => (11 * f(x) - 18 * f(x - h) + 9 * f(x - 2 * h) - 2 * f(x - 3 * h)) / (6 * h),
            4 => (12 * f(x - 5 * h) - 65 * f(x - 4 * h) + 120 * f(x - 3 * h) - 90 * f(x - 2 * h) + 20 * f(x - h) + 3 * f(x)) / (60 * h),
            _ => throw new ArgumentException("Order not supported for backward difference")
        };
    }

    private static double FivePointDifference(Func<double, double> f, double x, double h, ref int evals)
    {
        return (f(x - 2 * h) - 8 * f(x - h) + 8 * f(x + h) - f(x + 2 * h)) / (12 * h);
    }

    private static double SevenPointDifference(Func<double, double> f, double x, double h, ref int evals)
    {
        return (-f(x - 3 * h) + 9 * f(x - 2 * h) - 45 * f(x - h) + 45 * f(x + h) - 9 * f(x + 2 * h) + f(x + 3 * h)) / (60 * h);
    }

    private static double EstimateError(Func<double, double> f, double x, double h, FiniteDifference method, int order, double value, ref int evals)
    {
        double h2 = h / 2;
        int dummy = 0;
        double value2 = ComputeDerivative(f, x, h2, method, order, ref dummy);
        return System.Math.Abs(value2 - value);
    }

    public static Vector Gradient(Func<Vector, double> f, Vector x, DerivativeOptions? options = null)
    {
        options ??= DerivativeOptions.Default;
        int n = x.Size;
        var grad = new double[n];
        double h = options.StepSize;

        for (int i = 0; i < n; i++)
        {
            var xPlus = x.ToArray();
            var xMinus = x.ToArray();
            xPlus[i] += h;
            xMinus[i] -= h;

            double fp = f(new Vector(xPlus));
            double fm = f(new Vector(xMinus));
            grad[i] = (fp - fm) / (2 * h);
        }

        return new Vector(grad.ToImmutableArray());
    }

    public static Matrix Jacobian(Func<Vector, Vector> f, Vector x, DerivativeOptions? options = null)
    {
        options ??= DerivativeOptions.Default;
        int n = x.Size;
        var fx = f(x);
        int m = fx.Size;
        var jacData = new double[m][];

        for (int j = 0; j < m; j++)
        {
            jacData[j] = new double[n];
            var xPlus = x.ToArray();
            var xMinus = x.ToArray();
            double h = options.StepSize;

            for (int i = 0; i < n; i++)
            {
                xPlus[i] = x[i] + h;
                xMinus[i] = x[i] - h;

                double fp = f(new Vector(xPlus))[j];
                double fm = f(new Vector(xMinus))[j];
                jacData[j][i] = (fp - fm) / (2 * h);

                xPlus[i] = x[i];
                xMinus[i] = x[i];
            }
        }

        return new Matrix(jacData);
    }

    public static Matrix Hessian(Func<Vector, double> f, Vector x, DerivativeOptions? options = null)
    {
        options ??= DerivativeOptions.Default;
        int n = x.Size;
        var hessData = new double[n][];
        double h = options.StepSize;

        for (int i = 0; i < n; i++)
        {
            hessData[i] = new double[n];
            var xPlus = x.ToArray();
            var xMinus = x.ToArray();
            var xPlusPlus = x.ToArray();
            var xMinusMinus = x.ToArray();

            for (int j = 0; j < n; j++)
            {
                xPlus[i] = x[i] + h;
                xMinus[i] = x[i] - h;
                xPlusPlus[j] = x[j] + h;
                xMinusMinus[j] = x[j] - h;

                double fpp = f(new Vector(xPlusPlus));
                double fpm = f(new Vector(new double[] { xPlus[i], xMinus[j] }));
                double fmp = f(new Vector(new double[] { xMinus[i], xPlus[j] }));
                double fmm = f(new Vector(xMinusMinus));

                hessData[i][j] = (fpp - fpm - fmp + fmm) / (4 * h * h);

                xPlus[i] = x[i];
                xMinus[i] = x[i];
                xPlusPlus[j] = x[j];
                xMinusMinus[j] = x[j];
            }
        }

        return new Matrix(hessData);
    }

    public static double PartialDerivative(Func<Vector, double> f, Vector x, int varIndex, DerivativeOptions? options = null)
    {
        options ??= DerivativeOptions.Default;
        double h = options.StepSize;

        var xPlus = x.ToArray();
        var xMinus = x.ToArray();
        xPlus[varIndex] += h;
        xMinus[varIndex] -= h;

        double fp = f(new Vector(xPlus));
        double fm = f(new Vector(xMinus));

        return (fp - fm) / (2 * h);
    }
}