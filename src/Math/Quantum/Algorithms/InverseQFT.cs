namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using LinearAlgebra;

/// <summary>
/// Provides convenience methods for building and applying the inverse Quantum Fourier Transform.
/// </summary>
public static class InverseQFT
{
    /// <summary>
    /// Builds the inverse QFT circuit for the specified number of qubits.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the inverse QFT.</returns>
    public static QuantumCircuit Build(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        return QuantumFourierTransform.BuildInverseQFT(numQubits);
    }

    /// <summary>
    /// Applies the inverse QFT directly to a state vector.
    /// </summary>
    /// <param name="state">The input state vector (dimension must be a power of 2).</param>
    /// <returns>The inverse-transformed state vector.</returns>
    public static ComplexVector Apply(ComplexVector state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        return QuantumFourierTransform.ApplyInverse(state);
    }
}
