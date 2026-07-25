namespace MathVerse.Math.Performance.Optimization.Passes;

/// <summary>
/// Applies algebraic simplifications such as identity and annihilation rules.
/// </summary>
public sealed class AlgebraicSimplificationPass : OptimizationPass
{
    /// <inheritdoc/>
    public override string Name => "AlgebraicSimplification";

    /// <inheritdoc/>
    public override OptimizationStage Stage => OptimizationStage.AlgebraicOptimization;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override Expression Optimize(Expression input, OptimizationContext context)
    {
        if (input is not BinaryExpression binary)
        {
            if (input is UnaryExpression unary)
            {
                return OptimizeUnary(unary, context);
            }

            return input;
        }

        var left = Optimize(binary.Left, context);
        var right = Optimize(binary.Right, context);

        if (IsLiteral(right, out var rVal))
        {
            if (binary.Operator.Equals(MathOperator.Add))
            {
                if (rVal == 0.0)
                {
                    context.MarkChanged();
                    return left;
                }
            }

            if (binary.Operator.Equals(MathOperator.Multiply))
            {
                if (rVal == 1.0)
                {
                    context.MarkChanged();
                    return left;
                }

                if (rVal == 0.0)
                {
                    context.MarkChanged();
                    return new LiteralExpression(0);
                }
            }

            if (binary.Operator.Equals(MathOperator.Power))
            {
                if (rVal == 0.0)
                {
                    context.MarkChanged();
                    return new LiteralExpression(1);
                }

                if (rVal == 1.0)
                {
                    context.MarkChanged();
                    return left;
                }
            }
        }

        if (IsLiteral(left, out var lVal))
        {
            if (binary.Operator.Equals(MathOperator.Add))
            {
                if (lVal == 0.0)
                {
                    context.MarkChanged();
                    return right;
                }
            }

            if (binary.Operator.Equals(MathOperator.Multiply))
            {
                if (lVal == 1.0)
                {
                    context.MarkChanged();
                    return right;
                }

                if (lVal == 0.0)
                {
                    context.MarkChanged();
                    return new LiteralExpression(0);
                }
            }
        }

        if (binary.Operator.Equals(MathOperator.Subtract) && left.Equals(right))
        {
            context.MarkChanged();
            return new LiteralExpression(0);
        }

        if (binary.Operator.Equals(MathOperator.Divide) && left.Equals(right))
        {
            context.MarkChanged();
            return new LiteralExpression(1);
        }

        if (ReferenceEquals(left, binary.Left) && ReferenceEquals(right, binary.Right))
            return binary;

        return new BinaryExpression(binary.Operator, left, right);
    }

    private Expression OptimizeUnary(UnaryExpression unary, OptimizationContext context)
    {
        var operand = Optimize(unary.Operand, context);

        if (unary.Operator.Equals(MathOperator.Negate) && operand is UnaryExpression inner &&
            inner.Operator.Equals(MathOperator.Negate))
        {
            context.MarkChanged();
            return inner.Operand;
        }

        if (unary.Operator.Equals(MathOperator.Negate) && operand is BinaryExpression innerBinary &&
            innerBinary.Operator.Equals(MathOperator.Add))
        {
            var negatedRight = new UnaryExpression(MathOperator.Negate, innerBinary.Right);
            context.MarkChanged();
            return new BinaryExpression(MathOperator.Subtract, innerBinary.Left, negatedRight);
        }

        return ReferenceEquals(operand, unary.Operand)
            ? unary
            : new UnaryExpression(unary.Operator, operand);
    }

    private static bool IsLiteral(Expression expr, out double value)
    {
        if (expr is LiteralExpression lit)
        {
            value = lit.Value;
            return true;
        }

        value = 0;
        return false;
    }
}
