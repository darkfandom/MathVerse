namespace MathVerse.Math.Types.Generics;

/// <summary>Enumerates type parameter variance.</summary>
public enum TypeVariance
{
    /// <summary>Neither covariant nor contravariant.</summary>
    Invariant,
    /// <summary>Covariant (out).</summary>
    Covariant,
    /// <summary>Contravariant (in).</summary>
    Contravariant,
}

/// <summary>A generic constraint applied to a type parameter.</summary>
public sealed class GenericConstraint : IEquatable<GenericConstraint>
{
    /// <summary>The constraint kind.</summary>
    public GenericConstraintKind Kind { get; }

    /// <summary>The type argument for type constraints.</summary>
    public MathType? Type { get; }

    /// <summary>Creates a generic constraint.</summary>
    public GenericConstraint(GenericConstraintKind kind, MathType? type = null)
    {
        Kind = kind;
        Type = type;
    }

    /// <inheritdoc/>
    public bool Equals(GenericConstraint? other)
    {
        if (other is null) return false;
        if (other.Kind != Kind) return false;
        if (Kind == GenericConstraintKind.TypeConstraint && Type is not null)
            return Type.Equals(other.Type);
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as GenericConstraint);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, Type);
}

/// <summary>Enumerates generic constraint kinds.</summary>
public enum GenericConstraintKind
{
    /// <summary>Class constraint.</summary>
    Class,
    /// <summary>Struct constraint.</summary>
    Struct,
    /// <summary>Not null constraint.</summary>
    NotNull,
    /// <summary>Type constraint (must be or derive from a specific type).</summary>
    TypeConstraint,
    /// <summary>Has a default constructor.</summary>
    AllowsDefault,
    /// <summary>Must be a numeric type.</summary>
    Numeric,
    /// <summary>Must support addition.</summary>
    Additive,
    /// <summary>Must support multiplication.</summary>
    Multiplicative,
    /// <summary>Must form a field.</summary>
    FieldConstraint,
}
