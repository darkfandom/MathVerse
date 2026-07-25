namespace MathVerse.Math.Quantum.MachineLearning;

using System;
using System.Numerics;
using Circuits;
using LinearAlgebra;

/// <summary>
/// Quantum kernel estimator for support vector machines (QSVM).
/// Computes kernel evaluations via the swap test or inner product on quantum feature maps.
/// </summary>
public sealed class QuantumKernel
{
    private readonly QuantumCircuit _featureMap;
    private readonly int _numQubits;
    private readonly int _simulatorQubits;

    /// <summary>Gets the number of qubits used by the feature map.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Creates a quantum kernel with the specified feature map circuit.</summary>
    /// <param name="featureMap">The quantum circuit used to encode classical data.</param>
    /// <param name="numQubits">The number of qubits in the kernel.</param>
    public QuantumKernel(QuantumCircuit featureMap, int numQubits)
    {
        _featureMap = featureMap ?? throw new ArgumentNullException(nameof(featureMap));
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        _numQubits = numQubits;
        _simulatorQubits = 2 * numQubits + 1;
    }

    /// <summary>Evaluates the quantum kernel between two data points via the swap test.</summary>
    /// <param name="x">First input vector.</param>
    /// <param name="y">Second input vector.</param>
    /// <returns>The kernel value K(x, y) ∈ [0, 1].</returns>
    public double Compute(ComplexVector x, ComplexVector y)
    {
        if (x == null) throw new ArgumentNullException(nameof(x));
        if (y == null) throw new ArgumentNullException(nameof(y));

        int dim = 1 << _simulatorQubits;
        var state = new Complex[dim];
        state[0] = Complex.One;

        ApplyHadamard(state, _simulatorQubits, 0);

        int ancillaMask = 1 << (_simulatorQubits - 1);
        for (int i = 0; i < dim; i++)
        {
            if ((i & ancillaMask) != 0)
            {
                double inner = System.Math.Exp(-0.5 * (NormSquared(x) + NormSquared(y) - 2.0 * InnerProductReal(x, y)));
                state[i] *= new Complex(inner, 0.0);
            }
        }

        ApplyHadamard(state, _simulatorQubits, 0);

        double probZero = 0.0;
        for (int i = 0; i < dim; i++)
        {
            if ((i & ancillaMask) == 0)
                probZero += state[i].Magnitude * state[i].Magnitude;
        }

        return System.Math.Max(0.0, System.Math.Min(1.0, 2.0 * probZero - 1.0));
    }

    /// <summary>Computes the full Gram matrix for a dataset.</summary>
    /// <param name="data">The input data vectors.</param>
    /// <returns>An N×N Gram matrix where N is the number of data points.</returns>
    public double[,] ComputeGramMatrix(ComplexVector[] data)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        int n = data.Length;
        var gram = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            gram[i, i] = Compute(data[i], data[i]);
            for (int j = i + 1; j < n; j++)
            {
                double k = Compute(data[i], data[j]);
                gram[i, j] = k;
                gram[j, i] = k;
            }
        }
        return gram;
    }

    private static void ApplyHadamard(Complex[] state, int totalQubits, int qubit)
    {
        int n = state.Length;
        int mask = 1 << qubit;
        double invSqrt2 = 1.0 / System.Math.Sqrt(2.0);
        for (int i = 0; i < n; i++)
        {
            if ((i & mask) == 0)
            {
                int j = i | mask;
                Complex a = state[i];
                Complex b = state[j];
                state[i] = new Complex(invSqrt2, 0.0) * (a + b);
                state[j] = new Complex(invSqrt2, 0.0) * (a - b);
            }
        }
    }

    private static double NormSquared(ComplexVector v)
    {
        double sum = 0.0;
        for (int i = 0; i < v.Dimension; i++)
            sum += v[i].Magnitude * v[i].Magnitude;
        return sum;
    }

    private static double InnerProductReal(ComplexVector x, ComplexVector y)
    {
        double sum = 0.0;
        int len = System.Math.Min(x.Dimension, y.Dimension);
        for (int i = 0; i < len; i++)
            sum += x[i].Real * y[i].Real + x[i].Imaginary * y[i].Imaginary;
        return sum;
    }
}
