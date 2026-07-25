using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class InterningBenchmarks
{
    private ExpressionInterner _interner = null!;
    private Expression _literal = null!;
    private Expression _binary = null!;
    private Expression _complexTree = null!;
    private Expression[] _duplicateTrees = null!;
    private Expression[] _uniqueTrees = null!;

    [Params(50, 200)]
    public int TreeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _interner = new ExpressionInterner();
        _literal = Expr.Literal(42.0);
        _binary = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        _complexTree = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))),
            Expr.Sin(Expr.Add(Expr.Variable("z"), Expr.Literal(2.0))));

        _duplicateTrees = new Expression[TreeCount];
        for (var i = 0; i < TreeCount; i++)
            _duplicateTrees[i] = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));

        _uniqueTrees = new Expression[TreeCount];
        for (var i = 0; i < TreeCount; i++)
            _uniqueTrees[i] = Expr.Add(Expr.Variable($"v{i}"), Expr.Literal(i));
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Single")]
    public Expression Intern_Literal() => _interner.Intern(_literal);

    [Benchmark]
    [BenchmarkCategory("Single")]
    public Expression Intern_Binary() => _interner.Intern(_binary);

    [Benchmark]
    [BenchmarkCategory("Single")]
    public Expression Intern_ComplexTree() => _interner.Intern(_complexTree);

    [Benchmark]
    [BenchmarkCategory("Tree")]
    public int Intern_DuplicateTrees()
    {
        _interner.Clear();
        for (var i = 0; i < _duplicateTrees.Length; i++)
            _interner.Intern(_duplicateTrees[i]);
        return _interner.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Tree")]
    public int Intern_UniqueTrees()
    {
        _interner.Clear();
        for (var i = 0; i < _uniqueTrees.Length; i++)
            _interner.Intern(_uniqueTrees[i]);
        return _interner.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Lookup")]
    public Expression Intern_LookupHit()
    {
        _interner.Clear();
        _interner.Intern(_complexTree);
        return _interner.Intern(Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))),
            Expr.Sin(Expr.Add(Expr.Variable("z"), Expr.Literal(2.0)))));
    }

    [Benchmark]
    [BenchmarkCategory("Lookup")]
    public Expression Intern_LookupMiss()
    {
        _interner.Clear();
        _interner.Intern(_complexTree);
        return _interner.Intern(Expr.Add(
            Expr.Multiply(Expr.Variable("a"), Expr.Pow(Expr.Variable("b"), Expr.Literal(3.0))),
            Expr.Sin(Expr.Add(Expr.Variable("c"), Expr.Literal(2.0)))));
    }

    [Benchmark]
    [BenchmarkCategory("Statistics")]
    public InternStatistics GetStatistics_AfterWork()
    {
        _interner.Clear();
        for (var i = 0; i < _duplicateTrees.Length; i++)
            _interner.Intern(_duplicateTrees[i]);
        for (var i = 0; i < _uniqueTrees.Length; i++)
            _interner.Intern(_uniqueTrees[i]);
        return _interner.Statistics;
    }

    [Benchmark]
    [BenchmarkCategory("Cache")]
    public void ExpressionCache_AddAndLookup()
    {
        var cache = new ExpressionCache();
        for (var i = 0; i < _duplicateTrees.Length; i++)
            cache.Add(_duplicateTrees[i]);
        for (var i = 0; i < _duplicateTrees.Length; i++)
            cache.TryGet(_duplicateTrees[i], out _);
    }

    [Benchmark]
    [BenchmarkCategory("Cache")]
    public void ExpressionCache_IdentityComparison()
    {
        var identity = ExpressionIdentity.Instance;
        var same = ReferenceEquals(_binary, _binary);
        var diff = ReferenceEquals(_binary, Expr.Add(Expr.Variable("x"), Expr.Literal(1.0)));
        _ = same;
        _ = diff;
    }

    [Benchmark]
    [BenchmarkCategory("Clear")]
    public void Intern_ClearAndRepopulate()
    {
        for (var cycle = 0; cycle < 10; cycle++)
        {
            _interner.Clear();
            for (var i = 0; i < _uniqueTrees.Length; i++)
                _interner.Intern(_uniqueTrees[i]);
        }
    }
}
