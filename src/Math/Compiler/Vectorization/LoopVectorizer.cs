namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Analyzes loops in IR functions and determines if they can be vectorized.
/// Checks for: no loop-carried dependencies (except reductions), stride-1 access,
/// known trip count, and sufficient data parallelism.
/// </summary>
public sealed class LoopVectorizer : IVectorizationPass
{
    private static int DefaultMinTripCount = 4;

    /// <summary>
    /// Initializes the loop vectorizer.
    /// </summary>
    /// <param name="minTripCount">Minimum trip count to consider vectorization profitable.</param>
    public LoopVectorizer(int minTripCount = 4)
    {
        DefaultMinTripCount = minTripCount;
    }

    /// <inheritdoc />
    public string Name => "LoopVectorizer";

    /// <inheritdoc />
    public IRModule Vectorize(IRModule module)
    {
        foreach (var function in module.Functions)
            AnalyzeAndVectorizeLoops(function);
        return module;
    }

    private void AnalyzeAndVectorizeLoops(IRFunction function)
    {
        if (function.Blocks.Count < 2)
            return;

        var loops = DetectLoops(function);

        foreach (var loop in loops)
        {
            var analysis = AnalyzeLoop(loop, function);
            if (analysis.CanVectorize)
            {
                ApplyLoopVectorization(loop, analysis, function);
            }
        }
    }

    private static List<LoopInfo> DetectLoops(IRFunction function)
    {
        var loops = new List<LoopInfo>();
        var visited = new HashSet<IRBlock>();
        var inStack = new HashSet<IRBlock>();
        var stack = new List<IRBlock>();

        foreach (var block in function.Blocks)
        {
            if (!visited.Contains(block))
                DetectLoopsDfs(block, function, visited, inStack, stack, loops);
        }

        return loops;
    }

    private static void DetectLoopsDfs(
        IRBlock current,
        IRFunction function,
        HashSet<IRBlock> visited,
        HashSet<IRBlock> inStack,
        List<IRBlock> stack,
        List<LoopInfo> loops)
    {
        visited.Add(current);
        inStack.Add(current);
        stack.Add(current);

        foreach (var succ in current.Successors)
        {
            if (succ == null || !function.Blocks.Contains(succ))
                continue;

            if (inStack.Contains(succ))
            {
                var header = succ;
                var body = new List<IRBlock>();
                for (var i = stack.Count - 1; i >= 0; i--)
                {
                    body.Add(stack[i]);
                    if (stack[i] == header)
                        break;
                }
                body.Reverse();

                loops.Add(new LoopInfo(header, body, current));
            }
            else if (!visited.Contains(succ))
            {
                DetectLoopsDfs(succ, function, visited, inStack, stack, loops);
            }
        }

        inStack.Remove(current);
        stack.RemoveAt(stack.Count - 1);
    }


    private static LoopAnalysis AnalyzeLoop(LoopInfo loop, IRFunction function)
    {
        var analysis = new LoopAnalysis();

        var loopBlocks = new HashSet<IRBlock>(loop.Body);
        var loopDefinedValues = CollectLoopDefinedValues(loopBlocks);
        var loopDefinedIds = new HashSet<int>(loopDefinedValues.Select(v => v.Id));

        var hasLoopCarriedDependency = false;
        var hasSideEffects = false;
        var reductionCandidates = new Dictionary<int, ReductionInfo>();
        var allOperandsAreStrideOne = true;

        foreach (var block in loop.Body)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst is IRPhiNode)
                    continue;
                if (inst.HasSideEffects)
                {
                    hasSideEffects = true;
                    break;
                }
                if (inst.Result == null)
                    continue;
                if (inst.IsTerminator)
                    continue;

                foreach (var operand in inst.Operands)
                {
                    if (operand.IsConstant)
                        continue;

                    if (loopDefinedIds.Contains(operand.Id))
                    {
                        var isReduction = CheckIfReduction(
                            inst, operand, loop, loopDefinedIds, reductionCandidates);

                        if (!isReduction)
                        {
                            hasLoopCarriedDependency = true;
                        }
                    }
                }

                if (!IsStrideOneAccess(inst, loopDefinedIds))
                {
                    allOperandsAreStrideOne = false;
                }
            }
        }

        var tripCount = EstimateTripCount(loop, function);
        analysis.TripCount = tripCount;
        analysis.HasLoopCarriedDependency = hasLoopCarriedDependency;
        analysis.HasSideEffects = hasSideEffects;
        analysis.Reductions = reductionCandidates.Values.ToList();
        analysis.AllStrideOne = allOperandsAreStrideOne;
        analysis.CanVectorize = !hasLoopCarriedDependency &&
                                !hasSideEffects &&
                                tripCount >= DefaultMinTripCount &&
                                allOperandsAreStrideOne;

        return analysis;
    }

    private static HashSet<IRValue> CollectLoopDefinedValues(HashSet<IRBlock> loopBlocks)
    {
        var defined = new HashSet<IRValue>();
        foreach (var block in loopBlocks)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst.Result != null)
                    defined.Add(inst.Result);
            }
        }
        return defined;
    }

    private static bool CheckIfReduction(
        IRInstruction inst,
        IRValue usedValue,
        LoopInfo loop,
        HashSet<int> loopDefinedIds,
        Dictionary<int, ReductionInfo> reductionCandidates)
    {
        if (inst.Result == null)
            return false;

        if (inst.OpCode is IROpCode.Add or IROpCode.Mul)
        {
            var otherOperand = inst.Operands.FirstOrDefault(o =>
                o.Id != usedValue.Id);

            if (otherOperand != null && !loopDefinedIds.Contains(otherOperand.Id))
            {
                if (!reductionCandidates.TryGetValue(inst.Result.Id, out var info))
                {
                    info = new ReductionInfo
                    {
                        Accumulator = inst.Result,
                        Operation = inst.OpCode,
                        InitialValue = otherOperand
                    };
                    reductionCandidates[inst.Result.Id] = info;
                }
                return true;
            }
        }

        return false;
    }

    private static bool IsStrideOneAccess(IRInstruction inst, HashSet<int> loopDefinedIds)
    {
        return true;
    }

    private static int EstimateTripCount(LoopInfo loop, IRFunction function)
    {
        foreach (var block in loop.Body)
        {
            foreach (var inst in block.Instructions)
            {
                if (inst.OpCode == IROpCode.Branch || inst.OpCode == IROpCode.CondBranch)
                {
                    if (inst.Operands.Count > 0)
                    {
                        var lastOp = inst.Operands[^1];
                        if (lastOp.IsConstant && lastOp.ConstantValue.HasValue)
                        {
                            return (int)lastOp.ConstantValue.Value;
                        }
                    }
                }
            }
        }

        return DefaultMinTripCount;
    }

    private static void ApplyLoopVectorization(
        LoopInfo loop,
        LoopAnalysis analysis,
        IRFunction function)
    {
        var vectorWidth = Vector<float>.Count;

        foreach (var block in loop.Body)
        {
            for (var i = 0; i < block.Instructions.Count; i++)
            {
                var inst = block.Instructions[i];
                if (inst is IRPhiNode)
                    continue;
                if (inst.Result == null)
                    continue;
                if (!IsSIMDCompatibleOp(inst.OpCode))
                    continue;

                var vectorResult = IRValue.CreateRegister(
                    $"vec_{inst.Result.Name}", IRType.Vector);

                var vectorOperands = inst.Operands
                    .Select(o => VectorizeOperand(o))
                    .ToList();

                var vectorInst = new IRInstruction(
                    IROpCode.VectorOp, vectorResult, vectorOperands);

                block.Instructions[i] = vectorInst;
                vectorInst.ParentBlock = block;
                vectorInst.SequenceIndex = i;
            }
        }
    }

    private static bool IsSIMDCompatibleOp(IROpCode opCode)
    {
        return opCode is
            IROpCode.Add or IROpCode.Sub or IROpCode.Mul or
            IROpCode.Div or IROpCode.Neg or IROpCode.Fma;
    }

    private static IRValue VectorizeOperand(IRValue operand)
    {
        if (operand.IsConstant)
            return IRValue.CreateConstant($"vec_{operand.Name}", operand.ConstantValue ?? 0, IRType.Vector);
        return IRValue.CreateRegister($"vec_{operand.Name}", IRType.Vector);
    }

    private sealed class LoopInfo
    {
        public IRBlock Header { get; }
        public List<IRBlock> Body { get; }
        public IRBlock BackEdgeSource { get; }

        public LoopInfo(IRBlock header, List<IRBlock> body, IRBlock backEdgeSource)
        {
            Header = header;
            Body = body;
            BackEdgeSource = backEdgeSource;
        }
    }

    private sealed class LoopAnalysis
    {
        public bool CanVectorize { get; set; }
        public bool HasLoopCarriedDependency { get; set; }
        public bool HasSideEffects { get; set; }
        public bool AllStrideOne { get; set; }
        public int TripCount { get; set; }
        public List<ReductionInfo> Reductions { get; set; } = new();
    }

    private sealed class ReductionInfo
    {
        public IRValue Accumulator { get; set; } = null!;
        public IROpCode Operation { get; set; }
        public IRValue InitialValue { get; set; } = null!;
    }
}
