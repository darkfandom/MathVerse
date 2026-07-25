namespace MathVerse.Math.Types;

/// <summary>Represents a tensor type with element type and shape.</summary>
public sealed class TensorType : MathType
{
    /// <summary>The element type.</summary>
    public MathType ElementType { get; }

    /// <summary>The shape (dimensions per axis). Null entries mean dynamic.</summary>
    public IReadOnlyList<int?> Shape { get; }

    /// <summary>Creates a tensor type.</summary>
    public TensorType(MathType elementType, IReadOnlyList<int?> shape)
    {
        ElementType = elementType;
        Shape = shape;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Tensor;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var dims = string.Join("×", Shape.Select(d => d?.ToString() ?? "?"));
            return $"Tensor<{ElementType.Name}, [{dims}]>";
        }
    }

    /// <summary>The rank (number of axes).</summary>
    public int Rank => Shape.Count;

    /// <inheritdoc/>
    public override bool IsNumeric => ElementType is ScalarType s && s.IsNumeric;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not TensorType tt) return false;
        if (!tt.ElementType.Equals(ElementType)) return false;
        if (tt.Shape.Count != Shape.Count) return false;
        for (int i = 0; i < Shape.Count; i++)
        {
            if (tt.Shape[i] != Shape[i]) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ElementType);
        foreach (var d in Shape)
        {
            hash.Add(d);
        }
        return hash.ToHashCode();
    }
}
