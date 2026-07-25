namespace MathVerse.Math.Foundation.Quantities;

public sealed class QuantityFormatter
{
    private static readonly Lazy<QuantityFormatter> LazyInstance = new(() => new QuantityFormatter());

    public static QuantityFormatter Instance => LazyInstance.Value;

    private QuantityFormatter()
    {
    }

    public string Format(PhysicalQuantity quantity)
    {
        if (quantity is null) throw new ArgumentNullException(nameof(quantity));
        return quantity.ToString();
    }

    public string FormatWithPrecision(PhysicalQuantity quantity, int precision)
    {
        if (quantity is null) throw new ArgumentNullException(nameof(quantity));
        var unitSymbol = quantity.Unit?.Symbol ?? "";
        return $"{quantity.Value.ToString($"F{precision}")} {unitSymbol}".Trim();
    }

    public string FormatScientific(PhysicalQuantity quantity, int digits = 3)
    {
        if (quantity is null) throw new ArgumentNullException(nameof(quantity));
        var unitSymbol = quantity.Unit?.Symbol ?? "";
        return $"{quantity.Value.ToString($"E{digits}")} {unitSymbol}".Trim();
    }
}
