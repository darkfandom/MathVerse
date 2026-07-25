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

/// <summary>Compiles visualization expressions. Handles plot commands, data transforms.
/// Lowers visualization specifications to data pipeline computation graphs.</summary>
public sealed class VisualizationCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Visualization";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var lines = expression.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            var plotType = parts[0].ToLowerInvariant();
            if (plotType is "plot" or "scatter" or "hist" or "bar" or "surface" or "contour")
            {
                var args = new List<IROperand>();
                for (var i = 1; i < parts.Length; i++)
                {
                    var argDest = module.CreateTemp();
                    if (double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                    {
                        module.Append(IRInstruction.CreateLoadConst(val, argDest));
                        args.Add(argDest);
                    }
                    else
                    {
                        if (!module.HasVariable(parts[i]))
                            module.DeclareVariable(parts[i]);
                        module.Append(IRInstruction.CreateLoadVar(parts[i], argDest));
                        args.Add(argDest);
                    }
                }
                var dest = module.CreateTemp();
                module.Append(IRInstruction.CreateFunction(plotType, args.ToArray(), dest));
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
