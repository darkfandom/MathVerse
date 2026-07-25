namespace MathVerse.Math.Quantum.ErrorCorrection;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Utilities for measuring syndromes of quantum error-correcting codes
/// using stabilizer formalism on state vectors.
/// </summary>
public static class SyndromeMeasurement
{
    /// <summary>Measures X-type syndromes by computing parity overlaps on specified qubit pairs.</summary>
    /// <param name="state">The quantum state vector to measure.</param>
    /// <param name="stabilizerPairs">Array of qubit index pairs defining the stabilizers.</param>
    /// <returns>An array of syndrome bits (0 or 1) for each stabilizer.</returns>
    public static int[] MeasureXSyndromes(ComplexVector state, int[][] stabilizerPairs)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (stabilizerPairs == null) throw new ArgumentNullException(nameof(stabilizerPairs));

        int n = state.Dimension;
        int numQubits = 0;
        while ((1 << numQubits) < n) numQubits++;

        var syndromes = new int[stabilizerPairs.Length];
        for (int s = 0; s < stabilizerPairs.Length; s++)
        {
            int[] pairs = stabilizerPairs[s];
            double parity = 0.0;
            for (int basisIdx = 0; basisIdx < n; basisIdx++)
            {
                int flippedIdx = basisIdx;
                foreach (int qubit in pairs)
                {
                    if (qubit >= 0 && qubit < numQubits)
                        flippedIdx ^= 1 << qubit;
                }
                if (flippedIdx > basisIdx)
                {
                    double prob = state[basisIdx].Magnitude * state[basisIdx].Magnitude;
                    double probFlipped = state[flippedIdx].Magnitude * state[flippedIdx].Magnitude;
                    parity += prob + probFlipped;
                }
            }
            syndromes[s] = parity > 0.5 ? 1 : 0;
        }
        return syndromes;
    }

    /// <summary>Measures Z-type syndromes by computing phase parities on specified qubit pairs.</summary>
    /// <param name="state">The quantum state vector to measure.</param>
    /// <param name="stabilizerPairs">Array of qubit index pairs defining the stabilizers.</param>
    /// <returns>An array of syndrome bits (0 or 1) for each stabilizer.</returns>
    public static int[] MeasureZSyndromes(ComplexVector state, int[][] stabilizerPairs)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (stabilizerPairs == null) throw new ArgumentNullException(nameof(stabilizerPairs));

        int n = state.Dimension;
        int numQubits = 0;
        while ((1 << numQubits) < n) numQubits++;

        var syndromes = new int[stabilizerPairs.Length];
        for (int s = 0; s < stabilizerPairs.Length; s++)
        {
            int[] pairs = stabilizerPairs[s];
            Complex total = Complex.Zero;
            for (int basisIdx = 0; basisIdx < n; basisIdx++)
            {
                int parityCount = 0;
                foreach (int qubit in pairs)
                {
                    if (qubit >= 0 && qubit < numQubits && (basisIdx & (1 << qubit)) != 0)
                        parityCount++;
                }
                double sign = (parityCount % 2 == 0) ? 1.0 : -1.0;
                total += sign * state[basisIdx] * Complex.Conjugate(state[basisIdx]);
            }
            syndromes[s] = total.Real > 0.0 ? 0 : 1;
        }
        return syndromes;
    }

    /// <summary>Determines whether any error is detected from the syndrome.</summary>
    /// <param name="syndrome">The syndrome bit array.</param>
    /// <returns>True if any syndrome bit is non-zero; false otherwise.</returns>
    public static bool HasError(int[] syndrome)
    {
        if (syndrome == null) throw new ArgumentNullException(nameof(syndrome));
        for (int i = 0; i < syndrome.Length; i++)
        {
            if (syndrome[i] != 0) return true;
        }
        return false;
    }

    /// <summary>Locates the error index from a syndrome by interpreting it as a binary number.</summary>
    /// <param name="syndrome">The syndrome bit array.</param>
    /// <returns>The integer index of the error location, or -1 if no error.</returns>
    public static int LocateError(int[] syndrome)
    {
        if (syndrome == null) throw new ArgumentNullException(nameof(syndrome));
        if (!HasError(syndrome)) return -1;

        int index = 0;
        for (int i = 0; i < syndrome.Length; i++)
        {
            index |= syndrome[i] << i;
        }
        return index;
    }
}
