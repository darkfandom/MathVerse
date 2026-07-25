namespace MathVerse.Math.Foundation.Dimensions;

public readonly record struct DimensionVector
{
    private const int DimensionCount = 7;

    public double[] Components { get; init; }

    public DimensionVector()
    {
        Components = new double[DimensionCount];
    }

    public double this[BaseDimension dim]
    {
        get
        {
            var index = (int)dim;
            if (Components is null || index < 0 || index >= Components.Length)
                return 0;
            return Components[index];
        }
    }

    public DimensionVector Multiply(DimensionVector other)
    {
        var length = System.Math.Max(
            Components?.Length ?? 0,
            other.Components?.Length ?? 0);
        var result = new double[length];
        for (int i = 0; i < length; i++)
        {
            var a = Components is not null && i < Components.Length ? Components[i] : 0;
            var b = other.Components is not null && i < other.Components.Length ? other.Components[i] : 0;
            result[i] = a + b;
        }
        return new DimensionVector { Components = result };
    }

    public DimensionVector Scale(double factor)
    {
        var length = Components?.Length ?? 0;
        var result = new double[length];
        for (int i = 0; i < length; i++)
            result[i] = Components![i] * factor;
        return new DimensionVector { Components = result };
    }

    public DimensionVector Power(double exp)
    {
        var length = Components?.Length ?? 0;
        var result = new double[length];
        for (int i = 0; i < length; i++)
            result[i] = Components![i] * exp;
        return new DimensionVector { Components = result };
    }

    public bool Equals(DimensionVector other)
    {
        var len1 = Components?.Length ?? 0;
        var len2 = other.Components?.Length ?? 0;
        var maxLen = System.Math.Max(len1, len2);
        for (int i = 0; i < maxLen; i++)
        {
            var a = Components is not null && i < Components.Length ? Components[i] : 0;
            var b = other.Components is not null && i < other.Components.Length ? other.Components[i] : 0;
            if (a != b) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        if (Components is not null)
        {
            foreach (var c in Components)
                hash.Add(c);
        }
        return hash.ToHashCode();
    }

    public Dimension ToDimension()
    {
        var dict = new Dictionary<string, double>();
        var symbols = new[]
        {
            BaseDimension.Length, BaseDimension.Mass, BaseDimension.Time,
            BaseDimension.ElectricCurrent, BaseDimension.Temperature,
            BaseDimension.AmountOfSubstance, BaseDimension.LuminousIntensity
        };
        foreach (var dim in symbols)
        {
            var value = this[dim];
            if (value != 0)
                dict[dim.Symbol()] = value;
        }
        return new Dimension(dict.ToImmutableDictionary());
    }

    public static DimensionVector FromDimension(Dimension dimension)
    {
        var vector = new DimensionVector();
        var dict = new Dictionary<string, double>(dimension.Exponents);
        var symbols = new[]
        {
            BaseDimension.Length, BaseDimension.Mass, BaseDimension.Time,
            BaseDimension.ElectricCurrent, BaseDimension.Temperature,
            BaseDimension.AmountOfSubstance, BaseDimension.LuminousIntensity
        };
        var components = new double[DimensionCount];
        for (int i = 0; i < symbols.Length; i++)
        {
            if (dict.TryGetValue(symbols[i].Symbol(), out var value))
                components[i] = value;
        }
        return new DimensionVector { Components = components };
    }
}
