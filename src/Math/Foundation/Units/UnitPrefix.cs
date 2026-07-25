namespace MathVerse.Math.Foundation.Units;

public sealed record UnitPrefix
{
    public string Symbol { get; init; } = "";

    public string Name { get; init; } = "";

    public double Factor { get; init; } = 1.0;
}

public static class UnitPrefixes
{
    public static UnitPrefix Yotta { get; } = new() { Symbol = "Y", Name = "yotta", Factor = 1e24 };
    public static UnitPrefix Zetta { get; } = new() { Symbol = "Z", Name = "zetta", Factor = 1e21 };
    public static UnitPrefix Exa { get; } = new() { Symbol = "E", Name = "exa", Factor = 1e18 };
    public static UnitPrefix Peta { get; } = new() { Symbol = "P", Name = "peta", Factor = 1e15 };
    public static UnitPrefix Tera { get; } = new() { Symbol = "T", Name = "tera", Factor = 1e12 };
    public static UnitPrefix Giga { get; } = new() { Symbol = "G", Name = "giga", Factor = 1e9 };
    public static UnitPrefix Mega { get; } = new() { Symbol = "M", Name = "mega", Factor = 1e6 };
    public static UnitPrefix Kilo { get; } = new() { Symbol = "k", Name = "kilo", Factor = 1e3 };
    public static UnitPrefix Hecto { get; } = new() { Symbol = "h", Name = "hecto", Factor = 1e2 };
    public static UnitPrefix Deca { get; } = new() { Symbol = "da", Name = "deca", Factor = 1e1 };
    public static UnitPrefix Deci { get; } = new() { Symbol = "d", Name = "deci", Factor = 1e-1 };
    public static UnitPrefix Centi { get; } = new() { Symbol = "c", Name = "centi", Factor = 1e-2 };
    public static UnitPrefix Milli { get; } = new() { Symbol = "m", Name = "milli", Factor = 1e-3 };
    public static UnitPrefix Micro { get; } = new() { Symbol = "μ", Name = "micro", Factor = 1e-6 };
    public static UnitPrefix Nano { get; } = new() { Symbol = "n", Name = "nano", Factor = 1e-9 };
    public static UnitPrefix Pico { get; } = new() { Symbol = "p", Name = "pico", Factor = 1e-12 };
    public static UnitPrefix Femto { get; } = new() { Symbol = "f", Name = "femto", Factor = 1e-15 };
    public static UnitPrefix Atto { get; } = new() { Symbol = "a", Name = "atto", Factor = 1e-18 };
    public static UnitPrefix Zepto { get; } = new() { Symbol = "z", Name = "zepto", Factor = 1e-21 };
    public static UnitPrefix Yocto { get; } = new() { Symbol = "y", Name = "yocto", Factor = 1e-24 };

    private static readonly ImmutableDictionary<string, UnitPrefix> BySymbol;

    private static readonly ImmutableDictionary<string, UnitPrefix> ByName;

    static UnitPrefixes()
    {
        var all = new[]
        {
            Yotta, Zetta, Exa, Peta, Tera, Giga, Mega, Kilo, Hecto, Deca,
            Deci, Centi, Milli, Micro, Nano, Pico, Femto, Atto, Zepto, Yocto
        };
        BySymbol = all.ToImmutableDictionary(p => p.Symbol);
        ByName = all.ToImmutableDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static UnitPrefix? FromSymbol(string symbol) =>
        BySymbol.TryGetValue(symbol, out var prefix) ? prefix : null;

    public static UnitPrefix? FromName(string name) =>
        ByName.TryGetValue(name, out var prefix) ? prefix : null;

    public static UnitPrefix? FromFactor(double factor) =>
        All().FirstOrDefault(p => p.Factor == factor);

    public static IReadOnlyList<UnitPrefix> All() => BySymbol.Values.ToList();
}
