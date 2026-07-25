namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Factory for creating controlled versions of quantum gates.
/// </summary>
public static class ControlledGateFactory
{
    /// <summary>
    /// Creates a controlled version of any gate with specified number of control qubits.
    /// </summary>
    /// <param name="gate">The gate to control.</param>
    /// <param name="numControlQubits">The number of control qubits.</param>
    /// <returns>An IQuantumGate implementing the controlled version.</returns>
    public static IQuantumGate CreateControlled(IQuantumGate gate, int numControlQubits)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        if (numControlQubits < 1) throw new ArgumentException("Number of control qubits must be at least 1.", nameof(numControlQubits));

        return new ControlledGate(gate, numControlQubits);
    }

    /// <summary>
    /// Creates a controlled-X (CNOT) gate with specified control and target qubits.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="totalQubits">The total number of qubits in the system.</param>
    /// <returns>An IQuantumGate implementing the controlled-X.</returns>
    public static IQuantumGate ControlledX(int controlQubit, int targetQubit, int totalQubits)
    {
        return new ControlledSingleQubitGate(SingleQubitGates.PauliX, controlQubit, targetQubit, totalQubits);
    }

    /// <summary>
    /// Creates a controlled-Z gate with specified control and target qubits.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="totalQubits">The total number of qubits in the system.</param>
    /// <returns>An IQuantumGate implementing the controlled-Z.</returns>
    public static IQuantumGate ControlledZ(int controlQubit, int targetQubit, int totalQubits)
    {
        return new ControlledSingleQubitGate(SingleQubitGates.PauliZ, controlQubit, targetQubit, totalQubits);
    }

    private sealed class ControlledGate : IQuantumGate
    {
        private readonly IQuantumGate _gate;
        private readonly int _numControlQubits;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="ControlledGate"/> class.</summary>
        /// <param name="gate">The gate to control.</param>
        /// <param name="numControlQubits">The number of control qubits.</param>
        public ControlledGate(IQuantumGate gate, int numControlQubits)
        {
            _gate = gate;
            _numControlQubits = numControlQubits;
        }

        /// <inheritdoc/>
        public string Name => $"C{_numControlQubits}({_gate.Name})";

        /// <inheritdoc/>
        public int NumQubits => _numControlQubits + _gate.NumQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                if (_matrix == null)
                {
                    int dim = 1 << NumQubits;
                    int gateDim = 1 << _gate.NumQubits;
                    _matrix = new Complex[dim, dim];

                    for (int i = 0; i < dim; i++)
                    {
                        bool allControlsSet = true;
                        for (int c = 0; c < _numControlQubits; c++)
                        {
                            if ((i & (1 << c)) == 0)
                            {
                                allControlsSet = false;
                                break;
                            }
                        }

                        if (!allControlsSet)
                        {
                            _matrix[i, i] = 1;
                        }
                        else
                        {
                            int gateState = i >> _numControlQubits;
                            for (int j = 0; j < gateDim; j++)
                            {
                                int targetState = j;
                                int row = (i & ((1 << _numControlQubits) - 1)) | (gateState << _numControlQubits);
                                int col = (i & ((1 << _numControlQubits) - 1)) | (targetState << _numControlQubits);
                                _matrix[row, col] = _gate.Matrix[gateState, j];
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

            int[] controlIndices = new int[_numControlQubits];
            Array.Copy(qubitIndices, controlIndices, _numControlQubits);
            int[] targetIndices = new int[_gate.NumQubits];
            Array.Copy(qubitIndices, _numControlQubits, targetIndices, 0, _gate.NumQubits);

            int n = 1 << totalQubits;
            int controlMask = 0;
            foreach (int c in controlIndices)
            {
                controlMask |= 1 << c;
            }

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) == controlMask)
                {
                    _gate.Apply(stateVector, targetIndices, totalQubits);
                }
            }
        }
    }

    private sealed class ControlledSingleQubitGate : IQuantumGate
    {
        private readonly IQuantumGate _gate;
        private readonly int _controlQubit;
        private readonly int _targetQubit;
        private readonly int _totalQubits;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="ControlledSingleQubitGate"/> class.</summary>
        /// <param name="gate">The single-qubit gate to control.</param>
        /// <param name="controlQubit">The control qubit index.</param>
        /// <param name="targetQubit">The target qubit index.</param>
        /// <param name="totalQubits">The total number of qubits.</param>
        public ControlledSingleQubitGate(IQuantumGate gate, int controlQubit, int targetQubit, int totalQubits)
        {
            _gate = gate;
            _controlQubit = controlQubit;
            _targetQubit = targetQubit;
            _totalQubits = totalQubits;
        }

        /// <inheritdoc/>
        public string Name => $"C({_gate.Name})";

        /// <inheritdoc/>
        public int NumQubits => _totalQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                if (_matrix == null)
                {
                    int dim = 1 << _totalQubits;
                    _matrix = new Complex[dim, dim];
                    int controlMask = 1 << _controlQubit;
                    int targetMask = 1 << _targetQubit;

                    for (int i = 0; i < dim; i++)
                    {
                        if ((i & controlMask) == 0)
                        {
                            _matrix[i, i] = 1;
                        }
                        else
                        {
                            for (int a = 0; a < 2; a++)
                            {
                                for (int b = 0; b < 2; b++)
                                {
                                    int row = (i & ~targetMask) | (a << _targetQubit);
                                    int col = (i & ~targetMask) | (b << _targetQubit);
                                    _matrix[row, col] = _gate.Matrix[a, b];
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

            int controlMask = 1 << _controlQubit;
            int targetMask = 1 << _targetQubit;
            int n = 1 << totalQubits;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0)
                {
                    int targetBit = (i & targetMask) >> _targetQubit;
                    int iWithoutTarget = i & ~targetMask;

                    Complex sum = Complex.Zero;
                    for (int b = 0; b < 2; b++)
                    {
                        int col = iWithoutTarget | (b << _targetQubit);
                        sum += _gate.Matrix[targetBit, b] * stateVector[col];
                    }

                    stateVector[i] = sum;
                }
            }
        }
    }
}
