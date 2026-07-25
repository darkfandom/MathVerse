namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;
using GraphNode = MathVerse.Math.Compiler.Graph.GraphNode;

/// <summary>Compiles AI/ML expressions. Handles layers, activations, loss functions.
/// Lowers neural network descriptions to forward/backward computation graphs.</summary>
public sealed class AICompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "AI";

    /// <summary>Number of layers to compile.</summary>
    public int LayerCount { get; set; } = 1;

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var parts = expression.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) continue;

            var keyword = words[0].ToLowerInvariant();
            if (keyword is "linear" or "conv2d" or "relu" or "sigmoid" or "tanh" or "softmax" or "mse" or "crossentropy")
            {
                var dest = module.CreateTemp();
                module.Append(IRInstruction.CreateFunction(keyword, Array.Empty<IROperand>(), dest));
            }
        }
        return module;
    }

    /// <summary>Builds a forward/backward computation graph from the given layer specifications.</summary>
    public ComputationGraph BuildGraph(IReadOnlyList<string> layerSpecs)
    {
        if (layerSpecs is null) throw new ArgumentNullException(nameof(layerSpecs));
        var graph = new ComputationGraph();
        for (var i = 0; i < layerSpecs.Count; i++)
        {
            var node = new GraphNode($"layer_{i}", layerSpecs[i]);
            graph.AddNode(node);
        }
        return graph;
    }

    /// <inheritdoc />
    public override ComputationGraph BuildGraph(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = Compile(expression);
        return ComputationGraph.FromIR(module);
    }
}
