using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Math.Foundation.Quantities;

public sealed class QuantityParser
{
    private static readonly Lazy<QuantityParser> LazyInstance = new(() => new QuantityParser());

    public static QuantityParser Instance => LazyInstance.Value;

    private QuantityParser()
    {
    }

    public PhysicalQuantity Parse(string input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        if (TryParse(input, out var quantity))
            return quantity!;
        throw new FormatException($"Cannot parse '{input}' as a physical quantity");
    }

    public bool TryParse(string input, out PhysicalQuantity? quantity)
    {
        quantity = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        if (double.TryParse(parts[0], out var value))
        {
            if (parts.Length >= 2)
            {
                var unitSymbol = parts[1];
                var unit = UnitRegistry.Instance.Get(unitSymbol);
                if (unit is not null)
                {
                    quantity = new PhysicalQuantity { Value = value, Unit = unit, Dimension = unit.Dimension };
                    return true;
                }
            }
            quantity = new PhysicalQuantity { Value = value, Dimension = Dimension.None };
            return true;
        }

        return false;
    }
}
