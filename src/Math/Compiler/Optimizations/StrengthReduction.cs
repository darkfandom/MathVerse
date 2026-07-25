namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Replaces expensive operations with cheaper equivalents:
/// x * 2 → x + x, x / 2 → x * 0.5, x ^ 2 → x * x,
/// sqrt(x) * sqrt(x) → x.
/// </summary>
public sealed class StrengthReduction : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "StrengthReduction";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            ReduceInFunction(function);
        return module;
    }

    private static void ReduceInFunction(IRFunction function)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var block in function.Blocks)
            {
                var toReplace = new List<(IRInstruction Old, IRInstruction New)>();

                for (var i = 0; i < block.Instructions.Count; i++)
                {
                    var inst = block.Instructions[i];
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.Result == null)
                        continue;

                    var replacement = TryReduce(inst, block);
                    if (replacement != null)
                    {
                        toReplace.Add((inst, replacement));
                        changed = true;
                    }
                }

                foreach (var (old, replacement) in toReplace)
                {
                    var idx = block.Instructions.IndexOf(old);
                    if (idx < 0) continue;

                    block.Instructions[idx] = replacement;
                    replacement.ParentBlock = block;
                    replacement.SequenceIndex = idx;

                    if (replacement.IsTerminator)
                        block.Terminator = replacement;
                }
            }
        }
    }

    private static IRInstruction? TryReduce(IRInstruction inst, IRBlock block)
    {
        switch (inst.OpCode)
        {
            case IROpCode.Mul:
                return TryReduceMul(inst) ?? TryReduceSqrtMul(inst);
            case IROpCode.Div:
                return TryReduceDiv(inst);
            case IROpCode.Pow:
                return TryReducePow(inst, block);
            default:
                return null;
        }
    }

    private static IRInstruction? TryReduceMul(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (left.IsConstant && left.ConstantValue.HasValue)
        {
            var val = left.ConstantValue.Value;
            if (IsPowerOfTwo(val) && val > 1)
            {
                return new IRInstruction(IROpCode.Add, inst.Result, right, right);
            }
        }

        if (right.IsConstant && right.ConstantValue.HasValue)
        {
            var val = right.ConstantValue.Value;
            if (IsPowerOfTwo(val) && val > 1)
            {
                return new IRInstruction(IROpCode.Add, inst.Result, left, left);
            }
        }

        return null;
    }

    private static IRInstruction? TryReduceDiv(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var right = inst.Operands[1];
        if (!right.IsConstant || !right.ConstantValue.HasValue)
            return null;

        var val = right.ConstantValue.Value;
        if (IsPowerOfTwo(val) && val > 1)
        {
            var reciprocal = 1.0 / val;
            var recipConst = IRValue.CreateConstant($"sr_recip_{val}", reciprocal, IRType.Float64);
            return new IRInstruction(IROpCode.Mul, inst.Result, inst.Operands[0], recipConst);
        }

        return null;
    }

    private static IRInstruction? TryReducePow(IRInstruction inst, IRBlock block)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var exponent = inst.Operands[1];
        if (!exponent.IsConstant || !exponent.ConstantValue.HasValue)
            return null;

        var expVal = exponent.ConstantValue.Value;
        if (Math.Abs(expVal - 2.0) < 1e-10)
        {
            var baseOperand = inst.Operands[0];
            return new IRInstruction(IROpCode.Mul, inst.Result, baseOperand, baseOperand);
        }

        return null;
    }

    private static IRInstruction? TryReduceSqrtMul(IRInstruction inst)
    {
        if (inst.OpCode != IROpCode.Mul || inst.Operands.Count < 2)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (left != right)
            return null;

        if (inst.Result == null)
            return null;

        var definition = FindDefinition(inst, left);
        if (definition == null || definition.OpCode != IROpCode.Sqrt)
            return null;

        var sqrtOperand = definition.Operands[0];
        return new IRInstruction(IROpCode.Nop, inst.Result, sqrtOperand);
    }

    private static IRInstruction? FindDefinition(IRInstruction current, IRValue value)
    {
        var block = current.ParentBlock;
        if (block == null) return null;

        foreach (var inst in block.Instructions)
        {
            if (inst.Result != null && inst.Result.Id == value.Id)
                return inst;
        }
        return null;
    }

    private static bool IsPowerOfTwo(double value)
    {
        if (value <= 0 || value != Math.Floor(value))
            return false;
        var intVal = (long)value;
        return intVal > 0 && (intVal & (intVal - 1)) == 0;
    }
}
