using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ValidationBenchmarks
{
    private static readonly ValidationRule<int>[] _rules =
    [
        new() { Condition = x => x > 0, Error = Error.Validation("E", "must be positive") },
        new() { Condition = x => x < 100, Error = Error.Validation("E", "must be less than 100") },
        new() { Condition = x => x % 2 == 0, Error = Error.Validation("E", "must be even") }
    ];

    [Benchmark(Baseline = true)]
    public Validation<int> ValidCreation() => Validation<int>.Valid(42);

    [Benchmark]
    public Validation<int> InvalidCreation() => Validation<int>.Invalid(Error.Validation("E", "err"));

    [Benchmark]
    public Validation<int> MapValid() => Validation<int>.Valid(10).Map(x => x * 2);

    [Benchmark]
    public Validation<int> MapInvalid() =>
        Validation<int>.Invalid(Error.Validation("E", "err")).Map(x => x * 2);

    [Benchmark]
    public Validation<int> ValidateAll_Pass() => Validation.ValidateAll(50, _rules);

    [Benchmark]
    public Validation<int> ValidateAll_Fail() => Validation.ValidateAll(150, _rules);
}
