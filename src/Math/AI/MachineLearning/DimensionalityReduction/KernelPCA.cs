namespace MathVerse.Math.AI.MachineLearning.DimensionalityReduction;

using System;

/// <summary>Kernel Principal Component Analysis using the Radial Basis Function (RBF/Gaussian) kernel.</summary>
public sealed class KernelPCA
{
    private double[][] _projectedData = [];
    private double[] _eigenvalues = [];
    private double[][] _eigenvectors = [];
    private double[][] _trainData = [];
    private double _sigma;

    /// <summary>Gets the eigenvalues of the centered kernel matrix.</summary>
    public double[] Eigenvalues => _eigenvalues;

    /// <summary>Fits Kernel PCA to the data and returns the transformed result.</summary>
    /// <param name="data">Input data matrix (samples x features).</param>
    /// <param name="numComponents">Number of kernel principal components to retain.</param>
    /// <param name="sigma">Bandwidth parameter for the RBF kernel.</param>
    /// <returns>A <see cref="DimensionalityReductionResult"/> with transformed data.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public DimensionalityReductionResult Fit(double[][] data, int numComponents, double sigma = 1.0)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (numComponents <= 0 || numComponents > data.Length)
            throw new ArgumentException($"numComponents must be between 1 and {data.Length}.", nameof(numComponents));
        if (sigma <= 0.0)
            throw new ArgumentException("Sigma must be positive.", nameof(sigma));

        int n = data.Length;
        int d = data[0].Length;
        _sigma = sigma;
        _trainData = CopyData(data);

        // Compute RBF kernel matrix K(n x n)
        double[][] kernelMatrix = ComputeRBFKernelMatrix(data, n, sigma);

        // Center the kernel matrix: K_centered = K - 1_n K - K 1_n + 1_n K 1_n
        double[][] centeredKernel = CenterKernelMatrix(kernelMatrix, n);

        // Eigendecomposition using power iteration with deflation
        _eigenvalues = new double[numComponents];
        _eigenvectors = new double[numComponents][];

        double[][] workMatrix = CopySquareMatrix(centeredKernel, n);

        for (int comp = 0; comp < numComponents; comp++)
        {
            double[] eigenvec = PowerIteration(workMatrix, n, 500);
            double eigenval = RayleighQuotient(workMatrix, eigenvec, n);

            // Normalize eigenvector
            double norm = VectorNorm(eigenvec, n);
            if (norm > 1e-15)
            {
                for (int i = 0; i < n; i++)
                    eigenvec[i] /= norm;
            }

            _eigenvalues[comp] = eigenval;
            _eigenvectors[comp] = eigenvec;

            // Deflate
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    workMatrix[i][j] -= eigenval * eigenvec[i] * eigenvec[j];
        }

        // Project training data
        _projectedData = ProjectData(data, n, numComponents);

        double totalVariance = 0.0;
        for (int i = 0; i < numComponents; i++)
            totalVariance += _eigenvalues[i];

        double[] explainedRatio = new double[numComponents];
        for (int i = 0; i < numComponents; i++)
            explainedRatio[i] = totalVariance > 0.0 ? _eigenvalues[i] / totalVariance : 0.0;

        return new DimensionalityReductionResult
        {
            TransformedData = _projectedData,
            ExplainedVarianceRatio = explainedRatio,
            Components = _eigenvectors,
            OriginalDimensions = d,
            ReducedDimensions = numComponents
        };
    }

    /// <summary>Projects new data onto the kernel principal components learned from training data.</summary>
    /// <param name="newData">New data to project.</param>
    /// <returns>Projected data in reduced-dimensional space.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Fit has not been called.</exception>
    public double[][] Transform(double[][] newData)
    {
        if (_trainData.Length == 0)
            throw new InvalidOperationException("Model has not been fitted. Call Fit() first.");

        int m = newData.Length;
        int nTrain = _trainData.Length;
        int numComp = _eigenvectors.Length;

        // Compute kernel between new data and training data: K_new(m x nTrain)
        double[][] kNew = new double[m][];
        for (int i = 0; i < m; i++)
        {
            kNew[i] = new double[nTrain];
            for (int j = 0; j < nTrain; j++)
            {
                kNew[i][j] = RBFKernel(newData[i], _trainData[j], _sigma);
            }
        }

        // Project using eigenvectors: alpha = (1/lambda) * eigenvector
        double[][] projected = new double[m][];
        for (int i = 0; i < m; i++)
        {
            projected[i] = new double[numComp];
            for (int c = 0; c < numComp; c++)
            {
                double sum = 0.0;
                for (int j = 0; j < nTrain; j++)
                    sum += kNew[i][j] * _eigenvectors[c][j];
                projected[i][c] = _eigenvalues[c] > 1e-12 ? sum / _eigenvalues[c] : 0.0;
            }
        }

        return projected;
    }

    /// <summary>Projects training data onto the learned kernel principal components.</summary>
    /// <param name="data">Training data.</param>
    /// <param name="n">Number of training points.</param>
    /// <param name="numComponents">Number of components.</param>
    /// <returns>Projected training data.</returns>
    private double[][] ProjectData(double[][] data, int n, int numComponents)
    {
        double[][] projected = new double[n][];
        for (int i = 0; i < n; i++)
        {
            projected[i] = new double[numComponents];
            for (int c = 0; c < numComponents; c++)
            {
                double sum = 0.0;
                for (int j = 0; j < n; j++)
                    sum += RBFKernel(data[i], data[j], _sigma) * _eigenvectors[c][j];
                projected[i][c] = _eigenvalues[c] > 1e-12 ? sum / _eigenvalues[c] : 0.0;
            }
        }
        return projected;
    }

    /// <summary>Computes the RBF (Gaussian) kernel matrix for all pairs of data points.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="n">Number of data points.</param>
    /// <param name="sigma">RBF bandwidth.</param>
    /// <returns>Kernel matrix (n x n).</returns>
    private static double[][] ComputeRBFKernelMatrix(double[][] data, int n, double sigma)
    {
        double[][] K = new double[n][];
        double twoSigmaSq = 2.0 * sigma * sigma;

        for (int i = 0; i < n; i++)
        {
            K[i] = new double[n];
            K[i][i] = 1.0; // K(x, x) = 1 for RBF
            for (int j = i + 1; j < n; j++)
            {
                double distSq = SquaredDistance(data[i], data[j]);
                double kVal = System.Math.Exp(-distSq / twoSigmaSq);
                K[i][j] = kVal;
                K[j][i] = kVal;
            }
        }

        return K;
    }

    /// <summary>Centers the kernel matrix using the formula K_c = K - 1_n K - K 1_n + 1_n K 1_n.</summary>
    /// <param name="K">Original kernel matrix.</param>
    /// <param name="n">Matrix dimension.</param>
    /// <returns>Centered kernel matrix.</returns>
    private static double[][] CenterKernelMatrix(double[][] K, int n)
    {
        // Compute row means and column means (symmetric, so they're the same)
        double[] rowMeans = new double[n];
        double grandMean = 0.0;

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
                sum += K[i][j];
            rowMeans[i] = sum / n;
            grandMean += rowMeans[i];
        }
        grandMean /= n;

        // K_centered[i][j] = K[i][j] - rowMeans[i] - rowMeans[j] + grandMean
        double[][] centered = new double[n][];
        for (int i = 0; i < n; i++)
        {
            centered[i] = new double[n];
            for (int j = 0; j < n; j++)
                centered[i][j] = K[i][j] - rowMeans[i] - rowMeans[j] + grandMean;
        }

        return centered;
    }

    /// <summary>Computes the RBF kernel between two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="sigma">RBF bandwidth.</param>
    /// <returns>Kernel value.</returns>
    private static double RBFKernel(double[] a, double[] b, double sigma)
    {
        double distSq = SquaredDistance(a, b);
        return System.Math.Exp(-distSq / (2.0 * sigma * sigma));
    }

    /// <summary>Runs power iteration to find the dominant eigenvector of a matrix.</summary>
    /// <param name="matrix">Symmetric matrix.</param>
    /// <param name="d">Matrix dimension.</param>
    /// <param name="maxIterations">Maximum iterations.</param>
    /// <returns>Dominant eigenvector.</returns>
    private static double[] PowerIteration(double[][] matrix, int d, int maxIterations)
    {
        var rng = new Random(42);
        double[] v = new double[d];
        for (int i = 0; i < d; i++)
            v[i] = rng.NextDouble() - 0.5;

        double norm = VectorNorm(v, d);
        for (int i = 0; i < d; i++)
            v[i] /= norm;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] w = MatVecMul(matrix, v, d);
            double newNorm = VectorNorm(w, d);

            if (newNorm < 1e-15)
                break;

            double[] vNew = new double[d];
            for (int i = 0; i < d; i++)
                vNew[i] = w[i] / newNorm;

            double diff = 0.0;
            for (int i = 0; i < d; i++)
            {
                double dd = vNew[i] - v[i];
                diff += dd * dd;
            }

            v = vNew;

            if (System.Math.Sqrt(diff) < 1e-10)
                break;
        }

        return v;
    }

    /// <summary>Computes the Rayleigh quotient for eigenvalue estimation.</summary>
    /// <param name="matrix">Symmetric matrix.</param>
    /// <param name="v">Eigenvector approximation.</param>
    /// <param name="d">Matrix dimension.</param>
    /// <returns>Estimated eigenvalue.</returns>
    private static double RayleighQuotient(double[][] matrix, double[] v, int d)
    {
        double[] Av = MatVecMul(matrix, v, d);
        double num = 0.0;
        for (int i = 0; i < d; i++)
            num += v[i] * Av[i];
        return num;
    }

    /// <summary>Multiplies a matrix by a vector.</summary>
    private static double[] MatVecMul(double[][] A, double[] v, int d)
    {
        double[] result = new double[d];
        for (int i = 0; i < d; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < d; j++)
                sum += A[i][j] * v[j];
            result[i] = sum;
        }
        return result;
    }

    /// <summary>Computes Euclidean norm of a vector.</summary>
    private static double VectorNorm(double[] v, int d)
    {
        double sum = 0.0;
        for (int i = 0; i < d; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }

    /// <summary>Computes squared Euclidean distance between two vectors.</summary>
    private static double SquaredDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int j = 0; j < a.Length; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return sum;
    }

    /// <summary>Creates a deep copy of a data array.</summary>
    private static double[][] CopyData(double[][] data)
    {
        double[][] copy = new double[data.Length][];
        for (int i = 0; i < data.Length; i++)
        {
            copy[i] = new double[data[i].Length];
            for (int j = 0; j < data[i].Length; j++)
                copy[i][j] = data[i][j];
        }
        return copy;
    }

    /// <summary>Creates a deep copy of a square matrix.</summary>
    private static double[][] CopySquareMatrix(double[][] matrix, int d)
    {
        double[][] copy = new double[d][];
        for (int i = 0; i < d; i++)
        {
            copy[i] = new double[d];
            for (int j = 0; j < d; j++)
                copy[i][j] = matrix[i][j];
        }
        return copy;
    }
}
