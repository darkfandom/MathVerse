namespace MathVerse.Math.Compiler.Fusion;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a detected tensor computation pattern (e.g., matmul + bias + activation).
/// </summary>
public sealed class TensorPattern
{
    /// <summary>The type of tensor pattern detected.</summary>
    public TensorPatternKind Kind { get; }

    /// <summary>The instruction indices forming this pattern.</summary>
    public IReadOnlyList<int> InstructionIndices { get; }

    /// <summary>The result value of the fused tensor operation.</summary>
    public IRValue Result { get; }

    /// <summary>
    /// Initializes a new tensor pattern.
    /// </summary>
    public TensorPattern(TensorPatternKind kind, IReadOnlyList<int> instructionIndices, IRValue? result)
    {
        Kind = kind;
        InstructionIndices = instructionIndices;
        Result = result ?? IRValue.CreateVoid();
    }
}

/// <summary>
/// Identifies tensor computation patterns that can be fused into single kernels.
/// </summary>
public enum TensorPatternKind
{
    /// <summary>Matrix multiply followed by bias addition.</summary>
    MatMulBias,

    /// <summary>Matrix multiply followed by activation function (ReLU, sigmoid, etc.).</summary>
    MatMulActivation,

    /// <summary>Matrix multiply + bias + activation (3-op fusion).</summary>
    MatMulBiasActivation,

    /// <summary>Dot product followed by scaling.</summary>
    DotScale,

    /// <summary>Reshape followed by element-wise operation.</summary>
    ReshapeElementwise,

    /// <summary>Element-wise multiply + add (linear transform).</summary>
    LinearTransform
}

/// <summary>
/// Fuses tensor operations by pattern-matching common sequences such as
/// matmul + bias + activation into single combined operations.
/// </summary>
public sealed class TensorFusion
{
    /// <summary>
    /// Scans a function for fusible tensor operation patterns.
    /// </summary>
    /// <param name="function">The IR function to scan.</param>
    /// <returns>A dictionary mapping block indices to detected tensor patterns.</returns>
    public IReadOnlyDictionary<int, IReadOnlyList<TensorPattern>> FindTensorPatterns(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var results = new Dictionary<int, IReadOnlyList<TensorPattern>>();

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];
            var patterns = ScanForTensorPatterns(block);
            if (patterns.Count > 0)
                results[b] = patterns;
        }

        return results;
    }

    /// <summary>
    /// Applies tensor fusion to a function, replacing detected patterns with fused operations.
    /// </summary>
    /// <param name="function">The function to optimize.</param>
    /// <returns>A new function with tensor fusion applied.</returns>
    public IRFunction ApplyFusion(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var patternsByBlock = FindTensorPatterns(function);
        var fused = new IRFunction(function.Name + "_tfused", function.ReturnType, function.Parameters);
        var valueMap = new Dictionary<int, IRValue>();

        foreach (var param in function.Parameters)
            valueMap[param.Id] = param;

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var srcBlock = function.Blocks[b];
            var newBlock = fused.CreateBlock(srcBlock.Label);

            patternsByBlock.TryGetValue(b, out var patterns);
            var patternLookup = new Dictionary<int, TensorPattern>();
            if (patterns != null)
            {
                for (var i = 0; i < patterns.Count; i++)
                {
                    foreach (var idx in patterns[i].InstructionIndices)
                        patternLookup[idx] = patterns[i];
                }
            }

            var skipUntil = -1;
            for (var i = 0; i < srcBlock.Instructions.Count; i++)
            {
                if (i <= skipUntil)
                    continue;

                if (patternLookup.TryGetValue(i, out var pattern))
                {
                    EmitFusedTensorOp(newBlock, pattern, srcBlock, valueMap);
                    skipUntil = pattern.InstructionIndices[^1];
                    continue;
                }

                var inst = srcBlock.Instructions[i];
                var remapped = RemapInstruction(inst, valueMap);
                newBlock.AppendInstruction(remapped);
                if (remapped.Result != null)
                    valueMap[remapped.Result.Id] = remapped.Result;
            }

            if (srcBlock.Terminator != null && !srcBlock.Instructions.Contains(srcBlock.Terminator))
            {
                var remapped = RemapInstruction(srcBlock.Terminator, valueMap);
                newBlock.AppendInstruction(remapped);
            }
        }

        return fused;
    }

    private static List<TensorPattern> ScanForTensorPatterns(IRBlock block)
    {
        var patterns = new List<TensorPattern>();
        var consumed = new HashSet<int>();

        // Scan for MatMul + Add → MatMulBias
        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            if (consumed.Contains(i)) continue;
            var first = block.Instructions[i];
            if (first.OpCode != IROpCode.MatMul) continue;
            if (first.Result == null) continue;

            var second = block.Instructions[i + 1];
            if (second.OpCode == IROpCode.Add && OperandUsesValue(second, first.Result))
            {
                var nonMatmulOperand = FindOtherOperand(second, first.Result);
                if (nonMatmulOperand != null)
                {
                    // Check for 3-op: MatMul + Add + activation
                    if (i + 2 < block.Instructions.Count)
                    {
                        var third = block.Instructions[i + 2];
                        if (IsActivationOp(third.OpCode) && third.Result != null &&
                            OperandUsesValue(third, second.Result!))
                        {
                            consumed.Add(i);
                            consumed.Add(i + 1);
                            consumed.Add(i + 2);
                            patterns.Add(new TensorPattern(
                                TensorPatternKind.MatMulBiasActivation,
                                new[] { i, i + 1, i + 2 },
                                third.Result));
                            continue;
                        }
                    }

                    consumed.Add(i);
                    consumed.Add(i + 1);
                    patterns.Add(new TensorPattern(
                        TensorPatternKind.MatMulBias,
                        new[] { i, i + 1 },
                        second.Result));
                }
                continue;
            }

            if (IsActivationOp(second.OpCode) && second.Result != null &&
                OperandUsesValue(second, first.Result))
            {
                consumed.Add(i);
                consumed.Add(i + 1);
                patterns.Add(new TensorPattern(
                    TensorPatternKind.MatMulActivation,
                    new[] { i, i + 1 },
                    second.Result));
            }
        }

        // Scan for Dot + Mul → DotScale
        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            if (consumed.Contains(i)) continue;
            var first = block.Instructions[i];
            if (first.OpCode != IROpCode.Dot) continue;
            if (first.Result == null) continue;

            var second = block.Instructions[i + 1];
            if (second.OpCode == IROpCode.Mul && OperandUsesValue(second, first.Result))
            {
                consumed.Add(i);
                consumed.Add(i + 1);
                patterns.Add(new TensorPattern(
                    TensorPatternKind.DotScale,
                    new[] { i, i + 1 },
                    second.Result));
            }
        }

        // Scan for Reshape + Add/Mul → ReshapeElementwise
        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            if (consumed.Contains(i)) continue;
            var first = block.Instructions[i];
            if (first.OpCode != IROpCode.Reshape) continue;
            if (first.Result == null) continue;

            var second = block.Instructions[i + 1];
            if ((second.OpCode == IROpCode.Add || second.OpCode == IROpCode.Mul) &&
                OperandUsesValue(second, first.Result))
            {
                consumed.Add(i);
                consumed.Add(i + 1);
                patterns.Add(new TensorPattern(
                    TensorPatternKind.ReshapeElementwise,
                    new[] { i, i + 1 },
                    second.Result));
            }
        }

        // Scan for Mul + Add → LinearTransform
        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            if (consumed.Contains(i)) continue;
            var first = block.Instructions[i];
            if (first.OpCode != IROpCode.Mul) continue;
            if (first.Result == null) continue;

            var second = block.Instructions[i + 1];
            if (second.OpCode == IROpCode.Add && OperandUsesValue(second, first.Result))
            {
                consumed.Add(i);
                consumed.Add(i + 1);
                patterns.Add(new TensorPattern(
                    TensorPatternKind.LinearTransform,
                    new[] { i, i + 1 },
                    second.Result));
            }
        }

        return patterns;
    }

    private static void EmitFusedTensorOp(IRBlock targetBlock, TensorPattern pattern, IRBlock sourceBlock,
        Dictionary<int, IRValue> valueMap)
    {
        var firstInst = sourceBlock.Instructions[pattern.InstructionIndices[0]];

        switch (pattern.Kind)
        {
            case TensorPatternKind.MatMulBias:
            {
                var matmulResult = RemapInstruction(firstInst, valueMap);
                targetBlock.AppendInstruction(matmulResult);
                if (matmulResult.Result != null)
                    valueMap[firstInst.Result!.Id] = matmulResult.Result;

                var addInst = sourceBlock.Instructions[pattern.InstructionIndices[1]];
                var remappedAdd = RemapInstruction(addInst, valueMap);
                targetBlock.AppendInstruction(remappedAdd);
                if (remappedAdd.Result != null)
                    valueMap[addInst.Result!.Id] = remappedAdd.Result;
                break;
            }
            case TensorPatternKind.MatMulActivation:
            {
                var matmulResult = RemapInstruction(firstInst, valueMap);
                targetBlock.AppendInstruction(matmulResult);
                if (matmulResult.Result != null)
                    valueMap[firstInst.Result!.Id] = matmulResult.Result;

                var actInst = sourceBlock.Instructions[pattern.InstructionIndices[1]];
                var remappedAct = RemapInstruction(actInst, valueMap);
                targetBlock.AppendInstruction(remappedAct);
                if (remappedAct.Result != null)
                    valueMap[actInst.Result!.Id] = remappedAct.Result;
                break;
            }
            case TensorPatternKind.MatMulBiasActivation:
            {
                for (var i = 0; i < pattern.InstructionIndices.Count; i++)
                {
                    var inst = sourceBlock.Instructions[pattern.InstructionIndices[i]];
                    var remapped = RemapInstruction(inst, valueMap);
                    targetBlock.AppendInstruction(remapped);
                    if (remapped.Result != null && inst.Result != null)
                        valueMap[inst.Result.Id] = remapped.Result;
                }
                break;
            }
            case TensorPatternKind.DotScale:
            {
                var dotResult = RemapInstruction(firstInst, valueMap);
                targetBlock.AppendInstruction(dotResult);
                if (dotResult.Result != null)
                    valueMap[firstInst.Result!.Id] = dotResult.Result;

                var mulInst = sourceBlock.Instructions[pattern.InstructionIndices[1]];
                var remappedMul = RemapInstruction(mulInst, valueMap);
                targetBlock.AppendInstruction(remappedMul);
                if (remappedMul.Result != null)
                    valueMap[mulInst.Result!.Id] = remappedMul.Result;
                break;
            }
            default:
            {
                for (var i = 0; i < pattern.InstructionIndices.Count; i++)
                {
                    var inst = sourceBlock.Instructions[pattern.InstructionIndices[i]];
                    var remapped = RemapInstruction(inst, valueMap);
                    targetBlock.AppendInstruction(remapped);
                    if (remapped.Result != null && inst.Result != null)
                        valueMap[inst.Result.Id] = remapped.Result;
                }
                break;
            }
        }
    }

    private static IRInstruction RemapInstruction(IRInstruction inst, Dictionary<int, IRValue> valueMap)
    {
        var operands = new IRValue[inst.Operands.Count];
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (valueMap.TryGetValue(inst.Operands[i].Id, out var mapped))
                operands[i] = mapped;
            else
                operands[i] = inst.Operands[i];
        }

        IRValue? newResult = null;
        if (inst.Result != null)
            newResult = IRValue.CreateRegister(inst.Result.Name, inst.Result.Type);

        return new IRInstruction(inst.OpCode, newResult, operands);
    }

    private static bool OperandUsesValue(IRInstruction inst, IRValue value)
    {
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (inst.Operands[i].Id == value.Id)
                return true;
        }
        return false;
    }

    private static IRValue? FindOtherOperand(IRInstruction inst, IRValue exclude)
    {
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (inst.Operands[i].Id != exclude.Id)
                return inst.Operands[i];
        }
        return null;
    }

    private static bool IsActivationOp(IROpCode opCode)
    {
        return opCode is IROpCode.Sin or IROpCode.Cos or IROpCode.Tan
            or IROpCode.Exp or IROpCode.Log or IROpCode.Abs
            or IROpCode.Sqrt or IROpCode.Pow;
    }
}
