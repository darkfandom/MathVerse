namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed class CFGNode
{
    public IRBlock Block { get; }
    public int ReversePostOrderIndex { get; set; } = -1;
    public HashSet<CFGNode> DominatedBy { get; }
    public CFGNode? IDom { get; set; }
    public HashSet<CFGNode> DominanceFrontier { get; }

    public CFGNode(IRBlock block)
    {
        Block = block;
        DominatedBy = new HashSet<CFGNode>();
        DominanceFrontier = new HashSet<CFGNode>();
    }
}

public sealed class ControlFlowGraph
{
    public IReadOnlyList<IRBlock> Blocks => _blocks;
    public IRBlock EntryBlock { get; }
    public IRBlock? ExitBlock { get; }
    public IReadOnlyDictionary<IRBlock, CFGNode> NodeMap => _nodeMap;

    private readonly List<IRBlock> _blocks;
    private readonly Dictionary<IRBlock, CFGNode> _nodeMap = new();

    public ControlFlowGraph(IRFunction function)
    {
        _blocks = new List<IRBlock>(function.Blocks);
        EntryBlock = function.GetEntryBlock();
        ExitBlock = function.GetExitBlock();

        foreach (var block in _blocks)
            _nodeMap[block] = new CFGNode(block);

        BuildEdges();
        ComputeDominators();
        ComputeDominanceFrontiers();
    }

    private void BuildEdges()
    {
        foreach (var block in _blocks)
        {
            foreach (var succ in block.Successors)
            {
                if (_nodeMap.TryGetValue(succ, out var succNode))
                {
                    succNode.Block.AddPredecessor(block);
                }
            }
        }
    }

    public IReadOnlyList<CFGNode> ComputeDominators()
    {
        if (_blocks.Count == 0)
            return Array.Empty<CFGNode>();

        var nodes = _blocks.Select(b => _nodeMap[b]).ToList();
        var entryNode = _nodeMap[EntryBlock];

        foreach (var node in nodes)
            node.DominatedBy.Clear();
        entryNode.DominatedBy.UnionWith(nodes);

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var node in nodes)
            {
                if (node == entryNode) continue;

                CFGNode? newIDom = null;
                var predecessors = node.Block.Predecessors
                    .Where(p => _nodeMap.ContainsKey(p))
                    .Select(p => _nodeMap[p])
                    .ToList();

                if (predecessors.Count == 0) continue;

                newIDom = predecessors[0];
                foreach (var pred in predecessors.Skip(1))
                {
                    if (node.DominatedBy.Contains(pred))
                        newIDom = Intersect(newIDom!, pred, entryNode);
                }

                if (newIDom != null && node.IDom != newIDom)
                {
                    node.IDom = newIDom;
                    node.DominatedBy.Clear();
                    node.DominatedBy.Add(newIDom);
                    node.DominatedBy.UnionWith(newIDom.DominatedBy);
                    changed = true;
                }
            }
        }

        return nodes;
    }

    private static CFGNode? Intersect(CFGNode b1, CFGNode b2, CFGNode entryNode)
    {
        var finger1 = b1;
        var finger2 = b2;

        while (finger1 != finger2)
        {
            while (GetPostOrderIndex(finger1) > GetPostOrderIndex(finger2))
                finger1 = finger1.IDom ?? entryNode;
            while (GetPostOrderIndex(finger2) > GetPostOrderIndex(finger1))
                finger2 = finger2.IDom ?? entryNode;
        }

        return finger1;
    }

    private static int GetPostOrderIndex(CFGNode node)
        => node.ReversePostOrderIndex >= 0 ? node.ReversePostOrderIndex : 0;

    public IReadOnlyList<CFGNode> ComputeDFOrder()
    {
        var nodes = _blocks.Select(b => _nodeMap[b]).ToList();
        var visited = new HashSet<CFGNode>();
        var result = new List<CFGNode>();

        ReversePostOrder(EntryBlock, visited, result);
        result.Reverse();

        for (var i = 0; i < result.Count; i++)
            result[i].ReversePostOrderIndex = i;

        return result;
    }

    private void ReversePostOrder(IRBlock block, HashSet<CFGNode> visited, List<CFGNode> result)
    {
        if (!_nodeMap.TryGetValue(block, out var node)) return;
        if (!visited.Add(node)) return;

        foreach (var succ in block.Successors)
            ReversePostOrder(succ, visited, result);

        result.Add(node);
    }

    private void ComputeDominanceFrontiers()
    {
        var nodes = _blocks.Select(b => _nodeMap[b]).ToList();
        foreach (var node in nodes)
            node.DominanceFrontier.Clear();

        foreach (var node in nodes)
        {
            var predecessors = node.Block.Predecessors
                .Where(p => _nodeMap.ContainsKey(p))
                .Select(p => _nodeMap[p])
                .ToList();

            if (predecessors.Count < 2) continue;

            foreach (var pred in predecessors)
            {
                var runner = pred;
                while (runner != node.IDom && runner != null)
                {
                    runner.DominanceFrontier.Add(node);
                    runner = runner.IDom!;
                }
            }
        }
    }

    public bool Dominates(IRBlock dominator, IRBlock dominated)
    {
        if (!_nodeMap.TryGetValue(dominator, out var domNode)) return false;
        if (!_nodeMap.TryGetValue(dominated, out var domedNode)) return false;
        return domedNode.DominatedBy.Contains(domNode);
    }

    public bool IsBackEdge(IRBlock from, IRBlock to)
    {
        if (!_nodeMap.TryGetValue(from, out var fromNode)) return false;
        if (!_nodeMap.TryGetValue(to, out var toNode)) return false;
        return toNode.ReversePostOrderIndex < fromNode.ReversePostOrderIndex;
    }
}
