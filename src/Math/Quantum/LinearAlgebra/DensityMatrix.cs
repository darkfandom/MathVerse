namespace MathVerse.Math.Quantum.LinearAlgebra;

using System;
using System.Numerics;

/// <summary>
/// Represents a density matrix ρ for quantum states, supporting both pure and mixed states.
/// A valid density matrix satisfies: ρ† = ρ, Tr(ρ) = 1, ρ ≥ 0.
/// </summary>
public sealed class DensityMatrix
{
    /// <summary>Gets the underlying matrix.</summary>
    public ComplexMatrix Matrix { get; }

    /// <summary>Gets the dimension of the Hilbert space.</summary>
    public int Dimension => Matrix.Rows;

    /// <summary>Creates a density matrix from a complex matrix.</summary>
    public DensityMatrix(ComplexMatrix matrix)
    {
        Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        if (matrix.Rows != matrix.Cols)
            throw new ArgumentException("Density matrix must be square.", nameof(matrix));
    }

    /// <summary>Creates a density matrix from a pure state vector.</summary>
    public DensityMatrix(ComplexVector stateVector)
    {
        if (stateVector == null) throw new ArgumentNullException(nameof(stateVector));
        var outer = stateVector.OuterProduct(stateVector);
        Matrix = outer;
    }

    /// <summary>Computes the partial trace over the specified qubit subsystem.</summary>
    public ComplexMatrix PartialTrace(int systemQubit, int totalQubits)
    {
        if (systemQubit < 0 || systemQubit >= totalQubits)
            throw new ArgumentOutOfRangeException(nameof(systemQubit));
        if (totalQubits < 1) throw new ArgumentOutOfRangeException(nameof(totalQubits));

        int dim = Matrix.Rows;
        int expectedDim = 1 << totalQubits;
        if (dim != expectedDim)
            throw new ArgumentException($"Matrix dimension {dim} does not match {totalQubits} qubits (expected {expectedDim}).");

        int targetDim = dim >> 1;
        var result = new Complex[targetDim, targetDim];
        int blockSize = 1 << systemQubit;

        for (int r0 = 0; r0 < targetDim; r0++)
        {
            for (int c0 = 0; c0 < targetDim; c0++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < 2; k++)
                {
                    int fullRow = InsertBit(r0, systemQubit, k);
                    int fullCol = InsertBit(c0, systemQubit, k);
                    sum += Matrix[fullRow, fullCol];
                }
                result[r0, c0] = sum;
            }
        }
        return new ComplexMatrix(result);
    }

    /// <summary>Computes the fidelity F(ρ, σ) = (Tr√(√ρ σ √ρ))².</summary>
    public double Fidelity(DensityMatrix other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        if (Dimension != other.Dimension)
            throw new ArgumentException("Density matrices must have the same dimension.");

        if (Dimension == 2)
            return FidelityQubit(other);

        ComplexMatrix sqrtRho = MatrixSquareRoot(Matrix);
        ComplexMatrix product = sqrtRho.Multiply(other.Matrix).Multiply(sqrtRho);
        ComplexMatrix sqrtProduct = MatrixSquareRoot(product);
        Complex trace = sqrtProduct.Trace();
        return trace.Magnitude * trace.Magnitude;
    }

    /// <summary>Computes the purity Tr(ρ²) of the density matrix.</summary>
    public double Purity()
    {
        ComplexMatrix squared = Matrix.Multiply(Matrix);
        Complex trace = squared.Trace();
        return trace.Real;
    }

    /// <summary>Computes the tensor product with another density matrix.</summary>
    public DensityMatrix TensorProduct(DensityMatrix other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return new DensityMatrix(Matrix.TensorProduct(other.Matrix));
    }

    private static int InsertBit(int value, int position, int bit)
    {
        int mask = (1 << position) - 1;
        int low = value & mask;
        int high = value & ~mask;
        return (high << 1) | (bit << position) | low;
    }

    private double FidelityQubit(DensityMatrix other)
    {
        Complex a = Matrix[0, 0];
        Complex b = Matrix[0, 1];
        Complex c = Matrix[1, 0];
        Complex d = Matrix[1, 1];
        Complex e = other.Matrix[0, 0];
        Complex f = other.Matrix[0, 1];
        Complex g = other.Matrix[1, 0];
        Complex h = other.Matrix[1, 1];

        Complex trRho = a + d;
        Complex trSigma = e + h;

        double traceProd = (a * e + b * g + c * f + d * h).Real;

        if (trRho.Magnitude < 1e-15 || trSigma.Magnitude < 1e-15)
            return 0.0;

        double denom = trRho.Real * trSigma.Real;
        if (denom < 1e-30) return 0.0;

        double fidelity = traceProd / denom;
        return System.Math.Max(0.0, System.Math.Min(1.0, fidelity));
    }

    private static ComplexMatrix MatrixSquareRoot(ComplexMatrix m)
    {
        if (m.Rows != m.Cols) throw new InvalidOperationException("Cannot compute square root of non-square matrix.");

        int n = m.Rows;
        if (n == 1)
        {
            var result = new Complex[1, 1];
            result[0, 0] = Complex.Sqrt(m[0, 0]);
            return new ComplexMatrix(result);
        }

        var (eigenvalues, eigenvectors) = EigenSolver.Decompose(m);
        var diagValues = new Complex[n, n];
        for (int i = 0; i < n; i++)
            diagValues[i, i] = Complex.Sqrt(new Complex(System.Math.Max(0.0, eigenvalues[i]), 0.0));

        var diag = new ComplexMatrix(diagValues);
        var inverseEig = InvertMatrix(eigenvectors);
        return eigenvectors.Multiply(diag).Multiply(inverseEig);
    }

    private static ComplexMatrix InvertMatrix(ComplexMatrix m)
    {
        int n = m.Rows;
        var augmented = new Complex[n, 2 * n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                augmented[i, j] = m[i, j];
            augmented[i, n + i] = Complex.One;
        }

        for (int i = 0; i < n; i++)
        {
            int maxRow = i;
            double maxMag = augmented[i, i].Magnitude;
            for (int k = i + 1; k < n; k++)
            {
                double mag = augmented[k, i].Magnitude;
                if (mag > maxMag) { maxMag = mag; maxRow = k; }
            }
            if (maxMag < 1e-15) throw new InvalidOperationException("Matrix is singular.");

            if (maxRow != i)
                for (int j = 0; j < 2 * n; j++)
                    (augmented[i, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[i, j]);

            Complex pivot = augmented[i, i];
            for (int j = 0; j < 2 * n; j++)
                augmented[i, j] /= pivot;

            for (int k = 0; k < n; k++)
            {
                if (k == i) continue;
                Complex factor = augmented[k, i];
                for (int j = 0; j < 2 * n; j++)
                    augmented[k, j] -= factor * augmented[i, j];
            }
        }

        var result = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = augmented[i, n + j];
        return new ComplexMatrix(result);
    }
}
