namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Static utility class for Kronecker product operations and controlled gate construction.
/// </summary>
public static class KroneckerOperations
{
    /// <summary>Computes the Kronecker product of two matrices.</summary>
    public static ComplexMatrix Kronecker(ComplexMatrix a, ComplexMatrix b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        return a.TensorProduct(b);
    }

    /// <summary>Computes the Kronecker power M⊗n of a matrix.</summary>
    public static ComplexMatrix KroneckerPower(ComplexMatrix m, int power)
    {
        if (m == null) throw new ArgumentNullException(nameof(m));
        if (power < 1) throw new ArgumentOutOfRangeException(nameof(power));
        if (m.Rows != m.Cols)
            throw new ArgumentException("Kronecker power requires a square matrix.", nameof(m));

        ComplexMatrix result = m;
        for (int i = 1; i < power; i++)
            result = result.TensorProduct(m);
        return result;
    }

    /// <summary>
    /// Builds a controlled gate acting on totalQubits qubits, where the first
    /// numControlQubits are control qubits and the target gate acts on the last qubit.
    /// </summary>
    public static ComplexMatrix BuildControlledGate(ComplexMatrix gate, int numControlQubits, int totalQubits)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        if (numControlQubits < 1)
            throw new ArgumentOutOfRangeException(nameof(numControlQubits));
        if (totalQubits < numControlQubits + 1)
            throw new ArgumentException("Total qubits must be greater than control qubits.");
        if (gate.Rows != 2 || gate.Cols != 2)
            throw new ArgumentException("Target gate must be a 2×2 single-qubit gate.", nameof(gate));

        int dim = 1 << totalQubits;
        var result = new Complex[dim, dim];

        for (int row = 0; row < dim; row++)
        {
            bool allControlsSet = true;
            for (int c = 0; c < numControlQubits; c++)
            {
                if (((row >> (totalQubits - 1 - c)) & 1) == 0)
                {
                    allControlsSet = false;
                    break;
                }
            }

            if (allControlsSet)
            {
                int targetBitPos = totalQubits - 1;
                int bit = (row >> targetBitPos) & 1;
                int colWithoutTarget = row & ~(1 << targetBitPos);

                for (int t = 0; t < 2; t++)
                {
                    int col = colWithoutTarget | (t << targetBitPos);
                    result[row, col] += gate[bit, t];
                }
            }
            else
            {
                result[row, row] = Complex.One;
            }
        }
        return new ComplexMatrix(result);
    }
}
