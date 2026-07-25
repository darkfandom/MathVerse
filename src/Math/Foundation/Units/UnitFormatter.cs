namespace MathVerse.Math.Foundation.Units;

public sealed class UnitFormatter
{
    private static readonly Lazy<UnitFormatter> LazyInstance = new(() => new UnitFormatter());

    public static UnitFormatter Instance => LazyInstance.Value;

    private UnitFormatter()
    {
    }

    public string Format(Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        return string.IsNullOrEmpty(unit.Symbol) ? unit.Name : unit.Symbol;
    }

    public string FormatWithPrefix(Unit unit, UnitPrefix prefix)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        if (prefix is null) throw new ArgumentNullException(nameof(prefix));
        return $"{prefix.Symbol}{unit.Symbol}";
    }

    public string FormatQuantity(double value, Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        return $"{value} {Format(unit)}";
    }
}
