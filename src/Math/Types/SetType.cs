namespace MathVerse.Math.Types;

/// <summary>Represents a set type with element type.</summary>
public sealed class SetType : MathType
{
    /// <summary>The element type.</summary>
    public MathType ElementType { get; }

    /// <summary>The cardinality. Null for infinite sets.</summary>
    public int? Cardinality { get; }

    /// <summary>Creates a set type.</summary>
    public SetType(MathType elementType, int? cardinality = null)
    {
        ElementType = elementType;
        Cardinality = cardinality;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Set;

    /// <inheritdoc/>
    public override string Name => $"Set<{ElementType.Name}>";

    /// <summary>Whether this is a finite set.</summary>
    public bool IsFinite => Cardinality.HasValue;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not SetType st) return false;
        return st.ElementType.Equals(ElementType) && st.Cardinality == Cardinality;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ElementType);
        hash.Add(Cardinality);
        return hash.ToHashCode();
    }
}
