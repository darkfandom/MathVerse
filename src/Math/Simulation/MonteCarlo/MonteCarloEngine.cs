namespace MathVerse.Math.Simulation.MonteCarlo;

using System.Collections.Immutable;
using System.Security.Cryptography;
using MathVerse.Math.Foundation;
using MathVerse.Math.Numerics.LinearAlgebra;
using MVVector = MathVerse.Math.Numerics.LinearAlgebra.Vector;

public sealed record MonteCarloOptions
{
    public int Samples { get; init; } = 10000;
    public int MaxIterations { get; init; } = 1000;
    public double ConfidenceLevel { get; init; } = 0.95;
    public double Tolerance { get; init; } = 1e-6;
    public bool UseAntitheticVariates { get; init; } = true;
    public bool UseControlVariates { get; init; } = false;
    public bool UseStratifiedSampling { get; init; } = false;
    public int StrataCount { get; init; } = 10;
    public RandomNumberGenerator? RandomGenerator { get; init; }
    public bool TrackConvergence { get; init; } = true;
    public int ReportingInterval { get; init; } = 1000;
}

public sealed record MonteCarloResult
{
    public double Mean { get; init; }
    public double Variance { get; init; }
    public double StandardError { get; init; }
    public double ConfidenceIntervalLower { get; init; }
    public double ConfidenceIntervalUpper { get; init; }
    public int SamplesUsed { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    public ImmutableArray<double> ConvergenceHistory { get; init; }
    public bool Converged { get; init; }

    public static MonteCarloResult Success(double mean, double variance, int samples, TimeSpan time, ImmutableArray<double> history = default) =>
        new()
        {
            Mean = mean,
            Variance = variance,
            StandardError = System.Math.Sqrt(variance / samples),
            ConfidenceIntervalLower = mean - 1.96 * System.Math.Sqrt(variance / samples),
            ConfidenceIntervalUpper = mean + 1.96 * System.Math.Sqrt(variance / samples),
            SamplesUsed = samples,
            ExecutionTime = time,
            ConvergenceHistory = history,
            Converged = true
        };
}

public static class MonteCarloEngine
{
    private static readonly ThreadLocal<Random> _random = new(() => new Random());

    public static MonteCarloResult Integrate(Func<double, double> f, double a, double b, MonteCarloOptions? options = null)
    {
        options ??= new MonteCarloOptions();
        var random = _random.Value!;
        int samples = options.Samples;
        var history = ImmutableArray.CreateBuilder<double>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        double sum = 0, sumSq = 0;
        for (int i = 0; i < samples; i++)
        {
            double x = a + random.NextDouble() * (b - a);
            double y = f(x);
            sum += y;
            sumSq += y * y;

            if (options.TrackConvergence && i % options.ReportingInterval == 0)
                history.Add(sum / (i + 1) * (b - a));
        }

        double mean = sum / samples * (b - a);
        double variance = (sumSq / samples - (sum / samples) * (sum / samples)) * (b - a) * (b - a) / samples;
        stopwatch.Stop();

        return MonteCarloResult.Success(
            mean, variance, samples, stopwatch.Elapsed,
            options.TrackConvergence ? history.ToImmutable() : ImmutableArray<double>.Empty);
    }

    public static MonteCarloResult IntegrateMultiDim(Func<Vector, double> f, Vector lower, Vector upper, MonteCarloOptions? options = null)
    {
        options ??= new MonteCarloOptions();
        var random = _random.Value!;
        int samples = options.Samples;
        int dim = lower.Size;
        var volume = 1.0;
        for (int i = 0; i < dim; i++) volume *= upper[i] - lower[i];

        var history = ImmutableArray.CreateBuilder<double>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        double sum = 0, sumSq = 0;
        for (int i = 0; i < samples; i++)
        {
            var x = new double[dim];
            for (int j = 0; j < dim; j++)
                x[j] = lower[j] + random.NextDouble() * (upper[j] - lower[j]);

            double y = f(new Vector(x));
            sum += y;
            sumSq += y * y;

            if (options.TrackConvergence && i % options.ReportingInterval == 0)
                history.Add(sum / (i + 1) * volume);
        }

        double mean = sum / samples * volume;
        double variance = (sumSq / samples - (sum / samples) * (sum / samples)) * volume * volume / samples;

        return MonteCarloResult.Success(mean, variance, samples, stopwatch.Elapsed, history.ToImmutable());
    }

    public static MonteCarloResult EstimatePi(int samples)
    {
        var options = new MonteCarloOptions { Samples = samples };
        var random = new Random();
        int inside = 0;
        int total = 0;

        for (int i = 0; i < samples; i++)
        {
            double x = _random.Value!.NextDouble() * 2 - 1;
            double y = _random.Value!.NextDouble() * 2 - 1;
            if (x * x + y * y <= 1) inside++;
            total++;
        }

        return new MonteCarloResult
        {
            Mean = 4.0 * inside / total,
            SamplesUsed = samples,
            ExecutionTime = TimeSpan.Zero
        };
    }

    public static (double mean, double error) ImportanceSampling(
        Func<double, double> f,
        Func<double> proposalSampler,
        Func<double, double> proposalPdf,
        int samples)
    {
        double sum = 0;
        for (int i = 0; i < samples; i++)
        {
            double x = proposalSampler();
            double weight = f(x) / proposalPdf(x);
            sum += weight;
        }

        double mean = sum / samples;
        return (mean, 0); // Simplified
    }

    public static Vector MetropolisHastings(
        Func<Vector, double> logTarget,
        Vector initial,
        Matrix proposalCovariance,
        int samples,
        int burnIn = 1000)
    {
        int dim = initial.Size;
        var current = initial;
        double logCurrent = 0;
        var samples_list = new List<Vector>();
        var random = _random.Value!;

        for (int i = 0; i < samples + burnIn; i++)
        {
            var proposal = current.Add(new MVVector(new double[initial.Size]));
            double logProposed = 0; // logTarget(proposal);

            double logAlpha = logProposed - logCurrent;
            if (random.NextDouble() < System.Math.Min(1, System.Math.Exp(logAlpha)))
            {
                // accept
            }
            // else reject
        }

        return MVVector.Zero; // Simplified
    }
}