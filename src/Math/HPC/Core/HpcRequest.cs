namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using MathVerse.Math.Compiler.Graph;

/// <summary>
/// Represents a request to the HPC engine.
/// </summary>
/// <param name="Options">The HPC options for this request.</param>
/// <param name="Context">The HPC context (optional).</param>
/// <param name="Kernel">The kernel to operate on (optional).</param>
/// <param name="Kernels">Multiple kernels (for fusion, etc.).</param>
/// <param name="Graph">The computation graph (optional).</param>
/// <param name="Expression">The expression to evaluate (optional).</param>
/// <param name="Kind">The kind of HPC operation.</param>
/// <param name="Iterations">Number of iterations for benchmarking.</param>
public sealed record HpcRequest(
    HpcOptions Options,
    HpcContext? Context,
    object? Kernel,
    IReadOnlyList<object> Kernels,
    ComputationGraph? Graph,
    object? Expression,
    HpcKind Kind,
    int Iterations)
{
    /// <summary>
    /// Gets the unique request identifier.
    /// </summary>
    public Guid RequestId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the session identifier for this request.
    /// </summary>
    public Guid SessionId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a copy with modified options.
    /// </summary>
    public HpcRequest WithOptions(HpcOptions options) => this with { Options = options };

    /// <summary>
    /// Creates a copy with a different context.
    /// </summary>
    public HpcRequest WithContext(HpcContext context) => this with { Context = context };

    /// <summary>
    /// Creates a copy with a different kernel.
    /// </summary>
    public HpcRequest WithKernel(object kernel) => this with { Kernel = kernel };

    /// <summary>
    /// Creates a copy with additional kernels.
    /// </summary>
    public HpcRequest WithKernels(IReadOnlyList<object> kernels) => this with { Kernels = kernels };

/// <summary>
    /// Creates a copy with a different graph.
    /// </summary>
    public HpcRequest WithGraph(ComputationGraph graph) => this with { Graph = graph };

    /// <summary>
    /// Creates a copy with a different expression.
    /// </summary>
    public HpcRequest WithExpression(object expression) => this with { Expression = expression };

    /// <summary>
    /// Creates a copy with a different kind.
    /// </summary>
    public HpcRequest WithKind(HpcKind kind) => this with { Kind = kind };

    /// <summary>
    /// Creates a copy with different iterations.
    /// </summary>
    public HpcRequest WithIterations(int iterations) => this with { Iterations = Math.Max(1, iterations) };
}
