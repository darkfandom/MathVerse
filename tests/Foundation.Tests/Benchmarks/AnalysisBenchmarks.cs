using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Analysis;
using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class AnalysisBenchmarks
{
    private DimensionAnalyzer _analyzer = null!;
    private Expression _literal = null!;
    private Expression _variable = null!;
    private Expression _constant = null!;
    private Expression _addExpression = null!;
    private Expression _multiplyExpression = null!;
    private Expression _divideExpression = null!;
    private Expression _powerExpression = null!;
    private Expression _nestedExpression = null!;
    private Expression _functionExpression = null!;
    private Expression _unaryExpression = null!;
    private Expression _complexNested = null!;

    [GlobalSetup]
    public void Setup()
    {
        _analyzer = DimensionAnalyzer.Instance;
        _literal = new LiteralExpression(42.0);
        _variable = new VariableExpression("x");
        _constant = new ConstantExpression("pi", Math.PI);
        _addExpression = new BinaryExpression(MathOperator.Add, new VariableExpression("a"), new VariableExpression("b"));
        _multiplyExpression = new BinaryExpression(MathOperator.Multiply, new VariableExpression("x"), new VariableExpression("y"));
        _divideExpression = new BinaryExpression(MathOperator.Divide, new VariableExpression("distance"), new VariableExpression("time"));
        _powerExpression = new BinaryExpression(MathOperator.Power, new VariableExpression("x"), new LiteralExpression(2.0));
        _nestedExpression = new BinaryExpression(
            MathOperator.Multiply,
            new BinaryExpression(MathOperator.Add, new VariableExpression("a"), new VariableExpression("b")),
            new VariableExpression("c"));
        _functionExpression = new FunctionCallExpression("sqrt", new[] { new VariableExpression("x") });
        _unaryExpression = new UnaryExpression(MathOperator.Negate, new VariableExpression("x"));
        _complexNested = new BinaryExpression(
            MathOperator.Add,
            new BinaryExpression(
                MathOperator.Multiply,
                new VariableExpression("force"),
                new VariableExpression("distance")),
            new BinaryExpression(
                MathOperator.Divide,
                new BinaryExpression(MathOperator.Multiply,
                    new LiteralExpression(0.5),
                    new VariableExpression("mass")),
                new VariableExpression("time")));

        _analyzer.Clear();
        _analyzer.SetVariableDimension("a", Dimension.FromBaseDimensions(length: 1));
        _analyzer.SetVariableDimension("b", Dimension.FromBaseDimensions(length: 1));
        _analyzer.SetVariableDimension("x", Dimension.FromBaseDimensions(length: 1));
        _analyzer.SetVariableDimension("y", Dimension.FromBaseDimensions(time: -1));
        _analyzer.SetVariableDimension("distance", Dimension.FromBaseDimensions(length: 1));
        _analyzer.SetVariableDimension("time", Dimension.FromBaseDimensions(time: 1));
        _analyzer.SetVariableDimension("c", Dimension.FromBaseDimensions(mass: 1));
        _analyzer.SetVariableDimension("force", DerivedDimension.Force);
        _analyzer.SetVariableDimension("mass", Dimension.FromBaseDimensions(mass: 1));
    }

    [BenchmarkCategory("Analyze"), Benchmark(Baseline = true)]
    public Dimension Analyze_Literal()
    {
        return _analyzer.AnalyzeExpression(_literal);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_Variable()
    {
        return _analyzer.AnalyzeExpression(_variable);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_Constant()
    {
        return _analyzer.AnalyzeExpression(_constant);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_AddExpression()
    {
        return _analyzer.AnalyzeExpression(_addExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_MultiplyExpression()
    {
        return _analyzer.AnalyzeExpression(_multiplyExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_DivideExpression()
    {
        return _analyzer.AnalyzeExpression(_divideExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_PowerExpression()
    {
        return _analyzer.AnalyzeExpression(_powerExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_NestedExpression()
    {
        return _analyzer.AnalyzeExpression(_nestedExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_FunctionExpression()
    {
        return _analyzer.AnalyzeExpression(_functionExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_UnaryExpression()
    {
        return _analyzer.AnalyzeExpression(_unaryExpression);
    }

    [BenchmarkCategory("Analyze"), Benchmark]
    public Dimension Analyze_ComplexNested()
    {
        return _analyzer.AnalyzeExpression(_complexNested);
    }

    [BenchmarkCategory("Check"), Benchmark]
    public Dimension CheckDimensionalConsistency_Simple()
    {
        return _analyzer.CheckDimensionalConsistency(_addExpression);
    }

    [BenchmarkCategory("Check"), Benchmark]
    public Dimension CheckDimensionalConsistency_Nested()
    {
        return _analyzer.CheckDimensionalConsistency(_nestedExpression);
    }

    [BenchmarkCategory("Check"), Benchmark]
    public Dimension CheckDimensionalConsistency_Complex()
    {
        return _analyzer.CheckDimensionalConsistency(_complexNested);
    }

    [BenchmarkCategory("Diagnostics"), Benchmark]
    public bool HasErrors_AfterCheck()
    {
        _analyzer.CheckDimensionalConsistency(_addExpression);
        return _analyzer.Diagnostics.HasErrors;
    }

    [BenchmarkCategory("Diagnostics"), Benchmark]
    public IReadOnlyList<DimensionDiagnostic> GetDiagnostics_AfterCheck()
    {
        _analyzer.CheckDimensionalConsistency(_complexNested);
        return _analyzer.Diagnostics.Diagnostics;
    }

    [BenchmarkCategory("Variables"), Benchmark]
    public void SetVariableDimension_New()
    {
        _analyzer.SetVariableDimension("newVar", Dimension.FromBaseDimensions(length: 2));
    }

    [BenchmarkCategory("Variables"), Benchmark]
    public Dimension GetVariableDimension_Existing()
    {
        return _analyzer.GetVariableDimension("a");
    }

    [BenchmarkCategory("Variables"), Benchmark]
    public Dimension GetVariableDimension_NonExisting()
    {
        return _analyzer.GetVariableDimension("nonexistent");
    }

    [BenchmarkCategory("Variables"), Benchmark]
    public void Clear_All()
    {
        _analyzer.Clear();
    }

    [BenchmarkCategory("GetResult"), Benchmark]
    public Dimension GetResult_Literal()
    {
        return _analyzer.GetResultDimension(_literal);
    }

    [BenchmarkCategory("GetResult"), Benchmark]
    public Dimension GetResult_Multiply()
    {
        return _analyzer.GetResultDimension(_multiplyExpression);
    }

    [BenchmarkCategory("GetResult"), Benchmark]
    public Dimension GetResult_Divide()
    {
        return _analyzer.GetResultDimension(_divideExpression);
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? Infer_Add()
    {
        return DimensionInferenceEngine.InferFromContext("+", new[] { DerivedDimension.Force, DerivedDimension.Force });
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? Infer_Multiply()
    {
        return DimensionInferenceEngine.InferFromContext("*", new[] { DerivedDimension.Force, DerivedDimension.Velocity });
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? Infer_Divide()
    {
        return DimensionInferenceEngine.InferFromContext("/", new[] { DerivedDimension.Energy, DerivedDimension.Time });
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? Infer_Sqrt()
    {
        return DimensionInferenceEngine.InferFromContext("sqrt", new[] { DerivedDimension.Area });
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? Infer_Sin()
    {
        return DimensionInferenceEngine.InferFromContext("sin", new[] { DerivedDimension.Angle });
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? InferBinary_Add_Compatible()
    {
        return DimensionInferenceEngine.InferBinaryDimension(MathOperator.Add, DerivedDimension.Force, DerivedDimension.Force);
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? InferBinary_Multiply()
    {
        return DimensionInferenceEngine.InferBinaryDimension(MathOperator.Multiply, DerivedDimension.Force, DerivedDimension.Velocity);
    }

    [BenchmarkCategory("Inference"), Benchmark]
    public Dimension? InferBinary_Divide()
    {
        return DimensionInferenceEngine.InferBinaryDimension(MathOperator.Divide, DerivedDimension.Energy, DerivedDimension.Time);
    }
}
