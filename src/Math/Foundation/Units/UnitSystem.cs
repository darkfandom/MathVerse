namespace MathVerse.Math.Foundation.Units;

public abstract class UnitSystem
{
    public virtual string Name { get; protected init; } = "";

    public ImmutableDictionary<string, Unit> Units { get; init; } = ImmutableDictionary<string, Unit>.Empty;

    public abstract UnitSystem Default { get; }

    public abstract IReadOnlyList<Unit> BaseUnits { get; }

    public virtual Unit? GetUnit(string symbol) =>
        Units.TryGetValue(symbol, out var unit) ? unit : null;

    public IReadOnlyList<Unit> GetByCategory(UnitCategory cat) =>
        Units.Values.Where(u => u.Category == cat).ToList();

    public IReadOnlyList<Unit> GetAll() =>
        Units.Values.ToList();
}
