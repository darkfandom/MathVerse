namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Immutable;
using System.Collections.Generic;

/// <summary>Represents a compiled and optimized plan for executing a tensor expression.
/// The plan consists of a tensor graph, a schedule, and estimated execution cost.</summary>
public sealed class TensorPlan
{
    /// <summary>The tensor operation graph from which this plan was compiled.</summary>
    public TensorGraph Graph { get; }
    /// <summary>The scheduled sequence of tileable operations.</summary>
    public TensorSchedule Schedule { get; }
    /// <summary>Estimated total FLOPs for executing this plan.</summary>
    public double EstimatedFLOPs { get; }
    /// <summary>Estimated memory cost in bytes.</summary>
    public long EstimatedMemoryBytes { get; }

    /// <summary>Initializes a new instance of the <see cref="TensorPlan"/> class.</summary>
    public TensorPlan(TensorGraph graph, TensorSchedule schedule, double estimatedFLOPs = 0, long estimatedMemoryBytes = 0)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        EstimatedFLOPs = estimatedFLOPs;
        EstimatedMemoryBytes = estimatedMemoryBytes;
    }

    /// <summary>Returns a human-readable summary of the plan.</summary>
    public override string ToString()
    {
        return $"TensorPlan: {Graph.OperationCount} ops, {Schedule.Operations.Count} scheduled ops, " +
               $"{EstimatedFLOPs:G3} FLOPs, {EstimatedMemoryBytes} bytes";
    }
}
