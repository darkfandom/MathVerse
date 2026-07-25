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

/// <summary>Compiles data science pipelines. Handles transforms, aggregations, ML.
/// Lowers data pipelines to sequences of transform operations.</summary>
public sealed class DataScienceCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "DataScience";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var steps = expression.Split('|', StringSplitOptions.RemoveEmptyEntries);
        foreach (var step in steps)
        {
            var trimmed = step.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            var opName = parts[0].ToLowerInvariant();
            var dest = module.CreateTemp();
            if (opName is "filter" or "map" or "reduce" or "aggregate" or "sort" or "groupby" or "join")
            {
                module.Append(IRInstruction.CreateFunction(opName, Array.Empty<IROperand>(), dest));
            }
        }
        return module;
    }

    /// <inheritdoc />
    public override ComputationGraph BuildGraph(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = Compile(expression);
        return ComputationGraph.FromIR(module);
    }
}
