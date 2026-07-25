namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements the Bernstein-Vazirani algorithm for finding a hidden bit string s
/// such that f(x) = s·x (mod 2) using a single quantum query.
/// </summary>
public static class BernsteinVaziraniAlgorithm
{
    /// <summary>
    /// Runs the Bernstein-Vazirani algorithm to find the hidden string s.
    /// </summary>
    /// <param name="numQubits">The number of input qubits.</param>
    /// <param name="oracle">A function implementing f(x) = s·x mod 2 for a hidden string s.</param>
    /// <returns>The hidden bit string s as an integer array.</returns>
    public static int[] Run(int numQubits, Func<int[], int> oracle)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int totalQubits = numQubits + 1;
        int outputQubit = numQubits;
        int stateSize = 1 << totalQubits;
        var state = new Complex[stateSize];
        state[0] = Complex.One;

        for (int q = 0; q < totalQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        ApplyOracle(state, oracle, numQubits, outputQubit, totalQubits);

        for (int q = 0; q < numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        var result = new int[numQubits];
        double[] probs = new double[numQubits];
        for (int i = 0; i < stateSize; i++)
        {
            double amp = state[i].Magnitude * state[i].Magnitude;
            for (int q = 0; q < numQubits; q++)
            {
                if ((i & (1 << q)) != 0)
                    probs[q] += amp;
            }
        }
        for (int q = 0; q < numQubits; q++)
            result[q] = probs[q] > 0.5 ? 1 : 0;

        return result;
    }

    /// <summary>
    /// Builds the quantum circuit for the Bernstein-Vazirani algorithm.
    /// </summary>
    /// <param name="numQubits">The number of input qubits.</param>
    /// <param name="oracle">A function implementing f(x) = s·x mod 2.</param>
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
                    if (bits[q] == 0) circuit.AddGate(SingleQubitGates.PauliX, q);
                circuit.AddGate(MultiQubitGates.CCX, 0, 1, outputQubit);
                for (int q = 0; q < numQubits; q++)
                    if (bits[q] == 0) circuit.AddGate(SingleQubitGates.PauliX, q);
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
