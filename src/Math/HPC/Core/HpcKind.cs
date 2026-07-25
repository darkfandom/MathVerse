namespace MathVerse.Math.HPC.Core;

/// <summary>
/// Represents the kind of HPC operation being performed.
/// </summary>
public enum HpcKind
{
    /// <summary>
    /// No operation specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Execute operation.
    /// </summary>
    Execute = 1,

    /// <summary>
    /// Kernel optimization operation.
    /// </summary>
    Optimize = 2,

    /// <summary>
    /// SIMD vectorization operation.
    /// </summary>
    Vectorize = 3,

    /// <summary>
    /// Parallel execution planning operation.
    /// </summary>
    Parallelize = 4,

    /// <summary>
    /// Ahead-of-time compilation operation.
    /// </summary>
    Compile = 5,

    /// <summary>
    /// Graph execution operation.
    /// </summary>
    ExecuteGraph = 6,

    /// <summary>
    /// Numerical computation execution.
    /// </summary>
    RunNumerics = 7,

    /// <summary>
    /// Simulation execution.
    /// </summary>
    RunSimulation = 8,

    /// <summary>
    /// Geometry computation execution.
    /// </summary>
    RunGeometry = 9,

    /// <summary>
    /// Quantum computation execution.
    /// </summary>
    RunQuantum = 10,

    /// <summary>
    /// AI/ML computation execution.
    /// </summary>
    RunAI = 11,

    /// <summary>
    /// Task scheduling operation.
    /// </summary>
    Schedule = 12,

    /// <summary>
    /// Performance profiling operation.
    /// </summary>
    Profile = 13,

    /// <summary>
    /// Analysis operation.
    /// </summary>
    Analyze = 14,

    /// <summary>
    /// Auto-tuning operation.
    /// </summary>
    AutoTune = 15,

    /// <summary>
    /// Kernel fusion operation.
    /// </summary>
    FuseKernels = 16,

    /// <summary>
    /// Memory management operation.
    /// </summary>
    ManageMemory = 17,

    /// <summary>
    /// Cache management operation.
    /// </summary>
    ManageCache = 18,

/// <summary>
    /// Cache clearing operation.
    /// </summary>
    ClearCaches = 19,

    /// <summary>
    /// Distributed execution operation.
    /// </summary>
    Distribute = 20,

    /// <summary>
    /// Benchmarking operation.
    /// </summary>
    Benchmark = 21,

    /// <summary>
    /// Complexity analysis operation.
    /// </summary>
    AnalyzeComplexity = 22,
}
