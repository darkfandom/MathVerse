namespace MathVerse.Math.Quantum.MachineLearning;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Provides static factory methods for quantum feature maps that encode classical data
/// into quantum states for use in quantum machine learning algorithms.
/// </summary>
public static class QuantumFeatureMap
{
    /// <summary>Creates a ZZ-feature map that encodes data via ZZ-interactions and Hadamard layers.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="repetitions">The number of encoding repetitions.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the ZZ feature map.</returns>
    public static QuantumCircuit ZZFeatureMap(int numQubits, int repetitions = 2)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (repetitions < 1) throw new ArgumentOutOfRangeException(nameof(repetitions));

        var circuit = new QuantumCircuit(numQubits);

        for (int rep = 0; rep < repetitions; rep++)
        {
            for (int q = 0; q < numQubits; q++)
                circuit.AddGate(SingleQubitGates.Hadamard, q);

            for (int q = 0; q < numQubits; q++)
                circuit.AddGate(RotationGates.RZ(rep * (q + 1)), q);

            for (int i = 0; i < numQubits - 1; i++)
            {
                circuit.AddGate(MultiQubitGates.CX, i, i + 1);
                circuit.AddGate(RotationGates.RZ(rep + 1), i + 1);
                circuit.AddGate(MultiQubitGates.CX, i, i + 1);
            }

            for (int q = 0; q < numQubits; q++)
                circuit.AddGate(SingleQubitGates.Hadamard, q);
        }
        return circuit;
    }

    /// <summary>Creates an IQP (Instantaneous Quantum Polynomial) feature map.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the IQP feature map.</returns>
    public static QuantumCircuit IQPFeatureMap(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));

        var circuit = new QuantumCircuit(numQubits);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.TGate, q);

        for (int i = 0; i < numQubits - 1; i++)
        {
            circuit.AddGate(MultiQubitGates.CX, i, i + 1);
            circuit.AddGate(SingleQubitGates.TGate, i + 1);
            circuit.AddGate(MultiQubitGates.CX, i, i + 1);
        }

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.TGate, q);

        return circuit;
    }

    /// <summary>Creates a circuit that encodes classical data as quantum amplitudes.</summary>
    /// <param name="features">The feature vector to encode (dimension must be a power of 2).</param>
    /// <returns>A <see cref="QuantumCircuit"/> that prepares the encoded state.</returns>
    public static QuantumCircuit AmplitudeEncoding(double[] features)
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
}
