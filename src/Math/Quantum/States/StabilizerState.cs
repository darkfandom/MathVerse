namespace MathVerse.Math.Quantum.States;

using System;

/// <summary>
/// Represents a stabilizer state as a read-only snapshot of stabilizer generators.
/// </summary>
public sealed class StabilizerState
{
    /// <summary>Gets the stabilizer generators as Pauli strings (e.g. "+XIZ", "-ZXY").</summary>
    public string[] StabilizerGenerators { get; }

    /// <summary>Gets the number of qubits in this stabilizer state.</summary>
    public int NumQubits { get; }

    /// <summary>Gets a value indicating whether this is the |+...+⟩ state (all X generators with positive phase).</summary>
    public bool IsPlusState { get; }

    /// <summary>Creates a stabilizer state from an array of Pauli-string generators.</summary>
    /// <param name="stabilizerGenerators">The stabilizer generators in Pauli-string format.</param>
    public StabilizerState(string[] stabilizerGenerators)
    {
        StabilizerGenerators = stabilizerGenerators ?? throw new ArgumentNullException(nameof(stabilizerGenerators));
        if (stabilizerGenerators.Length == 0)
            throw new ArgumentException("At least one stabilizer generator is required.", nameof(stabilizerGenerators));
        NumQubits = stabilizerGenerators[0].Length;
        IsPlusState = ComputeIsPlusState(stabilizerGenerators);
    }

    private static bool ComputeIsPlusState(string[] generators)
    {
        for (int i = 0; i < generators.Length; i++)
        {
            string gen = generators[i];
            if (gen.Length != generators.Length) return false;
            if (gen[0] != '+') return false;
            for (int j = 0; j < gen.Length; j++)
            {
                char expected = i == j ? 'X' : 'I';
                if (gen[j] != expected) return false;
            }
        }
        return true;
    }
}
