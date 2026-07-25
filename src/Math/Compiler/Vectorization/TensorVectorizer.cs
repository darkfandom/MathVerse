namespace MathVerse.Math.Compiler.Vectorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Vectorizes tensor operations (matrix multiply, convolution) using SIMD-friendly
/// tiling. Decomposes large tensor operations into tiles that fit in SIMD registers
/// for efficient parallel computation.
/// </summary>
public sealed class TensorVectorizer : IVectorizationPass
{
    private readonly int _tileSize;

    /// <summary>
    /// Initializes the tensor vectorizer.
    /// </summary>
    /// <param name="tileSize">The tile size for decomposition (default: SIMD vector width).</param>
    public TensorVectorizer(int tileSize = 0)
    {
        _tileSize = tileSize > 0 ? tileSize : Vector<float>.Count;
    }

    /// <inheritdoc />
    public string Name => "TensorVectorizer";

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
        var tensorOps = IdentifyTensorOperations(block);
        if (tensorOps.Count == 0)
            return;

        foreach (var op in tensorOps)
        {
            switch (op.OpCode)
            {
                case IROpCode.MatMul:
                    VectorizeMatMul(block, op);
                    break;
                case IROpCode.Dot:
                    VectorizeDotProduct(block, op);
                    break;
                case IROpCode.Sum:
                    VectorizeSum(block, op);
                    break;
            }
        }
    }

    private static List<TensorOpInfo> IdentifyTensorOperations(IRBlock block)
    {
        var ops = new List<TensorOpInfo>();

        for (var i = 0; i < block.Instructions.Count; i++)
        {
            var inst = block.Instructions[i];
            if (inst is IRPhiNode)
                continue;
            if (inst.Result == null)
                continue;

            if (inst.OpCode is IROpCode.MatMul or IROpCode.Dot or IROpCode.Sum)
            {
                ops.Add(new TensorOpInfo
                {
                    Instruction = inst,
                    Index = i,
                    OpCode = inst.OpCode,
                    Result = inst.Result,
                    Operands = inst.Operands.ToList()
                });
            }
        }

        return ops;
    }

    private void VectorizeMatMul(IRBlock block, TensorOpInfo op)
    {
        var vectorWidth = _tileSize;
        var idx = op.Index;
        var result = op.Result;
        var left = op.Operands.Count > 0 ? op.Operands[0] : null;
        var right = op.Operands.Count > 1 ? op.Operands[1] : null;

        if (left == null || right == null)
            return;

        var tiledResult = IRValue.CreateRegister($"tmatmul_{result.Name}", IRType.Tensor);

        var tileOperands = new List<IRValue>
        {
            left,
            right,
            IRValue.CreateConstant($"tile_m_{result.Name}", vectorWidth, IRType.Int32),
            IRValue.CreateConstant($"tile_n_{result.Name}", vectorWidth, IRType.Int32),
            IRValue.CreateConstant($"tile_k_{result.Name}", vectorWidth, IRType.Int32)
        };

        var tileInst = new IRInstruction(IROpCode.MatMul, tiledResult, tileOperands);
        block.Instructions[idx] = tileInst;
        tileInst.ParentBlock = block;
        tileInst.SequenceIndex = idx;

        var vectorInst = new IRInstruction(IROpCode.VectorOp, result, tiledResult);
        block.InsertInstruction(idx + 1, vectorInst);
        vectorInst.ParentBlock = block;
    }

    private void VectorizeDotProduct(IRBlock block, TensorOpInfo op)
    {
        var vectorWidth = _tileSize;
        var idx = op.Index;
        var result = op.Result;

        if (op.Operands.Count < 2)
            return;

        var left = op.Operands[0];
        var right = op.Operands[1];

        var vectorResult = IRValue.CreateRegister($"tdot_{result.Name}", IRType.Vector);

        var tiledOperands = new List<IRValue>
        {
            left,
            right,
            IRValue.CreateConstant($"dot_width_{result.Name}", vectorWidth, IRType.Int32)
        };

        var vectorInst = new IRInstruction(IROpCode.VectorOp, vectorResult, tiledOperands);
        block.Instructions[idx] = vectorInst;
        vectorInst.ParentBlock = block;
        vectorInst.SequenceIndex = idx;

        var reduceInst = new IRInstruction(IROpCode.Sum, result, vectorResult);
        block.InsertInstruction(idx + 1, reduceInst);
        reduceInst.ParentBlock = block;
    }

    private void VectorizeSum(IRBlock block, TensorOpInfo op)
    {
        var vectorWidth = _tileSize;
        var idx = op.Index;
        var result = op.Result;

        if (op.Operands.Count < 1)
            return;

        var source = op.Operands[0];

        var vectorResult = IRValue.CreateRegister($"tsum_{result.Name}", IRType.Vector);

        var vectorOperands = new List<IRValue>
        {
            source,
            IRValue.CreateConstant($"sum_width_{result.Name}", vectorWidth, IRType.Int32)
        };

        var vectorInst = new IRInstruction(IROpCode.VectorOp, vectorResult, vectorOperands);
        block.Instructions[idx] = vectorInst;
        vectorInst.ParentBlock = block;
        vectorInst.SequenceIndex = idx;

        var reduceInst = new IRInstruction(IROpCode.Sum, result, vectorResult);
        block.InsertInstruction(idx + 1, reduceInst);
        reduceInst.ParentBlock = block;
    }

    private sealed class TensorOpInfo
    {
        public IRInstruction Instruction { get; set; } = null!;
        public int Index { get; set; }
        public IROpCode OpCode { get; set; }
        public IRValue Result { get; set; } = null!;
        public List<IRValue> Operands { get; set; } = new();
    }
}
