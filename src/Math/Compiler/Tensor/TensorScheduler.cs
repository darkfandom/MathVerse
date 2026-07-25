namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Schedules tensor operations for optimal memory access patterns. Supports tiling, loop reordering, and register blocking.</summary>
public sealed class TensorScheduler
{
    private readonly int _defaultTileSize;

    /// <summary>Initializes a new instance of the <see cref="TensorScheduler"/> class.</summary>
    /// <param name="defaultTileSize">The default tile size for each dimension.</param>
    public TensorScheduler(int defaultTileSize = 32)
    {
        _defaultTileSize = defaultTileSize > 0 ? defaultTileSize : throw new ArgumentOutOfRangeException(nameof(defaultTileSize));
    }

    /// <summary>Produces a <see cref="TensorSchedule"/> from the given tensor graph.</summary>
    /// <param name="graph">The input tensor graph.</param>
    /// <returns>A scheduled sequence of tileable operations.</returns>
    public TensorSchedule Schedule(TensorGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetExecutionOrder();
        var scheduled = new List<TileableOperation>();
        double totalFlops = 0;

        for (var i = 0; i < order.Count; i++)
        {
            var expr = order[i];
            var tileSizes = ChooseTileSizes(expr);
            var loopOrder = ChooseLoopOrder(expr);
            var regBlock = ChooseRegisterBlock(expr);
            totalFlops += EstimateFlops(expr);

            scheduled.Add(new TileableOperation(
                expr.OpType,
                Enumerable.Range(0, expr.Inputs.Count),
                tileSizes,
                i,
                loopOrder,
                regBlock));
        }

        return new TensorSchedule(scheduled, totalFlops);
    }

    private IReadOnlyList<int> ChooseTileSizes(TensorExpression expr)
    {
        var sizes = new int[expr.Shape.Count];
        for (var i = 0; i < sizes.Length; i++)
            sizes[i] = Math.Min(_defaultTileSize, Math.Max(1, expr.Shape[i]));
        return sizes;
    }

    private static IReadOnlyList<int> ChooseLoopOrder(TensorExpression expr)
    {
        var indices = Enumerable.Range(0, expr.Shape.Count).ToList();
        indices.Sort((a, b) => expr.Shape[b].CompareTo(expr.Shape[a]));
        return indices;
    }

    private static int ChooseRegisterBlock(TensorExpression expr) =>
        expr.OpType == TensorOpType.MatMul ? 4 : 1;

    private static double EstimateFlops(TensorExpression expr)
    {
        var totalElements = 1;
        foreach (var d in expr.Shape)
            totalElements *= d;

        return expr.OpType switch
        {
            TensorOpType.MatMul => 2.0 * totalElements * Math.Sqrt(totalElements),
            TensorOpType.Add or TensorOpType.Sub => totalElements,
            TensorOpType.Mul or TensorOpType.Div => totalElements,
            TensorOpType.Exp or TensorOpType.Log or TensorOpType.Sqrt => 10.0 * totalElements,
            TensorOpType.Sum or TensorOpType.Mean => totalElements,
            TensorOpType.Max or TensorOpType.Min => totalElements,
            TensorOpType.Neg or TensorOpType.Pos or TensorOpType.Copy => totalElements,
            _ => totalElements
        };
    }
}
