using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class OptionBenchmarks
{
    [Benchmark(Baseline = true)]
    public Option<int> SomeCreation() => Option<int>.Some(42);

    [Benchmark]
    public Option<int> NoneCreation() => Option<int>.None;

    [Benchmark]
    public Option<int> MapSome() => Option<int>.Some(10).Map(x => x * 2);

    [Benchmark]
    public Option<int> MapNone() => Option<int>.None.Map(x => x * 2);

    [Benchmark]
    public Option<string> BindSome() =>
        Option<int>.Some(10).Bind(x => Option<string>.Some(x.ToString()));

    [Benchmark]
    public int MatchSome() =>
        Option<int>.Some(10).Match(x => x * 2, () => 0);

    [Benchmark]
    public int MatchNone() =>
        Option<int>.None.Match(x => x * 2, () => 0);

    [Benchmark]
    public int OrSome() => Option<int>.Some(42).Or(0);

    [Benchmark]
    public int OrNone() => Option<int>.None.Or(0);
}
