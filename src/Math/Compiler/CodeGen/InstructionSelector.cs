namespace MathVerse.Math.Compiler.CodeGen;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a selected instruction pattern mapping one or more IR opcodes to a fused target operation.
/// </summary>
public sealed class SelectedPattern
{
    /// <summary>The source IR opcodes that are fused together.</summary>
    public IReadOnlyList<IROpCode> SourceOpCodes { get; }

    /// <summary>The name of the fused target operation.</summary>
    public string TargetOperation { get; }

    /// <summary>The number of result values produced by the fused pattern.</summary>
    public int ResultCount { get; }

    /// <summary>
    /// Initializes a new selected pattern.
    /// </summary>
    /// <param name="sourceOpCodes">The source opcodes being fused.</param>
    /// <param name="targetOperation">The target fused operation name.</param>
    /// <param name="resultCount">Number of result values.</param>
    public SelectedPattern(IReadOnlyList<IROpCode> sourceOpCodes, string targetOperation, int resultCount)
    {
        SourceOpCodes = sourceOpCodes;
        TargetOperation = targetOperation;
        ResultCount = resultCount;
    }
}

/// <summary>
/// Selects target instructions from IR operations by mapping IROpCode sequences to fused operations.
/// Identifies patterns such as multiply-add → FMA, load-arithmetic-store → fused memory operations, etc.
/// </summary>
public sealed class InstructionSelector
{
    /// <summary>
    /// Analyzes a function's instructions and returns fused instruction patterns.
    /// </summary>
    /// <param name="function">The IR function to analyze.</param>
    /// <returns>A list of selected fused patterns with their positions.</returns>
    public IReadOnlyList<(int BlockIndex, int InstructionIndex, SelectedPattern Pattern)> Select(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var results = new List<(int, int, SelectedPattern)>();

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];
            var skipUntil = -1;

            for (var i = 0; i < block.Instructions.Count; i++)
            {
                if (i <= skipUntil)
                    continue;

                var inst = block.Instructions[i];

                if (i + 1 < block.Instructions.Count)
                {
                    var next = block.Instructions[i + 1];
                    var fused = TryFusePair(inst, next);
                    if (fused != null)
                    {
                        results.Add((b, i, fused));
                        skipUntil = i + 1;
                        continue;
                    }
                }

                if (i + 2 < block.Instructions.Count)
                {
                    var fused = TryFuseTriple(block.Instructions[i], block.Instructions[i + 1], block.Instructions[i + 2]);
                    if (fused != null)
                    {
                        results.Add((b, i, fused));
                        skipUntil = i + 2;
                        continue;
                    }
                }
            }
        }

        return results;
    }

    private static SelectedPattern? TryFusePair(IRInstruction first, IRInstruction second)
    {
        if (first.Result == null)
            return null;

        var firstResult = first.Result;

        // Mul + Add → FMA: result = a * b + c
        if (first.OpCode == IROpCode.Mul && second.OpCode == IROpCode.Add)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Mul, IROpCode.Add },
                        "FMA",
                        1);
                }
            }
        }

        // Mul + Sub → FMS: result = a * b - c
        if (first.OpCode == IROpCode.Mul && second.OpCode == IROpCode.Sub)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Mul, IROpCode.Sub },
                        "FMS",
                        1);
                }
            }
        }

        // Load + Add → fused load-add
        if (first.OpCode == IROpCode.Load && second.OpCode == IROpCode.Add)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Load, IROpCode.Add },
                        "LoadAdd",
                        1);
                }
            }
        }

        // Load + Mul → fused load-mul
        if (first.OpCode == IROpCode.Load && second.OpCode == IROpCode.Mul)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Load, IROpCode.Mul },
                        "LoadMul",
                        1);
                }
            }
        }

        // Sin + Mul → scaled sin
        if (first.OpCode == IROpCode.Sin && second.OpCode == IROpCode.Mul)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Sin, IROpCode.Mul },
                        "ScaledSin",
                        1);
                }
            }
        }

        // Cos + Mul → scaled cos
        if (first.OpCode == IROpCode.Cos && second.OpCode == IROpCode.Mul)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Cos, IROpCode.Mul },
                        "ScaledCos",
                        1);
                }
            }
        }

        // Add + Mul → multiply-add (commutative FMA candidate)
        if (first.OpCode == IROpCode.Add && second.OpCode == IROpCode.Mul)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Add, IROpCode.Mul },
                        "MultiplyAdd",
                        1);
                }
            }
        }

        // Exp + Mul → scaled exp
        if (first.OpCode == IROpCode.Exp && second.OpCode == IROpCode.Mul)
        {
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    return new SelectedPattern(
                        new[] { IROpCode.Exp, IROpCode.Mul },
                        "ScaledExp",
                        1);
                }
            }
        }

        return null;
    }

    private static SelectedPattern? TryFuseTriple(IRInstruction first, IRInstruction second, IRInstruction third)
    {
        if (first.Result == null || second.Result == null)
            return null;

        var firstResult = first.Result;
        var secondResult = second.Result;

        // Mul + Mul + Add → chain multiply-add
        if (first.OpCode == IROpCode.Mul && second.OpCode == IROpCode.Mul && third.OpCode == IROpCode.Add)
        {
            var usedBySecond = false;
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    usedBySecond = true;
                    break;
                }
            }

            if (usedBySecond)
            {
                for (var i = 0; i < third.Operands.Count; i++)
                {
                    if (third.Operands[i] == secondResult)
                    {
                        return new SelectedPattern(
                            new[] { IROpCode.Mul, IROpCode.Mul, IROpCode.Add },
                            "ChainMulAdd",
                            1);
                    }
                }
            }
        }

        // Load + Add + Store → fused load-modify-store
        if (first.OpCode == IROpCode.Load && second.OpCode == IROpCode.Add && third.OpCode == IROpCode.Store)
        {
            var addUsesLoad = false;
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    addUsesLoad = true;
                    break;
                }
            }

            if (addUsesLoad && third.Operands.Count >= 2 && third.Operands[1] == secondResult)
            {
                return new SelectedPattern(
                    new[] { IROpCode.Load, IROpCode.Add, IROpCode.Store },
                    "FusedLoadAddStore",
                    0);
            }
        }

        // Load + Mul + Store → fused load-multiply-store
        if (first.OpCode == IROpCode.Load && second.OpCode == IROpCode.Mul && third.OpCode == IROpCode.Store)
        {
            var mulUsesLoad = false;
            for (var i = 0; i < second.Operands.Count; i++)
            {
                if (second.Operands[i] == firstResult)
                {
                    mulUsesLoad = true;
                    break;
                }
            }

            if (mulUsesLoad && third.Operands.Count >= 2 && third.Operands[1] == secondResult)
            {
                return new SelectedPattern(
                    new[] { IROpCode.Load, IROpCode.Mul, IROpCode.Store },
                    "FusedLoadMulStore",
                    0);
            }
        }

        return null;
    }
}
