using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Buffers;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PoolingBenchmarks
{
    private ObjectPool<ExpressionPool> _expressionPool = null!;
    private ObjectPool<List<Expression>> _listPool = null!;
    private ObjectPool<Dictionary<string, Expression>> _dictPool = null!;
    private BuilderPool<HashBuilder> _builderPool = null!;
    private ArrayPoolAdapter _arrayPool = null!;
    private BufferManager _bufferManager = null!;

    [Params(10, 100, 500)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _expressionPool = new ObjectPool<ExpressionPool>();
        _listPool = new ObjectPool<List<Expression>>();
        _dictPool = new ObjectPool<Dictionary<string, Expression>>();
        _builderPool = new BuilderPool<HashBuilder>();
        _arrayPool = new ArrayPoolAdapter();
        _bufferManager = new BufferManager();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("RentReturn")]
    public Expression ExpressionPool_RentReturn()
    {
        var entry = _expressionPool.Rent(static () => new ExpressionPool());
        entry.Current = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        _expressionPool.Return(entry);
        return entry.Current!;
    }

    [Benchmark]
    [BenchmarkCategory("RentReturn")]
    public List<Expression> ListPool_RentReturn()
    {
        var list = _listPool.Rent(static () => []);
        list.Add(Expr.Variable("x"));
        list.Add(Expr.Literal(1.0));
        var count = list.Count;
        list.Clear();
        _listPool.Return(list);
        _ = count;
        return list;
    }

    [Benchmark]
    [BenchmarkCategory("RentReturn")]
    public Dictionary<string, Expression> DictPool_RentReturn()
    {
        var dict = _dictPool.Rent(static () => []);
        dict["x"] = Expr.Variable("x");
        dict["y"] = Expr.Literal(1.0);
        var count = dict.Count;
        dict.Clear();
        _dictPool.Return(dict);
        _ = count;
        return dict;
    }

    [Benchmark]
    [BenchmarkCategory("RentReturn")]
    public HashBuilder BuilderPool_RentReturn()
    {
        var builder = _builderPool.Rent();
        builder.Add(42);
        builder.Add(3.14);
        var hash = builder.ToHashCode();
        _builderPool.Return(builder);
        _ = hash;
        return builder;
    }

    [Benchmark]
    [BenchmarkCategory("RentReturn")]
    public byte[] ArrayPool_RentReturn()
    {
        var buffer = _arrayPool.Rent(1024);
        buffer[0] = 1;
        buffer[1] = 2;
        _arrayPool.Return(buffer);
        return buffer;
    }

    [Benchmark]
    [BenchmarkCategory("Operations")]
    public int PooledList_AddRange()
    {
        using var list = new PooledList<Expression>(_listPool);
        for (var i = 0; i < ItemCount; i++)
            list.Add(Expr.Add(Expr.Variable("x"), Expr.Literal(i)));
        return list.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Operations")]
    public int PooledDictionary_AddRange()
    {
        using var dict = new PooledDictionary<string, Expression>(_dictPool);
        for (var i = 0; i < ItemCount; i++)
            dict.Add($"key{i}", Expr.Add(Expr.Variable("x"), Expr.Literal(i)));
        return dict.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Operations")]
    public int NonPooledList_AddRange()
    {
        var list = new List<Expression>();
        for (var i = 0; i < ItemCount; i++)
            list.Add(Expr.Add(Expr.Variable("x"), Expr.Literal(i)));
        return list.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Operations")]
    public int NonPooledDictionary_AddRange()
    {
        var dict = new Dictionary<string, Expression>();
        for (var i = 0; i < ItemCount; i++)
            dict[$"key{i}"] = Expr.Add(Expr.Variable("x"), Expr.Literal(i));
        return dict.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Operations")]
    public byte[] BufferManager_RentReturn()
    {
        var buffer = _bufferManager.RentBuffer(1024);
        buffer[0] = 42;
        _bufferManager.ReturnBuffer(buffer);
        return buffer;
    }

    [Benchmark]
    [BenchmarkCategory("Cycle")]
    public int Pool_RentUseReturn_Cycle()
    {
        var total = 0;
        for (var i = 0; i < ItemCount; i++)
        {
            var entry = _expressionPool.Rent(static () => new ExpressionPool());
            entry.Current = Expr.Add(Expr.Variable("x"), Expr.Literal(i));
            total += entry.Current.GetHashCode();
            entry.Reset();
            _expressionPool.Return(entry);
        }
        return total;
    }

    [Benchmark]
    [BenchmarkCategory("Cycle")]
    public int FreshAllocation_Cycle()
    {
        var total = 0;
        for (var i = 0; i < ItemCount; i++)
        {
            var entry = new ExpressionPool();
            entry.Current = Expr.Add(Expr.Variable("x"), Expr.Literal(i));
            total += entry.Current.GetHashCode();
        }
        return total;
    }
}
