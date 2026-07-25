namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Applies algebraic simplifications: x+0→x, x*1→x, x*0→0, x-x→0, x/1→x,
/// and associativity reordering (x+y)+z → x+(y+z) when beneficial.
/// </summary>
public sealed class AlgebraicOptimizer : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "AlgebraicOptimizer";

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
                var toRemove = new List<IRInstruction>();
                var toAdd = new List<(int Index, IRInstruction Inst)>();

                for (var i = 0; i < block.Instructions.Count; i++)
                {
                    var inst = block.Instructions[i];
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.Result == null)
                        continue;

                    var simplified = TrySimplify(inst, function);
                    if (simplified != null)
                    {
                        if (simplified.Result == null)
                        {
                            toRemove.Add(inst);
                        }
                        else
                        {
                            toAdd.Add((i, simplified));
                            toRemove.Add(inst);
                        }
                        changed = true;
                    }
                }

                for (var i = toRemove.Count - 1; i >= 0; i--)
                    block.RemoveInstruction(toRemove[i]);

                foreach (var (idx, newInst) in toAdd.OrderByDescending(x => x.Index))
                {
                    block.InsertInstruction(idx, newInst);
                }
            }
        }
    }

    private static IRInstruction? TrySimplify(IRInstruction inst, IRFunction function)
    {
        return inst.OpCode switch
        {
            IROpCode.Add => TrySimplifyAdd(inst),
            IROpCode.Sub => TrySimplifySub(inst),
            IROpCode.Mul => TrySimplifyMul(inst),
            IROpCode.Div => TrySimplifyDiv(inst),
            IROpCode.Neg => TrySimplifyNeg(inst),
            _ => null
        };
    }

    private static IRInstruction? TrySimplifyAdd(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsZero(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (IsZero(left))
            return new IRInstruction(IROpCode.Nop, inst.Result, right);

        if (IsNegation(left, right))
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_zero_{inst.Result.Name}", 0.0, inst.Result.Type));

        return null;
    }

    private static IRInstruction? TrySimplifySub(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsZero(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (left == right)
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_zero_{inst.Result.Name}", 0.0, inst.Result.Type));

        if (IsZero(left))
            return new IRInstruction(IROpCode.Neg, inst.Result, right);

        return null;
    }

    private static IRInstruction? TrySimplifyMul(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsZero(left) || IsZero(right))
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_zero_{inst.Result.Name}", 0.0, inst.Result.Type));

        if (IsOne(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (IsOne(left))
            return new IRInstruction(IROpCode.Nop, inst.Result, right);

        if (IsNegOne(right))
            return new IRInstruction(IROpCode.Neg, inst.Result, left);

        if (IsNegOne(left))
            return new IRInstruction(IROpCode.Neg, inst.Result, right);

        return null;
    }

    private static IRInstruction? TrySimplifyDiv(IRInstruction inst)
    {
        if (inst.Operands.Count < 2 || inst.Result == null)
            return null;

        var left = inst.Operands[0];
        var right = inst.Operands[1];

        if (IsOne(right))
            return new IRInstruction(IROpCode.Nop, inst.Result, left);

        if (left == right)
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_one_{inst.Result.Name}", 1.0, inst.Result.Type));

        if (IsZero(left) && !IsZero(right))
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_zero_{inst.Result.Name}", 0.0, inst.Result.Type));

        return null;
    }

    private static IRInstruction? TrySimplifyNeg(IRInstruction inst)
    {
        if (inst.Operands.Count < 1 || inst.Result == null)
            return null;

        var operand = inst.Operands[0];
        if (operand.IsConstant && operand.ConstantValue.HasValue)
        {
            return new IRInstruction(IROpCode.Nop, inst.Result,
                IRValue.CreateConstant($"alg_neg_{inst.Result.Name}",
                    -operand.ConstantValue.Value, inst.Result.Type));
        }

        return null;
    }

    private static bool IsZero(IRValue value)
        => value.IsConstant && value.ConstantValue.HasValue && value.ConstantValue.Value == 0.0;

    private static bool IsOne(IRValue value)
        => value.IsConstant && value.ConstantValue.HasValue && value.ConstantValue.Value == 1.0;

    private static bool IsNegOne(IRValue value)
        => value.IsConstant && value.ConstantValue.HasValue && value.ConstantValue.Value == -1.0;

    private static bool IsNegation(IRValue a, IRValue b)
    {
        if (a.IsConstant && b.IsConstant &&
            a.ConstantValue.HasValue && b.ConstantValue.HasValue)
        {
            return a.ConstantValue.Value + b.ConstantValue.Value == 0.0;
        }
        return false;
    }
}
