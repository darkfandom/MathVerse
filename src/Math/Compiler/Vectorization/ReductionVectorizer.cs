namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Vectorizes reduction operations (sum, dot product, max, min) using tree-based
/// reduction with SIMD. Converts scalar reduction loops into vectorized partial
/// reductions followed by a scalar final reduction.
/// </summary>
public sealed class ReductionVectorizer : IVectorizationPass
{
    private readonly int _minReductionWidth;

    /// <summary>
    /// Initializes the reduction vectorizer.
    /// </summary>
    /// <param name="minReductionWidth">Minimum reduction width to vectorize.</param>
    public ReductionVectorizer(int minReductionWidth = 4)
    {
        _minReductionWidth = minReductionWidth;
    }

    /// <inheritdoc />
    public string Name => "ReductionVectorizer";

    /// <inheritdoc />
    public IRModule Vectorize(IRModule module)
    {
        foreach (var function in module.Functions)
            VectorizeReductions(function);
        return module;
    }

    private void VectorizeReductions(IRFunction function)
    {
        foreach (var block in function.Blocks)
            VectorizeBlockReductions(block);
    }

    private void VectorizeBlockReductions(IRBlock block)
    {
        var reductions = IdentifyReductions(block);
        if (reductions.Count == 0)
            return;

        foreach (var reduction in reductions)
        {
            ApplyTreeReduction(block, reduction);
        }
    }

    private static List<ReductionOperation> IdentifyReductions(IRBlock block)
    {
        var reductions = new List<ReductionOperation>();
        var processedResults = new HashSet<int>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst is IRPhiNode)
                continue;
            if (inst.Result == null)
                continue;

            if (inst.OpCode is IROpCode.Add or IROpCode.Mul)
            {
                if (processedResults.Contains(inst.Result.Id))
                    continue;

                var reduction = TryIdentifySumReduction(inst, block, i);
                if (reduction != null)
                {
                    reductions.Add(reduction);
                    foreach (var node in reduction.SourceInstructions)
                        processedResults.Add(node.Id);
                }
            }

            if (inst.OpCode == IROpCode.Dot)
            {
                var dotReduction = new ReductionOperation
                {
                    Type = ReductionType.DotProduct,
                    OpCode = IROpCode.Add,
                    ResultValue = inst.Result,
                    SourceInstructions = new List<IRValue> { inst.Result }
                };
                reductions.Add(dotReduction);
                processedResults.Add(inst.Result.Id);
            }
        }

        return reductions;
    }

    private static ReductionOperation? TryIdentifySumReduction(
        IRInstruction inst,
        IRBlock block,
        int startIndex)
    {
        if (inst.Operands.Count < 2)
            return null;

        var accumulator = inst.Result;
        var hasAccumulatorInput = false;
        IRValue? dataInput = null;

        foreach (var operand in inst.Operands)
        {
            if (operand.Id == accumulator!.Id)
            {
                hasAccumulatorInput = true;
            }
            else if (!operand.IsConstant)
            {
                dataInput = operand;
            }
        }

        if (!hasAccumulatorInput || dataInput == null)
            return null;

        var sources = new List<IRValue> { inst.Result! };
        var current = inst.Result;

        for (var lookBack = 1; lookBack < 16 && startIndex - lookBack >= 0; lookBack++)
        {
            var prevIdx = startIndex - lookBack;
            if (prevIdx < 0 || prevIdx >= block.Instructions.Count)
                break;

            var prevInst = block.Instructions[prevIdx];
            if (prevInst.Result == null)
                continue;

            if (prevInst.Result!.Id == current!.Id)
            {
                foreach (var op in prevInst.Operands)
                {
                    if (op.Id != accumulator!.Id && !op.IsConstant)
                        sources.Add(op);
                }
                break;
            }
        }

        return new ReductionOperation
        {
            Type = ReductionType.Sum,
            OpCode = IROpCode.Add,
            ResultValue = accumulator!,
            SourceInstructions = sources
        };
    }

    private static void ApplyTreeReduction(IRBlock block, ReductionOperation reduction, int minReductionWidth = 2)
    {
        var vectorWidth = Vector<float>.Count;
        var sources = reduction.SourceInstructions;

        if (sources.Count < minReductionWidth)
            return;

        var result = reduction.ResultValue;
        var idx = FindInstructionIndex(block, result);
        if (idx < 0)
            return;

        var partialResults = new List<IRValue>();
        var numVectors = sources.Count / vectorWidth;

        for (var v = 0; v < numVectors; v++)
        {
            var vectorResult = IRValue.CreateRegister(
                $"red_partial_{v}_{result.Name}", IRType.Vector);

            var vectorOperands = new List<IRValue>();
            for (var lane = 0; lane < vectorWidth; lane++)
            {
                var srcIdx = v * vectorWidth + lane;
                if (srcIdx < sources.Count)
                    vectorOperands.Add(sources[srcIdx]);
            }

            while (vectorOperands.Count < vectorWidth)
                vectorOperands.Add(IRValue.CreateConstant(0.0, result.Type));

            var vectorInst = new IRInstruction(
                IROpCode.VectorOp, vectorResult, vectorOperands);
            block.InsertInstruction(idx, vectorInst);
            vectorInst.ParentBlock = block;

            partialResults.Add(vectorResult);
        }

        var remainderStart = numVectors * vectorWidth;
        IRValue? scalarAccumulator = null;

        for (var r = remainderStart; r < sources.Count; r++)
        {
            if (scalarAccumulator == null)
            {
                scalarAccumulator = sources[r];
            }
            else
            {
                var tempResult = IRValue.CreateRegister(
                    $"red_scalar_{result.Name}", result.Type);
                var scalarInst = new IRInstruction(
                    reduction.OpCode, tempResult, scalarAccumulator, sources[r]);
                block.InsertInstruction(idx, scalarInst);
                scalarInst.ParentBlock = block;
                scalarAccumulator = tempResult;
            }
        }

        if (partialResults.Count > 0)
        {
            var vectorAccum = partialResults[0];
            for (var i = 1; i < partialResults.Count; i++)
            {
                var tempResult = IRValue.CreateRegister(
                    $"red_vec_acc_{result.Name}", IRType.Vector);
                var addInst = new IRInstruction(
                    IROpCode.VectorOp, tempResult, vectorAccum, partialResults[i]);
                block.InsertInstruction(idx, addInst);
                addInst.ParentBlock = block;
                vectorAccum = tempResult;
            }

            var finalScalarResult = IRValue.CreateRegister(
                $"red_final_{result.Name}", result.Type);
            var reduceInst = new IRInstruction(
                IROpCode.Sum, finalScalarResult, vectorAccum);
            block.InsertInstruction(idx, reduceInst);
            reduceInst.ParentBlock = block;

            if (scalarAccumulator != null)
            {
                var finalAdd = new IRInstruction(
                    reduction.OpCode, result, finalScalarResult, scalarAccumulator);
                block.InsertInstruction(idx, finalAdd);
                finalAdd.ParentBlock = block;
            }
            else
            {
                var nop = new IRInstruction(IROpCode.Nop, result, finalScalarResult);
                block.InsertInstruction(idx, nop);
                nop.ParentBlock = block;
            }
        }
    }

    private static int FindInstructionIndex(IRBlock block, IRValue value)
    {
        for (var i = 0; i < block.Instructions.Count; i++)
        {
            if (block.Instructions[i].Result != null &&
                block.Instructions[i].Result!.Id == value.Id)
                return i;
        }
        return block.Instructions.Count;
    }

    private sealed class ReductionOperation
    {
        public ReductionType Type { get; set; }
        public IROpCode OpCode { get; set; }
        public IRValue ResultValue { get; set; } = null!;
        public List<IRValue> SourceInstructions { get; set; } = new();
    }

    private enum ReductionType
    {
        Sum,
        DotProduct,
        Max,
        Min
    }
}
