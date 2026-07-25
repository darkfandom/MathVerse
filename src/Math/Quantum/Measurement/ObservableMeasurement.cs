namespace MathVerse.Math.Quantum.Measurement;

using System;
using System.Numerics;
using MathVerse.Math.Quantum.LinearAlgebra;

/// <summary>
/// Provides measurement of quantum observables.
/// </summary>
public sealed class ObservableMeasurement
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableMeasurement"/> class.
    /// </summary>
    public ObservableMeasurement()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableMeasurement"/> class with a specified random seed.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public ObservableMeasurement(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Computes the expectation value of an observable.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="observable">The observable matrix.</param>
    /// <returns>The expectation value.</returns>
    public double ExpectationValue(ComplexVector state, ComplexMatrix observable)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (observable == null) throw new ArgumentNullException(nameof(observable));

        ComplexVector result = observable.Multiply(state);
        Complex innerProduct = Complex.Conjugate(state.InnerProduct(result));
        return innerProduct.Real;
    }

    /// <summary>
    /// Computes the variance of an observable: Var(A) = ⟨A²⟩ − ⟨A⟩².
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="observable">The observable matrix.</param>
    /// <returns>The variance.</returns>
    public double Variance(ComplexVector state, ComplexMatrix observable)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (observable == null) throw new ArgumentNullException(nameof(observable));

        double mean = ExpectationValue(state, observable);
        ComplexMatrix squared = observable.Multiply(observable);
        double meanSquared = ExpectationValue(state, squared);
        return meanSquared - mean * mean;
    }

    /// <summary>
    /// Measures an observable via eigenvalue decomposition and sampling.
    /// </summary>
    /// <param name="state">The state vector.</param>
    /// <param name="observable">The observable matrix.</param>
    /// <param name="shots">The number of measurements to perform.</param>
    /// <returns>The mean and variance of the measurements.</returns>
    public (double mean, double variance) Measure(ComplexVector state, ComplexMatrix observable, int shots)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (observable == null) throw new ArgumentNullException(nameof(observable));
        if (shots < 1) throw new ArgumentException("Number of shots must be at least 1.", nameof(shots));

        int dim = observable.Rows;
        double[] eigenvalues = new double[dim];
        ComplexVector[] eigenvectors = new ComplexVector[dim];

        DiagonalizeHermitian(observable, eigenvalues, eigenvectors);

        double[] probabilities = new double[dim];
        for (int i = 0; i < dim; i++)
        {
            probabilities[i] = Complex.Abs(Complex.Conjugate(eigenvectors[i].InnerProduct(state)));
            probabilities[i] *= probabilities[i];
        }

        double sum = 0.0;
        double sumSquared = 0.0;

        for (int s = 0; s < shots; s++)
        {
            int outcome = SampleFromDistribution(probabilities);
            sum += eigenvalues[outcome];
            sumSquared += eigenvalues[outcome] * eigenvalues[outcome];
        }

        double mean = sum / shots;
        double variance = sumSquared / shots - mean * mean;
        return (mean, variance);
    }

    /// <summary>
    /// Gets the Pauli-Z observable matrix.
    /// </summary>
    /// <returns>The Pauli-Z matrix.</returns>
    public static ComplexMatrix PauliZ()
    {
        return new ComplexMatrix(new Complex[,]
        {
            { 1, 0 },
            { 0, -1 }
        });
    }

    /// <summary>
    /// Gets the Pauli-X observable matrix.
    /// </summary>
    /// <returns>The Pauli-X matrix.</returns>
    public static ComplexMatrix PauliX()
    {
        return new ComplexMatrix(new Complex[,]
        {
            { 0, 1 },
            { 1, 0 }
        });
    }

    /// <summary>
    /// Gets the Pauli-Y observable matrix.
    /// </summary>
    /// <returns>The Pauli-Y matrix.</returns>
    public static ComplexMatrix PauliY()
    {
        return new ComplexMatrix(new Complex[,]
        {
            { 0, new Complex(0, -1) },
            { new Complex(0, 1), 0 }
        });
    }

    /// <summary>
    /// Gets the number operator N = Σ|1⟩⟨1| for the specified number of qubits.
    /// </summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <returns>The number operator matrix.</returns>
    public static ComplexMatrix NumberOperator(int numQubits)
    {
        if (numQubits < 1) throw new ArgumentException("Number of qubits must be at least 1.", nameof(numQubits));

        int dim = 1 << numQubits;
        var matrix = new Complex[dim, dim];

        for (int i = 0; i < dim; i++)
        {
            int count = 0;
            int temp = i;
            while (temp > 0)
            {
                count += temp & 1;
                temp >>= 1;
            }
            matrix[i, i] = new Complex(count, 0);
        }

        return new ComplexMatrix(matrix);
    }

    private void DiagonalizeHermitian(ComplexMatrix matrix, double[] eigenvalues, ComplexVector[] eigenvectors)
    {
        int dim = matrix.Rows;
        var a = new Complex[dim, dim];
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
                a[i, j] = matrix[i, j];

        var v = new Complex[dim, dim];
        for (int i = 0; i < dim; i++)
            v[i, i] = 1;

        for (int iter = 0; iter < 100; iter++)
        {
            int p = 0, q = 1;
            double maxOff = 0;
            for (int i = 0; i < dim; i++)
                for (int j = i + 1; j < dim; j++)
                {
                    double absVal = Complex.Abs(a[i, j]);
                    if (absVal > maxOff)
                    {
                        maxOff = absVal;
                        p = i;
                        q = j;
                    }
                }

            if (maxOff < 1e-12) break;

            double theta = 0.5 * System.Math.Atan2(2.0 * a[p, q].Real, a[p, p].Real - a[q, q].Real);
            double c = System.Math.Cos(theta);
            double s = System.Math.Sin(theta);

            var g = new Complex[dim, dim];
            for (int i = 0; i < dim; i++) g[i, i] = 1;
            g[p, p] = new Complex(c, 0);
            g[q, q] = new Complex(c, 0);
            g[p, q] = new Complex(-s, 0);
            g[q, p] = new Complex(s, 0);

            var gt = new Complex[dim, dim];
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    gt[i, j] = g[j, i];

            var newA = new Complex[dim, dim];
            var temp = new Complex[dim, dim];
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    for (int k = 0; k < dim; k++)
                        temp[i, j] += a[i, k] * g[k, j];
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    for (int k = 0; k < dim; k++)
                        newA[i, j] += gt[i, k] * temp[k, j];

            Array.Copy(newA, a, newA.Length);

            var newV = new Complex[dim, dim];
            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    for (int k = 0; k < dim; k++)
                        newV[i, j] += v[i, k] * g[k, j];
            Array.Copy(newV, v, newV.Length);
        }

        for (int i = 0; i < dim; i++)
        {
            eigenvalues[i] = a[i, i].Real;
            var col = new Complex[dim];
            for (int j = 0; j < dim; j++)
                col[j] = v[j, i];
            eigenvectors[i] = new ComplexVector(col);
        }
    }

    private int SampleFromDistribution(double[] probabilities)
    {
        double randomValue = _random.NextDouble();
        double cumulative = 0.0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue < cumulative)
            {
                return i;
            }
        }

        return probabilities.Length - 1;
    }
}
