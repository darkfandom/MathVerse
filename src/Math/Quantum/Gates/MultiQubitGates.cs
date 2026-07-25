namespace MathVerse.Math.Quantum.Gates;

using System;
using System.Numerics;

/// <summary>
/// Provides standard multi-qubit quantum gates.
/// </summary>
public static class MultiQubitGates
{
    /// <summary>Gets the CNOT (CX) gate: 4×4 controlled-NOT.</summary>
    public static IQuantumGate CX { get; } = new CXGate();

    /// <summary>Gets the controlled-Y (CY) gate: 4×4.</summary>
    public static IQuantumGate CY { get; } = new CYGate();

    /// <summary>Gets the controlled-Z (CZ) gate: 4×4.</summary>
    public static IQuantumGate CZ { get; } = new CZGate();

    /// <summary>Gets the SWAP gate: 4×4.</summary>
    public static IQuantumGate Swap { get; } = new SwapGate();

    /// <summary>Gets the Toffoli (CCX) gate: 8×8.</summary>
    public static IQuantumGate CCX { get; } = new CCXGate();

    /// <summary>Gets the Fredkin (CSwap) gate: controlled swap.</summary>
    public static IQuantumGate CSwap { get; } = new CSwapGate();

    /// <summary>Gets the iSWAP gate: 4×4.</summary>
    public static IQuantumGate iSWAP { get; } = new ISwapGate();

    private sealed class CXGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "CX";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, 1 },
            { 0, 0, 1, 0 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("CX gate acts on exactly two qubits.", nameof(qubitIndices));

            int control = qubitIndices[0];
            int target = qubitIndices[1];
            int n = 1 << totalQubits;
            int controlMask = 1 << control;
            int targetMask = 1 << target;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0 && (i & targetMask) == 0)
                {
                    int j = i | targetMask;
                    (stateVector[i], stateVector[j]) = (stateVector[j], stateVector[i]);
                }
            }
        }
    }

    private sealed class CYGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "CY";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, new Complex(0, -1) },
            { 0, 0, new Complex(0, 1), 0 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("CY gate acts on exactly two qubits.", nameof(qubitIndices));

            int control = qubitIndices[0];
            int target = qubitIndices[1];
            int n = 1 << totalQubits;
            int controlMask = 1 << control;
            int targetMask = 1 << target;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0 && (i & targetMask) == 0)
                {
                    int j = i | targetMask;
                    Complex a = stateVector[i];
                    Complex b = stateVector[j];
                    stateVector[i] = new Complex(0, -1) * b;
                    stateVector[j] = new Complex(0, 1) * a;
                }
            }
        }
    }

    private sealed class CZGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "CZ";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, -1 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("CZ gate acts on exactly two qubits.", nameof(qubitIndices));

            int control = qubitIndices[0];
            int target = qubitIndices[1];
            int n = 1 << totalQubits;
            int controlMask = 1 << control;
            int targetMask = 1 << target;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0 && (i & targetMask) != 0)
                {
                    stateVector[i] = -stateVector[i];
                }
            }
        }
    }

    private sealed class SwapGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "SWAP";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, 1 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("SWAP gate acts on exactly two qubits.", nameof(qubitIndices));

            int qubit0 = qubitIndices[0];
            int qubit1 = qubitIndices[1];
            int n = 1 << totalQubits;
            int mask0 = 1 << qubit0;
            int mask1 = 1 << qubit1;

            for (int i = 0; i < n; i++)
            {
                bool bit0 = (i & mask0) != 0;
                bool bit1 = (i & mask1) != 0;

                if (bit0 != bit1)
                {
                    int j = i ^ mask0 ^ mask1;
                    if (i < j)
                    {
                        (stateVector[i], stateVector[j]) = (stateVector[j], stateVector[i]);
                    }
                }
            }
        }
    }

    private sealed class CCXGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "CCX";

        /// <inheritdoc/>
        public int NumQubits => 3;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 1 },
            { 0, 0, 0, 0, 0, 0, 1, 0 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 3) throw new ArgumentException("CCX gate acts on exactly three qubits.", nameof(qubitIndices));

            int control0 = qubitIndices[0];
            int control1 = qubitIndices[1];
            int target = qubitIndices[2];
            int n = 1 << totalQubits;
            int mask0 = 1 << control0;
            int mask1 = 1 << control1;
            int targetMask = 1 << target;

            for (int i = 0; i < n; i++)
            {
                if ((i & mask0) != 0 && (i & mask1) != 0 && (i & targetMask) == 0)
                {
                    int j = i | targetMask;
                    (stateVector[i], stateVector[j]) = (stateVector[j], stateVector[i]);
                }
            }
        }
    }

    private sealed class CSwapGate : IQuantumGate
    {
        /// <inheritdoc/>
        public string Name => "CSWAP";

        /// <inheritdoc/>
        public int NumQubits => 3;

        /// <inheritdoc/>
        public Complex[,] Matrix
        {
            get
            {
                var m = new Complex[8, 8];
                for (int i = 0; i < 8; i++) m[i, i] = 1;
                // |110⟩ ↔ |101⟩
                m[6, 5] = 1; m[6, 6] = 0;
                m[5, 6] = 1; m[5, 5] = 0;
                return m;
            }
        }

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 3) throw new ArgumentException("CSWAP gate acts on exactly three qubits.", nameof(qubitIndices));

            int control = qubitIndices[0];
            int swap0 = qubitIndices[1];
            int swap1 = qubitIndices[2];
            int n = 1 << totalQubits;
            int controlMask = 1 << control;
            int mask0 = 1 << swap0;
            int mask1 = 1 << swap1;

            for (int i = 0; i < n; i++)
            {
                if ((i & controlMask) != 0)
                {
                    bool bit0 = (i & mask0) != 0;
                    bool bit1 = (i & mask1) != 0;
                    if (bit0 != bit1)
                    {
                        int j = i ^ mask0 ^ mask1;
                        if (i < j)
                        {
                            (stateVector[i], stateVector[j]) = (stateVector[j], stateVector[i]);
                        }
                    }
                }
            }
        }
    }

    private sealed class ISwapGate : IQuantumGate
    {
        private static readonly Complex IVal = new Complex(0, 1);

        /// <inheritdoc/>
        public string Name => "iSWAP";

        /// <inheritdoc/>
        public int NumQubits => 2;

        /// <inheritdoc/>
        public Complex[,] Matrix => new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 0, IVal, 0 },
            { 0, IVal, 0, 0 },
            { 0, 0, 0, 1 }
        };

        /// <inheritdoc/>
        public void Apply(Complex[] stateVector, int[] qubitIndices, int totalQubits)
        {
            if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
            if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));
            if (qubitIndices.Length != 2) throw new ArgumentException("iSWAP gate acts on exactly two qubits.", nameof(qubitIndices));

            int qubit0 = qubitIndices[0];
            int qubit1 = qubitIndices[1];
            int n = 1 << totalQubits;
            int mask0 = 1 << qubit0;
            int mask1 = 1 << qubit1;

            for (int i = 0; i < n; i++)
            {
                bool bit0 = (i & mask0) != 0;
                bool bit1 = (i & mask1) != 0;

                if (!bit0 && bit1)
                {
                    int j = (i ^ mask1) | mask0;
                    if (i < j)
                    {
                        Complex a = stateVector[i];
                        Complex b = stateVector[j];
                        stateVector[i] = IVal * b;
                        stateVector[j] = IVal * a;
                    }
                }
            }
        }
    }
}
