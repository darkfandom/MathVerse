namespace MathVerse.Math.Foundation.Dimensions;

public sealed class DimensionComparer : IComparer<Dimension>, IEqualityComparer<Dimension>
{
    public static DimensionComparer Instance { get; } = new();

    public int Compare(Dimension? x, Dimension? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
    }

    public bool Equals(Dimension? x, Dimension? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(Dimension obj)
    {
        return obj.GetHashCode();
    }
}
