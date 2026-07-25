namespace MathVerse.Math.Foundation.Dimensions;

public static class DimensionOperations
{
    public static Dimension Multiply(Dimension a, Dimension b) => a.Multiply(b);

    public static Dimension Divide(Dimension a, Dimension b) => a.Divide(b);

    public static Dimension Power(Dimension d, double n) => d.Power(n);

    public static Dimension Root(Dimension d, int n) => d.Root(n);

    public static Dimension Simplify(Dimension d)
    {
        var cleaned = d.Exponents
            .Where(kvp => kvp.Value != 0)
            .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return d with { Exponents = cleaned };
    }

    public static bool AreCompatible(Dimension a, Dimension b) => a.IsCompatibleWith(b);

    public static Dimension ComputeFromProduct(Dimension[] dims, double[] exponents)
    {
        if (dims.Length != exponents.Length)
            throw new ArgumentException("Dimensions and exponents arrays must have the same length.");

        var result = Dimension.None;
        for (int i = 0; i < dims.Length; i++)
        {
            if (exponents[i] != 0)
                result = result.Multiply(dims[i].Power(exponents[i]));
        }
        return result;
    }
}
