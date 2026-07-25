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

/// <summary>Compiles distributed computation expressions. Handles partitioning, communication.
/// Lowers distributed specifications to local computation + communication plan.</summary>
public sealed class DistributedCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Distributed";

    /// <summary>Number of workers/partitions available.</summary>
    public int WorkerCount { get; set; } = 2;

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var tok = parts[i].ToLowerInvariant();
            if (tok is "partition" or "broadcast" or "reduce" or "gather" or "scatter" or "allreduce")
            {
                var dest = module.CreateTemp();
                if (i + 1 < parts.Length && double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    var argDest = module.CreateTemp();
                    module.Append(IRInstruction.CreateLoadConst(double.Parse(parts[++i], CultureInfo.InvariantCulture), argDest));
                    module.Append(IRInstruction.CreateFunction(tok, new[] { argDest }, dest));
                }
                else
                {
                    module.Append(IRInstruction.CreateFunction(tok, Array.Empty<IROperand>(), dest));
                }
            }
        }
        return module;
    }

    /// <summary>Builds a communication plan graph from the distributed spec.</summary>
    public ComputationGraph BuildCommunicationPlan(string expression)
    {
        return BuildGraph(expression);
    }

    /// <inheritdoc />
    public override ComputationGraph BuildGraph(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = Compile(expression);
        return ComputationGraph.FromIR(module);
    }
}
