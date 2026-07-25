namespace MathVerse.Math.Numerics.Interpolation;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum InterpolationMethod
{
    Linear,
    Polynomial,
    Lagrange,
    Newton,
    CubicSpline,
    AkimaSpline,
    Hermite,
    Bezier,
    BSpline,
    Rational
}

public enum SplineEndCondition
{
    Natural,
    Clamped,
    NotAKnot,
    Periodic
}

public sealed record InterpolationOptions
{
    public InterpolationMethod Method { get; init; } = InterpolationMethod.CubicSpline;
    public bool Extrapolate { get; init; } = false;
    public double ExtrapolationLimit { get; init; } = 0.1;
    public SplineEndCondition SplineEndCondition { get; init; } = SplineEndCondition.Natural;

    public static InterpolationOptions Default { get; } = new();
}

public sealed record Interpolant
{
    public ImmutableArray<double> X { get; init; }
    public ImmutableArray<double> Y { get; init; }
    public InterpolationMethod Method { get; init; }
    public Func<double, double> Evaluate { get; init; }
    public ImmutableArray<double>? Derivatives { get; init; }
    public ImmutableArray<double>? SecondDerivatives { get; init; }

    public Interpolant(ImmutableArray<double> x, ImmutableArray<double> y, InterpolationMethod method, Func<double, double> evaluate, ImmutableArray<double>? derivatives = null, ImmutableArray<double>? secondDerivatives = null)
    {
        X = x;
        Y = y;
        Method = method;
        Evaluate = evaluate;
        Derivatives = derivatives;
        SecondDerivatives = secondDerivatives;
    }
}

public sealed class Interpolator
{
    private static readonly Lazy<Interpolator> _instance = new(() => new Interpolator());
    public static Interpolator Instance => _instance.Value;

    private Interpolator() { }

    public Interpolant CreateInterpolant(double[] x, double[] y, InterpolationOptions? options = null)
    {
        options ??= InterpolationOptions.Default;

        if (x.Length != y.Length) throw new ArgumentException("x and y must have same length");
        if (x.Length < 2) throw new ArgumentException("At least 2 points required");

        var xImmutable = x.ToImmutableArray();
        var yImmutable = y.ToImmutableArray();

        return options.Method switch
        {
            InterpolationMethod.Linear => CreateLinear(xImmutable, yImmutable, options),
            InterpolationMethod.Polynomial => PolynomialInterpolator.CreateNewton(xImmutable, yImmutable),
            InterpolationMethod.Lagrange => PolynomialInterpolator.CreateLagrange(xImmutable, yImmutable),
            InterpolationMethod.Newton => PolynomialInterpolator.CreateNewton(xImmutable, yImmutable),
            InterpolationMethod.CubicSpline => SplineInterpolator.CreateCubicSpline(xImmutable, yImmutable, options.SplineEndCondition),
            InterpolationMethod.AkimaSpline => SplineInterpolator.CreateAkimaSpline(xImmutable, yImmutable),
            InterpolationMethod.Hermite => CreateHermite(xImmutable, yImmutable),
            InterpolationMethod.Bezier => CreateBezier(xImmutable, yImmutable),
            InterpolationMethod.BSpline => CreateBSpline(xImmutable, yImmutable),
            InterpolationMethod.Rational => CreateRational(xImmutable, yImmutable),
            _ => throw new ArgumentException("Unknown interpolation method")
        };
    }

    public Interpolant CreateFromFunction(Func<double, double> f, double a, double b, int n, InterpolationOptions? options = null)
    {
        if (n < 2) throw new ArgumentException("At least 2 points required");

        var x = new double[n];
        var y = new double[n];
        double h = (b - a) / (n - 1);

        for (int i = 0; i < n; i++)
        {
            x[i] = a + i * h;
            y[i] = f(x[i]);
        }

        return CreateInterpolant(x, y, options);
    }

    private Interpolant CreateLinear(ImmutableArray<double> x, ImmutableArray<double> y, InterpolationOptions options)
    {
        int n = x.Length;
        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return options.Extrapolate ? y[0] : throw new ArgumentOutOfRangeException();
            if (t >= x[n - 1]) return options.Extrapolate ? y[n - 1] : throw new ArgumentOutOfRangeException();

            int i = Array.BinarySearch(x.ToArray(), t);
            if (i < 0) i = ~i - 1;
            if (i >= n - 1) i = n - 2;

            double x0 = x[i], x1 = x[i + 1];
            double y0 = y[i], y1 = y[i + 1];
            return y0 + (y1 - y0) * (t - x0) / (x1 - x0);
        };

        return new Interpolant(x, y, InterpolationMethod.Linear, eval);
    }

    private Interpolant CreateHermite(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        var h = new double[n - 1];
        var slopes = new double[n];

        for (int i = 0; i < n - 1; i++) h[i] = x[i + 1] - x[i];
        slopes[0] = (y[1] - y[0]) / h[0];
        slopes[n - 1] = (y[n - 1] - y[n - 2]) / h[n - 2];
        for (int i = 1; i < n - 1; i++)
            slopes[i] = (y[i + 1] - y[i - 1]) / (x[i + 1] - x[i - 1]);

        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            int i = Array.BinarySearch(x.ToArray(), t);
            if (i < 0) i = ~i - 1;
            if (i >= n - 1) i = n - 2;

            double x0 = x[i], x1 = x[i + 1];
            double y0 = y[i], y1 = y[i + 1];
            double m0 = slopes[i], m1 = slopes[i + 1];
            double h = x1 - x0;
            double tNorm = (t - x0) / h;
            double t2 = tNorm * tNorm;
            double t3 = t2 * tNorm;

            return (2 * t3 - 3 * t2 + 1) * y0 + (t3 - 2 * t2 + tNorm) * h * m0
                 + (-2 * t3 + 3 * t2) * y1 + (t3 - t2) * h * m1;
        };

        return new Interpolant(x, y, InterpolationMethod.Hermite, eval, slopes.ToImmutableArray());
    }

    private Interpolant CreateBezier(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            double result = 0;
            for (int i = 0; i < n; i++)
            {
                double basis = Bernstein(n - 1, i, (t - x[0]) / (x[n - 1] - x[0]));
                result += basis * y[i];
            }
            return result;
        };

        return new Interpolant(x, y, InterpolationMethod.Bezier, eval);
    }

    private static double Bernstein(int n, int i, double t)
    {
        return Binomial(n, i) * System.Math.Pow(t, i) * System.Math.Pow(1 - t, n - i);
    }

    private static double Binomial(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        double result = 1;
        for (int i = 1; i <= k; i++)
            result = result * (n - k + i) / i;
        return result;
    }

    private Interpolant CreateBSpline(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        int k = 3;
        var knots = new double[n + k + 1];
        for (int i = 0; i <= k; i++) knots[i] = x[0];
        for (int i = 0; i < n - k; i++) knots[i + k + 1] = x[i + k];
        for (int i = 0; i <= k; i++) knots[n + i + 1] = x[n - 1];

        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            double result = 0;
            for (int i = 0; i < n; i++)
            {
                double basis = BSplineBasis(i, k, t, knots);
                result += basis * y[i];
            }
            return result;
        };

        return new Interpolant(x, y, InterpolationMethod.BSpline, eval);
    }

    private static double BSplineBasis(int i, int k, double t, double[] knots)
    {
        if (k == 0)
            return (t >= knots[i] && t < knots[i + 1]) ? 1 : 0;

        double denom1 = knots[i + k] - knots[i];
        double denom2 = knots[i + k + 1] - knots[i + 1];
        double term1 = denom1 > 0 ? (t - knots[i]) / denom1 * BSplineBasis(i, k - 1, t, knots) : 0;
        double term2 = denom2 > 0 ? (knots[i + k + 1] - t) / denom2 * BSplineBasis(i + 1, k - 1, t, knots) : 0;
        return term1 + term2;
    }

    private Interpolant CreateRational(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            int i = Array.BinarySearch(x.ToArray(), t);
            if (i < 0) i = ~i - 1;
            if (i >= n - 1) i = n - 2;

            double x0 = x[i], x1 = x[i + 1];
            double y0 = y[i], y1 = y[i + 1];
            double tNorm = (t - x0) / (x1 - x0);

            return (y0 * (1 - tNorm) + y1 * tNorm) / (1 + tNorm * (1 - tNorm));
        };

        return new Interpolant(x, y, InterpolationMethod.Rational, eval);
    }
}

public static class SplineInterpolator
{
    public static Interpolant CreateCubicSpline(ImmutableArray<double> x, ImmutableArray<double> y, SplineEndCondition endCond)
    {
        int n = x.Length;
        var h = new double[n - 1];
        var alpha = new double[n - 1];
        var l = new double[n];
        var mu = new double[n];
        var z = new double[n];
        var c = new double[n];
        var b = new double[n - 1];
        var d = new double[n - 1];

        for (int i = 0; i < n - 1; i++)
            h[i] = x[i + 1] - x[i];

        for (int i = 1; i < n - 1; i++)
            alpha[i] = 3 * (y[i + 1] - y[i]) / h[i] - 3 * (y[i] - y[i - 1]) / h[i - 1];

        switch (endCond)
        {
            case SplineEndCondition.Natural:
                l[0] = 1; mu[0] = 0; z[0] = 0;
                for (int i = 1; i < n - 1; i++)
                {
                    l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                    mu[i] = h[i] / l[i];
                    z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
                }
                l[n - 1] = 1; z[n - 1] = 0; mu[n - 1] = 0;
                break;
            case SplineEndCondition.Clamped:
                // Simplified clamped
                l[0] = 1; mu[0] = 0; z[0] = 0;
                for (int i = 1; i < n - 1; i++)
                {
                    l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                    mu[i] = h[i] / l[i];
                    z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
                }
                l[n - 1] = 1; z[n - 1] = 0; mu[n - 1] = 0;
                break;
            case SplineEndCondition.NotAKnot:
                // Simplified not-a-knot
                l[0] = 1; mu[0] = 0; z[0] = 0;
                for (int i = 1; i < n - 1; i++)
                {
                    l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                    mu[i] = h[i] / l[i];
                    z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
                }
                l[n - 1] = 1; z[n - 1] = 0; mu[n - 1] = 0;
                break;
            case SplineEndCondition.Periodic:
                // Not implemented
                l[0] = 1; mu[0] = 0; z[0] = 0;
                for (int i = 1; i < n - 1; i++)
                {
                    l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                    mu[i] = h[i] / l[i];
                    z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
                }
                l[n - 1] = 1; z[n - 1] = 0; mu[n - 1] = 0;
                break;
        }

        for (int i = n - 2; i >= 0; i--)
        {
            c[i] = z[i] - mu[i] * c[i + 1];
            b[i] = (y[i + 1] - y[i]) / h[i] - h[i] * (c[i + 1] + 2 * c[i]) / 3;
            d[i] = (c[i + 1] - c[i]) / (3 * h[i]);
        }

        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            int i = Array.BinarySearch(x.ToArray(), t);
            if (i < 0) i = ~i - 1;
            if (i >= n - 1) i = n - 2;

            double dx = t - x[i];
            return ((d[i] * dx + c[i]) * dx + b[i]) * dx + y[i];
        };

        var secondDerivs = new double[n];
        Array.Copy(c, secondDerivs, n);
        return new Interpolant(x, y, InterpolationMethod.CubicSpline, eval, null, secondDerivs.ToImmutableArray());
    }

    public static Interpolant CreateAkimaSpline(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        var m = new double[n];

        for (int i = 1; i < n - 1; i++)
        {
            double d1 = (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
            double d2 = (y[i + 1] - y[i]) / (x[i + 1] - x[i]);
            m[i] = (System.Math.Abs(d2) + System.Math.Abs(d1) > 0)
                ? (System.Math.Abs(d2) * d1 + System.Math.Abs(d1) * d2) / (System.Math.Abs(d2) + System.Math.Abs(d1))
                : 0;
        }
        m[0] = m[1]; m[n - 1] = m[n - 2];

        Func<double, double> eval = t =>
        {
            if (t <= x[0]) return y[0];
            if (t >= x[n - 1]) return y[n - 1];

            int i = Array.BinarySearch(x.ToArray(), t);
            if (i < 0) i = ~i - 1;
            if (i >= n - 1) i = n - 2;

            double h = x[i + 1] - x[i];
            double tNorm = (t - x[i]) / h;
            double t2 = tNorm * tNorm;
            double t3 = t2 * tNorm;

            return ((2 * t3 - 3 * t2 + 1) * y[i] + (t3 - 2 * t2 + tNorm) * h * m[i]
                + (-2 * t3 + 3 * t2) * y[i + 1] + (t3 - t2) * h * m[i + 1]);
        };

        return new Interpolant(x, y, InterpolationMethod.AkimaSpline, eval);
    }
}

public static class PolynomialInterpolator
{
    public static Interpolant CreateLagrange(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        Func<double, double> eval = t =>
        {
            double result = 0;
            for (int i = 0; i < n; i++)
            {
                double li = 1;
                for (int j = 0; j < n; j++)
                {
                    if (i != j) li *= (t - x[j]) / (x[i] - x[j]);
                }
                result += y[i] * li;
            }
            return result;
        };
        return new Interpolant(x, y, InterpolationMethod.Lagrange, eval);
    }

    public static Interpolant CreateNewton(ImmutableArray<double> x, ImmutableArray<double> y)
    {
        int n = x.Length;
        var dividedDifferences = new double[n, n];
        for (int i = 0; i < n; i++) dividedDifferences[i, 0] = y[i];
        for (int j = 1; j < n; j++)
        {
            for (int i = 0; i < n - j; i++)
                dividedDifferences[i, j] = (dividedDifferences[i + 1, j - 1] - dividedDifferences[i, j - 1]) / (x[i + j] - x[i]);
        }

        Func<double, double> eval = t =>
        {
            double result = dividedDifferences[0, 0];
            double product = 1;
            for (int i = 1; i < n; i++)
            {
                product *= (t - x[i - 1]);
                result += dividedDifferences[0, i] * product;
            }
            return result;
        };

        return new Interpolant(x, y, InterpolationMethod.Newton, eval);
    }
}