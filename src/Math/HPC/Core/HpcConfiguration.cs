namespace MathVerse.Math.HPC.Core;

using System;

/// <summary>
/// Global configuration for the HPC engine.
/// </summary>
/// <param name="DefaultOptions">Default options for HPC operations.</param>
/// <param name="SIMDTimeout">Timeout for SIMD vectorization operations.</param>
/// <param name="ParallelTimeout">Timeout for parallel scheduling operations.</param>
/// <param name="CacheSize">Maximum cache size in bytes.</param>
/// <param name="CacheEnabled">Whether caching is globally enabled.</param>
/// <param name="IncrementalEnabled">Whether incremental analysis is globally enabled.</param>
/// <param name="MaxKernelSize">Maximum kernel size for fusion (in IR nodes).</param>
/// <param name="FusionThreshold">Minimum benefit threshold for kernel fusion (0.0 to 1.0).</param>
public sealed record HpcConfiguration(
    HpcOptions DefaultOptions,
    TimeSpan SIMDTimeout,
    TimeSpan ParallelTimeout,
    long CacheSize,
    bool CacheEnabled,
    bool IncrementalEnabled,
    int MaxKernelSize,
    double FusionThreshold
)
{
    /// <summary>
    /// Gets the default HPC configuration.
    /// </summary>
    public static HpcConfiguration Default => new(
        DefaultOptions: HpcOptions.Default,
        SIMDTimeout: TimeSpan.FromMinutes(2),
        ParallelTimeout: TimeSpan.FromMinutes(2),
        CacheSize: 1024L * 1024 * 1024, // 1 GB
        CacheEnabled: true,
        IncrementalEnabled: true,
        MaxKernelSize: 10000,
        FusionThreshold: 0.15
    );

    /// <summary>
    /// Creates a configuration for development/debugging.
    /// </summary>
    public static HpcConfiguration Development => new(
        DefaultOptions: HpcOptions.Fast,
        SIMDTimeout: TimeSpan.FromSeconds(10),
        ParallelTimeout: TimeSpan.FromSeconds(10),
        CacheSize: 100L * 1024 * 1024, // 100 MB
        CacheEnabled: true,
        IncrementalEnabled: true,
        MaxKernelSize: 1000,
        FusionThreshold: 0.1
    );

    /// <summary>
    /// Creates a configuration for production workloads.
    /// </summary>
    public static HpcConfiguration Production => new(
        DefaultOptions: HpcOptions.Default,
        SIMDTimeout: TimeSpan.FromMinutes(5),
        ParallelTimeout: TimeSpan.FromMinutes(5),
        CacheSize: 4L * 1024 * 1024 * 1024, // 4 GB
        CacheEnabled: true,
        IncrementalEnabled: true,
        MaxKernelSize: 50000,
        FusionThreshold: 0.2
    );

    /// <summary>
    /// Creates a configuration for maximum optimization.
    /// </summary>
    public static HpcConfiguration MaximumOptimization => new(
        DefaultOptions: HpcOptions.Exhaustive,
        SIMDTimeout: TimeSpan.FromMinutes(15),
        ParallelTimeout: TimeSpan.FromMinutes(15),
        CacheSize: 8L * 1024 * 1024 * 1024, // 8 GB
        CacheEnabled: true,
        IncrementalEnabled: false,
        MaxKernelSize: 100000,
        FusionThreshold: 0.25
    );

    /// <summary>
    /// Creates a copy of this configuration with the specified default options.
    /// </summary>
    public HpcConfiguration WithDefaultOptions(HpcOptions options) => this with { DefaultOptions = options };

    /// <summary>
    /// Creates a copy of this configuration with the specified cache size.
    /// </summary>
    public HpcConfiguration WithCacheSize(long cacheSize) => this with { CacheSize = cacheSize };

    /// <summary>
    /// Creates a copy of this configuration with caching enabled/disabled.
    /// </summary>
    public HpcConfiguration WithCacheEnabled(bool enabled) => this with { CacheEnabled = enabled };

    /// <summary>
    /// Creates a copy of this configuration with the specified fusion threshold.
    /// </summary>
    public HpcConfiguration WithFusionThreshold(double threshold) => this with { FusionThreshold = Math.Clamp(threshold, 0.0, 1.0) };
}
