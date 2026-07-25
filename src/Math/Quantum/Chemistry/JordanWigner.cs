namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Jordan-Wigner transformation mapping fermionic operators to qubit (Pauli) operators.
/// Maps a†ᵢ → (Xᵢ - iYᵢ)/2 ⊗ Z₀ ⊗ Z₁ ⊗ ... ⊗ Zᵢ₋₁.
/// </summary>
public static class JordanWigner
{
    /// <summary>Transforms a string of fermionic operators to their Pauli operator equivalents.</summary>
    /// <param name="operators">Array of operator strings, e.g., ["a†0", "a1"].</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>An array of <see cref="PauliOperator"/> representing the qubit operator.</returns>
    public static PauliOperator[] Transform(string[] operators, int numOrbitals)
    {
        if (operators == null || operators.Length == 0) throw new ArgumentException("Operators cannot be null or empty.", nameof(operators));
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        var result = new PauliOperator[operators.Length];
        for (int op = 0; op < operators.Length; op++)
        {
            string ops = operators[op];
            bool isCreation = ops.StartsWith("a†", StringComparison.Ordinal) || ops.StartsWith("ad", StringComparison.OrdinalIgnoreCase);
            string indexStr = ops.Replace("a†", "").Replace("aA", "").Replace("a", "").Replace("†", "").Trim();
            int index = int.Parse(indexStr);

            if (index < 0 || index >= numOrbitals)
                throw new ArgumentOutOfRangeException(nameof(operators), $"Orbital index {index} out of range.");

            var labels = new PauliLabel[numOrbitals];
            for (int q = 0; q < numOrbitals; q++)
                labels[q] = PauliLabel.Z;
            labels[index] = PauliLabel.X;

            if (isCreation)
            {
                result[op] = new PauliOperator(0.5, labels);
            }
            else
            {
                var yLabels = new PauliLabel[numOrbitals];
                Array.Copy(labels, yLabels, numOrbitals);
                yLabels[index] = PauliLabel.Y;
                result[op] = new PauliOperator(0.5, labels);
            }
        }
        return result;
    }

    /// <summary>Transforms a fermionic operator string to a single qubit matrix.</summary>
    /// <param name="operators">Array of operator strings to transform.</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>The matrix representation of the transformed operator.</returns>
    public static ComplexMatrix FermionicToQubit(string[] operators, int numOrbitals)
    {
        var pauliOps = Transform(operators, numOrbitals);
        if (pauliOps.Length == 0)
            return ComplexMatrix.Identity(1 << numOrbitals);

        ComplexMatrix result = pauliOps[0].ToMatrix(numOrbitals);
        for (int i = 1; i < pauliOps.Length; i++)
        {
            result = result.Multiply(pauliOps[i].ToMatrix(numOrbitals));
        }
        return result;
    }
}
