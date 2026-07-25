namespace MathVerse.Math.Types;

/// <summary>Represents the boolean type (true/false).</summary>
public sealed class BooleanType : PrimitiveType
{
    /// <summary>The singleton instance.</summary>
    public static readonly BooleanType Instance = new();

    private BooleanType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Boolean;

    /// <inheritdoc/>
    public override string Name => "Boolean";

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is BooleanType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(BooleanType).GetHashCode();
}
