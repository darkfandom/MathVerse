using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Quantities;
using MathVerse.Math.Foundation.Units;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class QuantityBenchmarks
{
    private QuantityFactory _factory = null!;
    private QuantityParser _parser = null!;
    private QuantityFormatter _formatter = null!;
    private Unit _meter = null!;
    private Unit _kilogram = null!;
    private Unit _second = null!;
    private Unit _newton = null!;
    private Unit _joule = null!;
    private Unit _watt = null!;
    private PhysicalQuantity _tenMeters = null!;
    private PhysicalQuantity _fiveMeters = null!;
    private PhysicalQuantity _twoKilograms = null!;
    private PhysicalQuantity _threeSeconds = null!;
    private PhysicalQuantity _tenNewtons = null!;
    private PhysicalQuantity _twentyJoules = null!;

    [GlobalSetup]
    public void Setup()
    {
        _factory = QuantityFactory.Instance;
        _parser = QuantityParser.Instance;
        _formatter = QuantityFormatter.Instance;
        _meter = UnitRegistry.Instance.Get("m")!;
        _kilogram = UnitRegistry.Instance.Get("kg")!;
        _second = UnitRegistry.Instance.Get("s")!;
        _newton = UnitRegistry.Instance.Get("N")!;
        _joule = UnitRegistry.Instance.Get("J")!;
        _watt = UnitRegistry.Instance.Get("W")!;
        _tenMeters = _factory.Create(10.0, _meter);
        _fiveMeters = _factory.Create(5.0, _meter);
        _twoKilograms = _factory.Create(2.0, _kilogram);
        _threeSeconds = _factory.Create(3.0, _second);
        _tenNewtons = _factory.Create(10.0, _newton);
        _twentyJoules = _factory.Create(20.0, _joule);
    }

    [BenchmarkCategory("Factory"), Benchmark(Baseline = true)]
    public PhysicalQuantity Create_10Meter()
    {
        return _factory.Create(10.0, _meter);
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity Create_2Kilogram()
    {
        return _factory.Create(2.0, _kilogram);
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity Create_3Second()
    {
        return _factory.Create(3.0, _second);
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity Create_9point8Newton()
    {
        return _factory.Create(9.8, _newton);
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity CreateBySymbol_Meter()
    {
        return _factory.Create(1.0, "m");
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity CreateBySymbol_Newton()
    {
        return _factory.Create(1.0, "N");
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity Zero_Meter()
    {
        return _factory.Zero(_meter);
    }

    [BenchmarkCategory("Factory"), Benchmark]
    public PhysicalQuantity One_Newton()
    {
        return _factory.One(_newton);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public bool TryParse_10m()
    {
        return _parser.TryParse("10 m", out _);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public bool TryParse_9point8N()
    {
        return _parser.TryParse("9.8 N", out _);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public bool TryParse_100kg()
    {
        return _parser.TryParse("100 kg", out _);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public bool TryParse_InvalidInput()
    {
        return _parser.TryParse("abc", out _);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public bool TryParse_NumberOnly()
    {
        return _parser.TryParse("42", out _);
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public PhysicalQuantity Parse_10m()
    {
        return _parser.Parse("10 m");
    }

    [BenchmarkCategory("Parser"), Benchmark]
    public PhysicalQuantity Parse_9point8N()
    {
        return _parser.Parse("9.8 N");
    }

    [BenchmarkCategory("Formatter"), Benchmark(Baseline = true)]
    public string Format_10Meter()
    {
        return _formatter.Format(_tenMeters);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string Format_2Kilogram()
    {
        return _formatter.Format(_twoKilograms);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string Format_10Newton()
    {
        return _formatter.Format(_tenNewtons);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatWithPrecision_10Meter_2()
    {
        return _formatter.FormatWithPrecision(_tenMeters, 2);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatWithPrecision_9point8Newton_4()
    {
        var qty = _factory.Create(9.8, _newton);
        return _formatter.FormatWithPrecision(qty, 4);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatScientific_10Meter()
    {
        return _formatter.FormatScientific(_tenMeters);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string FormatScientific_2Kilogram_5()
    {
        return _formatter.FormatScientific(_twoKilograms, 5);
    }

    [BenchmarkCategory("Formatter"), Benchmark]
    public string ToString_20Joule()
    {
        return _twentyJoules.ToString();
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Add_10m_5m()
    {
        return _tenMeters + _fiveMeters;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Subtract_10m_5m()
    {
        return _tenMeters - _fiveMeters;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Multiply_Scalar_10m()
    {
        return _tenMeters * 3.0;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Multiply_ScalarReverse_10m()
    {
        return 3.0 * _tenMeters;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Divide_Scalar_10m()
    {
        return _tenMeters / 2.0;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Negate_10m()
    {
        return -_tenMeters;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Multiply_10m_5m()
    {
        return _tenMeters * _fiveMeters;
    }

    [BenchmarkCategory("Arithmetic"), Benchmark]
    public PhysicalQuantity Divide_10m_5m()
    {
        return _tenMeters / _fiveMeters;
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public PhysicalQuantity ConvertTo_BaseUnit()
    {
        return _tenMeters.ToBase();
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool IsDimensionallyCompatible_Same()
    {
        return _tenMeters.IsDimensionallyCompatible(_fiveMeters);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public bool IsDimensionallyCompatible_Different()
    {
        return _tenMeters.IsDimensionallyCompatible(_twoKilograms);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public int CompareTo_Greater()
    {
        return _tenMeters.CompareTo(_fiveMeters);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public int CompareTo_Less()
    {
        return _fiveMeters.CompareTo(_tenMeters);
    }

    [BenchmarkCategory("Comparison"), Benchmark]
    public int CompareTo_Equal()
    {
        return _tenMeters.CompareTo(_factory.Create(10.0, _meter));
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public double Quantity_Value()
    {
        return _tenMeters.Value;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public Unit Quantity_Unit()
    {
        return _tenMeters.Unit;
    }

    [BenchmarkCategory("Properties"), Benchmark]
    public Dimension Quantity_Dimension()
    {
        return _tenMeters.Dimension;
    }
}
