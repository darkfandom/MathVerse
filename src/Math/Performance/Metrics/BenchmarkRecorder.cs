namespace MathVerse.Math.Performance.Metrics;

/// <summary>
/// Records benchmark results for operations, tracking duration, allocations, and iteration counts.
/// Thread-safe.
/// </summary>
public sealed class BenchmarkRecorder
{
    private readonly ConcurrentDictionary<string, List<BenchmarkResult>> _results = new(StringComparer.Ordinal);

    /// <summary>
    /// Records a single benchmark iteration result.
    /// </summary>
    /// <param name="benchmark">The name of the benchmark.</param>
    /// <param name="durationTicks">The elapsed time in <see cref="Stopwatch"/> ticks.</param>
    /// <param name="allocatedBytes">The number of bytes allocated during the iteration.</param>
    /// <param name="iterationCount">The number of iterations that were run to produce this measurement.</param>
    public void Record(string benchmark, long durationTicks, long allocatedBytes, int iterationCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(benchmark);

        if (iterationCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterationCount), iterationCount, "Must be greater than zero.");

        var averageMs = (double)durationTicks / Stopwatch.Frequency * 1000.0 / iterationCount;

        var result = new BenchmarkResult(benchmark, averageMs, allocatedBytes, iterationCount);

        _results.AddOrUpdate(
            benchmark,
            _ => [result],
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Add(result);
                }
                return existing;
            });
    }

    /// <summary>
    /// Gets all recorded benchmark results across all benchmarks.
    /// </summary>
    /// <returns>A read-only list of all benchmark results.</returns>
    public IReadOnlyList<BenchmarkResult> GetAll()
    {
        var all = new List<BenchmarkResult>();
        foreach (var kvp in _results)
        {
            lock (kvp.Value)
            {
                all.AddRange(kvp.Value);
            }
        }
        return all;
    }

    /// <summary>
    /// Gets the best (lowest average duration) result for the specified benchmark.
    /// </summary>
    /// <param name="benchmark">The benchmark name to look up.</param>
    /// <returns>The best <see cref="BenchmarkResult"/>, or null if no results exist.</returns>
    public BenchmarkResult? GetBest(string benchmark)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(benchmark);

        if (!_results.TryGetValue(benchmark, out var list))
            return null;

        lock (list)
        {
            if (list.Count == 0)
                return null;

            var best = list[0];
            for (var i = 1; i < list.Count; i++)
            {
                if (list[i].AverageMs < best.AverageMs)
                    best = list[i];
            }
            return best;
        }
    }

    /// <summary>
    /// Removes all recorded benchmark results.
    /// </summary>
    public void Clear()
    {
        _results.Clear();
    }
}

/// <summary>
/// Represents a single benchmark measurement.
/// </summary>
/// <param name="Name">The name of the benchmark.</param>
/// <param name="AverageMs">The average duration per iteration in milliseconds.</param>
/// <param name="AllocatedBytes">The total bytes allocated across all iterations.</param>
/// <param name="Iterations">The number of iterations in the benchmark run.</param>
public sealed record BenchmarkResult(string Name, double AverageMs, long AllocatedBytes, int Iterations);
