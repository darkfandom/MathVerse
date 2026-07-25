namespace MathVerse.Math.Foundation.Units;

public sealed class CustomUnitSystem : UnitSystem
{
    private readonly Dictionary<string, Unit> _units = new(StringComparer.OrdinalIgnoreCase);

    public override UnitSystem Default => this;

    public override IReadOnlyList<Unit> BaseUnits => _units.Values.ToList().AsReadOnly();

    private CustomUnitSystem() { }

    public sealed class Builder
    {
        private readonly Dictionary<string, Unit> _units = new();
        private string _name = "Custom";

        public Builder Named(string name)
        {
            _name = name;
            return this;
        }

        public Builder WithUnit(Unit unit)
        {
            _units[unit.Symbol] = unit;
            return this;
        }

        public Builder WithUnits(IEnumerable<Unit> units)
        {
            foreach (var unit in units)
                _units[unit.Symbol] = unit;
            return this;
        }

        public CustomUnitSystem Build()
        {
            var system = new CustomUnitSystem();
            foreach (var kv in _units)
                system._units[kv.Key] = kv.Value;
            return system;
        }
    }
}
