namespace MathVerse.Math.Types.Generics;

/// <summary>A generic type parameter (e.g., T, U).</summary>
public sealed class TypeParameter : MathType
{
    /// <summary>The parameter name (e.g., "T").</summary>
    public string Name_ { get; }

    /// <summary>The constraints on this type parameter.</summary>
    public IReadOnlyList<GenericConstraint> Constraints { get; }

    /// <summary>The variance of this type parameter.</summary>
    public TypeVariance Variance { get; }

    /// <summary>Creates a type parameter.</summary>
    public TypeParameter(string name, IReadOnlyList<GenericConstraint>? constraints = null,
        TypeVariance variance = TypeVariance.Invariant)
    {
        Name_ = name;
        Constraints = constraints ?? Array.Empty<GenericConstraint>();
        Variance = variance;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Generic;

    /// <inheritdoc/>
    public override string Name => Name_;

    /// <inheritdoc/>
    public override bool IsGenericParameter => true;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) =>
        other is TypeParameter tp && tp.Name_ == Name_;

    /// <inheritdoc/>
    public override int GetHashCode() => Name_.GetHashCode();
}
