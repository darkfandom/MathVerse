namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements the Deutsch-Jozsa algorithm for determining whether an n-qubit
/// Boolean function is constant or balanced using a single quantum query.
/// </summary>
public static class DeutschJozsaAlgorithm
{
    /// <summary>
    /// Runs the Deutsch-Jozsa algorithm.
    /// </summary>
    /// <param name="numQubits">The number of input qubits n.</param>
    /// <param name="oracle">A Boolean function f:{0,1}^n→{0,1}.</param>
    /// <returns><c>true</c> if the function is constant; <c>false</c> if balanced.</returns>
    public static bool Run(int numQubits, Func<int[], int> oracle)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int totalQubits = numQubits + 1;
        int stateSize = 1 << totalQubits;
        int outputQubit = numQubits;
        var state = new Complex[stateSize];
        state[0] = Complex.One;

        for (int q = 0; q < totalQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        ApplyOracle(state, oracle, numQubits, outputQubit, totalQubits);

        for (int q = 0; q < numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        double probAllZero = 0.0;
        for (int i = 0; i < stateSize; i++)
        {
            bool allZero = true;
            for (int q = 0; q < numQubits; q++)
            {
                if ((i & (1 << q)) != 0) { allZero = false; break; }
            }
            if (allZero)
                probAllZero += state[i].Magnitude * state[i].Magnitude;
        }

        return probAllZero > 0.5;
    }

    /// <summary>
    /// Builds the quantum circuit for the Deutsch-Jozsa algorithm.
    /// </summary>
    /// <param name="numQubits">The number of input qubits.</param>
    /// <param name="oracle">A Boolean function f:{0,1}^n→{0,1}.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the algorithm.</returns>
    public static QuantumCircuit BuildCircuit(int numQubits, Func<int[], int> oracle)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int totalQubits = numQubits + 1;
        int outputQubit = numQubits;
        var circuit = new QuantumCircuit(totalQubits);

        for (int q = 0; q < totalQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        int stateSize = 1 << numQubits;
        for (int x = 0; x < stateSize; x++)
        {
            var bits = new int[numQubits];
            for (int q = 0; q < numQubits; q++)
                bits[q] = (x >> q) & 1;
            if (oracle(bits) == 1)
            {
                for (int q = 0; q < numQubits; q++)
                {
                    if (bits[q] == 0) circuit.AddGate(SingleQubitGates.PauliX, q);
                }
                if (numQubits == 1)
                    circuit.AddGate(MultiQubitGates.CX, 0, outputQubit);
                else if (numQubits == 2)
                    circuit.AddGate(MultiQubitGates.CCX, 0, 1, outputQubit);
                else
                    circuit.AddGate(MultiQubitGates.CCX, 0, 1, outputQubit);
                for (int q = 0; q < numQubits; q++)
                {
                    if (bits[q] == 0) circuit.AddGate(SingleQubitGates.PauliX, q);
                }
            }
        }

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        return circuit;
    }

    private static void ApplyOracle(Complex[] state, Func<int[], int> oracle, int numQubits, int outputQubit, int totalQubits)
    {
        int n = 1 << totalQubits;
        int outputMask = 1 << outputQubit;
        var bits = new int[numQubits];
        for (int i = 0; i < n; i++)
        {
            for (int q = 0; q < numQubits; q++)
                bits[q] = (i >> q) & 1;
            if (oracle(bits) == 1)
            {
                int j = i ^ outputMask;
                (state[i], state[j]) = (state[j], state[i]);
            }
        }
    }
}
