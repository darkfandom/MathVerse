namespace MathVerse.Math.Performance.Interning;

/// <summary>
/// Provides a structural equality key for an expression tree, suitable for use as a dictionary key.
/// </summary>
public sealed class ExpressionKey : IEquatable<ExpressionKey>
{
    /// <summary>
    /// Initializes a new expression key from an expression.
    /// </summary>
    /// <param name="expression">The expression to key on.</param>
    public ExpressionKey(Expression expression)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        HashCode = expression.GetHashCode();
    }

    /// <summary>Gets the underlying expression.</summary>
    public Expression Expression { get; }

    /// <summary>Gets the precomputed hash code.</summary>
    public int HashCode { get; }

    /// <inheritdoc/>
    public bool Equals(ExpressionKey? other) =>
        other is not null && Expression.Equals(other.Expression);

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        Equals(obj as ExpressionKey);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode;
}
