using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Units;
using MathVerse.Math.Expressions;

namespace MathVerse.Math.Foundation.Integration;

public static class QuantityExpressionFactory
{
    public static Expression CreateQuantityExpression(double value, Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        return new LiteralExpression(value).WithAnnotation("unit", unit);
    }

    public static Expression CreateQuantityAdd(Expression left, Expression right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));

        var analyzer = DimensionAnalyzer.Instance;
        var leftDim = left.GetDimension(analyzer);
        var rightDim = right.GetDimension(analyzer);

        if (!leftDim.IsCompatibleWith(rightDim))
            throw new InvalidOperationException($"Cannot add expressions with incompatible dimensions: {leftDim} vs {rightDim}");

        return Expr.Add(left, right);
    }

    public static Expression CreateQuantityMultiply(Expression left, Expression right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));
        return Expr.Multiply(left, right);
    }

    public static Expression CreateQuantityDivide(Expression left, Expression right)
    {
        if (left is null) throw new ArgumentNullException(nameof(left));
        if (right is null) throw new ArgumentNullException(nameof(right));
        return Expr.Divide(left, right);
    }

    public static Expression CreateQuantityPower(Expression baseExpr, double exponent)
    {
        if (baseExpr is null) throw new ArgumentNullException(nameof(baseExpr));
        return Expr.Pow(baseExpr, new LiteralExpression(exponent));
    }
}
