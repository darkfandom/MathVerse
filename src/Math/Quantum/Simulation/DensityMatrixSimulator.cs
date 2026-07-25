namespace MathVerse.Math.Quantum.Simulation;

using System;
using System.Numerics;
using Gates;
using LinearAlgebra;
using Noise;

/// <summary>
/// Density matrix quantum simulator for mixed states. Maintains the full density matrix ρ
/// and applies gates via ρ → UρU†, supporting depolarizing noise injection.
/// </summary>
public sealed class DensityMatrixSimulator
{
    private readonly int _numQubits;
    private readonly int _dimension;
    private ComplexMatrix _densityMatrix;

    /// <summary>Gets the number of qubits.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a density matrix simulator initialized to |0...0⟩⟨0...0|.</summary>
    public DensityMatrixSimulator(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _dimension = 1 << numQubits;
        _densityMatrix = PureStateDensity(0);
    }

    /// <summary>
    /// Initializes the simulator to the specified density matrix.
    /// </summary>
    /// <param name="densityMatrix">The initial density matrix.</param>
    public void Initialize(ComplexMatrix densityMatrix)
    {
        if (densityMatrix == null) throw new ArgumentNullException(nameof(densityMatrix));
        if (densityMatrix.Rows != _dimension || densityMatrix.Cols != _dimension)
            throw new ArgumentException($"Density matrix must be {_dimension}×{_dimension}.");
        _densityMatrix = densityMatrix;
    }

    /// <summary>
    /// Applies a quantum gate to the density matrix: ρ → UρU†.
    /// </summary>
    /// <param name="gate">The gate to apply.</param>
    /// <param name="qubitIndices">The qubit indices the gate acts on.</param>
    public void ApplyGate(IQuantumGate gate, int[] qubitIndices)
    {
        if (gate == null) throw new ArgumentNullException(nameof(gate));
        if (qubitIndices == null) throw new ArgumentNullException(nameof(qubitIndices));

        var U = ReconstructUnitary(gate, qubitIndices);
        var Udagger = U.ConjugateTranspose();
        _densityMatrix = U.Multiply(_densityMatrix).Multiply(Udagger);
    }

    /// <summary>
    /// Gets the current density matrix.
    /// </summary>
    public DensityMatrix GetDensityMatrix()
    {
        return new DensityMatrix(_densityMatrix);
    }

    /// <summary>
    /// Applies a depolarizing noise channel to all qubits.
    /// </summary>
    /// <param name="depolarizingRate">The depolarizing probability in [0,1].</param>
    public void ApplyNoise(double depolarizingRate)
    {
        if (depolarizingRate < 0.0 || depolarizingRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(depolarizingRate));

        double p = depolarizingRate;
        var identity = ComplexMatrix.Identity(_dimension);
        _densityMatrix = _densityMatrix.Scale(new Complex(1.0 - p, 0))
                         .Add(identity.Scale(new Complex(p / _dimension, 0)));
    }

    /// <summary>
    /// Resets the simulator to the |0...0⟩ state.
    /// </summary>
    public void Reset()
    {
        _densityMatrix = PureStateDensity(0);
    }

    private ComplexMatrix PureStateDensity(int basisIndex)
    {
        var data = new Complex[_dimension, _dimension];
        data[basisIndex, basisIndex] = Complex.One;
        return new ComplexMatrix(data);
    }

    private ComplexMatrix ReconstructUnitary(IQuantumGate gate, int[] qubitIndices)
    {
        int n = 1 << _numQubits;
        var matrix = new Complex[n, n];
        for (int col = 0; col < n; col++)
        {
            var basis = new Complex[n];
            basis[col] = Complex.One;
            gate.Apply(basis, qubitIndices, _numQubits);
            for (int row = 0; row < n; row++)
                matrix[row, col] = basis[row];
        }
        return new ComplexMatrix(matrix);
    }
}
