namespace MathVerse.Math.Compiler.Fusion;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Fuses multiple kernel functions into a single combined function by analyzing shared inputs/outputs
/// and minimizing data movement between them.
/// </summary>
public sealed class KernelFusion
{
    /// <summary>
    /// Fuses multiple IR functions into a single combined function. The fused function takes
    /// the union of all input parameters and returns a tuple-like result.
    /// </summary>
    /// <param name="functions">The functions to fuse. Must share some input/output values.</param>
    /// <param name="fusedName">The name for the fused function.</param>
    /// <returns>A new IRFunction representing the fused computation.</returns>
    public IRFunction Fuse(IReadOnlyList<IRFunction> functions, string fusedName)
    {
        ArgumentNullException.ThrowIfNull(functions);
        if (functions.Count == 0)
            throw new ArgumentException("Must provide at least one function to fuse.", nameof(functions));
        if (functions.Count == 1)
            return functions[0];

        var sharedInputs = FindSharedInputs(functions);
        var sharedOutputs = FindSharedOutputs(functions);

        var fusedParams = BuildFusedParameters(functions, sharedInputs);
        var fusedFunction = new IRFunction(fusedName, IRType.Void, fusedParams);

        var inputMap = BuildInputMap(functions, fusedParams, sharedInputs);
        var resultMap = new Dictionary<int, IRValue>();

        var entryBlock = fusedFunction.CreateBlock("entry");
        var ilogueBlock = fusedFunction.CreateBlock("ilogue");

        ilogueBlock.AppendInstruction(new IRInstruction(IROpCode.Return, null));

        var currentBlock = entryBlock;

        for (var f = 0; f < functions.Count; f++)
        {
            var func = functions[f];
            var blockMap = new Dictionary<string, IRBlock>();

            for (var b = 0; b < func.Blocks.Count; b++)
            {
                var srcBlock = func.Blocks[b];
                var newLabel = $"f{f}_{srcBlock.Label}";
                var newBlock = fusedFunction.CreateBlock(newLabel);
                blockMap[srcBlock.Label] = newBlock;
            }

            for (var b = 0; b < func.Blocks.Count; b++)
            {
                var srcBlock = func.Blocks[b];
                var targetBlock = blockMap[srcBlock.Label];

                foreach (var inst in srcBlock.Instructions)
                {
                    var mappedInst = RemapInstruction(inst, inputMap, resultMap, blockMap);
                    if (mappedInst != null)
                        targetBlock.AppendInstruction(mappedInst);
                }

                if (srcBlock.Terminator != null && !srcBlock.Instructions.Contains(srcBlock.Terminator))
                {
                    var mappedTerm = RemapInstruction(srcBlock.Terminator, inputMap, resultMap, blockMap);
                    if (mappedTerm != null)
                        targetBlock.AppendInstruction(mappedTerm);
                }
            }

            if (func.Blocks.Count > 0)
            {
                var firstSrcBlock = func.Blocks[0];
                if (blockMap.TryGetValue(firstSrcBlock.Label, out var firstTarget))
                {
                    var blockRef = IRValue.CreateRegister(firstTarget.Label, IRType.Void);
                    currentBlock.AppendInstruction(new IRInstruction(
                        IROpCode.Branch, null, blockRef));
                    currentBlock = null!;
                }
            }

            RecordFunctionOutputs(func, resultMap);
        }

        if (currentBlock != null)
        {
            currentBlock.AppendInstruction(new IRInstruction(
                IROpCode.Branch, null, IRValue.CreateRegister(ilogueBlock.Label, IRType.Void)));
        }

        return fusedFunction;
    }

    /// <summary>
    /// Analyzes multiple functions and returns the set of values shared as inputs between them.
    /// </summary>
    /// <param name="functions">The functions to analyze.</param>
    /// <returns>A set of shared input values.</returns>
    public static IReadOnlySet<IRValue> AnalyzeSharedInputs(IReadOnlyList<IRFunction> functions)
    {
        return FindSharedInputs(functions);
    }

    /// <summary>
    /// Analyzes multiple functions and returns the set of values that are produced as outputs.
    /// </summary>
    /// <param name="functions">The functions to analyze.</param>
    /// <returns>A set of output values.</returns>
    public static IReadOnlySet<IRValue> AnalyzeSharedOutputs(IReadOnlyList<IRFunction> functions)
    {
        return FindSharedOutputs(functions);
    }

    private static HashSet<IRValue> FindSharedInputs(IReadOnlyList<IRFunction> functions)
    {
        var allParamCounts = new Dictionary<int, int>();
        for (var i = 0; i < functions.Count; i++)
        {
            for (var j = 0; j < functions[i].Parameters.Count; j++)
            {
                var paramId = functions[i].Parameters[j].Id;
                if (!allParamCounts.ContainsKey(paramId))
                    allParamCounts[paramId] = 0;
                allParamCounts[paramId]++;
            }
        }

        var shared = new HashSet<IRValue>();
        foreach (var kvp in allParamCounts)
        {
            if (kvp.Value > 1)
            {
                for (var f = 0; f < functions.Count; f++)
                {
                    for (var p = 0; p < functions[f].Parameters.Count; p++)
                    {
                        if (functions[f].Parameters[p].Id == kvp.Key)
                        {
                            shared.Add(functions[f].Parameters[p]);
                            break;
                        }
                    }
                }
            }
        }

        return shared;
    }

    private static HashSet<IRValue> FindSharedOutputs(IReadOnlyList<IRFunction> functions)
    {
        var definedIds = new Dictionary<int, int>();
        for (var f = 0; f < functions.Count; f++)
        {
            foreach (var val in functions[f].GetDefinedValues())
            {
                if (!definedIds.ContainsKey(val.Id))
                    definedIds[val.Id] = 0;
                definedIds[val.Id]++;
            }
        }

        var shared = new HashSet<IRValue>();
        foreach (var f in functions)
        {
            foreach (var val in f.GetDefinedValues())
            {
                if (definedIds.TryGetValue(val.Id, out var count) && count > 1)
                    shared.Add(val);
            }
        }

        return shared;
    }

    private static List<IRValue> BuildFusedParameters(IReadOnlyList<IRFunction> functions, HashSet<IRValue> sharedInputs)
    {
        var seen = new HashSet<int>();
        var result = new List<IRValue>();

        for (var f = 0; f < functions.Count; f++)
        {
            for (var p = 0; p < functions[f].Parameters.Count; p++)
            {
                var param = functions[f].Parameters[p];
                if (sharedInputs.Contains(param) && !seen.Add(param.Id))
                    continue;
                if (seen.Add(param.Id))
                    result.Add(param);
            }
        }

        return result;
    }

    private static Dictionary<int, IRValue> BuildInputMap(
        IReadOnlyList<IRFunction> functions,
        List<IRValue> fusedParams,
        HashSet<IRValue> sharedInputs)
    {
        var map = new Dictionary<int, IRValue>();
        for (var f = 0; f < functions.Count; f++)
        {
            for (var p = 0; p < functions[f].Parameters.Count; p++)
            {
                var srcParam = functions[f].Parameters[p];
                if (map.ContainsKey(srcParam.Id))
                    continue;

                for (var fp = 0; fp < fusedParams.Count; fp++)
                {
                    if (fusedParams[fp].Id == srcParam.Id)
                    {
                        map[srcParam.Id] = fusedParams[fp];
                        break;
                    }
                }
            }
        }

        return map;
    }

    private static IRInstruction? RemapInstruction(
        IRInstruction inst,
        Dictionary<int, IRValue> inputMap,
        Dictionary<int, IRValue> resultMap,
        Dictionary<string, IRBlock> blockMap)
    {
        var remappedOperands = new List<IRValue>(inst.Operands.Count);
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            var op = inst.Operands[i];
            if (inputMap.TryGetValue(op.Id, out var mapped))
                remappedOperands.Add(mapped);
            else if (resultMap.TryGetValue(op.Id, out var resultMapped))
                remappedOperands.Add(resultMapped);
            else
                remappedOperands.Add(op);
        }

        IRValue? newResult = null;
        if (inst.Result != null)
        {
            newResult = IRValue.CreateRegister($"fused_{inst.Result.Name}", inst.Result.Type);
            resultMap[inst.Result.Id] = newResult;
        }

        return new IRInstruction(inst.OpCode, newResult, remappedOperands);
    }

    private static void RecordFunctionOutputs(IRFunction function, Dictionary<int, IRValue> resultMap)
    {
        foreach (var block in function.Blocks)
        {
            if (block.Terminator?.OpCode == IROpCode.Return)
            {
                for (var i = 0; i < block.Terminator.Operands.Count; i++)
                {
                    var op = block.Terminator.Operands[i];
                    if (!resultMap.ContainsKey(op.Id))
                        resultMap[op.Id] = op;
                }
            }
        }
    }
}
