namespace MathVerse.Math.Quantum.Randomness;

using System;

/// <summary>
/// Abstract base class for random bit sources, providing a uniform interface
/// for both quantum and classical pseudo-random number generators.
/// </summary>
public abstract class RandomBitSource
{
    /// <summary>Generates an array of random bits.</summary>
    /// <param name="count">The number of bits to generate.</param>
    /// <returns>An array of boolean random bits.</returns>
    public abstract bool[] GetBits(int count);

    /// <summary>Generates a random double in the range [0, 1).</summary>
    /// <returns>A uniformly distributed random double.</returns>
    public abstract double GetDouble();

    /// <summary>Generates a random integer in the range [min, max).</summary>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The exclusive upper bound.</param>
    /// <returns>A uniformly distributed random integer.</returns>
    public abstract int GetInt(int min, int max);

    /// <summary>Creates a quantum-backed random bit source using Hadamard gate simulation.</summary>
    /// <returns>A <see cref="RandomBitSource"/> backed by quantum randomness.</returns>
    public static RandomBitSource Quantum()
    {
        return new QuantumBitSourceImpl();
    }

    /// <summary>Creates a classical pseudo-random bit source.</summary>
    /// <param name="seed">The seed for the PRNG. Use 0 for a time-based seed.</param>
    /// <returns>A <see cref="RandomBitSource"/> backed by a classical PRNG.</returns>
    public static RandomBitSource Classical(int seed = 0)
    {
        return new ClassicalBitSourceImpl(seed);
    }

    private sealed class QuantumBitSourceImpl : RandomBitSource
    {
        private readonly QuantumRandomGenerator _qrng = new();

        public override bool[] GetBits(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            var bits = new bool[count];
            for (int i = 0; i < count; i++)
                bits[i] = _qrng.NextBit();
            return bits;
        }

        public override double GetDouble() => _qrng.NextDouble();

        public override int GetInt(int min, int max) => _qrng.NextInt(min, max);
    }

    private sealed class ClassicalBitSourceImpl : RandomBitSource
    {
        private readonly Random _rng;

        public ClassicalBitSourceImpl(int seed)
        {
            _rng = seed == 0 ? new Random() : new Random(seed);
        }

        public override bool[] GetBits(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            var bits = new bool[count];
            var bytes = new byte[(count + 7) / 8];
            _rng.NextBytes(bytes);
            for (int i = 0; i < count; i++)
                bits[i] = (bytes[i / 8] & (1 << (i % 8))) != 0;
            return bits;
        }

        public override double GetDouble() => _rng.NextDouble();

        public override int GetInt(int min, int max) => _rng.Next(min, max);
    }
}
