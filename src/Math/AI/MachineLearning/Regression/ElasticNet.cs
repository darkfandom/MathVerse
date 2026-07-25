namespace MathVerse.Math.AI.MachineLearning.Regression;

using System;

/// <summary>
/// Implements Elastic Net regression with combined L1 and L2 regularization.
/// </summary>
public sealed class ElasticNet
{
    private double[] _coefficients = Array.Empty<double>();
    private readonly double _alpha;
    private readonly double _l1Ratio;
    private readonly int _maxIterations;
    private readonly double _tolerance;

    /// <summary>
    /// Gets the overall regularization strength.
    /// </summary>
    public double Alpha => _alpha;

    /// <summary>
    /// Gets the L1/L2 mixing ratio (0 = L2 only, 1 = L1 only).
    /// </summary>
    public double L1Ratio => _l1Ratio;

    /// <summary>
    /// Gets the model coefficients (weights) after training.
    /// </summary>
    public double[] Coefficients => _coefficients ?? Array.Empty<double>();

    /// <summary>
    /// Initializes a new instance of the ElasticNet class.
    /// </summary>
    /// <param name="alpha">Overall regularization strength.</param>
    /// <param name="l1Ratio">Mixing parameter between L1 and L2 penalties.</param>
    /// <param name="maxIterations">Maximum number of coordinate descent iterations.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public ElasticNet(double alpha = 1.0, double l1Ratio = 0.5, int maxIterations = 1000, double tolerance = 1e-6)
    {
        if (alpha < 0)
            throw new ArgumentException("Alpha must be non-negative.", nameof(alpha));
        if (l1Ratio < 0 || l1Ratio > 1)
            throw new ArgumentException("L1 ratio must be between 0 and 1.", nameof(l1Ratio));
        if (maxIterations <= 0)
            throw new ArgumentException("Max iterations must be positive.", nameof(maxIterations));
        if (tolerance <= 0)
            throw new ArgumentException("Tolerance must be positive.", nameof(tolerance));

        _alpha = alpha;
        _l1Ratio = l1Ratio;
        _maxIterations = maxIterations;
        _tolerance = tolerance;
    }

    /// <summary>
    /// Trains the Elastic Net regression model using coordinate descent.
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

        _coefficients = new double[p];

        double[] colNorms = new double[p];
        for (int j = 0; j < p; j++)
        {
            double norm = 0.0;
            for (int i = 0; i < n; i++)
            {
                norm += X[i][j] * X[i][j];
            }
            colNorms[j] = norm;
        }

        double[] residuals = new double[n];
        for (int i = 0; i < n; i++)
        {
            residuals[i] = y[i];
        }

        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            double maxChange = 0.0;

            for (int j = 0; j < p; j++)
            {
                double oldCoeff = _coefficients[j];

                double rho = 0.0;
                for (int i = 0; i < n; i++)
                {
                    rho += X[i][j] * (residuals[i] + X[i][j] * _coefficients[j]);
                }

                double l1Penalty = _alpha * _l1Ratio;
                double l2Penalty = _alpha * (1.0 - _l1Ratio);

                _coefficients[j] = SoftThreshold(rho, l1Penalty) / (colNorms[j] + l2Penalty);

                double change = System.Math.Abs(_coefficients[j] - oldCoeff);
                if (change > maxChange)
                    maxChange = change;

                if (System.Math.Abs(_coefficients[j] - oldCoeff) > 1e-12)
                {
                    for (int i = 0; i < n; i++)
                    {
                        residuals[i] -= X[i][j] * (_coefficients[j] - oldCoeff);
                    }
                }
            }

            if (maxChange < _tolerance)
                break;
        }
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
    /// <returns>R-squared score.</returns>
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
    /// Applies the soft thresholding operator for L1 regularization.
    /// </summary>
    /// <param name="z">Input value.</param>
    /// <param name="lambda">Regularization threshold.</param>
    /// <returns>Soft thresholded value.</returns>
    private static double SoftThreshold(double z, double lambda)
    {
        if (z > lambda)
            return z - lambda;
        if (z < -lambda)
            return z + lambda;
        return 0.0;
    }
}
