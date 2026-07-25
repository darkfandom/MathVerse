namespace MathVerse.Math.Performance.Optimization.Passes;

/// <summary>
/// Normalizes expressions by sorting commutative operands, flattening nested same-operator
/// binaries, and normalizing negation.
/// </summary>
public sealed class CanonicalizationPass : OptimizationPass
{
    private static readonly HashSet<string> CommutativeOperators =
    [
        MathOperator.Add.Name,
        MathOperator.Multiply.Name,
    ];

    /// <inheritdoc/>
    public override string Name => "Canonicalization";

    /// <inheritdoc/>
    public override OptimizationStage Stage => OptimizationStage.Canonicalization;

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override Expression Optimize(Expression input, OptimizationContext context)
    {
        return Canonicalize(input, context);
    }

    private static Expression Canonicalize(Expression expr, OptimizationContext context)
    {
        if (expr is UnaryExpression unary)
        {
            var operand = Canonicalize(unary.Operand, context);

            if (unary.Operator.Equals(MathOperator.Negate) && operand is LiteralExpression lit)
            {
                context.MarkChanged();
                return new LiteralExpression(-lit.Value);
            }

            return ReferenceEquals(operand, unary.Operand)
                ? unary
                : new UnaryExpression(unary.Operator, operand);
        }

        if (expr is not BinaryExpression binary)
        {
            return expr;
        }

        var left = Canonicalize(binary.Left, context);
        var right = Canonicalize(binary.Right, context);

        if (!IsCommutative(binary.Operator))
        {
            return ReferenceEquals(left, binary.Left) && ReferenceEquals(right, binary.Right)
                ? binary
                : new BinaryExpression(binary.Operator, left, right);
        }

        var terms = new List<Expression>();
        CollectTerms(binary.Operator, left, terms);
        CollectTerms(binary.Operator, right, terms);

        var sorted = terms
            .Select(static t => (Expression: t, Hash: t.GetHashCode()))
            .OrderBy(static x => x.Hash)
            .Select(static x => x.Expression)
            .ToList();

        var areSameOrder = true;
        for (var i = 0; i < terms.Count; i++)
        {
            if (!ReferenceEquals(terms[i], sorted[i]))
            {
                areSameOrder = false;
                break;
            }
        }

        var anyChildChanged = !ReferenceEquals(left, binary.Left) || !ReferenceEquals(right, binary.Right);

        if (areSameOrder && !anyChildChanged)
            return binary;

        if (!areSameOrder)
            context.MarkChanged();
        else if (anyChildChanged)
            context.MarkChanged();

        var result = sorted[0];
        for (var i = 1; i < sorted.Count; i++)
        {
            result = new BinaryExpression(binary.Operator, result, sorted[i]);
        }

        return result;
    }

    private static void CollectTerms(MathOperator op, Expression expr, List<Expression> terms)
    {
        if (expr is BinaryExpression b && b.Operator.Equals(op))
        {
            CollectTerms(op, b.Left, terms);
            CollectTerms(op, b.Right, terms);
        }
        else
        {
            terms.Add(expr);
        }
    }

    private static bool IsCommutative(MathOperator op) =>
        CommutativeOperators.Contains(op.Name);
}
