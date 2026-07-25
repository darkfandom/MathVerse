using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SemanticAnalysisBenchmarks
{
    private SemanticAnalyzer _analyzer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _analyzer = new SemanticAnalyzer();
    }

    [BenchmarkCategory("Analyze"), Benchmark(Baseline = true)]
    public SemanticModel Analyze_SimpleLiteral() => _analyzer.Analyze("42");

    [BenchmarkCategory("Analyze"), Benchmark]
    public SemanticModel Analyze_Arithmetic() => _analyzer.Analyze("2 + 3 * 4 - 5 / 6");

    [BenchmarkCategory("Analyze"), Benchmark]
    public SemanticModel Analyze_NestedFunction() => _analyzer.Analyze("sin(cos(tan(x)))");

    [BenchmarkCategory("Analyze"), Benchmark]
    public SemanticModel Analyze_PowerExpression() => _analyzer.Analyze("2 ^ 10 + sqrt(144)");

    [BenchmarkCategory("Analyze"), Benchmark]
    public SemanticModel Analyze_ConstantFolding() => _analyzer.Analyze("(2 + 3) * 4 - 6 / 2");

    [BenchmarkCategory("ConstantFolding"), Benchmark(Baseline = true)]
    public double? Fold_SimpleExpression() => _analyzer.Analyze("2 + 3").EvaluateConstant();

    [BenchmarkCategory("ConstantFolding"), Benchmark]
    public double? Fold_NestedExpression() => _analyzer.Analyze("(2 + 3) * 4").EvaluateConstant();

    [BenchmarkCategory("ConstantFolding"), Benchmark]
    public double? Fold_FunctionCall() => _analyzer.Analyze("sqrt(144)").EvaluateConstant();

    [BenchmarkCategory("ConstantFolding"), Benchmark]
    public double? Fold_Trigonometric() => _analyzer.Analyze("sin(0) + cos(0)").EvaluateConstant();

    [BenchmarkCategory("ConstantFolding"), Benchmark]
    public double? Fold_FactorialExpression() => _analyzer.Analyze("5!").EvaluateConstant();
}
