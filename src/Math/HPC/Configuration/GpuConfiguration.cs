namespace MathVerse.Math.HPC.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for GPU computing.
/// </summary>
/// <param name="Enabled">Whether GPU computing is enabled.</param>
/// <param name="PreferredVendor">Preferred GPU vendor.</param>
/// <param name="PreferredDeviceId">Preferred GPU device ID.</param>
/// <param name="EnableCuda">Whether to enable CUDA.</param>
/// <param name="EnableOpenCL">Whether to enable OpenCL.</param>
/// <param name="EnableHip">Whether to enable HIP.</param>
/// <param name="EnableMetal">Whether to enable Metal.</param>
/// <param name="EnableVulkan">Whether to enable Vulkan compute.</param>
/// <param name="EnableWebGpu">Whether to enable WebGPU.</param>
/// <param name="EnableDirectML">Whether to enable DirectML.</param>
/// <param name="PreferredComputeApi">Preferred compute API.</param>
/// <param name="EnableMultiGpu">Whether to enable multi-GPU support.</param>
/// <param name="MaxGpuDevices">Maximum number of GPU devices to use.</param>
/// <param name="EnableGpuDirect">Whether to enable GPUDirect.</param>
/// <param name="EnablePeerToPeer">Whether to enable peer-to-peer GPU communication.</param>
/// <param name="EnableUnifiedMemory">Whether to enable unified memory.</param>
/// <param name="EnableZeroCopy">Whether to enable zero-copy memory.</param>
/// <param name="EnablePinnedMemory">Whether to enable pinned host memory.</param>
/// <param name="EnableAsyncCopy">Whether to enable asynchronous memory copy.</param>
/// <param name="EnableStreamPriorities">Whether to enable stream priorities.</param>
/// <param name="MaxStreams">Maximum number of CUDA streams.</param>
/// <param name="EnableGraphCapture">Whether to enable CUDA graph capture.</param>
/// <param name="EnableGraphLaunch">Whether to enable CUDA graph launch.</param>
/// <param name="EnableTensorCores">Whether to enable Tensor Cores.</param>
/// <param name="EnableSparseTensorCores">Whether to enable sparse Tensor Cores.</param>
/// <param name="PreferredPrecision">Preferred compute precision.</param>
/// <param name="EnableMixedPrecision">Whether to enable mixed precision.</param>
/// <param name="EnableTf32">Whether to enable TF32 precision.</param>
/// <param name="EnableFp8">Whether to enable FP8 precision.</param>
/// <param name="MaxSharedMemoryPerBlock">Maximum shared memory per block in bytes.</param>
/// <param name="MaxRegistersPerThread">Maximum registers per thread.</param>
/// <param name="MaxThreadsPerBlock">Maximum threads per block.</param>
/// <param name="MaxBlocksPerSm">Maximum blocks per SM.</param>
/// <param name="EnableOccupancyOptimization">Whether to enable occupancy optimization.</param>
/// <param name="TargetOccupancy">Target occupancy percentage (0-100).</param>
/// <param name="EnableKernelFusion">Whether to enable kernel fusion on GPU.</param>
/// <param name="EnablePersistentKernels">Whether to enable persistent kernels.</param>
/// <param name="EnableCooperativeGroups">Whether to enable cooperative groups.</param>
/// <param name="EnableClusterLaunch">Whether to enable cluster launch.</param>
/// <param name="EnableDistributedGpu">Whether to enable distributed GPU computing.</param>
/// <param name="EnableGpuDirectRdma">Whether to enable GPUDirect RDMA.</param>
/// <param name="EnableNvLink">Whether to enable NVLink.</param>
/// <param name="EnableFabricManager">Whether to enable Fabric Manager.</param>
/// <param name="ProfilingLevel">Profiling level.</param>
/// <param name="EnableProfiling">Whether to enable profiling.</param>
/// <param name="ProfileOutputPath">Profile output path.</param>
/// <param name="CustomKernels">Custom kernel implementations.</param>
/// <param name="KernelCachePath">Kernel cache path.</param>
/// <param name="EnableKernelCaching">Whether to enable kernel caching.</param>
/// <param name="MaxKernelCacheSize">Maximum kernel cache size in bytes.</param>
/// <param name="EnableJitCompilation">Whether to enable JIT compilation.</param>
/// <param name="JitCachePath">JIT cache path.</param>
/// <param name="EnablePtxxas">Whether to enable PTX assembly.</param>
/// <param name="CompilerOptions">Compiler options.</param>
/// <param name="DeviceProperties">Target device properties.</param>
public sealed record GpuConfiguration(
    bool Enabled = true,
    GpuVendor PreferredVendor = GpuVendor.Any,
    int PreferredDeviceId = -1,
    bool EnableCuda = true,
    bool EnableOpenCL = false,
    bool EnableHip = false,
    bool EnableMetal = false,
    bool EnableVulkan = false,
    bool EnableWebGpu = false,
    bool EnableDirectML = false,
    ComputeApi PreferredComputeApi = ComputeApi.Auto,
    bool EnableMultiGpu = true,
    int MaxGpuDevices = 8,
    bool EnableGpuDirect = true,
    bool EnablePeerToPeer = true,
    bool EnableUnifiedMemory = true,
    bool EnableZeroCopy = true,
    bool EnablePinnedMemory = true,
    bool EnableAsyncCopy = true,
    bool EnableStreamPriorities = true,
    int MaxStreams = 32,
    bool EnableGraphCapture = true,
    bool EnableGraphLaunch = true,
    bool EnableTensorCores = true,
    bool EnableSparseTensorCores = true,
    ComputePrecision PreferredPrecision = ComputePrecision.Fp32,
    bool EnableMixedPrecision = true,
    bool EnableTf32 = true,
    bool EnableFp8 = false,
    int MaxSharedMemoryPerBlock = 48 * 1024,
    int MaxRegistersPerThread = 255,
    int MaxThreadsPerBlock = 1024,
    int MaxBlocksPerSm = 32,
    bool EnableOccupancyOptimization = true,
    int TargetOccupancy = 100,
    bool EnableKernelFusion = true,
    bool EnablePersistentKernels = true,
    bool EnableCooperativeGroups = true,
    bool EnableClusterLaunch = false,
    bool EnableDistributedGpu = false,
    bool EnableGpuDirectRdma = false,
    bool EnableNvLink = true,
    bool EnableFabricManager = false,
    ProfilingLevel ProfilingLevel = ProfilingLevel.None,
    bool EnableProfiling = false,
    string? ProfileOutputPath = null,
    IDictionary<string, object>? CustomKernels = null,
    string? KernelCachePath = null,
    bool EnableKernelCaching = true,
    long MaxKernelCacheSize = 256 * 1024 * 1024,
    bool EnableJitCompilation = true,
    string? JitCachePath = null,
    bool EnablePtxxas = false,
    string[]? CompilerOptions = null,
    IDictionary<string, object>? DeviceProperties = null)
{
    /// <summary>
    /// Default GPU configuration.
    /// </summary>
    public static GpuConfiguration Default { get; } = new();

    /// <summary>
    /// GPU configuration optimized for NVIDIA CUDA.
    /// </summary>
    public static GpuConfiguration CudaOptimized { get; } = new(
        PreferredVendor: GpuVendor.Nvidia,
        EnableCuda: true,
        EnableOpenCL: false,
        EnableHip: false,
        PreferredComputeApi: ComputeApi.Cuda,
        EnableTensorCores: true,
        EnableMixedPrecision: true,
        EnableTf32: true,
        PreferredPrecision: ComputePrecision.Fp16,
        EnableGraphCapture: true,
        EnableGraphLaunch: true);

    /// <summary>
    /// GPU configuration optimized for AMD ROCm/HIP.
    /// </summary>
    public static GpuConfiguration HipOptimized { get; } = new(
        PreferredVendor: GpuVendor.Amd,
        EnableCuda: false,
        EnableHip: true,
        PreferredComputeApi: ComputeApi.Hip,
        EnableTensorCores: true,
        EnableMixedPrecision: true);

    /// <summary>
    /// GPU configuration optimized for Intel GPUs.
    /// </summary>
    public static GpuConfiguration IntelOptimized { get; } = new(
        PreferredVendor: GpuVendor.Intel,
        EnableOpenCL: true,
        EnableDirectML: true,
        PreferredComputeApi: ComputeApi.OpenCL,
        EnableTensorCores: false,
        EnableMixedPrecision: true);

    /// <summary>
    /// GPU configuration optimized for Apple Metal.
    /// </summary>
    public static GpuConfiguration MetalOptimized { get; } = new(
        PreferredVendor: GpuVendor.Apple,
        EnableMetal: true,
        PreferredComputeApi: ComputeApi.Metal,
        EnableUnifiedMemory: true,
        EnableZeroCopy: true);

    /// <summary>
    /// GPU configuration for multi-GPU setups.
    /// </summary>
    public static GpuConfiguration MultiGpu { get; } = new(
        EnableMultiGpu: true,
        MaxGpuDevices: 8,
        EnablePeerToPeer: true,
        EnableGpuDirect: true,
        EnableNvLink: true);

    /// <summary>
    /// GPU configuration optimized for inference.
    /// </summary>
    public static GpuConfiguration InferenceOptimized { get; } = new(
        PreferredPrecision: ComputePrecision.Fp16,
        EnableMixedPrecision: true,
        EnableTensorCores: true,
        EnableGraphCapture: true,
        EnableGraphLaunch: true,
        EnableKernelFusion: true,
        EnablePersistentKernels: true,
        ProfilingLevel: ProfilingLevel.Minimal);

    /// <summary>
    /// GPU configuration optimized for training.
    /// </summary>
    public static GpuConfiguration TrainingOptimized { get; } = new(
        PreferredPrecision: ComputePrecision.Fp32,
        EnableMixedPrecision: true,
        EnableTensorCores: true,
        EnableTf32: true,
        EnableGraphCapture: true,
        EnableProfiling: true,
        ProfilingLevel: ProfilingLevel.Detailed);

    /// <summary>
    /// GPU configuration with minimal features.
    /// </summary>
    public static GpuConfiguration Minimal { get; } = new(
        Enabled: true,
        EnableCuda: false,
        EnableOpenCL: true,
        PreferredComputeApi: ComputeApi.OpenCL,
        EnableTensorCores: false,
        EnableMixedPrecision: false,
        EnableGraphCapture: false,
        EnableKernelFusion: false,
        EnableProfiling: false);

    /// <summary>
    /// Creates a GPU configuration for a specific vendor.
    /// </summary>
    public static GpuConfiguration ForVendor(GpuVendor vendor) => new(PreferredVendor: vendor);

    /// <summary>
    /// Creates a GPU configuration for a specific compute API.
    /// </summary>
    public static GpuConfiguration ForApi(ComputeApi api) => new(PreferredComputeApi: api);

    /// <summary>
    /// Creates a GPU configuration with custom device properties.
    /// </summary>
    public static GpuConfiguration WithDeviceProperties(IDictionary<string, object> properties) =>
        new(DeviceProperties: properties);
}

/// <summary>
/// GPU vendors.
/// </summary>
public enum GpuVendor
{
    /// <summary>
    /// Any vendor.
    /// </summary>
    Any,

    /// <summary>
    /// NVIDIA.
    /// </summary>
    Nvidia,

    /// <summary>
    /// AMD.
    /// </summary>
    Amd,

    /// <summary>
    /// Intel.
    /// </summary>
    Intel,

    /// <summary>
    /// Apple.
    /// </summary>
    Apple,

    /// <summary>
    /// Qualcomm.
    /// </summary>
    Qualcomm,

    /// <summary>
    /// ARM.
    /// </summary>
    Arm
}

/// <summary>
/// Compute APIs.
/// </summary>
public enum ComputeApi
{
    /// <summary>
    /// Automatic API selection.
    /// </summary>
    Auto,

    /// <summary>
    /// NVIDIA CUDA.
    /// </summary>
    Cuda,

    /// <summary>
    /// AMD HIP.
    /// </summary>
    Hip,

    /// <summary>
    /// OpenCL.
    /// </summary>
    OpenCL,

    /// <summary>
    /// Apple Metal.
    /// </summary>
    Metal,

    /// <summary>
    /// Vulkan Compute.
    /// </summary>
    Vulkan,

    /// <summary>
    /// WebGPU.
    /// </summary>
    WebGpu,

    /// <summary>
    /// Microsoft DirectML.
    /// </summary>
    DirectML,

    /// <summary>
    /// Intel oneAPI Level Zero.
    /// </summary>
    LevelZero
}

/// <summary>
/// Compute precision modes.
/// </summary>
public enum ComputePrecision
{
    /// <summary>
    /// FP64 (double precision).
    /// </summary>
    Fp64,

    /// <summary>
    /// FP32 (single precision).
    /// </summary>
    Fp32,

    /// <summary>
    /// FP16 (half precision).
    /// </summary>
    Fp16,

    /// <summary>
    /// BF16 (bfloat16).
    /// </summary>
    Bf16,

    /// <summary>
    /// TF32 (TensorFloat-32).
    /// </summary>
    Tf32,

    /// <summary>
    /// FP8 (E4M3).
    /// </summary>
    Fp8E4M3,

    /// <summary>
    /// FP8 (E5M2).
    /// </summary>
    Fp8E5M2,

    /// <summary>
    /// Mixed precision (FP16/FP32).
    /// </summary>
    Mixed,

    /// <summary>
    /// Automatic precision selection.
    /// </summary>
    Auto
}

/// <summary>
/// Profiling levels.
/// </summary>
public enum ProfilingLevel
{
    /// <summary>
    /// No profiling.
    /// </summary>
    None,

    /// <summary>
    /// Minimal profiling.
    /// </summary>
    Minimal,

    /// <summary>
    /// Basic profiling.
    /// </summary>
    Basic,

    /// <summary>
    /// Detailed profiling.
    /// </summary>
    Detailed,

    /// <summary>
    /// Full profiling with all metrics.
    /// </summary>
    Full
}
