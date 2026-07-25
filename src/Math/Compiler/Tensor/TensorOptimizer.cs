namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Optimizes tensor computation graphs. Fuses elementwise ops, reorders for cache locality.</summary>
public sealed class TensorOptimizer
{
    /// <summary>Optimizes the given tensor graph by fusing elementwise operations and reordering for better cache locality.</summary>
    /// <param name="graph">The input tensor graph to optimize.</param>
    /// <returns>A new optimized <see cref="TensorGraph"/>.</returns>
    public TensorGraph Optimize(TensorGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        var fused = graph.Optimize();
        var reordered = ReorderForCacheLocality(fused);
        return reordered;
    }

    /// <summary>Fuses adjacent elementwise operations into a single combined operation to reduce memory traffic.</summary>
    public TensorGraph FuseElementwise(TensorGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));
        return graph.Optimize();
    }

    private TensorGraph ReorderForCacheLocality(TensorGraph graph)
    {
        var order = graph.GetExecutionOrder();
        var reordered = new TensorGraph();
        var visited = new HashSet<TensorExpression>(ReferenceEqualityComparer.Instance);

        foreach (var op in order)
            VisitReorder(op, visited, reordered);

        return reordered;
    }

    private static void VisitReorder(TensorExpression op, HashSet<TensorExpression> visited, TensorGraph target)
    {
        if (!visited.Add(op)) return;
        foreach (var input in op.Inputs)
            VisitReorder(input, visited, target);
        target.AddOperation(op.OpType, op.Inputs, op.Shape, op.Label);
    }
}

/// <summary>Reference equality comparer for TensorExpression.</summary>
internal sealed class ReferenceEqualityComparer : IEqualityComparer<TensorExpression>
{
    public static readonly ReferenceEqualityComparer Instance = new();
    public bool Equals(TensorExpression? x, TensorExpression? y) => ReferenceEquals(x, y);
    public int GetHashCode(TensorExpression obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}
