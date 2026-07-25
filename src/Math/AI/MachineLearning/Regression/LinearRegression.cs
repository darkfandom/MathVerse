namespace MathVerse.Math.AI.MachineLearning.Regression;

using System;

/// <summary>
/// Implements ordinary least squares linear regression using the normal equation method.
/// </summary>
public sealed class LinearRegression
{
    private double[] _coefficients = Array.Empty<double>();

    /// <summary>
    /// Gets the model coefficients (weights) after training.
    /// </summary>
    public double[] Coefficients => _coefficients ?? Array.Empty<double>();

    /// <summary>
    /// Trains the linear regression model using the normal equation: (X^T X)^-1 X^T y.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Target values.</param>
    /// <exception cref="ArgumentException">Thrown when input data is invalid.</exception>
    public void Train(double[][] X, double[] y)
    {
        if (X == null || X.Length == 0)
            throw new ArgumentException("Feature matrix cannot be null or empty.", nameof(X));
        if (y == null || y.Length == 0)
            throw new ArgumentException("Target array cannot be null or empty.", nameof(y));
        if (X.Length != y.Length)
            throw new ArgumentException("Number of samples in X must match y length.");

        int n = X.Length;
        int p = X[0].Length;

        double[][] Xt = Transpose(X);
        double[][] XtX = Multiply(Xt, X);
        double[] Xty = MultiplyVector(Xt, y);

        double[][] XtXInverse = Inverse(XtX);
        _coefficients = MultiplyVector(XtXInverse, Xty);
    }

    /// <summary>
    /// Predicts target values for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public double[] Predict(double[][] X)
    {
        if (_coefficients == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int n = X.Length;
        int p = _coefficients.Length;
        double[] predictions = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < p; j++)
            {
                sum += X[i][j] * _coefficients[j];
            }
            predictions[i] = sum;
        }

        return predictions;
    }

    /// <summary>
    /// Calculates the R-squared (coefficient of determination) score.
    /// </summary>
    /// <param name="actual">Actual target values.</param>
    /// <param name="predicted">Predicted values.</param>
    /// <returns>R-squared score between negative infinity and 1.</returns>
    public double R2Score(double[] actual, double[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Arrays must have the same length.");

        double mean = 0.0;
        for (int i = 0; i < actual.Length; i++)
            mean += actual[i];
        mean /= actual.Length;

        double ssRes = 0.0;
        double ssTot = 0.0;
        for (int i = 0; i < actual.Length; i++)
        {
            double diff = actual[i] - predicted[i];
            ssRes += diff * diff;
            double totalDiff = actual[i] - mean;
            ssTot += totalDiff * totalDiff;
        }

        if (ssTot == 0.0)
            return 0.0;

        return 1.0 - (ssRes / ssTot);
    }

    /// <summary>
    /// Calculates the mean squared error between actual and predicted values.
    /// </summary>
    /// <param name="actual">Actual target values.</param>
    /// <param name="predicted">Predicted values.</param>
    /// <returns>Mean squared error value.</returns>
    public double MeanSquaredError(double[] actual, double[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Arrays must have the same length.");

        double sum = 0.0;
        for (int i = 0; i < actual.Length; i++)
        {
            double diff = actual[i] - predicted[i];
            sum += diff * diff;
        }

        return sum / actual.Length;
    }

    /// <summary>
    /// Transposes a matrix.
    /// </summary>
    /// <param name="matrix">Input matrix.</param>
    /// <returns>Transposed matrix.</returns>
    private static double[][] Transpose(double[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;
        double[][] result = new double[cols][];

        for (int j = 0; j < cols; j++)
        {
            result[j] = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                result[j][i] = matrix[i][j];
            }
        }

        return result;
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="A">First matrix (m x n).</param>
    /// <param name="B">Second matrix (n x p).</param>
    /// <returns>Result matrix (m x p).</returns>
    private static double[][] Multiply(double[][] A, double[][] B)
    {
        int m = A.Length;
        int n = A[0].Length;
        int p = B[0].Length;

        double[][] result = new double[m][];
        for (int i = 0; i < m; i++)
        {
            result[i] = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                {
                    sum += A[i][k] * B[k][j];
                }
                result[i][j] = sum;
            }
        }

        return result;
    }

    /// <summary>
    /// Multiplies a matrix by a vector.
    /// </summary>
    /// <param name="A">Matrix (m x n).</param>
    /// <param name="v">Vector (n).</param>
    /// <returns>Result vector (m).</returns>
    private static double[] MultiplyVector(double[][] A, double[] v)
    {
        int m = A.Length;
        int n = A[0].Length;
        double[] result = new double[m];

        for (int i = 0; i < m; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += A[i][j] * v[j];
            }
            result[i] = sum;
        }

        return result;
    }

    /// <summary>
    /// Computes the inverse of a square matrix using Gaussian elimination with partial pivoting.
    /// </summary>
    /// <param name="matrix">Square matrix to invert.</param>
    /// <returns>Inverse matrix.</returns>
    private static double[][] Inverse(double[][] matrix)
    {
        int n = matrix.Length;
        double[][] augmented = new double[n][];

        for (int i = 0; i < n; i++)
        {
            augmented[i] = new double[2 * n];
            for (int j = 0; j < n; j++)
            {
                augmented[i][j] = matrix[i][j];
                augmented[i][n + j] = (i == j) ? 1.0 : 0.0;
            }
        }

        for (int col = 0; col < n; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(augmented[col][col]);
            for (int row = col + 1; row < n; row++)
            {
                double absVal = System.Math.Abs(augmented[row][col]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                double[] temp = augmented[col];
                augmented[col] = augmented[maxRow];
                augmented[maxRow] = temp;
            }

            double pivot = augmented[col][col];
            if (System.Math.Abs(pivot) < 1e-12)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            for (int j = 0; j < 2 * n; j++)
                augmented[col][j] /= pivot;

            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = augmented[row][col];
                for (int j = 0; j < 2 * n; j++)
                    augmented[row][j] -= factor * augmented[col][j];
            }
        }

        double[][] inverse = new double[n][];
        for (int i = 0; i < n; i++)
        {
            inverse[i] = new double[n];
            for (int j = 0; j < n; j++)
            {
                inverse[i][j] = augmented[i][n + j];
            }
        }

        return inverse;
    }
}
