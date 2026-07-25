namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed class NewtonOptimizer : IOptimizer
{
    private readonly bool _useBFGS;
    private readonly int _bfgsMemory;

    public NewtonOptimizer(bool useBFGS = true, int bfgsMemory = 10)
    {
        _useBFGS = useBFGS;
        _bfgsMemory = System.Math.Max(1, bfgsMemory);
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

            Matrix hessian = ComputeHessian(f, x, options.StepSize);

            if (HasNaN(hessian))
                return OptimizationResult.NaNDetectedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector direction;
            try
            {
                direction = SolveLinearSystem(hessian, gradient.Negate());
            }
            catch
            {
                direction = gradient.Negate();
            }

            double alpha = LineSearch.PerformLineSearch(options, f, x => ComputeGradient(f, x, options.StepSize), x, direction, fCurrent, gradient, options.ArmijoC1, options.WolfeC2, options.StepSize);

            if (double.IsNaN(alpha) || double.IsInfinity(alpha))
                return OptimizationResult.LineSearchFailedResult(x, fCurrent, iterations, functionEvaluations, gradientEvaluations, history.ToImmutable());

            Vector xNew = x.Add(direction.Scale(alpha));
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

    private static Matrix ComputeHessian(Func<Vector, double> f, Vector x, double h)
    {
        int n = x.Size;
        var hess = new double[n, n];
        var grad0 = ComputeGradient(f, x, h);

        for (int j = 0; j < n; j++)
        {
            var xPlus = x.ToArray();
            xPlus[j] += h;
            var gradPlus = ComputeGradient(f, new Vector(xPlus), h);

            for (int i = 0; i < n; i++)
            {
                hess[i, j] = (gradPlus[i] - grad0[i]) / h;
            }
        }

        var values = new double[n * n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                values[i * n + j] = hess[i, j];

        var jagged = new double[n][];
        for (int i = 0; i < n; i++)
        {
            var row = new double[n];
            for (int j = 0; j < n; j++)
                row[j] = values[i * n + j];
            jagged[i] = row;
        }

        return new Matrix(jagged);
    }

    private static Vector SolveLinearSystem(Matrix A, Vector b)
    {
        int n = A.Rows;
        var a = new double[n, n];
        var x = new double[n];
        var bArr = b.ToArray();

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                a[i, j] = A[i, j];

        for (int i = 0; i < n; i++)
            x[i] = bArr[i];

        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
                if (System.Math.Abs(a[k, i]) > System.Math.Abs(a[maxRow, i]))
                    maxRow = k;

            if (maxRow != i)
            {
                for (int k = i; k < n; k++)
                    (a[i, k], a[maxRow, k]) = (a[maxRow, k], a[i, k]);
                (x[i], x[maxRow]) = (x[maxRow], x[i]);
            }

            if (System.Math.Abs(a[i, i]) < 1e-14)
                throw new InvalidOperationException("Singular matrix");

            for (int k = i + 1; k < n; k++)
            {
                double factor = a[k, i] / a[i, i];
                for (int j = i; j < n; j++)
                    a[k, j] -= factor * a[i, j];
                x[k] -= factor * x[i];
            }
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0;
            for (int j = i + 1; j < n; j++)
                sum += a[i, j] * x[j];
            x[i] = (x[i] - sum) / a[i, i];
        }

        return new Vector(x);
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