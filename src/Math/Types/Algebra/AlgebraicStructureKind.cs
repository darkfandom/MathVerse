namespace MathVerse.Math.Types.Algebra;

/// <summary>Enumerates algebraic structure kinds.</summary>
public enum AlgebraicStructureKind
{
    /// <summary>No structure.</summary>
    None,
    /// <summary>Magma (closed binary operation).</summary>
    Magma,
    /// <summary>Semigroup (associative magma).</summary>
    Semigroup,
    /// <summary>Monoid (associative with identity).</summary>
    Monoid,
    /// <summary>Group (monoid with inverses).</summary>
    Group,
    /// <summary>Abelian group (commutative group).</summary>
    AbelianGroup,
    /// <summary>Ring (abelian group + monoid under multiplication).</summary>
    Ring,
    /// <summary>Integral domain (commutative ring without zero divisors).</summary>
    IntegralDomain,
    /// <summary>Field (every nonzero element has a multiplicative inverse).</summary>
    Field,
    /// <summary>Vector space over a field.</summary>
    VectorSpace,
    /// <summary>Module over a ring.</summary>
    Module,
    /// <summary>Inner product space (vector space with inner product).</summary>
    InnerProductSpace,
    /// <summary>Metric space (set with distance function).</summary>
    MetricSpace,
    /// <summary>Ordered field (field with compatible total order).</summary>
    OrderedField,
    /// <summary>Matrix space over a field.</summary>
    MatrixSpace,
}
