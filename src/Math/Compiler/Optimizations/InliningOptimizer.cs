namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Inlines small functions (fewer than a configurable instruction threshold) by
/// replacing Call instructions with the inlined function body. Only inlines
/// leaf functions with simple, non-recursive bodies.
/// </summary>
public sealed class InliningOptimizer : IOptimizationPass
{
    private readonly int _instructionThreshold;

    /// <summary>
    /// Initializes the inlining optimizer with a configurable instruction threshold.
    /// </summary>
    /// <param name="instructionThreshold">Maximum number of instructions in a function to allow inlining.</param>
    public InliningOptimizer(int instructionThreshold = 32)
    {
        _instructionThreshold = instructionThreshold;
    }

    /// <inheritdoc />
    public string Name => "InliningOptimizer";

    /// <inheritdoc />
    public IRModule Optimize(IRModule module)
    {
        var inlineableFunctions = IdentifyInlineableFunctions(module);
        if (inlineableFunctions.Count == 0)
            return module;

        foreach (var function in module.Functions)
            InlineInFunction(function, inlineableFunctions);

        return module;
    }

    private Dictionary<string, IRFunction> IdentifyInlineableFunctions(IRModule module)
    {
        var result = new Dictionary<string, IRFunction>();
        var visited = new HashSet<string>();

        foreach (var function in module.Functions)
        {
            if (IsInlineableFunction(function, module, visited))
                result[function.Name] = function;
        }

        return result;
    }

    private bool IsInlineableFunction(
        IRFunction function,
        IRModule module,
        HashSet<string> visited)
    {
        if (!visited.Add(function.Name))
            return false;

        var totalInstructions = function.Blocks.Sum(b => b.Instructions.Count);
        if (totalInstructions > _instructionThreshold)
            return false;

        if (function.ReturnType == IRType.Void)
            return false;

        foreach (var block in function.Blocks)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst is IRPhiNode)
                    return false;

                if (inst.OpCode == IROpCode.Call)
                {
                    var calledName = GetCalledFunctionName(inst);
                    if (calledName != null && module.GetFunction(calledName) != null)
                    {
                        if (!visited.Contains(calledName))
                        {
                            var callee = module.GetFunction(calledName);
                            if (callee != null && !IsInlineableFunction(callee, module, visited))
                                return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private static string? GetCalledFunctionName(IRInstruction inst)
    {
        if (inst.OpCode != IROpCode.Call)
            return null;

        var firstNonBlock = inst.Operands.FirstOrDefault();
        if (firstNonBlock != null && !firstNonBlock.IsConstant)
            return firstNonBlock.Name;

        return null;
    }

    private static void InlineInFunction(
        IRFunction function,
        Dictionary<string, IRFunction> inlineableFunctions)
    {
        foreach (var block in function.Blocks)
        {
            var i = 0;
            while (i < block.Instructions.Count)
            {
                var inst = block.Instructions[i];
                if (inst.OpCode == IROpCode.Call && inst.Result != null)
                {
                    var calledName = GetCalledFunctionName(inst);
                    if (calledName != null && inlineableFunctions.TryGetValue(calledName, out var callee))
                    {
                        var args = GetCallArguments(inst);
                        if (args.Count == callee.Parameters.Count)
                        {
                            var inlinedBlock = InlineFunction(
                                callee, args, inst.Result, function);
                            ReplaceCallWithInlined(block, i, inlinedBlock, inst.Result);
                            continue;
                        }
                    }
                }
                i++;
            }
        }
    }

    private static List<IRValue> GetCallArguments(IRInstruction callInst)
    {
        var args = new List<IRValue>();
        foreach (var operand in callInst.Operands)
        {
            args.Add(operand);
        }
        return args;
    }

    private static List<(string Label, List<IRInstruction> Instructions, IRInstruction? Terminator)>
        InlineFunction(
            IRFunction callee,
            List<IRValue> args,
            IRValue resultPlaceholder,
            IRFunction targetFunction)
    {
        var labelMap = new Dictionary<string, string>();
        foreach (var block in callee.Blocks)
        {
            labelMap[block.Label] = $"inl_{callee.Name}_{block.Label}";
        }

        var paramMapping = new Dictionary<int, IRValue>();
        for (var i = 0; i < Math.Min(args.Count, callee.Parameters.Count); i++)
        {
            paramMapping[callee.Parameters[i].Id] = args[i];
        }

        var inlinedBlocks = new List<(string Label, List<IRInstruction> Instructions, IRInstruction? Terminator)>();

        foreach (var block in callee.Blocks)
        {
            var newLabel = labelMap[block.Label];
            var newInstructions = new List<IRInstruction>();

            foreach (var inst in block.Instructions)
            {
                if (inst is IRPhiNode)
                    continue;

                var newOperands = inst.Operands
                    .Select(o => paramMapping.TryGetValue(o.Id, out var mapped)
                            ? mapped
                            : o)
                    .ToList();

                var newResult = inst.Result != null
                    ? IRValue.CreateRegister(
                        $"inl_{inst.Result.Name}", inst.Result.Type)
                    : null;

                if (inst.OpCode == IROpCode.Return && newOperands.Count > 0 && inst.Operands.Count > 0)
                {
                    var retVal = paramMapping.TryGetValue(inst.Operands[0].Id, out var mappedRet)
                        ? mappedRet
                        : inst.Operands[0];

                    newResult = resultPlaceholder;
                    newInstructions.Add(new IRInstruction(
                        IROpCode.Nop, resultPlaceholder, retVal));
                }
                else
                {
                    newInstructions.Add(new IRInstruction(inst.OpCode, newResult, newOperands));
                }
            }

            var newTerminator = block.Terminator != null
                ? newInstructions.LastOrDefault()
                : null;

            inlinedBlocks.Add((newLabel, newInstructions, null));
        }

        return inlinedBlocks;
    }

    private static void ReplaceCallWithInlined(
        IRBlock block,
        int callIndex,
        List<(string Label, List<IRInstruction> Instructions, IRInstruction? Terminator)> inlinedBlocks,
        IRValue resultPlaceholder)
    {
        block.Instructions.RemoveAt(callIndex);

        foreach (var (_, instructions, _) in inlinedBlocks)
        {
            for (var j = 0; j < instructions.Count; j++)
            {
                block.InsertInstruction(callIndex + j, instructions[j]);
            }
        }
    }
}
