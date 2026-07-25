using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Foundation.Constants;
using MathVerse.Math.Foundation.Domains;
using MathVerse.Math.Foundation.Units;
using System.Collections.Concurrent;

namespace MathVerse.Foundation.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class RegistryBenchmarks
{
    private DomainRegistry _domainRegistry = null!;
    private ConstantRegistry _constantRegistry = null!;
    private UnitRegistry _unitRegistry = null!;
    private string[] _domainNames = null!;
    private string[] _constantNames = null!;
    private string[] _unitSymbols = null!;
    private ConstantCategory[] _constantCategories = null!;
    private UnitCategory[] _unitCategories = null!;

    [Params(1, 10, 100)]
    public int IterationCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _domainRegistry = DomainRegistry.Instance;
        _constantRegistry = ConstantRegistry.Instance;
        _unitRegistry = UnitRegistry.Instance;
        _domainNames = new[] { "Real", "Integer", "Complex", "Natural", "Rational", "Boolean", "Whole", "Quaternion" };
        _constantNames = new[] { "Pi", "E", "Tau", "Phi", "Gamma", "Catalan", "Apery", "FeigenbaumAlpha", "FeigenbaumDelta", "Epsilon", "Infinity", "NaN" };
        _unitSymbols = new[] { "m", "kg", "s", "A", "K", "mol", "cd", "N", "J", "W", "Pa", "Hz", "V" };
        _constantCategories = new[]
        {
            ConstantCategory.Transcendental,
            ConstantCategory.Fundamental,
            ConstantCategory.Analysis,
            ConstantCategory.NumberTheory,
            ConstantCategory.Combinatorics
        };
        _unitCategories = new[]
        {
            UnitCategory.Length,
            UnitCategory.Mass,
            UnitCategory.Time,
            UnitCategory.Force,
            UnitCategory.Energy,
            UnitCategory.Power,
            UnitCategory.Pressure,
            UnitCategory.Frequency,
            UnitCategory.Voltage
        };
    }

    [BenchmarkCategory("DomainRegistry"), Benchmark(Baseline = true)]
    public MathDomain? Domain_Get_AllNames()
    {
        MathDomain? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _domainRegistry.Get(_domainNames[i % _domainNames.Length]);
        }
        return result;
    }

    [BenchmarkCategory("DomainRegistry"), Benchmark]
    public IReadOnlyList<DomainKind> Domain_GetAll()
    {
        IReadOnlyList<MathDomain>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _domainRegistry.GetAll();
        }
        return result!.Count > Array.Empty<MathDomain>() ? throw new Exception() : Array.Empty<DomainKind>();
    }

    [BenchmarkCategory("DomainRegistry"), Benchmark]
    public MathDomain? Domain_GetByKind_All()
    {
        var kinds = new[] { DomainKind.Real, DomainKind.Integer, DomainKind.Complex, DomainKind.Natural, DomainKind.Rational, DomainKind.Whole, DomainKind.Boolean };
        MathDomain? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _domainRegistry.Get(kinds[i % kinds.Length]);
        }
        return result;
    }

    [BenchmarkCategory("DomainRegistry"), Benchmark]
    public MathDomain Domain_Register_Custom()
    {
        MathDomain? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            var domain = new DomainBuilder()
                .WithName($"TestDomain_{i}")
                .OfKind(DomainKind.None)
                .Containing(v => v > i)
                .Build();
            _domainRegistry.Register(domain);
            result = domain;
        }
        return result!;
    }

    [BenchmarkCategory("ConstantRegistry"), Benchmark]
    public MathConstant? Constant_Get_AllNames()
    {
        MathConstant? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _constantRegistry.Get(_constantNames[i % _constantNames.Length]);
        }
        return result;
    }

    [BenchmarkCategory("ConstantRegistry"), Benchmark]
    public IReadOnlyList<MathConstant> Constant_GetAll()
    {
        IReadOnlyList<MathConstant>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _constantRegistry.GetAll();
        }
        return result!;
    }

    [BenchmarkCategory("ConstantRegistry"), Benchmark]
    public IReadOnlyList<MathConstant> Constant_GetByCategory_All()
    {
        IReadOnlyList<MathConstant>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _constantRegistry.GetByCategory(_constantCategories[i % _constantCategories.Length]);
        }
        return result!;
    }

    [BenchmarkCategory("ConstantRegistry"), Benchmark]
    public MathConstant Constant_Register_Custom()
    {
        MathConstant? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            var constant = new MathConstant
            {
                Symbol = $"c{i}",
                Name = $"Custom{i}",
                Category = ConstantCategory.Mathematical,
                NumericValue = i,
                ComplexValue = new Complex(i, 0),
                Aliases = ImmutableArray.Create($"alias{i}"),
                Description = $"Custom constant {i}",
                IsExact = false
            };
            _constantRegistry.Register(constant);
            result = constant;
        }
        return result!;
    }

    [BenchmarkCategory("UnitRegistry"), Benchmark]
    public Unit? Unit_Get_AllSymbols()
    {
        Unit? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _unitRegistry.Get(_unitSymbols[i % _unitSymbols.Length]);
        }
        return result;
    }

    [BenchmarkCategory("UnitRegistry"), Benchmark]
    public IReadOnlyList<Unit> Unit_GetAll()
    {
        IReadOnlyList<Unit>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _unitRegistry.GetAll();
        }
        return result!;
    }

    [BenchmarkCategory("UnitRegistry"), Benchmark]
    public IReadOnlyList<Unit> Unit_GetByCategory_All()
    {
        IReadOnlyList<Unit>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _unitRegistry.GetByCategory(_unitCategories[i % _unitCategories.Length]);
        }
        return result!;
    }

    [BenchmarkCategory("UnitRegistry"), Benchmark]
    public IReadOnlyList<Unit> Unit_GetByDimension_All()
    {
        var dims = new[]
        {
            Dimension.FromBaseDimensions(length: 1),
            Dimension.FromBaseDimensions(mass: 1),
            Dimension.FromBaseDimensions(time: 1),
            DerivedDimension.Force,
            DerivedDimension.Energy,
            DerivedDimension.Power
        };
        IReadOnlyList<Unit>? result = null;
        for (int i = 0; i < IterationCount; i++)
        {
            result = _unitRegistry.GetByDimension(dims[i % dims.Length]);
        }
        return result!;
    }

    [BenchmarkCategory("ConcurrentReads"), Benchmark]
    public void Domain_ConcurrentReads()
    {
        Parallel.For(0, IterationCount, i =>
        {
            _domainRegistry.Get(_domainNames[i % _domainNames.Length]);
            _domainRegistry.GetAll();
        });
    }

    [BenchmarkCategory("ConcurrentReads"), Benchmark]
    public void Constant_ConcurrentReads()
    {
        Parallel.For(0, IterationCount, i =>
        {
            _constantRegistry.Get(_constantNames[i % _constantNames.Length]);
            _constantRegistry.GetAll();
            _constantRegistry.GetByCategory(_constantCategories[i % _constantCategories.Length]);
        });
    }

    [BenchmarkCategory("ConcurrentReads"), Benchmark]
    public void Unit_ConcurrentReads()
    {
        Parallel.For(0, IterationCount, i =>
        {
            _unitRegistry.Get(_unitSymbols[i % _unitSymbols.Length]);
            _unitRegistry.GetAll();
            _unitRegistry.GetByCategory(_unitCategories[i % _unitCategories.Length]);
        });
    }
}
