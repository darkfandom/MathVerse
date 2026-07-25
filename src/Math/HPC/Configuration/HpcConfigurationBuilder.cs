namespace MathVerse.Math.HPC.Configuration;

using System;
using MathVerse.Math.HPC.Core;

/// <summary>
/// Fluent builder for HPC settings.
/// </summary>
public sealed class HpcConfigurationBuilder
{
    private TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);
    private int _maxRecursionDepth = 1000;
    private int _maxBranchCount = 10000;
    private bool _enableSIMD = true;
    private bool _enableParallel = true;
    private bool _enableFusion = true;
    private bool _enableIncremental = true;
    private bool _enableParallelOptimization = true;
    private int _maxParallelism = 0;
    private long _maxCacheSizeBytes = 256L * 1024 * 1024 * 1024;
    private SimdConfiguration _simd = SimdConfiguration.Default;
    private ParallelConfiguration _parallel = ParallelConfiguration.Default;
    private FusionConfiguration _fusion = FusionConfiguration.Default;
    private MemoryConfiguration _memory = MemoryConfiguration.Default;
    private GpuConfiguration _gpu = GpuConfiguration.Default;
    private DistributedConfiguration _distributed = DistributedConfiguration.Default;

    /// <summary>
    /// Sets the default timeout.
    /// </summary>
    public HpcConfigurationBuilder WithDefaultTimeout(TimeSpan timeout)
    {
        _defaultTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the maximum recursion depth.
    /// </summary>
    public HpcConfigurationBuilder WithMaxRecursionDepth(int depth)
    {
        _maxRecursionDepth = depth;
        return this;
    }

    /// <summary>
    /// Sets the maximum branch count.
    /// </summary>
    public HpcConfigurationBuilder WithMaxBranchCount(int count)
    {
        _maxBranchCount = count;
        return this;
    }

    /// <summary>
    /// Enables or disables SIMD globally.
    /// </summary>
    public HpcConfigurationBuilder WithSIMD(bool enable)
    {
        _enableSIMD = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables parallel execution globally.
    /// </summary>
    public HpcConfigurationBuilder WithParallel(bool enable)
    {
        _enableParallel = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables kernel fusion globally.
    /// </summary>
    public HpcConfigurationBuilder WithFusion(bool enable)
    {
        _enableFusion = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables incremental execution globally.
    /// </summary>
    public HpcConfigurationBuilder WithIncremental(bool enable)
    {
        _enableIncremental = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables parallel optimization globally.
    /// </summary>
    public HpcConfigurationBuilder WithParallelOptimization(bool enable)
    {
        _enableParallelOptimization = enable;
        return this;
    }

    /// <summary>
    /// Sets the maximum global parallelism.
    /// </summary>
    public HpcConfigurationBuilder WithMaxParallelism(int max)
    {
        _maxParallelism = max;
        return this;
    }

    /// <summary>
    /// Sets the maximum global cache size.
    /// </summary>
    public HpcConfigurationBuilder WithMaxCacheSize(long maxBytes)
    {
        _maxCacheSizeBytes = maxBytes;
        return this;
    }

    /// <summary>
    /// Sets the SIMD configuration.
    /// </summary>
    public HpcConfigurationBuilder WithSimdConfig(SimdConfiguration config)
    {
        _simd = config;
        return this;
    }

    /// <summary>
    /// Sets the parallel execution configuration.
    /// </summary>
    public HpcConfigurationBuilder WithParallelConfig(ParallelConfiguration config)
    {
        _parallel = config;
        return this;
    }

    /// <summary>
    /// Sets the kernel fusion configuration.
    /// </summary>
    public HpcConfigurationBuilder WithFusionConfig(FusionConfiguration config)
    {
        _fusion = config;
        return this;
    }

    /// <summary>
    /// Sets the memory management configuration.
    /// </summary>
    public HpcConfigurationBuilder WithMemoryConfig(MemoryConfiguration config)
    {
        _memory = config;
        return this;
    }

    /// <summary>
    /// Sets the GPU computing configuration.
    /// </summary>
    public HpcConfigurationBuilder WithGpuConfig(GpuConfiguration config)
    {
        _gpu = config;
        return this;
    }

    /// <summary>
    /// Sets the distributed computing configuration.
    /// </summary>
    public HpcConfigurationBuilder WithDistributedConfig(DistributedConfiguration config)
    {
        _distributed = config;
        return this;
    }

    /// <summary>
    /// Applies a preset configuration.
    /// </summary>
    public HpcConfigurationBuilder ApplyPreset(HpcPreset preset)
    {
        return preset switch
        {
            HpcPreset.Default => ResetToDefaults(),
            HpcPreset.Minimal => WithMinimalSettings(),
            HpcPreset.HighPerformance => WithHighPerformanceSettings(),
            HpcPreset.GpuOptimized => WithGpuOptimizedSettings(),
            HpcPreset.DistributedOptimized => WithDistributedOptimizedSettings(),
            HpcPreset.Testing => WithTestingSettings(),
            _ => this
        };
    }

    /// <summary>
    /// Resets to default settings.
    /// </summary>
    public HpcConfigurationBuilder ResetToDefaults()
    {
        _defaultTimeout = TimeSpan.FromMinutes(5);
        _maxRecursionDepth = 1000;
        _maxBranchCount = 10000;
        _enableSIMD = true;
        _enableParallel = true;
        _enableFusion = true;
        _enableIncremental = true;
        _enableParallelOptimization = true;
        _maxParallelism = 0;
        _maxCacheSizeBytes = 256L * 1024 * 1024 * 1024;
        _simd = SimdConfiguration.Default;
        _parallel = ParallelConfiguration.Default;
        _fusion = FusionConfiguration.Default;
        _memory = MemoryConfiguration.Default;
        _gpu = GpuConfiguration.Default;
        _distributed = DistributedConfiguration.Default;
        return this;
    }

    /// <summary>
    /// Configures minimal settings.
    /// </summary>
    public HpcConfigurationBuilder WithMinimalSettings()
    {
        _defaultTimeout = TimeSpan.FromSeconds(30);
        _maxRecursionDepth = 100;
        _maxBranchCount = 100;
        _enableSIMD = false;
        _enableParallel = false;
        _enableFusion = false;
        _enableIncremental = false;
        _enableParallelOptimization = false;
        _maxParallelism = 1;
        _maxCacheSizeBytes = 16L * 1024 * 1024;
        _simd = SimdConfiguration.Minimal;
        _parallel = ParallelConfiguration.Sequential;
        _fusion = FusionConfiguration.Minimal;
        _memory = MemoryConfiguration.Minimal;
        _gpu = GpuConfiguration.Minimal;
        _distributed = DistributedConfiguration.Default;
        return this;
    }

    /// <summary>
    /// Configures high-performance settings.
    /// </summary>
    public HpcConfigurationBuilder WithHighPerformanceSettings()
    {
        _defaultTimeout = TimeSpan.FromHours(1);
        _maxRecursionDepth = 10000;
        _maxBranchCount = 100000;
        _enableSIMD = true;
        _enableParallel = true;
        _enableFusion = true;
        _enableIncremental = true;
        _enableParallelOptimization = true;
        _maxParallelism = 0;
        _maxCacheSizeBytes = 1L << 30;
        _simd = SimdConfiguration.Avx512Optimized;
        _parallel = ParallelConfiguration.CpuBoundOptimized;
        _fusion = FusionConfiguration.Aggressive;
        _memory = MemoryConfiguration.HpcOptimized;
        _gpu = GpuConfiguration.CudaOptimized;
        _distributed = DistributedConfiguration.MpiCluster;
        return this;
    }

    /// <summary>
    /// Configures GPU-optimized settings.
    /// </summary>
    public HpcConfigurationBuilder WithGpuOptimizedSettings()
    {
        _defaultTimeout = TimeSpan.FromMinutes(30);
        _maxRecursionDepth = 1000;
        _maxBranchCount = 10000;
        _enableSIMD = true;
        _enableParallel = true;
        _enableFusion = true;
        _enableIncremental = true;
        _enableParallelOptimization = true;
        _maxParallelism = 0;
        _maxCacheSizeBytes = 512L * 1024 * 1024 * 1024;
        _simd = SimdConfiguration.Avx512Optimized;
        _parallel = ParallelConfiguration.CpuBoundOptimized;
        _fusion = FusionConfiguration.GpuOptimized;
        _memory = MemoryConfiguration.GpuOptimized;
        _gpu = GpuConfiguration.CudaOptimized;
        _distributed = DistributedConfiguration.NcclCluster;
        return this;
    }

    /// <summary>
    /// Configures distributed-optimized settings.
    /// </summary>
    public HpcConfigurationBuilder WithDistributedOptimizedSettings()
    {
        _defaultTimeout = TimeSpan.FromHours(2);
        _maxRecursionDepth = 10000;
        _maxBranchCount = 100000;
        _enableSIMD = true;
        _enableParallel = true;
        _enableFusion = true;
        _enableIncremental = true;
        _enableParallelOptimization = true;
        _maxParallelism = 0;
        _maxCacheSizeBytes = 1L << 30;
        _simd = SimdConfiguration.Default;
        _parallel = ParallelConfiguration.NumaOptimized;
        _fusion = FusionConfiguration.Default;
        _memory = MemoryConfiguration.NumaOptimized;
        _gpu = GpuConfiguration.MultiGpu;
        _distributed = DistributedConfiguration.MpiCluster;
        return this;
    }

    /// <summary>
    /// Configures testing settings.
    /// </summary>
    public HpcConfigurationBuilder WithTestingSettings()
    {
        _defaultTimeout = TimeSpan.FromSeconds(10);
        _maxRecursionDepth = 10;
        _maxBranchCount = 10;
        _enableSIMD = false;
        _enableParallel = false;
        _enableFusion = false;
        _enableIncremental = false;
        _enableParallelOptimization = false;
        _maxParallelism = 1;
        _maxCacheSizeBytes = 1024 * 1024;
        _simd = SimdConfiguration.Minimal;
        _parallel = ParallelConfiguration.Sequential;
        _fusion = FusionConfiguration.Minimal;
        _memory = MemoryConfiguration.Minimal;
        _gpu = GpuConfiguration.Minimal;
        _distributed = DistributedConfiguration.Default;
        return this;
    }

    /// <summary>
    /// Builds the HPC settings.
    /// </summary>
    public HpcSettings Build()
    {
        return new HpcSettings(
            DefaultTimeout: _defaultTimeout,
            MaxRecursionDepth: _maxRecursionDepth,
            MaxBranchCount: _maxBranchCount,
            EnableSIMD: _enableSIMD,
            EnableParallel: _enableParallel,
            EnableFusion: _enableFusion,
            EnableIncremental: _enableIncremental,
            EnableParallelOptimization: _enableParallelOptimization,
            MaxParallelism: _maxParallelism,
            MaxCacheSizeBytes: _maxCacheSizeBytes,
            SIMD: _simd,
            Parallel: _parallel,
            Fusion: _fusion,
            Memory: _memory,
            Gpu: _gpu,
            Distributed: _distributed);
    }

    /// <summary>
    /// Creates a builder with default settings.
    /// </summary>
    public static HpcConfigurationBuilder Create() => new();

    /// <summary>
    /// Creates a builder with high-performance settings.
    /// </summary>
    public static HpcConfigurationBuilder CreateHighPerformance() => new HpcConfigurationBuilder().WithHighPerformanceSettings();

    /// <summary>
    /// Creates a builder with GPU-optimized settings.
    /// </summary>
    public static HpcConfigurationBuilder CreateGpuOptimized() => new HpcConfigurationBuilder().WithGpuOptimizedSettings();

    /// <summary>
    /// Creates a builder with distributed-optimized settings.
    /// </summary>
    public static HpcConfigurationBuilder CreateDistributedOptimized() => new HpcConfigurationBuilder().WithDistributedOptimizedSettings();

    /// <summary>
    /// Creates a builder with minimal settings.
    /// </summary>
    public static HpcConfigurationBuilder CreateMinimal() => new HpcConfigurationBuilder().WithMinimalSettings();

    /// <summary>
    /// Creates a builder with testing settings.
    /// </summary>
    public static HpcConfigurationBuilder CreateTesting() => new HpcConfigurationBuilder().WithTestingSettings();
}

/// <summary>
/// HPC configuration presets.
/// </summary>
public enum HpcPreset
{
    /// <summary>
    /// Default balanced configuration.
    /// </summary>
    Default,

    /// <summary>
    /// Minimal configuration for constrained environments.
    /// </summary>
    Minimal,

    /// <summary>
    /// High-performance computing configuration.
    /// </summary>
    HighPerformance,

    /// <summary>
    /// GPU-optimized configuration.
    /// </summary>
    GpuOptimized,

    /// <summary>
    /// Distributed computing optimized configuration.
    /// </summary>
    DistributedOptimized,

    /// <summary>
    /// Testing configuration.
    /// </summary>
    Testing
}
