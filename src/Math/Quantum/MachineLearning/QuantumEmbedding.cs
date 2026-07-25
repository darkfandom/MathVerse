namespace MathVerse.Math.Quantum.MachineLearning;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Provides static factory methods for quantum embedding circuits that encode
/// classical data into quantum states for variational quantum algorithms.
/// </summary>
public static class QuantumEmbedding
{
    /// <summary>Creates an angle encoding circuit that encodes each feature as an Rz rotation on a qubit.</summary>
    /// <param name="features">The feature values to encode.</param>
    /// <param name="numQubits">The number of qubits to use.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing angle encoding.</returns>
    public static QuantumCircuit AngleEncoding(double[] features, int numQubits)
    {
        if (features == null || features.Length == 0) throw new ArgumentException("Features cannot be null or empty.", nameof(features));
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));

        var circuit = new QuantumCircuit(numQubits);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        for (int i = 0; i < System.Math.Min(features.Length, numQubits); i++)
            circuit.AddGate(RotationGates.RZ(features[i]), i);

        return circuit;
    }

    /// <summary>Creates an amplitude embedding circuit that encodes features as state amplitudes.</summary>
    /// <param name="features">The features to encode as amplitudes (dimension must be a power of 2).</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing amplitude embedding.</returns>
    public static QuantumCircuit AmplitudeEmbedding(double[] features)
    {
        if (features == null || features.Length == 0) throw new ArgumentException("Features cannot be null or empty.", nameof(features));

        int dim = features.Length;
        int numQubits = 0;
        while ((1 << numQubits) < dim) numQubits++;
        if ((1 << numQubits) != dim)
            throw new ArgumentException($"Feature dimension ({dim}) must be a power of 2.", nameof(features));

        var circuit = new QuantumCircuit(numQubits);
        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);
        return circuit;
    }

    /// <summary>Creates a basis encoding circuit that encodes a bitstring as a computational basis state.</summary>
    /// <param name="bitstring">The bitstring to encode (0s and 1s).</param>
    /// <returns>A <see cref="QuantumCircuit"/> preparing the encoded basis state.</returns>
    public static QuantumCircuit BasisEncoding(int[] bitstring)
    {
        if (bitstring == null || bitstring.Length == 0) throw new ArgumentException("Bitstring cannot be null or empty.", nameof(bitstring));

        int numQubits = bitstring.Length;
        var circuit = new QuantumCircuit(numQubits);

        for (int q = 0; q < numQubits; q++)
        {
            if (bitstring[q] == 1)
                circuit.AddGate(SingleQubitGates.PauliX, q);
        }
        return circuit;
    }
}
