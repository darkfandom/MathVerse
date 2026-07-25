namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Represents a pure quantum state |psi> as a normalized state vector.
/// </summary>
public sealed class PureState
{
    /// <summary>Gets the underlying state vector.</summary>
    public StateVector State { get; }

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => State.NumQubits;

    /// <summary>Creates a pure state from a state vector.</summary>
    public PureState(StateVector state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>Creates a pure state from a complex array.</summary>
    public PureState(Complex[] amplitudes) : this(new StateVector(amplitudes))
    {
    }

    /// <summary>Computes the tensor product with another pure state.</summary>
    public PureState TensorProduct(PureState other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return new PureState(State.TensorProduct(other.State));
    }

    /// <summary>Converts this pure state to a density matrix rho = |psi&gt;&lt;psi|.</summary>
    public DensityMatrix ToDensityMatrix()
    {
        return new DensityMatrix(State.Vector);
    }

    /// <summary>Returns the probability of measuring the specified bitstring.</summary>
    public double Probability(string bitstring)
    {
        if (bitstring == null) throw new ArgumentNullException(nameof(bitstring));
        if (bitstring.Length != NumQubits)
            throw new ArgumentException($"Bitstring length ({bitstring.Length}) must equal NumQubits ({NumQubits}).");

        int index = 0;
        for (int i = 0; i < bitstring.Length; i++)
        {
            char c = bitstring[bitstring.Length - 1 - i];
            if (c == '1')
                index |= 1 << i;
            else if (c != '0')
                throw new ArgumentException($"Invalid bit character '{c}' at position {i}.", nameof(bitstring));
        }
        return State.Probability(index);
    }
}
