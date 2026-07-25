namespace MathVerse.Math.Types;

/// <summary>Represents a matrix type with element type and shape.</summary>
public sealed class MatrixType : MathType
{
    /// <summary>The element type.</summary>
    public MathType ElementType { get; }

    /// <summary>Number of rows. Null for dynamic-size matrices.</summary>
    public int? Rows { get; }

    /// <summary>Number of columns. Null for dynamic-size matrices.</summary>
    public int? Columns { get; }

    /// <summary>Creates a matrix type.</summary>
    public MatrixType(MathType elementType, int? rows = null, int? columns = null)
    {
        ElementType = elementType;
        Rows = rows;
        Columns = columns;
    }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Matrix;

    /// <inheritdoc/>
    public override string Name
    {
        get
        {
            if (Rows.HasValue && Columns.HasValue)
                return $"Matrix<{ElementType.Name}, {Rows.Value}×{Columns.Value}>";
            return $"Matrix<{ElementType.Name}>";
        }
    }

    /// <inheritdoc/>
    public override bool IsNumeric => ElementType is ScalarType s && s.IsNumeric;

    /// <summary>Whether the matrix is square.</summary>
    public bool IsSquare => Rows.HasValue && Columns.HasValue && Rows.Value == Columns.Value;

    /// <summary>Whether this matrix is symmetric.</summary>
    public bool IsSymmetric => IsSquare;

    /// <summary>Whether this matrix is diagonal.</summary>
    public bool IsDiagonal => IsSquare;

    /// <summary>Whether this matrix is triangular.</summary>
    public bool IsTriangular => IsSquare;

    /// <inheritdoc/>
    public override bool Equals(MathType? other)
    {
        if (other is not MatrixType mt) return false;
        return mt.ElementType.Equals(ElementType)
            && mt.Rows == Rows
            && mt.Columns == Columns;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ElementType);
        hash.Add(Rows);
        hash.Add(Columns);
        return hash.ToHashCode();
    }
}
