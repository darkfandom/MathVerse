namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using MathVerse.Math.HPC.Diagnostics;

/// <summary>
/// Result of an HPC operation.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="OptimizedKernel">The optimized kernel, if applicable.</param>
/// <param name="VectorizedCode">The vectorized code, if applicable.</param>
/// <param name="ParallelPlan">The parallel execution plan, if applicable.</param>
/// <param name="CompiledModule">The compiled module, if applicable.</param>
/// <param name="ExecutionResult">The execution result, if applicable.</param>
/// <param name="Diagnostics">Diagnostic messages from the operation.</param>
/// <param name="Duration">Time taken for the operation.</param>
/// <param name="Kind">The kind of HPC operation performed.</param>
public sealed record HpcResult(
    bool Success,
    IOptimizedKernel? OptimizedKernel,
    IVectorizedCode? VectorizedCode,
    IParallelPlan? ParallelPlan,
    ICompiledModule? CompiledModule,
    IExecutionResult? ExecutionResult,
    IReadOnlyList<DiagnosticMessage> Diagnostics,
    TimeSpan Duration,
    HpcKind Kind
)
{
    /// <summary>
    /// Creates a successful result with no outputs.
    /// </summary>
    public static HpcResult SuccessResult(HpcKind kind, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, null, null, null, null, null, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, kind);

    /// <summary>
    /// Creates a successful result with an optimized kernel.
    /// </summary>
    public static HpcResult Optimized(IOptimizedKernel kernel, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, kernel, null, null, null, null, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, HpcKind.Optimize);

    /// <summary>
    /// Creates a successful result with vectorized code.
    /// </summary>
    public static HpcResult Vectorized(IVectorizedCode code, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, null, code, null, null, null, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, HpcKind.Vectorize);

    /// <summary>
    /// Creates a successful result with a parallel plan.
    /// </summary>
    public static HpcResult Parallelized(IParallelPlan plan, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, null, null, plan, null, null, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, HpcKind.Parallelize);

    /// <summary>
    /// Creates a successful result with a compiled module.
    /// </summary>
    public static HpcResult Compiled(ICompiledModule module, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, null, null, null, module, null, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, HpcKind.Compile);

    /// <summary>
    /// Creates a successful result with an execution result.
    /// </summary>
    public static HpcResult Executed(IExecutionResult result, TimeSpan duration, IEnumerable<DiagnosticMessage>? diagnostics = null)
        => new(true, null, null, null, null, result, diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticMessage>.Empty, duration, HpcKind.Execute);

    /// <summary>
    /// Creates a failed result with diagnostics.
    /// </summary>
    public static HpcResult Failure(HpcKind kind, TimeSpan duration, IEnumerable<DiagnosticMessage> diagnostics)
        => new(false, null, null, null, null, null, diagnostics.ToImmutableArray(), duration, kind);

    /// <summary>
    /// Creates a failed result with a single diagnostic.
    /// </summary>
    public static HpcResult Failure(HpcKind kind, TimeSpan duration, DiagnosticMessage diagnostic)
        => new(false, null, null, null, null, null, ImmutableArray.Create(diagnostic), duration, kind);

    /// <summary>
    /// Gets whether the result has any diagnostics.
    /// </summary>
    public bool HasDiagnostics => Diagnostics.Count > 0;

    /// <summary>
    /// Gets whether the result has errors.
    /// </summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets whether the result has warnings.
    /// </summary>
    public bool HasWarnings => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets all error diagnostics.
    /// </summary>
    public IEnumerable<DiagnosticMessage> Errors => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets all warning diagnostics.
    /// </summary>
    public IEnumerable<DiagnosticMessage> Warnings => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets all info diagnostics.
    /// </summary>
    public IEnumerable<DiagnosticMessage> Infos => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info);
}

/// <summary>
/// Interface for an optimized kernel.
/// </summary>
public interface IOptimizedKernel
{
    /// <summary>
    /// Gets the unique identifier for this kernel.
    /// </summary>
    string KernelId { get; }

    /// <summary>
    /// Gets the original kernel IR.
    /// </summary>
    object OriginalKernel { get; }

    /// <summary>
    /// Gets the optimized kernel IR.
    /// </summary>
    object OptimizedKernelIR { get; }

    /// <summary>
    /// Gets the optimization passes applied.
    /// </summary>
    IReadOnlyList<string> AppliedPasses { get; }

    /// <summary>
    /// Gets the estimated speedup factor.
    /// </summary>
    double EstimatedSpeedup { get; }

    /// <summary>
    /// Gets the optimization metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Interface for vectorized code.
/// </summary>
public interface IVectorizedCode
{
    /// <summary>
    /// Gets the unique identifier for this vectorization.
    /// </summary>
    string VectorizationId { get; }

    /// <summary>
    /// Gets the original scalar code.
    /// </summary>
    object OriginalCode { get; }

    /// <summary>
    /// Gets the vectorized code.
    /// </summary>
    object VectorizedCodeIR { get; }

    /// <summary>
    /// Gets the vector width used.
    /// </summary>
    int VectorWidth { get; }

    /// <summary>
    /// Gets the SIMD instruction set targeted.
    /// </summary>
    string InstructionSet { get; }

    /// <summary>
    /// Gets the estimated speedup factor.
    /// </summary>
    double EstimatedSpeedup { get; }

    /// <summary>
    /// Gets the vectorization metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Interface for a parallel execution plan.
/// </summary>
public interface IParallelPlan
{
    /// <summary>
    /// Gets the unique identifier for this plan.
    /// </summary>
    string PlanId { get; }

    /// <summary>
    /// Gets the task graph representing parallel execution.
    /// </summary>
    object TaskGraph { get; }

    /// <summary>
    /// Gets the number of threads/workers to use.
    /// </summary>
    int ThreadCount { get; }

    /// <summary>
    /// Gets the scheduling strategy.
    /// </summary>
    string SchedulingStrategy { get; }

    /// <summary>
    /// Gets the estimated parallelism.
    /// </summary>
    double EstimatedParallelism { get; }

    /// <summary>
    /// Gets the load balance metric (0.0 to 1.0).
    /// </summary>
    double LoadBalance { get; }

    /// <summary>
    /// Gets the plan metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Interface for a compiled module.
/// </summary>
public interface ICompiledModule
{
    /// <summary>
    /// Gets the unique identifier for this module.
    /// </summary>
    string ModuleId { get; }

    /// <summary>
    /// Gets the target platform.
    /// </summary>
    string TargetPlatform { get; }

    /// <summary>
    /// Gets the compiled binary/assembly.
    /// </summary>
    object CompiledArtifact { get; }

    /// <summary>
    /// Gets the entry points.
    /// </summary>
    IReadOnlyList<string> EntryPoints { get; }

    /// <summary>
    /// Gets the compilation metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Interface for an execution result.
/// </summary>
public interface IExecutionResult
{
    /// <summary>
    /// Gets the unique identifier for this execution.
    /// </summary>
    string ExecutionId { get; }

    /// <summary>
    /// Gets the return value.
    /// </summary>
    object? ReturnValue { get; }

    /// <summary>
    /// Gets the execution time.
    /// </summary>
    TimeSpan ExecutionTime { get; }

    /// <summary>
    /// Gets the peak memory usage.
    /// </summary>
    long PeakMemoryBytes { get; }

    /// <summary>
    /// Gets the execution metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}
