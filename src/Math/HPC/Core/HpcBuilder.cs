namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.Graph;

/// <summary>
/// Fluent builder for HPC requests.
/// </summary>
public sealed class HpcBuilder
{
    private HpcOptions? _options;
    private HpcContext? _context;
    private object? _kernel;
    private IReadOnlyList<object>? _kernels;
    private ComputationGraph? _graph;
    private object? _expression;
    private HpcKind _kind;
    private int _iterations = 1;

    /// <summary>
    /// Sets the HPC options.
    /// </summary>
    public HpcBuilder WithOptions(HpcOptions options)
    {
        _options = options;
        return this;
    }

    /// <summary>
    /// Sets the HPC context.
    /// </summary>
    public HpcBuilder WithContext(HpcContext context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// Sets the kernel to operate on.
    /// </summary>
    public HpcBuilder ForKernel(object kernel)
    {
        _kernel = kernel;
        return this;
    }

    /// <summary>
    /// Sets multiple kernels to operate on.
    /// </summary>
    public HpcBuilder ForKernels(IReadOnlyList<object> kernels)
    {
        _kernels = kernels;
        return this;
    }

    /// <summary>
    /// Sets the computation graph to execute.
    /// </summary>
    public HpcBuilder ForGraph(ComputationGraph graph)
    {
        _graph = graph;
        return this;
    }

    /// <summary>
    /// Sets the expression to evaluate.
    /// </summary>
    public HpcBuilder ForExpression(object expression)
    {
        _expression = expression;
        return this;
    }

    /// <summary>
    /// Sets the operation kind.
    /// </summary>
    public HpcBuilder WithKind(HpcKind kind)
    {
        _kind = kind;
        return this;
    }

    /// <summary>
    /// Sets the number of iterations for benchmarking.
    /// </summary>
    public HpcBuilder WithIterations(int iterations)
    {
        _iterations = Math.Max(1, iterations);
        return this;
    }

    /// <summary>
    /// Builds the HPC request.
    /// </summary>
    public HpcRequest Build()
    {
        return new HpcRequest(
            Options: _options ?? HpcOptions.Default,
            Context: _context,
            Kernel: _kernel,
            Kernels: _kernels ?? Array.Empty<object>(),
            Graph: _graph,
            Expression: _expression,
            Kind: _kind,
            Iterations: _iterations);
    }

    /// <summary>
    /// Creates a builder for kernel optimization.
    /// </summary>
    public static HpcBuilder Optimize() => new HpcBuilder().WithKind(HpcKind.Optimize);

    /// <summary>
    /// Creates a builder for SIMD vectorization.
    /// </summary>
    public static HpcBuilder Vectorize() => new HpcBuilder().WithKind(HpcKind.Vectorize);

    /// <summary>
    /// Creates a builder for parallel execution planning.
    /// </summary>
    public static HpcBuilder Parallelize() => new HpcBuilder().WithKind(HpcKind.Parallelize);

    /// <summary>
    /// Creates a builder for AOT compilation.
    /// </summary>
    public static HpcBuilder Compile() => new HpcBuilder().WithKind(HpcKind.Compile);

    /// <summary>
    /// Creates a builder for JIT execution.
    /// </summary>
    public static HpcBuilder Execute() => new HpcBuilder().WithKind(HpcKind.Execute);

    /// <summary>
    /// Creates a builder for graph execution.
    /// </summary>
    public static HpcBuilder ExecuteGraph() => new HpcBuilder().WithKind(HpcKind.ExecuteGraph);

    /// <summary>
    /// Creates a builder for numerical computation.
    /// </summary>
    public static HpcBuilder RunNumerics() => new HpcBuilder().WithKind(HpcKind.RunNumerics);

    /// <summary>
    /// Creates a builder for simulation.
    /// </summary>
    public static HpcBuilder RunSimulation() => new HpcBuilder().WithKind(HpcKind.RunSimulation);

    /// <summary>
    /// Creates a builder for geometry computation.
    /// </summary>
    public static HpcBuilder RunGeometry() => new HpcBuilder().WithKind(HpcKind.RunGeometry);

    /// <summary>
    /// Creates a builder for quantum computation.
    /// </summary>
    public static HpcBuilder RunQuantum() => new HpcBuilder().WithKind(HpcKind.RunQuantum);

    /// <summary>
    /// Creates a builder for AI/ML computation.
    /// </summary>
    public static HpcBuilder RunAI() => new HpcBuilder().WithKind(HpcKind.RunAI);

    /// <summary>
    /// Creates a builder for task scheduling.
    /// </summary>
    public static HpcBuilder Schedule() => new HpcBuilder().WithKind(HpcKind.Schedule);

    /// <summary>
    /// Creates a builder for profiling.
    /// </summary>
    public static HpcBuilder Profile() => new HpcBuilder().WithKind(HpcKind.Profile);

    /// <summary>
    /// Creates a builder for analysis.
    /// </summary>
    public static HpcBuilder Analyze() => new HpcBuilder().WithKind(HpcKind.AnalyzeComplexity);

    /// <summary>
    /// Creates a builder for auto-tuning.
    /// </summary>
    public static HpcBuilder AutoTune() => new HpcBuilder().WithKind(HpcKind.AutoTune);
}
