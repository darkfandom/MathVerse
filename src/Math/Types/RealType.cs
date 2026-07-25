namespace MathVerse.Math.Types;

/// <summary>Represents real numbers.</summary>
public sealed class RealType : ScalarType
{
    /// <summary>The singleton instance.</summary>
    public static readonly RealType Instance = new();

    private RealType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Real;

    /// <inheritdoc/>
    public override string Name => "Real";

    /// <inheritdoc/>
    public override bool IsIntegral => false;

    /// <inheritdoc/>
    public override bool IsField => true;

    /// <inheritdoc/>
    public override bool IsOrdered => true;

    /// <inheritdoc/>
    public override bool IsAlgebraicallyClosed => false;

    /// <inheritdoc/>
    public override ScalarType Supertype => ComplexType.Instance;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is RealType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(RealType).GetHashCode();
}
