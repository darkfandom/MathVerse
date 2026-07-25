namespace MathVerse.Math.Quantum.Chemistry;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Provides static methods for constructing fermionic creation, annihilation,
/// number, and hopping operators in second quantization.
/// </summary>
public static class FermionicOperator
{
    /// <summary>Creates the fermionic creation operator a†ᵢ for orbital i.</summary>
    /// <param name="index">The orbital index.</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>The creation operator matrix.</returns>
    public static ComplexMatrix Creation(int index, int numOrbitals)
    {
        if (index < 0 || index >= numOrbitals) throw new ArgumentOutOfRangeException(nameof(index));
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        int dim = 1 << numOrbitals;
        var result = new Complex[dim, dim];

        for (int state = 0; state < dim; state++)
        {
            if ((state & (1 << index)) == 0)
            {
                int newState = state | (1 << index);
                int sign = ComputeFermionicSign(state, index);
                result[newState, state] = new Complex(sign, 0.0);
            }
        }
        return new ComplexMatrix(result);
    }

    /// <summary>Creates the fermionic annihilation operator aᵢ for orbital i.</summary>
    /// <param name="index">The orbital index.</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>The annihilation operator matrix.</returns>
    public static ComplexMatrix Annihilation(int index, int numOrbitals)
    {
        if (index < 0 || index >= numOrbitals) throw new ArgumentOutOfRangeException(nameof(index));
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        int dim = 1 << numOrbitals;
        var result = new Complex[dim, dim];

        for (int state = 0; state < dim; state++)
        {
            if ((state & (1 << index)) != 0)
            {
                int newState = state ^ (1 << index);
                int sign = ComputeFermionicSign(state, index);
                result[newState, state] = new Complex(sign, 0.0);
            }
        }
        return new ComplexMatrix(result);
    }

    /// <summary>Creates the number operator nᵢ = a†ᵢaᵢ for orbital i.</summary>
    /// <param name="index">The orbital index.</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>The number operator matrix.</returns>
    public static ComplexMatrix Number(int index, int numOrbitals)
    {
        if (index < 0 || index >= numOrbitals) throw new ArgumentOutOfRangeException(nameof(index));
        if (numOrbitals < 1) throw new ArgumentOutOfRangeException(nameof(numOrbitals));

        int dim = 1 << numOrbitals;
        var data = new Complex[dim, dim];

        for (int state = 0; state < dim; state++)
        {
            if ((state & (1 << index)) != 0)
                data[state, state] = Complex.One;
        }
        return new ComplexMatrix(data);
    }

    /// <summary>Creates the hopping operator: a†ᵢaⱼ + a†ⱼaᵢ.</summary>
    /// <param name="i">The first orbital index.</param>
    /// <param name="j">The second orbital index.</param>
    /// <param name="numOrbitals">The total number of orbitals.</param>
    /// <returns>The hopping operator matrix.</returns>
    public static ComplexMatrix Hopping(int i, int j, int numOrbitals)
    {
        if (i < 0 || i >= numOrbitals) throw new ArgumentOutOfRangeException(nameof(i));
        if (j < 0 || j >= numOrbitals) throw new ArgumentOutOfRangeException(nameof(j));
        if (i == j) throw new ArgumentException("Orbital indices must be different for hopping.");

        ComplexMatrix aDagI = Creation(i, numOrbitals);
        ComplexMatrix aJ = Annihilation(j, numOrbitals);
        ComplexMatrix aDagJ = Creation(j, numOrbitals);
        ComplexMatrix aI = Annihilation(i, numOrbitals);

        return aDagI.Multiply(aJ).Add(aDagJ.Multiply(aI));
    }

    private static int ComputeFermionicSign(int state, int targetQubit)
    {
        int count = 0;
        for (int q = 0; q < targetQubit; q++)
        {
            if ((state & (1 << q)) != 0)
                count++;
        }
        return (count % 2 == 0) ? 1 : -1;
    }
}
