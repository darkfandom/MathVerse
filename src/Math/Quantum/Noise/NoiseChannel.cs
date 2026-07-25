namespace MathVerse.Math.Quantum.Noise;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// General quantum noise channel implemented as a completely positive trace-preserving (CPTP) map
/// via Kraus operator representation: ρ → Σᵢ Kᵢ ρ Kᵢ†.
/// </summary>
public sealed class NoiseChannel
{
    private readonly ComplexMatrix[] _krausOperators;

    /// <summary>Gets the Kraus operators defining this channel.</summary>
    public ReadOnlySpan<ComplexMatrix> KrausOperators => _krausOperators;

    /// <summary>Gets the number of Kraus operators.</summary>
    public int NumOperators => _krausOperators.Length;

    /// <summary>Gets the dimension of the Hilbert space this channel acts on.</summary>
    public int Dimension => _krausOperators[0].Rows;

    /// <summary>Determines whether this channel is unitary (single Kraus operator with unitarity).</summary>
    public bool IsUnitary
    {
        get
        {
            if (_krausOperators.Length != 1) return false;
            ComplexMatrix k = _krausOperators[0];
            ComplexMatrix product = k.Multiply(k.ConjugateTranspose());
            ComplexMatrix identity = ComplexMatrix.Identity(k.Rows);
            for (int i = 0; i < k.Rows; i++)
                for (int j = 0; j < k.Cols; j++)
                    if ((product[i, j] - identity[i, j]).Magnitude > 1e-10)
                        return false;
            return true;
        }
    }

    /// <summary>Creates a noise channel from an array of Kraus operators.</summary>
    /// <param name="krausOperators">The Kraus operators Kᵢ for the channel.</param>
    public NoiseChannel(ComplexMatrix[] krausOperators)
    {
        if (krausOperators == null || krausOperators.Length == 0)
            throw new ArgumentException("Kraus operators cannot be null or empty.", nameof(krausOperators));
        int rows = krausOperators[0].Rows;
        int cols = krausOperators[0].Cols;
        for (int i = 0; i < krausOperators.Length; i++)
        {
            if (krausOperators[i] == null)
                throw new ArgumentNullException($"Kraus operator at index {i} is null.");
            if (krausOperators[i].Rows != rows || krausOperators[i].Cols != cols)
                throw new ArgumentException($"Kraus operator at index {i} has dimensions ({krausOperators[i].Rows}×{krausOperators[i].Cols}), expected ({rows}×{cols}).");
        }
        _krausOperators = (ComplexMatrix[])krausOperators.Clone();
    }

    /// <summary>Applies the noise channel to a density matrix: ρ → Σᵢ Kᵢ ρ Kᵢ†.</summary>
    /// <param name="densityMatrix">The input density matrix.</param>
    /// <returns>The output density matrix after applying the channel.</returns>
    public ComplexMatrix Apply(ComplexMatrix densityMatrix)
    {
        if (densityMatrix == null) throw new ArgumentNullException(nameof(densityMatrix));
        if (densityMatrix.Rows != Dimension || densityMatrix.Cols != Dimension)
            throw new ArgumentException($"Density matrix dimensions ({densityMatrix.Rows}×{densityMatrix.Cols}) must match channel dimension ({Dimension}).");

        ComplexMatrix result = ComplexMatrix.Zero(Dimension, Dimension);
        for (int i = 0; i < _krausOperators.Length; i++)
        {
            ComplexMatrix k = _krausOperators[i];
            ComplexMatrix kDag = k.ConjugateTranspose();
            ComplexMatrix term = k.Multiply(densityMatrix).Multiply(kDag);
            result = result.Add(term);
        }
        return result;
    }

    /// <summary>Creates an identity noise channel of the specified dimension.</summary>
    /// <param name="dimension">The Hilbert space dimension.</param>
    /// <returns>A noise channel that leaves the input unchanged.</returns>
    public static NoiseChannel Identity(int dimension)
    {
        return new NoiseChannel(new[] { ComplexMatrix.Identity(dimension) });
    }

    /// <summary>Composes two noise channels sequentially: first apply <paramref name="a"/>, then <paramref name="b"/>.</summary>
    /// <param name="a">The first noise channel.</param>
    /// <param name="b">The second noise channel.</param>
    /// <returns>A new noise channel equivalent to b(a(ρ)).</returns>
    public static NoiseChannel Compose(NoiseChannel a, NoiseChannel b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));
        if (a.Dimension != b.Dimension)
            throw new ArgumentException("Noise channels must have the same dimension.");

        int d = a.Dimension;
        var krausList = new ComplexMatrix[a.NumOperators * b.NumOperators];
        int idx = 0;
        for (int j = 0; j < b.NumOperators; j++)
        {
            for (int i = 0; i < a.NumOperators; i++)
            {
                krausList[idx++] = b._krausOperators[j].Multiply(a._krausOperators[i]);
            }
        }
        return new NoiseChannel(krausList);
    }
}
