namespace MathVerse.Math.Foundation.Constants;

public sealed class ConstantLookup
{
    private static readonly Lazy<ConstantLookup> LazyInstance = new(() => new ConstantLookup());

    public static ConstantLookup Instance => LazyInstance.Value;

    private readonly ImmutableDictionary<string, MathConstant> _bySymbol;

    private ConstantLookup()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, MathConstant>(StringComparer.Ordinal);

        AddConstant(builder, BuiltinConstants.Pi);
        AddConstant(builder, BuiltinConstants.Tau);
        AddConstant(builder, BuiltinConstants.E);
        AddConstant(builder, BuiltinConstants.Phi);
        AddConstant(builder, BuiltinConstants.Gamma);
        AddConstant(builder, BuiltinConstants.I);
        AddConstant(builder, BuiltinConstants.Infinity);
        AddConstant(builder, BuiltinConstants.NaN);
        AddConstant(builder, BuiltinConstants.Epsilon);
        AddConstant(builder, BuiltinConstants.Catalan);
        AddConstant(builder, BuiltinConstants.Apery);
        AddConstant(builder, BuiltinConstants.FeigenbaumAlpha);
        AddConstant(builder, BuiltinConstants.FeigenbaumDelta);

        _bySymbol = builder.ToImmutable();
    }

    public bool TryGetExact(string symbol, out MathConstant? constant)
    {
        if (symbol is null)
        {
            constant = null;
            return false;
        }
        if (_bySymbol.TryGetValue(symbol, out MathConstant? found) && found.IsExact)
        {
            constant = found;
            return true;
        }
        constant = null;
        return false;
    }

    public bool TryGetNumeric(string symbol, out double value)
    {
        if (symbol is null)
        {
            value = default;
            return false;
        }
        if (_bySymbol.TryGetValue(symbol, out MathConstant? constant) && !double.IsNaN(constant.NumericValue))
        {
            value = constant.NumericValue;
            return true;
        }
        value = default;
        return false;
    }

    private static void AddConstant(ImmutableDictionary<string, MathConstant>.Builder builder, MathConstant constant)
    {
        if (!builder.ContainsKey(constant.Symbol))
        {
            builder[constant.Symbol] = constant;
        }

        if (!builder.ContainsKey(constant.Name))
        {
            builder[constant.Name] = constant;
        }

        foreach (string alias in constant.Aliases)
        {
            if (!builder.ContainsKey(alias))
            {
                builder[alias] = constant;
            }
        }
    }
}
