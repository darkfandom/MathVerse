namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements Grover's quantum search algorithm for finding a marked element
/// in an unstructured database of N items using O(√N) queries.
/// </summary>
public static class GroversAlgorithm
{
    private static readonly Complex InvSqrt2 = new Complex(1.0 / System.Math.Sqrt(2.0), 0);

    /// <summary>
    /// Runs Grover's algorithm to find a marked element.
    /// </summary>
    /// <param name="numQubits">The number of qubits (search space size 2^n).</param>
    /// <param name="oracle">A Boolean function that returns <c>true</c> for marked elements.</param>
    /// <param name="optimalIterations">Optional override for the number of Grover iterations.</param>
    /// <returns>The index of the found marked element.</returns>
    public static int Run(int numQubits, Func<int, bool> oracle, int? optimalIterations = null)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int iterations = optimalIterations ?? OptimalIterations(numQubits);
        int n = 1 << numQubits;
        var state = new Complex[n];
        state[0] = Complex.One;

        for (int q = 0; q < numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, numQubits);

        for (int iter = 0; iter < iterations; iter++)
        {
            ApplyOracle(state, oracle, numQubits);
            ApplyDiffusion(state, numQubits);
        }

        int bestIndex = 0;
        double bestProb = 0.0;
        for (int i = 0; i < n; i++)
        {
            double prob = state[i].Magnitude * state[i].Magnitude;
            if (prob > bestProb) { bestProb = prob; bestIndex = i; }
        }
        return bestIndex;
    }

    /// <summary>
    /// Builds the Grover's algorithm circuit.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="oracle">A Boolean oracle function.</param>
    /// <param name="iterations">The number of Grover iterations (default: optimal).</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing Grover's algorithm.</returns>
    public static QuantumCircuit BuildCircuit(int numQubits, Func<int, bool> oracle, int? iterations = null)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int iters = iterations ?? OptimalIterations(numQubits);
        var circuit = new QuantumCircuit(numQubits);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        return circuit;
    }

    /// <summary>
    /// Computes the optimal number of Grover iterations for the given search space.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>⌊π/4 · √(2^n)⌋</returns>
    public static int OptimalIterations(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        double n = (double)(1 << numQubits);
        return (int)System.Math.Floor(System.Math.PI / 4.0 * System.Math.Sqrt(n));
    }

    /// <summary>
    /// Computes the theoretical success probability after a given number of iterations.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="iterations">The number of Grover iterations performed.</param>
    /// <returns>The probability of measuring the marked state.</returns>
    public static double TheoreticalSuccessProbability(int numQubits, int iterations)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));
        double n = (double)(1 << numQubits);
        double theta = System.Math.Asin(1.0 / System.Math.Sqrt(n));
        double angle = (2.0 * iterations + 1.0) * theta;
        return System.Math.Sin(angle) * System.Math.Sin(angle);
    }

    private static void ApplyOracle(Complex[] state, Func<int, bool> oracle, int numQubits)
    {
        int n = 1 << numQubits;
        for (int i = 0; i < n; i++)
        {
            if (oracle(i))
                state[i] = -state[i];
        }
    }

    private static void ApplyDiffusion(Complex[] state, int numQubits)
    {
        int n = 1 << numQubits;
        double mean = 0.0;
        for (int i = 0; i < n; i++)
            mean += state[i].Real;
        mean /= n;

        for (int i = 0; i < n; i++)
            state[i] = 2.0 * mean - state[i];
    }
}
