namespace MathVerse.Math.Performance;

/// <summary>
/// Top-level facade for the MathVerse performance infrastructure.
/// Provides a single entry point for interning, optimization, evaluation, hashing, and diagnostics.
/// Thread-safe.
/// </summary>
public sealed class PerformanceEngine
{
    private readonly PerformanceOptions _options;

    private PerformanceEngine(PerformanceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        Services = new PerformanceServices(options);
    }

    /// <summary>
    /// Gets the underlying service instances.
    /// </summary>
    public PerformanceServices Services { get; }

    /// <summary>
    /// Creates a new <see cref="PerformanceEngine"/> with default options.
    /// </summary>
    /// <returns>A configured performance engine.</returns>
    public static PerformanceEngine Create() => new(PerformanceOptions.Default);

    /// <summary>
    /// Creates a new <see cref="PerformanceEngine"/> with the specified options.
    /// </summary>
    /// <param name="options">The performance configuration options.</param>
    /// <returns>A configured performance engine.</returns>
    public static PerformanceEngine Create(PerformanceOptions options) => new(options);

    /// <summary>
    /// Interns the expression, returning the canonical instance if one already exists
    /// with the same structural content.
    /// </summary>
    /// <param name="expr">The expression to intern.</param>
    /// <returns>The interned (canonical) expression instance.</returns>
    public Expression Intern(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        return Services.Logger.Log("Intern", () => Services.Interner.Intern(expr));
    }

    /// <summary>
    /// Optimizes the expression through the configured optimization pipeline.
    /// </summary>
    /// <param name="expr">The expression to optimize.</param>
    /// <returns>The optimized expression.</returns>
    public Expression Optimize(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        if (!_options.EnableOptimization)
            return expr;

        return Services.Logger.Log("Optimize", () =>
            Services.Optimizer.Optimize(expr, _options.EnabledOptimizationStages));
    }

    /// <summary>
    /// Evaluates the expression using the incremental computation engine.
    /// </summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <returns>The evaluated expression.</returns>
    public Expression Evaluate(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        return Services.Logger.Log("Evaluate", () => Services.Incremental.Evaluate(expr));
    }

    /// <summary>
    /// Computes a structural hash code for the given expression.
    /// </summary>
    /// <param name="expr">The expression to hash.</param>
    /// <returns>The hash code.</returns>
    public int HashExpression(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        return Services.Hasher.ComputeHash(expr);
    }

    /// <summary>
    /// Invalidates all caches (evaluation, rewrite, simplification, type inference, memoization).
    /// </summary>
    public void InvalidateCaches()
    {
        Services.EvaluationCache.Clear();
        Services.RewriteCache.Clear();
        Services.SimplificationCache.Clear();
        Services.TypeCache.Clear();
        Services.Memoization.ClearAll();
        Services.Incremental.Reset();
        Services.Interner.Clear();
        Services.Hasher.ClearCache();
        Services.Memory.Reset();
        Services.Allocations.Reset();
    }

    /// <summary>
    /// Builds a comprehensive performance report from all collected metrics and diagnostics.
    /// </summary>
    /// <returns>A <see cref="PerformanceReport"/> summarizing all performance data.</returns>
    public PerformanceReport GetReport()
    {
        var memStats = Services.Memory.GetStatistics();
        var cacheStats = Services.EvaluationCache.Statistics;

        var totalOps = Services.Benchmarks.GetAll().Count + Services.Diagnostics.GetEvents().Count;
        var allocatedBytes = memStats.CurrentAllocations;
        var elapsedMs = 0.0;

        var events = Services.Diagnostics.GetEvents();
        foreach (var evt in events)
            elapsedMs += evt.DurationMs;

        var cacheHitRatio = cacheStats.HitRatio;
        var opsPerSecond = elapsedMs > 0.0 ? totalOps / (elapsedMs / 1000.0) : 0.0;

        var snapshot = new PerformanceSnapshot(
            DateTime.UtcNow,
            totalOps,
            elapsedMs,
            allocatedBytes,
            memStats.Gen0Collections,
            memStats.Gen1Collections,
            memStats.Gen2Collections,
            cacheHitRatio,
            opsPerSecond);

        var slowestOperations = events
            .OrderByDescending(e => e.DurationTicks)
            .ToList();

        var benchmarks = Services.Benchmarks.GetAll();

        var optimizationDiagnostics = Services.Optimizer.Passes.Count > 0
            ? Services.Diagnostics.GetDiagnostics()
                .Where(d => d.Category == "Optimization")
                .Select(d => new OptimizationDiagnostic(
                    d.Message,
                    0,
                    0,
                    TimeSpan.Zero,
                    true))
                .ToList()
            : [];

        return new PerformanceReport(snapshot, slowestOperations, benchmarks, optimizationDiagnostics);
    }
}
