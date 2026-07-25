using System.Collections.Concurrent;
using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Units;

public sealed class UnitRegistry
{
    private static readonly Lazy<UnitRegistry> LazyInstance = new(() => new UnitRegistry());

    public static UnitRegistry Instance => LazyInstance.Value;

    private readonly ConcurrentDictionary<string, Unit> _bySymbol = new(StringComparer.OrdinalIgnoreCase);

    private UnitRegistry()
    {
        var si = SIUnitSystem.Instance;
        foreach (var unit in si.BaseUnits)
            Register(unit);
    }

    public Unit? Get(string symbol)
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        _bySymbol.TryGetValue(symbol, out var unit);
        return unit;
    }

    public IReadOnlyList<Unit> GetByCategory(UnitCategory category)
    {
        return _bySymbol.Values.Where(u => u.Category == category).Distinct().ToList().AsReadOnly();
    }

    public IReadOnlyList<Unit> GetByDimension(Dimension dimension)
    {
        return _bySymbol.Values.Where(u => u.Dimension.IsCompatibleWith(dimension)).Distinct().ToList().AsReadOnly();
    }

    public IReadOnlyList<Unit> GetAll()
    {
        return _bySymbol.Values.Distinct().ToList().AsReadOnly();
    }

    public void Register(Unit unit)
    {
        if (unit is null) throw new ArgumentNullException(nameof(unit));
        _bySymbol[unit.Symbol] = unit;
        foreach (var alias in unit.Aliases)
            _bySymbol[alias] = unit;
    }
}
