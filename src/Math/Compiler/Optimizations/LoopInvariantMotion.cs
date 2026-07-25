namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// For loops identified by back-edges in the control flow graph, moves invariant
/// computations (those whose inputs are all defined outside the loop) out of the
/// loop body into the loop preheader.
/// </summary>
public sealed class LoopInvariantMotion : IOptimizationPass
{
    /// <inheritdoc />
    public string Name => "LoopInvariantMotion";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        foreach (var function in module.Functions)
            OptimizeFunction(function);
        return module;
    }

    private static void OptimizeFunction(IRFunction function)
    {
        if (function.Blocks.Count < 2)
            return;

        var domInfo = ComputeDominators(function);
        var loops = DetectLoops(function, domInfo);

        foreach (var loop in loops)
        {
            var preheader = FindOrCreatePreheader(function, loop, domInfo);
            if (preheader == null)
                continue;

            var loopBodySet = new HashSet<IRBlock>(loop.Body);
            var loopDefinedValues = CollectDefinedValues(loopBodySet);

            var loopDefinedInLoop = new HashSet<int>(loopDefinedValues.Select(v => v.Id));
            var loopDefinedInBodyPlusPreheader = new HashSet<int>(loopDefinedInLoop);

            foreach (var inst in preheader.Instructions)
            {
                if (inst.Result != null)
                    loopDefinedInBodyPlusPreheader.Add(inst.Result.Id);
            }
            if (preheader.Terminator?.Result != null)
                loopDefinedInBodyPlusPreheader.Add(preheader.Terminator.Result.Id);

            var movable = new List<IRInstruction>();

            foreach (var block in loop.Body)
            {
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
                    if (inst.OpCode is IROpCode.Load or IROpCode.Store or IROpCode.Alloc)
                        continue;

                    if (IsLoopInvariant(inst, loopDefinedInBodyPlusPreheader, loop))
                    {
                        movable.Add(inst);
                    }
                }
            }

            foreach (var inst in movable)
            {
                if (inst.ParentBlock == null)
                    continue;

                inst.ParentBlock.RemoveInstruction(inst);
                inst.ParentBlock = null;
                inst.SequenceIndex = -1;
                preheader.InsertInstruction(preheader.Instructions.Count, inst);
            }

            foreach (var block in loop.Body)
            {
                foreach (var inst in block.Instructions)
                {
                    if (inst is IRPhiNode)
                        continue;
                    if (inst.Result == null)
                        continue;

                    var key = inst.Result.Id;
                    if (loopDefinedInLoop.Contains(key))
                        loopDefinedInBodyPlusPreheader.Add(key);
                }
            }
        }
    }

    private static bool IsLoopInvariant(
        IRInstruction inst,
        HashSet<int> definedOutsideLoop,
        LoopInfo loop)
    {
        foreach (var operand in inst.Operands)
        {
            if (operand.IsConstant)
                continue;

            if (!definedOutsideLoop.Contains(operand.Id))
                return false;
        }
        return true;
    }

    private static HashSet<IRValue> CollectDefinedValues(HashSet<IRBlock> blocks)
    {
        var defined = new HashSet<IRValue>();
        foreach (var block in blocks)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst.Result != null)
                    defined.Add(inst.Result);
            }
        }
        return defined;
    }

    private static IRBlock? FindOrCreatePreheader(
        IRFunction function,
        LoopInfo loop,
        Dictionary<IRBlock, DomInfo> domInfo)
    {
        IRBlock? bestPreheader = null;
        var bestIdomScore = int.MaxValue;

        foreach (var pred in loop.Header.Predecessors)
        {
            if (loop.Body.Contains(pred))
                continue;

            if (domInfo.TryGetValue(pred, out var predDom))
            {
                if (predDom.Dominates(loop.Header))
                {
                    if (bestPreheader == null || predDom.Index < bestIdomScore)
                    {
                        bestPreheader = pred;
                        bestIdomScore = predDom.Index;
                    }
                }
            }
        }

        return bestPreheader;
    }

    private static Dictionary<IRBlock, DomInfo> ComputeDominators(IRFunction function)
    {
        var blocks = function.Blocks;
        if (blocks.Count == 0)
            return new Dictionary<IRBlock, DomInfo>();

        var blockList = new List<IRBlock>(blocks);
        var blockIndex = new Dictionary<IRBlock, int>();
        for (var i = 0; i < blockList.Count; i++)
            blockIndex[blockList[i]] = i;

        var allBlocks = new HashSet<IRBlock>(blockList);
        var dom = new HashSet<IRBlock>[blockList.Count];
        for (var i = 0; i < blockList.Count; i++)
        {
            dom[i] = new HashSet<IRBlock>(allBlocks);
        }

        var entryIdx = 0;
        dom[entryIdx].Clear();
        dom[entryIdx].Add(blockList[entryIdx]);

        var changed = true;
        while (changed)
        {
            changed = false;
            for (var i = 1; i < blockList.Count; i++)
            {
                var block = blockList[i];
                var predecessors = block.Predecessors
                    .Where(p => blockIndex.ContainsKey(p))
                    .Select(p => blockIndex[p])
                    .ToList();

                if (predecessors.Count == 0)
                    continue;

                var newDom = new HashSet<IRBlock>(dom[predecessors[0]]);
                for (var j = 1; j < predecessors.Count; j++)
                    newDom.IntersectWith(dom[predecessors[j]]);

                newDom.Add(block);

                if (!newDom.SetEquals(dom[i]))
                {
                    dom[i] = newDom;
                    changed = true;
                }
            }
        }

        var result = new Dictionary<IRBlock, DomInfo>();
        for (var i = 0; i < blockList.Count; i++)
        {
            result[blockList[i]] = new DomInfo(i, new HashSet<IRBlock>(dom[i]));
        }
        return result;
    }

    private static List<LoopInfo> DetectLoops(
        IRFunction function,
        Dictionary<IRBlock, DomInfo> domInfo)
    {
        var loops = new List<LoopInfo>();
        var visited = new HashSet<IRBlock>();
        var inStack = new HashSet<IRBlock>();
        var stack = new List<IRBlock>();

        foreach (var block in function.Blocks)
        {
            if (!visited.Contains(block))
                FindLoopsDfs(block, function, domInfo, visited, inStack, stack, loops);
        }

        return loops;
    }

    private static void FindLoopsDfs(
        IRBlock current,
        IRFunction function,
        Dictionary<IRBlock, DomInfo> domInfo,
        HashSet<IRBlock> visited,
        HashSet<IRBlock> inStack,
        List<IRBlock> stack,
        List<LoopInfo> loops)
    {
        visited.Add(current);
        inStack.Add(current);
        stack.Add(current);

        foreach (var succ in current.Successors)
        {
            if (succ == null || !function.Blocks.Contains(succ))
                continue;

            if (inStack.Contains(succ))
            {
                var header = succ;
                var body = new HashSet<IRBlock>();

                for (var i = stack.Count - 1; i >= 0; i--)
                {
                    body.Add(stack[i]);
                    if (stack[i] == header)
                        break;
                }

                if (body.Count > 1)
                    loops.Add(new LoopInfo(header, body));
            }
            else if (!visited.Contains(succ))
            {
                FindLoopsDfs(succ, function, domInfo, visited, inStack, stack, loops);
            }
        }

        inStack.Remove(current);
        stack.RemoveAt(stack.Count - 1);
    }

    private sealed class DomInfo
    {
        public int Index { get; }
        public HashSet<IRBlock> DominanceSet { get; }

        public DomInfo(int index, HashSet<IRBlock> dominanceSet)
        {
            Index = index;
            DominanceSet = dominanceSet;
        }

        public bool Dominates(IRBlock block) => DominanceSet.Contains(block);
    }

    private sealed class LoopInfo
    {
        public IRBlock Header { get; }
        public HashSet<IRBlock> Body { get; }

        public LoopInfo(IRBlock header, HashSet<IRBlock> body)
        {
            Header = header;
            Body = body;
        }
    }
}
