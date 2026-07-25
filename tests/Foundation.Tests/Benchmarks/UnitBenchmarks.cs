using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class UnitBenchmarks
{
    private UnitRegistry _registry = null!;
    private SIUnitSystem _siSystem = null!;
    private UnitFormatter _formatter = null!;
    private Unit _meter = null!;
    private Unit _kilogram = null!;
    private Unit _second = null!;
    private Unit _newton = null!;
    private Unit _joule = null!;
    private Unit _watt = null!;
    private Unit _pascal = null!;
    private Unit _volt = null!;
    private Unit _ohm = null!;
    private UnitPrefix _kiloPrefix = null!;
    private UnitPrefix _milliPrefix = null!;
    private UnitPrefix _megaPrefix = null!;

    [GlobalSetup]
    public void Setup()
    {
        _registry = UnitRegistry.Instance;
        _siSystem = SIUnitSystem.Instance;
        _formatter = UnitFormatter.Instance;
        _meter = _registry.Get("m")!;
        _kilogram = _registry.Get("kg")!;
        _second = _registry.Get("s")!;
        _newton = _registry.Get("N")!;
        _joule = _registry.Get("J")!;
        _watt = _registry.Get("W")!;
        _pascal = _registry.Get("Pa")!;
        _volt = _registry.Get("V")!;
        _ohm = _registry.Get("\u03A9")!;
        _kiloPrefix = UnitPrefixes.Kilo;
        _milliPrefix = UnitPrefixes.Milli;
        _megaPrefix = UnitPrefixes.Mega;
    }

    [BenchmarkCategory("RegistryGet"), Benchmark(Baseline = true)]
    public Unit? Registry_Get_Meter()
    {
        return _registry.Get("m");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_Kilogram()
    {
        return _registry.Get("kg");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_Newton()
    {
        return _registry.Get("N");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_Joule()
    {
        return _registry.Get("J");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_Ohm()
    {
        return _registry.Get("\u03A9");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_Ohm_Alias()
    {
        return _registry.Get("Ohm");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public Unit? Registry_Get_NotFound()
    {
        return _registry.Get("zzz");
    }

    [BenchmarkCategory("SIUnitSystem"), Benchmark]
    public Unit? SI_GetUnit_Meter()
    {
        return _siSystem.GetUnit("m");
    }

    [BenchmarkCategory("SIUnitSystem"), Benchmark]
    public Unit? SI_GetUnit_Newton()
    {
        return _siSystem.GetUnit("N");
    }

    [BenchmarkCategory("SIUnitSystem"), Benchmark]
    public Unit? SI_GetUnit_Joule()
    {
        return _siSystem.GetUnit("J");
    }

    [BenchmarkCategory("SIUnitSystem"), Benchmark]
    public IReadOnlyList<Unit> SI_BaseUnits()
    {
        return _siSystem.BaseUnits;
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> GetByCategory_Length()
    {
        return _registry.GetByCategory(UnitCategory.Length);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> GetByCategory_Mass()
    {
        return _registry.GetByCategory(UnitCategory.Mass);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> GetByCategory_Force()
    {
        return _registry.GetByCategory(UnitCategory.Force);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> GetByDimension_Length()
    {
        return _registry.GetByDimension(Dimension.FromBaseDimensions(length: 1));
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> GetByDimension_Force()
    {
        return _registry.GetByDimension(DerivedDimension.Force);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<Unit> Registry_GetAll()
    {
        return _registry.GetAll();
    }

    [BenchmarkCategory("Formatter"), Benchmark(Baseline = true)]
    public string Format_Meter()
    {
        return _formatter.Format(_meter);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string Format_Newton()
    {
        return _formatter.Format(_newton);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string Format_Ohm()
    {
        return _formatter.Format(_ohm);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatQuantity_42Meter()
    {
        return _formatter.FormatQuantity(42.0, _meter);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatQuantity_9point8Newton()
    {
        return _formatter.FormatQuantity(9.8, _newton);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatWithPrefix_KiloMeter()
    {
        return _formatter.FormatWithPrefix(_meter, _kiloPrefix);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatWithPrefix_MilliMeter()
    {
        return _formatter.FormatWithPrefix(_meter, _milliPrefix);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatWithPrefix_MegaJoule()
    {
        return _formatter.FormatWithPrefix(_joule, _megaPrefix);
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Unit Builder_Simple()
    {
        return new UnitBuilder()
            .WithSymbol("custom_m")
            .WithName("CustomMeter")
            .WithDimension(Dimension.FromBaseDimensions(length: 1))
            .WithCategory(UnitCategory.Length)
            .WithScaleFactor(1.0)
            .Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Unit Builder_WithAlias()
    {
        return new UnitBuilder()
            .WithSymbol("custom_N")
            .WithName("CustomNewton")
            .WithDimension(DerivedDimension.Force)
            .WithCategory(UnitCategory.Force)
            .WithScaleFactor(1.0)
            .WithAlias("CustomN")
            .Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public Unit Builder_FullChain()
    {
        return new UnitBuilder()
            .WithSymbol("custom_kg")
            .WithName("CustomKilogram")
            .WithDimension(Dimension.FromBaseDimensions(mass: 1))
            .WithCategory(UnitCategory.Mass)
            .WithScaleFactor(0.001)
            .WithAlias("CustomKg")
            .WithAlias("ckg")
            .Build();
    }

    [BenchmarkCategory("Prefix"), Benchmark]
    public Unit WithPrefix_Kilo()
    {
        return _meter.WithPrefix(_kiloPrefix);
    }

    [BenchmarkCategory("Prefix"), Benchmark]
    public Unit WithPrefix_Milli()
    {
        return _meter.WithPrefix(_milliPrefix);
    }

    [BenchmarkCategory("Prefix"), Benchmark]
    public Unit WithPrefix_Mega()
    {
        return _joule.WithPrefix(_megaPrefix);
    }

    [BenchmarkCategory("PrefixLookup"), Benchmark]
    public UnitPrefix? Prefix_FromSymbol_k()
    {
        return UnitPrefixes.FromSymbol("k");
    }

    [BenchmarkCategory("PrefixLookup"), Benchmark]
    public UnitPrefix? Prefix_FromSymbol_m()
    {
        return UnitPrefixes.FromSymbol("m");
    }

    [BenchmarkCategory("PrefixLookup"), Benchmark]
    public UnitPrefix? Prefix_FromName_kilo()
    {
        return UnitPrefixes.FromName("kilo");
    }

    [BenchmarkCategory("PrefixLookup"), Benchmark]
    public UnitPrefix? Prefix_FromName_milli()
    {
        return UnitPrefixes.FromName("milli");
    }

    [BenchmarkCategory("PrefixLookup"), Benchmark]
    public IReadOnlyList<UnitPrefix> Prefix_All()
    {
        return UnitPrefixes.All();
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool Unit_IsBaseUnit_Meter()
    {
        return _meter.IsBaseUnit;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool Unit_IsDerivedUnit_Newton()
    {
        return _newton.IsDerivedUnit;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public double Unit_ScaleFactor_Kilogram()
    {
        return _kilogram.ScaleFactor;
    }
}
