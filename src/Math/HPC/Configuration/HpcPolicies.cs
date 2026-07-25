namespace MathVerse.Math.HPC.Configuration;

using System;

/// <summary>
/// Static policy configurations for common HPC scenarios.
/// </summary>
public static class HpcPolicies
{
    /// <summary>
    /// Quick policy: Fast execution with basic optimizations.
    /// </summary>
    public static HpcSettings Quick => new HpcSettings(
        DefaultTimeout: TimeSpan.FromSeconds(30),
        MaxRecursionDepth: 100,
        MaxBranchCount: 1000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: false,
        EnableIncremental: true,
        EnableParallelOptimization: false,
        MaxParallelism: Environment.ProcessorCount,
        MaxCacheSizeBytes: 64L * 1024 * 1024,
        SIMD: SimdConfiguration.Default,
        Parallel: ParallelConfiguration.Default,
        Fusion: FusionConfiguration.Minimal,
        Memory: MemoryConfiguration.Default,
        Gpu: GpuConfiguration.Minimal,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Standard policy: Balanced optimization for general workloads.
    /// </summary>
    public static HpcSettings Standard => new HpcSettings(
        DefaultTimeout: TimeSpan.FromMinutes(5),
        MaxRecursionDepth: 1000,
        MaxBranchCount: 10000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: Environment.ProcessorCount,
        MaxCacheSizeBytes: 256L * 1024 * 1024 * 1024,
        SIMD: SimdConfiguration.Default,
        Parallel: ParallelConfiguration.Default,
        Fusion: FusionConfiguration.Default,
        Memory: MemoryConfiguration.Default,
        Gpu: GpuConfiguration.Default,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Thorough policy: Aggressive optimization with extended timeouts.
    /// </summary>
    public static HpcSettings Thorough => new HpcSettings(
        DefaultTimeout: TimeSpan.FromMinutes(15),
        MaxRecursionDepth: 5000,
        MaxBranchCount: 50000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 1L << 30,
        SIMD: SimdConfiguration.Avx2Optimized,
        Parallel: ParallelConfiguration.CpuBoundOptimized,
        Fusion: FusionConfiguration.Aggressive,
        Memory: MemoryConfiguration.HpcOptimized,
        Gpu: GpuConfiguration.CudaOptimized,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Exhaustive policy: Maximum optimization with no timeout constraints.
    /// </summary>
    public static HpcSettings Exhaustive => new HpcSettings(
        DefaultTimeout: TimeSpan.FromHours(2),
        MaxRecursionDepth: 10000,
        MaxBranchCount: 100000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: false,
        EnableParallelOptimization: true,
        MaxParallelism: 0,
        MaxCacheSizeBytes: 4L * 1024 * 1024 * 1024,
        SIMD: SimdConfiguration.Avx512Optimized,
        Parallel: ParallelConfiguration.CpuBoundOptimized,
        Fusion: FusionConfiguration.Aggressive,
        Memory: MemoryConfiguration.HpcOptimized,
        Gpu: GpuConfiguration.TrainingOptimized,
        Distributed: DistributedConfiguration.DeepSpeedZero);

    /// <summary>
    /// GPU-optimized policy: Maximum GPU utilization.
    /// </summary>
    public static HpcSettings GpuOptimized => new HpcSettings(
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
    /// Distributed-optimized policy: Maximum cluster utilization.
    /// </summary>
    public static HpcSettings DistributedOptimized => new HpcSettings(
        DefaultTimeout: TimeSpan.FromHours(4),
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
        Distributed: DistributedConfiguration.ThreeDParallel);

    /// <summary>
    /// Development/Debug policy: Fast feedback with full diagnostics.
    /// </summary>
    public static HpcSettings Development => new HpcSettings(
        DefaultTimeout: TimeSpan.FromSeconds(10),
        MaxRecursionDepth: 100,
        MaxBranchCount: 100,
        EnableSIMD: false,
        EnableParallel: false,
        EnableFusion: false,
        EnableIncremental: true,
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
    /// Production policy: Reliable, performant, with monitoring.
    /// </summary>
    public static HpcSettings Production => new HpcSettings(
        DefaultTimeout: TimeSpan.FromMinutes(10),
        MaxRecursionDepth: 2000,
        MaxBranchCount: 20000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: true,
        EnableParallelOptimization: true,
        MaxParallelism: Environment.ProcessorCount,
        MaxCacheSizeBytes: 1L << 30,
        SIMD: SimdConfiguration.Avx2Optimized,
        Parallel: ParallelConfiguration.CpuBoundOptimized,
        Fusion: FusionConfiguration.CpuOptimized,
        Memory: MemoryConfiguration.HpcOptimized,
        Gpu: GpuConfiguration.CudaOptimized,
        Distributed: DistributedConfiguration.MpiCluster);

    /// <summary>
    /// Benchmarking policy: Consistent results, no background noise.
    /// </summary>
    public static HpcSettings Benchmarking => new HpcSettings(
        DefaultTimeout: TimeSpan.FromMinutes(5),
        MaxRecursionDepth: 1000,
        MaxBranchCount: 10000,
        EnableSIMD: true,
        EnableParallel: true,
        EnableFusion: true,
        EnableIncremental: false,
        EnableParallelOptimization: true,
        MaxParallelism: 1,
        MaxCacheSizeBytes: 0,
        SIMD: SimdConfiguration.Avx2Optimized,
        Parallel: new ParallelConfiguration(
            Enabled: true,
            MaxDegreeOfParallelism: 1,
            EnableWorkStealing: false,
            EnableDynamicPartitioning: false,
            EnableStaticPartitioning: true,
            LoadBalancingStrategy: LoadBalancingStrategy.Static),
        Fusion: FusionConfiguration.Default,
        Memory: new MemoryConfiguration(
            EnableMemoryPool: false,
            EnableArrayPool: false,
            EnableMemoryPooling: false,
            EnableCacheOptimization: true),
        Gpu: GpuConfiguration.Minimal,
        Distributed: DistributedConfiguration.Default);

    /// <summary>
    /// Gets a policy by name.
    /// </summary>
    /// <param name="policyName">The policy name (case-insensitive).</param>
    /// <returns>The HPC settings for the policy.</returns>
    /// <exception cref="ArgumentException">Thrown when policy name is not recognized.</exception>
    public static HpcSettings GetPolicy(string policyName)
    {
        return policyName.ToLowerInvariant() switch
        {
            "quick" => Quick,
            "standard" => Standard,
            "thorough" => Thorough,
            "exhaustive" => Exhaustive,
            "gpu" or "gpuoptimized" => GpuOptimized,
            "distributed" or "distributedoptimized" => DistributedOptimized,
            "development" or "dev" => Development,
            "production" or "prod" => Production,
            "benchmarking" or "benchmark" => Benchmarking,
            _ => throw new ArgumentException($"Unknown policy: {policyName}", nameof(policyName))
        };
    }

    /// <summary>
    /// Gets all available policy names.
    /// </summary>
    public static IReadOnlyList<string> GetPolicyNames() =>
        new[] { "Quick", "Standard", "Thorough", "Exhaustive", "GpuOptimized", "DistributedOptimized", "Development", "Production", "Benchmarking" };
}
