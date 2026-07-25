namespace MathVerse.Math.Compiler.Fusion;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a connected subgraph of computation that can potentially be fused into a single kernel.
/// </summary>
public sealed class FusableSubgraph
{
    /// <summary>The blocks belonging to this subgraph.</summary>
    public IReadOnlyList<int> BlockIndices { get; }

    /// <summary>The input values required by this subgraph from outside.</summary>
    public IReadOnlyList<IRValue> ExternalInputs { get; }

    /// <summary>The output values produced by this subgraph for external use.</summary>
    public IReadOnlyList<IRValue> ExternalOutputs { get; }

    /// <summary>The estimated computation cost of this subgraph.</summary>
    public int EstimatedCost { get; }

    /// <summary>
    /// Initializes a new fusible subgraph.
    /// </summary>
    public FusableSubgraph(IReadOnlyList<int> blockIndices, IReadOnlyList<IRValue> externalInputs,
        IReadOnlyList<IRValue> externalOutputs, int estimatedCost)
    {
        BlockIndices = blockIndices;
        ExternalInputs = externalInputs;
        ExternalOutputs = externalOutputs;
        EstimatedCost = estimatedCost;
    }
}

/// <summary>
/// Fuses computation graph subgraphs into single kernels. Identifies connected components
/// that can be computed together without excessive register pressure.
/// </summary>
public sealed class GraphFusion
{
    private const int MaxFusableSize = 64;

    /// <summary>
    /// Identifies fusible subgraphs within a function. A subgraph is fusible if its blocks
    /// form a connected component with manageable size and external dependencies.
    /// </summary>
    /// <param name="function">The IR function to analyze.</param>
    /// <returns>A list of fusible subgraphs.</returns>
    public IReadOnlyList<FusableSubgraph> IdentifySubgraphs(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (function.Blocks.Count <= 1)
            return Array.Empty<FusableSubgraph>();

        var adjacency = BuildAdjacency(function);
        var visited = new bool[function.Blocks.Count];
        var subgraphs = new List<FusableSubgraph>();

        for (var i = 0; i < function.Blocks.Count; i++)
        {
            if (visited[i]) continue;

            var component = BFS(i, adjacency, visited, function.Blocks.Count);
            if (component.Count <= 1 || component.Count > MaxFusableSize) continue;

            var subgraph = AnalyzeSubgraph(component, function);
            if (subgraph != null)
                subgraphs.Add(subgraph);
        }

        return subgraphs;
    }

    /// <summary>
    /// Applies graph fusion to a function, merging identified subgraphs where beneficial.
    /// </summary>
    /// <param name="function">The function to optimize.</param>
    /// <returns>A new function with graph fusion applied.</returns>
    public IRFunction ApplyFusion(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var subgraphs = IdentifySubgraphs(function);
        if (subgraphs.Count == 0) return function;

        var fused = new IRFunction(function.Name + "_gfused", function.ReturnType, function.Parameters);
        var valueMap = new Dictionary<int, IRValue>();
        var processedBlocks = new HashSet<int>();

        foreach (var param in function.Parameters)
            valueMap[param.Id] = param;

        for (var s = 0; s < subgraphs.Count; s++)
        {
            var subgraph = subgraphs[s];
            var kernelBlock = fused.CreateBlock($"subgraph_{s}");

            for (var i = 0; i < subgraph.BlockIndices.Count; i++)
            {
                var blockIdx = subgraph.BlockIndices[i];
                if (!processedBlocks.Add(blockIdx))
                    continue;

                var srcBlock = function.Blocks[blockIdx];
                EmitBlockInstructions(kernelBlock, srcBlock, valueMap, fused);
            }
        }

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            if (processedBlocks.Contains(b)) continue;

            var srcBlock = function.Blocks[b];
            var newBlock = fused.CreateBlock(srcBlock.Label);
            EmitBlockInstructions(newBlock, srcBlock, valueMap, fused);
        }

        return fused;
    }

    private static Dictionary<int, List<int>> BuildAdjacency(IRFunction function)
    {
        var adj = new Dictionary<int, List<int>>();
        for (var i = 0; i < function.Blocks.Count; i++)
            adj[i] = new List<int>();

        for (var i = 0; i < function.Blocks.Count; i++)
        {
            var block = function.Blocks[i];
            var successors = block.Successors;
            for (var j = 0; j < successors.Count; j++)
            {
                for (var k = 0; k < function.Blocks.Count; k++)
                {
                    if (function.Blocks[k] == successors[j])
                    {
                        adj[i].Add(k);
                        if (!adj.ContainsKey(k))
                            adj[k] = new List<int>();
                        adj[k].Add(i);
                        break;
                    }
                }
            }

            var successorsFromTerminator = GetSuccessorIndices(block, function);
            foreach (var succIdx in successorsFromTerminator)
            {
                if (!adj[i].Contains(succIdx))
                    adj[i].Add(succIdx);
                if (!adj.ContainsKey(succIdx))
                    adj[succIdx] = new List<int>();
                if (!adj[succIdx].Contains(i))
                    adj[succIdx].Add(i);
            }
        }

        return adj;
    }

    private static List<int> GetSuccessorIndices(IRBlock block, IRFunction function)
    {
        var indices = new List<int>();
        if (block.Terminator == null) return indices;

        if (block.Terminator.OpCode == IROpCode.Branch || block.Terminator.OpCode == IROpCode.CondBranch)
        {
            for (var i = 0; i < block.Terminator.Operands.Count; i++)
            {
                var operand = block.Terminator.Operands[i];
                for (var j = 0; j < function.Blocks.Count; j++)
                {
                    if (function.Blocks[j].Label == operand.Name)
                    {
                        indices.Add(j);
                        break;
                    }
                }
            }
        }

        return indices;
    }

    private static List<int> BFS(int start, Dictionary<int, List<int>> adjacency, bool[] visited, int totalBlocks)
    {
        var component = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(start);
        visited[start] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            component.Add(current);

            if (adjacency.TryGetValue(current, out var neighbors))
            {
                for (var i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (!visited[neighbor] && neighbor < totalBlocks)
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return component;
    }

    private static FusableSubgraph? AnalyzeSubgraph(List<int> component, IRFunction function)
    {
        var externalInputs = new HashSet<int>();
        var externalOutputs = new HashSet<int>();
        var blockSet = new HashSet<int>(component);
        var cost = 0;

        foreach (var blockIdx in component)
        {
            var block = function.Blocks[blockIdx];
            foreach (var inst in block.Instructions)
            {
                cost += EstimateInstructionCost(inst.OpCode);

                if (inst.Result != null)
                    externalOutputs.Add(inst.Result.Id);

                for (var i = 0; i < inst.Operands.Count; i++)
                {
                    var operand = inst.Operands[i];
                    if (!blockSet.Contains(operand.Id))
                        externalInputs.Add(operand.Id);
                }
            }
        }

        var inputValues = new List<IRValue>();
        var outputValues = new List<IRValue>();

        foreach (var blockIdx in component)
        {
            var block = function.Blocks[blockIdx];
            foreach (var inst in block.Instructions)
            {
                if (inst.Result != null && externalOutputs.Contains(inst.Result.Id))
                {
                    var isUsedExternally = false;
                    foreach (var otherIdx in component)
                    {
                        if (otherIdx == blockIdx) continue;
                        var otherBlock = function.Blocks[otherIdx];
                        foreach (var otherInst in otherBlock.Instructions)
                        {
                            for (var i = 0; i < otherInst.Operands.Count; i++)
                            {
                                if (otherInst.Operands[i].Id == inst.Result.Id)
                                {
                                    isUsedExternally = true;
                                    break;
                                }
                            }
                            if (isUsedExternally) break;
                        }
                        if (isUsedExternally) break;
                    }

                    if (!isUsedExternally)
                        outputValues.Add(inst.Result);
                }
            }
        }

        return new FusableSubgraph(component, inputValues, outputValues, cost);
    }

    private static int EstimateInstructionCost(IROpCode opCode)
    {
        return opCode switch
        {
            IROpCode.Add or IROpCode.Sub or IROpCode.Mul or IROpCode.Neg => 1,
            IROpCode.Div or IROpCode.Mod => 3,
            IROpCode.Fma => 2,
            IROpCode.Sqrt or IROpCode.Abs => 4,
            IROpCode.Sin or IROpCode.Cos or IROpCode.Tan => 8,
            IROpCode.Log or IROpCode.Exp or IROpCode.Pow => 10,
            IROpCode.MatMul => 20,
            IROpCode.Dot or IROpCode.Sum => 5,
            IROpCode.Load or IROpCode.Store => 1,
            IROpCode.Branch or IROpCode.CondBranch or IROpCode.Return => 1,
            _ => 1
        };
    }

    private static void EmitBlockInstructions(IRBlock targetBlock, IRBlock sourceBlock,
        Dictionary<int, IRValue> valueMap, IRFunction targetFunction)
    {
        for (var i = 0; i < sourceBlock.Instructions.Count; i++)
        {
            var inst = sourceBlock.Instructions[i];
            var operands = new IRValue[inst.Operands.Count];
            for (var j = 0; j < inst.Operands.Count; j++)
            {
                if (valueMap.TryGetValue(inst.Operands[j].Id, out var mapped))
                    operands[j] = mapped;
                else
                    operands[j] = inst.Operands[j];
            }

            IRValue? newResult = null;
            if (inst.Result != null)
            {
                newResult = IRValue.CreateRegister(inst.Result.Name, inst.Result.Type);
            }

            var newInst = new IRInstruction(inst.OpCode, newResult, operands);
            targetBlock.AppendInstruction(newInst);

            if (newResult != null)
                valueMap[inst.Result!.Id] = newResult;
        }

        if (sourceBlock.Terminator != null && !sourceBlock.Instructions.Contains(sourceBlock.Terminator))
        {
            var term = sourceBlock.Terminator;
            var operands = new IRValue[term.Operands.Count];
            for (var j = 0; j < term.Operands.Count; j++)
            {
                if (valueMap.TryGetValue(term.Operands[j].Id, out var mapped))
                    operands[j] = mapped;
                else
                    operands[j] = term.Operands[j];
            }

            targetBlock.AppendInstruction(new IRInstruction(term.OpCode, null, operands));
        }
    }
}
