namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements the Quantum Fourier Transform (QFT) and its inverse.
/// The QFT maps |j⟩ → (1/√N) Σ_k e^{2πijk/N} |k⟩.
/// </summary>
public static class QuantumFourierTransform
{
    /// <summary>
    /// Builds the QFT circuit for the specified number of qubits.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the QFT.</returns>
    public static QuantumCircuit BuildQFT(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        var circuit = new QuantumCircuit(numQubits);

        for (int j = 0; j < numQubits; j++)
        {
            circuit.AddGate(SingleQubitGates.Hadamard, j);
            for (int k = j + 1; k < numQubits; k++)
            {
                double angle = System.Math.PI / (double)(1 << (k - j));
                circuit.AddGate(ParameterizedGates.ControlledPhase(angle), k, j);
            }
        }

        for (int i = 0; i < numQubits / 2; i++)
            circuit.AddGate(MultiQubitGates.Swap, i, numQubits - 1 - i);

        return circuit;
    }

    /// <summary>
    /// Builds the inverse QFT circuit for the specified number of qubits.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the inverse QFT.</returns>
    public static QuantumCircuit BuildInverseQFT(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        var circuit = new QuantumCircuit(numQubits);

        for (int i = 0; i < numQubits / 2; i++)
            circuit.AddGate(MultiQubitGates.Swap, i, numQubits - 1 - i);

        for (int j = numQubits - 1; j >= 0; j--)
        {
            for (int k = numQubits - 1; k > j; k--)
            {
                double angle = -System.Math.PI / (double)(1 << (k - j));
                circuit.AddGate(ParameterizedGates.ControlledPhase(angle), k, j);
            }
            circuit.AddGate(SingleQubitGates.Hadamard, j);
        }

        return circuit;
    }

    /// <summary>
    /// Applies the QFT directly to a state vector.
    /// </summary>
    /// <param name="state">The input state vector (dimension must be a power of 2).</param>
    /// <returns>The transformed state vector.</returns>
    public static ComplexVector Apply(ComplexVector state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        int n = state.Dimension;
        int numQubits = CountQubits(n);
        var result = ApplyQFTRecursive(state, numQubits);
        return new ComplexVector(result);
    }

    /// <summary>
    /// Applies the inverse QFT directly to a state vector.
    /// </summary>
    /// <param name="state">The input state vector (dimension must be a power of 2).</param>
    /// <returns>The transformed state vector.</returns>
    public static ComplexVector ApplyInverse(ComplexVector state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        int n = state.Dimension;
        int numQubits = CountQubits(n);
        var result = ApplyIQFTRecursive(state, numQubits);
        return new ComplexVector(result);
    }

    private static Complex[] ApplyQFTRecursive(ComplexVector state, int numQubits)
    {
        int n = 1 << numQubits;
        var input = new Complex[n];
        for (int i = 0; i < n; i++) input[i] = state[i];

        if (numQubits == 1)
        {
            var output = new Complex[2];
            double inv = 1.0 / System.Math.Sqrt(2.0);
            output[0] = new Complex(inv, 0) * (input[0] + input[1]);
            output[1] = new Complex(inv, 0) * (input[0] - input[1]);
            return output;
        }

        int half = n / 2;
        var even = new Complex[half];
        var odd = new Complex[half];
        for (int i = 0; i < half; i++)
        {
            even[i] = input[2 * i];
            odd[i] = input[2 * i + 1];
        }

        var evenVec = new ComplexVector(even);
        var oddVec = new ComplexVector(odd);
        var evenResult = ApplyQFTRecursive(evenVec, numQubits - 1);
        var oddResult = ApplyQFTRecursive(oddVec, numQubits - 1);

        var iqftResult = new Complex[n];
        for (int k = 0; k < half; k++)
        {
            double angle = 2.0 * System.Math.PI * k / n;
            var twiddle = Complex.FromPolarCoordinates(1.0, angle);
            iqftResult[k] = evenResult[k] + twiddle * oddResult[k];
            iqftResult[k + half] = evenResult[k] - twiddle * oddResult[k];
        }
        return iqftResult;
    }

    private static Complex[] ApplyIQFTRecursive(ComplexVector state, int numQubits)
    {
        int n = 1 << numQubits;
        var input = new Complex[n];
        for (int i = 0; i < n; i++) input[i] = state[i];

        if (numQubits == 1)
        {
            var result = new Complex[2];
            double inv = 1.0 / System.Math.Sqrt(2.0);
            result[0] = new Complex(inv, 0) * (input[0] + input[1]);
            result[1] = new Complex(inv, 0) * (input[0] - input[1]);
            return result;
        }

        int half = n / 2;
        for (int k = 0; k < half; k++)
        {
            double angle = -2.0 * System.Math.PI * k / n;
            var twiddle = Complex.FromPolarCoordinates(1.0, angle);
            var a = input[k];
            var b = input[k + half];
            input[k] = a + b;
            input[k + half] = twiddle * (a - b);
        }

        var even = new Complex[half];
        var odd = new Complex[half];
        for (int i = 0; i < half; i++)
        {
            even[i] = input[i];
            odd[i] = input[i + half];
        }

        var evenVec = new ComplexVector(even);
        var oddVec = new ComplexVector(odd);
        var evenResult = ApplyIQFTRecursive(evenVec, numQubits - 1);
        var oddResult = ApplyIQFTRecursive(oddVec, numQubits - 1);

        var output = new Complex[n];
        for (int i = 0; i < half; i++)
        {
            output[2 * i] = evenResult[i];
            output[2 * i + 1] = oddResult[i];
        }
        return output;
    }

    private static int CountQubits(int dimension)
    {
        if (dimension < 1 || (dimension & (dimension - 1)) != 0)
            throw new ArgumentException("Dimension must be a power of 2.", nameof(dimension));
        int count = 0;
        int d = dimension;
        while (d > 1) { d >>= 1; count++; }
        return count;
    }
}
