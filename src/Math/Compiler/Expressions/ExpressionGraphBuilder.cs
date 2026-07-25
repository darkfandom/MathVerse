namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.Graph;

/// <summary>Builds a ComputationGraph from an ExpressionNode AST.</summary>
public sealed class ExpressionGraphBuilder
{
    private ComputationGraph _graph = null!;
    private readonly Dictionary<string, int> _variableNodes = new(StringComparer.Ordinal);
    private readonly Dictionary<ExpressionNode, int> _nodeMap = new();

    /// <summary>Builds a computation graph from the given AST.</summary>
    public ComputationGraph Build(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _graph = new ComputationGraph();
        _variableNodes.Clear();
        _nodeMap.Clear();

        int outputId = BuildNode(root);

        _graph.AddNode(new GraphNode(
            _graph.NextNodeId(),
            GraphOperation.Output,
            [outputId],
            []));

        var result = _graph;
        _graph = null!;
        return result;
    }

    /// <summary>Builds a computation graph and returns both the graph and the output node ID.</summary>
    public (ComputationGraph Graph, int OutputNodeId) BuildWithOutput(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _graph = new ComputationGraph();
        _variableNodes.Clear();
        _nodeMap.Clear();

        int outputId = BuildNode(root);

        int outputNodeId = _graph.AddNode(GraphOperation.Output, [outputId]);
        _graph.AddEdge(new GraphEdge(outputId, outputNodeId));

        var result = _graph;
        _graph = null!;
        return (result, outputNodeId);
    }

    private int BuildNode(ExpressionNode node)
    {
        if (_nodeMap.TryGetValue(node, out int existingId))
            return existingId;

        int id = node switch
        {
            NumberNode num => BuildNumber(num),
            VariableNode var => BuildVariable(var),
            BinaryOpNode bin => BuildBinaryOp(bin),
            UnaryOpNode unary => BuildUnaryOp(unary),
            FunctionNode func => BuildFunction(func),
            _ => throw new ArgumentException($"Unknown node type: {node.GetType().Name}"),
        };

        _nodeMap[node] = id;
        return id;
    }

    private int BuildNumber(NumberNode node)
    {
        int id = _graph.AddNode(GraphOperation.Input);
        return id;
    }

    private int BuildVariable(VariableNode node)
    {
        if (_variableNodes.TryGetValue(node.Name, out int existingId))
            return existingId;

        int id = _graph.AddNode(GraphOperation.Input);
        _variableNodes[node.Name] = id;
        return id;
    }

    private int BuildBinaryOp(BinaryOpNode node)
    {
        int leftId = BuildNode(node.Left);
        int rightId = BuildNode(node.Right);

        GraphOperation op = node.Op switch
        {
            BinaryOperator.Add => GraphOperation.Add,
            BinaryOperator.Subtract => GraphOperation.Sub,
            BinaryOperator.Multiply => GraphOperation.Mul,
            BinaryOperator.Divide => GraphOperation.Div,
            BinaryOperator.Power => GraphOperation.Pow,
            _ => throw new ArgumentException($"Unknown binary operator: {node.Op}"),
        };

        int id = _graph.AddNode(op, [leftId, rightId]);
        _graph.AddEdge(new GraphEdge(leftId, id, 0, 0));
        _graph.AddEdge(new GraphEdge(rightId, id, 0, 1));
        return id;
    }

    private int BuildUnaryOp(UnaryOpNode node)
    {
        int operandId = BuildNode(node.Operand);

        GraphOperation op = node.Op switch
        {
            UnaryOperator.Negate => GraphOperation.Neg,
            UnaryOperator.Positive => GraphOperation.Add,
            _ => throw new ArgumentException($"Unknown unary operator: {node.Op}"),
        };

        int id = _graph.AddNode(op, [operandId]);
        _graph.AddEdge(new GraphEdge(operandId, id));
        return id;
    }

    private int BuildFunction(FunctionNode node)
    {
        var inputIds = new List<int>(node.Arguments.Count);
        foreach (var arg in node.Arguments)
            inputIds.Add(BuildNode(arg));

        GraphOperation op = node.FunctionName.ToLowerInvariant() switch
        {
            "sin" or "cos" or "tan" or "asin" or "acos" or "atan" => GraphOperation.Custom,
            "exp" => GraphOperation.Exp,
            "log" or "ln" => GraphOperation.Log,
            "sqrt" => GraphOperation.Sqrt,
            "abs" => GraphOperation.Abs,
            "ceil" or "floor" => GraphOperation.Custom,
            _ => GraphOperation.Custom,
        };

        var metadata = new Dictionary<string, object>
        {
            ["functionName"] = node.FunctionName,
        };

        int id = _graph.AddNode(op, inputIds, metadata);
        foreach (int inputId in inputIds)
            _graph.AddEdge(new GraphEdge(inputId, id));

        return id;
    }
}
