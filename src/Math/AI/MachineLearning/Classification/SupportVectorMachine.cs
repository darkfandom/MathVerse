namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;

/// <summary>
/// Implements a linear Support Vector Machine classifier using gradient descent on the primal form.
/// </summary>
public sealed class SupportVectorMachine
{
    private double[] _weights = Array.Empty<double>();
    private double _bias;
    private readonly double _learningRate;
    private readonly int _maxIterations;
    private readonly double _tolerance;

    /// <summary>
    /// Gets the model weights after training.
    /// </summary>
    public double[] Weights => _weights ?? Array.Empty<double>();

    /// <summary>
    /// Gets the model bias term after training.
    /// </summary>
    public double Bias => _bias;

    /// <summary>
    /// Initializes a new instance of the SupportVectorMachine class.
    /// </summary>
    /// <param name="learningRate">Learning rate for gradient descent.</param>
    /// <param name="maxIterations">Maximum number of training iterations.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public SupportVectorMachine(double learningRate = 0.01, int maxIterations = 1000, double tolerance = 1e-6)
    {
        if (learningRate <= 0)
            throw new ArgumentException("Learning rate must be positive.", nameof(learningRate));
        if (maxIterations <= 0)
            throw new ArgumentException("Max iterations must be positive.", nameof(maxIterations));
        if (tolerance <= 0)
            throw new ArgumentException("Tolerance must be positive.", nameof(tolerance));

        _learningRate = learningRate;
        _maxIterations = maxIterations;
        _tolerance = tolerance;
    }

    /// <summary>
    /// Trains the SVM classifier using gradient descent on the primal form with hinge loss.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Class labels (-1 or 1).</param>
    /// <exception cref="ArgumentException">Thrown when input data is invalid.</exception>
    public void Train(double[][] X, double[] y)
    {
        if (X == null || X.Length == 0)
            throw new ArgumentException("Feature matrix cannot be null or empty.", nameof(X));
        if (y == null || y.Length == 0)
            throw new ArgumentException("Label array cannot be null or empty.", nameof(y));
        if (X.Length != y.Length)
            throw new ArgumentException("Number of samples in X must match y length.");

        for (int i = 0; i < y.Length; i++)
        {
            if (y[i] != -1.0 && y[i] != 1.0)
                throw new ArgumentException("Labels must be -1 or 1 for SVM classification.");
        }

        int n = X.Length;
        int p = X[0].Length;

        _weights = new double[p];
        _bias = 0.0;

        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            double[] gradWeights = new double[p];
            double gradBias = 0.0;

            for (int i = 0; i < n; i++)
            {
                double decision = DotProduct(_weights, X[i]) + _bias;
                double margin = y[i] * decision;

                if (margin < 1.0)
                {
                    for (int j = 0; j < p; j++)
                    {
                        gradWeights[j] += -y[i] * X[i][j];
                    }
                    gradBias += -y[i];
                }
            }

            for (int j = 0; j < p; j++)
            {
                _weights[j] -= _learningRate * (gradWeights[j] / n + _weights[j]);
            }
            _bias -= _learningRate * gradBias / n;

            double totalLoss = CalculateHingeLoss(X, y);

            if (iteration > 0 && System.Math.Abs(totalLoss) < _tolerance)
                break;
        }
    }

    /// <summary>
    /// Predicts class labels for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted class labels (-1 or 1).</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public double[] Predict(double[][] X)
    {
        if (_weights == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int n = X.Length;
        double[] predictions = new double[n];

        for (int i = 0; i < n; i++)
        {
            double decision = DecisionFunction(X[i]);
            predictions[i] = decision >= 0 ? 1.0 : -1.0;
        }

        return predictions;
    }

    /// <summary>
    /// Calculates the decision function value for a single sample.
    /// </summary>
    /// <param name="x">Feature vector.</param>
    /// <returns>Decision function value.</returns>
    public double DecisionFunction(double[] x)
    {
        if (_weights == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        return DotProduct(_weights, x) + _bias;
    }

    /// <summary>
    /// Calculates the accuracy of predictions against actual labels.
    /// </summary>
    /// <param name="actual">Actual labels.</param>
    /// <param name="predicted">Predicted labels.</param>
    /// <returns>Accuracy score between 0 and 1.</returns>
    public double Accuracy(double[] actual, double[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Arrays must have the same length.");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (System.Math.Abs(actual[i] - predicted[i]) < 1e-10)
                correct++;
        }

        return (double)correct / actual.Length;
    }

    /// <summary>
    /// Calculates the hinge loss for the current model.
    /// </summary>
    /// <param name="X">Feature matrix.</param>
    /// <param name="y">Class labels.</param>
    /// <returns>Average hinge loss.</returns>
    public double CalculateHingeLoss(double[][] X, double[] y)
    {
        if (_weights == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        double totalLoss = 0.0;
        for (int i = 0; i < X.Length; i++)
        {
            double decision = DotProduct(_weights, X[i]) + _bias;
            double loss = System.Math.Max(0.0, 1.0 - y[i] * decision);
            totalLoss += loss;
        }

        return totalLoss / X.Length;
    }

    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Dot product value.</returns>
    private static double DotProduct(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
}
