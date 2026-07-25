using MathVerse.Math.Visitors;

namespace MathVerse.Math.Performance.Optimization.Passes;

/// <summary>
/// Finds duplicate subexpressions and reuses the first occurrence.
/// </summary>
public sealed class CommonSubexpressionEliminationPass : OptimizationPass
{
    /// <inheritdoc/>
    public override string Name => "CommonSubexpressionElimination";

    /// <inheritdoc/>
    public override OptimizationStage Stage => OptimizationStage.CommonSubexpressionElimination;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override Expression Optimize(Expression input, OptimizationContext context)
    {
        var seen = new Dictionary<int, Expression>();
        var result = Eliminate(input, seen, context);
        return result;
    }

    private static Expression Eliminate(Expression expr, Dictionary<int, Expression> seen, OptimizationContext context)
    {
        var hash = ExpressionHasher.Hash(expr);

        if (expr is LiteralExpression or VariableExpression or ConstantExpression)
        {
            return expr;
        }

        if (seen.TryGetValue(hash, out var existing) && existing.Equals(expr))
        {
            context.MarkChanged();
            return existing;
        }

        seen[hash] = expr;

        switch (expr)
        {
            case BinaryExpression binary:
            {
                var left = Eliminate(binary.Left, seen, context);
                var right = Eliminate(binary.Right, seen, context);

                if (ReferenceEquals(left, binary.Left) && ReferenceEquals(right, binary.Right))
                    return binary;

                return new BinaryExpression(binary.Operator, left, right);
            }

            case UnaryExpression unary:
            {
                var operand = Eliminate(unary.Operand, seen, context);

                return ReferenceEquals(operand, unary.Operand)
                    ? unary
                    : new UnaryExpression(unary.Operator, operand);
            }

            case FunctionCallExpression func:
            {
                var args = new Expression[func.Arguments.Count];
                var changed = false;

                for (var i = 0; i < func.Arguments.Count; i++)
                {
                    args[i] = Eliminate(func.Arguments[i], seen, context);
                    if (!ReferenceEquals(args[i], func.Arguments[i]))
                        changed = true;
                }

                return changed
                    ? new FunctionCallExpression(func.Name, args)
                    : func;
            }

            default:
                return expr;
        }
    }
}
