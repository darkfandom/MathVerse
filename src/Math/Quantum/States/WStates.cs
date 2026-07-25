namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Factory class for creating W states.
/// The n-qubit W state is an equal superposition of all single-excitation basis states:
/// |W_n> = (|100...0> + |010...0> + ... + |000...1>)/sqrt(n)
/// </summary>
public static class WStates
{
    /// <summary>
    /// Creates an n-qubit W state: equal superposition of all single-excitation states.
    /// </summary>
    public static StateVector Create(int numQubits)
    {
        if (numQubits < 1)
            throw new ArgumentOutOfRangeException(nameof(numQubits), "W state requires at least 1 qubit.");

        int dim = 1 << numQubits;
        var amps = new Complex[dim];
        double invNorm = 1.0 / System.Math.Sqrt((double)numQubits);

        for (int i = 0; i < numQubits; i++)
        {
            int basisIndex = 1 << i;
            amps[basisIndex] = new Complex(invNorm, 0.0);
        }
        return new StateVector(amps);
    }
}
