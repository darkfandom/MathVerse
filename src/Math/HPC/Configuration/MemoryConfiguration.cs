namespace MathVerse.Math.HPC.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for memory management and optimization.
/// </summary>
/// <param name="MaxCacheSizeBytes">Maximum cache size in bytes.</param>
/// <param name="EnableMemoryPool">Whether to enable memory pooling.</param>
/// <param name="EnableArrayPool">Whether to enable array pooling.</param>
/// <param name="EnableMemoryPooling">Whether to enable general memory pooling.</param>
/// <param name="PoolBlockSize">Memory pool block size in bytes.</param>
/// <param name="MaxPoolBlocks">Maximum number of blocks in pool.</param>
/// <param name="EnablePinnedMemory">Whether to enable pinned memory allocation.</param>
/// <param name="EnableZeroCopy">Whether to enable zero-copy operations.</param>
/// <param name="EnableMemoryMapping">Whether to enable memory mapping.</param>
/// <param name="EnableCompression">Whether to enable memory compression.</param>
/// <param name="EnableDeduplication">Whether to enable memory deduplication.</param>
/// <param name="EnablePrefetching">Whether to enable memory prefetching.</param>
/// <param name="EnableWriteCombining">Whether to enable write combining.</param>
/// <param name="EnableNonTemporalStores">Whether to enable non-temporal stores.</param>
/// <param name="EnableHugePages">Whether to enable huge pages.</param>
/// <param name="HugePageSize">Huge page size in bytes.</param>
/// <param name="EnableNumaLocalAllocation">Whether to enable NUMA-local allocation.</param>
/// <param name="PreferredNumaNode">Preferred NUMA node for allocation.</param>
/// <param name="EnableMemoryTracking">Whether to enable memory tracking.</param>
/// <param name="EnableLeakDetection">Whether to enable leak detection.</param>
/// <param name="EnableAllocationTracking">Whether to enable allocation tracking.</param>
/// <param name="MaxAllocationSize">Maximum single allocation size.</param>
/// <param name="AlignmentRequirement">Required memory alignment in bytes.</param>
/// <param name="EnableCacheOptimization">Whether to enable cache optimization.</param>
/// <param name="CacheLineSize">Cache line size in bytes.</param>
/// <param name="L1CacheSize">L1 cache size in bytes.</param>
/// <param name="L2CacheSize">L2 cache size in bytes.</param>
/// <param name="L3CacheSize">L3 cache size in bytes.</param>
/// <param name="EnableCacheBlocking">Whether to enable cache blocking.</param>
/// <param name="BlockSize">Cache block size in bytes.</param>
/// <param name="EnablePrefetchHints">Whether to enable prefetch hints.</param>
/// <param name="PrefetchDistance">Prefetch distance in cache lines.</param>
/// <param name="EnableMemoryTiering">Whether to enable memory tiering.</param>
/// <param name="TieringPolicy">Memory tiering policy.</param>
/// <param name="CustomAllocators">Custom allocator implementations.</param>
/// <param name="EnableAllocationProfiling">Whether to enable allocation profiling.</param>
/// <param name="AllocationProfilePath">Path to allocation profile data.</param>
public sealed record MemoryConfiguration(
    long MaxCacheSizeBytes = 256 * 1024 * 1024,
    bool EnableMemoryPool = true,
    bool EnableArrayPool = true,
    bool EnableMemoryPooling = true,
    int PoolBlockSize = 64 * 1024,
    int MaxPoolBlocks = 4096,
    bool EnablePinnedMemory = true,
    bool EnableZeroCopy = true,
    bool EnableMemoryMapping = true,
    bool EnableCompression = false,
    bool EnableDeduplication = false,
    bool EnablePrefetching = true,
    bool EnableWriteCombining = true,
    bool EnableNonTemporalStores = true,
    bool EnableHugePages = false,
    int HugePageSize = 2 * 1024 * 1024,
    bool EnableNumaLocalAllocation = true,
    int PreferredNumaNode = -1,
    bool EnableMemoryTracking = false,
    bool EnableLeakDetection = false,
    bool EnableAllocationTracking = false,
    long MaxAllocationSize = 1L << 30,
    int AlignmentRequirement = 64,
    bool EnableCacheOptimization = true,
    int CacheLineSize = 64,
    int L1CacheSize = 32 * 1024,
    int L2CacheSize = 256 * 1024,
    int L3CacheSize = 8 * 1024 * 1024,
    bool EnableCacheBlocking = true,
    int BlockSize = 64 * 1024,
    bool EnablePrefetchHints = true,
    int PrefetchDistance = 16,
    bool EnableMemoryTiering = false,
    MemoryTieringPolicy TieringPolicy = MemoryTieringPolicy.Auto,
    IDictionary<string, object>? CustomAllocators = null,
    bool EnableAllocationProfiling = false,
    string? AllocationProfilePath = null)
{
    /// <summary>
    /// Default memory configuration.
    /// </summary>
    public static MemoryConfiguration Default { get; } = new();

    /// <summary>
    /// Memory configuration optimized for high-performance computing.
    /// </summary>
    public static MemoryConfiguration HpcOptimized { get; } = new(
        MaxCacheSizeBytes: 1L << 30,
        PoolBlockSize: 256 * 1024,
        MaxPoolBlocks: 16384,
        EnablePinnedMemory: true,
        EnableZeroCopy: true,
        EnableHugePages: true,
        EnableNumaLocalAllocation: true,
        EnableCacheOptimization: true,
        BlockSize: 256 * 1024,
        EnablePrefetchHints: true,
        PrefetchDistance: 32,
        L1CacheSize: 48 * 1024,
        L2CacheSize: 1024 * 1024,
        L3CacheSize: 32 * 1024 * 1024);

    /// <summary>
    /// Memory configuration optimized for GPU workloads.
    /// </summary>
    public static MemoryConfiguration GpuOptimized { get; } = new(
        MaxCacheSizeBytes: 512 * 1024 * 1024,
        PoolBlockSize: 1024 * 1024,
        MaxPoolBlocks: 8192,
        EnablePinnedMemory: true,
        EnableZeroCopy: true,
        EnableMemoryMapping: true,
        EnableNonTemporalStores: true,
        EnableCacheOptimization: true,
        BlockSize: 128 * 1024,
        PrefetchDistance: 64,
        AlignmentRequirement: 256);

    /// <summary>
    /// Memory configuration optimized for low memory usage.
    /// </summary>
    public static MemoryConfiguration LowMemory { get; } = new(
        MaxCacheSizeBytes: 32 * 1024 * 1024,
        PoolBlockSize: 16 * 1024,
        MaxPoolBlocks: 1024,
        EnableCompression: true,
        EnableDeduplication: true,
        EnableHugePages: false,
        EnableCacheOptimization: true,
        BlockSize: 16 * 1024,
        MaxAllocationSize: 64 * 1024 * 1024);

    /// <summary>
    /// Memory configuration optimized for NUMA systems.
    /// </summary>
    public static MemoryConfiguration NumaOptimized { get; } = new(
        EnableNumaLocalAllocation: true,
        PreferredNumaNode: 0,
        EnableMemoryPool: true,
        PoolBlockSize: 128 * 1024,
        MaxPoolBlocks: 8192,
        EnableCacheOptimization: true,
        L3CacheSize: 64 * 1024 * 1024);

    /// <summary>
    /// Memory configuration with minimal features.
    /// </summary>
    public static MemoryConfiguration Minimal { get; } = new(
        EnableMemoryPool: false,
        EnableArrayPool: false,
        EnableMemoryPooling: false,
        EnablePinnedMemory: false,
        EnableZeroCopy: false,
        EnableMemoryMapping: false,
        EnablePrefetching: false,
        EnableCacheOptimization: false,
        MaxCacheSizeBytes: 16 * 1024 * 1024);

    /// <summary>
    /// Memory configuration with profiling enabled.
    /// </summary>
    public static MemoryConfiguration Profiling { get; } = new(
        EnableMemoryTracking: true,
        EnableLeakDetection: true,
        EnableAllocationTracking: true,
        AllocationProfilePath: "memory_profile.json",
        EnableAllocationProfiling: true);

    /// <summary>
    /// Creates a memory configuration with custom cache sizes.
    /// </summary>
    public static MemoryConfiguration WithCacheSizes(int l1, int l2, int l3) =>
        new(L1CacheSize: l1, L2CacheSize: l2, L3CacheSize: l3);

    /// <summary>
    /// Creates a memory configuration with custom cache size.
    /// </summary>
    public static MemoryConfiguration WithCacheSize(long maxBytes) =>
        new(MaxCacheSizeBytes: maxBytes);
}

/// <summary>
/// Memory tiering policies.
/// </summary>
public enum MemoryTieringPolicy
{
    /// <summary>
    /// Automatic tiering policy.
    /// </summary>
    Auto,

    /// <summary>
    /// Keep hot data in fast memory.
    /// </summary>
    HotInFastMemory,

    /// <summary>
    /// Tier by access frequency.
    /// </summary>
    AccessFrequency,

    /// <summary>
    /// Tier by data size.
    /// </summary>
    DataSize,

    /// <summary>
    /// Tier by access pattern.
    /// </summary>
    AccessPattern,

    /// <summary>
    /// Manual tiering.
    /// </summary>
    Manual,

    /// <summary>
    /// No tiering.
    /// </summary>
    None
}
