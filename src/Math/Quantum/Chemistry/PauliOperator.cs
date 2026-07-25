namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Enumeration of single-qubit Pauli labels for constructing Pauli string operators.
/// </summary>
public enum PauliLabel
{
    /// <summary>Identity operator.</summary>
    I,

    /// <summary>Pauli-X (bit-flip) operator.</summary>
    X,

    /// <summary>Pauli-Y operator.</summary>
    Y,

    /// <summary>Pauli-Z (phase-flip) operator.</summary>
    Z
}

/// <summary>
/// Represents a Pauli string operator as a tensor product of single-qubit Pauli matrices
/// with a scalar coefficient: P = c × (σ₁ ⊗ σ₂ ⊗ ... ⊗ σₙ).
/// </summary>
public sealed class PauliOperator
{
    private readonly PauliLabel[] _labels;

    /// <summary>Gets the scalar coefficient.</summary>
    public double Coefficient { get; }

    /// <summary>Gets the number of qubits this operator acts on.</summary>
    public int NumQubits => _labels.Length;

    /// <summary>Gets the Pauli label array.</summary>
    public ReadOnlySpan<PauliLabel> Labels => _labels;

    /// <summary>Creates a Pauli operator from a Pauli string (e.g., "XYZI").</summary>
    /// <param name="coefficient">The scalar coefficient.</param>
    /// <param name="pauliString">A string of Pauli labels: I, X, Y, Z.</param>
    public PauliOperator(double coefficient, string pauliString)
    {
        if (string.IsNullOrEmpty(pauliString)) throw new ArgumentNullException(nameof(pauliString));
        _labels = new PauliLabel[pauliString.Length];
        for (int i = 0; i < pauliString.Length; i++)
        {
            _labels[i] = pauliString[i] switch
            {
                'I' or 'i' => PauliLabel.I,
                'X' or 'x' => PauliLabel.X,
                'Y' or 'y' => PauliLabel.Y,
                'Z' or 'z' => PauliLabel.Z,
                _ => throw new ArgumentException($"Invalid Pauli label '{pauliString[i]}' at position {i}.", nameof(pauliString))
            };
        }
        Coefficient = coefficient;
    }

    /// <summary>Creates a Pauli operator from an array of Pauli labels.</summary>
    /// <param name="coefficient">The scalar coefficient.</param>
    /// <param name="labels">The Pauli labels for each qubit.</param>
    public PauliOperator(double coefficient, PauliLabel[] labels)
    {
        if (labels == null || labels.Length == 0) throw new ArgumentException("Labels cannot be null or empty.", nameof(labels));
        _labels = (PauliLabel[])labels.Clone();
        Coefficient = coefficient;
    }

    /// <summary>Computes the matrix representation of this Pauli operator for the given qubit count.</summary>
    /// <param name="numQubits">The total number of qubits in the system.</param>
    /// <returns>The matrix representation.</returns>
    public ComplexMatrix ToMatrix(int numQubits)
    {
        if (numQubits < NumQubits)
            throw new ArgumentOutOfRangeException(nameof(numQubits), $"Number of qubits ({numQubits}) must be ≥ operator width ({NumQubits}).");

        ComplexMatrix result = GetSinglePauliMatrix(_labels[0]);
        for (int i = 1; i < _labels.Length; i++)
        {
            result = result.TensorProduct(GetSinglePauliMatrix(_labels[i]));
        }

        for (int i = _labels.Length; i < numQubits; i++)
        {
            result = result.TensorProduct(GetSinglePauliMatrix(PauliLabel.I));
        }

        result = result.Scale(new Complex(Coefficient, 0.0));
        return result;
    }

    private static ComplexMatrix GetSinglePauliMatrix(PauliLabel label)
    {
        var data = new Complex[2, 2];
        switch (label)
        {
            case PauliLabel.I:
                data[0, 0] = Complex.One;
                data[1, 1] = Complex.One;
                break;
            case PauliLabel.X:
                data[0, 1] = Complex.One;
                data[1, 0] = Complex.One;
                break;
            case PauliLabel.Y:
                data[0, 1] = new Complex(0, -1);
                data[1, 0] = new Complex(0, 1);
                break;
            case PauliLabel.Z:
                data[0, 0] = Complex.One;
                data[1, 1] = -Complex.One;
                break;
        }
        return new ComplexMatrix(data);
    }
}
