using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CachingBenchmarks
{
    private EvaluationCache<Expression> _evalCache = null!;
    private RewriteCache _rewriteCache = null!;
    private SimplificationCache _simplificationCache = null!;
    private TypeInferenceCache _typeCache = null!;
    private MemoizationEngine _memoization = null!;
    private Expression[] _expressions = null!;
    private Expression[] _rewrittenExpressions = null!;
    private int _counter;

    [Params(100, 1000)]
    public int CacheSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _evalCache = new EvaluationCache<Expression>(CacheSize);
        _rewriteCache = new RewriteCache();
        _simplificationCache = new SimplificationCache();
        _typeCache = new TypeInferenceCache();
        _memoization = new MemoizationEngine();
        _counter = 0;

        _expressions = new Expression[CacheSize];
        _rewrittenExpressions = new Expression[CacheSize];
        for (var i = 0; i < CacheSize; i++)
        {
            _expressions[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));
            _rewrittenExpressions[i] = Expr.Multiply(Expr.Variable("x"), Expr.Literal(i));
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("EvalCache")]
    public void EvalCache_Store()
    {
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.Store($"expr_{i}", _expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("EvalCache")]
    public void EvalCache_StoreAndRetrieve()
    {
        _evalCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.Store($"expr_{i}", _expressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.TryGet($"expr_{i}", out _);
    }

    [Benchmark]
    [BenchmarkCategory("EvalCache")]
    public void EvalCache_CacheMiss()
    {
        _evalCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.TryGet($"missing_{i}", out _);
    }

    [Benchmark]
    [BenchmarkCategory("EvalCache")]
    public void EvalCache_Eviction()
    {
        _evalCache.Clear();
        for (var i = 0; i < CacheSize * 2; i++)
            _evalCache.Store($"expr_{i}", _expressions[i % CacheSize]);
    }

    [Benchmark]
    [BenchmarkCategory("EvalCache")]
    public CacheStatistics EvalCache_GetStatistics()
    {
        _evalCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.Store($"expr_{i}", _expressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.TryGet($"expr_{i}", out _);
        return _evalCache.Statistics;
    }

    [Benchmark]
    [BenchmarkCategory("RewriteCache")]
    public void RewriteCache_StoreAndLookup()
    {
        _rewriteCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _rewriteCache.Store(_expressions[i], _rewrittenExpressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _rewriteCache.GetRewritten(_expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("RewriteCache")]
    public void RewriteCache_Miss()
    {
        _rewriteCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _rewriteCache.GetRewritten(_expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("RewriteCache")]
    public bool RewriteCache_IsCached()
    {
        _rewriteCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _rewriteCache.Store(_expressions[i], _rewrittenExpressions[i]);
        var result = true;
        for (var i = 0; i < _expressions.Length; i++)
            result &= _rewriteCache.IsCached(_expressions[i]);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("SimplificationCache")]
    public void SimplificationCache_StoreAndLookup()
    {
        _simplificationCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _simplificationCache.Store(_expressions[i], _rewrittenExpressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _simplificationCache.GetSimplified(_expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("Memoization")]
    public void Memoization_HitPath()
    {
        _memoization.ClearAll();
        for (var i = 0; i < _expressions.Length; i++)
            _memoization.Memoize($"memo_{i}", () => _expressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _memoization.Memoize($"memo_{i}", () => Expr.Literal(999.0));
    }

    [Benchmark]
    [BenchmarkCategory("Memoization")]
    public void Memoization_MissPath()
    {
        _memoization.ClearAll();
        for (var i = 0; i < _expressions.Length; i++)
            _memoization.Memoize($"unique_{Interlocked.Increment(ref _counter)}", () => _expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("Memoization")]
    public void Memoization_WithArgument()
    {
        _memoization.ClearAll();
        for (var i = 0; i < _expressions.Length; i++)
            _memoization.Memoize(i, (int n) => Expr.Add(Expr.Literal(n), Expr.Variable("x")));
    }

    [Benchmark]
    [BenchmarkCategory("TypeCache")]
    public void TypeCache_StoreAndLookup()
    {
        _typeCache.Clear();
        var type = new MathVerse.Math.Types.RealType();
        for (var i = 0; i < _expressions.Length; i++)
            _typeCache.StoreInferredType(_expressions[i], type);
        for (var i = 0; i < _expressions.Length; i++)
            _typeCache.GetInferredType(_expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("TypeCache")]
    public void TypeCache_Miss()
    {
        _typeCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _typeCache.GetInferredType(_expressions[i]);
    }

    [Benchmark]
    [BenchmarkCategory("Invalidation")]
    public void EvalCache_Invalidate()
    {
        _evalCache.Clear();
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.Store($"expr_{i}", _expressions[i]);
        for (var i = 0; i < _expressions.Length; i++)
            _evalCache.Invalidate($"expr_{i}");
    }

    [Benchmark]
    [BenchmarkCategory("Invalidation")]
    public void RewriteCache_Clear()
    {
        for (var i = 0; i < _expressions.Length; i++)
            _rewriteCache.Store(_expressions[i], _rewrittenExpressions[i]);
        _rewriteCache.Clear();
    }
}
