using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using MathVerse.Math.Visitors;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class HashingBenchmarks
{
    private Expression _literal = null!;
    private Expression _binary = null!;
    private Expression _functionCall = null!;
    private Expression _deepTree = null!;
    private Expression[] _treeArray = null!;
    private CachedExpressionHasher _hasher = null!;
    private StructuralHasher _structuralHasher = null!;
    private HashCache _hashCache = null!;

    [Params(10, 100, 500)]
    public int TreeSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _literal = Expr.Literal(42.0);
        _binary = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(3.0)), Expr.Pow(Expr.Variable("y"), Expr.Literal(2.0)));
        _functionCall = Expr.Sin(Expr.Add(Expr.Variable("x"), Expr.Cos(Expr.Variable("y"))));

        Expression deep = Expr.Literal(1.0);
        for (var i = 0; i < TreeSize; i++)
            deep = Expr.Add(deep, Expr.Multiply(Expr.Variable($"v{i}"), Expr.Literal(i)));
        _deepTree = deep;

        _treeArray = new Expression[100];
        for (var i = 0; i < 100; i++)
            _treeArray[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));

        _hasher = new CachedExpressionHasher();
        _structuralHasher = new StructuralHasher();
        _hashCache = new HashCache();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("SingleHash")]
    public int HashLiteral() => _hasher.ComputeHash(_literal);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int HashBinary() => _hasher.ComputeHash(_binary);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int HashFunctionCall() => _hasher.ComputeHash(_functionCall);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int HashDeepTree() => _hasher.ComputeHash(_deepTree);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int StructuralHash_Literal() => _structuralHasher.Hash(_literal);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int StructuralHash_Binary() => _structuralHasher.Hash(_binary);

    [Benchmark]
    [BenchmarkCategory("SingleHash")]
    public int StructuralHash_DeepTree() => _structuralHasher.Hash(_deepTree);

    [Benchmark]
    [BenchmarkCategory("TreeHash")]
    public int HashAllTrees()
    {
        var result = 0;
        for (var i = 0; i < _treeArray.Length; i++)
            result ^= _hasher.ComputeHash(_treeArray[i]);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("TreeHash")]
    public int StructuralHashRange()
    {
        return _structuralHasher.HashRange(_treeArray);
    }

    [Benchmark]
    [BenchmarkCategory("CacheLookup")]
    public int HashWithCache_Hit()
    {
        _hashCache.Clear();
        _hashCache.Store(_deepTree, _deepTree.GetHashCode());
        return _hashCache.TryGet(_deepTree, out var cached) ? cached : 0;
    }

    [Benchmark]
    [BenchmarkCategory("CacheLookup")]
    public int HashWithCache_Miss()
    {
        _hashCache.Clear();
        return _hashCache.TryGet(_deepTree, out var cached) ? cached : 0;
    }

    [Benchmark]
    [BenchmarkCategory("CacheLookup")]
    public int HashWithCache_Populated()
    {
        _hashCache.Clear();
        for (var i = 0; i < _treeArray.Length; i++)
            _hashCache.Store(_treeArray[i], _treeArray[i].GetHashCode());
        var result = 0;
        for (var i = 0; i < _treeArray.Length; i++)
        {
            if (_hashCache.TryGet(_treeArray[i], out var h))
                result ^= h;
        }
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("StructuralCompare")]
    public bool CompareHashCodes_Same()
    {
        var h1 = _hasher.ComputeHash(_binary);
        var h2 = _hasher.ComputeHash(_binary);
        return h1 == h2;
    }

    [Benchmark]
    [BenchmarkCategory("StructuralCompare")]
    public int HashBuilder_Compute()
    {
        var builder = new HashBuilder();
        builder.Add(42);
        builder.Add(3.14);
        builder.Add("hello");
        builder.AddBytes(new byte[] { 1, 2, 3, 4, 5 });
        return builder.ToHashCode();
    }

    [Benchmark]
    [BenchmarkCategory("StructuralCompare")]
    public int GetHashCode_ExpressionKey()
    {
        var key = new ExpressionKey(_binary);
        return key.GetHashCode();
    }

    [Benchmark]
    [BenchmarkCategory("StructuralCompare")]
    public bool ExpressionKey_Equals()
    {
        var key1 = new ExpressionKey(_binary);
        var key2 = new ExpressionKey(_binary);
        return key1.Equals(key2);
    }

    [Benchmark]
    [BenchmarkCategory("StructuralCompare")]
    public void HashCache_StoreAndRetrieve()
    {
        _hashCache.Clear();
        for (var i = 0; i < _treeArray.Length; i++)
            _hashCache.Store(_treeArray[i], _treeArray[i].GetHashCode());
        var stats = _hashCache.Count;
        _ = stats;
    }
}
