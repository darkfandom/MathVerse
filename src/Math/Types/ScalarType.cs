namespace MathVerse.Math.Types;

/// <summary>Base class for scalar numeric types. Scalar types form a subtype lattice:
/// Integer ⊂ Rational ⊂ Real ⊂ Complex.</summary>
public abstract class ScalarType : PrimitiveType
{
    /// <summary>Scalars are numeric types.</summary>
    public override bool IsNumeric => true;

    /// <summary>Whether this scalar type is ordered.</summary>
    public abstract bool IsOrdered { get; }

    /// <summary>Whether this scalar type is algebraically closed.</summary>
    public abstract bool IsAlgebraicallyClosed { get; }

    /// <summary>The supertype of this scalar type.</summary>
    public abstract ScalarType? Supertype { get; }
}
