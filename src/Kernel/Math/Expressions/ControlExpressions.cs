namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a single case in a piecewise expression.
/// </summary>
public sealed record PiecewiseCase(Expression Value, Expression Condition);

/// <summary>
/// Represents a piecewise-defined expression with cases and an optional default.
/// </summary>
public sealed class PiecewiseExpression : Expression
{
    /// <summary>Initializes a piecewise expression.</summary>
    public PiecewiseExpression(IReadOnlyList<PiecewiseCase> cases, Expression? defaultCase = null)
        : base(ExpressionKind.Piecewise, ComputeDepth(cases, defaultCase), ComputeNodeCount(cases, defaultCase))
    {
        Cases = cases.ToArray();
        DefaultCase = defaultCase;
    }

    /// <summary>Gets the piecewise cases.</summary>
    public IReadOnlyList<PiecewiseCase> Cases { get; }

    /// <summary>Gets the optional default case.</summary>
    public Expression? DefaultCase { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression>();
            foreach (var c in Cases)
            {
                list.Add(c.Value);
                list.Add(c.Condition);
            }
            if (DefaultCase is not null) list.Add(DefaultCase);
            return list;
        }
    }

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other)
    {
        if (other is not PiecewiseExpression pw || Cases.Count != pw.Cases.Count)
            return false;

        for (var i = 0; i < Cases.Count; i++)
        {
            if (!Cases[i].Value.Equals(pw.Cases[i].Value) || !Cases[i].Condition.Equals(pw.Cases[i].Condition))
                return false;
        }

        return Equals(DefaultCase, pw.DefaultCase);
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Piecewise);
        foreach (var c in Cases)
        {
            hash.Add(c.Value);
            hash.Add(c.Condition);
        }
        hash.Add(DefaultCase);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<PiecewiseCase> cases, Expression? def)
    {
        var max = 0;
        foreach (var c in cases)
        {
            if (c.Value.Depth > max) max = c.Value.Depth;
            if (c.Condition.Depth > max) max = c.Condition.Depth;
        }
        if (def is not null && def.Depth > max) max = def.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<PiecewiseCase> cases, Expression? def)
    {
        var count = 1;
        foreach (var c in cases)
            count += c.Value.NodeCount + c.Condition.NodeCount;
        if (def is not null) count += def.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents an if-then-else conditional expression.
/// </summary>
public sealed class ConditionalExpression : Expression
{
    /// <summary>Initializes a conditional expression.</summary>
    public ConditionalExpression(Expression condition, Expression thenBranch, Expression elseBranch)
        : base(ExpressionKind.Conditional,
            1 + System.Math.Max(condition.Depth, System.Math.Max(thenBranch.Depth, elseBranch.Depth)),
            1 + condition.NodeCount + thenBranch.NodeCount + elseBranch.NodeCount)
    {
        Condition = Guard.NotNull(condition, nameof(condition));
        ThenBranch = Guard.NotNull(thenBranch, nameof(thenBranch));
        ElseBranch = Guard.NotNull(elseBranch, nameof(elseBranch));
    }

    /// <summary>Gets the condition.</summary>
    public Expression Condition { get; }

    /// <summary>Gets the then branch.</summary>
    public Expression ThenBranch { get; }

    /// <summary>Gets the else branch.</summary>
    public Expression ElseBranch { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Condition, ThenBranch, ElseBranch];

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other) =>
        other is ConditionalExpression c &&
        Condition.Equals(c.Condition) &&
        ThenBranch.Equals(c.ThenBranch) &&
        ElseBranch.Equals(c.ElseBranch);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Conditional, Condition, ThenBranch, ElseBranch);
}

/// <summary>
/// Represents an ordered tuple of expressions.
/// </summary>
public sealed class TupleExpression : Expression
{
    /// <summary>Initializes a tuple expression.</summary>
    public TupleExpression(IReadOnlyList<Expression> elements)
        : base(ExpressionKind.Tuple, ComputeDepth(elements), ComputeNodeCount(elements))
    {
        Elements = elements.ToArray();
    }

    /// <summary>Gets the tuple elements.</summary>
    public IReadOnlyList<Expression> Elements { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Elements;

    /// <inheritdoc/>
    public override Expression Accept(IExpressionTransformer transformer) =>
        transformer.Visit(this);

    /// <inheritdoc/>
    public override T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override void Accept(IExpressionVisitor visitor) =>
        visitor.Visit(this);

    /// <inheritdoc/>
    public override bool Equals(Expression? other)
    {
        if (other is not TupleExpression t || Elements.Count != t.Elements.Count)
            return false;

        for (var i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Equals(t.Elements[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Tuple);
        foreach (var e in Elements)
            hash.Add(e);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<Expression> elements)
    {
        var max = 0;
        foreach (var e in elements)
            if (e.Depth > max) max = e.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> elements)
    {
        var count = 1;
        foreach (var e in elements)
            count += e.NodeCount;
        return count;
    }
}
