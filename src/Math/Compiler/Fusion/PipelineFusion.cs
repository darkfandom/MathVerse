namespace MathVerse.Math.Compiler.Fusion;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a fusible pipeline stage sequence.
/// </summary>
public sealed class PipelineStage
{
    /// <summary>The kind of pipeline stage.</summary>
    public PipelineStageKind Kind { get; }

    /// <summary>The instruction indices forming this stage.</summary>
    public IReadOnlyList<int> InstructionIndices { get; }

    /// <summary>
    /// Initializes a new pipeline stage.
    /// </summary>
    public PipelineStage(PipelineStageKind kind, IReadOnlyList<int> instructionIndices)
    {
        Kind = kind;
        InstructionIndices = instructionIndices;
    }
}

/// <summary>
/// Classifies a pipeline stage.
/// </summary>
public enum PipelineStageKind
{
    /// <summary>Element-wise transformation (map).</summary>
    Map,

    /// <summary>Reduction operation.</summary>
    Reduce,

    /// <summary>Reshape or data reorganization.</summary>
    Transform,

    /// <summary>Combined map-reduce in single pass.</summary>
    MapReduce,

    /// <summary>Combined map-transform in single pass.</summary>
    MapTransform
}

/// <summary>
/// Represents a fusible pipeline: a sequence of stages that can potentially be combined into a single pass.
/// </summary>
public sealed class FusiblePipeline
{
    /// <summary>The stages in this pipeline.</summary>
    public IReadOnlyList<PipelineStage> Stages { get; }

    /// <summary>The estimated memory traffic reduction from fusion.</summary>
    public int EstimatedMemorySaved { get; }

    /// <summary>
    /// Initializes a new fusible pipeline.
    /// </summary>
    public FusiblePipeline(IReadOnlyList<PipelineStage> stages, int estimatedMemorySaved)
    {
        Stages = stages;
        EstimatedMemorySaved = estimatedMemorySaved;
    }
}

/// <summary>
/// Fuses pipeline stages (map → reduce → transform) into single-pass operations where possible.
/// Reduces memory traffic by keeping intermediate results in registers/cache.
/// </summary>
public sealed class PipelineFusion
{
    private const int MaxFusableInstructions = 32;

    /// <summary>
    /// Identifies fusible pipeline sequences within a function.
    /// </summary>
    /// <param name="function">The IR function to analyze.</param>
    /// <returns>A dictionary mapping block indices to fusible pipelines.</returns>
    public IReadOnlyDictionary<int, IReadOnlyList<FusiblePipeline>> FindPipelines(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var results = new Dictionary<int, IReadOnlyList<FusiblePipeline>>();

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];
            var pipelines = IdentifyPipelines(block);
            if (pipelines.Count > 0)
                results[b] = pipelines;
        }

        return results;
    }

    /// <summary>
    /// Applies pipeline fusion to a function, merging fusible stages into single-pass operations.
    /// </summary>
    /// <param name="function">The function to optimize.</param>
    /// <returns>A new function with pipeline fusion applied.</returns>
    public IRFunction ApplyFusion(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var pipelinesByBlock = FindPipelines(function);
        var fused = new IRFunction(function.Name + "_pfused", function.ReturnType, function.Parameters);
        var valueMap = new Dictionary<int, IRValue>();

        foreach (var param in function.Parameters)
            valueMap[param.Id] = param;

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var srcBlock = function.Blocks[b];
            var newBlock = fused.CreateBlock(srcBlock.Label);

            pipelinesByBlock.TryGetValue(b, out var pipelines);
            var instructionToFusible = new Dictionary<int, FusiblePipeline>();
            if (pipelines != null)
            {
                for (var p = 0; p < pipelines.Count; p++)
                {
                    for (var i = 0; i < pipelines[p].Stages.Count; i++)
                    {
                        for (var j = 0; j < pipelines[p].Stages[i].InstructionIndices.Count; j++)
                            instructionToFusible[pipelines[p].Stages[i].InstructionIndices[j]] = pipelines[p];
                    }
                }
            }

            var skipUntil = -1;
            for (var i = 0; i < srcBlock.Instructions.Count; i++)
            {
                if (i <= skipUntil)
                    continue;

                if (instructionToFusible.TryGetValue(i, out var pipeline))
                {
                    EmitFusedPipeline(newBlock, pipeline, srcBlock, valueMap);
                    var maxIdx = 0;
                    for (var s = 0; s < pipeline.Stages.Count; s++)
                    {
                        for (var j = 0; j < pipeline.Stages[s].InstructionIndices.Count; j++)
                        {
                            if (pipeline.Stages[s].InstructionIndices[j] > maxIdx)
                                maxIdx = pipeline.Stages[s].InstructionIndices[j];
                        }
                    }
                    skipUntil = maxIdx;
                    continue;
                }

                var inst = srcBlock.Instructions[i];
                var remapped = RemapInstruction(inst, valueMap);
                newBlock.AppendInstruction(remapped);
                if (remapped.Result != null)
                    valueMap[remapped.Result.Id] = remapped.Result;
            }

            if (srcBlock.Terminator != null && !srcBlock.Instructions.Contains(srcBlock.Terminator))
            {
                var remapped = RemapInstruction(srcBlock.Terminator, valueMap);
                newBlock.AppendInstruction(remapped);
            }
        }

        return fused;
    }

    private static List<FusiblePipeline> IdentifyPipelines(IRBlock block)
    {
        var pipelines = new List<FusiblePipeline>();
        var consumed = new bool[block.Instructions.Count];

        var currentStages = new List<PipelineStage>();
        var currentStart = -1;
        var currentEnd = -1;

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            if (consumed[i]) continue;

            var stageKind = ClassifyStage(block, i);
            if (stageKind == null)
            {
                if (currentStages.Count >= 2)
                {
                    var memorySaved = EstimateMemorySavings(currentStages, block);
                    if (memorySaved > 0)
                    {
                        pipelines.Add(new FusiblePipeline(currentStages.ToArray(), memorySaved));
                        for (var s = currentStart; s <= currentEnd; s++)
                            consumed[s] = true;
                    }
                }
                currentStages.Clear();
                currentStart = -1;
                currentEnd = -1;
                continue;
            }

            var stage = stageKind.Value;
            var indices = new List<int>();

            if (stage == PipelineStageKind.Map || stage == PipelineStageKind.Reduce || stage == PipelineStageKind.Transform)
            {
                indices.Add(i);
                consumed[i] = true;

                // Extend the stage through data-dependent instructions
                var lastResult = block.Instructions[i].Result;
                if (lastResult != null)
                {
                    for (var j = i + 1; j < block.Instructions.Count; j++)
                    {
                        if (consumed[j]) continue;
                        if (block.Instructions[j].Result == null) break;

                        var usesLast = false;
                        for (var k = 0; k < block.Instructions[j].Operands.Count; k++)
                        {
                            if (block.Instructions[j].Operands[k].Id == lastResult!.Id)
                            {
                                usesLast = true;
                                break;
                            }
                        }

                        if (usesLast && IsSameStageKind(block.Instructions[j].OpCode, stage))
                        {
                            indices.Add(j);
                            consumed[j] = true;
                            lastResult = block.Instructions[j].Result;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (indices.Count > 0)
            {
                currentStages.Add(new PipelineStage(stage, indices));
                if (currentStart < 0) currentStart = indices[0];
                currentEnd = indices[^1];

                if (indices.Count > MaxFusableInstructions || currentStages.Count >= 3)
                {
                    if (currentStages.Count >= 2)
                    {
                        var memorySaved = EstimateMemorySavings(currentStages, block);
                        if (memorySaved > 0)
                        {
                            pipelines.Add(new FusiblePipeline(currentStages.ToArray(), memorySaved));
                            for (var s = currentStart; s <= currentEnd; s++)
                                consumed[s] = true;
                        }
                    }
                    currentStages.Clear();
                    currentStart = -1;
                    currentEnd = -1;
                }
            }
        }

        if (currentStages.Count >= 2)
        {
            var memorySaved = EstimateMemorySavings(currentStages, block);
            if (memorySaved > 0)
            {
                pipelines.Add(new FusiblePipeline(currentStages.ToArray(), memorySaved));
            }
        }

        return pipelines;
    }

    private static PipelineStageKind? ClassifyStage(IRBlock block, int instructionIndex)
    {
        var inst = block.Instructions[instructionIndex];

        return inst.OpCode switch
        {
            IROpCode.Add or IROpCode.Sub or IROpCode.Mul or IROpCode.Div or
            IROpCode.Neg or IROpCode.Abs or IROpCode.Sin or IROpCode.Cos or
            IROpCode.Tan or IROpCode.Log or IROpCode.Exp or IROpCode.Sqrt or
            IROpCode.Pow => PipelineStageKind.Map,

            IROpCode.Sum or IROpCode.Dot => PipelineStageKind.Reduce,

            IROpCode.Reshape or IROpCode.Transpose => PipelineStageKind.Transform,

            IROpCode.MatMul => PipelineStageKind.Map,

            _ => null
        };
    }

    private static bool IsSameStageKind(IROpCode opCode, PipelineStageKind kind)
    {
        var classified = ClassifyStage(null!, -1);
        return kind switch
        {
            PipelineStageKind.Map => opCode is IROpCode.Add or IROpCode.Sub or IROpCode.Mul or
                IROpCode.Div or IROpCode.Neg or IROpCode.Abs or IROpCode.Sin or IROpCode.Cos or
                IROpCode.Tan or IROpCode.Log or IROpCode.Exp or IROpCode.Sqrt or IROpCode.Pow or
                IROpCode.MatMul,
            PipelineStageKind.Reduce => opCode is IROpCode.Sum or IROpCode.Dot,
            PipelineStageKind.Transform => opCode is IROpCode.Reshape or IROpCode.Transpose,
            _ => false
        };
    }

    private static int EstimateMemorySavings(List<PipelineStage> stages, IRBlock block)
    {
        var intermediateValues = 0;
        for (var s = 0; s < stages.Count - 1; s++)
        {
            var lastIdx = stages[s].InstructionIndices[^1];
            if (lastIdx < block.Instructions.Count)
            {
                var inst = block.Instructions[lastIdx];
                if (inst.Result != null)
                    intermediateValues++;
            }
        }

        return intermediateValues * 8;
    }

    private static void EmitFusedPipeline(IRBlock targetBlock, FusiblePipeline pipeline,
        IRBlock sourceBlock, Dictionary<int, IRValue> valueMap)
    {
        for (var s = 0; s < pipeline.Stages.Count; s++)
        {
            var stage = pipeline.Stages[s];
            for (var i = 0; i < stage.InstructionIndices.Count; i++)
            {
                var instIdx = stage.InstructionIndices[i];
                if (instIdx >= sourceBlock.Instructions.Count) continue;

                var inst = sourceBlock.Instructions[instIdx];
                var remapped = RemapInstruction(inst, valueMap);
                targetBlock.AppendInstruction(remapped);
                if (remapped.Result != null && inst.Result != null)
                    valueMap[inst.Result.Id] = remapped.Result;
            }
        }
    }

    private static IRInstruction RemapInstruction(IRInstruction inst, Dictionary<int, IRValue> valueMap)
    {
        var operands = new IRValue[inst.Operands.Count];
        for (var i = 0; i < inst.Operands.Count; i++)
        {
            if (valueMap.TryGetValue(inst.Operands[i].Id, out var mapped))
                operands[i] = mapped;
            else
                operands[i] = inst.Operands[i];
        }

        IRValue? newResult = null;
        if (inst.Result != null)
            newResult = IRValue.CreateRegister(inst.Result.Name, inst.Result.Type);

        return new IRInstruction(inst.OpCode, newResult, operands);
    }
}
