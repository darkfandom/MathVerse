namespace MathVerse.Math.HPC.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration for distributed computing.
/// </summary>
/// <param name="Enabled">Whether distributed computing is enabled.</param>
/// <param name="PreferredRuntime">Preferred distributed runtime.</param>
/// <param name="EnableMpi">Whether to enable MPI.</param>
/// <param name="EnableRpc">Whether to enable RPC.</param>
/// <param name="EnableGrpc">Whether to enable gRPC.</param>
/// <param name="EnableRay">Whether to enable Ray.</param>
/// <param name="EnableDask">Whether to enable Dask.</param>
/// <param name="EnableSpark">Whether to enable Spark.</param>
/// <param name="EnableFlink">Whether to enable Flink.</param>
/// <param name="EnableHorovod">Whether to enable Horovod.</param>
/// <param name="EnableNccl">Whether to enable NCCL.</param>
/// <param name="EnableGloo">Whether to enable Gloo.</param>
/// <param name="EnableMpiCollectives">Whether to enable MPI collectives.</param>
/// <param name="EnableNcclCollectives">Whether to enable NCCL collectives.</param>
/// <param name="EnableGlooCollectives">Whether to enable Gloo collectives.</param>
/// <param name="EnableRpcCommunication">Whether to enable RPC communication.</param>
/// <param name="EnableGrpcCommunication">Whether to enable gRPC communication.</param>
/// <param name="EnableZeroCopyNetwork">Whether to enable zero-copy networking.</param>
/// <param name="EnableRdma">Whether to enable RDMA.</param>
/// <param name="EnableGpuDirectRdma">Whether to enable GPUDirect RDMA.</param>
/// <param name="EnableSharedMemory">Whether to enable shared memory communication.</param>
/// <param name="MaxNodes">Maximum number of nodes.</param>
/// <param name="MaxWorkersPerNode">Maximum workers per node.</param>
/// <param name="EnableElasticScaling">Whether to enable elastic scaling.</param>
/// <param name="MinWorkers">Minimum number of workers.</param>
/// <param name="MaxWorkers">Maximum number of workers.</param>
/// <param name="ScaleUpThreshold">Scale-up threshold.</param>
/// <param name="ScaleDownThreshold">Scale-down threshold.</param>
/// <param name="EnableFaultTolerance">Whether to enable fault tolerance.</param>
/// <param name="EnableCheckpointing">Whether to enable checkpointing.</param>
/// <param name="CheckpointInterval">Checkpoint interval.</param>
/// <param name="CheckpointPath">Checkpoint path.</param>
/// <param name="EnableLineageTracking">Whether to enable lineage tracking.</param>
/// <param name="EnableSpeculativeExecution">Whether to enable speculative execution.</param>
/// <param name="EnableStragglerMitigation">Whether to enable straggler mitigation.</param>
/// <param name="EnableLoadBalancing">Whether to enable load balancing.</param>
/// <param name="LoadBalancingStrategy">Load balancing strategy.</param>
/// <param name="EnableDataLocality">Whether to enable data locality optimization.</param>
/// <param name="EnablePartitioning">Whether to enable data partitioning.</param>
/// <param name="PartitioningStrategy">Partitioning strategy.</param>
/// <param name="NumPartitions">Number of partitions.</param>
/// <param name="EnableShuffleOptimization">Whether to enable shuffle optimization.</param>
/// <param name="EnableBroadcastOptimization">Whether to enable broadcast optimization.</param>
/// <param name="EnableAllReduceOptimization">Whether to enable all-reduce optimization.</param>
/// <param name="EnablePipelineParallelism">Whether to enable pipeline parallelism.</param>
/// <param name="EnableTensorParallelism">Whether to enable tensor parallelism.</param>
/// <param name="EnableDataParallelism">Whether to enable data parallelism.</param>
/// <param name="EnableSequenceParallelism">Whether to enable sequence parallelism.</param>
/// <param name="EnableExpertParallelism">Whether to enable expert parallelism.</param>
/// <param name="TensorParallelSize">Tensor parallel size.</param>
/// <param name="PipelineParallelSize">Pipeline parallel size.</param>
/// <param name="DataParallelSize">Data parallel size.</param>
/// <param name="EnableZeroOptimization">Whether to enable ZeRO optimization.</param>
/// <param name="ZeroStage">ZeRO stage.</param>
/// <param name="EnableOffloading">Whether to enable CPU/NVMe offloading.</param>
/// <param name="OffloadDevice">Offload device.</param>
/// <param name="EnableGradientAccumulation">Whether to enable gradient accumulation.</param>
/// <param name="GradientAccumulationSteps">Gradient accumulation steps.</param>
/// <param name="EnableOverlapComputationCommunication">Whether to overlap computation and communication.</param>
/// <param name="CommunicationBackend">Communication backend.</param>
/// <param name="NetworkInterface">Network interface to use.</param>
/// <param name="EnableCompression">Whether to enable communication compression.</param>
/// <param name="CompressionAlgorithm">Compression algorithm.</param>
/// <param name="EnableQuantization">Whether to enable quantization.</param>
/// <param name="QuantizationBits">Quantization bits.</param>
/// <param name="ProfilingLevel">Profiling level.</param>
/// <param name="EnableProfiling">Whether to enable profiling.</param>
/// <param name="ProfileOutputPath">Profile output path.</param>
/// <param name="ClusterConfigPath">Cluster configuration path.</param>
/// <param name="CustomCommunicators">Custom communicators.</param>
/// <param name="SchedulerAddress">Scheduler address.</param>
/// <param name="WorkerAddresses">Worker addresses.</param>
/// <param name="EnableEncryption">Whether to enable encryption.</param>
/// <param name="EnableAuthentication">Whether to enable authentication.</param>
/// <param name="CertPath">Certificate path.</param>
/// <param name="KeyPath">Key path.</param>
public sealed record DistributedConfiguration(
    bool Enabled = false,
    DistributedRuntime PreferredRuntime = DistributedRuntime.Auto,
    bool EnableMpi = true,
    bool EnableRpc = true,
    bool EnableGrpc = true,
    bool EnableRay = false,
    bool EnableDask = false,
    bool EnableSpark = false,
    bool EnableFlink = false,
    bool EnableHorovod = false,
    bool EnableNccl = true,
    bool EnableGloo = true,
    bool EnableMpiCollectives = true,
    bool EnableNcclCollectives = true,
    bool EnableGlooCollectives = true,
    bool EnableRpcCommunication = true,
    bool EnableGrpcCommunication = true,
    bool EnableZeroCopyNetwork = true,
    bool EnableRdma = true,
    bool EnableGpuDirectRdma = true,
    bool EnableSharedMemory = true,
    int MaxNodes = 1024,
    int MaxWorkersPerNode = 8,
    bool EnableElasticScaling = false,
    int MinWorkers = 1,
    int MaxWorkers = 1024,
    double ScaleUpThreshold = 0.8,
    double ScaleDownThreshold = 0.3,
    bool EnableFaultTolerance = true,
    bool EnableCheckpointing = true,
    TimeSpan CheckpointInterval = default,
    string? CheckpointPath = null,
    bool EnableLineageTracking = false,
    bool EnableSpeculativeExecution = false,
    bool EnableStragglerMitigation = true,
    bool EnableLoadBalancing = true,
    DistributedLoadBalancingStrategy LoadBalancingStrategy = DistributedLoadBalancingStrategy.Adaptive,
    bool EnableDataLocality = true,
    bool EnablePartitioning = true,
    PartitioningStrategy PartitioningStrategy = PartitioningStrategy.Hash,
    int NumPartitions = 0,
    bool EnableShuffleOptimization = true,
    bool EnableBroadcastOptimization = true,
    bool EnableAllReduceOptimization = true,
    bool EnablePipelineParallelism = false,
    bool EnableTensorParallelism = false,
    bool EnableDataParallelism = true,
    bool EnableSequenceParallelism = false,
    bool EnableExpertParallelism = false,
    int TensorParallelSize = 1,
    int PipelineParallelSize = 1,
    int DataParallelSize = 1,
    bool EnableZeroOptimization = false,
    int ZeroStage = 0,
    bool EnableOffloading = false,
    OffloadDevice OffloadDevice = OffloadDevice.Cpu,
    bool EnableGradientAccumulation = false,
    int GradientAccumulationSteps = 1,
    bool EnableOverlapComputationCommunication = true,
    CommunicationBackend CommunicationBackend = CommunicationBackend.Auto,
    string? NetworkInterface = null,
    bool EnableCompression = false,
    CompressionAlgorithm CompressionAlgorithm = CompressionAlgorithm.None,
    bool EnableQuantization = false,
    int QuantizationBits = 8,
    ProfilingLevel ProfilingLevel = ProfilingLevel.None,
    bool EnableProfiling = false,
    string? ProfileOutputPath = null,
    string? ClusterConfigPath = null,
    IDictionary<string, object>? CustomCommunicators = null,
    string? SchedulerAddress = null,
    string[]? WorkerAddresses = null,
    bool EnableEncryption = false,
    bool EnableAuthentication = false,
    string? CertPath = null,
    string? KeyPath = null)
{
    /// <summary>
    /// Default distributed configuration (disabled).
    /// </summary>
    public static DistributedConfiguration Default { get; } = new();

    /// <summary>
    /// Distributed configuration for MPI-based clusters.
    /// </summary>
    public static DistributedConfiguration MpiCluster { get; } = new(
        Enabled: true,
        PreferredRuntime: DistributedRuntime.Mpi,
        EnableMpi: true,
        EnableNccl: true,
        EnableMpiCollectives: true,
        EnableNcclCollectives: true,
        EnableDataParallelism: true,
        CommunicationBackend: CommunicationBackend.Mpi);

    /// <summary>
    /// Distributed configuration for NCCL-based GPU clusters.
    /// </summary>
    public static DistributedConfiguration NcclCluster { get; } = new(
        Enabled: true,
        PreferredRuntime: DistributedRuntime.Nccl,
        EnableNccl: true,
        EnableNcclCollectives: true,
        EnableGpuDirectRdma: true,
        EnableDataParallelism: true,
        EnableTensorParallelism: true,
        EnableAllReduceOptimization: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for Ray clusters.
    /// </summary>
    public static DistributedConfiguration RayCluster { get; } = new(
        Enabled: true,
        PreferredRuntime: DistributedRuntime.Ray,
        EnableRay: true,
        EnableRpc: true,
        EnableGrpc: true,
        EnableDataParallelism: true,
        EnablePipelineParallelism: true,
        CommunicationBackend: CommunicationBackend.Grpc);

    /// <summary>
    /// Distributed configuration for Horovod.
    /// </summary>
    public static DistributedConfiguration HorovodCluster { get; } = new(
        Enabled: true,
        PreferredRuntime: DistributedRuntime.Horovod,
        EnableHorovod: true,
        EnableNccl: true,
        EnableNcclCollectives: true,
        EnableDataParallelism: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for DeepSpeed/ZeRO.
    /// </summary>
    public static DistributedConfiguration DeepSpeedZero { get; } = new(
        Enabled: true,
        PreferredRuntime: DistributedRuntime.DeepSpeed,
        EnableZeroOptimization: true,
        ZeroStage: 3,
        EnableOffloading: true,
        OffloadDevice: OffloadDevice.Cpu,
        EnableGradientAccumulation: true,
        EnableDataParallelism: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for tensor parallelism.
    /// </summary>
    public static DistributedConfiguration TensorParallel { get; } = new(
        Enabled: true,
        EnableTensorParallelism: true,
        TensorParallelSize: 4,
        EnableDataParallelism: true,
        DataParallelSize: 2,
        EnableNcclCollectives: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for pipeline parallelism.
    /// </summary>
    public static DistributedConfiguration PipelineParallel { get; } = new(
        Enabled: true,
        EnablePipelineParallelism: true,
        PipelineParallelSize: 4,
        EnableDataParallelism: true,
        DataParallelSize: 2,
        EnableOverlapComputationCommunication: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for 3D parallelism (tensor + pipeline + data).
    /// </summary>
    public static DistributedConfiguration ThreeDParallel { get; } = new(
        Enabled: true,
        EnableTensorParallelism: true,
        TensorParallelSize: 4,
        EnablePipelineParallelism: true,
        PipelineParallelSize: 4,
        EnableDataParallelism: true,
        DataParallelSize: 4,
        EnableNcclCollectives: true,
        CommunicationBackend: CommunicationBackend.Nccl);

    /// <summary>
    /// Distributed configuration for elastic scaling.
    /// </summary>
    public static DistributedConfiguration Elastic { get; } = new(
        Enabled: true,
        EnableElasticScaling: true,
        MinWorkers: 2,
        MaxWorkers: 128,
        ScaleUpThreshold: 0.8,
        ScaleDownThreshold: 0.3,
        EnableFaultTolerance: true,
        EnableCheckpointing: true,
        CheckpointInterval: TimeSpan.FromMinutes(10));

    /// <summary>
    /// Distributed configuration with minimal features.
    /// </summary>
    public static DistributedConfiguration Minimal { get; } = new(
        Enabled: true,
        EnableMpi: false,
        EnableRpc: false,
        EnableGrpc: true,
        PreferredRuntime: DistributedRuntime.Grpc,
        EnableDataParallelism: true,
        CommunicationBackend: CommunicationBackend.Grpc);

    /// <summary>
    /// Creates a distributed configuration for a specific runtime.
    /// </summary>
    public static DistributedConfiguration ForRuntime(DistributedRuntime runtime) =>
        new(Enabled: true, PreferredRuntime: runtime);

    /// <summary>
    /// Creates a distributed configuration with custom cluster configuration.
    /// </summary>
    public static DistributedConfiguration WithClusterConfig(string configPath) =>
        new(Enabled: true, ClusterConfigPath: configPath);
}

/// <summary>
/// Distributed runtimes.
/// </summary>
public enum DistributedRuntime
{
    /// <summary>
    /// Automatic runtime selection.
    /// </summary>
    Auto,

    /// <summary>
    /// MPI.
    /// </summary>
    Mpi,

    /// <summary>
    /// NCCL.
    /// </summary>
    Nccl,

    /// <summary>
    /// Gloo.
    /// </summary>
    Gloo,

    /// <summary>
    /// Ray.
    /// </summary>
    Ray,

    /// <summary>
    /// Dask.
    /// </summary>
    Dask,

    /// <summary>
    /// Spark.
    /// </summary>
    Spark,

    /// <summary>
    /// Flink.
    /// </summary>
    Flink,

    /// <summary>
    /// Horovod.
    /// </summary>
    Horovod,

    /// <summary>
    /// DeepSpeed.
    /// </summary>
    DeepSpeed,

    /// <summary>
    /// gRPC.
    /// </summary>
    Grpc,

    /// <summary>
    /// Custom runtime.
    /// </summary>
    Custom
}

/// <summary>
/// Distributed load balancing strategies.
/// </summary>
public enum DistributedLoadBalancingStrategy
{
    /// <summary>
    /// Automatic strategy.
    /// </summary>
    Auto,

    /// <summary>
    /// Round-robin.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Least loaded.
    /// </summary>
    LeastLoaded,

    /// <summary>
    /// Work stealing.
    /// </summary>
    WorkStealing,

    /// <summary>
    /// Adaptive.
    /// </summary>
    Adaptive,

    /// <summary>
    /// Data locality aware.
    /// </summary>
    DataLocalityAware,

    /// <summary>
    /// Cost-based.
    /// </summary>
    CostBased
}

/// <summary>
/// Partitioning strategies.
/// </summary>
public enum PartitioningStrategy
{
    /// <summary>
    /// Hash partitioning.
    /// </summary>
    Hash,

    /// <summary>
    /// Range partitioning.
    /// </summary>
    Range,

    /// <summary>
    /// Round-robin partitioning.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Custom partitioning.
    /// </summary>
    Custom
}

/// <summary>
/// Communication backends.
/// </summary>
public enum CommunicationBackend
{
    /// <summary>
    /// Automatic backend selection.
    /// </summary>
    Auto,

    /// <summary>
    /// MPI.
    /// </summary>
    Mpi,

    /// <summary>
    /// NCCL.
    /// </summary>
    Nccl,

    /// <summary>
    /// Gloo.
    /// </summary>
    Gloo,

    /// <summary>
    /// gRPC.
    /// </summary>
    Grpc,

    /// <summary>
    /// TCP.
    /// </summary>
    Tcp,

    /// <summary>
    /// UCX.
    /// </summary>
    Ucx,

    /// <summary>
    /// Libfabric.
    /// </summary>
    Libfabric,

    /// <summary>
    /// Custom backend.
    /// </summary>
    Custom
}

/// <summary>
/// Offload devices for ZeRO optimization.
/// </summary>
public enum OffloadDevice
{
    /// <summary>
    /// CPU offload.
    /// </summary>
    Cpu,

    /// <summary>
    /// NVMe offload.
    /// </summary>
    Nvme,

    /// <summary>
    /// GPU offload.
    /// </summary>
    Gpu
}

/// <summary>
/// Compression algorithms.
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// No compression.
    /// </summary>
    None,

    /// <summary>
    /// LZ4 compression.
    /// </summary>
    Lz4,

    /// <summary>
    /// ZSTD compression.
    /// </summary>
    Zstd,

    /// <summary>
    /// Snappy compression.
    /// </summary>
    Snappy,

    /// <summary>
    /// Gzip compression.
    /// </summary>
    Gzip,

    /// <summary>
    /// Custom compression.
    /// </summary>
    Custom
}
