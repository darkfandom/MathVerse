using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class MaybeBenchmarks
{
    [Benchmark(Baseline = true)]
    public Maybe<int> DefinedCreation() => Maybe<int>.Defined(42);

    [Benchmark]
    public Maybe<int> UndefinedCreation() => Maybe<int>.DivisionByZero;

    [Benchmark]
    public Maybe<int> MapDefined() => Maybe<int>.Defined(10).Map(x => x * 2);

    [Benchmark]
    public Maybe<int> MapUndefined() => Maybe<int>.Overflow.Map(x => x * 2);

    [Benchmark]
    public Maybe<string> BindDefined() =>
        Maybe<int>.Defined(10).Bind(x => Maybe<string>.Defined(x.ToString()));

    [Benchmark]
    public int MatchDefined() =>
        Maybe<int>.Defined(10).Match(x => x * 2, _ => 0);

    [Benchmark]
    public int MatchUndefined() =>
        Maybe<int>.DomainError.Match(x => x * 2, _ => 0);
}
