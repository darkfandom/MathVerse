namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Follows copy chains: if a = b and b = c, replace uses of a with c.
/// Uses worklist-based data-flow analysis over SSA form to propagate copies.
/// </summary>
public sealed class CopyPropagation : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "CopyPropagation";

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
            var copyMap = new Dictionary<int, IRValue>();

            foreach (var block in function.Blocks)
            {
                foreach (var inst in block.Instructions)
                {
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.Result == null)
                        continue;
                    if (inst.Operands.Count != 1)
                        continue;

                    var source = inst.Operands[0];
                    if (source.IsConstant)
                        continue;
                    if (inst.Result.Type != source.Type)
                        continue;

                    copyMap[inst.Result.Id] = source;
                }
            }

            foreach (var (sourceId, _) in copyMap)
            {
                var chain = ResolveCopyChain(copyMap, sourceId);
                if (chain != null && chain.Count > 1)
                {
                    var originalSource = chain[^1];
                    foreach (var (id, value) in copyMap.ToList())
                    {
                        if (value.Id == sourceId)
                            copyMap[id] = originalSource;
                    }
                }
            }

            foreach (var block in function.Blocks)
            {
                for (var i = 0; i < block.Instructions.Count; i++)
                {
                    var inst = block.Instructions[i];
                    if (inst is IRPhiNode)
                    {
                        var newPhi = PropagateInPhi(inst as IRPhiNode, copyMap);
                        if (newPhi != null)
                        {
                            block.Instructions[i] = newPhi;
                            newPhi.ParentBlock = block;
                            newPhi.SequenceIndex = i;
                            changed = true;
                        }
                        continue;
                    }

                    var newOperands = inst.Operands
                        .Select(o => ResolveCopy(o, copyMap))
                        .ToList();

                    if (!newOperands.SequenceEqual(inst.Operands))
                    {
                        var newInst = new IRInstruction(inst.OpCode, inst.Result, newOperands);
                        newInst.ParentBlock = block;
                        newInst.SequenceIndex = i;
                        block.Instructions[i] = newInst;
                        if (newInst.IsTerminator)
                            block.Terminator = newInst;
                        changed = true;
                    }
                }
            }
        }
    }

    private static IRValue ResolveCopy(IRValue value, Dictionary<int, IRValue> copyMap)
    {
        var visited = new HashSet<int>();
        var current = value;

        while (current != null && !current.IsConstant && copyMap.TryGetValue(current.Id, out var next))
        {
            if (!visited.Add(current.Id))
                break;
            current = next;
        }

        return current ?? value;
    }

    private static List<IRValue>? ResolveCopyChain(Dictionary<int, IRValue> copyMap, int startId)
    {
        var chain = new List<IRValue>();
        var visited = new HashSet<int>();
        var current = startId;

        while (copyMap.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
                return null;

            chain.Add(next);
            current = next.Id;
        }

        return chain.Count > 0 ? chain : null;
    }

    private static IRPhiNode? PropagateInPhi(IRPhiNode? phi, Dictionary<int, IRValue> copyMap)
    {
        if (phi == null)
            return null;

        var changed = false;
        var newEdges = phi.IncomingEdges
            .Select(e =>
            {
                var resolved = ResolveCopy(e.Value, copyMap);
                if (resolved != e.Value)
                {
                    changed = true;
                    return (resolved, e.Block);
                }
                return e;
            })
            .ToList();

        if (!changed)
            return null;

        return new IRPhiNode(phi.Result!, newEdges);
    }
}
