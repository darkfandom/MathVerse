namespace MathVerse.Math.Performance.Interning;

/// <summary>
/// Provides identity-based comparison for interned expressions using reference equality.
/// </summary>
public sealed class ExpressionIdentity : IEqualityComparer<Expression>
{
    /// <summary>Singleton instance.</summary>
    public static readonly ExpressionIdentity Instance = new();

    private ExpressionIdentity() { }

    /// <inheritdoc/>
    public bool Equals(Expression? x, Expression? y) =>
        ReferenceEquals(x, y);

    /// <inheritdoc/>
    public int GetHashCode(Expression obj) =>
        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}
