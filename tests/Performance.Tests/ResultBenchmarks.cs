using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ResultBenchmarks
{
    private readonly Error _error = new("BENCH", "benchmark", ErrorKind.Internal);

    [Benchmark(Baseline = true)]
    public Result<int> SuccessCreation() => Result<int>.Success(42);

    [Benchmark]
    public Result<int> FailureCreation() => Result<int>.Failure(_error);

    [Benchmark]
    public Result<int> MapSuccess() => Result<int>.Success(10).Map(x => x * 2);

    [Benchmark]
    public Result<int> MapFailure() => Result<int>.Failure(_error).Map(x => x * 2);

    [Benchmark]
    public Result<string> BindSuccess() =>
        Result<int>.Success(10).Bind(x => Result<string>.Success(x.ToString()));

    [Benchmark]
    public int MatchSuccess() =>
        Result<int>.Success(10).Match(x => x * 2, _ => 0);

    [Benchmark]
    public int MatchFailure() =>
        Result<int>.Failure(_error).Match(x => x * 2, _ => 0);
}
