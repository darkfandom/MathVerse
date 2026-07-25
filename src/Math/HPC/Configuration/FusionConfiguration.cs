namespace MathVerse.Math.HPC.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for kernel fusion optimization.
/// </summary>
/// <param name="Enabled">Whether kernel fusion is enabled.</param>
/// <param name="EnableHorizontalFusion">Whether to enable horizontal fusion (same-level operations).</param>
/// <param name="EnableVerticalFusion">Whether to enable vertical fusion (producer-consumer chains).</param>
/// <param name="EnableLoopFusion">Whether to enable loop fusion.</param>
/// <param name="EnableKernelFusion">Whether to enable kernel fusion.</param>
/// <param name="EnableOperatorFusion">Whether to enable operator fusion.</param>
/// <param name="EnableMemoryFusion">Whether to enable memory access fusion.</param>
/// <param name="EnableComputeFusion">Whether to enable compute fusion.</param>
/// <param name="MaxFusionDepth">Maximum fusion depth.</param>
/// <param name="MaxFusedOperations">Maximum number of operations to fuse.</param>
/// <param name="EnableDependencyAnalysis">Whether to enable dependency analysis for fusion.</param>
/// <param name="EnableAliasingAnalysis">Whether to enable aliasing analysis.</param>
/// <param name="EnableSideEffectAnalysis">Whether to enable side-effect analysis.</param>
/// <param name="EnableControlFlowAnalysis">Whether to enable control flow analysis.</param>
/// <param name="EnableLoopAnalysis">Whether to enable loop analysis.</param>
/// <param name="FusionStrategy">Fusion strategy to use.</param>
/// <param name="EnableAggressiveFusion">Whether to enable aggressive fusion.</param>
/// <param name="EnableConservativeFusion">Whether to enable conservative fusion.</param>
/// <param name="EnableSpeculativeFusion">Whether to enable speculative fusion.</param>
/// <param name="EnableProfileGuidedFusion">Whether to enable profile-guided fusion.</param>
/// <param name="FusionThreshold">Minimum benefit threshold for fusion.</param>
/// <param name="MaxRegisterPressure">Maximum register pressure allowed.</param>
/// <param name="MaxSharedMemoryUsage">Maximum shared memory usage for fused kernels.</param>
/// <param name="EnableCrossKernelFusion">Whether to enable cross-kernel fusion.</param>
/// <param name="EnableCrossIterationFusion">Whether to enable cross-iteration fusion.</param>
/// <param name="FusionHeuristics">Fusion heuristics to apply.</param>
/// <param name="BlacklistedOperations">Operations that should never be fused.</param>
/// <param name="WhitelistedOperations">Operations that are always candidates for fusion.</param>
/// <param name="EnableFusionProfiling">Whether to enable fusion profiling.</param>
/// <param name="FusionProfilePath">Path to fusion profile data.</param>
public sealed record FusionConfiguration(
    bool Enabled = true,
    bool EnableHorizontalFusion = true,
    bool EnableVerticalFusion = true,
    bool EnableLoopFusion = true,
    bool EnableKernelFusion = true,
    bool EnableOperatorFusion = true,
    bool EnableMemoryFusion = true,
    bool EnableComputeFusion = true,
    int MaxFusionDepth = 10,
    int MaxFusedOperations = 32,
    bool EnableDependencyAnalysis = true,
    bool EnableAliasingAnalysis = true,
    bool EnableSideEffectAnalysis = true,
    bool EnableControlFlowAnalysis = true,
    bool EnableLoopAnalysis = true,
    FusionStrategy FusionStrategy = FusionStrategy.Balanced,
    bool EnableAggressiveFusion = false,
    bool EnableConservativeFusion = false,
    bool EnableSpeculativeFusion = false,
    bool EnableProfileGuidedFusion = false,
    double FusionThreshold = 1.2,
    int MaxRegisterPressure = 255,
    long MaxSharedMemoryUsage = 48 * 1024,
    bool EnableCrossKernelFusion = false,
    bool EnableCrossIterationFusion = false,
    FusionHeuristics FusionHeuristics = FusionHeuristics.Default,
    string[]? BlacklistedOperations = null,
    string[]? WhitelistedOperations = null,
    bool EnableFusionProfiling = false,
    string? FusionProfilePath = null)
{
    /// <summary>
    /// Default fusion configuration.
    /// </summary>
    public static FusionConfiguration Default { get; } = new();

    /// <summary>
    /// Aggressive fusion configuration for maximum fusion.
    /// </summary>
    public static FusionConfiguration Aggressive { get; } = new(
        EnableAggressiveFusion: true,
        MaxFusionDepth: 20,
        MaxFusedOperations: 64,
        FusionThreshold: 1.05,
        EnableCrossKernelFusion: true,
        EnableCrossIterationFusion: true);

    /// <summary>
    /// Conservative fusion configuration (safe fusion only).
    /// </summary>
    public static FusionConfiguration Conservative { get; } = new(
        EnableConservativeFusion: true,
        MaxFusionDepth: 5,
        MaxFusedOperations: 16,
        FusionThreshold: 1.5,
        FusionStrategy: FusionStrategy.Conservative);

    /// <summary>
    /// Fusion configuration optimized for GPU.
    /// </summary>
    public static FusionConfiguration GpuOptimized { get; } = new(
        EnableKernelFusion: true,
        EnableMemoryFusion: true,
        EnableComputeFusion: true,
        MaxFusionDepth: 16,
        MaxFusedOperations: 48,
        MaxSharedMemoryUsage: 163840,
        FusionStrategy: FusionStrategy.GpuOptimized);

    /// <summary>
    /// Fusion configuration optimized for CPU.
    /// </summary>
    public static FusionConfiguration CpuOptimized { get; } = new(
        EnableLoopFusion: true,
        EnableOperatorFusion: true,
        EnableHorizontalFusion: true,
        EnableVerticalFusion: true,
        MaxFusionDepth: 12,
        MaxFusedOperations: 32,
        MaxRegisterPressure: 128,
        FusionStrategy: FusionStrategy.CpuOptimized);

    /// <summary>
    /// Fusion configuration with profile-guided optimization.
    /// </summary>
    public static FusionConfiguration ProfileGuided { get; } = new(
        EnableProfileGuidedFusion: true,
        FusionProfilePath: "fusion_profile.json",
        FusionStrategy: FusionStrategy.ProfileGuided);

    /// <summary>
    /// Minimal fusion configuration (loop fusion only).
    /// </summary>
    public static FusionConfiguration Minimal { get; } = new(
        EnableHorizontalFusion: false,
        EnableVerticalFusion: false,
        EnableLoopFusion: true,
        EnableKernelFusion: false,
        EnableOperatorFusion: false,
        EnableMemoryFusion: false,
        EnableComputeFusion: false,
        MaxFusionDepth: 2,
        MaxFusedOperations: 4,
        FusionStrategy: FusionStrategy.Minimal);

    /// <summary>
    /// Fusion configuration with custom blacklist/whitelist.
    /// </summary>
    public static FusionConfiguration WithFilter(string[]? blacklist, string[]? whitelist) =>
        new(BlacklistedOperations: blacklist, WhitelistedOperations: whitelist);

    /// <summary>
    /// Creates a fusion configuration with custom heuristics.
    /// </summary>
    public static FusionConfiguration WithHeuristics(FusionHeuristics heuristics) =>
        new(FusionHeuristics: heuristics);
}

/// <summary>
/// Fusion strategies.
/// </summary>
public enum FusionStrategy
{
    /// <summary>
    /// Automatic strategy selection.
    /// </summary>
    Auto,

    /// <summary>
    /// Balanced fusion strategy.
    /// </summary>
    Balanced,

    /// <summary>
    /// Aggressive fusion strategy.
    /// </summary>
    Aggressive,

    /// <summary>
    /// Conservative fusion strategy.
    /// </summary>
    Conservative,

    /// <summary>
    /// GPU-optimized fusion strategy.
    /// </summary>
    GpuOptimized,

    /// <summary>
    /// CPU-optimized fusion strategy.
    /// </summary>
    CpuOptimized,

    /// <summary>
    /// Profile-guided fusion strategy.
    /// </summary>
    ProfileGuided,

    /// <summary>
    /// Minimal fusion (loop fusion only).
    /// </summary>
    Minimal,

    /// <summary>
    /// Speculative fusion strategy.
    /// </summary>
    Speculative
}

/// <summary>
/// Fusion heuristics.
/// </summary>
[Flags]
public enum FusionHeuristics
{
    /// <summary>
    /// No heuristics.
    /// </summary>
    None = 0,

    /// <summary>
    /// Default heuristics.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Fuse adjacent operations.
    /// </summary>
    FuseAdjacent = 2,

    /// <summary>
    /// Fuse producer-consumer chains.
    /// </summary>
    FuseProducerConsumer = 4,

    /// <summary>
    /// Fuse loops with same iteration space.
    /// </summary>
    FuseSameIterationSpace = 8,

    /// <summary>
    /// Fuse memory accesses.
    /// </summary>
    FuseMemoryAccess = 16,

    /// <summary>
    /// Fuse compute operations.
    /// </summary>
    FuseCompute = 32,

    /// <summary>
    /// Fuse element-wise operations.
    /// </summary>
    FuseElementWise = 64,

    /// <summary>
    /// Fuse reduction operations.
    /// </summary>
    FuseReduction = 128,

    /// <summary>
    /// Fuse broadcast operations.
    /// </summary>
    FuseBroadcast = 256,

    /// <summary>
    /// Fuse transpose operations.
    /// </summary>
    FuseTranspose = 512,

    /// <summary>
    /// All heuristics enabled.
    /// </summary>
    All = FuseAdjacent | FuseProducerConsumer | FuseSameIterationSpace |
          FuseMemoryAccess | FuseCompute | FuseElementWise | FuseReduction |
          FuseBroadcast | FuseTranspose
}
