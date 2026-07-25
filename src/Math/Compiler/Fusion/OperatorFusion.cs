namespace MathVerse.Math.Compiler.Fusion;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a detected fusible operator pattern within a basic block.
/// </summary>
public sealed class FusiblePattern
{
    /// <summary>The starting instruction index within the block.</summary>
    public int StartIndex { get; }

    /// <summary>The number of consecutive instructions that form the pattern.</summary>
    public int Length { get; }

    /// <summary>The fused operation name (e.g., "FMA", "NegMul").</summary>
    public string FusedOperation { get; }

    /// <summary>The result value of the fused operation.</summary>
    public IRValue Result { get; }

    /// <summary>
    /// Initializes a new fusible pattern.
    /// </summary>
    public FusiblePattern(int startIndex, int length, string fusedOperation, IRValue result)
    {
        StartIndex = startIndex;
        Length = length;
        FusedOperation = fusedOperation;
        Result = result;
    }
}

/// <summary>
/// Fuses consecutive operators into combined operations. Scans IR blocks for fusible
/// patterns such as multiply-add → FMA, negate-multiply → NegMul, etc.
/// </summary>
public sealed class OperatorFusion
{
    /// <summary>
    /// Scans a function for fusible operator patterns and returns all detected patterns.
    /// </summary>
    /// <param name="function">The IR function to scan.</param>
    /// <returns>A dictionary mapping block indices to their fusible patterns.</returns>
    public IReadOnlyDictionary<int, IReadOnlyList<FusiblePattern>> FindFusiblePatterns(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var results = new Dictionary<int, IReadOnlyList<FusiblePattern>>();

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];
            var patterns = ScanBlock(block);
            if (patterns.Count > 0)
                results[b] = patterns;
        }

        return results;
    }

    /// <summary>
    /// Applies operator fusion to a function, replacing fusible patterns with combined operations.
    /// Returns a new function with fused operations.
    /// </summary>
    /// <param name="function">The function to optimize.</param>
    /// <returns>A new function with operator fusion applied.</returns>
    public IRFunction ApplyFusion(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var fused = new IRFunction(function.Name + "_fused", function.ReturnType, function.Parameters);

        var patternsByBlock = FindFusiblePatterns(function);
        var valueMap = new Dictionary<int, IRValue>();

        foreach (var param in function.Parameters)
            valueMap[param.Id] = param;

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var srcBlock = function.Blocks[b];
            var newBlock = fused.CreateBlock(srcBlock.Label);

            patternsByBlock.TryGetValue(b, out var patterns);
            var patternLookup = new Dictionary<int, FusiblePattern>();
            if (patterns != null)
            {
                for (var i = 0; i < patterns.Count; i++)
                    patternLookup[patterns[i].StartIndex] = patterns[i];
            }

            var skipUntil = -1;
            for (var i = 0; i < srcBlock.Instructions.Count; i++)
            {
                if (i <= skipUntil)
                    continue;

                if (patternLookup.TryGetValue(i, out var pattern))
                {
                    EmitFusedInstruction(newBlock, pattern, srcBlock, valueMap);
                    skipUntil = i + pattern.Length - 1;
                    continue;
                }

                var inst = srcBlock.Instructions[i];
                var remapped = RemapInst(inst, valueMap);
                newBlock.AppendInstruction(remapped);

                if (remapped.Result != null)
                    valueMap[remapped.Result.Id] = remapped.Result;
            }

            if (srcBlock.Terminator != null && !srcBlock.Instructions.Contains(srcBlock.Terminator))
            {
                var remappedTerm = RemapInst(srcBlock.Terminator, valueMap);
                newBlock.AppendInstruction(remappedTerm);
            }
        }

        return fused;
    }

    private static List<FusiblePattern> ScanBlock(IRBlock block)
    {
        var patterns = new List<FusiblePattern>();
        var usedAsOperand = new HashSet<int>();

        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            var first = block.Instructions[i];
            var second = block.Instructions[i + 1];

            if (first.Result == null || second.Result == null)
                continue;

            if (usedAsOperand.Contains(first.Result.Id) || usedAsOperand.Contains(second.Result.Id))
                continue;

            var pattern = MatchFusiblePair(first, second);
            if (pattern != null)
            {
                patterns.Add(pattern);
                usedAsOperand.Add(first.Result.Id);
                usedAsOperand.Add(second.Result.Id);
            }
        }

        return patterns;
    }

    private static FusiblePattern? MatchFusiblePair(IRInstruction first, IRInstruction second)
    {
        if (first.Result == null) return null;

        // Mul + Add → FMA
        if (first.OpCode == IROpCode.Mul && second.OpCode == IROpCode.Add)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "FMA",
                    second.Result!);
            }
        }

        // Mul + Sub → FMS
        if (first.OpCode == IROpCode.Mul && second.OpCode == IROpCode.Sub)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "FMS",
                    second.Result!);
            }
        }

        // Neg + Add → Sub
        if (first.OpCode == IROpCode.Neg && second.OpCode == IROpCode.Add)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "Sub",
                    second.Result!);
            }
        }

        // Neg + Mul → NegMul
        if (first.OpCode == IROpCode.Neg && second.OpCode == IROpCode.Mul)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "NegMul",
                    second.Result!);
            }
        }

        // Sin + Cos → SinCos (pair)
        if (first.OpCode == IROpCode.Sin && second.OpCode == IROpCode.Cos)
        {
            if (first.Operands.Count > 0 && second.Operands.Count > 0 &&
                first.Operands[0].Id == second.Operands[0].Id)
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "SinCos",
                    first.Result!);
            }
        }

        // Add + Mul → MultiplyAdd (reverse FMA)
        if (first.OpCode == IROpCode.Add && second.OpCode == IROpCode.Mul)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "MultiplyAdd",
                    second.Result!);
            }
        }

        // Sqrt + Mul → ScaledSqrt
        if (first.OpCode == IROpCode.Sqrt && second.OpCode == IROpCode.Mul)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "ScaledSqrt",
                    second.Result!);
            }
        }

        // Log + Mul → ScaledLog
        if (first.OpCode == IROpCode.Log && second.OpCode == IROpCode.Mul)
        {
            if (OperandUses(second, first.Result))
            {
                return new FusiblePattern(
                    first.SequenceIndex,
                    2,
                    "ScaledLog",
                    second.Result!);
            }
        }

        return null;
    }

    private static void EmitFusedInstruction(IRBlock targetBlock, FusiblePattern pattern, IRBlock sourceBlock, Dictionary<int, IRValue> valueMap)
    {
        var first = sourceBlock.Instructions[pattern.StartIndex];
        var second = sourceBlock.Instructions[pattern.StartIndex + 1];

        switch (pattern.FusedOperation)
        {
            case "FMA":
            {
                var a = ResolveOperand(first.Operands[0], valueMap);
                var b = ResolveOperand(first.Operands[1], valueMap);
                var c = FindNonFirstOperand(second, first.Result!, valueMap);
                var result = IRValue.CreateRegister($"fma_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Fma, result, a, b, c));
                valueMap[pattern.Result.Id] = result;
                break;
            }
            case "FMS":
            {
                var a = ResolveOperand(first.Operands[0], valueMap);
                var b = ResolveOperand(first.Operands[1], valueMap);
                var c = FindNonFirstOperand(second, first.Result!, valueMap);
                var mulResult = IRValue.CreateRegister($"fms_mul_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Mul, mulResult, a, b));
                var result = IRValue.CreateRegister($"fms_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Sub, result, mulResult, c));
                valueMap[pattern.Result.Id] = result;
                break;
            }
            case "Sub":
            {
                var a = FindNonFirstOperand(second, first.Result!, valueMap);
                var b = ResolveOperand(first.Operands[0], valueMap);
                var result = IRValue.CreateRegister($"neg_add_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Sub, result, a, b));
                valueMap[pattern.Result.Id] = result;
                break;
            }
            case "NegMul":
            {
                var a = ResolveOperand(first.Operands[0], valueMap);
                var negResult = IRValue.CreateRegister($"neg_{pattern.Result.Name}_neg", first.Result!.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Neg, negResult, a));
                var b = FindNonFirstOperand(second, first.Result, valueMap);
                var result = IRValue.CreateRegister($"neg_mul_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Mul, result, negResult, b));
                valueMap[pattern.Result.Id] = result;
                break;
            }
            case "SinCos":
            {
                var arg = ResolveOperand(first.Operands[0], valueMap);
                var sinResult = IRValue.CreateRegister($"sin_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Sin, sinResult, arg));
                var cosResult = IRValue.CreateRegister($"cos_{pattern.Result.Name}", second.Result!.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Cos, cosResult, arg));
                valueMap[pattern.Result.Id] = sinResult;
                valueMap[second.Result.Id] = cosResult;
                break;
            }
            case "MultiplyAdd":
            {
                var a = ResolveOperand(first.Operands[0], valueMap);
                var b = ResolveOperand(first.Operands[1], valueMap);
                var c = FindNonFirstOperand(second, first.Result!, valueMap);
                var result = IRValue.CreateRegister($"mul_add_{pattern.Result.Name}", pattern.Result.Type);
                targetBlock.AppendInstruction(new IRInstruction(IROpCode.Fma, result, a, b, c));
                valueMap[pattern.Result.Id] = result;
                break;
            }
            default:
            {
                var remapped = RemapInst(first, valueMap);
                targetBlock.AppendInstruction(remapped);
                if (remapped.Result != null)
                    valueMap[remapped.Result.Id] = remapped.Result;

                var remapped2 = RemapInst(second, valueMap);
                targetBlock.AppendInstruction(remapped2);
                if (remapped2.Result != null)
                    valueMap[remapped2.Result.Id] = remapped2.Result;
                break;
            }
        }
    }

    private static IRInstruction RemapInst(IRInstruction inst, Dictionary<int, IRValue> valueMap)
    {
        var operands = new IRValue[inst.Operands.Count];
        for (var i = 0; i < inst.Operands.Count; i++)
            operands[i] = ResolveOperand(inst.Operands[i], valueMap);

        IRValue? newResult = null;
        if (inst.Result != null)
        {
            newResult = IRValue.CreateRegister(inst.Result.Name, inst.Result.Type);
        }

        return new IRInstruction(inst.OpCode, newResult, operands);
    }

    private static IRValue ResolveOperand(IRValue operand, Dictionary<int, IRValue> valueMap)
    {
        if (valueMap.TryGetValue(operand.Id, out var mapped))
            return mapped;
        return operand;
    }

    private static bool OperandUses(IRInstruction inst, IRValue value)
    {
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (inst.Operands[i].Id == value.Id)
                return true;
        }
        return false;
    }

    private static IRValue FindNonFirstOperand(IRInstruction second, IRValue firstResult, Dictionary<int, IRValue> valueMap)
    {
        for (var i = 0; i < second.Operands.Count; i++)
        {
            if (second.Operands[i].Id != firstResult.Id)
                return ResolveOperand(second.Operands[i], valueMap);
        }
        return ResolveOperand(second.Operands[0], valueMap);
    }
}
