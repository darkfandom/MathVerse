using System.Collections.Immutable;

namespace MathVerse.Math.CAS.Matrix;

public static class MatrixSolver
{
    public static MatrixExpression SolveLinearSystem(MatrixExpression A, MatrixExpression b)
    {
        if (A.RowCount != b.RowCount)
            throw new ArgumentException("Matrix A and vector b must have the same number of rows");

        if (A.RowCount != A.ColCount)
            throw new ArgumentException("Matrix A must be square for linear system solving");

        if (b.ColCount != 1)
            throw new ArgumentException("Vector b must be a column vector (single column)");

        var detA = MatrixOperations.Determinant(A);
        if (IsZero(detA))
            throw new InvalidOperationException("Matrix A is singular (determinant is zero)");

        var invA = MatrixOperations.Inverse(A);
        return MatrixOperations.Multiply(invA, b);
    }

    public static MatrixExpression SolveLinearSystemLU(MatrixExpression A, MatrixExpression b)
    {
        if (A.RowCount != b.RowCount || b.ColCount != 1)
            throw new ArgumentException("Invalid dimensions for linear system");

        var (L, U, perm) = MatrixOperations.LUDecomposition(A);
        int n = A.RowCount;

        var Pb = PermuteRows(b, perm);

        var y = ForwardSubstitution(L, Pb);
        var x = BackwardSubstitution(U, y);

        return x;
    }

    private static MatrixExpression PermuteRows(MatrixExpression m, ImmutableArray<int> perm)
    {
        var rows = new ImmutableArray<Expression>[m.RowCount];
        for (int i = 0; i < m.RowCount; i++)
            rows[i] = m.Rows[perm[i]];
        return new MatrixExpression(rows.ToImmutableArray());
    }

    private static MatrixExpression ForwardSubstitution(MatrixExpression L, MatrixExpression b)
    {
        int n = L.RowCount;
        var y = new Expression[n];
        for (int i = 0; i < n; i++)
        {
            Expression sum = Expression.Zero;
            for (int j = 0; j < i; j++)
            {
                var prod = new BinaryExpression(L[i, j], BinaryOperator.Multiply, y[j]);
                sum = sum is ConstantExpression { Value: 0 } ? prod : new BinaryExpression(sum, BinaryOperator.Add, prod);
            }
            y[i] = new BinaryExpression(
                new BinaryExpression(b[i, 0], BinaryOperator.Subtract, sum),
                BinaryOperator.Divide,
                L[i, i]
            );
        }

        var colVec = new ImmutableArray<Expression>[n];
        for (int i = 0; i < n; i++)
            colVec[i] = [y[i]];
        return new MatrixExpression(colVec.ToImmutableArray());
    }

    private static MatrixExpression BackwardSubstitution(MatrixExpression U, MatrixExpression y)
    {
        int n = U.RowCount;
        var x = new Expression[n];
        for (int i = n - 1; i >= 0; i--)
        {
            Expression sum = Expression.Zero;
            for (int j = i + 1; j < n; j++)
            {
                var prod = new BinaryExpression(U[i, j], BinaryOperator.Multiply, x[j]);
                sum = sum is ConstantExpression { Value: 0 } ? prod : new BinaryExpression(sum, BinaryOperator.Add, prod);
            }
            x[i] = new BinaryExpression(
                new BinaryExpression(y[i, 0], BinaryOperator.Subtract, sum),
                BinaryOperator.Divide,
                U[i, i]
            );
        }

        var colVec = new ImmutableArray<Expression>[n];
        for (int i = 0; i < n; i++)
            colVec[i] = [x[i]];
        return new MatrixExpression(colVec.ToImmutableArray());
    }

    public static MatrixExpression LeastSquares(MatrixExpression A, MatrixExpression b)
    {
        if (A.RowCount != b.RowCount)
            throw new ArgumentException("Matrix A and vector b must have the same number of rows");

        if (b.ColCount != 1)
            throw new ArgumentException("Vector b must be a column vector");

        var AT = A.Transpose();
        var ATA = MatrixOperations.Multiply(AT, A);
        var ATb = MatrixOperations.Multiply(AT, b);

        return SolveLinearSystem(ATA, ATb);
    }

    public static MatrixExpression LeastSquaresQR(MatrixExpression A, MatrixExpression b)
    {
        var (Q, R) = MatrixOperations.QRDecomposition(A);
        var QT = Q.Transpose();
        var QTb = MatrixOperations.Multiply(QT, b);

        int n = A.ColCount;
        var x = new Expression[n];
        for (int i = n - 1; i >= 0; i--)
        {
            Expression sum = Expression.Zero;
            for (int j = i + 1; j < n; j++)
            {
                var prod = new BinaryExpression(R[i, j], BinaryOperator.Multiply, x[j]);
                sum = sum is ConstantExpression { Value: 0 } ? prod : new BinaryExpression(sum, BinaryOperator.Add, prod);
            }
            x[i] = new BinaryExpression(
                new BinaryExpression(QTb[i, 0], BinaryOperator.Subtract, sum),
                BinaryOperator.Divide,
                R[i, i]
            );
        }

        var colVec = new ImmutableArray<Expression>[n];
        for (int i = 0; i < n; i++)
            colVec[i] = [x[i]];
        return new MatrixExpression(colVec.ToImmutableArray());
    }

    public static ImmutableArray<MatrixExpression> NullSpace(MatrixExpression A)
    {
        return MatrixOperations.NullSpace(A);
    }

    public static ImmutableArray<MatrixExpression> ColumnSpace(MatrixExpression A)
    {
        return MatrixOperations.ColumnSpace(A);
    }

    public static ImmutableArray<MatrixExpression> RowSpace(MatrixExpression A)
    {
        var AT = A.Transpose();
        var colSpace = MatrixOperations.ColumnSpace(AT);
        return colSpace.Select(c => c.Transpose()).ToImmutableArray();
    }

    public static (int rank, int nullity) RankNullity(MatrixExpression A)
    {
        int rank = MatrixOperations.Rank(A);
        int nullity = A.ColCount - rank;
        return (rank, nullity);
    }

    public static MatrixExpression PseudoInverse(MatrixExpression A)
    {
        var (U, S, V) = MatrixOperations.SVD(A);
        int rows = A.RowCount;
        int cols = A.ColCount;

        var SPlus = new ImmutableArray<Expression>[cols];
        for (int i = 0; i < cols; i++)
        {
            var row = new Expression[rows];
            for (int j = 0; j < rows; j++)
            {
                if (i == j && !IsZero(S[i, i]))
                    row[j] = new BinaryExpression(Expression.One, BinaryOperator.Divide, S[i, i]);
                else
                    row[j] = Expression.Zero;
            }
            SPlus[i] = row.ToImmutableArray();
        }
        var SPlusMatrix = new MatrixExpression(SPlus.ToImmutableArray());

        var VTranspose = V.Transpose();
        var UTranspose = U.Transpose();

        var temp = MatrixOperations.Multiply(VTranspose, SPlusMatrix);
        return MatrixOperations.Multiply(temp, UTranspose);
    }

    public static (MatrixExpression particular, ImmutableArray<MatrixExpression> homogeneous) SolveGeneral(MatrixExpression A, MatrixExpression b)
    {
        var particular = LeastSquares(A, b);
        var homogeneous = NullSpace(A);
        return (particular, homogeneous);
    }

    public static bool IsConsistent(MatrixExpression A, MatrixExpression b)
    {
        int rankA = MatrixOperations.Rank(A);
        var augmented = AugmentMatrix(A, b);
        int rankAug = MatrixOperations.Rank(augmented);
        return rankA == rankAug;
    }

    private static MatrixExpression AugmentMatrix(MatrixExpression A, MatrixExpression b)
    {
        var rows = new ImmutableArray<Expression>[A.RowCount];
        for (int i = 0; i < A.RowCount; i++)
        {
            var row = new Expression[A.ColCount + b.ColCount];
            for (int j = 0; j < A.ColCount; j++)
                row[j] = A[i, j];
            for (int j = 0; j < b.ColCount; j++)
                row[A.ColCount + j] = b[i, j];
            rows[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(rows.ToImmutableArray());
    }

    private static bool IsZero(Expression expr)
    {
        return expr is ConstantExpression { Value: 0 };
    }
}