namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Bravyi-Kitaev transformation mapping fermionic operators to qubit operators
/// using a tree structure for more compact representations than Jordan-Wigner.
/// </summary>
public static class BravyiKitaev
{
    /// <summary>Transforms a string of fermionic operators to their Bravyi-Kitaev Pauli operator equivalents.</summary>
    /// <param name="operators">Array of operator strings, e.g., ["a†0", "a1"].</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>An array of <see cref="PauliOperator"/> representing the qubit operator.</returns>
    public static PauliOperator[] Transform(string[] operators, int numOrbitals)
    {
        if (operators == null || operators.Length == 0) throw new ArgumentException("Operators cannot be null or empty.", nameof(operators));
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        int[] map = GetTransformationMap(numOrbitals);
        var result = new PauliOperator[operators.Length];

        for (int op = 0; op < operators.Length; op++)
        {
            string ops = operators[op];
            bool isCreation = ops.StartsWith("a†", StringComparison.Ordinal) || ops.StartsWith("ad", StringComparison.OrdinalIgnoreCase);
            string indexStr = ops.Replace("a†", "").Replace("aA", "").Replace("a", "").Replace("†", "").Trim();
            int index = int.Parse(indexStr);

            if (index < 0 || index >= numOrbitals)
                throw new ArgumentOutOfRangeException(nameof(operators), $"Orbital index {index} out of range.");

            int numQubits = numOrbitals;
            var labels = new PauliLabel[numQubits];

            for (int q = 0; q < numQubits; q++)
                labels[q] = PauliLabel.I;

            int targetQubit = map[index];
            if (targetQubit >= 0 && targetQubit < numQubits)
                labels[targetQubit] = PauliLabel.X;

            for (int q = 0; q < index; q++)
            {
                int mappedQ = map[q];
                if (mappedQ >= 0 && mappedQ < numQubits)
                    labels[mappedQ] = PauliLabel.Z;
            }

            result[op] = new PauliOperator(isCreation ? 0.5 : 0.5, labels);
        }
        return result;
    }

    /// <summary>Transforms a fermionic operator string to a single qubit matrix via Bravyi-Kitaev.</summary>
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

    /// <summary>Computes the Bravyi-Kitaev transformation mapping from orbital index to qubit index.</summary>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>An array mapping each orbital index to its BK-transformed qubit index.</returns>
    public static int[] GetTransformationMap(int numOrbitals)
    {
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        var map = new int[numOrbitals];
        for (int i = 0; i < numOrbitals; i++)
        {
            map[i] = i;
        }

        int n = numOrbitals;
        int level = 1;
        while ((1 << level) <= n)
        {
            int step = 1 << level;
            int halfStep = 1 << (level - 1);
            for (int i = step - 1; i < n; i += step)
            {
                if (i - halfStep >= 0 && i - halfStep < numOrbitals)
                    map[i] = i - halfStep;
            }
            level++;
        }

        for (int i = 0; i < numOrbitals; i++)
        {
            if (map[i] < 0 || map[i] >= numOrbitals)
                map[i] = i % numOrbitals;
        }

        return map;
    }
}
