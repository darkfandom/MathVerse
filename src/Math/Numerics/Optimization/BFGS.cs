namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public enum BFGSType
{
    Full,
    LimitedMemory
}

public sealed class BFGS : IOptimizer
{
    private readonly BFGSType _type;
    private readonly int _memorySize;

    public BFGS(BFGSType type = BFGSType.Full, int memorySize = 10)
    {
        _type = type;
        _memorySize = System.Math.Max(1, memorySize);
    }

    public OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null)
    {
        options ??= OptimizationOptions.Default;
        Vector x = initialGuess;
        int n = x.Size;
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

        if (HasNaN(gradient) || double.IsNaN(fCurrent) || double.IsInfinity(fCurrent))
            return OptimizationResult.NaNDetectedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

        Matrix H = Matrix.Identity(n);

        var sHistory = new Vector[_memorySize];
        var yHistory = new Vector[_memorySize];
        var rhoHistory = new double[_memorySize];
        int historyCount = 0;

        for (int iter = 0; iter < options.MaxIterations; iter++)
        {
            iterations++;

            if (gradient.Norm() < options.Tolerance)
                return OptimizationResult.ConvergedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector searchDirection = _type == BFGSType.LimitedMemory
                ? ComputeLBFGSDirection(gradient, sHistory, yHistory, rhoHistory, historyCount)
                : H.Multiply(gradient).Negate();

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

            Vector s = xNew.Subtract(x);
            double stepNorm = s.Norm();

            Vector gradientNew = ComputeGradient(f, xNew, options.StepSize);
            gradientEvaluations++;

            if (HasNaN(gradientNew))
                return OptimizationResult.NaNDetectedResult(xNew, fNew, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector y = gradientNew.Subtract(gradient);
            double ys = y.Dot(s);

            if (ys > 1e-14)
            {
                if (_type == BFGSType.Full)
                {
                    UpdateBFGSMatrix(ref H, s, y, ys);
                }
                else
                {
                    UpdateLBFGSHistory(s, y, ys, sHistory, yHistory, rhoHistory, ref historyCount);
                }
            }

            if (stepNorm < options.Tolerance || System.Math.Abs(fNew - fCurrent) < options.Tolerance)
                return OptimizationResult.ConvergedResult(xNew, fNew, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            x = xNew;
            fCurrent = fNew;
            gradient = gradientNew;
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

    private static Vector ComputeLBFGSDirection(Vector gradient, Vector[] sHistory, Vector[] yHistory, double[] rhoHistory, int historyCount)
    {
        int n = gradient.Size;
        var q = gradient.ToArray();
        var alpha = new double[historyCount];

        for (int i = historyCount - 1; i >= 0; i--)
        {
            alpha[i] = rhoHistory[i] * sHistory[i].Dot(new Vector(q));
            q = new Vector(q).Subtract(yHistory[i].Scale(alpha[i])).ToArray();
        }

        double gamma = 1.0;
        if (historyCount > 0)
        {
            gamma = sHistory[historyCount - 1].Dot(yHistory[historyCount - 1]) / yHistory[historyCount - 1].Dot(yHistory[historyCount - 1]);
        }

        var r = new Vector(q).Scale(gamma);

        for (int i = 0; i < historyCount; i++)
        {
            double beta = rhoHistory[i] * yHistory[i].Dot(r);
            r = r.Add(sHistory[i].Scale(alpha[i] - beta));
        }

        return r.Negate();
    }

    private static void UpdateBFGSMatrix(ref Matrix H, Vector s, Vector y, double ys)
    {
        int n = s.Size;
        var sArray = s.ToArray();
        var yArray = y.ToArray();

        var Hy = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int j = 0; j < n; j++)
                sum += H[i, j] * yArray[j];
            Hy[i] = sum;
        }

        double yHy = 0;
        for (int i = 0; i < n; i++)
            yHy += yArray[i] * Hy[i];

        double rho = 1.0 / ys;

        var HNew = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sSy = sArray[i] * sArray[j];
                double sHy = sArray[i] * Hy[j];
                double yHs = yArray[i] * sArray[j];

                HNew[i, j] = H[i, j] + (1 + rho * yHy) * sSy * rho - rho * (sHy + yHs);
            }
        }

        H = new Matrix(HNew);
    }

    private static void UpdateLBFGSHistory(Vector s, Vector y, double ys, Vector[] sHistory, Vector[] yHistory, double[] rhoHistory, ref int historyCount)
    {
        int idx = historyCount % sHistory.Length;
        sHistory[idx] = s;
        yHistory[idx] = y;
        rhoHistory[idx] = 1.0 / ys;
        historyCount = System.Math.Min(historyCount + 1, sHistory.Length);
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

    private static bool HasNaN(Matrix m)
    {
        for (int i = 0; i < m.Rows; i++)
            for (int j = 0; j < m.Cols; j++)
                if (double.IsNaN(m[i, j]) || double.IsInfinity(m[i, j]))
                    return true;
        return false;
    }
}