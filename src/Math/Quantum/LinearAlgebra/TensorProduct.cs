namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Static utility class for computing tensor products of matrices and vectors.
/// </summary>
public static class TensorProduct
{
    /// <summary>Computes the tensor (Kronecker) product of two matrices.</summary>
    public static ComplexMatrix Matrices(ComplexMatrix a, ComplexMatrix b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        return a.TensorProduct(b);
    }

    /// <summary>Computes the tensor product of two vectors.</summary>
    public static ComplexVector Vectors(ComplexVector a, ComplexVector b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        return a.TensorProduct(b);
    }

    /// <summary>Builds a multi-qubit gate from single-qubit gates via successive tensor products.</summary>
    public static ComplexMatrix MultiQubitGate(ComplexMatrix[] gates)
    {
        if (gates == null || gates.Length == 0)
            throw new ArgumentException("At least one gate must be provided.", nameof(gates));

        ComplexMatrix result = gates[0];
        for (int i = 1; i < gates.Length; i++)
            result = result.TensorProduct(gates[i]);
        return result;
    }
}
