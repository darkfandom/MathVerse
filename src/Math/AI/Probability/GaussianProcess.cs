namespace MathVerse.Math.AI.Probability;

using System;

/// <summary>Gaussian Process regression with RBF kernel and Cholesky decomposition.</summary>
public sealed class GaussianProcess
{
    private double[][] _Xtrain = [];
    private double[] _ytrain = [];
    private double _sigma;
    private double _lengthScale;
    private double[][] _L = [];
    private double[] _alpha = [];

    /// <summary>Initializes a new Gaussian Process with default hyperparameters.</summary>
    public GaussianProcess()
    {
        _sigma = 1.0;
        _lengthScale = 1.0;
    }

    /// <summary>Initializes a new Gaussian Process with specified hyperparameters.</summary>
    /// <param name="sigma">Signal variance (kernel amplitude).</param>
    /// <param name="lengthScale">Length scale for the RBF kernel.</param>
    public GaussianProcess(double sigma, double lengthScale)
    {
        _sigma = sigma;
        _lengthScale = lengthScale;
    }

    /// <summary>Fits the GP to training data by computing the kernel matrix and its Cholesky decomposition.</summary>
    /// <param name="X">Training input points.</param>
    /// <param name="y">Training target values.</param>
    /// <param name="sigma">Signal variance.</param>
    /// <param name="lengthScale">Length scale.</param>
    public void Fit(double[][] X, double[] y, double sigma = 1.0, double lengthScale = 1.0)
    {
        if (X == null || X.Length == 0)
            throw new ArgumentException("Training data cannot be null or empty.", nameof(X));
        if (y == null || y.Length != X.Length)
            throw new ArgumentException("Target array length must match number of training points.", nameof(y));

        _sigma = sigma;
        _lengthScale = lengthScale;
        _Xtrain = X;
        _ytrain = (double[])y.Clone();

        int n = X.Length;
        double[][] K = ComputeKernelMatrix(X, X);

        for (int i = 0; i < n; i++)
            K[i][i] += 1e-6;

        _L = CholeskyDecomposition(K);
        _alpha = SolveCholesky(_L, y);
    }

    /// <summary>Predicts means and variances at test points.</summary>
    /// <param name="Xtest">Test input points.</param>
    /// <returns>Tuple of (means, variances) arrays.</returns>
    public (double[] Means, double[] Variances) Predict(double[][] Xtest)
    {
        if (Xtest == null || Xtest.Length == 0)
            throw new ArgumentException("Test data cannot be null or empty.", nameof(Xtest));
        if (_Xtrain.Length == 0)
            throw new InvalidOperationException("Model has not been trained. Call Fit() first.");

        int nTest = Xtest.Length;
        int nTrain = _Xtrain.Length;

        double[][] Ks = ComputeKernelMatrix(Xtest, _Xtrain);
        double[][] Kss = ComputeKernelMatrix(Xtest, Xtest);

        double[] means = new double[nTest];
        double[] variances = new double[nTest];

        for (int i = 0; i < nTest; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < nTrain; j++)
                sum += Ks[i][j] * _alpha[j];
            means[i] = sum;
        }

        double[][] v = new double[nTest][];
        for (int i = 0; i < nTest; i++)
            v[i] = ForwardSubstitution(_L, Ks[i]);

        for (int i = 0; i < nTest; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < nTest; j++)
                sum += v[i][j] * v[i][j];
            variances[i] = Kss[i][i] - sum;
            if (variances[i] < 0.0)
                variances[i] = 0.0;
        }

        return (means, variances);
    }

    /// <summary>Computes the RBF kernel between two sets of points.</summary>
    /// <param name="X1">First set of points.</param>
    /// <param name="X2">Second set of points.</param>
    /// <returns>Kernel matrix K(X1, X2).</returns>
    public double[][] ComputeKernelMatrix(double[][] X1, double[][] X2)
    {
        int n1 = X1.Length;
        int n2 = X2.Length;
        double[][] K = new double[n1][];

        for (int i = 0; i < n1; i++)
        {
            K[i] = new double[n2];
            for (int j = 0; j < n2; j++)
                K[i][j] = RBFKernel(X1[i], X2[j]);
        }

        return K;
    }

    /// <summary>Evaluates the RBF (squared exponential) kernel between two points.</summary>
    /// <param name="x1">First point.</param>
    /// <param name="x2">Second point.</param>
    /// <returns>Kernel value k(x1, x2).</returns>
    public double RBFKernel(double[] x1, double[] x2)
    {
        double sqDist = 0.0;
        for (int d = 0; d < x1.Length; d++)
        {
            double diff = x1[d] - x2[d];
            sqDist += diff * diff;
        }
        return _sigma * _sigma * System.Math.Exp(-sqDist / (2.0 * _lengthScale * _lengthScale));
    }

    /// <summary>Performs Cholesky decomposition L * L^T = A for a symmetric positive-definite matrix.</summary>
    /// <param name="A">Input symmetric positive-definite matrix.</param>
    /// <returns>Lower triangular matrix L.</returns>
    public double[][] CholeskyDecomposition(double[][] A)
    {
        int n = A.Length;
        double[][] L = new double[n][];

        for (int i = 0; i < n; i++)
            L[i] = new double[n];

        for (int j = 0; j < n; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < j; k++)
                sum += L[j][k] * L[j][k];

            double diag = A[j][j] - sum;
            if (diag <= 0.0)
                throw new InvalidOperationException("Matrix is not positive definite.");

            L[j][j] = System.Math.Sqrt(diag);

            for (int i = j + 1; i < n; i++)
            {
                double innerSum = 0.0;
                for (int k = 0; k < j; k++)
                    innerSum += L[i][k] * L[j][k];
                L[i][j] = (A[i][j] - innerSum) / L[j][j];
            }
        }

        return L;
    }

    /// <summary>Solves L * x = b using forward substitution.</summary>
    /// <param name="L">Lower triangular matrix.</param>
    /// <param name="b">Right-hand side vector.</param>
    /// <returns>Solution vector x.</returns>
    public double[] SolveCholesky(double[][] L, double[] b)
    {
        int n = L.Length;
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < i; j++)
                sum += L[i][j] * y[j];
            y[i] = (b[i] - sum) / L[i][i];
        }

        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0.0;
            for (int j = i + 1; j < n; j++)
                sum += L[j][i] * x[j];
            x[i] = (y[i] - sum) / L[i][i];
        }

        return x;
    }

    private static double[] ForwardSubstitution(double[][] L, double[] b)
    {
        int n = L.Length;
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < i; j++)
                sum += L[i][j] * y[j];
            y[i] = (b[i] - sum) / L[i][i];
        }

        return y;
    }
}
