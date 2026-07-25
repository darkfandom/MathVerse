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

/// <summary>Compiles interoperability expressions. Handles format conversion, protocol bridging.
/// Lowers interop specs to conversion pipeline computation graphs.</summary>
public sealed class InteropCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Interop";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var parts = expression.Split("->", StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var format = parts[i].Trim().Trim('"');
            if (string.IsNullOrEmpty(format)) continue;
            var dest = module.CreateTemp();
            module.Append(IRInstruction.CreateFunction("convert", new[] { IROperand.CreateConstant(i) }, dest));
            if (!module.HasVariable(format))
                module.DeclareVariable(format);
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
