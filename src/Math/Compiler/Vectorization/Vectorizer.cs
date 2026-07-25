namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

public sealed class Vectorizer
{
    public IRModule Vectorize(IRModule module)
    {
        foreach (var func in module.Functions)
        {
            foreach (var block in func.Blocks)
            {
                VectorizeBlock(block);
            }
        }
        return module;
    }

    private void VectorizeBlock(IRBlock block)
    {
        var candidates = FindSIMDCandidates(block);
        foreach (var candidate in candidates)
        {
            var vectorized = TryVectorize(candidate);
            if (vectorized != null)
            {
                ReplaceWithVectorOp(block, candidate, vectorized);
            }
        }
    }

    private List<List<IRInstruction>> FindSIMDCandidates(IRBlock block)
    {
        var candidates = new List<List<IRInstruction>>();
        var currentGroup = new List<IRInstruction>();

        foreach (var inst in block.Instructions)
        {
            if (IsSIMDCandidate(inst))
            {
                currentGroup.Add(inst);
            }
            else
            {
                if (currentGroup.Count >= Vector<double>.Count)
                    candidates.Add(currentGroup);
                currentGroup = new List<IRInstruction>();
            }
        }

        if (currentGroup.Count >= Vector<double>.Count)
            candidates.Add(currentGroup);

        return candidates;
    }

    private static bool IsSIMDCandidate(IRInstruction inst)
    {
        return inst.OpCode is IROpCode.Add or IROpCode.Sub or IROpCode.Mul or IROpCode.Div
            && inst.Result?.Type is IRType.Vector or IRType.Float64;
    }

    private IRInstruction? TryVectorize(List<IRInstruction> instructions)
    {
        if (instructions.Count < Vector<double>.Count)
            return null;

        var first = instructions[0];
        if (first.Result == null) return null;

        var vectorResult = IRValue.CreateRegister($"{first.Result.Name}_vec", IRType.Vector);
        return new IRInstruction(IROpCode.VectorOp, vectorResult,
            first.Operands.Prepend(IRValue.CreateConstant((double)instructions.Count)).ToArray());
    }

    private static void ReplaceWithVectorOp(IRBlock block, List<IRInstruction> candidates, IRInstruction vectorized)
    {
        var insertIndex = block.Instructions.IndexOf(candidates[0]);
        foreach (var candidate in candidates)
            block.RemoveInstruction(candidate);
        block.InsertInstruction(Math.Max(0, insertIndex), vectorized);
    }
}
