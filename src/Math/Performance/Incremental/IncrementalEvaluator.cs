namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Evaluates expressions incrementally by caching results and tracking dependencies.
/// Only recomputes expressions whose inputs have changed.
/// </summary>
public sealed class IncrementalEvaluator
{
    private readonly DependencyTracker _tracker = new();
    private readonly ConcurrentDictionary<int, Expression> _evaluationCache = new();
    private readonly ConcurrentDictionary<Expression, int> _expressionToNode = new();
    private int _nextId;

    /// <summary>Gets the current number of cached evaluation results.</summary>
    public int CacheSize => _evaluationCache.Count;

    /// <summary>Evaluates the given expression, returning a cached result if available.</summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <returns>The evaluated (possibly simplified) expression.</returns>
    public Expression Evaluate(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        if (_expressionToNode.TryGetValue(expr, out var nodeId))
        {
            var node = _tracker.GetNode(nodeId);
            if (node is not null && !node.IsDirty && _evaluationCache.TryGetValue(nodeId, out var cached))
                return cached;
        }

        var id = Interlocked.Increment(ref _nextId);
        _expressionToNode[expr] = id;
        _tracker.AddNode(expr.ToString());

        var result = SimplifyExpression(expr);
        _evaluationCache[id] = result;

        var nodeRef = _tracker.GetNode(id);
        nodeRef?.MarkClean();

        return result;
    }

    /// <summary>Invalidates the cached result for the specified expression.</summary>
    /// <param name="expr">The expression to invalidate.</param>
    public void Invalidate(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        if (_expressionToNode.TryGetValue(expr, out var nodeId))
        {
            _tracker.MarkDirty(nodeId);
            _evaluationCache.TryRemove(nodeId, out _);
        }
    }

    /// <summary>Invalidates all cached results.</summary>
    public void InvalidateAll()
    {
        _evaluationCache.Clear();
        _expressionToNode.Clear();
    }

    private static Expression SimplifyExpression(Expression expr)
    {
        return expr.Kind switch
        {
            ExpressionKind.Binary => SimplifyBinary((BinaryExpression)expr),
            ExpressionKind.Unary => SimplifyUnary((UnaryExpression)expr),
            ExpressionKind.Literal => expr,
            ExpressionKind.Variable => expr,
            ExpressionKind.Constant => expr,
            _ => expr
        };
    }

    private static Expression SimplifyBinary(BinaryExpression binary)
    {
        if (binary.Left is LiteralExpression leftLit && binary.Right is LiteralExpression rightLit)
        {
            return binary.Operator.Name switch
            {
                "Add" => new LiteralExpression(leftLit.Value + rightLit.Value),
                "Subtract" => new LiteralExpression(leftLit.Value - rightLit.Value),
                "Multiply" => new LiteralExpression(leftLit.Value * rightLit.Value),
                "Divide" => new LiteralExpression(leftLit.Value / rightLit.Value),
                "Power" => new LiteralExpression(System.Math.Pow(leftLit.Value, rightLit.Value)),
                "Modulo" => new LiteralExpression(leftLit.Value % rightLit.Value),
                _ => binary
            };
        }

        if (binary.Operator.Name is "Add" or "Multiply")
        {
            if (IsZero(binary.Right) && binary.Operator.Name == "Add")
                return binary.Left;

            if (IsZero(binary.Left) && binary.Operator.Name == "Add")
                return binary.Right;

            if (IsOne(binary.Right) && binary.Operator.Name == "Multiply")
                return binary.Left;

            if (IsOne(binary.Left) && binary.Operator.Name == "Multiply")
                return binary.Right;

            if (IsZero(binary.Right) && binary.Operator.Name == "Multiply")
                return new LiteralExpression(0.0);

            if (IsZero(binary.Left) && binary.Operator.Name == "Multiply")
                return new LiteralExpression(0.0);
        }

        return binary;
    }

    private static Expression SimplifyUnary(UnaryExpression unary)
    {
        if (unary.Operand is LiteralExpression lit && unary.Operator.Name == "Negate")
            return new LiteralExpression(-lit.Value);

        return unary;
    }

    private static bool IsZero(Expression expr) =>
        expr is LiteralExpression l && l.Value == 0.0;

    private static bool IsOne(Expression expr) =>
        expr is LiteralExpression l && l.Value == 1.0;
}
