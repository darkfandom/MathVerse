namespace MathVerse.Math.Types.Tensor;

/// <summary>Represents a single dimension of a tensor axis.</summary>
public sealed class Dimension : IEquatable<Dimension>
{
    /// <summary>The size of this dimension. Null means dynamic/unknown.</summary>
    public int? Size { get; }

    /// <summary>The name of this dimension (optional).</summary>
    public string? Name { get; }

    /// <summary>Creates a dimension.</summary>
    public Dimension(int? size = null, string? name = null)
    {
        Size = size;
        Name = name;
    }

    /// <summary>Whether this dimension is fixed.</summary>
    public bool IsFixed => Size.HasValue;

    /// <summary>Whether this dimension is a scalar (rank 0).</summary>
    public bool IsScalar => Size.HasValue && Size.Value <= 1;

    /// <inheritdoc/>
    public bool Equals(Dimension? other) =>
        other is not null && other.Size == Size && other.Name == Name;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Dimension);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Size, Name);

    /// <inheritdoc/>
    public override string ToString() => Name ?? Size?.ToString() ?? "?";
}

/// <summary>Represents the shape (dimensions) of a tensor.</summary>
public sealed class TensorShape : IEquatable<TensorShape>
{
    /// <summary>The dimensions.</summary>
    public IReadOnlyList<Dimension> Dimensions { get; }

    /// <summary>Creates a tensor shape.</summary>
    public TensorShape(IReadOnlyList<Dimension> dimensions)
    {
        Dimensions = dimensions;
    }

    /// <summary>Creates a shape from dimension sizes.</summary>
    public TensorShape(params int[] sizes)
    {
        Dimensions = sizes.Select(s => new Dimension(s)).ToList();
    }

    /// <summary>The rank (number of dimensions).</summary>
    public int Rank => Dimensions.Count;

    /// <summary>The total number of elements. Null if any dimension is dynamic.</summary>
    public long? TotalSize
    {
        get
        {
            long total = 1;
            foreach (var dim in Dimensions)
            {
                if (!dim.Size.HasValue) return null;
                total *= dim.Size.Value;
            }
            return total;
        }
    }

    /// <summary>Whether all dimensions are fixed.</summary>
    public bool IsFullyStatic => Dimensions.All(d => d.IsFixed);

    /// <summary>Whether this represents a scalar.</summary>
    public bool IsScalar => Rank == 0;

    /// <summary>Whether this represents a vector.</summary>
    public bool IsVector => Rank == 1;

    /// <summary>Whether this represents a matrix.</summary>
    public bool IsMatrix => Rank == 2;

    /// <summary>Whether this shape is compatible with another for element-wise ops.</summary>
    public bool IsBroadcastableWith(TensorShape other)
    {
        var maxRank = System.Math.Max(Rank, other.Rank);
        for (int i = 0; i < maxRank; i++)
        {
            var a = i < Rank ? Dimensions[i] : new Dimension(1);
            var b = i < other.Rank ? other.Dimensions[i] : new Dimension(1);

            if (a.IsFixed && b.IsFixed && a.Size != b.Size && a.Size != 1 && b.Size != 1)
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public bool Equals(TensorShape? other)
    {
        if (other is null) return false;
        if (other.Rank != Rank) return false;
        for (int i = 0; i < Rank; i++)
        {
            if (!other.Dimensions[i].Equals(Dimensions[i])) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TensorShape);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var d in Dimensions)
            hash.Add(d);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Rank == 0) return "scalar";
        var dims = string.Join("×", Dimensions.Select(d => d.ToString()));
        return $"[{dims}]";
    }
}

/// <summary>Represents the rank of a tensor.</summary>
public sealed class TensorRank : IEquatable<TensorRank>
{
    /// <summary>The rank value.</summary>
    public int Value { get; }

    /// <summary>Creates a tensor rank.</summary>
    public TensorRank(int value)
    {
        Value = value;
    }

    /// <summary>Scalar rank.</summary>
    public static readonly TensorRank Scalar = new(0);

    /// <summary>Vector rank.</summary>
    public static readonly TensorRank Vector = new(1);

    /// <summary>Matrix rank.</summary>
    public static readonly TensorRank Matrix = new(2);

    /// <inheritdoc/>
    public bool Equals(TensorRank? other) =>
        other is not null && other.Value == Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TensorRank);

    /// <inheritdoc/>
    public override int GetHashCode() => Value;

    /// <inheritdoc/>
    public override string ToString() => Value switch
    {
        0 => "scalar",
        1 => "vector",
        2 => "matrix",
        _ => $"rank-{Value}",
    };

    /// <summary>Operator +.</summary>
    public static TensorRank operator +(TensorRank a, TensorRank b) =>
        new(a.Value + b.Value);

    /// <summary>Operator -.</summary>
    public static TensorRank operator -(TensorRank a, TensorRank b) =>
        new(a.Value - b.Value);
}

/// <summary>A dimension vector representing the shape of a tensor.</summary>
public sealed class DimensionVector : IEquatable<DimensionVector>
{
    /// <summary>The dimension sizes.</summary>
    public IReadOnlyList<int> Sizes { get; }

    /// <summary>Creates a dimension vector.</summary>
    public DimensionVector(IReadOnlyList<int> sizes)
    {
        Sizes = sizes;
    }

    /// <summary>Creates a dimension vector from params.</summary>
    public DimensionVector(params int[] sizes)
    {
        Sizes = sizes;
    }

    /// <summary>The rank.</summary>
    public int Rank => Sizes.Count;

    /// <summary>Total number of elements.</summary>
    public long TotalSize => Sizes.Aggregate(1L, (a, b) => a * b);

    /// <summary>Whether this matches another dimension vector.</summary>
    public bool Matches(DimensionVector other)
    {
        if (other.Rank != Rank) return false;
        for (int i = 0; i < Rank; i++)
        {
            if (Sizes[i] != other.Sizes[i] && Sizes[i] != 1 && other.Sizes[i] != 1)
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public bool Equals(DimensionVector? other)
    {
        if (other is null) return false;
        if (other.Rank != Rank) return false;
        for (int i = 0; i < Rank; i++)
        {
            if (other.Sizes[i] != Sizes[i]) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DimensionVector);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var s in Sizes)
            hash.Add(s);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{string.Join("×", Sizes)}]";
}
