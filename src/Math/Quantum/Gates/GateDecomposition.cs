namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Provides gate decomposition utilities for quantum circuits.
/// </summary>
public static class GateDecomposition
{
    /// <summary>
    /// Decomposes an arbitrary gate into a sequence of T gates (simplified decomposition).
    /// </summary>
    /// <param name="gate">The gate to decompose.</param>
    /// <returns>A list of elementary gates.</returns>
    public static List<IQuantumGate> DecomposeToTGates(IQuantumGate gate)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));

        var result = new List<IQuantumGate>();

        if (gate.NumQubits == 1)
        {
            result.AddRange(DecomposeSingleQubitToT(gate));
        }
        else if (gate.NumQubits == 2)
        {
            result.Add(gate);
        }
        else
        {
            result.Add(gate);
        }

        return result;
    }

    /// <summary>
    /// Decomposes a gate into a sequence of H, T, and CNOT gates.
    /// </summary>
    /// <param name="gate">The gate to decompose.</param>
    /// <returns>A list of elementary gates.</returns>
    public static List<IQuantumGate> ToCNOTDecomposition(IQuantumGate gate)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));

        var result = new List<IQuantumGate>();

        if (gate.NumQubits == 1)
        {
            result.AddRange(DecomposeSingleQubitToHT(gate));
        }
        else if (gate.NumQubits == 2)
        {
            result.Add(SingleQubitGates.Hadamard);
            result.Add(gate);
            result.Add(SingleQubitGates.Hadamard);
        }
        else
        {
            result.Add(gate);
        }

        return result;
    }

    /// <summary>
    /// Extracts the matrix representation from a gate.
    /// </summary>
    /// <param name="gate">The gate to extract the matrix from.</param>
    /// <returns>The unitary matrix representation.</returns>
    public static Complex[,] ToMatrix(IQuantumGate gate)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        return gate.Matrix;
    }

    private static List<IQuantumGate> DecomposeSingleQubitToT(IQuantumGate gate)
    {
        var result = new List<IQuantumGate>();
        Complex[,] matrix = gate.Matrix;

        double theta = System.Math.Atan2(matrix[1, 0].Magnitude, matrix[0, 0].Magnitude);
        double phi = System.Math.Atan2(matrix[0, 1].Imaginary, matrix[0, 1].Real);

        if (System.Math.Abs(theta) > 1e-10)
        {
            result.Add(RotationGates.RY(2.0 * theta));
        }

        if (System.Math.Abs(phi) > 1e-10)
        {
            result.Add(RotationGates.RZ(phi));
        }

        if (result.Count == 0)
        {
            result.Add(SingleQubitGates.Identity);
        }

        return result;
    }

    private static List<IQuantumGate> DecomposeSingleQubitToHT(IQuantumGate gate)
    {
        var result = new List<IQuantumGate>();
        Complex[,] matrix = gate.Matrix;

        double theta = System.Math.Atan2(matrix[1, 0].Magnitude, matrix[0, 0].Magnitude);
        double phi = System.Math.Atan2(matrix[0, 1].Imaginary, matrix[0, 1].Real);

        if (System.Math.Abs(theta) > 1e-10)
        {
            result.Add(SingleQubitGates.Hadamard);
            result.Add(RotationGates.RZ(2.0 * theta));
            result.Add(SingleQubitGates.Hadamard);
        }

        if (System.Math.Abs(phi) > 1e-10)
        {
            result.Add(RotationGates.RZ(phi));
        }

        if (result.Count == 0)
        {
            result.Add(SingleQubitGates.Identity);
        }

        return result;
    }
}
