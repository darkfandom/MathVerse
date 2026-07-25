namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;

/// <summary>
/// Schedules gates into parallel layers for circuit execution.
/// </summary>
public static class CircuitScheduler
{
    /// <summary>
    /// Groups gates into parallel layers where gates in the same layer act on disjoint qubits.
    /// </summary>
    /// <param name="circuit">The circuit to schedule.</param>
    /// <returns>An array of layers, each layer being an array of gate indices.</returns>
    public static int[][] ScheduleLayers(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var layers = new List<List<int>>();
        var qubitOccupancy = new Dictionary<int, int>();

        for (int i = 0; i < circuit.GateCount; i++)
        {
            var gate = circuit.GetGate(i);
            int assignedLayer = -1;

            for (int l = 0; l < layers.Count; l++)
            {
                bool canFit = true;
                foreach (int q in gate.QubitIndices)
                {
                    if (qubitOccupancy.ContainsKey(q) && qubitOccupancy[q] >= l)
                    {
                        canFit = false;
                        break;
                    }
                }

                if (canFit)
                {
                    assignedLayer = l;
                    break;
                }
            }

            if (assignedLayer == -1)
            {
                assignedLayer = layers.Count;
                layers.Add(new List<int>());
            }

            layers[assignedLayer].Add(i);
            foreach (int q in gate.QubitIndices)
            {
                qubitOccupancy[q] = assignedLayer;
            }
        }

        var result = new int[layers.Count][];
        for (int i = 0; i < layers.Count; i++)
        {
            result[i] = layers[i].ToArray();
        }
        return result;
    }

    /// <summary>
    /// Computes the depth of the circuit.
    /// </summary>
    /// <param name="circuit">The circuit to analyze.</param>
    /// <returns>The circuit depth.</returns>
    public static int ComputeDepth(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));
        return circuit.Depth;
    }

    /// <summary>
    /// Gets the busy time for each qubit (number of layers in which it is used).
    /// </summary>
    /// <param name="circuit">The circuit to analyze.</param>
    /// <returns>A list where index i gives the busy time for qubit i.</returns>
    public static List<int> GetQubitBusyTimes(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var busyTimes = new List<int>(new int[circuit.NumQubits]);
        var qubitLastLayer = new int[circuit.NumQubits];

        for (int i = 0; i < circuit.GateCount; i++)
        {
            var gate = circuit.GetGate(i);
            foreach (int q in gate.QubitIndices)
            {
                busyTimes[q]++;
            }
        }

        return busyTimes;
    }
}
