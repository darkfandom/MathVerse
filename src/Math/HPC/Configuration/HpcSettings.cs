namespace MathVerse.Math.HPC.Configuration;

using System;

/// <summary>
/// Global HPC settings record containing all subsystem configurations.
/// </summary>
/// <param name="DefaultTimeout">Default timeout for operations.</param>
/// <param name="MaxRecursionDepth">Maximum recursion depth for recursive algorithms.</param>
/// <param name="MaxBranchCount">Maximum branch count for branching algorithms.</param>
/// <param name="EnableSIMD">Whether SIMD operations are enabled globally.</param>
/// <param name="EnableParallel">Whether parallel execution is enabled globally.</param>
/// <param name="EnableFusion">Whether kernel fusion is enabled globally.</param>
/// <param name="EnableIncremental">Whether incremental execution is enabled globally.</param>
/// <param name="EnableParallelOptimization">Whether parallel optimization is enabled globally.</param>
/// <param name="MaxParallelism">Maximum global parallelism (0 = unlimited).</param>
/// <param name="MaxCacheSizeBytes">Maximum global cache size in bytes.</param>
/// <param name="SIMD">SIMD configuration.</param>
/// <param name="Parallel">Parallel execution configuration.</param>
/// <param name="Fusion">Kernel fusion configuration.</param>
/// <param name="Memory">Memory management configuration.</param>
/// <param name="Gpu">GPU computing configuration.</param>
/// <param name="Distributed">Distributed computing configuration.</param>
public sealed record HpcSettings(
    TimeSpan DefaultTimeout,
    int MaxRecursionDepth,
    int MaxBranchCount,
    bool EnableSIMD,
    bool EnableParallel,
    bool EnableFusion,
    bool EnableIncremental,
    bool EnableParallelOptimization,
    int MaxParallelism,
    long MaxCacheSizeBytes,
    SimdConfiguration SIMD,
    ParallelConfiguration Parallel,
    FusionConfiguration Fusion,
    MemoryConfiguration Memory,
    GpuConfiguration Gpu,
    DistributedConfiguration Distributed)
{
    /// <summary>
    /// Creates default HPC settings with all subsystems enabled.
    /// </summary>
    public static HpcSettings Default { get; } = new(
        DefaultTimeout: TimeSpan.FromMinutes(5),
        MaxRecursionDepth: 1000,
        MaxBranchCount: 10000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 256L * 1024 * 1024 * 1024,
        SIMD: SimdConfiguration.Default,
        Parallel: ParallelConfiguration.Default,
        Fusion: FusionConfiguration.Default,
        Memory: MemoryConfiguration.Default,
        Gpu: GpuConfiguration.Default,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Creates minimal HPC settings with only essential features enabled.
    /// </summary>
    public static HpcSettings Minimal { get; } = new(
        DefaultTimeout: TimeSpan.FromSeconds(30),
        MaxRecursionDepth: 100,
        MaxBranchCount: 100,
        EnableSIMD: false,
        EnableParallel: false,
        EnableFusion: false,
        EnableIncremental: false,
        EnableParallelOptimization: false,
        MaxParallelism: 1,
        MaxCacheSizeBytes: 16L * 1024 * 1024,
        SIMD: SimdConfiguration.Minimal,
        Parallel: ParallelConfiguration.Sequential,
        Fusion: FusionConfiguration.Minimal,
        Memory: MemoryConfiguration.Minimal,
        Gpu: GpuConfiguration.Minimal,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Creates HPC settings optimized for high-performance computing.
    /// </summary>
    public static HpcSettings HighPerformance { get; } = new(
        DefaultTimeout: TimeSpan.FromHours(1),
        MaxRecursionDepth: 10000,
        MaxBranchCount: 100000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 1L << 30,
        SIMD: SimdConfiguration.Avx512Optimized,
        Parallel: ParallelConfiguration.CpuBoundOptimized,
        Fusion: FusionConfiguration.Aggressive,
        Memory: MemoryConfiguration.HpcOptimized,
        Gpu: GpuConfiguration.CudaOptimized,
        Distributed: DistributedConfiguration.MpiCluster);

    /// <summary>
    /// Creates HPC settings optimized for GPU computing.
    /// </summary>
    public static HpcSettings GpuOptimized { get; } = new(
        DefaultTimeout: TimeSpan.FromMinutes(30),
        MaxRecursionDepth: 1000,
        MaxBranchCount: 10000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 512L * 1024 * 1024 * 1024,
        SIMD: SimdConfiguration.Avx512Optimized,
        Parallel: ParallelConfiguration.CpuBoundOptimized,
        Fusion: FusionConfiguration.GpuOptimized,
        Memory: MemoryConfiguration.GpuOptimized,
        Gpu: GpuConfiguration.CudaOptimized,
        Distributed: DistributedConfiguration.NcclCluster);

    /// <summary>
    /// Creates HPC settings optimized for distributed computing.
    /// </summary>
    public static HpcSettings DistributedOptimized { get; } = new(
        DefaultTimeout: TimeSpan.FromHours(2),
        MaxRecursionDepth: 10000,
        MaxBranchCount: 100000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 1L << 30,
        SIMD: SimdConfiguration.Default,
        Parallel: ParallelConfiguration.NumaOptimized,
        Fusion: FusionConfiguration.Default,
        Memory: MemoryConfiguration.NumaOptimized,
        Gpu: GpuConfiguration.MultiGpu,
        Distributed: DistributedConfiguration.MpiCluster);

    /// <summary>
    /// Creates minimal settings for testing.
    /// </summary>
    public static HpcSettings Testing { get; } = new(
        DefaultTimeout: TimeSpan.FromSeconds(10),
        MaxRecursionDepth: 10,
        MaxBranchCount: 10,
        EnableSIMD: false,
        EnableParallel: false,
        EnableFusion: false,
        EnableIncremental: false,
        EnableParallelOptimization: false,
        MaxParallelism: 1,
        MaxCacheSizeBytes: 1024 * 1024,
        SIMD: SimdConfiguration.Minimal,
        Parallel: ParallelConfiguration.Sequential,
        Fusion: FusionConfiguration.Minimal,
        Memory: MemoryConfiguration.Minimal,
        Gpu: GpuConfiguration.Minimal,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Creates HPC settings with custom timeout.
    /// </summary>
    public HpcSettings WithTimeout(TimeSpan timeout) => this with { DefaultTimeout = timeout };

    /// <summary>
    /// Creates HPC settings with custom recursion depth.
    /// </summary>
    public HpcSettings WithMaxDepth(int depth) => this with { MaxRecursionDepth = depth };

    /// <summary>
    /// Creates HPC settings with custom branch count.
    /// </summary>
    public HpcSettings WithMaxBranches(int count) => this with { MaxBranchCount = count };

    /// <summary>
    /// Creates HPC settings with SIMD enabled/disabled.
    /// </summary>
    public HpcSettings WithSIMD(bool enable) => this with { EnableSIMD = enable };

    /// <summary>
    /// Creates HPC settings with parallel execution enabled/disabled.
    /// </summary>
    public HpcSettings WithParallel(bool enable) => this with { EnableParallel = enable };

    /// <summary>
    /// Creates HPC settings with fusion enabled/disabled.
    /// </summary>
    public HpcSettings WithFusion(bool enable) => this with { EnableFusion = enable };

    /// <summary>
    /// Creates HPC settings with custom cache size.
    /// </summary>
    public HpcSettings WithCacheSize(long maxBytes) => this with { MaxCacheSizeBytes = maxBytes };

    /// <summary>
    /// Creates HPC settings with custom parallelism limit.
    /// </summary>
    public HpcSettings WithParallelism(int max) => this with { MaxParallelism = max };

    /// <summary>
    /// Creates HPC settings with custom SIMD configuration.
    /// </summary>
    public HpcSettings WithSimdConfig(SimdConfiguration config) => this with { SIMD = config };

    /// <summary>
    /// Creates HPC settings with custom parallel configuration.
    /// </summary>
    public HpcSettings WithParallelConfig(ParallelConfiguration config) => this with { Parallel = config };

    /// <summary>
    /// Creates HPC settings with custom fusion configuration.
    /// </summary>
    public HpcSettings WithFusionConfig(FusionConfiguration config) => this with { Fusion = config };

    /// <summary>
    /// Creates HPC settings with custom memory configuration.
    /// </summary>
    public HpcSettings WithMemoryConfig(MemoryConfiguration config) => this with { Memory = config };

    /// <summary>
    /// Creates HPC settings with custom GPU configuration.
    /// </summary>
    public HpcSettings WithGpuConfig(GpuConfiguration config) => this with { Gpu = config };

    /// <summary>
    /// Creates HPC settings with custom distributed configuration.
    /// </summary>
    public HpcSettings WithDistributedConfig(DistributedConfiguration config) => this with { Distributed = config };
}
