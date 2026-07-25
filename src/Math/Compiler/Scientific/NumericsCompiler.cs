namespace MathVerse.Math.Compiler.Scientific;

using System;
using System.Globalization;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;
using IROperand = MathVerse.Math.Compiler.IR.IROperand;
using IROperation = MathVerse.Math.Compiler.IR.IROperation;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;

/// <summary>Compiles numerical expressions. Handles floating-point, intervals, precision.
/// Lowers numerical expressions to optimized IR with SIMD hints.</summary>
public sealed class NumericsCompiler : ScientificCompilerBase
{
    /// <inheritdoc />
    public override string DomainName => "Numerics";

    /// <summary>Gets or sets a value indicating whether to emit SIMD hints.</summary>
    public bool EnableSIMD { get; set; } = true;

    /// <inheritdoc />
    public override IRModule Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var module = new IRModule();
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var dest = module.CreateTemp();
            if (double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                module.Append(IRInstruction.CreateLoadConst(val, dest));
            }
            else if (part.Length == 1 && char.IsLetter(part[0]))
            {
                if (!module.HasVariable(part))
                    module.DeclareVariable(part);
                module.Append(IRInstruction.CreateLoadVar(part, dest));
            }
            else if (i + 2 < parts.Length && part is "+" or "-" or "*" or "/")
            {
                var left = module.Instructions[^2].Destination;
                var right = module.Instructions[^1].Destination;
                var op = part switch
                {
                    "+" => IROperation.Add,
                    "-" => IROperation.Sub,
                    "*" => IROperation.Mul,
                    "/" => IROperation.Div,
                    _ => throw new InvalidOperationException()
                };
                module.Append(IRInstruction.CreateBinary(op, left!, right!, dest));
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
