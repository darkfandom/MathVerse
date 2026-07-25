namespace MathVerse.Math.Visitors;

/// <summary>
/// Counts the total number of nodes in an expression tree.
/// </summary>
public sealed class ExpressionNodeCounter : IExpressionVisitor<int>
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionNodeCounter Instance = new();

    private ExpressionNodeCounter() { }

    /// <summary>Counts the nodes in the expression.</summary>
    public static int Count(Expression expression) => expression.NodeCount;

    private int CountChildren(IReadOnlyList<Expression> children)
    {
        var count = 1;
        foreach (var child in children)
            count += child.Accept(this);
        return count;
    }

    /// <inheritdoc/>
    public int Visit(LiteralExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(VariableExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(ConstantExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(BinaryExpression expression) =>
        1 + expression.Left.Accept(this) + expression.Right.Accept(this);

    /// <inheritdoc/>
    public int Visit(UnaryExpression expression) =>
        1 + expression.Operand.Accept(this);

    /// <inheritdoc/>
    public int Visit(FunctionCallExpression expression) => CountChildren(expression.Arguments);

    /// <inheritdoc/>
    public int Visit(LambdaExpression expression) =>
        1 + expression.Parameters.Count + expression.Body.Accept(this);

    /// <inheritdoc/>
    public int Visit(ParameterExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(EquationExpression expression) =>
        1 + expression.Left.Accept(this) + expression.Right.Accept(this);

    /// <inheritdoc/>
    public int Visit(PiecewiseExpression expression)
    {
        var count = 1;
        foreach (var c in expression.Cases)
            count += c.Value.Accept(this) + c.Condition.Accept(this);
        if (expression.DefaultCase is not null)
            count += expression.DefaultCase.Accept(this);
        return count;
    }

    /// <inheritdoc/>
    public int Visit(ConditionalExpression expression) =>
        1 + expression.Condition.Accept(this) + expression.ThenBranch.Accept(this) + expression.ElseBranch.Accept(this);

    /// <inheritdoc/>
    public int Visit(TupleExpression expression) => CountChildren(expression.Elements);

    /// <inheritdoc/>
    public int Visit(VectorExpression expression) => CountChildren(expression.Components);

    /// <inheritdoc/>
    public int Visit(MatrixExpression expression) => CountChildren(expression.Rows);

    /// <inheritdoc/>
    public int Visit(TensorExpression expression) => CountChildren(expression.Components);

    /// <inheritdoc/>
    public int Visit(IndexExpression expression)
    {
        var count = 1 + expression.Target.Accept(this);
        foreach (var idx in expression.Indices)
            count += idx.Accept(this);
        return count;
    }

    /// <inheritdoc/>
    public int Visit(SliceExpression expression) =>
        1 + expression.Target.Accept(this);

    /// <inheritdoc/>
    public int Visit(DerivativeExpression expression) =>
        1 + expression.Function.Accept(this) + expression.Variable.Accept(this);

    /// <inheritdoc/>
    public int Visit(IntegralExpression expression)
    {
        var count = 1 + expression.Integrand.Accept(this) + expression.Variable.Accept(this);
        if (expression.LowerBound is not null) count += expression.LowerBound.Accept(this);
        if (expression.UpperBound is not null) count += expression.UpperBound.Accept(this);
        return count;
    }

    /// <inheritdoc/>
    public int Visit(SummationExpression expression) =>
        1 + expression.Variable.Accept(this) + expression.LowerBound.Accept(this) + expression.UpperBound.Accept(this) + expression.Body.Accept(this);

    /// <inheritdoc/>
    public int Visit(ProductExpression expression) =>
        1 + expression.Variable.Accept(this) + expression.LowerBound.Accept(this) + expression.UpperBound.Accept(this) + expression.Body.Accept(this);

    /// <inheritdoc/>
    public int Visit(LimitExpression expression) =>
        1 + expression.Body.Accept(this) + expression.Variable.Accept(this) + expression.Target.Accept(this);

    /// <inheritdoc/>
    public int Visit(FactorialExpression expression) =>
        1 + expression.Operand.Accept(this);

    /// <inheritdoc/>
    public int Visit(RangeExpression expression)
    {
        var count = 1 + expression.Start.Accept(this) + expression.End.Accept(this);
        if (expression.Step is not null) count += expression.Step.Accept(this);
        return count;
    }

    /// <inheritdoc/>
    public int Visit(IntervalExpression expression) =>
        1 + expression.Lower.Accept(this) + expression.Upper.Accept(this);

    /// <inheritdoc/>
    public int Visit(SetExpression expression) => CountChildren(expression.Elements);

    /// <inheritdoc/>
    public int Visit(ComplexExpression expression) =>
        1 + expression.Real.Accept(this) + expression.Imaginary.Accept(this);

    /// <inheritdoc/>
    public int Visit(PolynomialExpression expression) => CountChildren(expression.Coefficients) + 1;

    /// <inheritdoc/>
    public int Visit(BooleanExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(RelationExpression expression) =>
        1 + expression.Left.Accept(this) + expression.Right.Accept(this);

    /// <inheritdoc/>
    public int Visit(AssignmentExpression expression) =>
        1 + expression.Target.Accept(this) + expression.Value.Accept(this);

    /// <inheritdoc/>
    public int Visit(CompositionExpression expression) => CountChildren(expression.Functions);

    /// <inheritdoc/>
    public int Visit(IdentityExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(NullExpression expression) => 1;

    /// <inheritdoc/>
    public int Visit(AnnotatedExpression expression) =>
        1 + expression.Inner.Accept(this);
}

/// <summary>
/// Collects statistics about an expression tree.
/// </summary>
public sealed class ExpressionStatisticsVisitor : ExpressionWalker
{
    private int _nodeCount;
    private int _maxDepth;
    private int _currentDepth;
    private readonly Dictionary<ExpressionKind, int> _kindCounts = new();
    private readonly HashSet<string> _variables = new(StringComparer.Ordinal);
    private readonly HashSet<string> _functions = new(StringComparer.Ordinal);

    /// <summary>Gets the total node count.</summary>
    public int NodeCount => _nodeCount;

    /// <summary>Gets the maximum depth.</summary>
    public int MaxDepth => _maxDepth;

    /// <summary>Gets counts per expression kind.</summary>
    public IReadOnlyDictionary<ExpressionKind, int> KindCounts => _kindCounts;

    /// <summary>Gets the set of variable names.</summary>
    public IReadOnlySet<string> Variables => _variables;

    /// <summary>Gets the set of function names.</summary>
    public IReadOnlySet<string> Functions => _functions;

    /// <summary>Collects statistics for the expression.</summary>
    public static ExpressionStatisticsVisitor Collect(Expression expression)
    {
        var visitor = new ExpressionStatisticsVisitor();
        expression.Accept(visitor);
        return visitor;
    }

    private void Track(Expression expression)
    {
        _nodeCount++;
        _currentDepth++;
        if (_currentDepth > _maxDepth) _maxDepth = _currentDepth;

        _kindCounts.TryGetValue(expression.Kind, out var count);
        _kindCounts[expression.Kind] = count + 1;
    }

    /// <inheritdoc/>
    public override void Visit(LiteralExpression expression) { Track(expression); }

    /// <inheritdoc/>
    public override void Visit(VariableExpression expression)
    {
        Track(expression);
        _variables.Add(expression.Name);
    }

    /// <inheritdoc/>
    public override void Visit(ConstantExpression expression) => Track(expression);

    /// <inheritdoc/>
    public override void Visit(BinaryExpression expression)
    {
        Track(expression);
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(UnaryExpression expression)
    {
        Track(expression);
        expression.Operand.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(FunctionCallExpression expression)
    {
        Track(expression);
        _functions.Add(expression.Name);
        foreach (var arg in expression.Arguments)
            arg.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(LambdaExpression expression)
    {
        Track(expression);
        expression.Body.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(ParameterExpression expression)
    {
        Track(expression);
        _variables.Add(expression.Name);
    }

    /// <inheritdoc/>
    public override void Visit(EquationExpression expression)
    {
        Track(expression);
        expression.Left.Accept(this);
        expression.Right.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(PiecewiseExpression expression)
    {
        Track(expression);
        foreach (var c in expression.Cases)
        {
            c.Value.Accept(this);
            c.Condition.Accept(this);
        }
        expression.DefaultCase?.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(ConditionalExpression expression)
    {
        Track(expression);
        expression.Condition.Accept(this);
        expression.ThenBranch.Accept(this);
        expression.ElseBranch.Accept(this);
    }

    /// <inheritdoc/>
    public override void Visit(TupleExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(VectorExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(MatrixExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(TensorExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(IndexExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(SliceExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(DerivativeExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(IntegralExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(SummationExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(ProductExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(LimitExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(FactorialExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(RangeExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(IntervalExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(SetExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(ComplexExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(PolynomialExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(BooleanExpression expression) => Track(expression);

    /// <inheritdoc/>
    public override void Visit(RelationExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(AssignmentExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(CompositionExpression expression) { Track(expression); base.Visit(expression); }

    /// <inheritdoc/>
    public override void Visit(IdentityExpression expression) => Track(expression);

    /// <inheritdoc/>
    public override void Visit(NullExpression expression) => Track(expression);

    /// <inheritdoc/>
    public override void Visit(AnnotatedExpression expression) { Track(expression); base.Visit(expression); }
}
