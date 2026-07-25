using MathVerse.Math.Foundation.Units;

namespace MathVerse.Math.Foundation.Conversion;

public sealed class UnitConverter
{
    private static readonly Lazy<UnitConverter> LazyInstance = new(() => new UnitConverter());

    public static UnitConverter Instance => LazyInstance.Value;

    private UnitConverter()
    {
    }

    public ConversionResult Convert(double value, string fromUnit, string toUnit)
    {
        return ConversionGraph.Instance.Convert(value, fromUnit, toUnit);
    }

    public ConversionResult Convert(double value, Unit fromUnit, Unit toUnit)
    {
        if (fromUnit is null) throw new ArgumentNullException(nameof(fromUnit));
        if (toUnit is null) throw new ArgumentNullException(nameof(toUnit));
        return Convert(value, fromUnit.Symbol, toUnit.Symbol);
    }

    public bool TryConvert(double value, string fromUnit, string toUnit, out double result)
    {
        var conversionResult = Convert(value, fromUnit, toUnit);
        if (conversionResult.Success)
        {
            result = conversionResult.ConvertedValue;
            return true;
        }
        result = default;
        return false;
    }

    public bool CanConvert(string fromUnit, string toUnit)
    {
        return ConversionGraph.Instance.CanConvert(fromUnit, toUnit);
    }
}
