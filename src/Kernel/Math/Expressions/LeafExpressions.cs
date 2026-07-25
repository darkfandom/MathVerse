namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a literal numeric value in an expression tree.
/// </summary>
public sealed class LiteralExpression : Expression
{
    /// <summary>Initializes a literal expression with the specified value.</summary>
    public LiteralExpression(double value)
        : base(ExpressionKind.Literal, 0, 1)
    {
        Value = value;
    }

    /// <summary>Gets the numeric value.</summary>
    public double Value { get; }

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
        other is LiteralExpression lit && Value.Equals(lit.Value);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Literal, Value);
}

/// <summary>
/// Represents a named variable in an expression tree.
/// </summary>
public sealed class VariableExpression : Expression
{
    /// <summary>Initializes a variable expression with the specified name.</summary>
    public VariableExpression(string name)
        : base(ExpressionKind.Variable, 0, 1)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
    }

    /// <summary>Gets the variable name.</summary>
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
        other is VariableExpression v && Name == v.Name;

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Variable, Name);
}

/// <summary>
/// Represents a named mathematical constant (pi, e, infinity, etc.).
/// </summary>
public sealed class ConstantExpression : Expression
{
    /// <summary>Initializes a constant expression.</summary>
    public ConstantExpression(string name, double value)
        : base(ExpressionKind.Constant, 0, 1)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Value = value;
    }

    /// <summary>Gets the constant name.</summary>
    public string Name { get; }

    /// <summary>Gets the constant value.</summary>
    public double Value { get; }

    /// <summary>The mathematical constant pi.</summary>
    public static readonly ConstantExpression Pi = new("pi", System.Math.PI);

    /// <summary>The mathematical constant e.</summary>
    public static readonly ConstantExpression E = new("e", System.Math.E);

    /// <summary>The imaginary unit i.</summary>
    public static readonly ConstantExpression I = new("i", double.NaN);

    /// <summary>Positive infinity.</summary>
    public static readonly ConstantExpression PositiveInfinity = new("∞", double.PositiveInfinity);

    /// <summary>Negative infinity.</summary>
    public static readonly ConstantExpression NegativeInfinity = new("-∞", double.NegativeInfinity);

    /// <summary>Not a number.</summary>
    public static readonly ConstantExpression NaN = new("NaN", double.NaN);

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
        other is ConstantExpression c && Name == c.Name && Value.Equals(c.Value);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Constant, Name, Value);
}
