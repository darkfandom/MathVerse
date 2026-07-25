namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DFGNode
{
    public IRValue Value { get; }
    public int Index { get; }
    public List<DFGNode> DefUsers { get; }
    public List<DFGNode> UseDefs { get; }

    public DFGNode(IRValue value, int index)
    {
        Value = value;
        Index = index;
        DefUsers = new List<DFGNode>();
        UseDefs = new List<DFGNode>();
    }
}

public sealed class DataFlowGraph
{
    public IReadOnlyList<DFGNode> Nodes => _nodes;
    public IReadOnlyDictionary<IRValue, DFGNode> ValueToNode => _valueToNode;

    private readonly List<DFGNode> _nodes = new();
    private readonly Dictionary<IRValue, DFGNode> _valueToNode = new();

    public DataFlowGraph(IRFunction function)
    {
        BuildFromFunction(function);
    }

    private void BuildFromFunction(IRFunction function)
    {
        var index = 0;
        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                if (instruction.Result != null)
                {
                    var node = new DFGNode(instruction.Result, index++);
                    _nodes.Add(node);
                    _valueToNode[instruction.Result] = node;
                }
            }
        }

        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                if (instruction.Result == null) continue;
                if (!_valueToNode.TryGetValue(instruction.Result, out var defNode)) continue;

                foreach (var operand in instruction.Operands)
                {
                    if (_valueToNode.TryGetValue(operand, out var useNode))
                    {
                        defNode.UseDefs.Add(useNode);
                        useNode.DefUsers.Add(defNode);
                    }
                }
            }
        }
    }

    public DFGNode? GetNode(IRValue value)
        => _valueToNode.TryGetValue(value, out var node) ? node : null;

    public bool IsUsed(IRValue value)
        => _valueToNode.TryGetValue(value, out var node) && node.DefUsers.Count > 0;

    public int GetUseCount(IRValue value)
        => _valueToNode.TryGetValue(value, out var node) ? node.DefUsers.Count : 0;

    public IEnumerable<IRValue> GetLiveIn(IRBlock block)
    {
        var live = new HashSet<IRValue>();
        foreach (var instruction in block.Instructions)
        {
            foreach (var operand in instruction.Operands)
            {
                if (!IsDefinedInBlock(operand, block))
                    live.Add(operand);
            }
        }
        return live;
    }

    public IEnumerable<IRValue> GetLiveOut(IRBlock block)
    {
        var live = new HashSet<IRValue>();
        var blockIndex = block.Instructions.Count > 0
            ? block.Instructions[0].SequenceIndex
            : 0;

        foreach (var otherBlock in block.ParentFunction?.Blocks ?? Enumerable.Empty<IRBlock>())
        {
            foreach (var instruction in otherBlock.Instructions)
            {
                if (instruction.SequenceIndex <= blockIndex) continue;
                foreach (var operand in instruction.Operands)
                    live.Add(operand);
            }
        }
        return live;
    }

    private static bool IsDefinedInBlock(IRValue value, IRBlock block)
        => block.Instructions.Any(i => i.Result != null && i.Result.Id == value.Id);

    public IEnumerable<IRValue> ComputeAllDefinedValues()
        => _nodes.Select(n => n.Value);

    public IEnumerable<IRValue> ComputeAllUsedValues()
    {
        var used = new HashSet<IRValue>();
        foreach (var node in _nodes)
        {
            foreach (var dep in node.UseDefs)
                used.Add(dep.Value);
        }
        return used;
    }

    public IEnumerable<IRValue> ComputeDeadValues()
    {
        var defined = new HashSet<IRValue>(_nodes.Select(n => n.Value));
        var used = new HashSet<IRValue>(ComputeAllUsedValues());
        return defined.Except(used);
    }
}
