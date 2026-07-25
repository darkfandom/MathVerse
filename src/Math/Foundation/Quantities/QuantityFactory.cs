using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Math.Foundation.Quantities;

public sealed class QuantityFactory
{
    private static readonly Lazy<QuantityFactory> LazyInstance = new(() => new QuantityFactory());

    public static QuantityFactory Instance => LazyInstance.Value;

    private QuantityFactory()
    {
    }

    public PhysicalQuantity Create(double value, Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        return new PhysicalQuantity { Value = value, Unit = unit, Dimension = unit.Dimension };
    }

    public PhysicalQuantity Create(double value, string unitSymbol)
    {
        var unit = UnitRegistry.Instance.Get(unitSymbol)
            ?? throw new ArgumentException($"Unknown unit symbol: {unitSymbol}", nameof(unitSymbol));
        return Create(value, unit);
    }

    public PhysicalQuantity FromValue(double value, Dimension dimension)
    {
        return new PhysicalQuantity { Value = value, Unit = UnitRegistry.Instance.GetAll().FirstOrDefault()!, Dimension = dimension };
    }

    public PhysicalQuantity Zero(Unit unit)
    {
        return Create(0.0, unit);
    }

    public PhysicalQuantity Zero(string unitSymbol)
    {
        return Create(0.0, unitSymbol);
    }

    public PhysicalQuantity One(Unit unit)
    {
        return Create(1.0, unit);
    }

    public PhysicalQuantity One(string unitSymbol)
    {
        return Create(1.0, unitSymbol);
    }
}
