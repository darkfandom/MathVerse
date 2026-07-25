using BenchmarkDotNet.Attributes;
using MathVerse.Core;

namespace MathVerse.Performance.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class GuardBenchmarks
{
    private static readonly string _validString = "hello";
    private static readonly IReadOnlyCollection<int> _validCollection = new List<int> { 1, 2, 3 };

    [Benchmark]
    public string NotNull() => Guard.NotNull(_validString, "param");

    [Benchmark]
    public string NotNullOrEmpty() => Guard.NotNullOrEmpty(_validString, "param");

    [Benchmark]
    public string NotNullOrWhiteSpace() => Guard.NotNullOrWhiteSpace(_validString, "param");

    [Benchmark]
    public int GreaterThan() => Guard.GreaterThan(10, 5, "param");

    [Benchmark]
    public int LessThan() => Guard.LessThan(3, 5, "param");

    [Benchmark]
    public int Between() => Guard.Between(5, 1, 10, "param");

    [Benchmark]
    public IReadOnlyCollection<int> NotNullOrEmpty_Collection() => Guard.NotNullOrEmpty(_validCollection, "param");
}
