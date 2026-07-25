namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class IRModule
{
    public string Name { get; }
    public List<IRFunction> Functions { get; }
    public List<IRGlobalConstant> GlobalConstants { get; }
    public Dictionary<string, string> Metadata { get; }
    private readonly object _lock = new();
    private int _tempCounter;

    public IReadOnlyList<IRInstruction> Instructions => GetAllInstructions();

    public IRModule() : this("unnamed") { }

    public IRModule(string name)
    {
        Name = name;
        Functions = new List<IRFunction>();
        GlobalConstants = new List<IRGlobalConstant>();
        Metadata = new Dictionary<string, string>();
    }

    public IRFunction CreateFunction(string name, IRType returnType, IEnumerable<IRValue>? parameters = null)
    {
        var func = new IRFunction(name, returnType, parameters);
        lock (_lock)
        {
            Functions.Add(func);
        }
        return func;
    }

    public void AddFunction(IRFunction function)
    {
        lock (_lock)
        {
            Functions.Add(function);
        }
    }

    public void Append(IRInstruction instruction)
    {
        lock (_lock)
        {
            if (Functions.Count > 0)
            {
                var lastFunc = Functions[^1];
                if (lastFunc.Blocks.Count > 0)
                {
                    lastFunc.Blocks[^1].Instructions.Add(instruction);
                    instruction.ParentBlock = lastFunc.Blocks[^1];
                    instruction.SequenceIndex = lastFunc.Blocks[^1].Instructions.Count - 1;
                }
            }
        }
    }

    public void AddGlobalConstant(string name, IRValue value)
    {
        lock (_lock)
        {
            GlobalConstants.Add(new IRGlobalConstant(name, value));
        }
    }

    public void SetMetadata(string key, string value)
    {
        lock (_lock)
        {
            Metadata[key] = value;
        }
    }

    public string? GetMetadata(string key)
        => Metadata.TryGetValue(key, out var value) ? value : null;

    public IRFunction? GetFunction(string name)
        => Functions.FirstOrDefault(f => f.Name == name);

    public int TotalInstructionCount()
        => Functions.Sum(f => f.Blocks.Sum(b => b.Instructions.Count));

    public int InstructionCount => TotalInstructionCount();

    public int TotalBlockCount()
        => Functions.Sum(f => f.Blocks.Count);

    public IRValue CreateTemp(IRType type = IRType.Float64)
        => IRValue.CreateRegister($"%t{_tempCounter++}", type);

    public IROperand CreateTemp(string debugName)
        => IROperand.CreateTemporary(_tempCounter++, debugName);

    public int NextTempId() => _tempCounter++;

    public void DeclareVariable(string name, IRType type)
    {
        GlobalConstants.Add(new IRGlobalConstant(name, IRValue.CreateConstant(0, type)));
    }

    public void DeclareVariable(string name)
    {
        GlobalConstants.Add(new IRGlobalConstant(name, IRValue.CreateConstant(0)));
    }

    public bool HasVariable(string name)
        => GlobalConstants.Any(c => c.Name == name);

    public IRModule Clone()
    {
        var clone = new IRModule(Name + "_clone");
        foreach (var kvp in Metadata)
            clone.Metadata[kvp.Key] = kvp.Value;
        foreach (var func in Functions)
            clone.Functions.Add(func);
        foreach (var c in GlobalConstants)
            clone.GlobalConstants.Add(new IRGlobalConstant(c.Name, c.Value));
        return clone;
    }

    public void RemoveDeadFunctions()
    {
        lock (_lock)
        {
            Functions.RemoveAll(f => f.Blocks.Count == 0);
        }
    }

    private IReadOnlyList<IRInstruction> GetAllInstructions()
    {
        var all = new List<IRInstruction>();
        foreach (var func in Functions)
            foreach (var block in func.Blocks)
                all.AddRange(block.Instructions);
        return all;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"module @{Name} {{");
        foreach (var c in GlobalConstants)
            sb.AppendLine($"  const {c.Name} = {c.Value}");
        sb.AppendLine();
        foreach (var func in Functions)
            sb.Append(func);
        sb.AppendLine("}");
        return sb.ToString();
    }
}

public sealed class IRGlobalConstant
{
    public string Name { get; }
    public IRValue Value { get; }

    public IRGlobalConstant(string name, IRValue value)
    {
        Name = name;
        Value = value;
    }
}
