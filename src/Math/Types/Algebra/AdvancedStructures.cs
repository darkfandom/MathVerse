namespace MathVerse.Math.Types.Algebra;

/// <summary>A vector space over a field.</summary>
public sealed class VectorSpace : AlgebraicStructure
{
    /// <summary>The scalar field.</summary>
    public Field ScalarField { get; }

    /// <summary>Creates a vector space.</summary>
    public VectorSpace(MathType elementType, Field scalarField) : base(elementType)
    {
        ScalarField = scalarField;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.VectorSpace;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is VectorSpace vs && vs.ElementType.Equals(ElementType)
        && vs.ScalarField.Equals(ScalarField);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, ScalarField);
}

/// <summary>A module over a ring (generalization of vector space).</summary>
public sealed class Module : AlgebraicStructure
{
    /// <summary>The coefficient ring.</summary>
    public Ring CoefficientRing { get; }

    /// <summary>Creates a module.</summary>
    public Module(MathType elementType, Ring coefficientRing) : base(elementType)
    {
        CoefficientRing = coefficientRing;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.Module;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool HasIdentity => true;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is Module m && m.ElementType.Equals(ElementType)
        && m.CoefficientRing.Equals(CoefficientRing);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, CoefficientRing);
}

/// <summary>An inner product space: a vector space with an inner product.</summary>
public sealed class InnerProductSpace : AlgebraicStructure
{
    /// <summary>The underlying vector space.</summary>
    public VectorSpace VectorSpaceInstance { get; }

    /// <summary>Whether the inner product is symmetric.</summary>
    public bool IsSymmetric { get; }

    /// <summary>Whether the inner product is positive-definite.</summary>
    public bool IsPositiveDefinite { get; }

    /// <summary>Creates an inner product space.</summary>
    public InnerProductSpace(MathType elementType, VectorSpace vectorSpace,
        bool isSymmetric = true, bool isPositiveDefinite = true)
        : base(elementType)
    {
        VectorSpaceInstance = vectorSpace;
        IsSymmetric = isSymmetric;
        IsPositiveDefinite = isPositiveDefinite;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.InnerProductSpace;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is InnerProductSpace ips && ips.ElementType.Equals(ElementType)
        && ips.IsSymmetric == IsSymmetric && ips.IsPositiveDefinite == IsPositiveDefinite;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(Kind, ElementType, IsSymmetric, IsPositiveDefinite);
}

/// <summary>A metric space: a set with a distance function.</summary>
public sealed class MetricSpace : AlgebraicStructure
{
    /// <summary>Whether the metric is induced by a norm.</summary>
    public bool IsNormed { get; }

    /// <summary>Creates a metric space.</summary>
    public MetricSpace(MathType elementType, bool isNormed = false) : base(elementType)
    {
        IsNormed = isNormed;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.MetricSpace;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is MetricSpace ms && ms.ElementType.Equals(ElementType) && ms.IsNormed == IsNormed;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, IsNormed);
}
