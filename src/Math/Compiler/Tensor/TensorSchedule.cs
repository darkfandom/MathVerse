namespace MathVerse.Math.Compiler.Tensor;

using System.Collections.Immutable;
using System.Collections.Generic;

/// <summary>Represents a scheduled sequence of tileable tensor operations produced by the tensor scheduler.</summary>
public sealed class TensorSchedule
{
    /// <summary>The ordered list of operations in the schedule.</summary>
    public IReadOnlyList<TileableOperation> Operations { get; }

    /// <summary>The total estimated FLOPs for this schedule.</summary>
    public double EstimatedFLOPs { get; }

    /// <summary>Initializes a new instance of the <see cref="TensorSchedule"/> class.</summary>
    public TensorSchedule(IEnumerable<TileableOperation> operations, double estimatedFLOPs = 0)
    {
        Operations = operations?.ToImmutableArray() ?? throw new System.ArgumentNullException(nameof(operations));
        EstimatedFLOPs = estimatedFLOPs;
    }
}
