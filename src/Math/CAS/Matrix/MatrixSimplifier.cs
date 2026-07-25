using System.Collections.Immutable;

namespace MathVerse.Math.CAS.Matrix;

public static class MatrixSimplifier
{
    public static MatrixExpression Simplify(MatrixExpression matrix)
    {
        var simplified = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Simplify(matrix[i, j]);
            simplified[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(simplified.ToImmutableArray());
    }

    public static MatrixExpression Factor(MatrixExpression matrix)
    {
        var factored = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Factor(matrix[i, j]);
            factored[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(factored.ToImmutableArray());
    }

    public static MatrixExpression Expand(MatrixExpression matrix)
    {
        var expanded = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Expand(matrix[i, j]);
            expanded[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(expanded.ToImmutableArray());
    }

    public static MatrixExpression Collect(MatrixExpression matrix, SymbolExpression symbol)
    {
        var collected = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Collect(matrix[i, j], symbol);
            collected[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(collected.ToImmutableArray());
    }

    public static MatrixExpression Together(MatrixExpression matrix)
    {
        var combined = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Together(matrix[i, j]);
            combined[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(combined.ToImmutableArray());
    }

    public static MatrixExpression Apart(MatrixExpression matrix, SymbolExpression symbol)
    {
        var apart = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Apart(matrix[i, j], symbol);
            apart[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(apart.ToImmutableArray());
    }

    public static MatrixExpression Cancel(MatrixExpression matrix)
    {
        var cancelled = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = ExpressionSimplifier.Cancel(matrix[i, j]);
            cancelled[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(cancelled.ToImmutableArray());
    }

    public static MatrixExpression SimplifyElementwise(MatrixExpression matrix, Func<Expression, Expression> simplifier)
    {
        var simplified = new ImmutableArray<Expression>[matrix.RowCount];
        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
                row[j] = simplifier(matrix[i, j]);
            simplified[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(simplified.ToImmutableArray());
    }

    public static MatrixExpression MatrixSimplify(MatrixExpression matrix)
    {
        var simplified = Simplify(matrix);

        if (simplified.RowCount == simplified.ColCount)
        {
            var det = MatrixOperations.Determinant(simplified);
            var simplifiedDet = ExpressionSimplifier.Simplify(det);

            if (simplifiedDet is ConstantExpression c && c.Value == 0)
            {
                var rank = MatrixOperations.Rank(simplified);
                if (rank < simplified.RowCount)
                    return simplified;
            }
        }

        return simplified;
    }

    public static (MatrixExpression factored, ImmutableArray<SymbolExpression> factors) FactorMatrix(MatrixExpression matrix)
    {
        var factors = new HashSet<SymbolExpression>();
        var factored = new ImmutableArray<Expression>[matrix.RowCount];

        for (int i = 0; i < matrix.RowCount; i++)
        {
            var row = new Expression[matrix.ColCount];
            for (int j = 0; j < matrix.ColCount; j++)
            {
                var (expr, exprFactors) = ExpressionSimplifier.FactorWithFactors(matrix[i, j]);
                foreach (var f in exprFactors)
                    factors.Add(f);
                row[j] = expr;
            }
            factored[i] = row.ToImmutableArray();
        }

        return (new MatrixExpression(factored.ToImmutableArray()), factors.ToImmutableArray());
    }

    public static MatrixExpression MatrixNormalForm(MatrixExpression matrix)
    {
        var (rref, _, _) = GaussianElimination(matrix);
        return rref;
    }

    public static MatrixExpression SmithNormalForm(MatrixExpression matrix)
    {
        var m = matrix;
        int rows = m.RowCount;
        int cols = m.ColCount;

        bool changed = true;
        while (changed)
        {
            changed = false;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (IsZero(m[i, j])) continue;

                    for (int k = i + 1; k < rows; k++)
                    {
                        if (IsZero(m[k, j])) continue;
                        var (g, s, t) = ExtendedGCD(m[i, j], m[k, j]);
                        RowOperation(ref m, i, k, s, t);
                        changed = true;
                    }

                    for (int k = j + 1; k < cols; k++)
                    {
                        if (IsZero(m[i, k])) continue;
                        var (g, s, t) = ExtendedGCD(m[i, j], m[i, k]);
                        ColOperation(ref m, j, k, s, t);
                        changed = true;
                    }
                }
            }
        }

        return m;
    }

    private static void RowOperation(ref MatrixExpression m, int i, int k, Expression s, Expression t)
    {
        var rows = m.Rows.Select(r => r.ToArray()).ToArray();
        for (int j = 0; j < m.ColCount; j++)
        {
            var newI = new BinaryExpression(
                new BinaryExpression(s, BinaryOperator.Multiply, rows[i][j]),
                BinaryOperator.Add,
                new BinaryExpression(t, BinaryOperator.Multiply, rows[k][j])
            );
            var newK = new BinaryExpression(
                new BinaryExpression(t, BinaryOperator.Multiply, rows[i][j]),
                BinaryOperator.Subtract,
                new BinaryExpression(s, BinaryOperator.Multiply, rows[k][j])
            );
            rows[i][j] = newI;
            rows[k][j] = newK;
        }
        m = new MatrixExpression(rows.Select(r => r.ToImmutableArray()).ToImmutableArray());
    }

    private static void ColOperation(ref MatrixExpression m, int j, int k, Expression s, Expression t)
    {
        var rows = m.Rows.Select(r => r.ToArray()).ToArray();
        for (int i = 0; i < m.RowCount; i++)
        {
            var newJ = new BinaryExpression(
                new BinaryExpression(s, BinaryOperator.Multiply, rows[i][j]),
                BinaryOperator.Add,
                new BinaryExpression(t, BinaryOperator.Multiply, rows[i][k])
            );
            var newK = new BinaryExpression(
                new BinaryExpression(t, BinaryOperator.Multiply, rows[i][j]),
                BinaryOperator.Subtract,
                new BinaryExpression(s, BinaryOperator.Multiply, rows[i][k])
            );
            rows[i][j] = newJ;
            rows[i][k] = newK;
        }
        m = new MatrixExpression(rows.Select(r => r.ToImmutableArray()).ToImmutableArray());
    }

    private static (Expression g, Expression s, Expression t) ExtendedGCD(Expression a, Expression b)
    {
        return (a, Expression.One, Expression.Zero);
    }

    private static bool IsZero(Expression expr)
    {
        return expr is ConstantExpression { Value: 0 };
    }

    private static (MatrixExpression rref, ImmutableArray<int> pivots, int rank) GaussianElimination(MatrixExpression m)
    {
        int rows = m.RowCount;
        int cols = m.ColCount;
        var matrix = m.Rows.Select(r => r.ToArray()).ToArray();
        var pivotCols = new List<int>();
        int row = 0;

        for (int col = 0; col < cols && row < rows; col++)
        {
            int pivotRow = -1;
            for (int i = row; i < rows; i++)
            {
                if (!IsZero(matrix[i][col]))
                {
                    pivotRow = i;
                    break;
                }
            }

            if (pivotRow == -1) continue;

            (matrix[row], matrix[pivotRow]) = (matrix[pivotRow], matrix[row]);
            pivotCols.Add(col);

            var pivot = matrix[row][col];
            for (int j = col; j < cols; j++)
                matrix[row][j] = new BinaryExpression(matrix[row][j], BinaryOperator.Divide, pivot);

            for (int i = 0; i < rows; i++)
            {
                if (i == row) continue;
                var factor = matrix[i][col];
                if (IsZero(factor)) continue;
                for (int j = col; j < cols; j++)
                {
                    var prod = new BinaryExpression(factor, BinaryOperator.Multiply, matrix[row][j]);
                    matrix[i][j] = new BinaryExpression(matrix[i][j], BinaryOperator.Subtract, prod);
                }
            }
            row++;
        }

        var resultRows = matrix.Select(r => r.ToImmutableArray()).ToImmutableArray();
        return (new MatrixExpression(resultRows), pivotCols.ToImmutableArray(), pivotCols.Count);
    }
}

public static class ExpressionSimplifier
{
    public static Expression Simplify(Expression expr) => expr;
    public static Expression Factor(Expression expr) => expr;
    public static Expression Expand(Expression expr) => expr;
    public static Expression Collect(Expression expr, SymbolExpression symbol) => expr;
    public static Expression Together(Expression expr) => expr;
    public static Expression Apart(Expression expr, SymbolExpression symbol) => expr;
    public static Expression Cancel(Expression expr) => expr;
    public static (Expression expr, ImmutableArray<SymbolExpression> factors) FactorWithFactors(Expression expr) => (expr, ImmutableArray<SymbolExpression>.Empty);
}