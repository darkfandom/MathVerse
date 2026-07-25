namespace MathVerse.Math.Foundation.Units;

using MathVerse.Math.Foundation.Dimensions;

public sealed class ImperialUnitSystem : UnitSystem
{
    private static readonly Lazy<ImperialUnitSystem> _default = new(() => new ImperialUnitSystem());

    public override UnitSystem Default => _default.Value;

    public override IReadOnlyList<Unit> BaseUnits => Units.Values.ToList().AsReadOnly();

    public ImperialUnitSystem()
    {
        Name = "Imperial";
        Units = CreateUnits();
    }

    private static ImmutableDictionary<string, Unit> CreateUnits()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, Unit>();

        AddUnit(builder, "ft", "foot", UnitCategory.Length,
            Dimension.FromBaseDimensions(length: 1), 0.3048, new[] { "feet" });
        AddUnit(builder, "in", "inch", UnitCategory.Length,
            Dimension.FromBaseDimensions(length: 1), 0.0254, new[] { "inches" });
        AddUnit(builder, "yd", "yard", UnitCategory.Length,
            Dimension.FromBaseDimensions(length: 1), 0.9144, new[] { "yards" });
        AddUnit(builder, "mi", "mile", UnitCategory.Length,
            Dimension.FromBaseDimensions(length: 1), 1609.344, new[] { "miles" });

        AddUnit(builder, "lb", "pound", UnitCategory.Mass,
            Dimension.FromBaseDimensions(mass: 1), 0.45359237, new[] { "lbs", "pounds" });
        AddUnit(builder, "oz", "ounce", UnitCategory.Mass,
            Dimension.FromBaseDimensions(mass: 1), 0.028349523125, new[] { "ounces" });
        AddUnit(builder, "st", "stone", UnitCategory.Mass,
            Dimension.FromBaseDimensions(mass: 1), 6.35029318, new[] { "stones" });

        AddUnit(builder, "gal", "gallon", UnitCategory.Volume,
            Dimension.FromBaseDimensions(length: 3), 0.003785411784, new[] { "gallons" });
        AddUnit(builder, "qt", "quart", UnitCategory.Volume,
            Dimension.FromBaseDimensions(length: 3), 0.000946352946, new[] { "quarts" });
        AddUnit(builder, "pt", "pint", UnitCategory.Volume,
            Dimension.FromBaseDimensions(length: 3), 0.000473176473, new[] { "pints" });
        AddUnit(builder, "fl oz", "fluid ounce", UnitCategory.Volume,
            Dimension.FromBaseDimensions(length: 3), 2.95735295625e-5, new[] { "fluid ounces" });

        AddUnit(builder, "°F", "degree Fahrenheit", UnitCategory.Temperature,
            Dimension.FromBaseDimensions(temperature: 1), 1.0, new[] { "F" });

        AddUnit(builder, "lbf", "pound-force", UnitCategory.Force,
            DerivedDimension.Force, 4.4482216152605, new[] { "pounds-force" });

        AddUnit(builder, "hp", "horsepower", UnitCategory.Power,
            DerivedDimension.Power, 745.69987158227022, new[] { "horsepowers" });

        AddUnit(builder, "BTU", "British thermal unit", UnitCategory.Energy,
            DerivedDimension.Energy, 1055.06, new[] { "btu" });

        return builder.ToImmutable();
    }

    private static void AddUnit(
        ImmutableDictionary<string, Unit>.Builder builder,
        string symbol, string name, UnitCategory category,
        Dimension dimension, double scaleFactor, string[] aliases)
    {
        var unit = new Unit
        {
            Symbol = symbol,
            Name = name,
            Category = category,
            Dimension = dimension,
            ScaleFactor = scaleFactor,
            Aliases = aliases.ToImmutableArray()
        };
        builder[symbol] = unit;
    }
}
