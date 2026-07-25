namespace MathVerse.Math.Types.Algebra;

/// <summary>Base class for all algebraic structures. Each structure describes properties
/// of operations on a set.</summary>
public abstract class AlgebraicStructure : IEquatable<AlgebraicStructure>
{
    /// <summary>The kind of algebraic structure.</summary>
    public abstract AlgebraicStructureKind Kind { get; }

    /// <summary>The underlying element type.</summary>
    public MathType ElementType { get; }

    /// <summary>Whether the primary operation is associative.</summary>
    public virtual bool IsAssociative => false;

    /// <summary>Whether the primary operation is commutative.</summary>
    public virtual bool IsCommutative => false;

    /// <summary>Whether the structure has an identity element.</summary>
    public virtual bool HasIdentity => false;

    /// <summary>Whether every element has an inverse.</summary>
    public virtual bool HasInverses => false;

    /// <summary>Whether multiplication distributes over addition.</summary>
    public virtual bool IsDistributive => false;

    /// <summary>Whether there are zero divisors.</summary>
    public virtual bool HasZeroDivisors => true;

    /// <summary>Creates an algebraic structure on the given element type.</summary>
    protected AlgebraicStructure(MathType elementType)
    {
        ElementType = elementType;
    }

    /// <inheritdoc/>
    public abstract bool Equals(AlgebraicStructure? other);

    /// <inheritdoc/>
    public abstract override int GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AlgebraicStructure);

    /// <inheritdoc/>
    public override string ToString() => $"{Kind}({ElementType.Name})";
}
