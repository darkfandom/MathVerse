namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Local pattern-matching optimizations on instruction sequences:
/// Add x, 0 → x; Mul x, 1 → x; Mul x, 0 → const 0; two consecutive Neg → identity.
/// </summary>
public sealed class PeepholeOptimizer : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "PeepholeOptimizer";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            OptimizeFunction(function);
        return module;
    }

    private static void OptimizeFunction(IRFunction function)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var block in function.Blocks)
            {
                changed |= ApplyLocalPatterns(block);
                changed |= ApplyConsecutivePatterns(block);
            }
        }
    }

    private static bool ApplyLocalPatterns(IRBlock block)
    {
        var changed = false;
        var toRemove = new List<IRInstruction>();
        var replacements = new Dictionary<int, IRInstruction>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst is IRPhiNode)
                continue;
            if (inst.Result == null)
                continue;

            var result = TryMatchPattern(inst);
            if (result != null)
            {
                replacements[i] = result;
                changed = true;
            }
        }

        foreach (var (idx, newInst) in replacements)
        {
            block.Instructions[idx] = newInst;
            newInst.ParentBlock = block;
            newInst.SequenceIndex = idx;
        }

        return changed;
    }

    private static IRInstruction? TryMatchPattern(IRInstruction inst)
    {
        switch (inst.OpCode)
        {
            case IROpCode.Add:
                return TryMatchAddZero(inst);
            case IROpCode.Sub:
                return TryMatchSubZero(inst);
            case IROpCode.Mul:
                return TryMatchMulOneOrZero(inst);
            case IROpCode.Neg:
                return TryMatchDoubleNeg(inst);
            default:
                return null;
        }
    }

    private static IRInstruction? TryMatchAddZero(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsZeroConstant(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (IsZeroConstant(left))
            return new IRInstruction(IROpCode.Nop, inst.Result, right);

        return null;
    }

    private static IRInstruction? TryMatchSubZero(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var right = inst.Operands[1];
        if (IsZeroConstant(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, inst.Operands[0]);

        return null;
    }

    private static IRInstruction? TryMatchMulOneOrZero(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsZeroConstant(left) || IsZeroConstant(right))
        {
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"peep_zero_{inst.Result.Name}", 0.0, inst.Result.Type));
        }

        if (IsOneConstant(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (IsOneConstant(left))
            return new IRInstruction(IROpCode.Nop, inst.Result, right);

        return null;
    }

    private static IRInstruction? TryMatchDoubleNeg(IRInstruction inst)
    {
        if (inst.Operands.Count < 1 || inst.Result == null)
            return null;

        var operand = inst.Operands[0];
        var definer = FindDefiningInstruction(inst, operand);

        if (definer != null && definer.OpCode == IROpCode.Neg)
        {
            var innerOperand = definer.Operands.Count > 0 ? definer.Operands[0] : operand;
            return new IRInstruction(IROpCode.Nop, inst.Result, innerOperand);
        }

        return null;
    }

    private static bool ApplyConsecutivePatterns(IRBlock block)
    {
        var changed = false;
        var toRemove = new List<IRInstruction>();

        for (var i = 0; i < block.Instructions.Count - 1; i++)
        {
            var inst1 = block.Instructions[i];
            var inst2 = block.Instructions[i + 1];

            if (inst1 is IRPhiNode || inst2 is IRPhiNode)
                continue;
            if (inst1.Result == null || inst2.Result == null)
                continue;

            if (inst1.OpCode == IROpCode.Neg && inst2.OpCode == IROpCode.Neg)
            {
                if (inst2.Operands.Count > 0 && inst2.Operands[0] == inst1.Result)
                {
                    var innerOperand = inst1.Operands.Count > 0 ? inst1.Operands[0] : inst1.Result;
                    var replacement = new IRInstruction(IROpCode.Nop, inst2.Result, innerOperand);

                    block.Instructions[i + 1] = replacement;
                    replacement.ParentBlock = block;
                    replacement.SequenceIndex = i + 1;
                    toRemove.Add(inst1);
                    changed = true;
                }
            }
        }

        foreach (var inst in toRemove)
            block.RemoveInstruction(inst);

        return changed;
    }

    private static IRInstruction? FindDefiningInstruction(IRInstruction current, IRValue value)
    {
        var block = current.ParentBlock;
        if (block == null) return null;

        for (var i = block.Instructions.Count - 1; i >= 0; i--)
        {
            var inst = block.Instructions[i];
            if (inst.Result != null && inst.Result.Id == value.Id)
                return inst;
        }
        return null;
    }

    private static bool IsZeroConstant(IRValue value)
        => value.IsConstant && value.ConstantValue.HasValue && value.ConstantValue.Value == 0.0;

    private static bool IsOneConstant(IRValue value)
        => value.IsConstant && value.ConstantValue.HasValue && value.ConstantValue.Value == 1.0;
}
