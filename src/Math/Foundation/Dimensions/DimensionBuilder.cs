namespace MathVerse.Math.Foundation.Dimensions;

public sealed class DimensionBuilder
{
    private readonly Dictionary<string, double> _exponents = new();

    public DimensionBuilder Length(double exp = 1)
    {
        AddExponent("L", exp);
        return this;
    }

    public DimensionBuilder Mass(double exp = 1)
    {
        AddExponent("M", exp);
        return this;
    }

    public DimensionBuilder Time(double exp = 1)
    {
        AddExponent("T", exp);
        return this;
    }

    public DimensionBuilder Current(double exp = 1)
    {
        AddExponent("I", exp);
        return this;
    }

    public DimensionBuilder Temperature(double exp = 1)
    {
        AddExponent("K", exp);
        return this;
    }

    public DimensionBuilder Substance(double exp = 1)
    {
        AddExponent("N", exp);
        return this;
    }

    public DimensionBuilder Luminous(double exp = 1)
    {
        AddExponent("J", exp);
        return this;
    }

    public Dimension Build()
    {
        var cleaned = _exponents
            .Where(kvp => kvp.Value != 0)
            .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return new Dimension(cleaned);
    }

    private void AddExponent(string key, double value)
    {
        _exponents.TryGetValue(key, out var existing);
        var sum = existing + value;
        if (sum != 0)
            _exponents[key] = sum;
        else
            _exponents.Remove(key);
    }
}
