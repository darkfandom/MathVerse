namespace MathVerse.Math.CAS.Canonicalization;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using MathVerse.Math.Visitors;
using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class Canonicalizer
{
    private static readonly Lazy<Canonicalizer> _instance = new(() => new Canonicalizer());
    public static Canonicalizer Instance => _instance.Value;

    private readonly ConcurrentDictionary<Expression, CanonicalForm> _cache = new(ExpressionEqualityComparer.Instance);
    private readonly CanonicalizationTransformer _transformer = new();

    private Canonicalizer() { }

    public CanonicalForm Canonicalize(Expression expr)
    {
        if (_cache.TryGetValue(expr, out var cached))
            return cached;

        var result = CanonicalizeCore(expr);
        _cache.TryAdd(expr, result);
        return result;
    }

    public Expression CanonicalizeInPlace(Expression expr)
    {
        return Canonicalize(expr).Expression;
    }

    private CanonicalForm CanonicalizeCore(Expression expr)
    {
        var context = CanonicalizationContext.Default;
        var rules = new List<string>();
        var current = expr;

        var transformed = _transformer.Transform(current, context, rules);

        var appliedRules = rules.Distinct().ToImmutableArray();
        var isCanonical = transformed.Equals(current);

        return new CanonicalForm
        {
            Expression = transformed,
            AppliedRules = appliedRules,
            IsCanonical = isCanonical
        };
    }

    public void ClearCache() => _cache.Clear();
    public int CacheCount => _cache.Count;
}

internal sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
{
    public static readonly ExpressionEqualityComparer Instance = new();

    public bool Equals(Expression? x, Expression? y) => x?.Equals(y) ?? y is null;

    public int GetHashCode(Expression obj) => obj?.GetHashCode() ?? 0;
}

internal sealed class CanonicalizationTransformer : IExpressionTransformer
{
    public Expression Transform(Expression expr, CanonicalizationContext context, List<string> appliedRules)
    {
        var visited = new HashSet<int>();
        return TransformRecursive(expr, context, appliedRules, visited);
    }

    private Expression TransformRecursive(Expression expr, CanonicalizationContext context, List<string> appliedRules, HashSet<int> visited)
    {
        if (!visited.Add(expr.NodeId))
            return expr;

        var transformed = expr switch
        {
            BinaryExpression b => TransformBinary(b, context, appliedRules, visited),
            UnaryExpression u => TransformUnary(u, context, appliedRules, visited),
            FunctionCallExpression f => TransformFunctionCall(f, context, appliedRules, visited),
            _ => expr
        };

        if (!transformed.Equals(expr))
        {
            appliedRules.Add(GetRuleName(expr, transformed));
            return TransformRecursive(transformed, context, appliedRules, visited);
        }

        return transformed;
    }

    private Expression TransformBinary(BinaryExpression expr, CanonicalizationContext context, List<string> appliedRules, HashSet<int> visited)
    {
        var left = TransformRecursive(expr.Left, context, appliedRules, visited);
        var right = TransformRecursive(expr.Right, context, appliedRules, visited);

        var binary = new BinaryExpression(expr.Operator, left, right);

        if (context.FlattenAssociative && CanonicalizationRules.IsAssociativeOp(expr.Operator))
        {
            var flattened = CanonicalizationRules.FlattenAssociative(binary);
            if (flattened is BinaryExpression binaryFlattened)
            {
                binary = binaryFlattened;
                if (!binary.Equals(expr)) appliedRules.Add("FlattenAssociative");
            }
        }

        if (context.SortCommutative && CanonicalizationRules.IsCommutativeOp(expr.Operator))
        {
            var sorted = SortCommutative(binary);
            if (!sorted.Equals(binary))
            {
                binary = sorted;
                appliedRules.Add("SortCommutative");
            }
        }

        if (context.NormalizeNegation)
        {
            var normalized = CanonicalizationRules.NormalizeNegation(binary);
            if (!normalized.Equals(binary))
            {
                if (normalized is BinaryExpression binNorm)
                {
                    binary = binNorm;
                    appliedRules.Add("NormalizeNegation");
                }
            }
        }

        if (context.NormalizeDivision && expr.Operator.Equals(MathOperator.Divide))
        {
            var normalized = CanonicalizationRules.NormalizeDivision(binary);
            if (!normalized.Equals(binary))
            {
                if (normalized is BinaryExpression binNorm)
                {
                    binary = binNorm;
                    appliedRules.Add("NormalizeDivision");
                }
            }
        }

        if (context.NormalizePower && expr.Operator.Equals(MathOperator.Power))
        {
            var normalized = CanonicalizationRules.NormalizePower(binary);
            if (!normalized.Equals(binary))
            {
                if (normalized is BinaryExpression binNorm)
                {
                    binary = binNorm;
                    appliedRules.Add("NormalizePower");
                }
            }
        }

        if (context.CollectLikeTerms && expr.Operator.Equals(MathOperator.Add))
        {
            var collected = CanonicalizationRules.CollectLikeTerms(binary);
            if (!collected.Equals(binary))
            {
                if (collected is BinaryExpression binNorm)
                {
                    binary = binNorm;
                    appliedRules.Add("CollectLikeTerms");
                }
            }
        }

        return binary;
    }

    private Expression TransformUnary(UnaryExpression expr, CanonicalizationContext context, List<string> appliedRules, HashSet<int> visited)
    {
        var operand = TransformRecursive(expr.Operand, context, appliedRules, visited);
        return new UnaryExpression(expr.Operator, operand);
    }

    private Expression TransformFunctionCall(FunctionCallExpression expr, CanonicalizationContext context, List<string> appliedRules, HashSet<int> visited)
    {
        var args = expr.Arguments.Select(a => TransformRecursive(a, context, appliedRules, visited)).ToArray();

        var canonicalized = CanonicalizationRules.CanonicalizeFunctionArgs(new FunctionCallExpression(expr.Name, args));
        if (!canonicalized.Equals(expr))
            appliedRules.Add($"CanonicalizeFunctionArgs:{expr.Name}");

        return canonicalized;
    }

    private BinaryExpression SortCommutative(BinaryExpression expr)
    {
        var leftKey = CanonicalizationRules.GetSortKey(expr.Left);
        var rightKey = CanonicalizationRules.GetSortKey(expr.Right);

        if (string.CompareOrdinal(leftKey, rightKey) > 0)
            return new BinaryExpression(expr.Operator, expr.Right, expr.Left);

        return expr;
    }

    private string GetRuleName(Expression original, Expression transformed)
    {
        if (original is BinaryExpression ob && transformed is BinaryExpression tb)
        {
            if (!ob.Operator.Equals(tb.Operator)) return $"OperatorChange:{ob.Operator.Symbol}->{tb.Operator.Symbol}";
        }
        return "Transform";
    }

    public Expression Visit(LiteralExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(VariableExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(ConstantExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(BinaryExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(UnaryExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(FunctionCallExpression expression) => Transform(expression, CanonicalizationContext.Default, []);
    public Expression Visit(LambdaExpression expression) => expression;
    public Expression Visit(ParameterExpression expression) => expression;
    public Expression Visit(EquationExpression expression) => expression;
    public Expression Visit(PiecewiseExpression expression) => expression;
    public Expression Visit(ConditionalExpression expression) => expression;
    public Expression Visit(TupleExpression expression) => expression;
    public Expression Visit(VectorExpression expression) => expression;
    public Expression Visit(MatrixExpression expression) => expression;
    public Expression Visit(TensorExpression expression) => expression;
    public Expression Visit(IndexExpression expression) => expression;
    public Expression Visit(SliceExpression expression) => expression;
    public Expression Visit(DerivativeExpression expression) => expression;
    public Expression Visit(IntegralExpression expression) => expression;
    public Expression Visit(SummationExpression expression) => expression;
    public Expression Visit(ProductExpression expression) => expression;
    public Expression Visit(LimitExpression expression) => expression;
    public Expression Visit(FactorialExpression expression) => expression;
    public Expression Visit(RangeExpression expression) => expression;
    public Expression Visit(IntervalExpression expression) => expression;
    public Expression Visit(SetExpression expression) => expression;
    public Expression Visit(ComplexExpression expression) => expression;
    public Expression Visit(PolynomialExpression expression) => expression;
    public Expression Visit(BooleanExpression expression) => expression;
    public Expression Visit(RelationExpression expression) => expression;
    public Expression Visit(AssignmentExpression expression) => expression;
    public Expression Visit(CompositionExpression expression) => expression;
    public Expression Visit(IdentityExpression expression) => expression;
    public Expression Visit(NullExpression expression) => expression;
    public Expression Visit(AnnotatedExpression expression) => expression;
}