namespace MathVerse.Math.Quantum.States;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Represents a mixed quantum state described by a density matrix rho.
/// </summary>
public sealed class MixedState
{
    /// <summary>Gets the underlying density matrix.</summary>
    public DensityMatrix Matrix { get; }

    /// <summary>Gets the dimension of the Hilbert space.</summary>
    public int Dimension => Matrix.Dimension;

    /// <summary>Creates a mixed state from a density matrix.</summary>
    public MixedState(DensityMatrix matrix)
    {
        Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
    }

    /// <summary>Computes the fidelity F(rho, sigma) between this and another mixed state.</summary>
    public double Fidelity(MixedState other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return Matrix.Fidelity(other.Matrix);
    }

    /// <summary>Computes the purity Tr(rho^2).</summary>
    public double Purity()
    {
        return Matrix.Purity();
    }

    /// <summary>Computes the von Neumann entropy S(rho) = -Tr(rho * log2(rho)).</summary>
    public double VonNeumannEntropy()
    {
        double[] eigenvalues = EigenSolver.Eigenvalues(Matrix.Matrix);
        double entropy = 0.0;
        for (int i = 0; i < eigenvalues.Length; i++)
        {
            double lambda = eigenvalues[i];
            if (lambda > 1e-15)
                entropy -= lambda * System.Math.Log(lambda, 2.0);
        }
        return entropy;
    }
}
