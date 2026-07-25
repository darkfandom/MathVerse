namespace MathVerse.Math.Types;

/// <summary>Represents an ordered sequence type.</summary>
public sealed class SequenceType : MathType
{
    /// <summary>The element type.</summary>
    public MathType ElementType { get; }

    /// <summary>The fixed length. Null for variable-length sequences.</summary>
    public int? Length { get; }

    /// <summary>Creates a sequence type.</summary>
    public SequenceType(MathType elementType, int? length = null)
    {
        ElementType = elementType;
        Length = length;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Sequence;

    /// <inheritdoc/>
    public override string Name => Length.HasValue
        ? $"Seq<{ElementType.Name}, {Length.Value}>"
        : $"Seq<{ElementType.Name}>";

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not SequenceType st) return false;
        return st.ElementType.Equals(ElementType) && st.Length == Length;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ElementType);
        hash.Add(Length);
        return hash.ToHashCode();
    }
}
