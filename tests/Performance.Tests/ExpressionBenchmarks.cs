using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using MathVerse.Math.Visitors;
using MathVerse.Math.Rewriting;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ExpressionCreationBenchmarks
{
    [Benchmark(Baseline = true)]
    public Expression CreateSimpleExpression() =>
        Expr.Add(Expr.Literal(1), Expr.Literal(2));

    [Benchmark]
    public Expression CreateNestedExpression() =>
        Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Literal(3)),
            Expr.Pow(Expr.Variable("y"), Expr.Literal(2)));

    [Benchmark]
    public Expression CreateComplexExpression() =>
        Expr.Derivative(Expr.Sin(Expr.Variable("x")), Expr.Variable("x"));

    [Benchmark]
    public Expression CreateLargeExpression()
    {
        Expression result = Expr.Literal(0);
        for (var i = 1; i <= 10; i++)
            result = Expr.Add(result, Expr.Literal(i));
        return result;
    }
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ExpressionVisitorBenchmarks
{
    private Expression _simple = null!;
    private Expression _complex = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simple = Expr.Add(Expr.Literal(1), Expr.Literal(2));

        _complex = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3))),
            Expr.Sin(Expr.Variable("z")));
    }

    [Benchmark(Baseline = true)]
    public Expression Clone_SimpleTree() => ExpressionCloner.Clone(_simple);

    [Benchmark]
    public Expression Clone_ComplexTree() => ExpressionCloner.Clone(_complex);

    [Benchmark]
    public string Print_SimpleTree() => ExpressionPrinter.Print(_simple);

    [Benchmark]
    public string Print_ComplexTree() => ExpressionPrinter.Print(_complex);

    [Benchmark]
    public int CountNodes_ComplexTree() => ExpressionNodeCounter.Count(_complex);
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ExpressionRewriteBenchmarks
{
    private Expression _simple = null!;
    private Expression _complex = null!;
    private RewriteEngine _engine = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simple = Expr.Add(Expr.Literal(1), Expr.Literal(2));

        _complex = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3))),
            Expr.Sin(Expr.Variable("z")));

        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "Identity Addition",
            expr => expr is BinaryExpression b && b.Operator == MathOperator.Add &&
                    b.Left is LiteralExpression l && l.Value == 0,
            expr => ((BinaryExpression)expr).Right));
        _engine = new RewriteEngine(ruleSet);
    }

    [Benchmark(Baseline = true)]
    public Expression ReplaceVariable_SimpleTree() =>
        ExpressionReplacer.Replace(_simple, Expr.Variable("x"), Expr.Variable("y"));

    [Benchmark]
    public Expression ApplyOnce_SimpleRule() => _engine.ApplyOnce(_simple);

    [Benchmark]
    public Expression ApplyToFixpoint_SimpleRule() => _engine.ApplyToFixpoint(_complex);
}
