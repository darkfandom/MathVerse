namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;
using GraphNode = MathVerse.Math.Compiler.Graph.GraphNode;

/// <summary>Compiles geometric expressions. Handles transformations, intersections, projections.
/// Lowers geometric operations to matrix/vector computation graphs.</summary>
public sealed class GeometryCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Geometry";

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var tok = parts[i];
            var dest = module.CreateTemp();
            if (double.TryParse(tok, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                module.Append(IRInstruction.CreateLoadConst(val, dest));
            else if (tok is "translate" or "rotate" or "scale" or "project")
            {
                var args = new List<IROperand>();
                for (var j = i + 1; j < parts.Length && double.TryParse(parts[j], NumberStyles.Float, CultureInfo.InvariantCulture, out _); j++)
                {
                    var argDest = module.CreateTemp();
                    module.Append(IRInstruction.CreateLoadConst(double.Parse(parts[j], CultureInfo.InvariantCulture), argDest));
                    args.Add(argDest);
                }
                module.Append(IRInstruction.CreateFunction(tok, args.ToArray(), dest));
            }
            else if (char.IsLetter(tok[0]))
            {
                if (!module.HasVariable(tok))
                    module.DeclareVariable(tok);
                module.Append(IRInstruction.CreateLoadVar(tok, dest));
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

    /// <summary>Builds a matrix transformation computation graph for the given 4x4 transformation matrix elements.</summary>
    public ComputationGraph BuildTransformationGraph(double[] matrixElements)
    {
        if (matrixElements is null) throw new ArgumentNullException(nameof(matrixElements));
        if (matrixElements.Length != 16) throw new ArgumentException("Matrix must have 16 elements (4x4).");

        var graph = new ComputationGraph();
        for (var i = 0; i < 16; i++)
        {
            var name = $"m{i / 4},{i % 4}";
            var node = new GraphNode(name, matrixElements[i].ToString(CultureInfo.InvariantCulture));
            graph.AddNode(node);
        }
        return graph;
    }
}
