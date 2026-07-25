using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation;
using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Constants;
using MathVerse.Math.Foundation.Conversion;
using MathVerse.Math.Foundation.Domains;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Foundation.Quantities;
using MathVerse.Math.Foundation.Units;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EngineBenchmarks
{
    private FoundationEngine _engine = null!;
    private Unit _meter = null!;
    private Unit _kilogram = null!;
    private Unit _second = null!;
    private Unit _newton = null!;
    private Unit _joule = null!;
    private Expression _addExpr = null!;
    private Expression _multiplyExpr = null!;
    private Expression _divideExpr = null!;
    private Expression _complexExpr = null!;

    [GlobalSetup]
    public void Setup()
    {
        _engine = new FoundationEngine();
        _meter = UnitRegistry.Instance.Get("m")!;
        _kilogram = UnitRegistry.Instance.Get("kg")!;
        _second = UnitRegistry.Instance.Get("s")!;
        _newton = UnitRegistry.Instance.Get("N")!;
        _joule = UnitRegistry.Instance.Get("J")!;

        _addExpr = new BinaryExpression(
            MathOperator.Multiply,
            new VariableExpression("force"),
            new VariableExpression("distance"));
        _multiplyExpr = new BinaryExpression(
            MathOperator.Multiply,
            new VariableExpression("mass"),
            new VariableExpression("acceleration"));
        _divideExpr = new BinaryExpression(
            MathOperator.Divide,
            new VariableExpression("energy"),
            new VariableExpression("time"));
        _complexExpr = new BinaryExpression(
            MathOperator.Add,
            new BinaryExpression(MathOperator.Multiply, new VariableExpression("F"), new VariableExpression("d")),
            new BinaryExpression(MathOperator.Divide,
                new BinaryExpression(MathOperator.Multiply, new LiteralExpression(0.5), new VariableExpression("m")),
                new VariableExpression("t")));

        _engine.AnalyzeExpression(_addExpr);
        _engine.AnalyzeExpression(_multiplyExpr);
    }

    [BenchmarkCategory("Domain"), Benchmark(Baseline = true)]
    public MathDomain? Engine_GetDomain_Real()
    {
        return _engine.GetDomain(DomainKind.Real);
    }

    [BenchmarkCategory("Domain"), Benchmark]
    public MathDomain? Engine_GetDomain_Integer()
    {
        return _engine.GetDomain(DomainKind.Integer);
    }

    [BenchmarkCategory("Domain"), Benchmark]
    public MathDomain? Engine_GetDomain_Complex()
    {
        return _engine.GetDomain(DomainKind.Complex);
    }

    [BenchmarkCategory("Domain"), Benchmark]
    public MathDomain? Engine_GetDomain_ByString()
    {
        return _engine.GetDomain("Real");
    }

    [BenchmarkCategory("Domain"), Benchmark]
    public bool Engine_AreDomainsCompatible()
    {
        var real = _engine.GetDomain(DomainKind.Real)!;
        var integer = _engine.GetDomain(DomainKind.Integer)!;
        return _engine.AreDomainsCompatible(real, integer);
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public MathConstant? Engine_GetConstant_Pi()
    {
        return _engine.GetConstant("Pi");
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public MathConstant? Engine_GetConstant_E()
    {
        return _engine.GetConstant("E");
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public double Engine_GetConstantValue_Pi()
    {
        return _engine.GetConstantValue("Pi");
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public double Engine_GetConstantValue_E()
    {
        return _engine.GetConstantValue("E");
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public bool Engine_TryGetConstant_Pi()
    {
        return _engine.TryGetConstant("Pi", out _);
    }

    [BenchmarkCategory("Constant"), Benchmark]
    public bool Engine_TryGetConstant_NotFound()
    {
        return _engine.TryGetConstant("NonExistent", out _);
    }

    [BenchmarkCategory("Unit"), Benchmark]
    public Unit? Engine_GetUnit_Meter()
    {
        return _engine.GetUnit("m");
    }

    [BenchmarkCategory("Unit"), Benchmark]
    public Unit? Engine_GetUnit_Newton()
    {
        return _engine.GetUnit("N");
    }

    [BenchmarkCategory("Unit"), Benchmark]
    public IReadOnlyList<Unit> Engine_GetUnitsByCategory_Length()
    {
        return _engine.GetUnitsByCategory(UnitCategory.Length);
    }

    [BenchmarkCategory("Unit"), Benchmark]
    public IReadOnlyList<Unit> Engine_GetUnitsByDimension_Force()
    {
        return _engine.GetUnitsByDimension(DerivedDimension.Force);
    }

    [BenchmarkCategory("Quantity"), Benchmark]
    public PhysicalQuantity Engine_CreateQuantity_10m()
    {
        return _engine.CreateQuantity(10.0, "m");
    }

    [BenchmarkCategory("Quantity"), Benchmark]
    public PhysicalQuantity Engine_CreateQuantity_9point8N()
    {
        return _engine.CreateQuantity(9.8, "N");
    }

    [BenchmarkCategory("Quantity"), Benchmark]
    public PhysicalQuantity Engine_Convert()
    {
        var qty = _engine.CreateQuantity(1.0, "km");
        return _engine.Convert(qty, _meter);
    }

    [BenchmarkCategory("Analysis"), Benchmark]
    public Dimension? Engine_AnalyzeExpression()
    {
        return _engine.AnalyzeExpression(_addExpr);
    }

    [BenchmarkCategory("Analysis"), Benchmark]
    public bool Engine_CheckConsistency()
    {
        return _engine.CheckConsistency(_multiplyExpr);
    }

    [BenchmarkCategory("Analysis"), Benchmark]
    public IReadOnlyList<DimensionDiagnostic> Engine_GetDiagnostics()
    {
        return _engine.GetDiagnostics(_complexExpr);
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public ConversionResult Engine_Convert_km_m()
    {
        return _engine.Convert(1.0, "km", "m");
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public ConversionResult Engine_Convert_kg_g()
    {
        return _engine.Convert(1.0, "kg", "g");
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public ConversionResult Engine_Convert_h_s()
    {
        return _engine.Convert(1.0, "h", "s");
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public bool Engine_CanConvert_km_m()
    {
        return _engine.CanConvert("km", "m");
    }

    [BenchmarkCategory("Conversion"), Benchmark]
    public bool Engine_CanConvert_NotFound()
    {
        return _engine.CanConvert("m", "kg");
    }

    [BenchmarkCategory("Workflow"), Benchmark]
    public string FullWorkflow_ParseConvertFormat()
    {
        var qty = _engine.CreateQuantity(1.0, "km");
        var converted = _engine.Convert(qty, _meter);
        return converted.ToString();
    }

    [BenchmarkCategory("Workflow"), Benchmark]
    public string FullWorkflow_CreateAnalyzeToString()
    {
        var qty = _engine.CreateQuantity(9.8, "N");
        return qty.ToString();
    }

    [BenchmarkCategory("Workflow"), Benchmark]
    public string FullWorkflow_ConstantLookupFormat()
    {
        var val = _engine.GetConstantValue("Pi");
        return val.ToString("F10");
    }

    [BenchmarkCategory("Workflow"), Benchmark]
    public bool FullWorkflow_CreateConvertCheck()
    {
        var qty = _engine.CreateQuantity(100.0, "km");
        var converted = _engine.Convert(qty, _meter);
        return converted.Value > 0;
    }

    [BenchmarkCategory("Clear"), Benchmark]
    public void Engine_Clear()
    {
        _engine.Clear();
    }

    [BenchmarkCategory("Services"), Benchmark]
    public FoundationServices Engine_Services()
    {
        return _engine.Services;
    }
}
