using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Constants;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ConstantBenchmarks
{
    private ConstantRegistry _registry = null!;
    private ConstantLookup _lookup = null!;
    private string[] _nameOrSymbolKeys = null!;
    private string[] _exactSymbols = null!;
    private string[] _numericSymbols = null!;

    [GlobalSetup]
    public void Setup()
    {
        _registry = ConstantRegistry.Instance;
        _lookup = ConstantLookup.Instance;
        _nameOrSymbolKeys = new[] { "Pi", "E", "Tau", "Phi", "Gamma", "Catalan", "Apery", "Infinity", "NaN", "Epsilon", "FeigenbaumAlpha", "FeigenbaumDelta" };
        _exactSymbols = new[] { "\u03C0", "e", "\u03C4", "\u03C6", "\u03B3", "G", "\u03B6(3)" };
        _numericSymbols = new[] { "\u03C0", "e", "\u03C4", "\u03C6", "\u03B3", "2.2204460492503131e-16", "G", "\u03B6(3)", "2.502907875095892822283", "4.669201609102990671853" };
    }

    [BenchmarkCategory("RegistryGet"), Benchmark(Baseline = true)]
    public MathConstant? Registry_GetByName_Pi()
    {
        return _registry.Get("Pi");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public MathConstant? Registry_GetByName_E()
    {
        return _registry.Get("E");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public MathConstant? Registry_GetBySymbol_Pi()
    {
        return _registry.Get("\u03C0");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public MathConstant? Registry_GetByAlias()
    {
        return _registry.Get("Euler");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public MathConstant? Registry_GetByName_Tau()
    {
        return _registry.Get("Tau");
    }

    [BenchmarkCategory("RegistryGet"), Benchmark]
    public MathConstant? Registry_Get_NotFound()
    {
        return _registry.Get("NonExistent");
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> GetByCategory_Transcendental()
    {
        return _registry.GetByCategory(ConstantCategory.Transcendental);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> GetByCategory_Fundamental()
    {
        return _registry.GetByCategory(ConstantCategory.Fundamental);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> GetByCategory_Analysis()
    {
        return _registry.GetByCategory(ConstantCategory.Analysis);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> GetByCategory_NumberTheory()
    {
        return _registry.GetByCategory(ConstantCategory.NumberTheory);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> GetByCategory_Combinatorics()
    {
        return _registry.GetByCategory(ConstantCategory.Combinatorics);
    }

    [BenchmarkCategory("RegistryCategory"), Benchmark]
    public IReadOnlyList<MathConstant> Registry_GetAll()
    {
        return _registry.GetAll();
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark(Baseline = true)]
    public bool TryGetExact_Pi()
    {
        return _lookup.TryGetExact("\u03C0", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetExact_E()
    {
        return _lookup.TryGetExact("e", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetExact_NotFound()
    {
        return _lookup.TryGetExact("x", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetExact_Null()
    {
        return _lookup.TryGetExact(null!, out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_Pi()
    {
        return _lookup.TryGetNumeric("\u03C0", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_Tau()
    {
        return _lookup.TryGetNumeric("\u03C4", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_Epsilon()
    {
        return _lookup.TryGetNumeric("\u03B5", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_GoldenRatio()
    {
        return _lookup.TryGetNumeric("\u03C6", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_I()
    {
        return _lookup.TryGetNumeric("I", out _);
    }

    [BenchmarkCategory("LookupTryGet"), Benchmark]
    public bool TryGetNumeric_NotFound()
    {
        return _lookup.TryGetNumeric("NonExistent", out _);
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public double Constant_NumericValue_Pi()
    {
        return BuiltinConstants.Pi.NumericValue;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public Complex Constant_ComplexValue_E()
    {
        return BuiltinConstants.E.ComplexValue;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public bool Constant_IsExact_Gamma()
    {
        return BuiltinConstants.Gamma.IsExact;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public ImmutableArray<string> Constant_Aliases_Phi()
    {
        return BuiltinConstants.Phi.Aliases;
    }
}
