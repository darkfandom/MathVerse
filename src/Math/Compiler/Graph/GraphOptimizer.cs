namespace MathVerse.Math.Compiler.Graph;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Optimizes computation graphs through dead node elimination, constant folding, and node fusion.</summary>
public sealed class GraphOptimizer
{
    /// <summary>Applies all optimization passes to the graph and returns a new optimized graph.</summary>
    public ComputationGraph Optimize(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var result = graph;
        result = EliminateDeadNodes(result);
        result = FoldConstants(result);
        result = FuseNodes(result);
        return result;
    }

    /// <summary>Removes nodes whose results are never consumed (dead code elimination).</summary>
    public ComputationGraph EliminateDeadNodes(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetTopologicalOrder();
        var used = new HashSet<int>();

        foreach (var node in graph.GetOutputNodes())
            used.Add(node.Id);

        for (int i = order.Count - 1; i >= 0; i--)
        {
            if (!graph.TryGetNode(order[i], out var node) || node is null) continue;
            if (used.Contains(node.Id))
            {
                foreach (int inputId in node.Inputs)
                    used.Add(inputId);
            }
        }

        var optimized = new ComputationGraph();
        var idMap = new Dictionary<int, int>();

        foreach (int nodeId in order)
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;
            if (!used.Contains(nodeId) && !node.IsInput) continue;

            int newId = optimized.AddNode(node.Operation, [], node.Metadata);
            idMap[nodeId] = newId;
        }

        foreach (var edge in graph.Edges)
        {
            if (idMap.TryGetValue(edge.From, out int newFrom) && idMap.TryGetValue(edge.To, out int newTo))
            {
                optimized.AddEdge(new GraphEdge(newFrom, newTo, edge.FromPort, edge.ToPort, edge.Weight));
            }
        }

        return optimized;
    }

    /// <summary>Folds constant expressions (evaluates operations on constant inputs at compile time).</summary>
    public ComputationGraph FoldConstants(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetTopologicalOrder();
        var constantValues = new Dictionary<int, double>();
        var optimized = new ComputationGraph();
        var idMap = new Dictionary<int, int>();

        foreach (int nodeId in order)
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;

            if (node.Operation == GraphOperation.Input || node.IsInput)
            {
                int newId = optimized.AddNode(node.Operation, [], node.Metadata);
                idMap[nodeId] = newId;
                continue;
            }

            bool allConstants = node.Inputs.Count > 0 &&
                node.Inputs.All(id => constantValues.ContainsKey(id) && idMap.ContainsKey(id));

            if (allConstants && IsFoldableOperation(node.Operation))
            {
                var inputVals = node.Inputs.Select(id => constantValues[id]).ToArray();
                double result = EvaluateConstant(node.Operation, inputVals);
                int newId = optimized.AddNode(GraphOperation.Input, [], node.Metadata);
                constantValues[nodeId] = result;
                idMap[nodeId] = newId;
            }
            else
            {
                var mappedInputs = node.Inputs.Where(id => idMap.ContainsKey(id)).Select(id => idMap[id]).ToArray();
                int newId = optimized.AddNode(node.Operation, mappedInputs, node.Metadata);
                idMap[nodeId] = newId;
            }
        }

        foreach (var edge in graph.Edges)
        {
            if (idMap.TryGetValue(edge.From, out int newFrom) && idMap.TryGetValue(edge.To, out int newTo))
                optimized.AddEdge(new GraphEdge(newFrom, newTo, edge.FromPort, edge.ToPort, edge.Weight));
        }

        return optimized;
    }

    /// <summary>Fuses adjacent element-wise operations into a single node.</summary>
    public ComputationGraph FuseNodes(ComputationGraph graph)
    {
        if (graph is null) throw new ArgumentNullException(nameof(graph));

        var order = graph.GetTopologicalOrder();
        var fused = new HashSet<int>();
        var idMap = new Dictionary<int, int>();
        var optimized = new ComputationGraph();

        foreach (int nodeId in order)
        {
            if (!graph.TryGetNode(nodeId, out var node) || node is null) continue;
            if (fused.Contains(nodeId)) continue;

            if (IsElementWiseOperation(node.Operation) && node.Inputs.Count == 2)
            {
                int inputA = node.Inputs[0];
                int inputB = node.Inputs[1];

                if (graph.TryGetNode(inputA, out var nodeA) && nodeA is not null &&
                    graph.TryGetNode(inputB, out var nodeB) && nodeB is not null &&
                    nodeA.Operation == nodeB.Operation &&
                    IsElementWiseOperation(nodeA.Operation) &&
                    !fused.Contains(inputA) && !fused.Contains(inputB))
                {
                    int newId = optimized.AddNode(node.Operation, [], node.Metadata);
                    idMap[nodeId] = newId;
                    fused.Add(inputA);
                    fused.Add(inputB);
                    continue;
                }
            }

            var mappedInputs = node.Inputs.Where(id => idMap.ContainsKey(id)).Select(id => idMap[id]).ToArray();
            int nid = optimized.AddNode(node.Operation, mappedInputs, node.Metadata);
            idMap[nodeId] = nid;
        }

        foreach (var edge in graph.Edges)
        {
            if (idMap.TryGetValue(edge.From, out int newFrom) && idMap.TryGetValue(edge.To, out int newTo))
                optimized.AddEdge(new GraphEdge(newFrom, newTo, edge.FromPort, edge.ToPort, edge.Weight));
        }

        return optimized;
    }

    private static bool IsFoldableOperation(GraphOperation op) =>
        op is GraphOperation.Add or GraphOperation.Sub or GraphOperation.Mul or
            GraphOperation.Div or GraphOperation.Pow or GraphOperation.Neg or
            GraphOperation.Exp or GraphOperation.Log or GraphOperation.Sqrt or GraphOperation.Abs;

    private static bool IsElementWiseOperation(GraphOperation op) =>
        op is GraphOperation.Add or GraphOperation.Sub or GraphOperation.Mul or GraphOperation.Div;

    private static double EvaluateConstant(GraphOperation op, double[] inputs)
    {
        return op switch
        {
            GraphOperation.Add => inputs.Length >= 2 ? inputs[0] + inputs[1] : inputs.Length > 0 ? inputs[0] : 0,
            GraphOperation.Sub => inputs.Length >= 2 ? inputs[0] - inputs[1] : inputs.Length > 0 ? inputs[0] : 0,
            GraphOperation.Mul => inputs.Length >= 2 ? inputs[0] * inputs[1] : inputs.Length > 0 ? inputs[0] : 0,
            GraphOperation.Div => inputs.Length >= 2 ? inputs[0] / inputs[1] : inputs.Length > 0 ? inputs[0] : 0,
            GraphOperation.Pow => inputs.Length >= 2 ? Math.Pow(inputs[0], inputs[1]) : inputs.Length > 0 ? inputs[0] : 0,
            GraphOperation.Neg => inputs.Length > 0 ? -inputs[0] : 0,
            GraphOperation.Exp => inputs.Length > 0 ? Math.Exp(inputs[0]) : 0,
            GraphOperation.Log => inputs.Length > 0 ? Math.Log(inputs[0]) : 0,
            GraphOperation.Sqrt => inputs.Length > 0 ? Math.Sqrt(inputs[0]) : 0,
            GraphOperation.Abs => inputs.Length > 0 ? Math.Abs(inputs[0]) : 0,
            _ => 0,
        };
    }
}
