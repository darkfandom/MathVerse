namespace MathVerse.Math.Compiler.IR;

using System;
using System.Collections.Generic;
using System.Linq;

public static class SSAForm
{
    public static void ConvertToSSA(IRFunction function)
    {
        InsertPhiNodes(function);
        RenameVariablesInFunction(function);
    }

    private static void InsertPhiNodes(IRFunction function)
    {
        var cfg = new ControlFlowGraph(function);
        var dfg = new DataFlowGraph(function);
        var allDefs = new Dictionary<string, List<IRBlock>>();

        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                if (instruction.Result == null) continue;
                var name = instruction.Result.Name;
                if (!allDefs.ContainsKey(name))
                    allDefs[name] = new List<IRBlock>();
                if (!allDefs[name].Contains(block))
                    allDefs[name].Add(block);
            }
        }

        foreach (var (varName, defBlocks) in allDefs)
        {
            if (defBlocks.Count <= 1) continue;

            var worklist = new Queue<IRBlock>(defBlocks);
            var phiInserted = new HashSet<IRBlock>();

            while (worklist.Count > 0)
            {
                var block = worklist.Dequeue();
                if (!cfg.NodeMap.ContainsKey(block)) continue;

                foreach (var df in GetDominanceFrontier(cfg, block))
                {
                    if (phiInserted.Contains(df)) continue;

                    var phiType = GetVariableType(function, varName);
                    var phiResult = IRValue.CreateRegister($"{varName}_phi", phiType);
                    var incoming = df.Predecessors
                        .Where(p => p != null)
                        .Select(p => (IRValue.CreateRegister(varName, phiType), p!))
                        .ToArray();

                    if (incoming.Length > 0)
                    {
                        var phi = new IRPhiNode(phiResult, incoming);
                        df.AppendPhiNode(phi);
                        phiInserted.Add(df);

                        if (!defBlocks.Contains(df))
                            worklist.Enqueue(df);
                    }
                }
            }
        }
    }

    private static void RenameVariablesInFunction(IRFunction function)
    {
        var counters = new Dictionary<string, int>();
        var stacks = new Dictionary<string, Stack<IRValue>>();

        foreach (var block in function.Blocks)
        {
            foreach (var phi in block.PhiNodes)
            {
                if (phi.Result != null)
                {
                    var newName = GetOrNewName(counters, stacks, phi.Result.Name);
                    var newResult = IRValue.CreateRegister(newName, phi.Result.Type);
                    stacks[phi.Result.Name].Push(newResult);
                }
            }

            foreach (var instruction in block.Instructions)
            {
                if (instruction.Result != null)
                {
                    var newName = GetOrNewName(counters, stacks, instruction.Result.Name);
                    var newResult = IRValue.CreateRegister(newName, instruction.Result.Type);
                    stacks[instruction.Result.Name].Push(newResult);
                }
            }
        }

        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                for (var i = 0; i < instruction.Operands.Count; i++)
                {
                    var operand = instruction.Operands[i];
                    if (stacks.TryGetValue(operand.Name, out var stack) && stack.Count > 0)
                    {
                        var replacement = stack.Peek();
                        instruction.MutableOperands[i] = replacement;
                    }
                }
            }
        }
    }

    private static string GetOrNewName(
        Dictionary<string, int> counters,
        Dictionary<string, Stack<IRValue>> stacks,
        string baseName)
    {
        if (!counters.ContainsKey(baseName))
        {
            counters[baseName] = 0;
            stacks[baseName] = new Stack<IRValue>();
        }

        var version = counters[baseName]++;
        return $"{baseName}v{version}";
    }

    private static IEnumerable<IRBlock> GetDominanceFrontier(ControlFlowGraph cfg, IRBlock block)
    {
        if (cfg.NodeMap.TryGetValue(block, out var node))
            return node.DominanceFrontier.Select(n => n.Block);
        return Enumerable.Empty<IRBlock>();
    }

    private static IRType GetVariableType(IRFunction function, string varName)
    {
        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                if (instruction.Result != null && instruction.Result.Name == varName)
                    return instruction.Result.Type;
            }
        }

        foreach (var param in function.Parameters)
        {
            if (param.Name == varName)
                return param.Type;
        }

        return IRType.Float64;
    }

    public static void RemovePhiNodes(IRFunction function)
    {
        foreach (var block in function.Blocks.ToList())
        {
            block.Instructions.RemoveAll(i => i is IRPhiNode);
        }
    }
}
