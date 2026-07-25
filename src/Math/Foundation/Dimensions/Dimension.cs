namespace MathVerse.Math.Foundation.Dimensions;

public sealed record Dimension(ImmutableDictionary<string, double> Exponents)
{
    public static Dimension None { get; } = new(ImmutableDictionary<string, double>.Empty);

    public bool IsDimensionless => Exponents.Count == 0 || Exponents.Values.All(v => v == 0);

    public bool IsBaseDimension => Exponents.Count(e => e.Value != 0) == 1;

    public bool Equals(Dimension? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(Exponents, other.Exponents)) return true;
        if (Exponents.Count != other.Exponents.Count) return false;
        return Exponents.All(kvp =>
            other.Exponents.TryGetValue(kvp.Key, out var otherValue) &&
            kvp.Value == otherValue);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var kvp in Exponents.OrderBy(k => k.Key))
            {
                hash = hash * 31 + (kvp.Key?.GetHashCode() ?? 0);
                hash = hash * 31 + kvp.Value.GetHashCode();
            }
            return hash;
        }
    }

    public Dimension Multiply(Dimension other)
    {
        var result = new Dictionary<string, double>(Exponents);
        foreach (var kvp in other.Exponents)
        {
            result.TryGetValue(kvp.Key, out var existing);
            var sum = existing + kvp.Value;
            if (sum != 0)
                result[kvp.Key] = sum;
            else
                result.Remove(kvp.Key);
        }
        return new(result.ToImmutableDictionary());
    }

    public Dimension Divide(Dimension other)
    {
        var result = new Dictionary<string, double>(Exponents);
        foreach (var kvp in other.Exponents)
        {
            result.TryGetValue(kvp.Key, out var existing);
            var diff = existing - kvp.Value;
            if (diff != 0)
                result[kvp.Key] = diff;
            else
                result.Remove(kvp.Key);
        }
        return new(result.ToImmutableDictionary());
    }

    public Dimension Power(double exponent)
    {
        if (exponent == 0) return None;
        var result = new Dictionary<string, double>();
        foreach (var kvp in Exponents)
        {
            var product = kvp.Value * exponent;
            if (product != 0)
                result[kvp.Key] = product;
        }
        return new(result.ToImmutableDictionary());
    }

    public Dimension Root(int n)
    {
        if (n == 0) throw new ArgumentException("Root index cannot be zero.", nameof(n));
        var result = new Dictionary<string, double>();
        foreach (var kvp in Exponents)
        {
            var quotient = kvp.Value / n;
            if (quotient != 0)
                result[kvp.Key] = quotient;
        }
        return new(result.ToImmutableDictionary());
    }

    public bool IsCompatibleWith(Dimension other)
    {
        var nonZero1 = Exponents.Where(e => e.Value != 0).OrderBy(e => e.Key).ToList();
        var nonZero2 = other.Exponents.Where(e => e.Value != 0).OrderBy(e => e.Key).ToList();
        if (nonZero1.Count != nonZero2.Count) return false;
        return nonZero1.Zip(nonZero2).All(pair =>
            pair.First.Key == pair.Second.Key && pair.First.Value == pair.Second.Value);
    }

    public override string ToString()
    {
        if (IsDimensionless) return "1";

        var sb = new StringBuilder();
        var ordered = Exponents
            .Where(e => e.Value != 0)
            .OrderBy(e => e.Key);

        foreach (var kvp in ordered)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(kvp.Key);
            if (kvp.Value != 1)
            {
                sb.Append('^');
                sb.Append(kvp.Value);
            }
        }

        return sb.ToString();
    }

    public static Dimension FromBaseDimensions(
        double length = 0,
        double mass = 0,
        double time = 0,
        double current = 0,
        double temperature = 0,
        double substance = 0,
        double luminous = 0)
    {
        var dict = new Dictionary<string, double>();
        if (length != 0) dict["L"] = length;
        if (mass != 0) dict["M"] = mass;
        if (time != 0) dict["T"] = time;
        if (current != 0) dict["I"] = current;
        if (temperature != 0) dict["K"] = temperature;
        if (substance != 0) dict["N"] = substance;
        if (luminous != 0) dict["J"] = luminous;
        return new(dict.ToImmutableDictionary());
    }
}
