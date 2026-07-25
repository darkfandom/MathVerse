using System.Collections.Immutable;
using System.Numerics;

namespace MathVerse.Math.CAS.Matrix;

public sealed record MatrixExpression
{
    public ImmutableArray<ImmutableArray<Expression>> Rows { get; init; }
    public int RowCount => Rows.Length;
    public int ColCount => RowCount > 0 ? Rows[0].Length : 0;

    public MatrixExpression(ImmutableArray<ImmutableArray<Expression>> rows)
    {
        if (rows.Length > 0)
        {
            int cols = rows[0].Length;
            if (rows.Any(row => row.Length != cols))
                throw new ArgumentException("All rows must have the same number of columns");
        }
        Rows = rows;
    }

    public static MatrixExpression FromExpression(Expression expr)
    {
        if (expr is ListExpression list && list.Items.All(item => item is ListExpression))
        {
            var matrixRows = list.Items.Select(item =>
            {
                var rowList = (ListExpression)item;
                return rowList.Items.ToImmutableArray();
            }).ToImmutableArray();
            return new MatrixExpression(matrixRows);
        }

        throw new ArgumentException("Expression is not a valid matrix representation");
    }

    public static MatrixExpression FromColumns(ImmutableArray<MatrixExpression> columns)
    {
        if (columns.Length == 0)
            return new MatrixExpression(ImmutableArray<ImmutableArray<Expression>>.Empty);

        int rows = columns[0].RowCount;
        int cols = columns.Length;

        var rowData = new ImmutableArray<Expression>[rows];
        for (int i = 0; i < rows; i++)
        {
            var row = new Expression[cols];
            for (int j = 0; j < cols; j++)
                row[j] = columns[j][i, 0];
            rowData[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(rowData.ToImmutableArray());
    }

    public Expression ToExpression()
    {
        var rows = Rows.Select(row => new ListExpression(row.Cast<Expression>().ToImmutableArray())).Cast<Expression>().ToImmutableArray();
        return new ListExpression(rows);
    }

    public MatrixExpression Transpose()
    {
        if (RowCount == 0)
            return this;

        var transposed = new ImmutableArray<Expression>[ColCount];
        for (int j = 0; j < ColCount; j++)
        {
            var column = new Expression[RowCount];
            for (int i = 0; i < RowCount; i++)
                column[i] = Rows[i][j];
            transposed[j] = column.ToImmutableArray();
        }
        return new MatrixExpression(transposed.ToImmutableArray());
    }

    public MatrixExpression ConjugateTranspose()
    {
        return Transpose().Conjugate();
    }

    public MatrixExpression Conjugate()
    {
        var conjugated = Rows.Select(row =>
            row.Select(expr => new FunctionExpression("Conjugate", [expr])).Cast<Expression>().ToImmutableArray()
        ).ToImmutableArray();
        return new MatrixExpression(conjugated);
    }

    public MatrixExpression WithElement(int row, int col, Expression value)
    {
        var newRows = Rows.Select(r => r.ToArray()).ToArray();
        newRows[row][col] = value;
        return new MatrixExpression(newRows.Select(r => r.ToImmutableArray()).ToImmutableArray());
    }

    public Expression this[int row, int col] => Rows[row][col];

    public static MatrixExpression Zero(int rows, int cols)
    {
        var rowData = Enumerable.Range(0, rows).Select(_ =>
            Enumerable.Repeat(Expression.Zero, cols).ToImmutableArray()
        ).ToImmutableArray();
        return new MatrixExpression(rowData);
    }

    public static MatrixExpression Identity(int size)
    {
        var rows = new ImmutableArray<Expression>[size];
        for (int i = 0; i < size; i++)
        {
            var row = new Expression[size];
            for (int j = 0; j < size; j++)
                row[j] = i == j ? Expression.One : Expression.Zero;
            rows[i] = row.ToImmutableArray();
        }
        return new MatrixExpression(rows.ToImmutableArray());
    }

    public override string ToString()
    {
        if (RowCount == 0)
            return "Matrix[]";

        var rowsStr = Rows.Select(row =>
            "[" + string.Join(", ", row.Select(e => e.ToString())) + "]"
        );
        return "Matrix[" + string.Join(", ", rowsStr) + "]";
    }
}

public abstract record Expression
{
    public static Expression Zero => new ConstantExpression(0);
    public static Expression One => new ConstantExpression(1);
    public static Expression ZeroComplex => new ComplexConstantExpression(Complex.Zero);
    public static Expression OneComplex => new ComplexConstantExpression(Complex.One);
}

public sealed record ConstantExpression(double Value) : Expression;
public sealed record ComplexConstantExpression(Complex Value) : Expression;
public sealed record SymbolExpression(string Name) : Expression;
public sealed record FunctionExpression(string Name, ImmutableArray<Expression> Arguments) : Expression;
public sealed record ListExpression(ImmutableArray<Expression> Items) : Expression;
public sealed record BinaryExpression(Expression Left, BinaryOperator Op, Expression Right) : Expression;
public sealed record UnaryExpression(UnaryOperator Op, Expression Operand) : Expression;

public enum BinaryOperator { Add, Subtract, Multiply, Divide, Power, Equal, NotEqual, Less, Greater, LessEqual, GreaterEqual, And, Or }
public enum UnaryOperator { Negate, Not, Sin, Cos, Tan, Exp, Log, Sqrt, Abs, Conjugate, Transpose }