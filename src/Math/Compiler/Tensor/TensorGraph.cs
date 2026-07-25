namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Graph representation of tensor operations. Nodes are tensor ops, edges are tensor data flow.</summary>
public sealed class TensorGraph
{
    private readonly List<TensorExpression> _operations = new();
    private readonly object _lock = new();

    /// <summary>The number of operations in the graph.</summary>
    public int OperationCount
    {
        get { lock (_lock) { return _operations.Count; } }
    }

    /// <summary>All operations in the graph.</summary>
    public IReadOnlyList<TensorExpression> Operations
    {
        get { lock (_lock) { return _operations.ToList(); } }
    }

    /// <summary>Adds a tensor operation to the graph.</summary>
    /// <param name="opType">The type of operation.</param>
    /// <param name="inputs">The input expression nodes.</param>
    /// <param name="shape">The output shape.</param>
    /// <param name="label">Optional label.</param>
    /// <returns>The newly created tensor expression.</returns>
    public TensorExpression AddOperation(TensorOpType opType, IReadOnlyList<TensorExpression> inputs, IReadOnlyList<int> shape, string? label = null)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));
        if (shape is null) throw new ArgumentNullException(nameof(shape));
        var expr = new TensorExpression(opType, inputs, shape, label);
        lock (_lock)
        {
            _operations.Add(expr);
        }
        return expr;
    }

    /// <summary>Computes a topological execution order of the graph.</summary>
    /// <returns>Operations in topological order.</returns>
    public IReadOnlyList<TensorExpression> GetExecutionOrder()
    {
        lock (_lock)
        {
            var visited = new HashSet<TensorExpression>();
            var order = new List<TensorExpression>();
            foreach (var op in _operations)
                VisitTopological(op, visited, order);
            return order;
        }
    }

    /// <summary>Performs basic optimizations: fuses adjacent elementwise ops with no other consumers.</summary>
    /// <returns>A new optimized <see cref="TensorGraph"/>.</returns>
    public TensorGraph Optimize()
    {
        lock (_lock)
        {
            var optimized = new TensorGraph();
            var consumerCount = new Dictionary<TensorExpression, int>();
            foreach (var op in _operations)
            {
                if (!consumerCount.ContainsKey(op))
                    consumerCount[op] = 0;
                foreach (var input in op.Inputs)
                {
                    consumerCount.TryGetValue(input, out var count);
                    consumerCount[input] = count + 1;
                }
            }

            foreach (var op in GetExecutionOrder())
            {
                if (op.Inputs.Count == 1 && IsElementwise(op.OpType) && IsElementwise(op.Inputs[0].OpType))
                {
                    var parent = op.Inputs[0];
                    if (consumerCount.TryGetValue(parent, out var c) && c == 1)
                    {
                        consumerCount.TryGetValue(parent, out var parentC);
                        foreach (var input in parent.Inputs)
                        {
                            if (consumerCount.ContainsKey(input))
                                consumerCount[input] = consumerCount[input] - 1 + 1;
                        }
                        var fused = new TensorExpression(op.OpType, parent.Inputs, op.Shape, $"fused({parent.Label},{op.Label})");
                        optimized._operations.Add(fused);
                        continue;
                    }
                }
                optimized._operations.Add(op);
            }
            return optimized;
        }
    }

    private static bool IsElementwise(TensorOpType op) => op switch
    {
        TensorOpType.Add or TensorOpType.Sub or TensorOpType.Mul or TensorOpType.Div
            or TensorOpType.Neg or TensorOpType.Pos or TensorOpType.Exp
            or TensorOpType.Log or TensorOpType.Sqrt or TensorOpType.Copy => true,
        _ => false
    };

    private static void VisitTopological(TensorExpression op, HashSet<TensorExpression> visited, List<TensorExpression> order)
    {
        if (!visited.Add(op)) return;
        foreach (var input in op.Inputs)
            VisitTopological(input, visited, order);
        order.Add(op);
    }
}
