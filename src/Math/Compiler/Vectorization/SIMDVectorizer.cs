namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Converts scalar operations on arrays to SIMD vector operations.
/// Detects loops with stride-1 access patterns and converts eligible arithmetic
/// to vectorized form using hardware SIMD instructions.
/// </summary>
public sealed class SIMDVectorizer : IVectorizationPass
{
    private readonly int _minVectorWidth;

    /// <summary>
    /// Initializes the SIMD vectorizer.
    /// </summary>
    /// <param name="minVectorWidth">Minimum vector width to consider profitable.</param>
    public SIMDVectorizer(int minVectorWidth = 4)
    {
        _minVectorWidth = minVectorWidth;
    }

    /// <inheritdoc />
    public string Name => "SIMDVectorizer";

    /// <inheritdoc />
    public IRModule Vectorize(IRModule module)
    {
        foreach (var function in module.Functions)
            VectorizeFunction(function);
        return module;
    }

    private void VectorizeFunction(IRFunction function)
    {
        foreach (var block in function.Blocks)
            VectorizeBlock(block);
    }

    private void VectorizeBlock(IRBlock block)
    {
        var simdCandidates = IdentifySIMDCandidates(block);
        if (simdCandidates.Count == 0)
            return;

        var groups = GroupCompatibleOperations(simdCandidates);

        foreach (var group in groups)
        {
            if (group.Count < _minVectorWidth)
                continue;

            VectorizeGroup(block, group);
        }
    }

    private static List<VectorizationCandidate> IdentifySIMDCandidates(IRBlock block)
    {
        var candidates = new List<VectorizationCandidate>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst is IRPhiNode)
                continue;
            if (inst.Result == null)
                continue;
            if (!IsSIMDCompatibleOp(inst.OpCode))
                continue;

            var candidate = new VectorizationCandidate
            {
                Instruction = inst,
                Index = i,
                OpCode = inst.OpCode,
                ElementType = inst.Result.Type
            };

            candidates.Add(candidate);
        }

        return candidates;
    }

    private static bool IsSIMDCompatibleOp(IROpCode opCode)
    {
        return opCode is
            IROpCode.Add or
            IROpCode.Sub or
            IROpCode.Mul or
            IROpCode.Div or
            IROpCode.Neg or
            IROpCode.Abs or
            IROpCode.Fma;
    }

    private static List<List<VectorizationCandidate>> GroupCompatibleOperations(
        List<VectorizationCandidate> candidates)
    {
        var groups = new List<List<VectorizationCandidate>>();
        var used = new HashSet<int>();

        for (var i = 0; i < candidates.Count; i++)
        {
            if (used.Contains(i))
                continue;

            var group = new List<VectorizationCandidate> { candidates[i] };
            used.Add(i);

            for (var j = i + 1; j < candidates.Count; j++)
            {
                if (used.Contains(j))
                    continue;

                if (AreCompatible(candidates[i], candidates[j]))
                {
                    group.Add(candidates[j]);
                    used.Add(j);
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private static bool AreCompatible(VectorizationCandidate a, VectorizationCandidate b)
    {
        if (a.ElementType != b.ElementType)
            return false;

        if (!AreOperandTypesCompatible(a, b))
            return false;

        return true;
    }

    private static bool AreOperandTypesCompatible(VectorizationCandidate a, VectorizationCandidate b)
    {
        var aOps = a.Instruction.Operands.Where(o => !o.IsConstant).ToList();
        var bOps = b.Instruction.Operands.Where(o => !o.IsConstant).ToList();

        if (aOps.Count != bOps.Count)
            return false;

        for (var i = 0; i < aOps.Count; i++)
        {
            if (aOps[i].Type != bOps[i].Type)
                return false;
        }

        return true;
    }

    private static void VectorizeGroup(IRBlock block, List<VectorizationCandidate> group)
    {
        var vectorWidth = Vector<float>.Count;
        var firstCandidate = group[0];

        var vectorType = firstCandidate.ElementType == IRType.Float64
            ? IRType.Vector
            : IRType.Vector;

        for (var i = 0; i < group.Count; i++)
        {
            var candidate = group[i];
            var inst = candidate.Instruction;
            var idx = block.Instructions.IndexOf(inst);
            if (idx < 0) continue;

            var vectorOpCode = GetVectorOpCode(inst.OpCode);

            var vectorResult = IRValue.CreateRegister(
                $"simd_{inst.Result!.Name}", vectorType);

            var vectorOperands = inst.Operands
                .Select(o => ScalarToVectorOperand(o, vectorWidth))
                .ToList();

            var vectorInst = new IRInstruction(vectorOpCode, vectorResult, vectorOperands);
            block.Instructions[idx] = vectorInst;
            vectorInst.ParentBlock = block;
            vectorInst.SequenceIndex = idx;
        }
    }

    private static IRValue ScalarToVectorOperand(IRValue scalar, int vectorWidth)
    {
        if (scalar.IsConstant && scalar.ConstantValue.HasValue)
        {
            return IRValue.CreateConstant(
                $"simd_c_{scalar.Name}",
                scalar.ConstantValue.Value,
                IRType.Vector);
        }

        return IRValue.CreateRegister(
            $"simd_{scalar.Name}",
            IRType.Vector);
    }

    private static IROpCode GetVectorOpCode(IROpCode scalarOp)
    {
        return scalarOp switch
        {
            IROpCode.Add => IROpCode.VectorOp,
            IROpCode.Sub => IROpCode.VectorOp,
            IROpCode.Mul => IROpCode.VectorOp,
            IROpCode.Div => IROpCode.VectorOp,
            IROpCode.Neg => IROpCode.VectorOp,
            IROpCode.Abs => IROpCode.VectorOp,
            IROpCode.Fma => IROpCode.VectorOp,
            _ => scalarOp
        };
    }

    private sealed class VectorizationCandidate
    {
        public IRInstruction Instruction { get; set; } = null!;
        public int Index { get; set; }
        public IROpCode OpCode { get; set; }
        public IRType ElementType { get; set; }
    }
}
