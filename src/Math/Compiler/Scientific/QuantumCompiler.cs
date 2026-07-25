namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Collections.Generic;
using System.Globalization;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;
using GraphNode = MathVerse.Math.Compiler.Graph.GraphNode;

/// <summary>Compiles quantum expressions. Handles gate sequences, measurement, state preparation.
/// Lowers quantum operations to linear algebra computation graphs.</summary>
public sealed class QuantumCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Quantum";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var gates = expression.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var gate in gates)
        {
            var trimmed = gate.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            var gateName = parts[0].ToUpperInvariant();
            var args = new List<IROperand>();
            for (var i = 1; i < parts.Length; i++)
            {
                var argDest = module.CreateTemp();
                if (double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    module.Append(IRInstruction.CreateLoadConst(val, argDest));
                }
                else
                {
                    if (!module.HasVariable(parts[i]))
                        module.DeclareVariable(parts[i]);
                    module.Append(IRInstruction.CreateLoadVar(parts[i], argDest));
                }
                args.Add(argDest);
            }

            var dest = module.CreateTemp();
            module.Append(IRInstruction.CreateFunction(gateName, args.ToArray(), dest));
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
