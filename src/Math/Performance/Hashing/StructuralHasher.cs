namespace MathVerse.Math.Performance.Hashing;

/// <summary>
/// Computes structural hash codes for expression trees by walking all nodes.
/// </summary>
public sealed class StructuralHasher
{
    /// <summary>
    /// Computes a structural hash code for the given expression tree.
    /// </summary>
    /// <param name="expression">The expression to hash.</param>
    /// <returns>The structural hash code.</returns>
    public int Hash(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return expression.GetHashCode();
    }

    /// <summary>
    /// Computes a structural hash code for a collection of expressions.
    /// </summary>
    /// <param name="expressions">The expressions to hash.</param>
    /// <returns>The combined structural hash code.</returns>
    public int HashRange(IReadOnlyList<Expression> expressions)
    {
        ArgumentNullException.ThrowIfNull(expressions);

        var builder = new HashBuilder();
        foreach (var expr in expressions)
            builder.Add(expr.GetHashCode());

        return builder.ToHashCode();
    }
}
