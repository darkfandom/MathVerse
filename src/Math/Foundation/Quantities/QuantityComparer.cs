namespace MathVerse.Math.Foundation.Quantities;

public sealed class QuantityComparer : IComparer<PhysicalQuantity>, IEqualityComparer<PhysicalQuantity>
{
    private static readonly Lazy<QuantityComparer> LazyInstance = new(() => new QuantityComparer());

    public static QuantityComparer Instance => LazyInstance.Value;

    private QuantityComparer()
    {
    }

    public int Compare(PhysicalQuantity? x, PhysicalQuantity? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        return x.CompareTo(y);
    }

    public bool Equals(PhysicalQuantity? x, PhysicalQuantity? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        if (!x.IsDimensionallyCompatible(y)) return false;
        var diff = System.Math.Abs(x.ToBase().Value - y.ToBase().Value);
        return diff < 1e-10;
    }

    public int GetHashCode(PhysicalQuantity? obj)
    {
        if (obj is null) return 0;
        return HashCode.Combine(obj.Value, obj.Unit?.Symbol, obj.Dimension);
    }
}
