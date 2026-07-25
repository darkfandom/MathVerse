namespace MathVerse.Math.Quantum.States;

using System;

/// <summary>
/// Represents a computational basis state as an integer bit string.
/// </summary>
public readonly record struct BasisState
{
    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits { get; }

    /// <summary>Gets the integer representation of the bit string.</summary>
    public int BitString { get; }

    /// <summary>Creates a basis state with the specified parameters.</summary>
    public BasisState(int numQubits, int bitString)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        int maxVal = 1 << numQubits;
        if (bitString < 0 || bitString >= maxVal)
            throw new ArgumentOutOfRangeException(nameof(bitString), $"Bit string must be in [0, {maxVal - 1}].");
        NumQubits = numQubits;
        BitString = bitString;
    }

    /// <summary>Returns the value of the bit at the specified position (0 = least significant).</summary>
    public bool GetBit(int position)
    {
        if (position < 0 || position >= NumQubits)
            throw new ArgumentOutOfRangeException(nameof(position));
        return ((BitString >> position) & 1) == 1;
    }

    /// <summary>Returns a new basis state with the specified bit flipped.</summary>
    public BasisState FlipBit(int position)
    {
        if (position < 0 || position >= NumQubits)
            throw new ArgumentOutOfRangeException(nameof(position));
        return new BasisState(NumQubits, BitString ^ (1 << position));
    }

    /// <summary>Creates a basis state from an integer bit string.</summary>
    public static BasisState FromBitString(int numQubits, int bitString) => new BasisState(numQubits, bitString);

    /// <summary>Creates a basis state from a boolean array (index 0 = least significant).</summary>
    public static BasisState FromBooleans(bool[] bits)
    {
        if (bits == null) throw new ArgumentNullException(nameof(bits));
        if (bits.Length == 0) throw new ArgumentException("Bits array cannot be empty.", nameof(bits));

        int value = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
                value |= 1 << i;
        }
        return new BasisState(bits.Length, value);
    }

    /// <summary>Returns the string representation |bitstring⟩.</summary>
    public override string ToString() => $"|{Convert.ToString(BitString, 2).PadLeft(NumQubits, '0')}⟩";
}
