using System.Collections.Immutable;
using System.Numerics;

namespace MathVerse.Math.CAS.Matrix;

public sealed record MatrixFunction
{
    public string Name { get; init; } = string.Empty;
    public Func<MatrixExpression, MatrixExpression> Function { get; init; } = _ => MatrixExpression.Zero(0, 0);

    public static MatrixFunction MatrixExpFunc { get; } = new MatrixFunction
    {
        Name = "MatrixExp",
        Function = ComputeMatrixExp
    };

    public static MatrixFunction MatrixLogFunc { get; } = new MatrixFunction
    {
        Name = "MatrixLog",
        Function = ComputeMatrixLog
    };

    public static MatrixFunction MatrixSqrtFunc { get; } = new MatrixFunction
    {
        Name = "MatrixSqrt",
        Function = ComputeMatrixSqrt
    };

    public static MatrixFunction MatrixPower(double p) => new MatrixFunction
    {
        Name = $"MatrixPower({p})",
        Function = m => ComputeMatrixPower(m, p)
    };

    private static MatrixExpression ComputeMatrixExp(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Matrix exponential requires a square matrix");

        int n = m.RowCount;
        var result = MatrixExpression.Identity(n);
        var term = MatrixExpression.Identity(n);
        int maxTerms = 50;
        double tolerance = 1e-15;

        for (int k = 1; k < maxTerms; k++)
        {
            var scalarDiv = new BinaryExpression(Expression.One, BinaryOperator.Divide, new ConstantExpression(k));
            term = MatrixOperations.ScalarMultiply(scalarDiv, MatrixOperations.Multiply(term, m));

            var norm = MatrixNorm(term);
            if (norm < tolerance) break;

            result = MatrixOperations.Add(result, term);
        }

        return result;
    }

    private static MatrixExpression ComputeMatrixLog(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Matrix logarithm requires a square matrix");

        var (eigenvalues, eigenvectors) = MatrixOperations.Eigen(m);

        var logEigenvalues = eigenvalues.Select(v =>
            new FunctionExpression("Log", [v])
        ).ToImmutableArray();

        var diagLog = MatrixExpression.Zero(eigenvalues.Length, eigenvalues.Length);
        for (int i = 0; i < eigenvalues.Length; i++)
            diagLog = diagLog.WithElement(i, i, logEigenvalues[i]);

        var V = MatrixExpression.FromColumns(eigenvectors);
        var VInv = MatrixOperations.Inverse(V);

        return MatrixOperations.Multiply(
            MatrixOperations.Multiply(V, diagLog),
            VInv
        );
    }

    private static MatrixExpression ComputeMatrixSqrt(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Matrix square root requires a square matrix");

        var (eigenvalues, eigenvectors) = MatrixOperations.Eigen(m);

        var sqrtEigenvalues = eigenvalues.Select(v =>
            new FunctionExpression("Sqrt", [v])
        ).ToImmutableArray();

        var diagSqrt = MatrixExpression.Zero(eigenvalues.Length, eigenvalues.Length);
        for (int i = 0; i < eigenvalues.Length; i++)
            diagSqrt = diagSqrt.WithElement(i, i, sqrtEigenvalues[i]);

        var V = MatrixExpression.FromColumns(eigenvectors);
        var VInv = MatrixOperations.Inverse(V);

        return MatrixOperations.Multiply(
            MatrixOperations.Multiply(V, diagSqrt),
            VInv
        );
    }

    private static MatrixExpression ComputeMatrixPower(MatrixExpression m, double p)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Matrix power requires a square matrix");

        if (p == 0) return MatrixExpression.Identity(m.RowCount);
        if (p == 1) return m;
        if (p == -1) return MatrixOperations.Inverse(m);

        if (p == (int)p && p > 0)
        {
            int n = (int)p;
            var result = MatrixExpression.Identity(m.RowCount);
            for (int i = 0; i < n; i++)
                result = MatrixOperations.Multiply(result, m);
            return result;
        }

        var (eigenvalues, eigenvectors) = MatrixOperations.Eigen(m);

        var powEigenvalues = eigenvalues.Select(v =>
            new BinaryExpression(v, BinaryOperator.Power, new ConstantExpression(p))
        ).ToImmutableArray();

        var diagPow = MatrixExpression.Zero(eigenvalues.Length, eigenvalues.Length);
        for (int i = 0; i < eigenvalues.Length; i++)
            diagPow = diagPow.WithElement(i, i, powEigenvalues[i]);

        var V = MatrixExpression.FromColumns(eigenvectors);
        var VInv = MatrixOperations.Inverse(V);

        return MatrixOperations.Multiply(
            MatrixOperations.Multiply(V, diagPow),
            VInv
        );
    }

    private static double MatrixNorm(MatrixExpression m)
    {
        double sum = 0;
        foreach (var row in m.Rows)
        {
            foreach (var elem in row)
            {
                if (elem is ConstantExpression c)
                    sum += c.Value * c.Value;
            }
        }
        return System.Math.Sqrt(sum);
    }
}

public static class MatrixFunctions
{
    public static readonly MatrixFunction Exp = MatrixFunction.MatrixExpFunc;
    public static readonly MatrixFunction Log = MatrixFunction.MatrixLogFunc;
    public static readonly MatrixFunction Sqrt = MatrixFunction.MatrixSqrtFunc;

    public static MatrixFunction Power(double p) => MatrixFunction.MatrixPower(p);

    public static MatrixExpression Evaluate(MatrixFunction func, MatrixExpression matrix)
    {
        return func.Function(matrix);
    }
}