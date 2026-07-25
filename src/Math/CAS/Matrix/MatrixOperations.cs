using System.Collections.Immutable;
using System.Numerics;

namespace MathVerse.Math.CAS.Matrix;

public static class MatrixOperations
{
    public static MatrixExpression Add(MatrixExpression a, MatrixExpression b)
    {
        if (a.RowCount != b.RowCount || a.ColCount != b.ColCount)
            throw new ArgumentException("Matrix dimensions must match for addition");

        var result = new ImmutableArray<Expression>[a.RowCount];
        for (int i = 0; i < a.RowCount; i++)
        {
            var row = new Expression[a.ColCount];
            for (int j = 0; j < a.ColCount; j++)
                row[j] = new BinaryExpression(a[i, j], BinaryOperator.Add, b[i, j]);
            result[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(result.ToImmutableArray());
    }

    public static MatrixExpression Subtract(MatrixExpression a, MatrixExpression b)
    {
        if (a.RowCount != b.RowCount || a.ColCount != b.ColCount)
            throw new ArgumentException("Matrix dimensions must match for subtraction");

        var result = new ImmutableArray<Expression>[a.RowCount];
        for (int i = 0; i < a.RowCount; i++)
        {
            var row = new Expression[a.ColCount];
            for (int j = 0; j < a.ColCount; j++)
                row[j] = new BinaryExpression(a[i, j], BinaryOperator.Subtract, b[i, j]);
            result[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(result.ToImmutableArray());
    }

    public static MatrixExpression Multiply(MatrixExpression a, MatrixExpression b)
    {
        if (a.ColCount != b.RowCount)
            throw new ArgumentException($"Cannot multiply {a.RowCount}x{a.ColCount} matrix with {b.RowCount}x{b.ColCount} matrix");

        var result = new ImmutableArray<Expression>[a.RowCount];
        for (int i = 0; i < a.RowCount; i++)
        {
            var row = new Expression[b.ColCount];
            for (int j = 0; j < b.ColCount; j++)
            {
                Expression sum = Expression.Zero;
                for (int k = 0; k < a.ColCount; k++)
                {
                    var product = new BinaryExpression(a[i, k], BinaryOperator.Multiply, b[k, j]);
                    sum = sum is ConstantExpression { Value: 0 } ? product : new BinaryExpression(sum, BinaryOperator.Add, product);
                }
                row[j] = sum;
            }
            result[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(result.ToImmutableArray());
    }

    public static MatrixExpression ScalarMultiply(Expression scalar, MatrixExpression m)
    {
        var result = new ImmutableArray<Expression>[m.RowCount];
        for (int i = 0; i < m.RowCount; i++)
        {
            var row = new Expression[m.ColCount];
            for (int j = 0; j < m.ColCount; j++)
                row[j] = new BinaryExpression(scalar, BinaryOperator.Multiply, m[i, j]);
            result[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(result.ToImmutableArray());
    }

    public static Expression Determinant(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Determinant requires a square matrix");

        return DeterminantRecursive(m);
    }

    private static Expression DeterminantRecursive(MatrixExpression m)
    {
        int n = m.RowCount;
        if (n == 1) return m[0, 0];
        if (n == 2)
        {
            var a = m[0, 0];
            var b = m[0, 1];
            var c = m[1, 0];
            var d = m[1, 1];
            return new BinaryExpression(
                new BinaryExpression(a, BinaryOperator.Multiply, d),
                BinaryOperator.Subtract,
                new BinaryExpression(b, BinaryOperator.Multiply, c)
            );
        }

        Expression det = Expression.Zero;
        for (int j = 0; j < n; j++)
        {
            var minor = GetMinor(m, 0, j);
            var cofactor = DeterminantRecursive(minor);
            Expression term = new BinaryExpression(m[0, j], BinaryOperator.Multiply, cofactor);
            if (j % 2 == 1)
                term = new UnaryExpression(UnaryOperator.Negate, term);

            det = det is ConstantExpression { Value: 0 } ? term : new BinaryExpression(det, BinaryOperator.Add, term);
        }
        return det;
    }

    private static MatrixExpression GetMinor(MatrixExpression m, int row, int col)
    {
        var result = new ImmutableArray<Expression>[m.RowCount - 1];
        int r = 0;
        for (int i = 0; i < m.RowCount; i++)
        {
            if (i == row) continue;
            var newRow = new Expression[m.ColCount - 1];
            int c = 0;
            for (int j = 0; j < m.ColCount; j++)
            {
                if (j == col) continue;
                newRow[c++] = m[i, j];
            }
            result[r++] = newRow.ToImmutableArray();
        }
        return new MatrixExpression(result.ToImmutableArray());
    }

    public static MatrixExpression Adjugate(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Adjugate requires a square matrix");

        int n = m.RowCount;
        var cofactors = new ImmutableArray<Expression>[n];
        for (int i = 0; i < n; i++)
        {
            var row = new Expression[n];
            for (int j = 0; j < n; j++)
            {
                var minor = GetMinor(m, i, j);
                var cofactor = DeterminantRecursive(minor);
                if ((i + j) % 2 == 1)
                    cofactor = new UnaryExpression(UnaryOperator.Negate, cofactor);
                row[j] = cofactor;
            }
            cofactors[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(cofactors.ToImmutableArray()).Transpose();
    }

    public static MatrixExpression Inverse(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Inverse requires a square matrix");

        var det = Determinant(m);
        var adj = Adjugate(m);

        return ScalarMultiply(
            new BinaryExpression(Expression.One, BinaryOperator.Divide, det),
            adj
        );
    }

    public static Expression Trace(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Trace requires a square matrix");

        Expression trace = Expression.Zero;
        for (int i = 0; i < m.RowCount; i++)
        {
            trace = trace is ConstantExpression { Value: 0 }
                ? m[i, i]
                : new BinaryExpression(trace, BinaryOperator.Add, m[i, i]);
        }
        return trace;
    }

    public static int Rank(MatrixExpression m)
    {
        var (_, _, rank) = GaussianElimination(m);
        return rank;
    }

    private static (MatrixExpression upper, ImmutableArray<int> pivotCols, int rank) GaussianElimination(MatrixExpression m)
    {
        int rows = m.RowCount;
        int cols = m.ColCount;
        var matrix = m.Rows.Select(r => r.ToArray()).ToArray();
        var pivotCols = new List<int>();
        int row = 0;

        for (int col = 0; col < cols && row < rows; col++)
        {
            int pivotRow = FindPivot(matrix, row, col);
            if (pivotRow == -1) continue;

            SwapRows(matrix, row, pivotRow);
            pivotCols.Add(col);

            for (int i = row + 1; i < rows; i++)
            {
                if (IsZero(matrix[i][col])) continue;
                var factor = new BinaryExpression(matrix[i][col], BinaryOperator.Divide, matrix[row][col]);
                for (int j = col; j < cols; j++)
                {
                    var product = new BinaryExpression(factor, BinaryOperator.Multiply, matrix[row][j]);
                    matrix[i][j] = new BinaryExpression(matrix[i][j], BinaryOperator.Subtract, product);
                }
            }
            row++;
        }

        var resultRows = matrix.Select(r => r.ToImmutableArray()).ToImmutableArray();
        return (new MatrixExpression(resultRows), pivotCols.ToImmutableArray(), pivotCols.Count);
    }

    private static int FindPivot(Expression[][] matrix, int startRow, int col)
    {
        for (int i = startRow; i < matrix.Length; i++)
        {
            if (!IsZero(matrix[i][col]))
                return i;
        }
        return -1;
    }

    private static bool IsZero(Expression expr)
    {
        return expr is ConstantExpression { Value: 0 };
    }

    private static void SwapRows(Expression[][] matrix, int r1, int r2)
    {
        (matrix[r1], matrix[r2]) = (matrix[r2], matrix[r1]);
    }

    public static (ImmutableArray<Expression> eigenvalues, ImmutableArray<MatrixExpression> eigenvectors) Eigen(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Eigenvalues/vectors require a square matrix");

        var eigenvalues = Eigenvalues(m);
        var eigenvectors = Eigenvectors(m);
        return (eigenvalues, eigenvectors);
    }

    public static ImmutableArray<Expression> Eigenvalues(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Eigenvalues require a square matrix");

        int n = m.RowCount;
        if (n == 1) return [m[0, 0]];
        if (n == 2) return Eigenvalues2x2(m);
        if (n == 3) return Eigenvalues3x3(m);

        return ComputeEigenvaluesViaCharacteristicPolynomial(m);
    }

    private static ImmutableArray<Expression> Eigenvalues2x2(MatrixExpression m)
    {
        var a = m[0, 0];
        var b = m[0, 1];
        var c = m[1, 0];
        var d = m[1, 1];

        var trace = new BinaryExpression(a, BinaryOperator.Add, d);
        var det = new BinaryExpression(
            new BinaryExpression(a, BinaryOperator.Multiply, d),
            BinaryOperator.Subtract,
            new BinaryExpression(b, BinaryOperator.Multiply, c)
        );

        var traceSquared = new BinaryExpression(trace, BinaryOperator.Power, new ConstantExpression(2));
        var fourDet = new BinaryExpression(new ConstantExpression(4), BinaryOperator.Multiply, det);
        var discriminant = new BinaryExpression(traceSquared, BinaryOperator.Subtract, fourDet);
        var sqrtDisc = new UnaryExpression(UnaryOperator.Sqrt, discriminant);

        var half = new ConstantExpression(0.5);
        var lambda1 = new BinaryExpression(
            new BinaryExpression(trace, BinaryOperator.Add, sqrtDisc),
            BinaryOperator.Multiply,
            half
        );
        var lambda2 = new BinaryExpression(
            new BinaryExpression(trace, BinaryOperator.Subtract, sqrtDisc),
            BinaryOperator.Multiply,
            half
        );

        return [lambda1, lambda2];
    }

    private static ImmutableArray<Expression> Eigenvalues3x3(MatrixExpression m)
    {
        var charPoly = CharacteristicPolynomial(m);
        return SolveCubic(charPoly);
    }

    private static Expression CharacteristicPolynomial(MatrixExpression m)
    {
        int n = m.RowCount;
        var lambda = new SymbolExpression("λ");
        var lambdaI = MatrixExpression.Identity(n);
        var lambdaIMatrix = ScalarMultiply(lambda, lambdaI);
        var diff = Subtract(m, lambdaIMatrix);
        return Determinant(diff);
    }

    private static ImmutableArray<Expression> SolveCubic(Expression poly)
    {
        return [new SymbolExpression("λ₁"), new SymbolExpression("λ₂"), new SymbolExpression("λ₃")];
    }

    private static ImmutableArray<Expression> ComputeEigenvaluesViaCharacteristicPolynomial(MatrixExpression m)
    {
        int n = m.RowCount;
        var eigenvalues = new Expression[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = new SymbolExpression($"λ{i + 1}");
        return eigenvalues.ToImmutableArray();
    }

    public static ImmutableArray<MatrixExpression> Eigenvectors(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("Eigenvectors require a square matrix");

        var eigenvalues = Eigenvalues(m);
        var eigenvectors = new List<MatrixExpression>();

        foreach (var eigenvalue in eigenvalues)
        {
            var eigVec = ComputeEigenvector(m, eigenvalue);
            if (eigVec != null)
                eigenvectors.Add(eigVec);
        }

        return eigenvectors.ToImmutableArray();
    }

    private static MatrixExpression? ComputeEigenvector(MatrixExpression m, Expression eigenvalue)
    {
        int n = m.RowCount;
        var lambdaI = ScalarMultiply(eigenvalue, MatrixExpression.Identity(n));
        var diff = Subtract(m, lambdaI);

        var nullSpace = NullSpace(diff);
        if (nullSpace.Length > 0)
        {
            var vec = nullSpace[0];
            var colVec = new ImmutableArray<Expression>[n];
            for (int i = 0; i < n; i++)
                colVec[i] = [vec[i, 0]];
            return new MatrixExpression(colVec.ToImmutableArray());
        }
        return null;
    }

    public static (MatrixExpression L, MatrixExpression U, ImmutableArray<int> permutation) LUDecomposition(MatrixExpression m)
    {
        if (m.RowCount != m.ColCount)
            throw new ArgumentException("LU decomposition requires a square matrix");

        int n = m.RowCount;
        var L = MatrixExpression.Identity(n).Rows.Select(r => r.ToArray()).ToArray();
        var U = m.Rows.Select(r => r.ToArray()).ToArray();
        var permutation = Enumerable.Range(0, n).ToArray();

        for (int k = 0; k < n; k++)
        {
            int maxRow = k;
            for (int i = k + 1; i < n; i++)
            {
                if (CompareAbs(U[i][k], U[maxRow][k]) > 0)
                    maxRow = i;
            }

            if (maxRow != k)
            {
                SwapRows(U, k, maxRow);
                SwapRows(L, k, maxRow);
                (permutation[k], permutation[maxRow]) = (permutation[maxRow], permutation[k]);
            }

            for (int i = k + 1; i < n; i++)
            {
                if (IsZero(U[k][k])) continue;
                L[i][k] = new BinaryExpression(U[i][k], BinaryOperator.Divide, U[k][k]);
                for (int j = k; j < n; j++)
                {
                    var product = new BinaryExpression(L[i][k], BinaryOperator.Multiply, U[k][j]);
                    U[i][j] = new BinaryExpression(U[i][j], BinaryOperator.Subtract, product);
                }
            }
        }

        return (
            new MatrixExpression(L.Select(r => r.ToImmutableArray()).ToImmutableArray()),
            new MatrixExpression(U.Select(r => r.ToImmutableArray()).ToImmutableArray()),
            permutation.ToImmutableArray()
        );
    }

    private static int CompareAbs(Expression a, Expression b)
    {
        return 0;
    }

    public static (MatrixExpression Q, MatrixExpression R) QRDecomposition(MatrixExpression m)
    {
        int rows = m.RowCount;
        int cols = m.ColCount;

        var Q = MatrixExpression.Identity(rows).Rows.Select(r => r.ToArray()).ToArray();
        var R = m.Rows.Select(r => r.ToArray()).ToArray();

        for (int j = 0; j < System.Math.Min(rows, cols); j++)
        {
            var v = new Expression[rows];
            for (int i = 0; i < rows; i++)
                v[i] = R[i][j];

            double norm = 0;
            for (int i = j; i < rows; i++)
            {
                if (v[i] is ConstantExpression c)
                    norm += c.Value * c.Value;
            }
            norm = System.Math.Sqrt(norm);

            if (norm == 0) continue;

            for (int i = j; i < rows; i++)
            {
                if (v[i] is ConstantExpression c)
                    v[i] = new ConstantExpression(c.Value / norm);
            }

            v[j] = new BinaryExpression(v[j], BinaryOperator.Add, Expression.One);

            for (int k = j; k < cols; k++)
            {
                Expression dot = Expression.Zero;
                for (int i = j; i < rows; i++)
                {
                    var product = new BinaryExpression(v[i], BinaryOperator.Multiply, R[i][k]);
                    dot = dot is ConstantExpression { Value: 0 } ? product : new BinaryExpression(dot, BinaryOperator.Add, product);
                }

                var twoDot = new BinaryExpression(new ConstantExpression(2), BinaryOperator.Multiply, dot);
                for (int i = j; i < rows; i++)
                {
                    var product = new BinaryExpression(twoDot, BinaryOperator.Multiply, v[i]);
                    R[i][k] = new BinaryExpression(R[i][k], BinaryOperator.Subtract, product);
                }
            }
        }

        return (
            new MatrixExpression(Q.Select(r => r.ToImmutableArray()).ToImmutableArray()),
            new MatrixExpression(R.Select(r => r.ToImmutableArray()).ToImmutableArray())
        );
    }

    public static (MatrixExpression U, MatrixExpression S, MatrixExpression V) SVD(MatrixExpression m)
    {
        int rows = m.RowCount;
        int cols = m.ColCount;

        var U = MatrixExpression.Identity(rows);
        var S = MatrixExpression.Zero(rows, cols);
        var V = MatrixExpression.Identity(cols);

        return (U, S, V);
    }

    public static ImmutableArray<MatrixExpression> NullSpace(MatrixExpression m)
    {
        var (rref, pivotCols, rank) = GaussianElimination(m);
        int cols = m.ColCount;

        if (rank == cols)
            return ImmutableArray<MatrixExpression>.Empty;

        var freeVars = Enumerable.Range(0, cols).Where(i => !pivotCols.Contains(i)).ToArray();
        var nullSpace = new List<MatrixExpression>();

        foreach (var freeVar in freeVars)
        {
            var vec = new Expression[cols];
            vec[freeVar] = Expression.One;

            for (int i = 0; i < rank; i++)
            {
                int pivotCol = pivotCols[i];
                Expression sum = Expression.Zero;
                for (int j = pivotCol + 1; j < cols; j++)
                {
                    if (!IsZero(rref[i, j]))
                    {
                        var product = new BinaryExpression(rref[i, j], BinaryOperator.Multiply, vec[j]);
                        sum = sum is ConstantExpression { Value: 0 } ? product : new BinaryExpression(sum, BinaryOperator.Add, product);
                    }
                }
                vec[pivotCol] = new UnaryExpression(UnaryOperator.Negate, sum);
            }

            var colVec = new ImmutableArray<Expression>[cols];
            for (int i = 0; i < cols; i++)
                colVec[i] = [vec[i]];
            nullSpace.Add(new MatrixExpression(colVec.ToImmutableArray()));
        }

        return nullSpace.ToImmutableArray();
    }

    public static ImmutableArray<MatrixExpression> ColumnSpace(MatrixExpression m)
    {
        var (rref, pivotCols, _) = GaussianElimination(m);
        var basis = new List<MatrixExpression>();

        foreach (int pivotCol in pivotCols)
        {
            var col = new Expression[m.RowCount];
            for (int i = 0; i < m.RowCount; i++)
                col[i] = m[i, pivotCol];

            var colVec = new ImmutableArray<Expression>[m.RowCount];
            for (int i = 0; i < m.RowCount; i++)
                colVec[i] = [col[i]];
            basis.Add(new MatrixExpression(colVec.ToImmutableArray()));
        }

        return basis.ToImmutableArray();
    }

    public static Expression Norm(MatrixExpression m, MatrixNorm norm = MatrixNorm.Frobenius)
    {
        switch (norm)
        {
            case MatrixNorm.Frobenius:
                Expression sum = Expression.Zero;
                for (int i = 0; i < m.RowCount; i++)
                {
                    for (int j = 0; j < m.ColCount; j++)
                    {
                        var conj = new UnaryExpression(UnaryOperator.Conjugate, m[i, j]);
                        var prod = new BinaryExpression(m[i, j], BinaryOperator.Multiply, conj);
                        sum = sum is ConstantExpression { Value: 0 } ? prod : new BinaryExpression(sum, BinaryOperator.Add, prod);
                    }
                }
                return new UnaryExpression(UnaryOperator.Sqrt, sum);

            case MatrixNorm.OneNorm:
                Expression maxColSum = Expression.Zero;
                for (int j = 0; j < m.ColCount; j++)
                {
                    Expression colSum = Expression.Zero;
                    for (int i = 0; i < m.RowCount; i++)
                    {
                        var abs = new UnaryExpression(UnaryOperator.Abs, m[i, j]);
                        colSum = colSum is ConstantExpression { Value: 0 } ? abs : new BinaryExpression(colSum, BinaryOperator.Add, abs);
                    }
                    maxColSum = new FunctionExpression("Max", [maxColSum, colSum]);
                }
                return maxColSum;

            case MatrixNorm.InfinityNorm:
                Expression maxRowSum = Expression.Zero;
                for (int i = 0; i < m.RowCount; i++)
                {
                    Expression rowSum = Expression.Zero;
                    for (int j = 0; j < m.ColCount; j++)
                    {
                        var abs = new UnaryExpression(UnaryOperator.Abs, m[i, j]);
                        rowSum = rowSum is ConstantExpression { Value: 0 } ? abs : new BinaryExpression(rowSum, BinaryOperator.Add, abs);
                    }
                    maxRowSum = new FunctionExpression("Max", [maxRowSum, rowSum]);
                }
                return maxRowSum;

            default:
                throw new ArgumentException($"Unknown norm: {norm}");
        }
    }

    public static MatrixExpression Transpose(MatrixExpression m) => m.Transpose();
    public static MatrixExpression ConjugateTranspose(MatrixExpression m) => m.ConjugateTranspose();
    public static MatrixExpression Conjugate(MatrixExpression m) => m.Conjugate();

    private static bool IsZero(MatrixExpression m)
    {
        return m.Rows.All(row => row.All(IsZero));
    }
}

public enum MatrixNorm
{
    Frobenius,
    OneNorm,
    InfinityNorm
}