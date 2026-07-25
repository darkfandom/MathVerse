using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Units;

public sealed class CGSUnitSystem : UnitSystem
{
    private static readonly Lazy<CGSUnitSystem> LazyInstance = new(() => new CGSUnitSystem());

    public static CGSUnitSystem Instance => LazyInstance.Value;

    private readonly Dictionary<string, Unit> _units = new(StringComparer.OrdinalIgnoreCase);

    private CGSUnitSystem()
    {
        var centimeter = new Unit { Symbol = "cm", Name = "Centimeter", Dimension = new DimensionBuilder().Length().Build(), Category = UnitCategory.Length, ScaleFactor = 0.01 };
        var gram = new Unit { Symbol = "g", Name = "Gram", Dimension = new DimensionBuilder().Mass().Build(), Category = UnitCategory.Mass, ScaleFactor = 0.001 };
        var second = new Unit { Symbol = "s", Name = "Second", Dimension = new DimensionBuilder().Time().Build(), Category = UnitCategory.Time, ScaleFactor = 1.0 };

        RegisterUnit(centimeter);
        RegisterUnit(gram);
        RegisterUnit(second);
    }

    private void RegisterUnit(Unit unit)
    {
        _units[unit.Symbol] = unit;
        foreach (var alias in unit.Aliases)
            _units[alias] = unit;
    }

    public override string Name => "CGS";

    public override UnitSystem Default => Instance;

    public override IReadOnlyList<Unit> BaseUnits => _units.Values.ToList().AsReadOnly();

    public override Unit? GetUnit(string symbol)
    {
        _units.TryGetValue(symbol, out var unit);
        return unit;
    }
}
