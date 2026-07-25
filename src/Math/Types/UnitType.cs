namespace MathVerse.Math.Types;

/// <summary>Represents the unit (void) type.</summary>
public sealed class UnitType : MathType
{
    /// <summary>The singleton instance.</summary>
    public static readonly UnitType Instance = new();

    private UnitType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Unit;

    /// <inheritdoc/>
    public override string Name => "Unit";

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is UnitType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(UnitType).GetHashCode();
}
