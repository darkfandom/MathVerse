namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a function call expression.
/// </summary>
public sealed class FunctionCallExpression : Expression
{
    /// <summary>Initializes a function call expression.</summary>
    public FunctionCallExpression(string name, IReadOnlyList<Expression> arguments)
        : base(ExpressionKind.FunctionCall, ComputeDepth(arguments), ComputeNodeCount(arguments))
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Arguments = arguments.ToArray();
    }

    /// <summary>Gets the function name.</summary>
    public string Name { get; }

    /// <summary>Gets the function arguments.</summary>
    public IReadOnlyList<Expression> Arguments { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => Arguments;

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
        if (other is not FunctionCallExpression f || Name != f.Name || Arguments.Count != f.Arguments.Count)
            return false;

        for (var i = 0; i < Arguments.Count; i++)
        {
            if (!Arguments[i].Equals(f.Arguments[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.FunctionCall);
        hash.Add(Name);
        foreach (var arg in Arguments)
            hash.Add(arg);
        return hash.ToHashCode();
    }

    private static int ComputeDepth(IReadOnlyList<Expression> args)
    {
        var max = 0;
        foreach (var arg in args)
            if (arg.Depth > max) max = arg.Depth;
        return 1 + max;
    }

    private static int ComputeNodeCount(IReadOnlyList<Expression> args)
    {
        var count = 1;
        foreach (var arg in args)
            count += arg.NodeCount;
        return count;
    }
}

/// <summary>
/// Represents a lambda (anonymous function) expression.
/// </summary>
public sealed class LambdaExpression : Expression
{
    /// <summary>Initializes a lambda expression.</summary>
    public LambdaExpression(IReadOnlyList<ParameterExpression> parameters, Expression body)
        : base(ExpressionKind.Lambda, 1 + body.Depth, 1 + body.NodeCount + parameters.Count)
    {
        Parameters = parameters.ToArray();
        Body = Guard.NotNull(body, nameof(body));
    }

    /// <summary>Gets the lambda parameters.</summary>
    public IReadOnlyList<ParameterExpression> Parameters { get; }

    /// <summary>Gets the lambda body.</summary>
    public Expression Body { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children
    {
        get
        {
            var list = new List<Expression>(Parameters) { Body };
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
        if (other is not LambdaExpression l || Parameters.Count != l.Parameters.Count)
            return false;

        for (var i = 0; i < Parameters.Count; i++)
        {
            if (!Parameters[i].Equals(l.Parameters[i]))
                return false;
        }

        return Body.Equals(l.Body);
    }

    /// <inheritdoc/>
    protected override int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ExpressionKind.Lambda);
        foreach (var p in Parameters)
            hash.Add(p);
        hash.Add(Body);
        return hash.ToHashCode();
    }
}

/// <summary>
/// Represents a named parameter in a lambda expression.
/// </summary>
public sealed class ParameterExpression : Expression
{
    /// <summary>Initializes a parameter expression.</summary>
    public ParameterExpression(string name)
        : base(ExpressionKind.Parameter, 0, 1)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
    }

    /// <summary>Gets the parameter name.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [];

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
        other is ParameterExpression p && Name == p.Name;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Parameter, Name);
}

/// <summary>
/// Represents an equation expression (left = right).
/// </summary>
public sealed class EquationExpression : Expression
{
    /// <summary>Initializes an equation expression.</summary>
    public EquationExpression(Expression left, Expression right)
        : base(ExpressionKind.Equation, 1 + System.Math.Max(left.Depth, right.Depth), 1 + left.NodeCount + right.NodeCount)
    {
        Left = Guard.NotNull(left, nameof(left));
        Right = Guard.NotNull(right, nameof(right));
    }

    /// <summary>Gets the left-hand side.</summary>
    public Expression Left { get; }

    /// <summary>Gets the right-hand side.</summary>
    public Expression Right { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Left, Right];

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
        other is EquationExpression e && Left.Equals(e.Left) && Right.Equals(e.Right);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Equation, Left, Right);
}
