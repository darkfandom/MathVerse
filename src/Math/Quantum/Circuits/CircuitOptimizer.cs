namespace MathVerse.Math.Quantum.Circuits;

using System;
using System.Collections.Generic;
using System.Numerics;
using MathVerse.Math.Quantum.Gates;

/// <summary>
/// Provides circuit optimization techniques.
/// </summary>
public static class CircuitOptimizer
{
    private static readonly string[] SelfInverseGates = { "X", "Y", "Z", "H", "CNOT", "CX", "CY", "CZ", "CCX", "SWAP" };

    /// <summary>
    /// Applies all optimizations to a circuit.
    /// </summary>
    /// <param name="circuit">The circuit to optimize.</param>
    /// <returns>The optimized circuit.</returns>
    public static QuantumCircuit Optimize(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        QuantumCircuit result = CancelAdjacentGates(circuit);
        result = FuseGates(result);
        result = RemoveIdentityGates(result);
        return result;
    }

    /// <summary>
    /// Cancels adjacent inverse gate pairs (e.g., XX=I, HH=I).
    /// </summary>
    /// <param name="circuit">The circuit to optimize.</param>
    /// <returns>The optimized circuit.</returns>
    public static QuantumCircuit CancelAdjacentGates(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var result = new QuantumCircuit(circuit.NumQubits);
        var gates = new List<CircuitGate>(circuit.Gates);
        var removed = new bool[gates.Count];

        for (int i = 0; i < gates.Count; i++)
        {
            if (removed[i]) continue;

            bool cancelled = false;
            for (int j = i + 1; j < gates.Count; j++)
            {
                if (removed[j]) continue;

                if (AreOnSameQubits(gates[i], gates[j]) && AreSelfInverse(gates[i].Gate) && GatesEqual(gates[i].Gate, gates[j].Gate))
                {
                    removed[i] = true;
                    removed[j] = true;
                    cancelled = true;
                    break;
                }
            }

            if (!cancelled)
            {
                result.AddGate(gates[i].Gate, gates[i].QubitIndices);
            }
        }

        return result;
    }

    /// <summary>
    /// Fuses adjacent single-qubit gates into a single gate.
    /// </summary>
    /// <param name="circuit">The circuit to optimize.</param>
    /// <returns>The optimized circuit.</returns>
    public static QuantumCircuit FuseGates(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var result = new QuantumCircuit(circuit.NumQubits);
        var gates = new List<CircuitGate>(circuit.Gates);

        int i = 0;
        while (i < gates.Count)
        {
            if (gates[i].Gate.NumQubits == 1)
            {
                var fusedMatrix = gates[i].Gate.Matrix;
                int j = i + 1;

                while (j < gates.Count && gates[j].Gate.NumQubits == 1 && gates[j].QubitIndices[0] == gates[i].QubitIndices[0])
                {
                    fusedMatrix = MultiplyMatrices(gates[j].Gate.Matrix, fusedMatrix);
                    j++;
                }

                result.AddGate(new FusedGate(fusedMatrix), gates[i].QubitIndices);
                i = j;
            }
            else
            {
                result.AddGate(gates[i].Gate, gates[i].QubitIndices);
                i++;
            }
        }

        return result;
    }

    /// <summary>
    /// Removes identity gates from the circuit.
    /// </summary>
    /// <param name="circuit">The circuit to optimize.</param>
    /// <returns>The optimized circuit.</returns>
    public static QuantumCircuit RemoveIdentityGates(QuantumCircuit circuit)
    {
        if (circuit == null) throw new ArgumentNullException(nameof(circuit));

        var result = new QuantumCircuit(circuit.NumQubits);

        foreach (var gate in circuit.Gates)
        {
            if (gate.Gate.Name != "I" && gate.Gate.Name != "Barrier")
            {
                result.AddGate(gate.Gate, gate.QubitIndices);
            }
        }

        return result;
    }

    private static bool AreOnSameQubits(CircuitGate a, CircuitGate b)
    {
        if (a.QubitIndices.Length != b.QubitIndices.Length) return false;
        var setA = new HashSet<int>(a.QubitIndices);
        foreach (int q in b.QubitIndices)
        {
            if (!setA.Contains(q)) return false;
        }
        return true;
    }

    private static bool AreSelfInverse(IQuantumGate gate)
    {
        foreach (string name in SelfInverseGates)
        {
            if (gate.Name == name) return true;
        }
        return false;
    }

    private static bool GatesEqual(IQuantumGate a, IQuantumGate b)
    {
        if (a.Name != b.Name) return false;
        Complex[,] ma = a.Matrix;
        Complex[,] mb = b.Matrix;
        for (int i = 0; i < ma.GetLength(0); i++)
        {
            for (int j = 0; j < ma.GetLength(1); j++)
            {
                if (Complex.Abs(ma[i, j] - mb[i, j]) > 1e-10) return false;
            }
        }
        return true;
    }

    private static Complex[,] MultiplyMatrices(Complex[,] a, Complex[,] b)
    {
        int dim = a.GetLength(0);
        var result = new Complex[dim, dim];
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < dim; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    private sealed class FusedGate : IQuantumGate
    {
        private readonly Complex[,] _matrix;

        /// <summary>Initializes a new instance of the <see cref="FusedGate"/> class.</summary>
        /// <param name="matrix">The fused matrix.</param>
        public FusedGate(Complex[,] matrix)
        {
            _matrix = matrix;
        }

        /// <inheritdoc/>
        public string Name => "Fused";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => _matrix;

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) == 0)
                {
                    int j = i | mask;
                    Complex a = stateVector[i];
                    Complex b = stateVector[j];
                    stateVector[i] = _matrix[0, 0] * a + _matrix[0, 1] * b;
                    stateVector[j] = _matrix[1, 0] * a + _matrix[1, 1] * b;
                }
            }
        }
    }
}
