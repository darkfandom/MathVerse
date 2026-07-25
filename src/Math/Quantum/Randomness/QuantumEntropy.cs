namespace MathVerse.Math.Quantum.Randomness;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Entropy source that generates quantum random bits and computes entropy measures
/// for randomness assessment and quality testing.
/// </summary>
public sealed class QuantumEntropy
{
    private readonly QuantumRandomGenerator _qrng;

    /// <summary>Creates a quantum entropy source.</summary>
    public QuantumEntropy()
    {
        _qrng = new QuantumRandomGenerator();
    }

    /// <summary>Computes the Shannon entropy of a byte array in bits per symbol.</summary>
    /// <param name="data">The data to analyze.</param>
    /// <returns>The Shannon entropy H(X) in bits.</returns>
    public double MeasureEntropy(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return 0.0;

        int[] frequencies = new int[256];
        for (int i = 0; i < data.Length; i++)
            frequencies[data[i]]++;

        double entropy = 0.0;
        double log2 = System.Math.Log(2.0);
        for (int i = 0; i < 256; i++)
        {
            if (frequencies[i] > 0)
            {
                double p = (double)frequencies[i] / data.Length;
                entropy -= p * System.Math.Log(p) / log2;
            }
        }
        return entropy;
    }

    /// <summary>Computes the min-entropy H∞(X) = -log₂(max P(x)), measuring worst-case unpredictability.</summary>
    /// <param name="data">The data to analyze.</param>
    /// <returns>The min-entropy in bits per symbol.</returns>
    public double MinEntropy(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return 0.0;

        int[] frequencies = new int[256];
        for (int i = 0; i < data.Length; i++)
            frequencies[data[i]]++;

        int maxFreq = 0;
        for (int i = 0; i < 256; i++)
        {
            if (frequencies[i] > maxFreq)
                maxFreq = frequencies[i];
        }

        double maxProb = (double)maxFreq / data.Length;
        if (maxProb <= 0.0) return 8.0;

        return -System.Math.Log(maxProb) / System.Math.Log(2.0);
    }

    /// <summary>Generates quantum random bits using Hadamard-based simulation.</summary>
    /// <param name="count">The number of random bits to generate.</param>
    /// <returns>An array of random booleans.</returns>
    public bool[] GenerateRandomBits(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var bits = new bool[count];
        for (int i = 0; i < count; i++)
            bits[i] = _qrng.NextBit();
        return bits;
    }

    /// <summary>Generates quantum random bytes using Hadamard-based simulation.</summary>
    /// <param name="count">The number of random bytes to generate.</param>
    /// <returns>An array of random bytes.</returns>
    public byte[] GenerateRandomBytes(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        return _qrng.NextBytes(count);
    }
}
