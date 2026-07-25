namespace MathVerse.Math.Types;

/// <summary>Represents an unresolved/unknown type.</summary>
public sealed class UnknownType : MathType
{
    /// <summary>The singleton instance.</summary>
    public static readonly UnknownType Instance = new();

    private UnknownType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Unknown;

    /// <inheritdoc/>
    public override string Name => "?";

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is UnknownType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(UnknownType).GetHashCode();
}
