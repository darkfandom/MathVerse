namespace MathVerse.Math.Types;

/// <summary>Represents a tuple type with ordered element types.</summary>
public sealed class TupleType : MathType
{
    /// <summary>The element types.</summary>
    public IReadOnlyList<MathType> ElementTypes { get; }

    /// <summary>Creates a tuple type.</summary>
    public TupleType(IReadOnlyList<MathType> elementTypes)
    {
        ElementTypes = elementTypes;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Tuple;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var elements = string.Join(", ", ElementTypes.Select(t => t.Name));
            return $"({elements})";
        }
    }

    /// <summary>The arity of the tuple.</summary>
    public int Arity => ElementTypes.Count;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not TupleType tt) return false;
        if (tt.Arity != Arity) return false;
        for (int i = 0; i < Arity; i++)
        {
            if (!tt.ElementTypes[i].Equals(ElementTypes[i])) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var t in ElementTypes)
        {
            hash.Add(t);
        }
        return hash.ToHashCode();
    }
}
