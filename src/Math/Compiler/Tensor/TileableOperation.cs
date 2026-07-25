namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>Represents a tiling-aware tensor operation that can be scheduled to optimize memory access.</summary>
public sealed class TileableOperation
{
    /// <summary>The type of tensor operation.</summary>
    public TensorOpType OpType { get; }
    /// <summary>Indices of inputs from the tensor graph.</summary>
    public IReadOnlyList<int> InputNodeIndices { get; }
    /// <summary>Preferred tile sizes for each dimension.</summary>
    public IReadOnlyList<int> TileSizes { get; }
    /// <summary>Original operation index in the tensor graph.</summary>
    public int OperationIndex { get; }
    /// <summary>The loop nesting order for this operation (indices of dimensions).</summary>
    public IReadOnlyList<int> LoopOrder { get; }
    /// <summary>Register blocking factor.</summary>
    public int RegisterBlockFactor { get; }

    /// <summary>Initializes a new instance of the <see cref="TileableOperation"/> class.</summary>
    public TileableOperation(TensorOpType opType, IEnumerable<int> inputs, IEnumerable<int> tileSizes, int operationIndex, IEnumerable<int> loopOrder, int registerBlockFactor = 1)
    {
        if (operationIndex < 0) throw new ArgumentOutOfRangeException(nameof(operationIndex));
        if (registerBlockFactor < 1) throw new ArgumentOutOfRangeException(nameof(registerBlockFactor));

        OpType = opType;
        InputNodeIndices = inputs?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(inputs));
        TileSizes = tileSizes?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(tileSizes));
        OperationIndex = operationIndex;
        LoopOrder = loopOrder?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(loopOrder));
        RegisterBlockFactor = registerBlockFactor;
    }

    /// <summary>Returns a human-readable string representation.</summary>
    public override string ToString()
    {
        return $"TileableOp[{OperationIndex}] {OpType} inputs={string.Join(",", InputNodeIndices)}";
    }
}
