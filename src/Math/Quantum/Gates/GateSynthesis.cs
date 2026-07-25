namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Provides gate synthesis from unitary matrices.
/// </summary>
public static class GateSynthesis
{
    /// <summary>
    /// Decomposes a unitary matrix into elementary gates.
    /// </summary>
    /// <param name="unitary">The unitary matrix to decompose.</param>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>A list of elementary gates.</returns>
    public static List<IQuantumGate> Synthesize(Complex[,] unitary, int numQubits)
    {
        if (unitary == null) throw new ArgumentNullException(nameof(unitary));
        if (numQubits < 1) throw new ArgumentException("Number of qubits must be at least 1.", nameof(numQubits));

        int dim = 1 << numQubits;
        if (unitary.GetLength(0) != dim || unitary.GetLength(1) != dim)
        {
            throw new ArgumentException($"Matrix dimensions must be {dim}x{dim}.", nameof(unitary));
        }

        var result = new List<IQuantumGate>();

        if (numQubits == 1)
        {
            result.AddRange(ZYZDecomposition(unitary));
        }
        else
        {
            result.Add(new UnitaryGate(unitary, numQubits));
        }

        return result;
    }

    /// <summary>
    /// Decomposes a single-qubit unitary into Z-Y-Z rotation gates.
    /// </summary>
    /// <param name="singleQubitUnitary">The 2×2 unitary matrix.</param>
    /// <returns>A list of rotation gates.</returns>
    public static List<IQuantumGate> ZYZDecomposition(Complex[,] singleQubitUnitary)
    {
        if (singleQubitUnitary == null) throw new ArgumentNullException(nameof(singleQubitUnitary));
        if (singleQubitUnitary.GetLength(0) != 2 || singleQubitUnitary.GetLength(1) != 2)
        {
            throw new ArgumentException("Matrix must be 2×2.", nameof(singleQubitUnitary));
        }

        var result = new List<IQuantumGate>();

        Complex u00 = singleQubitUnitary[0, 0];
        Complex u01 = singleQubitUnitary[0, 1];
        Complex u10 = singleQubitUnitary[1, 0];
        Complex u11 = singleQubitUnitary[1, 1];

        double theta = 2.0 * System.Math.Acos(System.Math.Min(1.0, System.Math.Abs(u00.Magnitude)));
        double phi = System.Math.Atan2(u10.Imaginary, u10.Real);
        double lambda = System.Math.Atan2(u01.Imaginary, u01.Real);

        if (System.Math.Abs(theta) > 1e-10)
        {
            result.Add(RotationGates.RZ(phi));
            result.Add(RotationGates.RY(theta));
            result.Add(RotationGates.RZ(lambda));
        }
        else
        {
            result.Add(RotationGates.RZ(phi + lambda));
        }

        return result;
    }

    private sealed class UnitaryGate : IQuantumGate
    {
        private readonly Complex[,] _matrix;
        private readonly int _numQubits;

        /// <summary>Initializes a new instance of the <see cref="UnitaryGate"/> class.</summary>
        /// <param name="matrix">The unitary matrix.</param>
        /// <param name="numQubits">The number of qubits.</param>
        public UnitaryGate(Complex[,] matrix, int numQubits)
        {
            _matrix = matrix;
            _numQubits = numQubits;
        }

        /// <inheritdoc/>
        public string Name => "U";

        /// <inheritdoc/>
        public int NumQubits => _numQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix => _matrix;

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));

            int n = 1 << totalQubits;
            var temp = new Complex[n];
            Array.Copy(stateVector, temp, n);

            for (int i = 0; i < n; i++)
            {
                stateVector[i] = Complex.Zero;
                for (int j = 0; j < n; j++)
                {
                    stateVector[i] += _matrix[i, j] * temp[j];
                }
            }
        }
    }
}
