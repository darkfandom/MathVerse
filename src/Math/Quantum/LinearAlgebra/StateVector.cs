namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Represents a quantum state vector |ψ⟩ in a 2ⁿ-dimensional Hilbert space.
/// </summary>
public sealed class StateVector
{
    private readonly ComplexVector _vector;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the dimension of the Hilbert space (2^NumQubits).</summary>
    public int Dimension => _vector.Dimension;

    /// <summary>Gets the underlying complex vector.</summary>
    public ComplexVector Vector => _vector;

    /// <summary>Gets the amplitude at the specified basis index.</summary>
    public Complex this[int index] => _vector[index];

    /// <summary>Creates a state vector from a complex vector.</summary>
    public StateVector(ComplexVector vector)
    {
        _vector = vector ?? throw new ArgumentNullException(nameof(vector));
        if (vector.Dimension == 0)
            throw new ArgumentException("State vector dimension must be positive.", nameof(vector));

        int n = vector.Dimension;
        int qubits = 0;
        while ((1 << qubits) < n) qubits++;
        if ((1 << qubits) != n)
            throw new ArgumentException($"Vector dimension ({n}) must be a power of 2.", nameof(vector));
        NumQubits = qubits;
    }

    /// <summary>Creates a state vector from a complex array.</summary>
    public StateVector(Complex[] amplitudes) : this(new ComplexVector(amplitudes))
    {
    }

    /// <summary>Computes the probability of measuring the specified basis state.</summary>
    public double Probability(int basisIndex)
    {
        if (basisIndex < 0 || basisIndex >= Dimension)
            throw new ArgumentOutOfRangeException(nameof(basisIndex));
        Complex amp = _vector[basisIndex];
        return amp.Magnitude * amp.Magnitude;
    }

    /// <summary>Returns all probabilities for each basis state.</summary>
    public double[] Probabilities()
    {
        var probs = new double[Dimension];
        for (int i = 0; i < Dimension; i++)
            probs[i] = Probability(i);
        return probs;
    }

    /// <summary>Returns a normalized copy of this state vector.</summary>
    public StateVector Normalize()
    {
        return new StateVector(_vector.Normalize());
    }

    /// <summary>Computes the tensor product with another state vector.</summary>
    public StateVector TensorProduct(StateVector other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return new StateVector(_vector.TensorProduct(other._vector));
    }

    /// <summary>Creates the zero state |00...0⟩.</summary>
    public static StateVector Zero(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        var amps = new Complex[1 << numQubits];
        amps[0] = Complex.One;
        return new StateVector(amps);
    }

    /// <summary>Creates the uniform superposition state |+⟩⊗n = H⊗n|0⟩⊗n.</summary>
    public static StateVector PlusState(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        int dim = 1 << numQubits;
        var amps = new Complex[dim];
        double norm = 1.0 / System.Math.Sqrt(dim);
        for (int i = 0; i < dim; i++)
            amps[i] = new Complex(norm, 0.0);
        return new StateVector(amps);
    }
}
