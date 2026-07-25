namespace MathVerse.Math.Performance.Metrics;

/// <summary>
/// Comprehensive performance report combining a metrics snapshot with the slowest operations,
/// benchmark results, and optimization diagnostics.
/// </summary>
public sealed class PerformanceReport
{
    /// <summary>
    /// An empty performance report with default values.
    /// </summary>
    public static readonly PerformanceReport Empty = new(
        new PerformanceSnapshot(DateTime.UtcNow, 0, 0.0, 0L, 0, 0, 0, 0.0, 0.0),
        [],
        [],
        []);

    /// <summary>
    /// Initializes a new performance report.
    /// </summary>
    /// <param name="snapshot">The metrics snapshot at report time.</param>
    /// <param name="slowestOperations">The operations sorted by descending duration.</param>
    /// <param name="benchmarks">The recorded benchmark results.</param>
    /// <param name="optimizationResults">The diagnostics from optimization passes.</param>
    public PerformanceReport(
        PerformanceSnapshot snapshot,
        IReadOnlyList<PerformanceEvent> slowestOperations,
        IReadOnlyList<BenchmarkResult> benchmarks,
        IReadOnlyList<OptimizationDiagnostic> optimizationResults)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        SlowestOperations = slowestOperations ?? [];
        Benchmarks = benchmarks ?? [];
        OptimizationResults = optimizationResults ?? [];
    }

    /// <summary>Gets the metrics snapshot at report time.</summary>
    public PerformanceSnapshot Snapshot { get; }

    /// <summary>Gets the operations sorted by descending duration.</summary>
    public IReadOnlyList<PerformanceEvent> SlowestOperations { get; }

    /// <summary>Gets the recorded benchmark results.</summary>
    public IReadOnlyList<BenchmarkResult> Benchmarks { get; }

    /// <summary>Gets the diagnostics from optimization passes.</summary>
    public IReadOnlyList<OptimizationDiagnostic> OptimizationResults { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Performance Report @ {Snapshot.Timestamp:O}");
        sb.AppendLine($"  Operations: {Snapshot.TotalOperations}, Elapsed: {Snapshot.ElapsedMs:F2}ms");
        sb.AppendLine($"  Allocated: {Snapshot.AllocatedBytes}B, Cache Hit: {Snapshot.CacheHitRatio:F4}");
        sb.AppendLine($"  GC: Gen0={Snapshot.Gen0Collections}, Gen1={Snapshot.Gen1Collections}, Gen2={Snapshot.Gen2Collections}");

        if (SlowestOperations.Count > 0)
        {
            sb.AppendLine($"  Slowest Operations ({SlowestOperations.Count}):");
            for (var i = 0; i < System.Math.Min(5, SlowestOperations.Count); i++)
            {
                var op = SlowestOperations[i];
                sb.AppendLine($"    {op.Operation}: {op.DurationMs:F2}ms");
            }
        }

        if (Benchmarks.Count > 0)
        {
            sb.AppendLine($"  Benchmarks ({Benchmarks.Count}):");
            for (var i = 0; i < Benchmarks.Count; i++)
            {
                var b = Benchmarks[i];
                sb.AppendLine($"    {b.Name}: {b.AverageMs:F2}ms/iter ({b.Iterations} iterations)");
            }
        }

        if (OptimizationResults.Count > 0)
        {
            sb.AppendLine($"  Optimization Results ({OptimizationResults.Count}):");
            for (var i = 0; i < OptimizationResults.Count; i++)
            {
                var r = OptimizationResults[i];
                sb.AppendLine($"    {r}");
            }
        }

        return sb.ToString();
    }
}
