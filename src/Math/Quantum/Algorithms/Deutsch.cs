namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements Deutsch's algorithm for determining whether a 1-qubit Boolean function
/// is constant or balanced using a single quantum query.
/// </summary>
public static class DeutschAlgorithm
{
    /// <summary>
    /// Runs Deutsch's algorithm to determine whether the oracle function is constant or balanced.
    /// </summary>
    /// <param name="oracle">A Boolean function f:{0,1}→{0,1}.</param>
    /// <param name="inputQubit">The index of the input qubit (default 0).</param>
    /// <returns><c>true</c> if the function is constant; <c>false</c> if balanced.</returns>
    public static bool Run(Func<int, int> oracle, int inputQubit = 0)
    {
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));
        int numQubits = 2;
        int outputQubit = 1;
        var state = new Complex[1 << numQubits];
        state[0] = Complex.One;

        SingleQubitGates.Hadamard.Apply(state, new[] { inputQubit }, numQubits);
        SingleQubitGates.Hadamard.Apply(state, new[] { outputQubit }, numQubits);

        ApplyOracle(state, oracle, inputQubit, outputQubit, numQubits);

        SingleQubitGates.Hadamard.Apply(state, new[] { inputQubit }, numQubits);

        double probZero = 0.0;
        int inputMask = 1 << inputQubit;
        for (int i = 0; i < (1 << numQubits); i++)
        {
            if ((i & inputMask) == 0)
                probZero += state[i].Magnitude * state[i].Magnitude;
        }

        return probZero > 0.5;
    }

    /// <summary>
    /// Builds the quantum circuit for Deutsch's algorithm.
    /// </summary>
    /// <param name="oracle">A Boolean function f:{0,1}→{0,1}.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the algorithm.</returns>
    public static QuantumCircuit BuildCircuit(Func<int, int> oracle)
    {
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));
        var circuit = new QuantumCircuit(2);
        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        circuit.AddGate(SingleQubitGates.Hadamard, 1);

        int f0 = oracle(0);
        int f1 = oracle(1);
        if (f0 == 1) circuit.AddGate(SingleQubitGates.PauliX, 1);
        if (f0 != f1) circuit.AddGate(MultiQubitGates.CX, 0, 1);

        circuit.AddGate(SingleQubitGates.Hadamard, 0);
        return circuit;
    }

    private static void ApplyOracle(Complex[] state, Func<int, int> oracle, int inputQubit, int outputQubit, int numQubits)
    {
        int n = 1 << numQubits;
        int outputMask = 1 << outputQubit;
        for (int i = 0; i < n; i++)
        {
            int x = (i >> inputQubit) & 1;
            if (oracle(x) == 1)
            {
                int j = i ^ outputMask;
                (state[i], state[j]) = (state[j], state[i]);
            }
        }
    }
}
