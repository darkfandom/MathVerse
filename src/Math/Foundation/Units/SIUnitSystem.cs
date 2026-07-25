using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Units;

public sealed class SIUnitSystem : UnitSystem
{
    private static readonly Lazy<SIUnitSystem> LazyInstance = new(() => new SIUnitSystem());

    public static SIUnitSystem Instance => LazyInstance.Value;

    private readonly Dictionary<string, Unit> _units = new(StringComparer.OrdinalIgnoreCase);

    private SIUnitSystem()
    {
        var length = new Unit { Symbol = "m", Name = "Meter", Dimension = new DimensionBuilder().Length().Build(), Category = UnitCategory.Length, ScaleFactor = 1.0 };
        var mass = new Unit { Symbol = "kg", Name = "Kilogram", Dimension = new DimensionBuilder().Mass().Build(), Category = UnitCategory.Mass, ScaleFactor = 1.0 };
        var time = new Unit { Symbol = "s", Name = "Second", Dimension = new DimensionBuilder().Time().Build(), Category = UnitCategory.Time, ScaleFactor = 1.0 };
        var current = new Unit { Symbol = "A", Name = "Ampere", Dimension = new DimensionBuilder().Current().Build(), Category = UnitCategory.ElectricCurrent, ScaleFactor = 1.0 };
        var temp = new Unit { Symbol = "K", Name = "Kelvin", Dimension = new DimensionBuilder().Temperature().Build(), Category = UnitCategory.Temperature, ScaleFactor = 1.0 };
        var amount = new Unit { Symbol = "mol", Name = "Mole", Dimension = new DimensionBuilder().Substance().Build(), Category = UnitCategory.AmountOfSubstance, ScaleFactor = 1.0 };
        var luminous = new Unit { Symbol = "cd", Name = "Candela", Dimension = new DimensionBuilder().Luminous().Build(), Category = UnitCategory.LuminousIntensity, ScaleFactor = 1.0 };

        RegisterUnit(length);
        RegisterUnit(mass);
        RegisterUnit(time);
        RegisterUnit(current);
        RegisterUnit(temp);
        RegisterUnit(amount);
        RegisterUnit(luminous);

        var force = new Unit { Symbol = "N", Name = "Newton", Dimension = DerivedDimension.Force, Category = UnitCategory.Force, ScaleFactor = 1.0 };
        var energy = new Unit { Symbol = "J", Name = "Joule", Dimension = DerivedDimension.Energy, Category = UnitCategory.Energy, ScaleFactor = 1.0 };
        var power = new Unit { Symbol = "W", Name = "Watt", Dimension = DerivedDimension.Power, Category = UnitCategory.Power, ScaleFactor = 1.0 };
        var pressure = new Unit { Symbol = "Pa", Name = "Pascal", Dimension = DerivedDimension.Pressure, Category = UnitCategory.Pressure, ScaleFactor = 1.0 };
        var frequency = new Unit { Symbol = "Hz", Name = "Hertz", Dimension = DerivedDimension.Frequency, Category = UnitCategory.Frequency, ScaleFactor = 1.0 };
        var voltage = new Unit { Symbol = "V", Name = "Volt", Dimension = DerivedDimension.Voltage, Category = UnitCategory.Voltage, ScaleFactor = 1.0 };
        var resistance = new Unit { Symbol = "\u03A9", Name = "Ohm", Dimension = DerivedDimension.Resistance, Category = UnitCategory.Resistance, ScaleFactor = 1.0, Aliases = ImmutableArray.Create("Ohm") };

        RegisterUnit(force);
        RegisterUnit(energy);
        RegisterUnit(power);
        RegisterUnit(pressure);
        RegisterUnit(frequency);
        RegisterUnit(voltage);
        RegisterUnit(resistance);
    }

    private void RegisterUnit(Unit unit)
    {
        _units[unit.Symbol] = unit;
        foreach (var alias in unit.Aliases)
            _units[alias] = unit;
    }

    public override string Name => "SI";

    public override UnitSystem Default => Instance;

    public override IReadOnlyList<Unit> BaseUnits => _units.Values.Where(u =>
        u.Category == UnitCategory.Length ||
        u.Category == UnitCategory.Mass ||
        u.Category == UnitCategory.Time ||
        u.Category == UnitCategory.ElectricCurrent ||
        u.Category == UnitCategory.Temperature ||
        u.Category == UnitCategory.AmountOfSubstance ||
        u.Category == UnitCategory.LuminousIntensity).ToList().AsReadOnly();

    public override Unit? GetUnit(string symbol)
    {
        _units.TryGetValue(symbol, out var unit);
        return unit;
    }
}
