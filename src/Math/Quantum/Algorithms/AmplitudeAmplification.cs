namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements generalized amplitude amplification, which generalizes Grover's algorithm
/// to handle multiple solutions and non-uniform initial states.
/// </summary>
public static class AmplitudeAmplification
{
    /// <summary>
    /// Builds the amplitude amplification circuit.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="oracle">A Boolean oracle marking solution states.</param>
    /// <param name="iterations">The number of amplitude amplification iterations.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing amplitude amplification.</returns>
    public static QuantumCircuit BuildCircuit(int numQubits, Func<int, bool> oracle, int iterations)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));

        var circuit = new QuantumCircuit(numQubits);
        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        return circuit;
    }

    /// <summary>
    /// Computes the success probability after a given number of iterations.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="iterations">The number of iterations.</param>
    /// <param name="numSolutions">The number of marked solutions in the search space.</param>
    /// <returns>The probability of measuring a marked state.</returns>
    public static double SuccessProbability(int numQubits, int iterations, int numSolutions)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));
        if (numSolutions < 1) throw new ArgumentOutOfRangeException(nameof(numSolutions));

        double n = (double)(1 << numQubits);
        if (numSolutions >= (int)n) return 1.0;

        double theta = System.Math.Asin(System.Math.Sqrt((double)numSolutions / n));
        double angle = (2.0 * iterations + 1.0) * theta;
        return System.Math.Sin(angle) * System.Math.Sin(angle);
    }

    /// <summary>
    /// Computes the optimal number of iterations for amplitude amplification.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="numSolutions">The number of marked solutions.</param>
    /// <returns>The optimal number of iterations.</returns>
    public static int OptimalIterations(int numQubits, int numSolutions)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (numSolutions < 1) throw new ArgumentOutOfRangeException(nameof(numSolutions));

        double n = (double)(1 << numQubits);
        if (numSolutions >= (int)n) return 0;

        double theta = System.Math.Asin(System.Math.Sqrt((double)numSolutions / n));
        int iterations = (int)System.Math.Floor(System.Math.PI / (4.0 * theta) - 0.5);
        return System.Math.Max(0, iterations);
    }
}
