namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Collections.Generic;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Represents a quantum Hamiltonian operator as a sum of weighted Pauli terms:
/// H = Σᵢ cᵢ Pᵢ, where Pᵢ are Pauli strings and cᵢ are real coefficients.
/// </summary>
public sealed class Hamiltonian
{
    private readonly List<(double Coefficient, ComplexMatrix PauliTerm)> _terms = new();

    /// <summary>Gets the number of qubits this Hamiltonian acts on.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the number of Pauli terms in the Hamiltonian.</summary>
    public int NumTerms => _terms.Count;

    /// <summary>Creates a Hamiltonian for the specified number of qubits.</summary>
    /// <param name="numQubits">The number of qubits in the system.</param>
    public Hamiltonian(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        NumQubits = numQubits;
    }

    /// <summary>Adds a Pauli term to the Hamiltonian: H += coefficient × pauliTerm.</summary>
    /// <param name="coefficient">The scalar coefficient cᵢ.</param>
    /// <param name="pauliTerm">The Pauli operator matrix Pᵢ.</param>
    public void AddTerm(double coefficient, ComplexMatrix pauliTerm)
    {
        if (pauliTerm == null) throw new ArgumentNullException(nameof(pauliTerm));
        int expectedDim = 1 << NumQubits;
        if (pauliTerm.Rows != expectedDim || pauliTerm.Cols != expectedDim)
            throw new ArgumentException($"Pauli term dimensions ({pauliTerm.Rows}×{pauliTerm.Cols}) must match Hamiltonian dimension ({expectedDim}×{expectedDim}).");
        _terms.Add((coefficient, pauliTerm));
    }

    /// <summary>Builds the full matrix representation of the Hamiltonian: H = Σᵢ cᵢ Pᵢ.</summary>
    /// <returns>The Hamiltonian matrix.</returns>
    public ComplexMatrix ToMatrix()
    {
        int dim = 1 << NumQubits;
        ComplexMatrix result = ComplexMatrix.Zero(dim, dim);
        for (int i = 0; i < _terms.Count; i++)
        {
            result = result.Add(_terms[i].PauliTerm.Scale(new Complex(_terms[i].Coefficient, 0.0)));
        }
        return result;
    }

    /// <summary>Gets the coefficient of the term at the specified index.</summary>
    /// <param name="index">The term index.</param>
    /// <returns>The coefficient value.</returns>
    public double GetCoefficient(int index)
    {
        if (index < 0 || index >= _terms.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return _terms[index].Coefficient;
    }

    /// <summary>Gets the Pauli term matrix at the specified index.</summary>
    /// <param name="index">The term index.</param>
    /// <returns>The Pauli term matrix.</returns>
    public ComplexMatrix GetTerm(int index)
    {
        if (index < 0 || index >= _terms.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return _terms[index].PauliTerm;
    }
}
