namespace MathVerse.Math.Types;

/// <summary>Represents rational numbers (p/q where p, q are integers, q ≠ 0).</summary>
public sealed class RationalType : ScalarType
{
    /// <summary>The singleton instance.</summary>
    public static readonly RationalType Instance = new();

    private RationalType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Rational;

    /// <inheritdoc/>
    public override string Name => "Rational";

    /// <inheritdoc/>
    public override bool IsIntegral => false;

    /// <inheritdoc/>
    public override bool IsField => true;

    /// <inheritdoc/>
    public override bool IsOrdered => true;

    /// <inheritdoc/>
    public override bool IsAlgebraicallyClosed => false;

    /// <inheritdoc/>
    public override ScalarType Supertype => RealType.Instance;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is RationalType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(RationalType).GetHashCode();
}
