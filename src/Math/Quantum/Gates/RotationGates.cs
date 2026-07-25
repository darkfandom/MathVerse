namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Provides parameterized rotation quantum gates.
/// </summary>
public static class RotationGates
{
    /// <summary>
    /// Creates a rotation gate around the X-axis: RX(θ) = e^{-iθX/2}.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the RX rotation.</returns>
    public static IQuantumGate RX(double theta)
    {
        return new RXGate(theta);
    }

    /// <summary>
    /// Creates a rotation gate around the Y-axis: RY(θ) = e^{-iθY/2}.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the RY rotation.</returns>
    public static IQuantumGate RY(double theta)
    {
        return new RYGate(theta);
    }

    /// <summary>
    /// Creates a rotation gate around the Z-axis: RZ(θ) = e^{-iθZ/2}.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>An IQuantumGate implementing the RZ rotation.</returns>
    public static IQuantumGate RZ(double theta)
    {
        return new RZGate(theta);
    }

    /// <summary>
    /// Creates a phase shift gate: [[1,0],[0,e^(iφ)]].
    /// </summary>
    /// <param name="phi">The phase angle in radians.</param>
    /// <returns>An IQuantumGate implementing the phase shift.</returns>
    public static IQuantumGate PhaseShift(double phi)
    {
        return new PhaseShiftGate(phi);
    }

    /// <summary>
    /// Creates a general single-qubit rotation gate U3(θ,φ,λ).
    /// </summary>
    /// <param name="theta">The polar angle θ in radians.</param>
    /// <param name="phi">The azimuthal angle φ in radians.</param>
    /// <param name="lambda">The global phase λ in radians.</param>
    /// <returns>An IQuantumGate implementing the U3 rotation.</returns>
    public static IQuantumGate U3(double theta, double phi, double lambda)
    {
        return new U3Gate(theta, phi, lambda);
    }

    private sealed class RXGate : IQuantumGate
    {
        private readonly double _theta;
        private readonly Complex _cosHalf;
        private readonly Complex _iSinHalf;

        /// <summary>Initializes a new instance of the <see cref="RXGate"/> class.</summary>
        /// <param name="theta">The rotation angle.</param>
        public RXGate(double theta)
        {
            _theta = theta;
            double half = theta / 2.0;
            _cosHalf = new Complex(System.Math.Cos(half), 0);
            _iSinHalf = new Complex(0, -System.Math.Sin(half));
        }

        /// <inheritdoc/>
        public string Name => $"RX({_theta:F4})";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { _cosHalf, _iSinHalf },
            { _iSinHalf, _cosHalf }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("RX gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = _cosHalf * a + _iSinHalf * b;
                    stateVector[j] = _iSinHalf * a + _cosHalf * b;
                }
            }
        }
    }

    private sealed class RYGate : IQuantumGate
    {
        private readonly double _theta;
        private readonly Complex _cosHalf;
        private readonly Complex _sinHalf;

        /// <summary>Initializes a new instance of the <see cref="RYGate"/> class.</summary>
        /// <param name="theta">The rotation angle.</param>
        public RYGate(double theta)
        {
            _theta = theta;
            double half = theta / 2.0;
            _cosHalf = new Complex(System.Math.Cos(half), 0);
            _sinHalf = new Complex(System.Math.Sin(half), 0);
        }

        /// <inheritdoc/>
        public string Name => $"RY({_theta:F4})";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { _cosHalf, -_sinHalf },
            { _sinHalf, _cosHalf }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("RY gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = _cosHalf * a - _sinHalf * b;
                    stateVector[j] = _sinHalf * a + _cosHalf * b;
                }
            }
        }
    }

    private sealed class RZGate : IQuantumGate
    {
        private readonly double _theta;
        private readonly Complex _expNeg;
        private readonly Complex _expPos;

        /// <summary>Initializes a new instance of the <see cref="RZGate"/> class.</summary>
        /// <param name="theta">The rotation angle.</param>
        public RZGate(double theta)
        {
            _theta = theta;
            double half = theta / 2.0;
            _expNeg = Complex.FromPolarCoordinates(1.0, -half);
            _expPos = Complex.FromPolarCoordinates(1.0, half);
        }

        /// <inheritdoc/>
        public string Name => $"RZ({_theta:F4})";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { _expNeg, Complex.Zero },
            { Complex.Zero, _expPos }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("RZ gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) == 0)
                {
                    stateVector[i] = _expNeg * stateVector[i];
                }
                else
                {
                    stateVector[i] = _expPos * stateVector[i];
                }
            }
        }
    }

    private sealed class PhaseShiftGate : IQuantumGate
    {
        private readonly double _phi;
        private readonly Complex _expPhi;

        /// <summary>Initializes a new instance of the <see cref="PhaseShiftGate"/> class.</summary>
        /// <param name="phi">The phase angle.</param>
        public PhaseShiftGate(double phi)
        {
            _phi = phi;
            _expPhi = Complex.FromPolarCoordinates(1.0, phi);
        }

        /// <inheritdoc/>
        public string Name => $"P({_phi:F4})";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.One, Complex.Zero },
            { Complex.Zero, _expPhi }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("Phase shift gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) != 0)
                {
                    stateVector[i] = _expPhi * stateVector[i];
                }
            }
        }
    }

    private sealed class U3Gate : IQuantumGate
    {
        private readonly double _theta;
        private readonly double _phi;
        private readonly double _lambda;
        private readonly Complex _c00;
        private readonly Complex _c01;
        private readonly Complex _c10;
        private readonly Complex _c11;

        /// <summary>Initializes a new instance of the <see cref="U3Gate"/> class.</summary>
        /// <param name="theta">The polar angle θ.</param>
        /// <param name="phi">The azimuthal angle φ.</param>
        /// <param name="lambda">The global phase λ.</param>
        public U3Gate(double theta, double phi, double lambda)
        {
            _theta = theta;
            _phi = phi;
            _lambda = lambda;

            double cosHalf = System.Math.Cos(theta / 2.0);
            double sinHalf = System.Math.Sin(theta / 2.0);
            _c00 = new Complex(cosHalf, 0);
            _c01 = -Complex.FromPolarCoordinates(sinHalf, lambda);
            _c10 = Complex.FromPolarCoordinates(sinHalf, phi);
            _c11 = Complex.FromPolarCoordinates(cosHalf, phi + lambda);
        }

        /// <inheritdoc/>
        public string Name => $"U3({_theta:F4},{_phi:F4},{_lambda:F4})";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { _c00, _c01 },
            { _c10, _c11 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("U3 gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = _c00 * a + _c01 * b;
                    stateVector[j] = _c10 * a + _c11 * b;
                }
            }
        }
    }
}
