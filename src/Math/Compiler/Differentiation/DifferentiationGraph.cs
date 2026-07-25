namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Enumerates the kinds of nodes in the differentiation graph.</summary>
public enum DiffGraphNodeKind
{
    /// <summary>An input variable.</summary>
    Input,

    /// <summary>An addition operation.</summary>
    Add,

    /// <summary>A multiplication operation.</summary>
    Mul,

    /// <summary>A division operation.</summary>
    Div,

    /// <summary>A power operation.</summary>
    Pow,

    /// <summary>A negation operation.</summary>
    Neg,

    /// <summary>A mathematical function call.</summary>
    Function,

    /// <summary>A constant value.</summary>
    Constant,

    /// <summary>An output node.</summary>
    Output,
}

/// <summary>Represents a node in the differentiation computation graph.</summary>
/// <param name="Id">Unique node identifier.</param>
/// <param name="Kind">The kind of operation this node represents.</param>
/// <param name="Value">The forward-pass value at this node.</param>
/// <param name="Gradient">The backward-pass gradient accumulator.</param>
/// <param name="Inputs">IDs of input nodes.</param>
/// <param name="FunctionName">Name of the function (for Function nodes).</param>
/// <param name="ConstantValue">Value of the constant (for Constant nodes).</param>
/// <param name="VariableName">Name of the variable (for Input nodes).</param>
public sealed record DiffGraphNode(
    int Id,
    DiffGraphNodeKind Kind,
    double Value,
    double Gradient,
    IReadOnlyList<int> Inputs,
    string? FunctionName = null,
    double? ConstantValue = null,
    string? VariableName = null);

/// <summary>Tracks the computation graph for automatic differentiation.</summary>
public sealed class DifferentiationGraph
{
    private readonly Dictionary<int, DiffGraphNode> _nodes = new();
    private readonly List<(int From, int To)> _edges = [];
    private int _nextId;

    /// <summary>All nodes in the differentiation graph.</summary>
    public IReadOnlyDictionary<int, DiffGraphNode> Nodes => _nodes;

    /// <summary>All edges in the differentiation graph.</summary>
    public IReadOnlyList<(int From, int To)> Edges => _edges;

    /// <summary>The number of nodes.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Adds an input (variable) node and returns its ID.</summary>
    public int AddInput(string variableName, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Input, value, 0.0, [], VariableName: variableName);
        _nodes[id] = node;
        return id;
    }

    /// <summary>Adds a constant node and returns its ID.</summary>
    public int AddConstant(double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Constant, value, 0.0, [], ConstantValue: value);
        _nodes[id] = node;
        return id;
    }

    /// <summary>Adds an addition node and returns its ID.</summary>
    public int AddAdd(int leftId, int rightId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Add, value, 0.0, [leftId, rightId]);
        _nodes[id] = node;
        _edges.Add((leftId, id));
        _edges.Add((rightId, id));
        return id;
    }

    /// <summary>Adds a multiplication node and returns its ID.</summary>
    public int AddMul(int leftId, int rightId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Mul, value, 0.0, [leftId, rightId]);
        _nodes[id] = node;
        _edges.Add((leftId, id));
        _edges.Add((rightId, id));
        return id;
    }

    /// <summary>Adds a division node and returns its ID.</summary>
    public int AddDiv(int numeratorId, int denominatorId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Div, value, 0.0, [numeratorId, denominatorId]);
        _nodes[id] = node;
        _edges.Add((numeratorId, id));
        _edges.Add((denominatorId, id));
        return id;
    }

    /// <summary>Adds a power node and returns its ID.</summary>
    public int AddPow(int baseId, int exponentId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Pow, value, 0.0, [baseId, exponentId]);
        _nodes[id] = node;
        _edges.Add((baseId, id));
        _edges.Add((exponentId, id));
        return id;
    }

    /// <summary>Adds a negation node and returns its ID.</summary>
    public int AddNeg(int operandId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Neg, value, 0.0, [operandId]);
        _nodes[id] = node;
        _edges.Add((operandId, id));
        return id;
    }

    /// <summary>Adds a function node and returns its ID.</summary>
    public int AddFunction(string functionName, IReadOnlyList<int> argumentIds, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Function, value, 0.0, argumentIds, FunctionName: functionName);
        _nodes[id] = node;
        foreach (int argId in argumentIds)
            _edges.Add((argId, id));
        return id;
    }

    /// <summary>Adds an output node and returns its ID.</summary>
    public int AddOutput(int inputId, double value)
    {
        int id = _nextId++;
        var node = new DiffGraphNode(id, DiffGraphNodeKind.Output, value, 0.0, [inputId]);
        _nodes[id] = node;
        _edges.Add((inputId, id));
        return id;
    }

    /// <summary>Performs backpropagation through the graph to compute gradients.</summary>
    public void Backpropagate(int outputNodeId)
    {
        if (!_nodes.TryGetValue(outputNodeId, out var outputNode))
            throw new ArgumentException($"Node {outputNodeId} not found.", nameof(outputNodeId));

        _nodes[outputNodeId] = outputNode with { Gradient = 1.0 };

        var reversedTopo = GetReverseTopologicalOrder();

        foreach (int nodeId in reversedTopo)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) continue;
            if (node.Gradient == 0) continue;

            switch (node.Kind)
            {
                case DiffGraphNodeKind.Add:
                    PropagateAdd(node);
                    break;
                case DiffGraphNodeKind.Mul:
                    PropagateMul(node);
                    break;
                case DiffGraphNodeKind.Div:
                    PropagateDiv(node);
                    break;
                case DiffGraphNodeKind.Pow:
                    PropagatePow(node);
                    break;
                case DiffGraphNodeKind.Neg:
                    PropagateNeg(node);
                    break;
                case DiffGraphNodeKind.Function:
                    PropagateFunction(node);
                    break;
            }
        }
    }

    /// <summary>Gets the gradient of a node after backpropagation.</summary>
    public double GetGradient(int nodeId)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
            return node.Gradient;
        return 0;
    }

    /// <summary>Resets all gradients to zero.</summary>
    public void ResetGradients()
    {
        var keys = _nodes.Keys.ToArray();
        foreach (int key in keys)
        {
            if (_nodes.TryGetValue(key, out var node))
                _nodes[key] = node with { Gradient = 0.0 };
        }
    }

    /// <summary>Gets the input nodes.</summary>
    public IReadOnlyList<DiffGraphNode> GetInputNodes() =>
        _nodes.Values.Where(n => n.Kind == DiffGraphNodeKind.Input).ToArray();

    /// <summary>Gets the output nodes.</summary>
    public IReadOnlyList<DiffGraphNode> GetOutputNodes() =>
        _nodes.Values.Where(n => n.Kind == DiffGraphNodeKind.Output).ToArray();

    /// <summary>Clears the graph.</summary>
    public void Clear()
    {
        _nodes.Clear();
        _edges.Clear();
        _nextId = 0;
    }

    private IReadOnlyList<int> GetReverseTopologicalOrder()
    {
        var visited = new HashSet<int>();
        var order = new List<int>(_nodes.Count);

        foreach (int nodeId in _nodes.Keys)
            TopologicalVisit(nodeId, visited, order);

        order.Reverse();
        return order;
    }

    private void TopologicalVisit(int nodeId, HashSet<int> visited, List<int> order)
    {
        if (!visited.Add(nodeId)) return;
        if (!_nodes.TryGetValue(nodeId, out var node)) return;

        foreach (int inputId in node.Inputs)
            TopologicalVisit(inputId, visited, order);

        order.Add(nodeId);
    }

    private void PropagateAdd(DiffGraphNode node)
    {
        if (node.Inputs.Count < 2) return;
        AddGradient(node.Inputs[0], node.Gradient);
        AddGradient(node.Inputs[1], node.Gradient);
    }

    private void PropagateMul(DiffGraphNode node)
    {
        if (node.Inputs.Count < 2) return;
        if (_nodes.TryGetValue(node.Inputs[0], out var left) && _nodes.TryGetValue(node.Inputs[1], out var right))
        {
            AddGradient(node.Inputs[0], node.Gradient * right.Value);
            AddGradient(node.Inputs[1], node.Gradient * left.Value);
        }
    }

    private void PropagateDiv(DiffGraphNode node)
    {
        if (node.Inputs.Count < 2) return;
        if (_nodes.TryGetValue(node.Inputs[0], out var num) && _nodes.TryGetValue(node.Inputs[1], out var denom))
        {
            double denomSq = denom.Value * denom.Value;
            AddGradient(node.Inputs[0], node.Gradient / denom.Value);
            AddGradient(node.Inputs[1], -node.Gradient * num.Value / denomSq);
        }
    }

    private void PropagatePow(DiffGraphNode node)
    {
        if (node.Inputs.Count < 2) return;
        if (_nodes.TryGetValue(node.Inputs[0], out var baseNode) && _nodes.TryGetValue(node.Inputs[1], out var expNode))
        {
            if (baseNode.Value > 0)
            {
                AddGradient(node.Inputs[0], node.Gradient * expNode.Value * Math.Pow(baseNode.Value, expNode.Value - 1));
                AddGradient(node.Inputs[1], node.Gradient * Math.Pow(baseNode.Value, expNode.Value) * Math.Log(baseNode.Value));
            }
        }
    }

    private void PropagateNeg(DiffGraphNode node)
    {
        if (node.Inputs.Count < 1) return;
        AddGradient(node.Inputs[0], -node.Gradient);
    }

    private void PropagateFunction(DiffGraphNode node)
    {
        if (node.Inputs.Count < 1 || string.IsNullOrEmpty(node.FunctionName)) return;
        if (!_nodes.TryGetValue(node.Inputs[0], out var arg)) return;

        double localGrad = node.FunctionName.ToLowerInvariant() switch
        {
            "sin" => Math.Cos(arg.Value),
            "cos" => -Math.Sin(arg.Value),
            "tan" => 1.0 / (Math.Cos(arg.Value) * Math.Cos(arg.Value)),
            "exp" => Math.Exp(arg.Value),
            "ln" => 1.0 / arg.Value,
            "sqrt" => 1.0 / (2.0 * Math.Sqrt(arg.Value)),
            "asin" => 1.0 / Math.Sqrt(1.0 - arg.Value * arg.Value),
            "acos" => -1.0 / Math.Sqrt(1.0 - arg.Value * arg.Value),
            "atan" => 1.0 / (1.0 + arg.Value * arg.Value),
            "abs" => Math.Sign(arg.Value),
            "ceil" => 0,
            "floor" => 0,
            _ => 0,
        };

        AddGradient(node.Inputs[0], node.Gradient * localGrad);
    }

    private void AddGradient(int nodeId, double gradient)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
            _nodes[nodeId] = node with { Gradient = node.Gradient + gradient };
    }
}
