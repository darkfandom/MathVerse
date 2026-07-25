namespace MathVerse.Math.Types.Inference;

/// <summary>Represents a type variable used during type inference.</summary>
public sealed class TypeVariable : MathType
{
    /// <summary>Unique identifier for this type variable.</summary>
    public int Id { get; }

    /// <summary>Optional human-readable name.</summary>
    public string? SourceName { get; }

    /// <summary>Creates a type variable.</summary>
    public TypeVariable(int id, string? sourceName = null)
    {
        Id = id;
        SourceName = sourceName;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Unknown;

    /// <inheritdoc/>
    public override string Name => SourceName ?? $"?{Id}";

    /// <inheritdoc/>
    public override bool IsGenericParameter => true;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) =>
        other is TypeVariable tv && tv.Id == Id;

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
