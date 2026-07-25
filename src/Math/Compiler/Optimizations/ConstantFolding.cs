namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Pre-computes constant expressions at compile time. More aggressive than
/// constant propagation — evaluates entire constant subexpressions including
/// nested chains of constant computations.
/// </summary>
public sealed class ConstantFolding : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "ConstantFolding";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            FoldInFunction(function);
        return module;
    }

    private static void FoldInFunction(IRFunction function)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            var constantMap = new Dictionary<int, double>();

            foreach (var param in function.Parameters)
            {
                if (param.IsConstant && param.ConstantValue.HasValue)
                    constantMap[param.Id] = param.ConstantValue.Value;
            }

            foreach (var block in function.Blocks)
            {
                var toRemove = new List<IRInstruction>();

                for (var i = 0; i < block.Instructions.Count; i++)
                {
                    var inst = block.Instructions[i];
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.Result == null)
                        continue;

                    if (inst.Operands.Count == 0 && !inst.HasSideEffects)
                        continue;

                    if (TryFoldInstruction(inst, constantMap, out var result))
                    {
                        var resultType = inst.Result.Type;
                        var folded = IRValue.CreateConstant(
                            $"fold_{inst.Result.Name}", result, resultType);
                        constantMap[inst.Result.Id] = result;

                        ReplaceAllUsesInFunction(function, inst.Result, folded);

                        if (!inst.HasSideEffects && !inst.IsTerminator)
                            toRemove.Add(inst);

                        changed = true;
                    }
                }

                foreach (var inst in toRemove)
                {
                    if (inst.ParentBlock != null)
                        inst.ParentBlock.RemoveInstruction(inst);
                }
            }
        }
    }

    private static bool TryFoldInstruction(
        IRInstruction inst,
        Dictionary<int, double> constantMap,
        out double result)
    {
        result = 0;

        switch (inst.OpCode)
        {
            case IROpCode.Add:
            case IROpCode.Sub:
            case IROpCode.Mul:
            case IROpCode.Div:
            case IROpCode.Mod:
                return TryFoldBinaryArithmetic(inst, constantMap, out result);

            case IROpCode.Neg:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var negVal))
                {
                    result = -negVal;
                    return true;
                }
                return false;

            case IROpCode.Abs:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var absVal))
                {
                    result = Math.Abs(absVal);
                    return true;
                }
                return false;

            case IROpCode.Sqrt:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var sqrtVal))
                {
                    result = Math.Sqrt(sqrtVal);
                    return true;
                }
                return false;

            case IROpCode.Sin:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var sinVal))
                {
                    result = Math.Sin(sinVal);
                    return true;
                }
                return false;

            case IROpCode.Cos:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var cosVal))
                {
                    result = Math.Cos(cosVal);
                    return true;
                }
                return false;

            case IROpCode.Tan:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var tanVal))
                {
                    result = Math.Tan(tanVal);
                    return true;
                }
                return false;

            case IROpCode.Log:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var logVal))
                {
                    result = Math.Log(logVal);
                    return true;
                }
                return false;

            case IROpCode.Exp:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var expVal))
                {
                    result = Math.Exp(expVal);
                    return true;
                }
                return false;

            case IROpCode.Pow:
                if (inst.Operands.Count >= 2 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var baseVal) &&
                    ResolveConstant(inst.Operands[1], constantMap, out var powExpVal))
                {
                    result = Math.Pow(baseVal, powExpVal);
                    return true;
                }
                return false;

            case IROpCode.Cast:
                if (inst.Operands.Count >= 1 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var castVal))
                {
                    result = castVal;
                    return true;
                }
                return false;

            case IROpCode.Fma:
                if (inst.Operands.Count >= 3 &&
                    ResolveConstant(inst.Operands[0], constantMap, out var fmaA) &&
                    ResolveConstant(inst.Operands[1], constantMap, out var fmaB) &&
                    ResolveConstant(inst.Operands[2], constantMap, out var fmaC))
                {
                    result = fmaA * fmaB + fmaC;
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static bool TryFoldBinaryArithmetic(
        IRInstruction inst,
        Dictionary<int, double> constantMap,
        out double result)
    {
        result = 0;
        if (inst.Operands.Count < 2)
            return false;

        if (!ResolveConstant(inst.Operands[0], constantMap, out var left) ||
            !ResolveConstant(inst.Operands[1], constantMap, out var right))
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

    private static bool ResolveConstant(
        IRValue value,
        Dictionary<int, double> constantMap,
        out double result)
    {
        if (value.IsConstant && value.ConstantValue.HasValue)
        {
            result = value.ConstantValue.Value;
            return true;
        }

        if (constantMap.TryGetValue(value.Id, out var val))
        {
            result = val;
            return true;
        }

        result = 0;
        return false;
    }

    private static void ReplaceAllUsesInFunction(
        IRFunction function, IRValue oldValue, IRValue newValue)
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
