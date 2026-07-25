namespace MathVerse.Math.Types;

/// <summary>Represents complex numbers.</summary>
public sealed class ComplexType : ScalarType
{
    /// <summary>The singleton instance.</summary>
    public static readonly ComplexType Instance = new();

    private ComplexType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Complex;

    /// <inheritdoc/>
    public override string Name => "Complex";

    /// <inheritdoc/>
    public override bool IsIntegral => false;

    /// <inheritdoc/>
    public override bool IsField => true;

    /// <inheritdoc/>
    public override bool IsOrdered => false;

    /// <inheritdoc/>
    public override bool IsAlgebraicallyClosed => true;

    /// <inheritdoc/>
    public override ScalarType? Supertype => null;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is ComplexType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(ComplexType).GetHashCode();
}
