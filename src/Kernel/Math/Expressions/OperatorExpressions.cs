namespace MathVerse.Math.Expressions;

/// <summary>
/// Represents a binary operation (e.g., addition, multiplication, power).
/// </summary>
public sealed class BinaryExpression : Expression
{
    /// <summary>Initializes a binary expression.</summary>
    public BinaryExpression(MathOperator op, Expression left, Expression right)
        : base(ExpressionKind.Binary, 1 + System.Math.Max(left.Depth, right.Depth), 1 + left.NodeCount + right.NodeCount)
    {
        Operator = Guard.NotNull(op, nameof(op));
        Left = Guard.NotNull(left, nameof(left));
        Right = Guard.NotNull(right, nameof(right));
    }

    /// <summary>Gets the binary operator.</summary>
    public MathOperator Operator { get; }

    /// <summary>Gets the left operand.</summary>
    public Expression Left { get; }

    /// <summary>Gets the right operand.</summary>
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
        other is BinaryExpression b &&
        Operator.Equals(b.Operator) &&
        Left.Equals(b.Left) &&
        Right.Equals(b.Right);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Binary, Operator, Left, Right);
}

/// <summary>
/// Represents a unary operation (e.g., negation, absolute value).
/// </summary>
public sealed class UnaryExpression : Expression
{
    /// <summary>Initializes a unary expression.</summary>
    public UnaryExpression(MathOperator op, Expression operand)
        : base(ExpressionKind.Unary, 1 + operand.Depth, 1 + operand.NodeCount)
    {
        Operator = Guard.NotNull(op, nameof(op));
        Operand = Guard.NotNull(operand, nameof(operand));
    }

    /// <summary>Gets the unary operator.</summary>
    public MathOperator Operator { get; }

    /// <summary>Gets the operand.</summary>
    public Expression Operand { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Expression> Children => [Operand];

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
        other is UnaryExpression u &&
        Operator.Equals(u.Operator) &&
        Operand.Equals(u.Operand);

    /// <inheritdoc/>
    protected override int ComputeHashCode() =>
        HashCode.Combine(ExpressionKind.Unary, Operator, Operand);
}
