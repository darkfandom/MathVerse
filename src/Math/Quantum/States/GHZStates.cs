namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Factory class for creating GHZ (Greenberger-Horne-Zeilinger) states.
/// The n-qubit GHZ state is (|00...0> + |11...1>)/sqrt(2).
/// </summary>
public static class GHZStates
{
    /// <summary>
    /// Creates an n-qubit GHZ state: (|00...0> + |11...1>)/sqrt(2).
    /// </summary>
    public static StateVector Create(int numQubits)
    {
        if (numQubits < 2)
            throw new ArgumentOutOfRangeException(nameof(numQubits), "GHZ state requires at least 2 qubits.");

        int dim = 1 << numQubits;
        var amps = new Complex[dim];
        double invSqrt2 = 1.0 / System.Math.Sqrt(2.0);
        amps[0] = new Complex(invSqrt2, 0.0);
        amps[dim - 1] = new Complex(invSqrt2, 0.0);
        return new StateVector(amps);
    }
}
