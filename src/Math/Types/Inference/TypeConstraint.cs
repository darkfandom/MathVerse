namespace MathVerse.Math.Types.Inference;

/// <summary>Enumerates type constraint kinds.</summary>
public enum TypeConstraintKind
{
    /// <summary>Two types must be equal.</summary>
    Equality,
    /// <summary>A type must be a subtype of another.</summary>
    Subtype,
    /// <summary>A type must implement an algebraic structure.</summary>
    Structural,
    /// <summary>A type must be numeric.</summary>
    Numeric,
    /// <summary>A type must be a function type.</summary>
    Callable,
    /// <summary>A type must support a specific operator.</summary>
    Operator,
    /// <summary>Arity constraint on a function type.</summary>
    Arity,
}

/// <summary>Represents a type constraint generated during inference.</summary>
public sealed class TypeConstraint : IEquatable<TypeConstraint>
{
    /// <summary>The constraint kind.</summary>
    public TypeConstraintKind Kind { get; }

    /// <summary>The left-hand type (or type variable).</summary>
    public MathType Left { get; }

    /// <summary>The right-hand type (or type variable). May be null for unary constraints.</summary>
    public MathType? Right { get; }

    /// <summary>Optional description for diagnostics.</summary>
    public string? Description { get; }

    /// <summary>Source expression that generated this constraint.</summary>
    public string? SourceExpression { get; }

    /// <summary>Creates a type constraint.</summary>
    public TypeConstraint(TypeConstraintKind kind, MathType left, MathType? right = null,
        string? description = null, string? sourceExpression = null)
    {
        Kind = kind;
        Left = left;
        Right = right;
        Description = description;
        SourceExpression = sourceExpression;
    }

    /// <inheritdoc/>
    public bool Equals(TypeConstraint? other)
    {
        if (other is null) return false;
        return other.Kind == Kind
            && other.Left.Equals(Left)
            && Equals(other.Right, Right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TypeConstraint);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, Left, Right);

    /// <inheritdoc/>
    public override string ToString()
    {
        return Kind switch
        {
            TypeConstraintKind.Equality => $"{Left} = {Right}",
            TypeConstraintKind.Subtype => $"{Left} <: {Right}",
            TypeConstraintKind.Numeric => $"numeric({Left})",
            TypeConstraintKind.Callable => $"{Left} callable",
            TypeConstraintKind.Arity => $"{Left} arity {Right}",
            _ => $"{Kind}({Left}, {Right})",
        };
    }
}
