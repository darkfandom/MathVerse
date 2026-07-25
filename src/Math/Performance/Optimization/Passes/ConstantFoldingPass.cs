namespace MathVerse.Math.Performance.Optimization.Passes;

/// <summary>
/// Evaluates constant binary expressions (Literal op Literal → Literal).
/// Handles +, -, *, /, %. Preserves division by zero.
/// </summary>
public sealed class ConstantFoldingPass : OptimizationPass
{
    /// <inheritdoc/>
    public override string Name => "ConstantFolding";

    /// <inheritdoc/>
    public override OptimizationStage Stage => OptimizationStage.ConstantFolding;

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

        if (left is LiteralExpression leftLit && right is LiteralExpression rightLit)
        {
            var result = FoldConstants(binary.Operator, leftLit.Value, rightLit.Value);
            if (result.HasValue)
            {
                context.MarkChanged();
                return new LiteralExpression(result.Value);
            }
        }

        return ReferenceEquals(left, binary.Left) && ReferenceEquals(right, binary.Right)
            ? binary
            : new BinaryExpression(binary.Operator, left, right);
    }

    private Expression OptimizeUnary(UnaryExpression unary, OptimizationContext context)
    {
        var operand = Optimize(unary.Operand, context);

        if (operand is LiteralExpression lit && unary.Operator.Equals(MathOperator.Negate))
        {
            context.MarkChanged();
            return new LiteralExpression(-lit.Value);
        }

        return ReferenceEquals(operand, unary.Operand)
            ? unary
            : new UnaryExpression(unary.Operator, operand);
    }

    private static double? FoldConstants(MathOperator op, double left, double right)
    {
        if (op.Equals(MathOperator.Add))
            return left + right;

        if (op.Equals(MathOperator.Subtract))
            return left - right;

        if (op.Equals(MathOperator.Multiply))
            return left * right;

        if (op.Equals(MathOperator.Divide))
            return left / right;

        if (op.Equals(MathOperator.Modulo))
            return left % right;

        if (op.Equals(MathOperator.Power))
            return System.Math.Pow(left, right);

        return null;
    }
}
