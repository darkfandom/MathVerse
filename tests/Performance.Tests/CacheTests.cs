using MathVerse.Math.Types;

namespace MathVerse.Performance.Tests;

public sealed class CacheTests
{
    [Fact]
    public void EvaluationCache_StoreAndRetrieve()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key1", 42);
        cache.TryGet("key1", out var result).Should().BeTrue();
        result.Should().Be(42);
    }

    [Fact]
    public void EvaluationCache_Miss()
    {
        var cache = new EvaluationCache<int>();

        cache.TryGet("missing", out _).Should().BeFalse();
    }

    [Fact]
    public void EvaluationCache_Overwrite()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key", 1);
        cache.Store("key", 2);
        cache.TryGet("key", out var result).Should().BeTrue();
        result.Should().Be(2);
    }

    [Fact]
    public void EvaluationCache_Invalidate()
    {
        var cache = new EvaluationCache<int>();
        cache.Store("key", 42);

        cache.Invalidate("key");

        cache.TryGet("key", out _).Should().BeFalse();
    }

    [Fact]
    public void EvaluationCache_Invalidate_NonExistent()
    {
        var cache = new EvaluationCache<int>();

        Action act = () => cache.Invalidate("missing");

        act.Should().NotThrow();
    }

    [Fact]
    public void EvaluationCache_Clear()
    {
        var cache = new EvaluationCache<int>();
        cache.Store("a", 1);
        cache.Store("b", 2);

        cache.Clear();

        cache.TryGet("a", out _).Should().BeFalse();
        cache.TryGet("b", out _).Should().BeFalse();
    }

    [Fact]
    public void EvaluationCache_CapacityEvictsOldest()
    {
        var cache = new EvaluationCache<int>(capacity: 3);

        cache.Store("a", 1);
        cache.Store("b", 2);
        cache.Store("c", 3);
        cache.Store("d", 4);

        cache.TryGet("a", out _).Should().BeFalse();
        cache.TryGet("b", out _).Should().BeTrue();
        cache.TryGet("d", out _).Should().BeTrue();
    }

    [Fact]
    public void EvaluationCache_LRU_TouchRefreshes()
    {
        var cache = new EvaluationCache<int>(capacity: 3);

        cache.Store("a", 1);
        cache.Store("b", 2);
        cache.Store("c", 3);
        cache.TryGet("a", out _);
        cache.Store("d", 4);

        cache.TryGet("a", out _).Should().BeTrue();
        cache.TryGet("b", out _).Should().BeFalse();
    }

    [Fact]
    public void EvaluationCache_Statistics()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key", 42);
        cache.TryGet("key", out _);
        cache.TryGet("missing", out _);

        var stats = cache.Statistics;
        stats.Hits.Should().Be(1);
        stats.Misses.Should().Be(1);
        stats.Count.Should().Be(1);
    }

    [Fact]
    public void EvaluationCache_StatisticsHitRatio()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key", 42);
        cache.TryGet("key", out _);
        cache.TryGet("key", out _);
        cache.TryGet("missing", out _);

        var stats = cache.Statistics;
        stats.HitRatio.Should().BeApproximately(2.0 / 3.0, 0.01);
    }

    [Fact]
    public void EvaluationCache_StatisticsAfterClear()
    {
        var cache = new EvaluationCache<int>();
        cache.Store("key", 42);
        cache.TryGet("key", out _);

        cache.Clear();
        var stats = cache.Statistics;

        stats.Hits.Should().Be(0);
        stats.Misses.Should().Be(0);
        stats.Evictions.Should().Be(0);
        stats.Count.Should().Be(0);
    }

    [Fact]
    public void EvaluationCache_Capacity()
    {
        var cache = new EvaluationCache<int>(capacity: 5);

        cache.Statistics.Capacity.Should().Be(5);
    }

    [Fact]
    public void EvaluationCache_ZeroCapacity_Throws()
    {
        Action act = () => new EvaluationCache<int>(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EvaluationCache_NegativeCapacity_Throws()
    {
        Action act = () => new EvaluationCache<int>(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EvaluationCache_NullKey_Throws()
    {
        var cache = new EvaluationCache<int>();
        Action act = () => cache.Store(null!, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EvaluationCache_EmptyKey_Throws()
    {
        var cache = new EvaluationCache<int>();
        Action act = () => cache.Store("", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EvaluationCache_WhitespaceKey_Throws()
    {
        var cache = new EvaluationCache<int>();
        Action act = () => cache.Store("   ", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EvaluationCache_TryGetNullKey_Throws()
    {
        var cache = new EvaluationCache<int>();
        Action act = () => cache.TryGet(null!, out _);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EvaluationCache_TtlExpiration()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key", 42, TimeSpan.FromMilliseconds(1));
        Thread.Sleep(50);

        cache.TryGet("key", out _).Should().BeFalse();
    }

    [Fact]
    public void EvaluationCache_NoExpiration()
    {
        var cache = new EvaluationCache<int>();

        cache.Store("key", 42);
        Thread.Sleep(10);

        cache.TryGet("key", out var result).Should().BeTrue();
        result.Should().Be(42);
    }

    [Fact]
    public void EvaluationCache_EvictionCount()
    {
        var cache = new EvaluationCache<int>(capacity: 2);

        cache.Store("a", 1);
        cache.Store("b", 2);
        cache.Store("c", 3);

        cache.Statistics.Evictions.Should().Be(1);
    }

    [Fact]
    public void EvaluationCache_StatCapacity()
    {
        var cache = new EvaluationCache<int>(capacity: 10);

        cache.Statistics.Capacity.Should().Be(10);
    }

    [Fact]
    public void EvaluationCache_StringValues()
    {
        var cache = new EvaluationCache<string>();

        cache.Store("key", "hello");
        cache.TryGet("key", out var result).Should().BeTrue();
        result.Should().Be("hello");
    }

    [Fact]
    public void EvaluationCache_ComplexValues()
    {
        var cache = new EvaluationCache<List<int>>();

        var list = new List<int> { 1, 2, 3 };
        cache.Store("key", list);

        cache.TryGet("key", out var result).Should().BeTrue();
        result.Should().BeSameAs(list);
    }

    [Fact]
    public void EvaluationCache_ManyEntries()
    {
        var cache = new EvaluationCache<int>(capacity: 1000);

        for (int i = 0; i < 500; i++)
            cache.Store($"key{i}", i);

        for (int i = 0; i < 500; i++)
        {
            cache.TryGet($"key{i}", out var result).Should().BeTrue();
            result.Should().Be(i);
        }
    }

    [Fact]
    public async Task EvaluationCache_ThreadSafety()
    {
        var cache = new EvaluationCache<int>(capacity: 100);
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                cache.Store($"key{i}", i);
                cache.TryGet($"key{i}", out _);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void RewriteCache_StoreAndGet()
    {
        var rewriteCache = new RewriteCache();
        var original = Expr.Add(Expr.Variable("x"), Expr.Literal(0.0));
        var rewritten = Expr.Variable("x");

        rewriteCache.Store(original, rewritten);

        rewriteCache.GetRewritten(original).Should().BeSameAs(rewritten);
    }

    [Fact]
    public void RewriteCache_Miss()
    {
        var rewriteCache = new RewriteCache();

        rewriteCache.GetRewritten(Expr.Literal(1.0)).Should().BeNull();
    }

    [Fact]
    public void RewriteCache_IsCached()
    {
        var rewriteCache = new RewriteCache();
        var original = Expr.Literal(42.0);

        rewriteCache.IsCached(original).Should().BeFalse();

        rewriteCache.Store(original, Expr.Literal(42.0));

        rewriteCache.IsCached(original).Should().BeTrue();
    }

    [Fact]
    public void RewriteCache_Clear()
    {
        var rewriteCache = new RewriteCache();
        var original = Expr.Literal(1.0);
        rewriteCache.Store(original, Expr.Literal(2.0));

        rewriteCache.Clear();

        rewriteCache.IsCached(original).Should().BeFalse();
    }

    [Fact]
    public void RewriteCache_Statistics()
    {
        var rewriteCache = new RewriteCache();
        var original = Expr.Literal(1.0);

        rewriteCache.GetRewritten(original);
        rewriteCache.Store(original, Expr.Literal(2.0));
        rewriteCache.GetRewritten(original);
        rewriteCache.GetRewritten(original);

        var stats = rewriteCache.Statistics;
        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(1);
    }

    [Fact]
    public void RewriteCache_StructuralEquality()
    {
        var rewriteCache = new RewriteCache();
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(0.0));
        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(0.0));
        var rewritten = Expr.Variable("x");

        rewriteCache.Store(a, rewritten);

        rewriteCache.GetRewritten(b).Should().BeSameAs(rewritten);
    }

    [Fact]
    public void RewriteCache_NullOriginal_Throws()
    {
        var rewriteCache = new RewriteCache();
        Action act = () => rewriteCache.Store(null!, Expr.Literal(1.0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RewriteCache_NullRewritten_Throws()
    {
        var rewriteCache = new RewriteCache();
        Action act = () => rewriteCache.Store(Expr.Literal(1.0), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RewriteCache_GetRewrittenNull_Throws()
    {
        var rewriteCache = new RewriteCache();
        Action act = () => rewriteCache.GetRewritten(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RewriteCache_IsCachedNull_Throws()
    {
        var rewriteCache = new RewriteCache();
        Action act = () => rewriteCache.IsCached(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RewriteCache_Overwrite()
    {
        var rewriteCache = new RewriteCache();
        var original = Expr.Literal(1.0);

        rewriteCache.Store(original, Expr.Literal(2.0));
        rewriteCache.Store(original, Expr.Literal(3.0));

        rewriteCache.GetRewritten(original).Should().Be(Expr.Literal(3.0));
    }

    [Fact]
    public void SimplificationCache_StoreAndGet()
    {
        var simplCache = new SimplificationCache();
        var original = Expr.Add(Expr.Literal(0.0), Expr.Variable("x"));
        var simplified = Expr.Variable("x");

        simplCache.Store(original, simplified);

        simplCache.GetSimplified(original).Should().BeSameAs(simplified);
    }

    [Fact]
    public void SimplificationCache_Miss()
    {
        var simplCache = new SimplificationCache();

        simplCache.GetSimplified(Expr.Literal(1.0)).Should().BeNull();
    }

    [Fact]
    public void SimplificationCache_Clear()
    {
        var simplCache = new SimplificationCache();
        var original = Expr.Literal(1.0);
        simplCache.Store(original, Expr.Literal(2.0));

        simplCache.Clear();

        simplCache.GetSimplified(original).Should().BeNull();
    }

    [Fact]
    public void SimplificationCache_Statistics()
    {
        var simplCache = new SimplificationCache();
        var original = Expr.Literal(1.0);

        simplCache.GetSimplified(original);
        simplCache.Store(original, Expr.Literal(2.0));
        simplCache.GetSimplified(original);

        var stats = simplCache.Statistics;
        stats.Hits.Should().Be(1);
        stats.Misses.Should().Be(1);
        stats.Count.Should().Be(1);
    }

    [Fact]
    public void SimplificationCache_StructuralEquality()
    {
        var simplCache = new SimplificationCache();
        var a = Expr.Add(Expr.Literal(0.0), Expr.Variable("x"));
        var b = Expr.Add(Expr.Literal(0.0), Expr.Variable("x"));
        var simplified = Expr.Variable("x");

        simplCache.Store(a, simplified);

        simplCache.GetSimplified(b).Should().BeSameAs(simplified);
    }

    [Fact]
    public void SimplificationCache_NullOriginal_Throws()
    {
        var simplCache = new SimplificationCache();
        Action act = () => simplCache.Store(null!, Expr.Literal(1.0));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SimplificationCache_NullSimplified_Throws()
    {
        var simplCache = new SimplificationCache();
        Action act = () => simplCache.Store(Expr.Literal(1.0), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SimplificationCache_ThreadSafety()
    {
        var simplCache = new SimplificationCache();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i);
                simplCache.Store(expr, Expr.Literal(i + 1000));
                simplCache.GetSimplified(expr);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void TypeInferenceCache_StoreAndGet()
    {
        var cache = new TypeInferenceCache();
        var expr = Expr.Literal(42.0);
        var type = RealType.Instance;

        cache.StoreInferredType(expr, type);

        cache.GetInferredType(expr).Should().BeSameAs(type);
    }

    [Fact]
    public void TypeInferenceCache_Miss()
    {
        var cache = new TypeInferenceCache();

        cache.GetInferredType(Expr.Literal(1.0)).Should().BeNull();
    }

    [Fact]
    public void TypeInferenceCache_Clear()
    {
        var cache = new TypeInferenceCache();
        var expr = Expr.Literal(1.0);
        cache.StoreInferredType(expr, RealType.Instance);

        cache.Clear();

        cache.GetInferredType(expr).Should().BeNull();
    }

    [Fact]
    public void TypeInferenceCache_Statistics()
    {
        var cache = new TypeInferenceCache();
        var expr = Expr.Literal(1.0);

        cache.GetInferredType(expr);
        cache.StoreInferredType(expr, RealType.Instance);
        cache.GetInferredType(expr);
        cache.GetInferredType(expr);

        var stats = cache.Statistics;
        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(1);
        stats.Count.Should().Be(1);
    }

    [Fact]
    public void TypeInferenceCache_StructuralEquality()
    {
        var cache = new TypeInferenceCache();
        var a = Expr.Literal(42.0);
        var b = Expr.Literal(42.0);

        cache.StoreInferredType(a, RealType.Instance);

        cache.GetInferredType(b).Should().BeSameAs(RealType.Instance);
    }

    [Fact]
    public void TypeInferenceCache_NullExpression_Throws()
    {
        var cache = new TypeInferenceCache();
        Action act = () => cache.StoreInferredType(null!, RealType.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TypeInferenceCache_NullType_Throws()
    {
        var cache = new TypeInferenceCache();
        Action act = () => cache.StoreInferredType(Expr.Literal(1.0), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TypeInferenceCache_GetNullExpression_Throws()
    {
        var cache = new TypeInferenceCache();
        Action act = () => cache.GetInferredType(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TypeInferenceCache_DifferentTypes()
    {
        var cache = new TypeInferenceCache();
        var intExpr = Expr.Literal(1.0);
        var realExpr = Expr.Variable("x");

        cache.StoreInferredType(intExpr, IntegerType.Instance);
        cache.StoreInferredType(realExpr, RealType.Instance);

        cache.GetInferredType(intExpr).Should().BeSameAs(IntegerType.Instance);
        cache.GetInferredType(realExpr).Should().BeSameAs(RealType.Instance);
    }

    [Fact]
    public void MemoizationEngine_MemoizeByKey()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        var result1 = engine.Memoize("key", () => { callCount++; return 42; });
        var result2 = engine.Memoize("key", () => { callCount++; return 42; });

        result1.Should().Be(42);
        result2.Should().Be(42);
        callCount.Should().Be(1);
    }

    [Fact]
    public void MemoizationEngine_DifferentKeys_CalledSeparately()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        engine.Memoize("a", () => { callCount++; return 1; });
        engine.Memoize("b", () => { callCount++; return 2; });

        callCount.Should().Be(2);
    }

    [Fact]
    public void MemoizationEngine_Invalidate()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        engine.Memoize("key", () => { callCount++; return 42; });
        engine.Invalidate("key");
        engine.Memoize("key", () => { callCount++; return 42; });

        callCount.Should().Be(2);
    }

    [Fact]
    public void MemoizationEngine_ClearAll()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        engine.Memoize("key", () => { callCount++; return 42; });
        engine.ClearAll();
        engine.Memoize("key", () => { callCount++; return 42; });

        callCount.Should().Be(2);
    }

    [Fact]
    public void MemoizationEngine_Statistics()
    {
        var engine = new MemoizationEngine();

        engine.Memoize("key", () => 42);
        engine.Memoize("key", () => 42);
        engine.Memoize("other", () => 42);
        engine.Memoize("other", () => 42);

        var stats = engine.Statistics;
        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(2);
        stats.Count.Should().Be(2);
    }

    [Fact]
    public void MemoizationEngine_StatisticsHitRatio()
    {
        var engine = new MemoizationEngine();

        engine.Memoize("key", () => 42);
        engine.Memoize("key", () => 42);
        engine.Memoize("missing", () => 0);

        var stats = engine.Statistics;
        stats.HitRatio.Should().BeApproximately(1.0 / 3.0, 0.01);
    }

    [Fact]
    public void MemoizationEngine_WithArgument()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        var r1 = engine.Memoize(42, x => { callCount++; return x * 2; });
        var r2 = engine.Memoize(42, x => { callCount++; return x * 2; });

        r1.Should().Be(84);
        r2.Should().Be(84);
        callCount.Should().Be(1);
    }

    [Fact]
    public void MemoizationEngine_DifferentArguments()
    {
        var engine = new MemoizationEngine();
        int callCount = 0;

        engine.Memoize(1, x => { callCount++; return x; });
        engine.Memoize(2, x => { callCount++; return x; });

        callCount.Should().Be(2);
    }

    [Fact]
    public void MemoizationEngine_NullKey_Throws()
    {
        var engine = new MemoizationEngine();
        Action act = () => engine.Memoize<int>(null!, () => 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MemoizationEngine_EmptyKey_Throws()
    {
        var engine = new MemoizationEngine();
        Action act = () => engine.Memoize("", () => 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MemoizationEngine_NullCompute_Throws()
    {
        var engine = new MemoizationEngine();
        Func<int>? compute = null;
        Action act = () => engine.Memoize("key", compute!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MemoizationEngine_InvalidateNonExistent()
    {
        var engine = new MemoizationEngine();

        Action act = () => engine.Invalidate("missing");

        act.Should().NotThrow();
    }

    [Fact]
    public void MemoizationEngine_StringResult()
    {
        var engine = new MemoizationEngine();

        var result = engine.Memoize("key", () => "hello");

        result.Should().Be("hello");
    }

    [Fact]
    public void MemoizationEngine_StatisticsAfterClearAll()
    {
        var engine = new MemoizationEngine();

        engine.Memoize("key", () => 42);
        engine.ClearAll();
        var stats = engine.Statistics;

        stats.Hits.Should().Be(0);
        stats.Misses.Should().Be(0);
        stats.Count.Should().Be(0);
    }

    [Fact]
    public async Task MemoizationEngine_ThreadSafety()
    {
        var engine = new MemoizationEngine();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => engine.Memoize($"key{i % 10}", () => i)))
            .ToArray();

        await Task.WhenAll(tasks);

        engine.Statistics.Count.Should().Be(10);
    }

    [Fact]
    public void CacheStatistics_DefaultValues()
    {
        var stats = new CacheStatistics();

        stats.Hits.Should().Be(0);
        stats.Misses.Should().Be(0);
        stats.Evictions.Should().Be(0);
        stats.Count.Should().Be(0);
        stats.Capacity.Should().Be(0);
    }

    [Fact]
    public void CacheStatistics_HitRatio_NoLookups()
    {
        var stats = new CacheStatistics();
        stats.HitRatio.Should().Be(0.0);
    }

    [Fact]
    public void CacheStatistics_HitRatio_AllHits()
    {
        var stats = new CacheStatistics { Hits = 10, Misses = 0 };
        stats.HitRatio.Should().Be(1.0);
    }

    [Fact]
    public void CacheStatistics_HitRatio_AllMisses()
    {
        var stats = new CacheStatistics { Hits = 0, Misses = 10 };
        stats.HitRatio.Should().Be(0.0);
    }

    [Fact]
    public void CacheStatistics_ToString()
    {
        var stats = new CacheStatistics { Hits = 5, Misses = 3, Evictions = 1, Count = 10, Capacity = 100 };

        var str = stats.ToString();
        str.Should().Contain("Hits=5");
        str.Should().Contain("Misses=3");
        str.Should().Contain("Evictions=1");
    }

    [Fact]
    public void CacheStatistics_RecordStructEquality()
    {
        var a = new CacheStatistics { Hits = 1, Misses = 2, Count = 5, Capacity = 10 };
        var b = new CacheStatistics { Hits = 1, Misses = 2, Count = 5, Capacity = 10 };

        a.Should().Be(b);
    }

    [Fact]
    public async Task RewriteCache_ThreadSafety()
    {
        var cache = new RewriteCache();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i);
                cache.Store(expr, Expr.Literal(i + 1000));
                cache.GetRewritten(expr);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void EvaluationCache_InvalidateNonExistentKey()
    {
        var cache = new EvaluationCache<int>();

        Action act = () => cache.Invalidate("nonexistent");
        act.Should().NotThrow();
    }

    [Fact]
    public void MemoizationEngine_InvalidateNullKey_Throws()
    {
        var engine = new MemoizationEngine();
        Action act = () => engine.Invalidate(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TypeInferenceCache_Overwrite()
    {
        var cache = new TypeInferenceCache();
        var expr = Expr.Literal(1.0);

        cache.StoreInferredType(expr, IntegerType.Instance);
        cache.StoreInferredType(expr, RealType.Instance);

        cache.GetInferredType(expr).Should().BeSameAs(RealType.Instance);
    }

    [Fact]
    public void SimplificationCache_Overwrite()
    {
        var cache = new SimplificationCache();
        var original = Expr.Literal(1.0);

        cache.Store(original, Expr.Literal(2.0));
        cache.Store(original, Expr.Literal(3.0));

        cache.GetSimplified(original).Should().Be(Expr.Literal(3.0));
    }

    [Fact]
    public void EvaluationCache_BooleanValues()
    {
        var cache = new EvaluationCache<bool>();

        cache.Store("key", true);
        cache.TryGet("key", out var result).Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public void MemoizationEngine_CachesExceptionResult()
    {
        var engine = new MemoizationEngine();

        var result = engine.Memoize("key", () => 42);
        result.Should().Be(42);

        var cached = engine.Memoize("key", () => 99);
        cached.Should().Be(42);
    }
}
