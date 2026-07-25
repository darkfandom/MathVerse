namespace MathVerse.Math.AI.MachineLearning.DimensionalityReduction;

using System;

/// <summary>Principal Component Analysis using power iteration for eigendecomposition.</summary>
public sealed class PrincipalComponentAnalysis
{
    private double[][] _components = [];
    private double[] _explainedVariance = [];
    private double[] _explainedVarianceRatio = [];
    private double[] _mean = [];

    /// <summary>Gets the principal component directions (eigenvectors of the covariance matrix).</summary>
    public double[][] Components => _components;

    /// <summary>Gets the proportion of variance explained by each component.</summary>
    public double[] ExplainedVarianceRatio => _explainedVarianceRatio;

    /// <summary>Fits PCA to the data by computing principal components via power iteration.</summary>
    /// <param name="data">Input data matrix (samples x features).</param>
    /// <param name="numComponents">Number of principal components to retain.</param>
    /// <returns>A <see cref="DimensionalityReductionResult"/> containing the fitted model.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or numComponents is invalid.</exception>
    public DimensionalityReductionResult Fit(double[][] data, int numComponents)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        int n = data.Length;
        int d = data[0].Length;

        if (numComponents <= 0 || numComponents > d)
            throw new ArgumentException($"numComponents must be between 1 and {d}.", nameof(numComponents));

        // Center data (subtract mean)
        _mean = ComputeMean(data, d);
        double[][] centered = CenterData(data, _mean);

        // Compute covariance matrix
        double[][] covMatrix = ComputeCovarianceMatrix(centered, n, d);

        // Compute total variance for explained variance ratio
        double totalVariance = 0.0;
        for (int j = 0; j < d; j++)
            totalVariance += covMatrix[j][j];

        // Find top numComponents eigenvectors using deflation power iteration
        _components = new double[numComponents][];
        _explainedVariance = new double[numComponents];
        _explainedVarianceRatio = new double[numComponents];

        // Work on a copy that gets deflated
        double[][] workMatrix = CopyMatrix(covMatrix, d);

        for (int comp = 0; comp < numComponents; comp++)
        {
            double[] eigenvec = PowerIteration(workMatrix, d, 500);
            double eigenval = RayleighQuotient(workMatrix, eigenvec, d);

            _components[comp] = eigenvec;
            _explainedVariance[comp] = eigenval;
            _explainedVarianceRatio[comp] = totalVariance > 0.0 ? eigenval / totalVariance : 0.0;

            // Deflate: subtract rank-1 approximation
            for (int i = 0; i < d; i++)
                for (int j = 0; j < d; j++)
                    workMatrix[i][j] -= eigenval * eigenvec[i] * eigenvec[j];
        }

        // Transform the data
        double[][] transformed = Transform(centered);

        return new DimensionalityReductionResult
        {
            TransformedData = transformed,
            ExplainedVarianceRatio = _explainedVarianceRatio,
            Components = _components,
            OriginalDimensions = d,
            ReducedDimensions = numComponents
        };
    }

    /// <summary>Projects new data onto the fitted principal components.</summary>
    /// <param name="data">Data to transform (samples x features).</param>
    /// <returns>Transformed data in reduced-dimensional space.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Fit has not been called.</exception>
    public double[][] Transform(double[][] data)
    {
        if (_components.Length == 0)
            throw new InvalidOperationException("Model has not been fitted. Call Fit() first.");

        int n = data.Length;
        int numComp = _components.Length;
        double[][] result = new double[n][];

        for (int i = 0; i < n; i++)
        {
            result[i] = new double[numComp];
            for (int c = 0; c < numComp; c++)
            {
                double sum = 0.0;
                for (int j = 0; j < data[i].Length; j++)
                {
                    sum += (data[i][j] - _mean[j]) * _components[c][j];
                }
                result[i][c] = sum;
            }
        }

        return result;
    }

    /// <summary>Runs the power iteration algorithm to find the dominant eigenvector.</summary>
    /// <param name="matrix">Symmetric matrix.</param>
    /// <param name="d">Matrix dimension.</param>
    /// <param name="maxIterations">Maximum iterations.</param>
    /// <returns>The dominant eigenvector.</returns>
    private static double[] PowerIteration(double[][] matrix, int d, int maxIterations)
    {
        var rng = new Random(42);
        double[] v = new double[d];
        for (int i = 0; i < d; i++)
            v[i] = rng.NextDouble() - 0.5;

        // Normalize
        double norm = VectorNorm(v, d);
        for (int i = 0; i < d; i++)
            v[i] /= norm;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // w = A * v
            double[] w = MatVecMul(matrix, v, d);

            // New norm
            double newNorm = VectorNorm(w, d);

            if (newNorm < 1e-15)
                break;

            // v_new = w / ||w||
            double[] vNew = new double[d];
            for (int i = 0; i < d; i++)
                vNew[i] = w[i] / newNorm;

            // Check convergence: |v_new - v| < tolerance
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

    /// <summary>Computes the Rayleigh quotient (v^T A v) for eigenvalue estimation.</summary>
    /// <param name="matrix">Symmetric matrix.</param>
    /// <param name="v">Eigenvector approximation.</param>
    /// <param name="d">Matrix dimension.</param>
    /// <returns>Estimated eigenvalue.</returns>
    private static double RayleighQuotient(double[][] matrix, double[] v, int d)
    {
        double[] Av = MatVecMul(matrix, v, d);
        double numerator = 0.0;
        for (int i = 0; i < d; i++)
            numerator += v[i] * Av[i];
        return numerator;
    }

    /// <summary>Multiplies a matrix by a vector.</summary>
    /// <param name="A">Matrix (d x d).</param>
    /// <param name="v">Vector (d).</param>
    /// <param name="d">Dimension.</param>
    /// <returns>Result vector (d).</returns>
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

    /// <summary>Computes the Euclidean norm of a vector.</summary>
    /// <param name="v">Input vector.</param>
    /// <param name="d">Dimension.</param>
    /// <returns>Vector norm.</returns>
    private static double VectorNorm(double[] v, int d)
    {
        double sum = 0.0;
        for (int i = 0; i < d; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }

    /// <summary>Computes the mean of each feature.</summary>
    /// <param name="data">Data matrix.</param>
    /// <param name="d">Number of features.</param>
    /// <returns>Mean vector.</returns>
    private static double[] ComputeMean(double[][] data, int d)
    {
        int n = data.Length;
        double[] mean = new double[d];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < d; j++)
                mean[j] += data[i][j];
        for (int j = 0; j < d; j++)
            mean[j] /= n;
        return mean;
    }

    /// <summary>Centers data by subtracting the mean.</summary>
    /// <param name="data">Original data.</param>
    /// <param name="mean">Mean vector.</param>
    /// <returns>Centered data matrix.</returns>
    private static double[][] CenterData(double[][] data, double[] mean)
    {
        int n = data.Length;
        int d = mean.Length;
        double[][] centered = new double[n][];
        for (int i = 0; i < n; i++)
        {
            centered[i] = new double[d];
            for (int j = 0; j < d; j++)
                centered[i][j] = data[i][j] - mean[j];
        }
        return centered;
    }

    /// <summary>Computes the covariance matrix (1/(n-1) * X^T X) for centered data.</summary>
    /// <param name="centered">Centered data matrix.</param>
    /// <param name="n">Number of samples.</param>
    /// <param name="d">Number of features.</param>
    /// <returns>Covariance matrix (d x d).</returns>
    private static double[][] ComputeCovarianceMatrix(double[][] centered, int n, int d)
    {
        double[][] cov = new double[d][];
        for (int i = 0; i < d; i++)
        {
            cov[i] = new double[d];
            for (int j = i; j < d; j++)
            {
                double sum = 0.0;
                for (int s = 0; s < n; s++)
                    sum += centered[s][i] * centered[s][j];
                cov[i][j] = sum / (n - 1);
                cov[j][i] = cov[i][j];
            }
        }
        return cov;
    }

    /// <summary>Copies a square matrix.</summary>
    /// <param name="matrix">Source matrix.</param>
    /// <param name="d">Dimension.</param>
    /// <returns>Deep copy.</returns>
    private static double[][] CopyMatrix(double[][] matrix, int d)
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
