namespace MathVerse.Math.Types;

/// <summary>Represents a vector type with element type and dimension.</summary>
public sealed class VectorType : MathType
{
    /// <summary>The element type.</summary>
    public MathType ElementType { get; }

    /// <summary>The dimension (number of elements). Null for dynamic-size vectors.</summary>
    public int? Dimension { get; }

    /// <summary>Creates a vector type.</summary>
    public VectorType(MathType elementType, int? dimension = null)
    {
        ElementType = elementType;
        Dimension = dimension;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Vector;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            var dim = Dimension.HasValue ? $", {Dimension.Value}" : string.Empty;
            return $"Vector<{ElementType.Name}{dim}>";
        }
    }

    /// <inheritdoc/>
    public override bool IsNumeric => ElementType is ScalarType s && s.IsNumeric;

    /// <inheritdoc/>
    public override bool IsField => ElementType is ScalarType s && s.IsField;

    /// <summary>Whether this is a row or column vector. Default is column.</summary>
    public bool IsRowVector { get; init; }

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not VectorType vt) return false;
        return vt.ElementType.Equals(ElementType)
            && vt.Dimension == Dimension
            && vt.IsRowVector == IsRowVector;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ElementType);
        hash.Add(Dimension);
        hash.Add(IsRowVector);
        return hash.ToHashCode();
    }
}
