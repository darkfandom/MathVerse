namespace MathVerse.Math.Types.Algebra;

/// <summary>A magma: a set with a closed binary operation.</summary>
public sealed class Magma : AlgebraicStructure
{
    /// <summary>Creates a magma.</summary>
    public Magma(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Magma;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Magma m && m.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A semigroup: an associative magma.</summary>
public sealed class Semigroup : AlgebraicStructure
{
    /// <summary>Creates a semigroup.</summary>
    public Semigroup(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Semigroup;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Semigroup s && s.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A monoid: a semigroup with an identity element.</summary>
public sealed class Monoid : AlgebraicStructure
{
    /// <summary>Creates a monoid.</summary>
    public Monoid(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Monoid;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Monoid m && m.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A group: a monoid where every element has an inverse.</summary>
public sealed class Group : AlgebraicStructure
{
    /// <summary>Whether this group is abelian (commutative).</summary>
    public bool IsAbelianGroup { get; }

    /// <summary>Creates a group.</summary>
    public Group(MathType elementType, bool isAbelian = false) : base(elementType)
    {
        IsAbelianGroup = isAbelian;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind =>
        IsAbelianGroup ? AlgebraicStructureKind.AbelianGroup : AlgebraicStructureKind.Group;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool HasInverses => true;

    /// <inheritdoc/>
    public override bool IsCommutative => IsAbelianGroup;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Group g && g.ElementType.Equals(ElementType) && g.IsAbelianGroup == IsAbelianGroup;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, IsAbelianGroup);
}

/// <summary>An abelian group: a commutative group.</summary>
public sealed class AbelianGroup : AlgebraicStructure
{
    /// <summary>Creates an abelian group.</summary>
    public AbelianGroup(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.AbelianGroup;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool HasInverses => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is AbelianGroup ag && ag.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A ring: an abelian group under addition and a monoid under multiplication.</summary>
public sealed class Ring : AlgebraicStructure
{
    /// <summary>Whether this ring is commutative.</summary>
    public bool IsCommutativeRing { get; }

    /// <summary>Whether this ring has a multiplicative identity (unital).</summary>
    public bool IsUnital { get; }

    /// <summary>Creates a ring.</summary>
    public Ring(MathType elementType, bool isCommutative = false, bool isUnital = true)
        : base(elementType)
    {
        IsCommutativeRing = isCommutative;
        IsUnital = isUnital;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Ring;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => IsCommutativeRing;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Ring r && r.ElementType.Equals(ElementType)
        && r.IsCommutativeRing == IsCommutativeRing && r.IsUnital == IsUnital;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, IsCommutativeRing, IsUnital);
}

/// <summary>An integral domain: a commutative ring without zero divisors.</summary>
public sealed class IntegralDomain : AlgebraicStructure
{
    /// <summary>Creates an integral domain.</summary>
    public IntegralDomain(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.IntegralDomain;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool HasZeroDivisors => false;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is IntegralDomain id && id.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A field: an integral domain where every nonzero element has a multiplicative inverse.</summary>
public class Field : AlgebraicStructure
{
    /// <summary>Creates a field.</summary>
    public Field(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Field;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool HasInverses => true;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool HasZeroDivisors => false;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Field f && f.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}
