namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Factory for creating parameterized quantum gates.
/// </summary>
public static class ParameterizedGates
{
    /// <summary>
    /// Creates a rotation gate around the X-axis.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the rotation.</returns>
    public static IQuantumGate RotationX(double theta)
    {
        return RotationGates.RX(theta);
    }

    /// <summary>
    /// Creates a rotation gate around the Y-axis.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the rotation.</returns>
    public static IQuantumGate RotationY(double theta)
    {
        return RotationGates.RY(theta);
    }

    /// <summary>
    /// Creates a rotation gate around the Z-axis.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the rotation.</returns>
    public static IQuantumGate RotationZ(double theta)
    {
        return RotationGates.RZ(theta);
    }

    /// <summary>
    /// Creates a controlled phase rotation gate.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the controlled rotation.</returns>
    public static IQuantumGate ControlledPhase(double theta)
    {
        return new ControlledPhaseGate(theta);
    }

    /// <summary>
    /// Creates a controlled version of any gate.
    /// </summary>
    /// <param name="gate">The gate to control.</param>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="totalQubits">The total number of qubits in the system.</param>
    /// <returns>An IQuantumGate implementing the controlled version.</returns>
    public static IQuantumGate ControlledU(IQuantumGate gate, int controlQubit, int targetQubit, int totalQubits)
    {
        return new ControlledUGate(gate, controlQubit, targetQubit, totalQubits);
    }

    /// <summary>
    /// Creates a gate raised to an integer power.
    /// </summary>
    /// <param name="gate">The base gate.</param>
    /// <param name="power">The integer power.</param>
    /// <returns>An IQuantumGate implementing gate^power.</returns>
    public static IQuantumGate Power(IQuantumGate gate, int power)
    {
        return new PowerGate(gate, power);
    }

    private sealed class ControlledPhaseGate : IQuantumGate
    {
        private readonly double _theta;
        private readonly Complex _expTheta;

        /// <summary>Initializes a new instance of the <see cref="ControlledPhaseGate"/> class.</summary>
        /// <param name="theta">The rotation angle.</param>
        public ControlledPhaseGate(double theta)
        {
            _theta = theta;
            _expTheta = Complex.FromPolarCoordinates(1.0, theta);
        }

        /// <inheritdoc/>
        public string Name => $"CP({_theta:F4})";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, _expTheta }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("Controlled phase gate acts on exactly two qubits.", nameof(qubitIndices));

            int control = qubitIndices[0];
            int target = qubitIndices[1];
            int n = 1 << totalQubits;
            int controlMask = 1 << control;
            int targetMask = 1 << target;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0 && (i & targetMask) != 0)
                {
                    stateVector[i] = _expTheta * stateVector[i];
                }
            }
        }
    }

    private sealed class ControlledUGate : IQuantumGate
    {
        private readonly IQuantumGate _gate;
        private readonly int _controlQubit;
        private readonly int _targetQubit;
        private readonly int _totalQubits;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="ControlledUGate"/> class.</summary>
        /// <param name="gate">The gate to control.</param>
        /// <param name="controlQubit">The control qubit index.</param>
        /// <param name="targetQubit">The target qubit index.</param>
        /// <param name="totalQubits">The total number of qubits.</param>
        public ControlledUGate(IQuantumGate gate, int controlQubit, int targetQubit, int totalQubits)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
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
                    Complex[,] gateMatrix = _gate.Matrix;

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
                                    _matrix[row, col] = gateMatrix[a, b];
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

    private sealed class PowerGate : IQuantumGate
    {
        private readonly IQuantumGate _gate;
        private readonly int _power;
        private Complex[,]? _matrix;

        /// <summary>Initializes a new instance of the <see cref="PowerGate"/> class.</summary>
        /// <param name="gate">The base gate.</param>
        /// <param name="power">The integer power.</param>
        public PowerGate(IQuantumGate gate, int power)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
            _power = power;
        }

        /// <inheritdoc/>
        public string Name => $"({_gate.Name})^{_power}";

        /// <inheritdoc/>
        public int NumQubits => _gate.NumQubits;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                if (_matrix == null)
                {
                    int dim = 1 << _gate.NumQubits;
                    _matrix = IdentityMatrix(dim);

                    for (int p = 0; p < System.Math.Abs(_power); p++)
                    {
                        _matrix = MultiplyMatrices(_matrix, _gate.Matrix, dim);
                    }

                    if (_power < 0)
                    {
                        _matrix = ConjugateTranspose(_matrix, dim);
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

            int count = _power >= 0 ? _power : -_power;
            for (int p = 0; p < count; p++)
            {
                _gate.Apply(stateVector, qubitIndices, totalQubits);
            }

            if (_power < 0)
            {
                Complex[,] invMatrix = ConjugateTranspose(_gate.Matrix, 1 << _gate.NumQubits);
                ApplyMatrix(stateVector, invMatrix, qubitIndices, totalQubits);
            }
        }

        private static Complex[,] IdentityMatrix(int dim)
        {
            var m = new Complex[dim, dim];
            for (int i = 0; i < dim; i++) m[i, i] = 1;
            return m;
        }

        private static Complex[,] MultiplyMatrices(Complex[,] a, Complex[,] b, int dim)
        {
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

        private static Complex[,] ConjugateTranspose(Complex[,] m, int dim)
        {
            var result = new Complex[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    result[i, j] = Complex.Conjugate(m[j, i]);
                }
            }
            return result;
        }

        private static void ApplyMatrix(Complex[] stateVector, Complex[,] matrix, int[] qubitIndices, int totalQubits)
        {
            int n = 1 << totalQubits;
            var temp = new Complex[n];
            Array.Copy(stateVector, temp, n);

            for (int i = 0; i < n; i++)
            {
                stateVector[i] = Complex.Zero;
                for (int j = 0; j < n; j++)
                {
                    stateVector[i] += matrix[i, j] * temp[j];
                }
            }
        }
    }
}
