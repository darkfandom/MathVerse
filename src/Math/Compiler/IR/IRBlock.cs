namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class IRBlock
{
    public string Label { get; }
    public List<IRInstruction> Instructions { get; }
    public IRInstruction? Terminator { get; internal set; }
    public IRFunction? ParentFunction { get; internal set; }

    public IReadOnlyList<IRBlock> Predecessors => _predecessors;
    private readonly List<IRBlock> _predecessors = new();

    public IReadOnlyList<IRBlock> Successors => ComputeSuccessors();

    public IRBlock(string label)
    {
        Label = label;
        Instructions = new List<IRInstruction>();
    }

    public void AppendInstruction(IRInstruction instruction)
    {
        instruction.ParentBlock = this;
        instruction.SequenceIndex = Instructions.Count;
        if (instruction.IsTerminator)
            Terminator = instruction;
        Instructions.Add(instruction);
    }

    public void InsertInstruction(int index, IRInstruction instruction)
    {
        instruction.ParentBlock = this;
        instruction.SequenceIndex = index;
        Instructions.Insert(index, instruction);
        for (var i = index; i < Instructions.Count; i++)
            Instructions[i].SequenceIndex = i;
        if (instruction.IsTerminator)
            Terminator = instruction;
    }

    public void RemoveInstruction(IRInstruction instruction)
    {
        Instructions.Remove(instruction);
        if (Terminator == instruction)
            Terminator = Instructions.LastOrDefault(i => i.IsTerminator);
        for (var i = 0; i < Instructions.Count; i++)
            Instructions[i].SequenceIndex = i;
    }

    public void AddPredecessor(IRBlock block)
    {
        if (!_predecessors.Contains(block))
            _predecessors.Add(block);
    }

    public void RemovePredecessor(IRBlock block) => _predecessors.Remove(block);

    public void ReplacePredecessor(IRBlock oldPred, IRBlock newPred)
    {
        var idx = _predecessors.IndexOf(oldPred);
        if (idx >= 0) _predecessors[idx] = newPred;
    }

    public bool IsEmpty => Instructions.Count == 0;

    public bool IsTerminated => Terminator != null;

    public IEnumerable<IRValue> GetDefinedValues()
        => Instructions.Where(i => i.Result != null).Select(i => i.Result!);

    public IEnumerable<IRValue> GetUsedValues()
        => Instructions.SelectMany(i => i.Operands);

    public IEnumerable<IRPhiNode> GetPhiNodes()
        => Instructions.OfType<IRPhiNode>();

    public List<IRPhiNode> PhiNodes => Instructions.OfType<IRPhiNode>().ToList();

    public void AppendPhiNode(IRPhiNode phi)
    {
        phi.ParentBlock = this;
        Instructions.Insert(0, phi);
    }

    public IReadOnlyList<IRBlock> ComputeSuccessors()
    {
        return Array.Empty<IRBlock>();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"  {Label}:");
        foreach (var inst in Instructions)
            sb.AppendLine($"    {inst}");
        return sb.ToString();
    }
}
