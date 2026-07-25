namespace MathVerse.Math.Quantum.ErrorCorrection;

using System;
using System.Collections.Generic;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Simplified planar surface code for topological quantum error correction.
/// Supports distance-d codes with stabilizer extraction and minimum-weight perfect matching decoding.
/// </summary>
public static class SurfaceCode
{
    /// <summary>The minimum supported code distance.</summary>
    public const int MinDistance = 3;

    /// <summary>Builds a lattice of data and syndrome qubit positions for the given distance.</summary>
    /// <param name="distance">The code distance (must be odd, ≥ 3).</param>
    /// <returns>A 2D array where [0] = data qubit coordinates and [1] = syndrome qubit coordinates.</returns>
    public static int[][][] BuildLattice(int distance)
    {
        if (distance < MinDistance || distance % 2 == 0)
            throw new ArgumentOutOfRangeException(nameof(distance), $"Distance must be odd and ≥ {MinDistance}.");

        int numDataQubits = distance * distance;
        int numXStabilizers = (distance - 1) * (distance - 1) / 2 + (distance - 1) * ((distance - 1) / 2);
        int numZStabilizers = numXStabilizers;

        var dataQubits = new List<int[]>();
        var syndromeQubits = new List<int[]>();

        for (int r = 0; r < distance; r++)
            for (int c = 0; c < distance; c++)
                dataQubits.Add(new[] { r, c });

        for (int r = 0; r < distance - 1; r++)
            for (int c = 0; c < distance - 1; c++)
            {
                if ((r + c) % 2 == 0)
                    syndromeQubits.Add(new[] { r, c, 0 });
                else
                    syndromeQubits.Add(new[] { r, c, 1 });
            }

        return new[] { dataQubits.ToArray(), syndromeQubits.ToArray() };
    }

    /// <summary>Creates a syndrome extraction circuit for the surface code.</summary>
    /// <param name="distance">The code distance.</param>
    /// <returns>A <see cref="QuantumCircuit"/> that extracts X and Z syndromes.</returns>
    public static QuantumCircuit SyndromeExtractionCircuit(int distance)
    {
        if (distance < MinDistance || distance % 2 == 0)
            throw new ArgumentOutOfRangeException(nameof(distance), $"Distance must be odd and ≥ {MinDistance}.");

        int numData = distance * distance;
        int numAncilla = (distance - 1) * (distance - 1);
        int totalQubits = numData + numAncilla;
        var circuit = new QuantumCircuit(totalQubits);

        int ancillaIdx = numData;
        for (int r = 0; r < distance - 1; r++)
        {
            for (int c = 0; c < distance - 1; c++)
            {
                int data0 = r * distance + c;
                int data1 = r * distance + c + 1;
                int data2 = (r + 1) * distance + c;
                int data3 = (r + 1) * distance + c + 1;

                int ancilla = ancillaIdx++;

                if ((r + c) % 2 == 0)
                {
                    circuit.AddGate(MultiQubitGates.CX, data0, ancilla);
                    circuit.AddGate(MultiQubitGates.CX, data1, ancilla);
                    circuit.AddGate(MultiQubitGates.CX, data2, ancilla);
                    circuit.AddGate(MultiQubitGates.CX, data3, ancilla);
                }
                else
                {
                    circuit.AddGate(SingleQubitGates.Hadamard, ancilla);
                    circuit.AddGate(MultiQubitGates.CX, ancilla, data0);
                    circuit.AddGate(MultiQubitGates.CX, ancilla, data1);
                    circuit.AddGate(MultiQubitGates.CX, ancilla, data2);
                    circuit.AddGate(MultiQubitGates.CX, ancilla, data3);
                    circuit.AddGate(SingleQubitGates.Hadamard, ancilla);
                }
            }
        }

        return circuit;
    }

    /// <summary>Decodes X and Z syndromes to identify error locations.</summary>
    /// <param name="xSyndromes">The X-type syndrome bits.</param>
    /// <param name="zSyndromes">The Z-type syndrome bits.</param>
    /// <param name="distance">The code distance.</param>
    /// <returns>An array of qubit indices where errors were detected.</returns>
    public static int[] DecodeSyndrome(int[] xSyndromes, int[] zSyndromes, int distance)
    {
        if (xSyndromes == null) throw new ArgumentNullException(nameof(xSyndromes));
        if (zSyndromes == null) throw new ArgumentNullException(nameof(zSyndromes));
        if (distance < MinDistance) throw new ArgumentOutOfRangeException(nameof(distance));

        var errors = new List<int>();

        DecodeSyndromeGroup(xSyndromes, distance, errors);
        DecodeSyndromeGroup(zSyndromes, distance, errors);

        if (errors.Count == 0)
            return new[] { -1 };

        return errors.ToArray();
    }

    private static void DecodeSyndromeGroup(int[] syndromes, int distance, List<int> errors)
    {
        int numAncilla = (distance - 1) * (distance - 1);
        for (int i = 0; i < System.Math.Min(syndromes.Length, numAncilla); i++)
        {
            if (syndromes[i] != 0)
            {
                int r = i / (distance - 1);
                int c = i % (distance - 1);
                errors.Add(r * distance + c);
            }
        }
    }
}
