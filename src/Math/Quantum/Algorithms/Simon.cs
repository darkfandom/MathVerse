namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements Simon's algorithm for finding a hidden period s of a function f
/// such that f(x) = f(x⊕s) for all x, using O(n) quantum queries.
/// </summary>
public static class SimonsAlgorithm
{
    /// <summary>
    /// Runs Simon's algorithm to find the hidden period s.
    /// </summary>
    /// <param name="numQubits">The number of input qubits n.</param>
    /// <param name="oracle">A function f:{0,1}^n→{0,1}^n with hidden period s.</param>
    /// <returns>The hidden period s as an integer array, or <c>null</c> if s=0.</returns>
    public static int[]? Run(int numQubits, Func<int[], int> oracle)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        var measurements = new List<int[]>();
        int maxAttempts = 10 * numQubits;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int[] measured = RunSingleShot(numQubits, oracle);
            measurements.Add(measured);

            if (measurements.Count >= numQubits && SolveFromMeasurements(measurements.ToArray(), numQubits))
            {
                var solution = SolveLinearSystem(measurements.ToArray(), numQubits);
                if (solution != null) return solution;
            }
        }

        if (measurements.Count > 0)
            return SolveLinearSystem(measurements.ToArray(), numQubits);

        return null;
    }

    /// <summary>
    /// Builds the quantum circuit for a single shot of Simon's algorithm.
    /// </summary>
    /// <param name="numQubits">The number of input qubits.</param>
    /// <param name="oracle">The oracle function f:{0,1}^n→{0,1}^n.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing one shot.</returns>
    public static QuantumCircuit BuildCircuit(int numQubits, Func<int[], int> oracle)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (oracle == null) throw new ArgumentNullException(nameof(oracle));

        int totalQubits = 2 * numQubits;
        var circuit = new QuantumCircuit(totalQubits);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        for (int q = 0; q < numQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        return circuit;
    }

    /// <summary>
    /// Determines whether the collected measurements contain enough information to solve for s.
    /// </summary>
    /// <param name="measurements">Array of measurement results (each is an n-bit vector).</param>
    /// <param name="numQubits">The number of input qubits.</param>
    /// <returns><c>true</c> if a full-rank system has been found.</returns>
    public static bool SolveFromMeasurements(int[][] measurements, int numQubits)
    {
        if (measurements == null) throw new ArgumentNullException(nameof(measurements));
        if (measurements.Length < numQubits) return false;

        int rank = ComputeRank(measurements, numQubits);
        return rank >= numQubits - 1;
    }

    private static int[] RunSingleShot(int numQubits, Func<int[], int> oracle)
    {
        int totalQubits = 2 * numQubits;
        int stateSize = 1 << totalQubits;
        var state = new Complex[stateSize];
        state[0] = Complex.One;

        for (int q = 0; q < numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

        ApplySimonOracle(state, oracle, numQubits, totalQubits);

        for (int q = 0; q < numQubits; q++)
            SingleQubitGates.Hadamard.Apply(state, new[] { q }, totalQubits);

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

        var result = new int[numQubits];
        for (int q = 0; q < numQubits; q++)
            result[q] = probs[q] > 0.5 ? 1 : 0;
        return result;
    }

    private static void ApplySimonOracle(Complex[] state, Func<int[], int> oracle, int numQubits, int totalQubits)
    {
        int n = 1 << totalQubits;
        var xBits = new int[numQubits];
        var fBits = new int[numQubits];
        for (int i = 0; i < n; i++)
        {
            for (int q = 0; q < numQubits; q++)
                xBits[q] = (i >> q) & 1;
            int fVal = oracle(xBits);
            for (int q = 0; q < numQubits; q++)
                fBits[q] = (fVal >> q) & 1;
            int target = 0;
            for (int q = 0; q < numQubits; q++)
                target |= fBits[q] << (numQubits + q);
            int j = i ^ target;
            if (j > i)
                (state[i], state[j]) = (state[j], state[i]);
        }
    }

    private static int ComputeRank(int[][] matrix, int numCols)
    {
        int rows = matrix.Length;
        int rank = 0;
        var work = new int[rows][];
        for (int i = 0; i < rows; i++)
        {
            work[i] = new int[numCols];
            Array.Copy(matrix[i], work[i], System.Math.Min(matrix[i].Length, numCols));
        }

        for (int col = 0; col < numCols && rank < rows; col++)
        {
            int pivot = -1;
            for (int row = rank; row < rows; row++)
            {
                if (work[row][col] == 1) { pivot = row; break; }
            }
            if (pivot == -1) continue;

            (work[rank], work[pivot]) = (work[pivot], work[rank]);
            for (int row = 0; row < rows; row++)
            {
                if (row != rank && work[row][col] == 1)
                {
                    for (int c = 0; c < numCols; c++)
                        work[row][c] ^= work[rank][c];
                }
            }
            rank++;
        }
        return rank;
    }

    private static int[]? SolveLinearSystem(int[][] measurements, int numQubits)
    {
        int rows = measurements.Length;
        if (rows == 0) return null;

        var augmented = new int[rows][];
        for (int i = 0; i < rows; i++)
        {
            augmented[i] = new int[numQubits + 1];
            Array.Copy(measurements[i], augmented[i], System.Math.Min(measurements[i].Length, numQubits));
        }

        int rank = 0;
        for (int col = 0; col < numQubits && rank < rows; col++)
        {
            int pivot = -1;
            for (int row = rank; row < rows; row++)
            {
                if (augmented[row][col] == 1) { pivot = row; break; }
            }
            if (pivot == -1) continue;

            (augmented[rank], augmented[pivot]) = (augmented[pivot], augmented[rank]);
            for (int row = 0; row < rows; row++)
            {
                if (row != rank && augmented[row][col] == 1)
                {
                    for (int c = 0; c <= numQubits; c++)
                        augmented[row][c] ^= augmented[rank][c];
                }
            }
            rank++;
        }

        var solution = new int[numQubits];
        for (int i = 0; i < rank && i < numQubits; i++)
        {
            int col = -1;
            for (int c = 0; c < numQubits; c++)
            {
                if (augmented[i][c] == 1) { col = c; break; }
            }
            if (col >= 0)
                solution[col] = augmented[i][numQubits];
        }
        return solution;
    }
}
