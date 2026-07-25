namespace MathVerse.Math.Foundation.Domains;

public sealed class DomainComparer : IComparer<MathDomain>, IEqualityComparer<MathDomain>
{
    private static readonly Lazy<DomainComparer> LazyInstance = new(() => new DomainComparer());

    public static DomainComparer Instance => LazyInstance.Value;

    private DomainComparer() { }

    public int Compare(MathDomain? x, MathDomain? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return 1;
        if (y is null) return -1;

        int xCount = CountFlags(x.Kind);
        int yCount = CountFlags(y.Kind);

        if (xCount != yCount) return xCount.CompareTo(yCount);

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }

    public bool Equals(MathDomain? x, MathDomain? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(MathDomain? obj)
    {
        return obj?.GetHashCode() ?? 0;
    }

    private static int CountFlags(DomainKind kind)
    {
        int count = 0;
        int value = (int)kind;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }
        return count;
    }
}
