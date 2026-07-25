namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed class SimulatedAnnealing : IOptimizer
{
    private readonly double _initialTemperature;
    private readonly double _coolingRate;
    private readonly int _iterationsPerTemp;
    private readonly int _maxIterations;

    public SimulatedAnnealing(double initialTemperature = 1000.0, double coolingRate = 0.95, int iterationsPerTemp = 100, int maxIterations = 10000)
    {
        _initialTemperature = initialTemperature;
        _coolingRate = coolingRate;
        _iterationsPerTemp = iterationsPerTemp;
        _maxIterations = maxIterations;
    }

public OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null)
    {
        options ??= OptimizationOptions.Default;
        Vector x = initialGuess;
        int n = x.Size;
        int iterations = 0;
        int functionEvaluations = 0;
        var history = ImmutableArray.CreateBuilder<double>();

        double fCurrent = f(x);
        functionEvaluations++;

        if (options.TrackHistory)
            history.Add(fCurrent);

        double temperature = _initialTemperature;
        var random = new System.Random();

        for (int iter = 0; iter < _maxIterations; iter++)
        {
            double temperatureRatio = temperature / _initialTemperature;

            for (int i = 0; i < _iterationsPerTemp; i++)
            {
                iterations++;
                if (iterations >= options.MaxIterations)
                    return OptimizationResult.MaxIterationsResult(x, fCurrent, iterations, functionEvaluations, 0, history.ToImmutable());

                var xNew = new double[n];
                for (int j = 0; j < n; j++)
                    xNew[j] = x[j] + (random.NextDouble() - 0.5) * temperature * options.StepSize;

                var xNewVec = new Vector(xNew);
                double fNew = f(xNewVec);
                functionEvaluations++;

                if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                    continue;

                double deltaE = fNew - fCurrent;

                if (deltaE < 0 || random.NextDouble() < System.Math.Exp(-deltaE / temperature))
                {
                    x = xNewVec;
                    fCurrent = fNew;
                }

                if (options.TrackHistory)
                    history.Add(fCurrent);

                if (System.Math.Abs(deltaE) < options.Tolerance)
                    return OptimizationResult.ConvergedResult(x, fCurrent, iterations, functionEvaluations, 0, history.ToImmutable());
            }

            temperature *= _coolingRate;

            if (temperature < 1e-12)
                break;
        }

        return OptimizationResult.MaxIterationsResult(x, fCurrent, iterations, functionEvaluations, 0, history.ToImmutable());
    }

    public OptimizationResult OptimizeConstrained(Func<Vector, double> f, Vector initialGuess, ImmutableArray<Constraint> constraints, OptimizationOptions? options = null)
    {
        var penaltyFunc = CreatePenaltyFunction(f, constraints, 1e4);
        return Optimize(penaltyFunc, initialGuess, options);
    }

    private static Func<Vector, double> CreatePenaltyFunction(Func<Vector, double> f, ImmutableArray<Constraint> constraints, double penalty)
    {
        return x =>
        {
            double penaltyTerm = 0.0;
            foreach (var c in constraints)
            {
                double v = c.Evaluate(x);
                penaltyTerm += c.Type switch
                {
                    ConstraintType.Equality => v * v,
                    ConstraintType.InequalityLess => System.Math.Max(0, v) * System.Math.Max(0, v),
                    ConstraintType.InequalityGreater => System.Math.Max(0, -v) * System.Math.Max(0, -v),
                    _ => 0
                };
            }
            return f(x) + penalty * penaltyTerm;
        };
    }

    private static bool HasNaN(Vector v)
    {
        foreach (double val in v.Values)
            if (double.IsNaN(val) || double.IsInfinity(val))
                return true;
        return false;
    }
}


