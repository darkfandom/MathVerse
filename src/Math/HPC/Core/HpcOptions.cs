namespace MathVerse.Math.HPC.Core;

using System;

/// <summary>
/// Optimization level for HPC operations.
/// </summary>
public enum OptimizationLevel
{
    /// <summary>
    /// No optimization.
    /// </summary>
    None = 0,

    /// <summary>
    /// Basic optimizations only.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Aggressive optimizations.
    /// </summary>
    Aggressive = 2,

    /// <summary>
    /// Exhaustive optimization search.
    /// </summary>
    Exhaustive = 3,
}

/// <summary>
/// Configuration options for HPC operations.
/// </summary>
/// <param name="Timeout">Maximum time allowed for the operation.</param>
/// <param name="MaxDepth">Maximum analysis depth.</param>
/// <param name="EnableSIMD">Whether to enable SIMD vectorization.</param>
/// <param name="EnableParallel">Whether to enable parallel execution.</param>
/// <param name="EnableFusion">Whether to enable kernel fusion.</param>
/// <param name="MaxParallelism">Maximum degree of parallelism.</param>
/// <param name="Incremental">Whether to use incremental analysis.</param>
/// <param name="CacheEnabled">Whether caching is enabled.</param>
/// <param name="OptimizationLevel">The optimization level to apply.</param>
public sealed record HpcOptions(
    TimeSpan Timeout,
    int MaxDepth,
    bool EnableSIMD,
    bool EnableParallel,
    bool EnableFusion,
    int MaxParallelism,
    bool Incremental,
    bool CacheEnabled,
    OptimizationLevel OptimizationLevel
)
{
    /// <summary>
    /// Gets the default HPC options.
    /// </summary>
    public static HpcOptions Default => new(
        Timeout: TimeSpan.FromMinutes(5),
        MaxDepth: 1000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        MaxParallelism: Environment.ProcessorCount,
        Incremental: true,
        CacheEnabled: true,
        OptimizationLevel: OptimizationLevel.Aggressive
    );

    /// <summary>
    /// Creates options optimized for fast compilation.
    /// </summary>
    public static HpcOptions Fast => new(
        Timeout: TimeSpan.FromSeconds(30),
        MaxDepth: 100,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: false,
        MaxParallelism: Environment.ProcessorCount,
        Incremental: true,
        CacheEnabled: true,
        OptimizationLevel: OptimizationLevel.Basic
    );

    /// <summary>
    /// Creates options for exhaustive optimization.
    /// </summary>
    public static HpcOptions Exhaustive => new(
        Timeout: TimeSpan.FromMinutes(30),
        MaxDepth: 10000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        MaxParallelism: Environment.ProcessorCount,
        Incremental: false,
        CacheEnabled: true,
        OptimizationLevel: OptimizationLevel.Exhaustive
    );

    /// <summary>
    /// Creates a copy of these options with the specified timeout.
    /// </summary>
    public HpcOptions WithTimeout(TimeSpan timeout) => this with { Timeout = timeout };

    /// <summary>
    /// Creates a copy of these options with the specified max depth.
    /// </summary>
    public HpcOptions WithMaxDepth(int maxDepth) => this with { MaxDepth = maxDepth };

    /// <summary>
    /// Creates a copy of these options with SIMD enabled/disabled.
    /// </summary>
    public HpcOptions WithSIMD(bool enable) => this with { EnableSIMD = enable };

    /// <summary>
    /// Creates a copy of these options with parallel execution enabled/disabled.
    /// </summary>
    public HpcOptions WithParallel(bool enable) => this with { EnableParallel = enable };

    /// <summary>
    /// Creates a copy of these options with kernel fusion enabled/disabled.
    /// </summary>
    public HpcOptions WithFusion(bool enable) => this with { EnableFusion = enable };

    /// <summary>
    /// Creates a copy of these options with the specified max parallelism.
    /// </summary>
    public HpcOptions WithMaxParallelism(int maxParallelism) => this with { MaxParallelism = maxParallelism };

    /// <summary>
    /// Creates a copy of these options with incremental mode enabled/disabled.
    /// </summary>
    public HpcOptions WithIncremental(bool incremental) => this with { Incremental = incremental };

    /// <summary>
    /// Creates a copy of these options with caching enabled/disabled.
    /// </summary>
    public HpcOptions WithCache(bool cacheEnabled) => this with { CacheEnabled = cacheEnabled };

    /// <summary>
    /// Creates a copy of these options with the specified optimization level.
    /// </summary>
    public HpcOptions WithOptimizationLevel(OptimizationLevel level) => this with { OptimizationLevel = level };
}
