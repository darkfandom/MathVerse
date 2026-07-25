namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;

/// <summary>
/// Represents a quantum register of n qubits with 2ⁿ complex amplitudes.
/// </summary>
public sealed class QuantumRegister
{
    private readonly Complex[] _amplitudes;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the dimension of the Hilbert space (2^NumQubits).</summary>
    public int Dimension => _amplitudes.Length;

    /// <summary>Gets the amplitude array (read-only view).</summary>
    public ReadOnlySpan<Complex> Amplitudes => _amplitudes;

    /// <summary>Creates a quantum register from an array of amplitudes.</summary>
    public QuantumRegister(Complex[] amplitudes)
    {
        _amplitudes = amplitudes ?? throw new ArgumentNullException(nameof(amplitudes));
        if (amplitudes.Length == 0)
            throw new ArgumentException("Amplitude array cannot be empty.", nameof(amplitudes));

        int n = amplitudes.Length;
        int qubits = 0;
        while ((1 << qubits) < n) qubits++;
        if ((1 << qubits) != n)
            throw new ArgumentException($"Amplitude count ({n}) must be a power of 2.", nameof(amplitudes));
        NumQubits = qubits;
    }

    /// <summary>Creates a quantum register of the specified number of qubits, initialized to |0⟩.</summary>
    public QuantumRegister(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        NumQubits = numQubits;
        _amplitudes = new Complex[1 << numQubits];
        _amplitudes[0] = Complex.One;
    }

    /// <summary>Gets the amplitude at the specified basis index.</summary>
    public Complex GetAmplitude(int basisIndex)
    {
        if (basisIndex < 0 || basisIndex >= Dimension)
            throw new ArgumentOutOfRangeException(nameof(basisIndex));
        return _amplitudes[basisIndex];
    }

    /// <summary>Sets the amplitude at the specified basis index.</summary>
    public void SetAmplitude(int basisIndex, Complex value)
    {
        if (basisIndex < 0 || basisIndex >= Dimension)
            throw new ArgumentOutOfRangeException(nameof(basisIndex));
        _amplitudes[basisIndex] = value;
    }

    /// <summary>Returns the probability of measuring the specified basis state.</summary>
    public double Probability(int basisIndex)
    {
        Complex amp = GetAmplitude(basisIndex);
        return amp.Magnitude * amp.Magnitude;
    }

    /// <summary>Returns a normalized copy of this register.</summary>
    public QuantumRegister Normalize()
    {
        double sum = 0.0;
        for (int i = 0; i < _amplitudes.Length; i++)
            sum += _amplitudes[i].Magnitude * _amplitudes[i].Magnitude;
        double norm = System.Math.Sqrt(sum);
        if (norm < 1e-15) throw new InvalidOperationException("Cannot normalize a zero state.");

        var normalized = new Complex[_amplitudes.Length];
        for (int i = 0; i < _amplitudes.Length; i++)
            normalized[i] = _amplitudes[i] / norm;
        return new QuantumRegister(normalized);
    }

    /// <summary>Returns a deep copy of this register.</summary>
    public QuantumRegister Clone()
    {
        var copy = new Complex[_amplitudes.Length];
        Array.Copy(_amplitudes, copy, _amplitudes.Length);
        return new QuantumRegister(copy);
    }
}
