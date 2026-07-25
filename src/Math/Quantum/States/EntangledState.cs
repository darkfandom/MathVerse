namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Represents an entangled multi-qubit state and provides entanglement diagnostics.
/// </summary>
public sealed class EntangledState
{
    /// <summary>Gets the state vector amplitudes.</summary>
    public ComplexVector Amplitudes { get; }

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Creates an entangled state from a complex vector.</summary>
    public EntangledState(ComplexVector amplitudes)
    {
        Amplitudes = amplitudes ?? throw new ArgumentNullException(nameof(amplitudes));
        int n = amplitudes.Dimension;
        int qubits = 0;
        while ((1 << qubits) < n) qubits++;
        if ((1 << qubits) != n)
            throw new ArgumentException($"Dimension ({n}) must be a power of 2.", nameof(amplitudes));
        NumQubits = qubits;
    }

    /// <summary>
    /// Determines whether this state is entangled by checking if it can be
    /// written as a tensor product of two subsystem states (Schmidt decomposition).
    /// </summary>
    public bool IsEntangled()
    {
        if (NumQubits < 2) return false;
        return SchmidtRank() > 1;
    }

    /// <summary>
    /// Computes the concurrence for a 2-qubit entangled state.
    /// Returns a value in [0, 1] where 0 = separable, 1 = maximally entangled.
    /// </summary>
    public double Concurrence()
    {
        if (NumQubits != 2)
            throw new InvalidOperationException("Concurrence is only defined for 2-qubit states.");

        double norm = Amplitudes.Norm();
        if (norm < 1e-15) return 0.0;

        Complex a00 = Amplitudes[0];
        Complex a01 = Amplitudes[1];
        Complex a10 = Amplitudes[2];
        Complex a11 = Amplitudes[3];

        double r00 = a00.Magnitude;
        double r01 = a01.Magnitude;
        double r10 = a10.Magnitude;
        double r11 = a11.Magnitude;

        double c1 = System.Math.Abs(r00 * r11 - r01 * r10);
        double c2 = System.Math.Abs(a00.Magnitude * a11.Magnitude - a01.Magnitude * a10.Magnitude);

        double[] sorted = { r00 * r11, r01 * r10, c1, c2 };
        Array.Sort(sorted);
        Array.Reverse(sorted);

        double lambda = sorted[0];
        if (norm > 1e-15)
            lambda = System.Math.Min(1.0, 2.0 * sorted[0] / (norm * norm));

        return System.Math.Max(0.0, System.Math.Min(1.0, lambda));
    }

    private int SchmidtRank()
    {
        if (NumQubits < 2) return 1;

        int dimA = 1 << (NumQubits / 2);
        int dimB = Amplitudes.Dimension / dimA;

        var matrix = new Complex[dimA, dimB];
        for (int i = 0; i < dimA; i++)
            for (int j = 0; j < dimB; j++)
                matrix[i, j] = Amplitudes[i * dimB + j];

        int rank = 0;
        double threshold = 1e-10;

        for (int col = 0; col < System.Math.Min(dimA, dimB); col++)
        {
            double maxVal = 0.0;
            int maxRow = -1;
            for (int row = col; row < dimA; row++)
            {
                double val = 0.0;
                for (int k = 0; k < dimB; k++)
                    val += matrix[row, k].Magnitude * matrix[row, k].Magnitude;
                if (val > maxVal) { maxVal = val; maxRow = row; }
            }

            if (maxVal < threshold) break;

            if (maxRow != col)
                for (int k = 0; k < dimB; k++)
                    (matrix[col, k], matrix[maxRow, k]) = (matrix[maxRow, k], matrix[col, k]);

            Complex pivot = matrix[col, col];
            if (pivot.Magnitude < threshold)
            {
                for (int c2 = col + 1; c2 < dimB; c2++)
                {
                    if (matrix[col, c2].Magnitude > threshold)
                    {
                        for (int r = 0; r < dimA; r++)
                            (matrix[r, col], matrix[r, c2]) = (matrix[r, c2], matrix[r, col]);
                        pivot = matrix[col, col];
                        break;
                    }
                }
            }

            if (pivot.Magnitude < threshold) break;

            for (int k = 0; k < dimB; k++)
                matrix[col, k] /= pivot;

            for (int row = 0; row < dimA; row++)
            {
                if (row == col) continue;
                Complex factor = matrix[row, col];
                if (factor.Magnitude < threshold) continue;
                for (int k = 0; k < dimB; k++)
                    matrix[row, k] -= factor * matrix[col, k];
            }

            rank++;
        }
        return rank;
    }
}
