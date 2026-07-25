namespace MathVerse.Math.Types;

/// <summary>Represents the string type.</summary>
public sealed class StringType : PrimitiveType
{
    /// <summary>The singleton instance.</summary>
    public static readonly StringType Instance = new();

    private StringType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.String;

    /// <inheritdoc/>
    public override string Name => "String";

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is StringType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(StringType).GetHashCode();
}
