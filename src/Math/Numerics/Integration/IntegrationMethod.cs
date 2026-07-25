namespace MathVerse.Math.Numerics.Integration;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum IntegrationMethod
{
    Trapezoidal,
    Simpson,
    AdaptiveSimpson,
    Romberg,
    GaussLegendre,
    GaussKronrod,
    MonteCarlo,
    QuasiMonteCarlo
}

public sealed record IntegrationOptions
{
    public IntegrationMethod Method { get; init; } = IntegrationMethod.AdaptiveSimpson;
    public double Tolerance { get; init; } = 1e-12;
    public int MaxIterations { get; init; } = 1000;
    public int MaxFunctionEvaluations { get; init; } = 10000;
    public bool UseAdaptive { get; init; } = true;
    public int MaxSubdivisions { get; init; } = 1000;
    public double MinStepSize { get; init; } = 1e-15;

    public static IntegrationOptions Default { get; } = new();
}

public sealed record IntegrationResult
{
    public double Value { get; init; }
    public double ErrorEstimate { get; init; }
    public int FunctionEvaluations { get; init; }
    public int Subdivisions { get; init; }
    public bool Converged { get; init; }
    public ImmutableArray<double> SubintervalErrors { get; init; }
    public IntegrationResult(double value, double errorEstimate, int functionEvaluations, int subdivisions, bool converged, ImmutableArray<double> subintervalErrors)
    {
        Value = value;
        ErrorEstimate = errorEstimate;
        FunctionEvaluations = functionEvaluations;
        Subdivisions = subdivisions;
        Converged = converged;
        SubintervalErrors = subintervalErrors;
    }
}

public sealed class Integrator
{
    private static readonly Lazy<Integrator> _instance = new(() => new Integrator());
    public static Integrator Instance => _instance.Value;

    private Integrator() { }

    public IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions? options = null)
    {
        options ??= IntegrationOptions.Default;
        return options.Method switch
        {
            IntegrationMethod.Trapezoidal => TrapezoidalRule.Integrate(f, a, b, options),
            IntegrationMethod.Simpson => SimpsonRule.Integrate(f, a, b, options),
            IntegrationMethod.AdaptiveSimpson => AdaptiveIntegrator.Integrate(f, a, b, options),
            IntegrationMethod.Romberg => RombergIntegrator.Integrate(f, a, b, options),
            IntegrationMethod.GaussLegendre => GaussianQuadrature.Integrate(f, a, b, 10),
            IntegrationMethod.GaussKronrod => GaussKronrodIntegrator.Integrate(f, a, b, options),
            IntegrationMethod.MonteCarlo => MonteCarloIntegrator.Integrate(f, a, b, options),
            IntegrationMethod.QuasiMonteCarlo => QuasiMonteCarloIntegrator.Integrate(f, a, b, options),
            _ => throw new ArgumentException("Unknown integration method")
        };
    }

    public IntegrationResult Integrate(Func<Vector, double> f, Vector a, Vector b, IntegrationOptions? options = null)
    {
        if (a.Size != b.Size) throw new ArgumentException("Bounds must have same dimension");
        return MultiDimensionalIntegrator.Integrate(f, a, b, options ?? IntegrationOptions.Default);
    }

    public IntegrationResult IntegrateMonteCarlo(Func<Vector, double> f, Vector min, Vector max, int samples, IntegrationOptions? options = null)
    {
        return MonteCarloIntegrator.IntegrateMultiDimensional(f, min, max, samples, options);
    }
}

public static class TrapezoidalRule
{
    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        int n = options.MaxIterations;
        double h = (b - a) / n;
        double sum = 0.5 * (f(a) + f(b));
        int evals = 2;

        for (int i = 1; i < n; i++)
        {
            sum += f(a + i * h);
            evals++;
        }

        double value = sum * h;
        double error = System.Math.Abs(value) * 1e-10;

        return new IntegrationResult(value, error, evals, 1, true, ImmutableArray<double>.Empty);
    }
}

public static class SimpsonRule
{
    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        int n = options.MaxIterations;
        if (n % 2 == 1) n++;
        double h = (b - a) / n;
        double sum = f(a) + f(b);
        int evals = 2;

        for (int i = 1; i < n; i += 2)
        {
            sum += 4 * f(a + i * h);
            evals++;
        }
        for (int i = 2; i < n; i += 2)
        {
            sum += 2 * f(a + i * h);
            evals++;
        }

        double value = sum * h / 3;
        double error = System.Math.Abs(value) * 1e-12;

        return new IntegrationResult(value, error, evals, 1, true, ImmutableArray<double>.Empty);
    }
}

public static class AdaptiveIntegrator
{
    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        var errors = ImmutableArray.CreateBuilder<double>();
        int evals = 0;
        int subdivs = 0;

        double result = AdaptiveSimpsonRecursive(f, a, b, options.Tolerance, options.MaxSubdivisions, 0, ref evals, ref subdivs, errors);

        return new IntegrationResult(result, 0, evals, subdivs, true, errors.ToImmutable());
    }

    private static double AdaptiveSimpsonRecursive(
        Func<double, double> f, double a, double b, double tol, int maxSubdivs, int depth,
        ref int evals, ref int subdivs, ImmutableArray<double>.Builder errors)
    {
        double c = (a + b) / 2;
        double fa = f(a); evals++;
        double fb = f(b); evals++;
        double fc = f(c); evals++;

        double S = (fa + 4 * fc + fb) * (b - a) / 6;

        double d = (a + c) / 2;
        double e = (c + b) / 2;
        double fd = f(d); evals++;
        double fe = f(e); evals++;

        double Sleft = (fa + 4 * fd + fc) * (c - a) / 6;
        double Sright = (fc + 4 * fe + fb) * (b - c) / 6;
        double S2 = Sleft + Sright;

        double error = System.Math.Abs(S2 - S);
        errors.Add(error);

        if (depth >= maxSubdivs || error <= 15 * tol)
        {
            subdivs++;
            return S2 + (S2 - S) / 15;
        }

        double left = AdaptiveSimpsonRecursive(f, a, c, tol / 2, maxSubdivs, depth + 1, ref evals, ref subdivs, errors);
        double right = AdaptiveSimpsonRecursive(f, c, b, tol / 2, maxSubdivs, depth + 1, ref evals, ref subdivs, errors);

        return left + right;
    }
}

public static class RombergIntegrator
{
    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        int maxIter = System.Math.Min(options.MaxIterations, 20);
        var R = new double[maxIter, maxIter];
        int evals = 0;

        double h = b - a;
        R[0, 0] = (f(a) + f(b)) * h * 0.5;
        evals += 2;

        for (int i = 1; i < maxIter; i++)
        {
            h *= 0.5;
            double sum = 0;
            for (int k = 1; k <= (1 << (i - 1)); k++)
            {
                sum += f(a + (k - 0.5) * 2 * h);
                evals++;
            }
            R[i, 0] = 0.5 * R[i - 1, 0] + h * sum;

            for (int j = 1; j <= i; j++)
            {
                double factor = System.Math.Pow(4, j);
                R[i, j] = (factor * R[i, j - 1] - R[i - 1, j - 1]) / (factor - 1);
            }

            if (System.Math.Abs(R[i, i] - R[i - 1, i - 1]) < options.Tolerance)
            {
                return new IntegrationResult(R[i, i], System.Math.Abs(R[i, i] - R[i - 1, i - 1]), evals, i, true, ImmutableArray<double>.Empty);
            }
        }

        return new IntegrationResult(R[maxIter - 1, maxIter - 1], System.Math.Abs(R[maxIter - 1, maxIter - 1] - R[maxIter - 2, maxIter - 2]), evals, maxIter, false, ImmutableArray<double>.Empty);
    }
}

public static class GaussianQuadrature
{
    private static readonly (double[] nodes, double[] weights)[] _gaussData = InitializeGaussData();

    private static (double[] nodes, double[] weights)[] InitializeGaussData()
    {
        var data = new (double[] nodes, double[] weights)[21];

        data[1] = (new[] { 0.0 }, new[] { 2.0 });
        data[2] = (new[] { -1.0 / System.Math.Sqrt(3), 1.0 / System.Math.Sqrt(3) }, new[] { 1.0, 1.0 });
        data[3] = (new[] { -System.Math.Sqrt(3.0 / 5), 0.0, System.Math.Sqrt(3.0 / 5) }, new[] { 5.0 / 9, 8.0 / 9, 5.0 / 9 });
        data[4] = (new[] { -0.861136311594053, -0.339981043584856, 0.339981043584856, 0.861136311594053 },
                   new[] { 0.347854845137454, 0.652145154862546, 0.652145154862546, 0.347854845137454 });
        data[5] = (new[] { -0.906179845938664, -0.538469310105683, 0.0, 0.538469310105683, 0.906179845938664 },
                   new[] { 0.236926885056189, 0.478628670499366, 0.568888888888889, 0.478628670499366, 0.236926885056189 });

        for (int n = 6; n <= 20; n++)
        {
            var (nodes, weights) = ComputeGaussLegendre(n);
            data[n] = (nodes, weights);
        }

        return data;
    }

    private static (double[] nodes, double[] weights) ComputeGaussLegendre(int n)
    {
        var nodes = new double[n];
        var weights = new double[n];
        double eps = 1e-15;
        int m = (n + 1) / 2;

        for (int i = 0; i < m; i++)
        {
            double z = System.Math.Cos(System.Math.PI * (i + 0.75) / (n + 0.5));
            double z1;
            do
            {
                double p1 = 1, p2 = 0;
                for (int j = 1; j <= n; j++)
                {
                    double p3 = p2;
                    p2 = p1;
                    p1 = ((2 * j - 1) * z * p2 - (j - 1) * p3) / j;
                }
                double pp = n * (z * p1 - p2) / (z * z - 1);
                z1 = z;
                z = z1 - p1 / pp;
            } while (System.Math.Abs(z - z1) > eps);

            nodes[i] = -z;
            nodes[n - 1 - i] = z;

            double p1b = 1, p2b = 0;
            for (int j = 1; j <= n; j++)
            {
                double p3 = p2b;
                p2b = p1b;
                p1b = ((2 * j - 1) * z * p2b - (j - 1) * p3) / j;
            }
            double pp2 = n * (z * p1b - p2b) / (z * z - 1);
            double w = 2 / ((1 - z * z) * pp2 * pp2);
            weights[i] = w;
            weights[n - 1 - i] = w;
        }

        return (nodes, weights);
    }

    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, int order)
    {
        if (order < 1 || order > 20) throw new ArgumentException("Order must be between 1 and 20");
        var (nodes, weights) = _gaussData[order];
        double sum = 0;
        double mid = (b + a) / 2;
        double halfLen = (b - a) / 2;

        for (int i = 0; i < order; i++)
        {
            double x = mid + halfLen * nodes[i];
            sum += weights[i] * f(x);
        }

        double value = sum * halfLen;
        return new IntegrationResult(value, 0, order, 1, true, ImmutableArray<double>.Empty);
    }
}

public static class GaussKronrodIntegrator
{
    private static readonly double[] _g7Nodes = { -0.949107912342759, -0.741531185599394, -0.405845151377397, 0.0, 0.405845151377397, 0.741531185599394, 0.949107912342759 };
    private static readonly double[] _g7Weights = { 0.129484966168870, 0.279705391489277, 0.381830050505119, 0.417959183673469, 0.381830050505119, 0.279705391489277, 0.129484966168870 };
    private static readonly double[] _k15Nodes = { -0.991455371120813, -0.949107912342759, -0.864864423359769, -0.741531185599394, -0.586087235467691, -0.405845151377397, -0.207784955007898, 0.0, 0.207784955007898, 0.405845151377397, 0.586087235467691, 0.741531185599394, 0.864864423359769, 0.949107912342759, 0.991455371120813 };
    private static readonly double[] _k15Weights = { 0.022935322010529, 0.063092092629979, 0.104790010322250, 0.140653259715525, 0.169004726639267, 0.190350578064785, 0.204432940075299, 0.209482141084728, 0.204432940075299, 0.190350578064785, 0.169004726639267, 0.140653259715525, 0.104790010322250, 0.063092092629979, 0.022935322010529 };

    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        double mid = (a + b) / 2;
        double halfLen = (b - a) / 2;

        double g7 = 0, k15 = 0;
        int evals = 0;

        for (int i = 0; i < 7; i++)
        {
            double x = mid + halfLen * _g7Nodes[i];
            double fx = f(x);
            evals++;
            g7 += _g7Weights[i] * fx;
        }

        for (int i = 0; i < 15; i++)
        {
            double x = mid + halfLen * _k15Nodes[i];
            double fx = f(x);
            if (i % 2 == 1) evals++;
            k15 += _k15Weights[i] * fx;
        }

        double value = k15 * halfLen;
        double error = System.Math.Abs((k15 - g7) * halfLen);

        return new IntegrationResult(value, error, evals, 1, error < options.Tolerance, ImmutableArray<double>.Empty);
    }
}

public static class MonteCarloIntegrator
{
    private static readonly Random _rng = new();

    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        int samples = options.MaxFunctionEvaluations;
        double sum = 0, sumSq = 0;

        for (int i = 0; i < samples; i++)
        {
            double x = a + _rng.NextDouble() * (b - a);
            double fx = f(x);
            sum += fx;
            sumSq += fx * fx;
        }

        double mean = sum / samples;
        double variance = sumSq / samples - mean * mean;
        double value = mean * (b - a);
        double error = System.Math.Sqrt(variance / samples) * (b - a);

        return new IntegrationResult(value, error, samples, 1, error < options.Tolerance, ImmutableArray<double>.Empty);
    }

    public static IntegrationResult IntegrateMultiDimensional(Func<Vector, double> f, Vector min, Vector max, int samples, IntegrationOptions? options = null)
    {
        int dim = min.Size;
        double volume = 1;
        for (int i = 0; i < dim; i++) volume *= (max[i] - min[i]);

        double sum = 0, sumSq = 0;

        for (int i = 0; i < samples; i++)
        {
            var x = new double[dim];
            for (int j = 0; j < dim; j++)
                x[j] = min[j] + _rng.NextDouble() * (max[j] - min[j]);

            double fx = f(new Vector(x));
            sum += fx;
            sumSq += fx * fx;
        }

        double mean = sum / samples;
        double variance = sumSq / samples - mean * mean;
        double value = mean * volume;
        double error = System.Math.Sqrt(variance / samples) * volume;

        return new IntegrationResult(value, error, samples, 1, true, ImmutableArray<double>.Empty);
    }
}

public static class QuasiMonteCarloIntegrator
{
    public static IntegrationResult Integrate(Func<double, double> f, double a, double b, IntegrationOptions options)
    {
        int samples = options.MaxFunctionEvaluations;
        double sum = 0, sumSq = 0;

        for (int i = 0; i < samples; i++)
        {
            double u = HaltonSequence(i + 1, 2);
            double x = a + u * (b - a);
            double fx = f(x);
            sum += fx;
            sumSq += fx * fx;
        }

        double mean = sum / samples;
        double variance = sumSq / samples - mean * mean;
        double value = mean * (b - a);
        double error = System.Math.Sqrt(variance / samples) * (b - a);

        return new IntegrationResult(value, error, samples, 1, error < options.Tolerance, ImmutableArray<double>.Empty);
    }

    private static double HaltonSequence(int index, int base_)
    {
        double result = 0, f = 1.0 / base_;
        while (index > 0)
        {
            result += (index % base_) * f;
            index /= base_;
            f /= base_;
        }
        return result;
    }
}

public static class MultiDimensionalIntegrator
{
    public static IntegrationResult Integrate(Func<Vector, double> f, Vector a, Vector b, IntegrationOptions options)
    {
        return IntegrateRecursive(f, a, b, options, 0, 0);
    }

    private static IntegrationResult IntegrateRecursive(Func<Vector, double> f, Vector a, Vector b, IntegrationOptions options, int dim, int evals)
    {
        if (dim == a.Size - 1)
        {
            return AdaptiveIntegrator.Integrate(
                x => f(AppendVector(a, dim, x)),
                a[dim], b[dim], options);
        }

        int n = 10;
        double h = (b[dim] - a[dim]) / n;
        double sum = 0;

        for (int i = 0; i <= n; i++)
        {
            double weight = (i == 0 || i == n) ? 0.5 : 1.0;
            var subResult = IntegrateRecursive(f, a, b, options, dim + 1, evals);
            sum += weight * subResult.Value;
            evals += subResult.FunctionEvaluations;
        }

        double value = sum * h;
        return new IntegrationResult(value, 0, evals, 1, true, ImmutableArray<double>.Empty);
    }

    private static Vector AppendVector(Vector baseVec, int dim, double value)
    {
        var arr = baseVec.ToArray();
        arr[dim] = value;
        return new Vector(arr);
    }
}