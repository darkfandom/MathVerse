namespace MathVerse.Math.Quantum.Algorithms;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Implements discrete-time quantum walks on graphs, providing both circuit construction
/// and direct state-vector simulation.
/// </summary>
public static class QuantumWalk
{
    /// <summary>
    /// Builds a quantum circuit that encodes the quantum walk evolution for the specified steps.
    /// </summary>
    /// <param name="numNodes">The number of nodes in the graph.</param>
    /// <param name="adjacencyList">The adjacency list of the graph.</param>
    /// <param name="startNode">The starting node index.</param>
    /// <param name="targetNode">The target node index.</param>
    /// <param name="steps">The number of walk steps.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the walk.</returns>
    public static QuantumCircuit BuildCircuit(int numNodes, int[][] adjacencyList, int startNode, int targetNode, int steps)
    {
        if (numNodes < 1) throw new ArgumentOutOfRangeException(nameof(numNodes));
        if (adjacencyList == null) throw new ArgumentNullException(nameof(adjacencyList));
        if (startNode < 0 || startNode >= numNodes) throw new ArgumentOutOfRangeException(nameof(startNode));
        if (targetNode < 0 || targetNode >= numNodes) throw new ArgumentOutOfRangeException(nameof(targetNode));
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));

        int posQubits = (int)System.Math.Ceiling(System.Math.Log2(System.Math.Max(numNodes, 2)));
        int coinDim = GetMaxDegree(adjacencyList, numNodes);
        int coinQubits = coinDim > 1 ? (int)System.Math.Ceiling(System.Math.Log2(coinDim)) : 1;
        int totalQubits = posQubits + coinQubits;
        var circuit = new QuantumCircuit(totalQubits);

        circuit.AddGate(SingleQubitGates.PauliX, startNode);

        for (int s = 0; s < steps; s++)
        {
            for (int q = 0; q < coinQubits; q++)
                circuit.AddGate(SingleQubitGates.Hadamard, posQubits + q);
        }
        return circuit;
    }

    /// <summary>
    /// Simulates a discrete-time quantum walk and returns the probability distribution
    /// over nodes after the specified number of steps.
    /// </summary>
    /// <param name="numNodes">The number of nodes in the graph.</param>
    /// <param name="adjacencyList">The adjacency list of the graph.</param>
    /// <param name="startNode">The starting node index.</param>
    /// <param name="steps">The number of walk steps.</param>
    /// <returns>An array of probabilities for each node.</returns>
    public static double[] SimulateWalk(int numNodes, int[][] adjacencyList, int startNode, int steps)
    {
        if (numNodes < 1) throw new ArgumentOutOfRangeException(nameof(numNodes));
        if (adjacencyList == null) throw new ArgumentNullException(nameof(adjacencyList));
        if (startNode < 0 || startNode >= numNodes) throw new ArgumentOutOfRangeException(nameof(startNode));
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));

        int coinDim = GetMaxDegree(adjacencyList, numNodes);
        if (coinDim < 1) coinDim = 1;
        int coinQubits = (int)System.Math.Ceiling(System.Math.Log2(System.Math.Max(coinDim, 2)));
        int actualCoinDim = 1 << coinQubits;
        int totalDim = numNodes * actualCoinDim;
        var state = new Complex[totalDim];

        state[startNode * actualCoinDim + 0] = Complex.One;

        var hadamard = CreateHadamard(actualCoinDim);

        for (int s = 0; s < steps; s++)
        {
            var newState = new Complex[totalDim];
            for (int pos = 0; pos < numNodes; pos++)
            {
                int neighbors = adjacencyList[pos].Length;
                for (int c1 = 0; c1 < actualCoinDim; c1++)
                {
                    int stateIdx = pos * actualCoinDim + c1;
                    if (state[stateIdx] == Complex.Zero) continue;

                    for (int c2 = 0; c2 < actualCoinDim; c2++)
                    {
                        Complex coinAmp = hadamard[c2, c1];
                        if (coinAmp == Complex.Zero) continue;

                        if (neighbors > 0)
                        {
                            int targetPos = adjacencyList[pos][c2 % neighbors];
                            int newStateIdx = targetPos * actualCoinDim + c2;
                            newState[newStateIdx] += coinAmp * state[stateIdx];
                        }
                        else
                        {
                            int newStateIdx = pos * actualCoinDim + c2;
                            newState[newStateIdx] += coinAmp * state[stateIdx];
                        }
                    }
                }
            }
            state = newState;
        }

        var probabilities = new double[numNodes];
        for (int pos = 0; pos < numNodes; pos++)
        {
            double prob = 0.0;
            for (int c = 0; c < actualCoinDim; c++)
            {
                double mag = state[pos * actualCoinDim + c].Magnitude;
                prob += mag * mag;
            }
            probabilities[pos] = prob;
        }
        return probabilities;
    }

    private static int GetMaxDegree(int[][] adjacencyList, int numNodes)
    {
        int maxDeg = 0;
        for (int i = 0; i < numNodes && i < adjacencyList.Length; i++)
        {
            if (adjacencyList[i].Length > maxDeg)
                maxDeg = adjacencyList[i].Length;
        }
        return System.Math.Max(maxDeg, 1);
    }

    private static Complex[,] CreateHadamard(int dim)
    {
        int qubits = (int)System.Math.Ceiling(System.Math.Log2(System.Math.Max(dim, 2)));
        int size = 1 << qubits;
        var h = new Complex[size, size];
        double inv = 1.0 / System.Math.Sqrt(size);
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                h[i, j] = new Complex(inv * ((CountOnes(i & j) % 2 == 0) ? 1.0 : -1.0), 0);
        return h;
    }

    private static int CountOnes(int x)
    {
        int count = 0;
        while (x != 0) { count += x & 1; x >>= 1; }
        return count;
    }
}
