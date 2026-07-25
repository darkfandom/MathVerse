using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Domains;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class DomainBenchmarks
{
    private DomainRegistry _registry = null!;
    private MathDomain _realDomain = null!;
    private MathDomain _integerDomain = null!;
    private MathDomain _complexDomain = null!;
    private MathDomain _naturalDomain = null!;
    private MathDomain _rationalDomain = null!;
    private MathDomain _booleanDomain = null!;
    private string[] _domainNames = null!;

    [GlobalSetup]
    public void Setup()
    {
        _registry = DomainRegistry.Instance;
        _realDomain = RealDomain.Instance;
        _integerDomain = IntegerDomain.Instance;
        _complexDomain = ComplexDomain.Instance;
        _naturalDomain = NaturalDomain.Instance;
        _rationalDomain = RationalDomain.Instance;
        _booleanDomain = BooleanDomain.Instance;
        _domainNames = new[] { "Real", "Integer", "Complex", "Natural", "Rational", "Boolean", "Whole", "Quaternion" };
    }

    [BenchmarkCategory("Lookup"), Benchmark(Baseline = true)]
    public MathDomain? LookupByName_Real()
    {
        return _registry.Get("Real");
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public MathDomain? LookupByName_Complex()
    {
        return _registry.Get("Complex");
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public MathDomain? LookupByName_Boolean()
    {
        return _registry.Get("Boolean");
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public MathDomain? LookupByKind_Real()
    {
        return _registry.Get(DomainKind.Real);
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public MathDomain? LookupByKind_Integer()
    {
        return _registry.Get(DomainKind.Integer);
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public MathDomain? LookupByKind_Complex()
    {
        return _registry.Get(DomainKind.Complex);
    }

    [BenchmarkCategory("Lookup"), Benchmark]
    public IReadOnlyList<MathDomain> GetAll()
    {
        return _registry.GetAll();
    }

    [BenchmarkCategory("Predicates"), Benchmark(Baseline = true)]
    public bool Contains_Real_Positive()
    {
        return _realDomain.Contains(42.5);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Real_Negative()
    {
        return _realDomain.Contains(-7.3);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Real_NaN()
    {
        return _realDomain.Contains(double.NaN);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Integer_Valid()
    {
        return _integerDomain.Contains(42.0);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Integer_NonInteger()
    {
        return _integerDomain.Contains(3.14);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Natural_Positive()
    {
        return _naturalDomain.Contains(5.0);
    }

    [BenchmarkCategory("Predicates"), Benchmark]
    public bool Contains_Natural_Negative()
    {
        return _naturalDomain.Contains(-1.0);
    }

    [BenchmarkCategory("Relationships"), Benchmark(Baseline = true)]
    public bool IsCompatibleWith_Real_vs_Integer()
    {
        return _realDomain.IsCompatibleWith(_integerDomain);
    }

    [BenchmarkCategory("Relationships"), Benchmark]
    public bool IsCompatibleWith_Real_vs_Complex()
    {
        return _realDomain.IsCompatibleWith(_complexDomain);
    }

    [BenchmarkCategory("Relationships"), Benchmark]
    public bool IsCompatibleWith_Natural_vs_Real()
    {
        return _naturalDomain.IsCompatibleWith(_realDomain);
    }

    [BenchmarkCategory("Relationships"), Benchmark]
    public bool IsSupersetOf_Real_vs_Integer()
    {
        return _realDomain.IsSupersetOf(_integerDomain);
    }

    [BenchmarkCategory("Relationships"), Benchmark]
    public bool IsSubsetOf_Integer_vs_Real()
    {
        return _integerDomain.IsSubsetOf(_realDomain);
    }

    [BenchmarkCategory("Relationships"), Benchmark]
    public bool IsSubsetOf_Natural_vs_Integer()
    {
        return _naturalDomain.IsSubsetOf(_integerDomain);
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public MathDomain Builder_Simple()
    {
        return new DomainBuilder()
            .WithName("BenchmarkDomain")
            .OfKind(DomainKind.Real)
            .Containing(v => v > 0)
            .Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public MathDomain Builder_WithParent()
    {
        return new DomainBuilder()
            .WithName("BenchmarkDomainExtended")
            .OfKind(DomainKind.Integer)
            .Extending(_realDomain)
            .Containing(v => v >= 0)
            .Build();
    }

    [BenchmarkCategory("Builder"), Benchmark]
    public MathDomain Builder_FullChain()
    {
        return new DomainBuilder()
            .WithName("ComplexBenchmarkDomain")
            .OfKind(DomainKind.Complex)
            .Extending(_realDomain)
            .Extending(_complexDomain)
            .Containing(v => v >= 0 && v <= 100)
            .ContainingComplex(c => c.Magnitude <= 100)
            .Build();
    }
}
