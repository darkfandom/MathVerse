namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Replaces expressions with known constant values. If both operands of an
/// arithmetic instruction are constants, the instruction is folded to a constant.
/// </summary>
public sealed class ConstantPropagation : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "ConstantPropagation";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            PropagateInFunction(function);
        return module;
    }

    private static void PropagateInFunction(IRFunction function)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var block in function.Blocks)
            {
                var toRemove = new List<IRInstruction>();
                for (var i = 0; i < block.Instructions.Count; i++)
                {
                    var inst = block.Instructions[i];
                    if (inst is IRPhiNode)
                        continue;

                    if (!TryEvaluate(inst, out var result))
                        continue;

                    var replacement = IRValue.CreateConstant(
                        $"cp_{inst.Result?.Name ?? $"t{i}"}", result, inst.Result?.Type ?? IRType.Float64);

                    ReplaceAllUses(function, inst.Result!, replacement);
                    toRemove.Add(inst);
                    changed = true;
                }

                foreach (var inst in toRemove)
                    block.RemoveInstruction(inst);
            }
        }
    }

    private static bool TryEvaluate(IRInstruction inst, out double result)
    {
        result = 0;
        if (inst.Result == null || inst.Operands.Count == 0)
            return false;

        switch (inst.OpCode)
        {
            case IROpCode.Add:
            case IROpCode.Sub:
            case IROpCode.Mul:
            case IROpCode.Div:
            case IROpCode.Mod:
                return TryFoldBinary(inst, out result);
            case IROpCode.Neg:
                return TryFoldUnary(inst, out result);
            case IROpCode.Abs:
                if (TryGetConstantValue(inst.Operands[0], out var v))
                {
                    result = Math.Abs(v);
                    return true;
                }
                return false;
            case IROpCode.Sqrt:
                if (TryGetConstantValue(inst.Operands[0], out var sv))
                {
                    result = Math.Sqrt(sv);
                    return true;
                }
                return false;
            case IROpCode.Sin:
                if (TryGetConstantValue(inst.Operands[0], out var sinv))
                {
                    result = Math.Sin(sinv);
                    return true;
                }
                return false;
            case IROpCode.Cos:
                if (TryGetConstantValue(inst.Operands[0], out var cosv))
                {
                    result = Math.Cos(cosv);
                    return true;
                }
                return false;
            case IROpCode.Tan:
                if (TryGetConstantValue(inst.Operands[0], out var tanv))
                {
                    result = Math.Tan(tanv);
                    return true;
                }
                return false;
            case IROpCode.Log:
                if (TryGetConstantValue(inst.Operands[0], out var logv))
                {
                    result = Math.Log(logv);
                    return true;
                }
                return false;
            case IROpCode.Exp:
                if (TryGetConstantValue(inst.Operands[0], out var expv))
                {
                    result = Math.Exp(expv);
                    return true;
                }
                return false;
            case IROpCode.Pow:
                if (inst.Operands.Count >= 2 &&
                    TryGetConstantValue(inst.Operands[0], out var baseVal) &&
                    TryGetConstantValue(inst.Operands[1], out var expVal))
                {
                    result = Math.Pow(baseVal, expVal);
                    return true;
                }
                return false;
            case IROpCode.Cast:
                if (TryGetConstantValue(inst.Operands[0], out var castVal))
                {
                    result = castVal;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static bool TryFoldBinary(IRInstruction inst, out double result)
    {
        result = 0;
        if (inst.Operands.Count < 2)
            return false;

        if (!TryGetConstantValue(inst.Operands[0], out var left) ||
            !TryGetConstantValue(inst.Operands[1], out var right))
            return false;

        result = inst.OpCode switch
        {
            IROpCode.Add => left + right,
            IROpCode.Sub => left - right,
            IROpCode.Mul => left * right,
            IROpCode.Div => right != 0 ? left / right : double.NaN,
            IROpCode.Mod => right != 0 ? left % right : double.NaN,
            _ => 0
        };
        return true;
    }

    private static bool TryFoldUnary(IRInstruction inst, out double result)
    {
        result = 0;
        if (inst.Operands.Count < 1)
            return false;

        if (!TryGetConstantValue(inst.Operands[0], out var val))
            return false;

        result = inst.OpCode switch
        {
            IROpCode.Neg => -val,
            _ => 0
        };
        return true;
    }

    private static bool TryGetConstantValue(IRValue value, out double result)
    {
        if (value.IsConstant && value.ConstantValue.HasValue)
        {
            result = value.ConstantValue.Value;
            return true;
        }
        result = 0;
        return false;
    }

    private static void ReplaceAllUses(IRFunction function, IRValue oldValue, IRValue newValue)
    {
        foreach (var block in function.Blocks)
        {
            for (var i = 0; i < block.Instructions.Count; i++)
            {
                var inst = block.Instructions[i];
                if (inst is IRPhiNode phi)
                {
                    var newEdges = phi.IncomingEdges
                        .Select(e => e.Value == oldValue ? (newValue, e.Block) : e)
                        .ToList();
                    if (newEdges.Any(e => e.Item1 == newValue))
                    {
                        var newPhi = new IRPhiNode(phi.Result!, newEdges);
                        block.Instructions[i] = newPhi;
                        newPhi.ParentBlock = block;
                        newPhi.SequenceIndex = i;
                    }
                }
                else
                {
                    var newOperands = inst.Operands
                        .Select(o => o == oldValue ? newValue : o)
                        .ToList();
                    if (!newOperands.SequenceEqual(inst.Operands))
                    {
                        var newInst = new IRInstruction(inst.OpCode, inst.Result, newOperands);
                        newInst.ParentBlock = block;
                        newInst.SequenceIndex = i;
                        block.Instructions[i] = newInst;
                        if (newInst.IsTerminator)
                            block.Terminator = newInst;
                    }
                }
            }
        }
    }
}
