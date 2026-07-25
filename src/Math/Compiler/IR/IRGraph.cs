namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed class IRGraphNode
{
    public IRInstruction Instruction { get; }
    public int Index { get; }
    public List<IRGraphNode> DefUseEdges { get; }
    public List<IRGraphNode> UseDefEdges { get; }

    public IRGraphNode(IRInstruction instruction, int index)
    {
        Instruction = instruction;
        Index = index;
        DefUseEdges = new List<IRGraphNode>();
        UseDefEdges = new List<IRGraphNode>();
    }
}

public sealed class IRGraph
{
    public IReadOnlyList<IRGraphNode> Nodes => _nodes;
    public IReadOnlyDictionary<IRValue, IRGraphNode> ValueToNode => _valueToNode;
    private readonly List<IRGraphNode> _nodes = new();
    private readonly Dictionary<IRValue, IRGraphNode> _valueToNode = new();
    private readonly Dictionary<IRValue, List<IRGraphNode>> _defMap = new();

    public IRGraph(IRFunction function)
    {
        BuildFromFunction(function);
    }

    private void BuildFromFunction(IRFunction function)
    {
        var allInstructions = new List<IRInstruction>();
        foreach (var block in function.Blocks)
        {
            allInstructions.AddRange(block.Instructions);
            if (block.Terminator != null)
                allInstructions.Add(block.Terminator);
        }

        for (var i = 0; i < allInstructions.Count; i++)
        {
            var node = new IRGraphNode(allInstructions[i], i);
            _nodes.Add(node);

            if (allInstructions[i].Result is { } result)
            {
                _valueToNode[result] = node;
                if (!_defMap.ContainsKey(result))
                    _defMap[result] = new List<IRGraphNode>();
                _defMap[result].Add(node);
            }
        }

        foreach (var node in _nodes)
        {
            foreach (var operand in node.Instruction.Operands)
            {
                if (_valueToNode.TryGetValue(operand, out var defNode))
                {
                    node.UseDefEdges.Add(defNode);
                    defNode.DefUseEdges.Add(node);
                }
            }
        }
    }

    public IRGraphNode? GetDefiningNode(IRValue value)
        => _valueToNode.TryGetValue(value, out var node) ? node : null;

    public IReadOnlyList<IRGraphNode> GetUsers(IRValue value)
        => _defMap.TryGetValue(value, out var users) ? users : Array.Empty<IRGraphNode>();

    public int ComputeLiveRange(IRValue value)
    {
        if (!_valueToNode.TryGetValue(value, out var defNode))
            return -1;

        var maxUse = defNode.Index;
        var worklist = new Stack<IRGraphNode>();
        foreach (var user in defNode.DefUseEdges)
            worklist.Push(user);

        var visited = new HashSet<IRGraphNode> { defNode };
        while (worklist.Count > 0)
        {
            var current = worklist.Pop();
            if (!visited.Add(current)) continue;
            if (current.Index > maxUse)
                maxUse = current.Index;
            foreach (var user in current.DefUseEdges)
            {
                if (!visited.Contains(user))
                    worklist.Push(user);
            }
        }

        return maxUse - defNode.Index;
    }

    public bool IsReachable(IRGraphNode from, IRGraphNode to)
    {
        if (from == to) return true;
        var visited = new HashSet<IRGraphNode>();
        var worklist = new Stack<IRGraphNode>();
        worklist.Push(from);

        while (worklist.Count > 0)
        {
            var current = worklist.Pop();
            if (!visited.Add(current)) continue;
            if (current == to) return true;
            foreach (var succ in current.DefUseEdges)
            {
                if (!visited.Contains(succ))
                    worklist.Push(succ);
            }
        }
        return false;
    }
}
