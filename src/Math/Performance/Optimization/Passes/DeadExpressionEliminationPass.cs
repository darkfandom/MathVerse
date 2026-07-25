namespace MathVerse.Math.Performance.Optimization.Passes;

/// <summary>
/// Removes leaf expressions whose results are never referenced by any parent node
/// other than the root itself.
/// </summary>
public sealed class DeadExpressionEliminationPass : OptimizationPass
{
    /// <inheritdoc/>
    public override string Name => "DeadExpressionElimination";

    /// <inheritdoc/>
    public override OptimizationStage Stage => OptimizationStage.DeadExpressionElimination;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override Expression Optimize(Expression input, OptimizationContext context)
    {
        var references = new Dictionary<int, int>();
        CountReferences(input, references);

        return Prune(input, references, context);
    }

    private static void CountReferences(Expression expr, Dictionary<int, int> references)
    {
        foreach (var child in expr.Children)
        {
            if (references.ContainsKey(child.NodeId))
            {
                references[child.NodeId]++;
            }
            else
            {
                references[child.NodeId] = 1;
            }

            CountReferences(child, references);
        }
    }

    private static Expression Prune(Expression expr, Dictionary<int, int> references, OptimizationContext context)
    {
        if (expr is LiteralExpression or VariableExpression or ConstantExpression)
        {
            return expr;
        }

        switch (expr)
        {
            case BinaryExpression binary:
            {
                var left = Prune(binary.Left, references, context);
                var right = Prune(binary.Right, references, context);

                if (left is not null && right is not null)
                    return new BinaryExpression(binary.Operator, left, right);

                if (left is not null)
                    return left;

                if (right is not null)
                    return right;

                context.MarkChanged();
                return new LiteralExpression(0);
            }

            case UnaryExpression unary:
            {
                var operand = Prune(unary.Operand, references, context);

                if (operand is not null)
                    return new UnaryExpression(unary.Operator, operand);

                context.MarkChanged();
                return new LiteralExpression(0);
            }

            case FunctionCallExpression func:
            {
                var args = new List<Expression>();
                var changed = false;

                for (var i = 0; i < func.Arguments.Count; i++)
                {
                    var pruned = Prune(func.Arguments[i], references, context);
                    if (pruned is not null)
                    {
                        args.Add(pruned);
                        if (!ReferenceEquals(pruned, func.Arguments[i]))
                            changed = true;
                    }
                    else
                    {
                        changed = true;
                    }
                }

                if (args.Count == func.Arguments.Count && !changed)
                    return func;

                context.MarkChanged();

                return args.Count > 0
                    ? new FunctionCallExpression(func.Name, args)
                    : new LiteralExpression(0);
            }

            default:
                return expr;
        }
    }
}
