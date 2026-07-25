namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;

/// <summary>
/// Implements logistic regression for binary classification using gradient descent.
/// </summary>
public sealed class LogisticRegression
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
    /// Initializes a new instance of the LogisticRegression class.
    /// </summary>
    /// <param name="learningRate">Learning rate for gradient descent.</param>
    /// <param name="maxIterations">Maximum number of training iterations.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public LogisticRegression(double learningRate = 0.01, int maxIterations = 1000, double tolerance = 1e-6)
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
    /// Trains the logistic regression model using gradient descent.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Binary labels (0 or 1).</param>
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
            if (y[i] != 0.0 && y[i] != 1.0)
                throw new ArgumentException("Labels must be 0 or 1 for binary classification.");
        }

        int n = X.Length;
        int p = X[0].Length;

        _weights = new double[p];
        _bias = 0.0;

        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            double[] gradWeights = new double[p];
            double gradBias = 0.0;
            double totalLoss = 0.0;

            for (int i = 0; i < n; i++)
            {
                double z = DotProduct(_weights, X[i]) + _bias;
                double prediction = Sigmoid(z);
                double error = prediction - y[i];

                for (int j = 0; j < p; j++)
                {
                    gradWeights[j] += error * X[i][j];
                }
                gradBias += error;

                double loss = -y[i] * System.Math.Log(prediction + 1e-15) -
                              (1.0 - y[i]) * System.Math.Log(1.0 - prediction + 1e-15);
                totalLoss += loss;
            }

            for (int j = 0; j < p; j++)
            {
                _weights[j] -= _learningRate * gradWeights[j] / n;
            }
            _bias -= _learningRate * gradBias / n;

            double avgLoss = totalLoss / n;

            if (iteration > 0 && System.Math.Abs(avgLoss) < _tolerance)
                break;
        }
    }

    /// <summary>
    /// Predicts class probabilities for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted probabilities.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public double[] Predict(double[][] X)
    {
        if (_weights == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int n = X.Length;
        double[] probabilities = new double[n];

        for (int i = 0; i < n; i++)
        {
            double z = DotProduct(_weights, X[i]) + _bias;
            probabilities[i] = Sigmoid(z);
        }

        return probabilities;
    }

    /// <summary>
    /// Predicts class labels (0 or 1) for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted class labels.</returns>
    public double[] PredictClass(double[][] X)
    {
        double[] probabilities = Predict(X);
        double[] labels = new double[probabilities.Length];

        for (int i = 0; i < probabilities.Length; i++)
        {
            labels[i] = probabilities[i] >= 0.5 ? 1.0 : 0.0;
        }

        return labels;
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
    /// Calculates the binary cross-entropy loss.
    /// </summary>
    /// <param name="actual">Actual labels (0 or 1).</param>
    /// <param name="predicted">Predicted probabilities.</param>
    /// <returns>Average cross-entropy loss.</returns>
    public double BinaryCrossEntropy(double[] actual, double[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Arrays must have the same length.");

        double sum = 0.0;
        for (int i = 0; i < actual.Length; i++)
        {
            double p = System.Math.Max(1e-15, System.Math.Min(1.0 - 1e-15, predicted[i]));
            sum += -actual[i] * System.Math.Log(p) - (1.0 - actual[i]) * System.Math.Log(1.0 - p);
        }

        return sum / actual.Length;
    }

    /// <summary>
    /// Computes the sigmoid function: 1 / (1 + exp(-z)).
    /// </summary>
    /// <param name="z">Input value.</param>
    /// <returns>Sigmoid output between 0 and 1.</returns>
    private static double Sigmoid(double z)
    {
        if (z >= 0)
            return 1.0 / (1.0 + System.Math.Exp(-z));
        double expZ = System.Math.Exp(z);
        return expZ / (1.0 + expZ);
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
