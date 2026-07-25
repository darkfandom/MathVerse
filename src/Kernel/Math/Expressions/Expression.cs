namespace MathVerse.Math.Expressions;

/// <summary>
/// Abstract base class for all mathematical expression nodes.
/// All expressions are immutable, thread-safe, and AOT-compatible.
/// </summary>
public abstract class Expression : IEquatable<Expression>
{
    private int? _hashCode;
    private string? _cachedToString;

    /// <summary>Initializes a new expression.</summary>
    protected Expression(ExpressionKind kind, int depth, int nodeCount)
    {
        Kind = kind;
        Depth = depth;
        NodeCount = nodeCount;
        NodeId = Interlocked.Increment(ref _nextNodeId);
    }

    private static int _nextNodeId;

    /// <summary>Gets the kind of this expression node.</summary>
    public ExpressionKind Kind { get; }

    /// <summary>Gets the depth of the expression tree.</summary>
    public int Depth { get; }

    /// <summary>Gets the total number of nodes in this expression tree.</summary>
    public int NodeCount { get; }

    /// <summary>Gets the unique node identifier.</summary>
    public int NodeId { get; }

    /// <summary>Gets the child expressions.</summary>
    public abstract IReadOnlyList<Expression> Children { get; }

    /// <summary>Accepts a visitor for double dispatch.</summary>
    public abstract Expression Accept(IExpressionTransformer transformer);

    /// <summary>Accepts a visitor that returns a value.</summary>
    public abstract T Accept<T>(IExpressionVisitor<T> visitor);

    /// <summary>Accepts a void visitor.</summary>
    public abstract void Accept(IExpressionVisitor visitor);

    /// <inheritdoc/>
    public abstract bool Equals(Expression? other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Expression other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        _hashCode ??= ComputeHashCode();
        return _hashCode.Value;
    }

    /// <summary>Computes the structural hash code.</summary>
    protected abstract int ComputeHashCode();

    /// <inheritdoc/>
    public override string ToString()
    {
        _cachedToString ??= ExpressionPrinter.Print(this);
        return _cachedToString;
    }

    /// <summary>Returns a new expression with the specified annotations.</summary>
    public Expression WithAnnotation(string key, object value) =>
        new AnnotatedExpression(this, key, value);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Expression? left, Expression? right) =>
        Equals(left, right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Expression? left, Expression? right) =>
        !Equals(left, right);
}
