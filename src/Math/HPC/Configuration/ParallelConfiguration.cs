namespace MathVerse.Math.HPC.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for parallel execution.
/// </summary>
/// <param name="Enabled">Whether parallel execution is enabled.</param>
/// <param name="MaxDegreeOfParallelism">Maximum degree of parallelism (0 = unlimited).</param>
/// <param name="MinChunkSize">Minimum chunk size for parallel partitioning.</param>
/// <param name="MaxChunkSize">Maximum chunk size for parallel partitioning.</param>
/// <param name="EnableWorkStealing">Whether to enable work-stealing scheduler.</param>
/// <param name="EnableDynamicPartitioning">Whether to enable dynamic partitioning.</param>
/// <param name="EnableStaticPartitioning">Whether to enable static partitioning.</param>
/// <param name="EnableTaskParallelism">Whether to enable task-level parallelism.</param>
/// <param name="EnableDataParallelism">Whether to enable data parallelism.</param>
/// <param name="EnablePipelineParallelism">Whether to enable pipeline parallelism.</param>
/// <param name="EnableNestedParallelism">Whether to enable nested parallelism.</param>
/// <param name="MaxNestedParallelismDepth">Maximum depth of nested parallelism.</param>
/// <param name="EnableLoadBalancing">Whether to enable load balancing.</param>
/// <param name="LoadBalancingStrategy">Load balancing strategy.</param>
/// <param name="EnablePriorityScheduling">Whether to enable priority-based scheduling.</param>
/// <param name="TaskPriority">Default task priority.</param>
/// <param name="EnableCancellation">Whether to enable cancellation support.</param>
/// <param name="CancellationToken">Cancellation token for cancellation.</param>
/// <param name="EnableProfiling">Whether to enable parallel execution profiling.</param>
/// <param name="PartitionerType">Type of partitioner to use.</param>
/// <param name="CustomPartitioner">Custom partitioner instance.</param>
/// <param name="ThreadPoolType">Type of thread pool to use.</param>
/// <param name="CustomThreadPool">Custom thread pool instance.</param>
/// <param name="EnableAffinity">Whether to enable thread affinity.</param>
/// <param name="AffinityMask">Thread affinity mask.</param>
/// <param name="EnableNumaAwareness">Whether to enable NUMA awareness.</param>
/// <param name="NumaNodeAffinity">Preferred NUMA node affinity.</param>
public sealed record ParallelConfiguration(
    bool Enabled = true,
    int MaxDegreeOfParallelism = 0,
    int MinChunkSize = 1,
    int MaxChunkSize = 1024,
    bool EnableWorkStealing = true,
    bool EnableDynamicPartitioning = true,
    bool EnableStaticPartitioning = true,
    bool EnableTaskParallelism = true,
    bool EnableDataParallelism = true,
    bool EnablePipelineParallelism = true,
    bool EnableNestedParallelism = false,
    int MaxNestedParallelismDepth = 2,
    bool EnableLoadBalancing = true,
    LoadBalancingStrategy LoadBalancingStrategy = LoadBalancingStrategy.WorkStealing,
    bool EnablePriorityScheduling = false,
    TaskPriority TaskPriority = TaskPriority.Normal,
    bool EnableCancellation = true,
    System.Threading.CancellationToken CancellationToken = default,
    bool EnableProfiling = false,
    PartitionerType PartitionerType = PartitionerType.Auto,
    object? CustomPartitioner = null,
    ThreadPoolType ThreadPoolType = ThreadPoolType.Default,
    object? CustomThreadPool = null,
    bool EnableAffinity = false,
    ulong AffinityMask = 0xFFFFFFFFFFFFFFFF,
    bool EnableNumaAwareness = false,
    int[]? NumaNodeAffinity = null)
{
    /// <summary>
    /// Default parallel configuration.
    /// </summary>
    public static ParallelConfiguration Default { get; } = new();

    /// <summary>
    /// Parallel configuration optimized for CPU-bound workloads.
    /// </summary>
    public static ParallelConfiguration CpuBoundOptimized { get; } = new(
        MaxDegreeOfParallelism: Environment.ProcessorCount,
        MinChunkSize: 32,
        MaxChunkSize: 4096,
        EnableWorkStealing: true,
        EnableDynamicPartitioning: true,
        LoadBalancingStrategy: LoadBalancingStrategy.WorkStealing,
        EnableAffinity: true);

    /// <summary>
    /// Parallel configuration optimized for I/O-bound workloads.
    /// </summary>
    public static ParallelConfiguration IoBoundOptimized { get; } = new(
        MaxDegreeOfParallelism: Environment.ProcessorCount * 4,
        MinChunkSize: 1,
        MaxChunkSize: 64,
        EnableWorkStealing: true,
        EnableDynamicPartitioning: true,
        LoadBalancingStrategy: LoadBalancingStrategy.Dynamic,
        EnablePriorityScheduling: true);

    /// <summary>
    /// Parallel configuration optimized for NUMA systems.
    /// </summary>
    public static ParallelConfiguration NumaOptimized { get; } = new(
        MaxDegreeOfParallelism: 0,
        EnableWorkStealing: true,
        EnableNumaAwareness: true,
        NumaNodeAffinity: Array.Empty<int>(),
        EnableAffinity: true);

    /// <summary>
    /// Parallel configuration with minimal parallelism (single-threaded fallback).
    /// </summary>
    public static ParallelConfiguration Sequential { get; } = new(
        Enabled: false,
        MaxDegreeOfParallelism: 1);

    /// <summary>
    /// Parallel configuration optimized for high-throughput batch processing.
    /// </summary>
    public static ParallelConfiguration BatchOptimized { get; } = new(
        MaxDegreeOfParallelism: Environment.ProcessorCount,
        MinChunkSize: 1024,
        MaxChunkSize: 65536,
        EnableStaticPartitioning: true,
        EnableDynamicPartitioning: false,
        LoadBalancingStrategy: LoadBalancingStrategy.Static,
        EnablePipelineParallelism: true);

    /// <summary>
    /// Creates a parallel configuration with a specific degree of parallelism.
    /// </summary>
    public static ParallelConfiguration WithDegree(int degree) => new(MaxDegreeOfParallelism: degree);

    /// <summary>
    /// Creates a parallel configuration with a custom partitioner.
    /// </summary>
    public static ParallelConfiguration WithCustomPartitioner(object partitioner) =>
        new(PartitionerType: PartitionerType.Custom, CustomPartitioner: partitioner);
}

/// <summary>
/// Load balancing strategies for parallel execution.
/// </summary>
public enum LoadBalancingStrategy
{
    /// <summary>
    /// Automatic strategy selection.
    /// </summary>
    Auto,

    /// <summary>
    /// Static partitioning (equal-sized chunks).
    /// </summary>
    Static,

    /// <summary>
    /// Dynamic partitioning (work-stealing).
    /// </summary>
    Dynamic,

    /// <summary>
    /// Work-stealing scheduler.
    /// </summary>
    WorkStealing,

    /// <summary>
    /// Guided self-scheduling.
    /// </summary>
    GuidedSelfScheduling,

    /// <summary>
    /// Factor-based scheduling.
    /// </summary>
    Factoring,

    /// <summary>
    /// Adaptive scheduling based on workload characteristics.
    /// </summary>
    Adaptive
}

/// <summary>
/// Partitioner types for parallel partitioning.
/// </summary>
public enum PartitionerType
{
    /// <summary>
    /// Automatic partitioner selection.
    /// </summary>
    Auto,

    /// <summary>
    /// Default partitioner.
    /// </summary>
    Default,

    /// <summary>
    /// Static partitioner (equal chunks).
    /// </summary>
    Static,

    /// <summary>
    /// Dynamic partitioner (work-stealing).
    /// </summary>
    Dynamic,

    /// <summary>
    /// Chunk partitioner with configurable chunk size.
    /// </summary>
    Chunk,

    /// <summary>
    /// Range partitioner for range-based iteration.
    /// </summary>
    Range,

    /// <summary>
    /// Custom partitioner.
    /// </summary>
    Custom
}

/// <summary>
/// Thread pool types.
/// </summary>
public enum ThreadPoolType
{
    /// <summary>
    /// Default thread pool.
    /// </summary>
    Default,

    /// <summary>
    /// Thread pool with dedicated threads.
    /// </summary>
    Dedicated,

    /// <summary>
    /// Thread pool with work-stealing.
    /// </summary>
    WorkStealing,

    /// <summary>
    /// Thread pool with NUMA awareness.
    /// </summary>
    NumaAware,

    /// <summary>
    /// Custom thread pool.
    /// </summary>
    Custom
}

/// <summary>
/// Task priority levels.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority.
    /// </summary>
    Low,

    /// <summary>
    /// Below normal priority.
    /// </summary>
    BelowNormal,

    /// <summary>
    /// Normal priority.
    /// </summary>
    Normal,

    /// <summary>
    /// Above normal priority.
    /// </summary>
    AboveNormal,

    /// <summary>
    /// High priority.
    /// </summary>
    High,

    /// <summary>
    /// Critical priority.
    /// </summary>
    Critical
}
