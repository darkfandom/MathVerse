namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Removes instructions whose results are never used by any live instruction.
/// Performs iterative dead code elimination until no more dead instructions are found.
/// </summary>
public sealed class DeadCodeElimination : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "DeadCodeElimination";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            EliminateInFunction(function);
        return module;
    }

    private static void EliminateInFunction(IRFunction function)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            var liveValues = CollectLiveValues(function);
            var removed = 0;

            foreach (var block in function.Blocks)
            {
                var toRemove = new List<IRInstruction>();

                foreach (var inst in block.Instructions)
                {
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.HasSideEffects)
                        continue;
                    if (inst.IsTerminator)
                        continue;
                    if (inst.Result == null)
                        continue;

                    if (!liveValues.Contains(inst.Result))
                    {
                        toRemove.Add(inst);
                        removed++;
                    }
                }

                foreach (var inst in toRemove)
                    block.RemoveInstruction(inst);
            }

            if (removed > 0)
                changed = true;
        }
    }

    private static HashSet<IRValue> CollectLiveValues(IRFunction function)
    {
        var liveValues = new HashSet<IRValue>();

        foreach (var param in function.Parameters)
            liveValues.Add(param);

        foreach (var block in function.Blocks)
        {
            if (block.Terminator != null)
                MarkUsedValues(block.Terminator, liveValues);

            foreach (var inst in block.Instructions)
            {
                if (inst.HasSideEffects)
                    MarkUsedValues(inst, liveValues);
            }
        }

        var worklist = new Queue<IRValue>(liveValues);
        var processed = new HashSet<IRValue>();

        while (worklist.Count > 0)
        {
            var value = worklist.Dequeue();
            if (!processed.Add(value))
                continue;

            var definer = FindDefinition(function, value);
            if (definer == null)
                continue;

            if (definer is IRPhiNode phi)
            {
                foreach (var (val, block) in phi.IncomingEdges)
                {
                    if (processed.Add(val) || liveValues.Add(val))
                        worklist.Enqueue(val);
                }
            }
            else
            {
                foreach (var operand in definer.Operands)
                {
                    if (processed.Add(operand) || liveValues.Add(operand))
                        worklist.Enqueue(operand);
                }
            }
        }

        return liveValues;
    }

    private static void MarkUsedValues(IRInstruction inst, HashSet<IRValue> liveValues)
    {
        foreach (var operand in inst.Operands)
        {
            liveValues.Add(operand);
        }
    }

    private static IRInstruction? FindDefinition(IRFunction function, IRValue value)
    {
        foreach (var block in function.Blocks)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst.Result != null && inst.Result.Id == value.Id)
                    return inst;
            }
        }
        return null;
    }
}
