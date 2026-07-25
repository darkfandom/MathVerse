namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Collections.Generic;
using System.Linq;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;
using GraphNode = MathVerse.Math.Compiler.Graph.GraphNode;

/// <summary>Compiles simulation models. Handles ODEs, PDEs, discrete-time systems.
/// Lowers simulation descriptions to time-stepping computation graphs.</summary>
public sealed class SimulationCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Simulation";

    /// <summary>Time step for the simulation.</summary>
    public double TimeStep { get; set; } = 0.01;

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var lines = expression.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            var eqParts = trimmed.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (eqParts.Length == 2)
            {
                var varName = eqParts[0].Trim();
                var rhs = eqParts[1].Trim();
                CompileRHS(rhs, module);
                if (!module.HasVariable(varName))
                    module.DeclareVariable(varName);
            }
        }
        return module;
    }

    /// <summary>Generates a time-stepping computation graph from a set of ODE equations.</summary>
    public ComputationGraph BuildTimeSteppingGraph(IReadOnlyList<string> stateVariables, IReadOnlyList<string> derivativeExpressions)
    {
        if (stateVariables is null) throw new ArgumentNullException(nameof(stateVariables));
        if (derivativeExpressions is null) throw new ArgumentNullException(nameof(derivativeExpressions));
        if (stateVariables.Count != derivativeExpressions.Count)
            throw new ArgumentException("State variables and derivative expressions must have the same count.");

        var graph = new ComputationGraph();
        for (var i = 0; i < stateVariables.Count; i++)
        {
            var node = new GraphNode(stateVariables[i], derivativeExpressions[i]);
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

    private static void CompileRHS(string rhs, IRModule module)
    {
        var tokens = rhs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var tok in tokens)
        {
            var dest = module.CreateTemp();
            if (double.TryParse(tok, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var val))
            {
                module.Append(IRInstruction.CreateLoadConst(val, dest));
            }
            else if (tok is "+" or "-" or "*" or "/" && module.InstructionCount >= 2)
            {
                var left = module.Instructions[^2].Destination;
                var right = module.Instructions[^1].Destination;
                var op = tok switch
                {
                    "+" => IROperation.Add,
                    "-" => IROperation.Sub,
                    "*" => IROperation.Mul,
                    "/" => IROperation.Div,
                    _ => throw new InvalidOperationException()
                };
                module.Append(IRInstruction.CreateBinary(op, left!, right!, dest));
            }
            else if (char.IsLetter(tok[0]) || tok[0] == '_')
            {
                if (!module.HasVariable(tok))
                    module.DeclareVariable(tok);
                module.Append(IRInstruction.CreateLoadVar(tok, dest));
            }
        }
    }
}
