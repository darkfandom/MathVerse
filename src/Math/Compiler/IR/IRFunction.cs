namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

public sealed class IRFunction
{
    public string Name { get; }
    public IRType ReturnType { get; }
    public IReadOnlyList<IRValue> Parameters { get; }
    public List<IRBlock> Blocks { get; }
    private int _blockCounter;

    public IRFunction(string name, IRType returnType, IEnumerable<IRValue>? parameters = null)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters?.ToImmutableArray() ?? ImmutableArray<IRValue>.Empty;
        Blocks = new List<IRBlock>();
    }

    public IRBlock CreateBlock(string? label = null)
    {
        label ??= $"bb{_blockCounter++}";
        var block = new IRBlock(label) { ParentFunction = this };
        Blocks.Add(block);
        return block;
    }

    public IRBlock GetEntryBlock()
    {
        if (Blocks.Count == 0)
            throw new InvalidOperationException("Function has no blocks");
        return Blocks[0];
    }

    public IRBlock? GetExitBlock()
    {
        for (var i = Blocks.Count - 1; i >= 0; i--)
        {
            if (Blocks[i].Terminator?.OpCode == IROpCode.Return)
                return Blocks[i];
        }
        return null;
    }

    public IEnumerable<IRValue> GetDefinedValues()
        => Blocks.SelectMany(b => b.GetDefinedValues());

    public IEnumerable<IRValue> GetUsedValues()
        => Blocks.SelectMany(b => b.GetUsedValues());

    public IEnumerable<IRPhiNode> GetAllPhiNodes()
        => Blocks.SelectMany(b => b.GetPhiNodes());

    public int ComputeTempRegisterCount()
        => GetDefinedValues().Count(v => !Parameters.Contains(v));

    public void RemoveDeadBlocks()
    {
        var reachable = new HashSet<IRBlock>();
        var worklist = new Stack<IRBlock>();
        var entry = GetEntryBlock();
        worklist.Push(entry);
        reachable.Add(entry);

        while (worklist.Count > 0)
        {
            var current = worklist.Pop();
            foreach (var succ in current.Successors)
            {
                if (reachable.Add(succ))
                    worklist.Push(succ);
            }
        }

        Blocks.RemoveAll(b => !reachable.Contains(b));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var paramStr = string.Join(", ", Parameters.Select(p => p.ToString()));
        sb.AppendLine($"func @{Name}({paramStr}) -> {IRTypeHelper.ToDisplayName(ReturnType)} {{");
        foreach (var block in Blocks)
            sb.Append(block);
        sb.AppendLine("}");
        return sb.ToString();
    }
}
