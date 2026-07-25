namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed class GradientDescent : IOptimizer
{
    private readonly double _momentum;
    private readonly bool _nesterov;

    public GradientDescent(double momentum = 0.0, bool nesterov = false)
    {
        _momentum = System.Math.Clamp(momentum, 0.0, 1.0);
        _nesterov = nesterov;
    }

    public OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null)
    {
        options ??= OptimizationOptions.Default;
        Vector x = initialGuess;
        Vector velocity = new Vector(new double[x.Size]);
        int iterations = 0;
        int functionEvaluations = 0;
        int gradientEvaluations = 0;
        var history = ImmutableArray.CreateBuilder<double>();
        double fCurrent = f(x);
        functionEvaluations++;

        if (options.TrackHistory)
            history.Add(fCurrent);

        for (int iter = 0; iter < options.MaxIterations; iter++)
        {
            iterations++;
            Vector gradient = ComputeGradient(f, x, options.StepSize);
            gradientEvaluations++;

            if (HasNaN(gradient) || double.IsNaN(fCurrent) || double.IsInfinity(fCurrent))
                return OptimizationResult.NaNDetectedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            double gradNorm = gradient.Norm();
            if (gradNorm < options.Tolerance)
                return OptimizationResult.ConvergedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector searchDirection = _nesterov
                ? ComputeNesterovDirection(gradient, velocity)
                : gradient.Negate();

            velocity = velocity.Scale(_momentum).Subtract(searchDirection.Scale(options.StepSize));
            Vector xNew = _nesterov ? x.Add(velocity) : x.Add(velocity);

            double alpha = LineSearch.PerformLineSearch(options, f, x => ComputeGradient(f, x, options.StepSize), x, searchDirection, fCurrent, gradient, options.ArmijoC1, options.WolfeC2, options.StepSize);

            if (double.IsNaN(alpha) || double.IsInfinity(alpha))
                return OptimizationResult.LineSearchFailedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            xNew = x.Add(searchDirection.Scale(alpha));
            double fNew = f(xNew);
            functionEvaluations++;

            if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                return OptimizationResult.DivergedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            if (options.TrackHistory)
                history.Add(fNew);

            double stepNorm = xNew.Subtract(x).Norm();
            if (stepNorm < options.Tolerance || System.Math.Abs(fNew - fCurrent) < options.Tolerance)
                return OptimizationResult.ConvergedResult(xNew, fNew, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            x = xNew;
            fCurrent = fNew;
        }

        return OptimizationResult.MaxIterationsResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());
    }

    public OptimizationResult OptimizeConstrained(Func<Vector, double> f, Vector initialGuess, ImmutableArray<Constraint> constraints, OptimizationOptions? options = null)
    {
        var penaltyFunc = CreatePenaltyFunction(f, constraints, 1e4);
        return Optimize(penaltyFunc, initialGuess, options);
    }

    private static Vector ComputeGradient(Func<Vector, double> f, Vector x, double h)
    {
        int n = x.Size;
        var grad = new double[n];
        double f0 = f(x);

        for (int i = 0; i < n; i++)
        {
            var xPlus = x.ToArray();
            xPlus[i] += h;
            double fPlus = f(new Vector(xPlus));
            grad[i] = (fPlus - f0) / h;
        }

        return new Vector(grad);
    }

    private static Vector ComputeNesterovDirection(Vector gradient, Vector velocity)
    {
        return velocity.Add(gradient).Negate();
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

