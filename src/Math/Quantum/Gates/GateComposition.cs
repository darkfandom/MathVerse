namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Provides gate composition utilities for building complex gates from simpler ones.
/// </summary>
public static class GateComposition
{
    /// <summary>
    /// Creates a gate that applies a sequence of gates in order.
    /// </summary>
    /// <param name="gates">The gates to apply sequentially.</param>
    /// <returns>An IQuantumGate implementing the composition.</returns>
    public static IQuantumGate Sequence(params IQuantumGate[] gates)
    {
        if (gates == null) throw new ArgumentNullException(nameof(gates));
        if (gates.Length == 0) throw new ArgumentException("At least one gate must be provided.", nameof(gates));

        return new SequenceGate(gates);
    }

    /// <summary>
    /// Creates a gate that applies two gates on different qubits in parallel.
    /// </summary>
    /// <param name="gate1">The first gate.</param>
    /// <param name="gate2">The second gate.</param>
    /// <returns>An IQuantumGate implementing the parallel application.</returns>
    public static IQuantumGate Parallel(IQuantumGate gate1, IQuantumGate gate2)
    {
        if (gate1 == null) throw new ArgumentNullException(nameof(gate1));
        if (gate2 == null) throw new ArgumentNullException(nameof(gate2));

        return new ParallelGate(gate1, gate2);
    }

    /// <summary>
    /// Multiplies the matrices of a sequence of gates.
    /// </summary>
    /// <param name="gates">The gates whose matrices to multiply.</param>
    /// <returns>The resulting matrix.</returns>
    public static Complex[,] ComposeMatrix(params IQuantumGate[] gates)
    {
        if (gates == null) throw new ArgumentNullException(nameof(gates));
        if (gates.Length == 0) throw new ArgumentException("At least one gate must be provided.", nameof(gates));

        Complex[,] result = gates[0].Matrix;
        for (int i = 1; i < gates.Length; i++)
        {
            result = MultiplyMatrices(gates[i].Matrix, result);
        }
        return result;
    }

    private static Complex[,] MultiplyMatrices(Complex[,] a, Complex[,] b)
    {
        int rows = a.GetLength(0);
        int cols = b.GetLength(1);
        int inner = a.GetLength(1);
        var result = new Complex[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < inner; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    private sealed class SequenceGate : IQuantumGate
    {
        private readonly IQuantumGate[] _gates;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="SequenceGate"/> class.</summary>
        /// <param name="gates">The gates in the sequence.</param>
        public SequenceGate(IQuantumGate[] gates)
        {
            _gates = gates;
        }

        /// <inheritdoc/>
        public string Name => $"Seq({string.Join("*", Array.ConvertAll(_gates, g => g.Name))})";

        /// <inheritdoc/>
        public int NumQubits => _gates[0].NumQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                if (_matrix == null)
                {
                    _matrix = _gates[0].Matrix;
                    for (int i = 1; i < _gates.Length; i++)
                    {
                        _matrix = MultiplyMatrices(_gates[i].Matrix, _matrix);
                    }
                }
                return _matrix;
            }
        }

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));

            for (int i = 0; i < _gates.Length; i++)
            {
                _gates[i].Apply(stateVector, qubitIndices, totalQubits);
            }
        }
    }

    private sealed class ParallelGate : IQuantumGate
    {
        private readonly IQuantumGate _gate1;
        private readonly IQuantumGate _gate2;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="ParallelGate"/> class.</summary>
        /// <param name="gate1">The first gate.</param>
        /// <param name="gate2">The second gate.</param>
        public ParallelGate(IQuantumGate gate1, IQuantumGate gate2)
        {
            _gate1 = gate1;
            _gate2 = gate2;
        }

        /// <inheritdoc/>
        public string Name => $"Parallel({_gate1.Name},{_gate2.Name})";

        /// <inheritdoc/>
        public int NumQubits => _gate1.NumQubits + _gate2.NumQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                if (_matrix == null)
                {
                    int dim1 = 1 << _gate1.NumQubits;
                    int dim2 = 1 << _gate2.NumQubits;
                    int totalDim = dim1 * dim2;
                    _matrix = new Complex[totalDim, totalDim];

                    for (int i = 0; i < dim1; i++)
                    {
                        for (int j = 0; j < dim1; j++)
                        {
                            for (int k = 0; k < dim2; k++)
                            {
                                for (int l = 0; l < dim2; l++)
                                {
                                    int row = i * dim2 + k;
                                    int col = j * dim2 + l;
                                    _matrix[row, col] = _gate1.Matrix[i, j] * _gate2.Matrix[k, l];
                                }
                            }
                        }
                    }
                }
                return _matrix;
            }
        }

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != NumQubits) throw new ArgumentException($"Expected {NumQubits} qubit indices.", nameof(qubitIndices));

            int[] indices1 = new int[_gate1.NumQubits];
            Array.Copy(qubitIndices, indices1, _gate1.NumQubits);
            int[] indices2 = new int[_gate2.NumQubits];
            Array.Copy(qubitIndices, _gate1.NumQubits, indices2, 0, _gate2.NumQubits);

            _gate1.Apply(stateVector, indices1, totalQubits);
            _gate2.Apply(stateVector, indices2, totalQubits);
        }
    }
}
