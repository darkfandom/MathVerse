namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum ConjugateGradientMethod
{
    FletcherReeves,
    PolakRibiere,
    HestenesStiefel,
    DaiYuan
}

public sealed class ConjugateGradient : IOptimizer
{
    private readonly ConjugateGradientMethod _method;
    private readonly bool _restartEnabled;
    private readonly int _restartIterations;

    public ConjugateGradient(ConjugateGradientMethod method = ConjugateGradientMethod.PolakRibiere, bool restartEnabled = true, int restartIterations = 100)
    {
        _method = method;
        _restartEnabled = restartEnabled;
        _restartIterations = restartIterations;
    }

    public OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null)
    {
        options ??= OptimizationOptions.Default;
        Vector x = initialGuess;
        int iterations = 0;
        int functionEvaluations = 0;
        int gradientEvaluations = 0;
        var history = ImmutableArray.CreateBuilder<double>();

        double fCurrent = f(x);
        functionEvaluations++;

        if (options.TrackHistory)
            history.Add(fCurrent);

        Vector gradient = ComputeGradient(f, x, options.StepSize);
        gradientEvaluations++;
        Vector searchDirection = gradient.Negate();
        double prevGradNormSq = gradient.Dot(gradient);

        for (int iter = 0; iter < options.MaxIterations; iter++)
        {
            iterations++;

            if (HasNaN(gradient) || double.IsNaN(fCurrent) || double.IsInfinity(fCurrent))
                return OptimizationResult.NaNDetectedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            double gradNorm = gradient.Norm();
            if (gradNorm < options.Tolerance)
                return OptimizationResult.ConvergedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            if (_restartEnabled && iter > 0 && iter % _restartIterations == 0)
            {
                searchDirection = gradient.Negate();
            }

            double alpha = LineSearch.PerformLineSearch(options, f, x => ComputeGradient(f, x, options.StepSize), x, searchDirection, fCurrent, gradient, options.ArmijoC1, options.WolfeC2, options.StepSize);

            if (double.IsNaN(alpha) || double.IsInfinity(alpha))
                return OptimizationResult.LineSearchFailedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector xNew = x.Add(searchDirection.Scale(alpha));
            double fNew = f(xNew);
            functionEvaluations++;

            if (double.IsNaN(fNew) || double.IsInfinity(fNew))
                return OptimizationResult.DivergedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            if (options.TrackHistory)
                history.Add(fNew);

            Vector gradientNew = ComputeGradient(f, xNew, options.StepSize);
            gradientEvaluations++;

            if (HasNaN(gradientNew))
                return OptimizationResult.NaNDetectedResult(xNew, fNew, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            double beta = ComputeBeta(gradient, gradientNew, searchDirection, _method);
            searchDirection = gradientNew.Negate().Add(searchDirection.Scale(beta));

            double stepNorm = xNew.Subtract(x).Norm();
            if (stepNorm < options.Tolerance || System.Math.Abs(fNew - fCurrent) < options.Tolerance)
                return OptimizationResult.ConvergedResult(xNew, fNew, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            x = xNew;
            fCurrent = fNew;
            gradient = gradientNew;
            prevGradNormSq = gradient.Dot(gradient);
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

    private static double ComputeBeta(Vector gOld, Vector gNew, Vector dOld, ConjugateGradientMethod method)
    {
        double gNewDotGNew = gNew.Dot(gNew);
        double gOldDotGOld = gOld.Dot(gOld);
        double yDotGNew = gNew.Subtract(gOld).Dot(gNew);
        double yDotDOld = gNew.Subtract(gOld).Dot(dOld);

        return method switch
        {
            ConjugateGradientMethod.FletcherReeves => gNewDotGNew / gOldDotGOld,
            ConjugateGradientMethod.PolakRibiere => yDotGNew / gOldDotGOld,
            ConjugateGradientMethod.HestenesStiefel => yDotGNew / yDotDOld,
            ConjugateGradientMethod.DaiYuan => gNewDotGNew / yDotDOld,
            _ => yDotGNew / gOldDotGOld
        };
    }

    private static double PerformLineSearch(OptimizationOptions options, Func<Vector, double> f, Func<Vector, Vector> grad, Vector x, Vector d, double f0, Vector g0, double c1, double c2, double stepSize)
    {
        return options.LineSearch switch
        {
            LineSearchMethod.Backtracking => LineSearch.Backtracking(f, x, d, f0, g0, c1, 0.5, stepSize),
            LineSearchMethod.Armijo => LineSearch.Armijo(f, x, d, f0, g0, c1, 0.5, stepSize),
            LineSearchMethod.Wolfe => LineSearch.Wolfe(f, grad, x, d, f0, g0, c1, c2, stepSize),
            LineSearchMethod.StrongWolfe => LineSearch.StrongWolfe(f, grad, x, d, f0, g0, c1, c2, stepSize),
            _ => LineSearch.Backtracking(f, x, d, f0, g0, c1, 0.5, stepSize)
        };
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

