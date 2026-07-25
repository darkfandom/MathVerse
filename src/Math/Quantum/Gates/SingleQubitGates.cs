namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Provides static factory properties for all standard single-qubit quantum gates.
/// </summary>
public static class SingleQubitGates
{
    /// <summary>Gets the Identity (I) gate: [[1,0],[0,1]].</summary>
    public static IQuantumGate Identity { get; } = new IdentityGate();

    /// <summary>Gets the Pauli-X gate: [[0,1],[1,0]].</summary>
    public static IQuantumGate PauliX { get; } = new PauliXGate();

    /// <summary>Gets the Pauli-Y gate: [[0,-i],[i,0]].</summary>
    public static IQuantumGate PauliY { get; } = new PauliYGate();

    /// <summary>Gets the Pauli-Z gate: [[1,0],[0,-1]].</summary>
    public static IQuantumGate PauliZ { get; } = new PauliZGate();

    /// <summary>Gets the Hadamard (H) gate: 1/√2 [[1,1],[1,-1]].</summary>
    public static IQuantumGate Hadamard { get; } = new HadamardGate();

    /// <summary>Gets the Phase (S) gate: [[1,0],[0,i]].</summary>
    public static IQuantumGate Phase { get; } = new PhaseGate();

    /// <summary>Gets the S gate (alias for Phase).</summary>
    public static IQuantumGate SGate => Phase;

    /// <summary>Gets the T gate: [[1,0],[0,e^(iπ/4)]].</summary>
    public static IQuantumGate TGate { get; } = new TGateImpl();

    /// <summary>Gets the √X (SX) gate: 1/2 [[1+i, 1-i],[1-i, 1+i]].</summary>
    public static IQuantumGate SX { get; } = new SXGate();

    private sealed class IdentityGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "I";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.One, Complex.Zero },
            { Complex.Zero, Complex.One }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("Identity gate acts on exactly one qubit.", nameof(qubitIndices));
            // Identity gate does nothing
        }
    }

    private sealed class PauliXGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "X";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.Zero, Complex.One },
            { Complex.One, Complex.Zero }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("PauliX gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) == 0)
                {
                    int j = i | mask;
                    (stateVector[i], stateVector[j]) = (stateVector[j], stateVector[i]);
                }
            }
        }
    }

    private sealed class PauliYGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "Y";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.Zero, new Complex(0, -1) },
            { new Complex(0, 1), Complex.Zero }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("PauliY gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = new Complex(0, -1) * b;
                    stateVector[j] = new Complex(0, 1) * a;
                }
            }
        }
    }

    private sealed class PauliZGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "Z";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.One, Complex.Zero },
            { Complex.Zero, -Complex.One }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("PauliZ gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) != 0)
                {
                    stateVector[i] = -stateVector[i];
                }
            }
        }
    }

    private sealed class HadamardGate : IQuantumGate
    {
        private static readonly Complex InvSqrt2 = new Complex(1.0 / System.Math.Sqrt(2.0), 0);

        /// <inheritdoc/>
        public string Name => "H";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { InvSqrt2, InvSqrt2 },
            { InvSqrt2, -InvSqrt2 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("Hadamard gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = InvSqrt2 * (a + b);
                    stateVector[j] = InvSqrt2 * (a - b);
                }
            }
        }
    }

    private sealed class PhaseGate : IQuantumGate
    {
        private static readonly Complex IValue = new Complex(0, 1);

        /// <inheritdoc/>
        public string Name => "S";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.One, Complex.Zero },
            { Complex.Zero, IValue }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("Phase gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) != 0)
                {
                    stateVector[i] = IValue * stateVector[i];
                }
            }
        }
    }

    private sealed class TGateImpl : IQuantumGate
    {
        private static readonly Complex ExpIPiOver4 = Complex.FromPolarCoordinates(1.0, System.Math.PI / 4.0);

        /// <inheritdoc/>
        public string Name => "T";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Complex.One, Complex.Zero },
            { Complex.Zero, ExpIPiOver4 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("T gate acts on exactly one qubit.", nameof(qubitIndices));

            int qubit = qubitIndices[0];
            int n = 1 << totalQubits;
            int mask = 1 << qubit;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask) != 0)
                {
                    stateVector[i] = ExpIPiOver4 * stateVector[i];
                }
            }
        }
    }

    private sealed class SXGate : IQuantumGate
    {
        private static readonly Complex IVal = new Complex(0, 1);
        private static readonly Complex Half = new Complex(0.5, 0);

        /// <inheritdoc/>
        public string Name => "SX";

        /// <inheritdoc/>
        public int NumQubits => 1;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { Half * (Complex.One + IVal), Half * (Complex.One - IVal) },
            { Half * (Complex.One - IVal), Half * (Complex.One + IVal) }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 1) throw new ArgumentException("SX gate acts on exactly one qubit.", nameof(qubitIndices));

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
                    stateVector[i] = Half * ((Complex.One + IVal) * a + (Complex.One - IVal) * b);
                    stateVector[j] = Half * ((Complex.One - IVal) * a + (Complex.One + IVal) * b);
                }
            }
        }
    }
}
