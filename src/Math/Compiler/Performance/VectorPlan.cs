namespace MathVerse.Math.Compiler.Performance;

using System;

/// <summary>Record describing a SIMD-vectorized operation plan.</summary>
public sealed record VectorPlan
{
    /// <summary>The name of the operation to be vectorized.</summary>
    public string OperationName { get; init; }

    /// <summary>The width of the SIMD vector (e.g., 128, 256, 512 bits).</summary>
    public int VectorWidth { get; init; }

    /// <summary>Number of elements processed per vector operation.</summary>
    public int ElementsPerVector { get; init; }

    /// <summary>Number of loop iterations required.</summary>
    public int LoopCount { get; init; }

    /// <summary>Initializes a new instance of the <see cref="VectorPlan"/> record.</summary>
    public VectorPlan(string operationName, int vectorWidth, int elementsPerVector, int loopCount)
    {
        if (string.IsNullOrEmpty(operationName)) throw new ArgumentException("Operation name must not be null or empty.", nameof(operationName));
        if (vectorWidth <= 0) throw new ArgumentOutOfRangeException(nameof(vectorWidth));
        if (elementsPerVector <= 0) throw new ArgumentOutOfRangeException(nameof(elementsPerVector));
        if (loopCount < 0) throw new ArgumentOutOfRangeException(nameof(loopCount));

        OperationName = operationName;
        VectorWidth = vectorWidth;
        ElementsPerVector = elementsPerVector;
        LoopCount = loopCount;
    }
}
