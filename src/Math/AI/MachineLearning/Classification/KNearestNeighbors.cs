namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;
using System.Collections.Generic;

/// <summary>
/// Implements K-Nearest Neighbors classifier using Euclidean distance.
/// </summary>
public sealed class KNearestNeighbors
{
    private double[][] _trainingX = [];
    private int[] _trainingY = [];
    private readonly int _defaultK;

    /// <summary>
    /// Gets the number of training samples.
    /// </summary>
    public int TrainingSampleCount => _trainingX?.Length ?? 0;

    /// <summary>
    /// Gets the default k value.
    /// </summary>
    public int DefaultK => _defaultK;

    /// <summary>
    /// Initializes a new instance of the KNearestNeighbors class.
    /// </summary>
    /// <param name="k">Default number of neighbors to consider.</param>
    /// <exception cref="ArgumentException">Thrown when k is less than 1.</exception>
    public KNearestNeighbors(int k = 5)
    {
        if (k < 1)
            throw new ArgumentException("k must be at least 1.", nameof(k));

        _defaultK = k;
    }

    /// <summary>
    /// Stores the training data for later prediction.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Class labels.</param>
    /// <exception cref="ArgumentException">Thrown when input data is invalid.</exception>
    public void Train(double[][] X, int[] y)
    {
        if (X == null || X.Length == 0)
            throw new ArgumentException("Feature matrix cannot be null or empty.", nameof(X));
        if (y == null || y.Length == 0)
            throw new ArgumentException("Label array cannot be null or empty.", nameof(y));
        if (X.Length != y.Length)
            throw new ArgumentException("Number of samples in X must match y length.");

        _trainingX = new double[X.Length][];
        for (int i = 0; i < X.Length; i++)
        {
            _trainingX[i] = (double[])X[i].Clone();
        }

        _trainingY = new int[y.Length];
        Array.Copy(y, _trainingY, y.Length);
    }

    /// <summary>
    /// Predicts the class label for a single sample using k nearest neighbors.
    /// </summary>
    /// <param name="x">Feature vector to classify.</param>
    /// <param name="k">Number of neighbors to consider (uses default if not specified).</param>
    /// <returns>Predicted class label.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    /// <exception cref="ArgumentException">Thrown when k is invalid.</exception>
    public int Predict(double[] x, int k = -1)
    {
        if (_trainingX == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int actualK = k > 0 ? k : _defaultK;
        if (actualK > _trainingX.Length)
            actualK = _trainingX.Length;

        double[] distances = new double[_trainingX.Length];
        for (int i = 0; i < _trainingX.Length; i++)
        {
            distances[i] = EuclideanDistance(x, _trainingX[i]);
        }

        int[] sortedIndices = GetSortedIndices(distances);

        Dictionary<int, int> votes = new Dictionary<int, int>();
        for (int i = 0; i < actualK; i++)
        {
            int idx = sortedIndices[i];
            int label = _trainingY[idx];
            if (votes.ContainsKey(label))
                votes[label]++;
            else
                votes[label] = 1;
        }

        int majorityClass = -1;
        int maxVotes = 0;
        foreach (var kvp in votes)
        {
            if (kvp.Value > maxVotes)
            {
                maxVotes = kvp.Value;
                majorityClass = kvp.Key;
            }
        }

        return majorityClass;
    }

    /// <summary>
    /// Predicts class labels for multiple samples.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <param name="k">Number of neighbors to consider.</param>
    /// <returns>Array of predicted class labels.</returns>
    public int[] Predict(double[][] X, int k = -1)
    {
        int[] predictions = new int[X.Length];
        for (int i = 0; i < X.Length; i++)
        {
            predictions[i] = Predict(X[i], k);
        }
        return predictions;
    }

    /// <summary>
    /// Calculates the accuracy of predictions against actual labels.
    /// </summary>
    /// <param name="actual">Actual labels.</param>
    /// <param name="predicted">Predicted labels.</param>
    /// <returns>Accuracy score between 0 and 1.</returns>
    public double Accuracy(int[] actual, int[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Arrays must have the same length.");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == predicted[i])
                correct++;
        }

        return (double)correct / actual.Length;
    }

    /// <summary>
    /// Calculates Euclidean distance between two vectors.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Euclidean distance.</returns>
    private static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double diff = a[i] - b[i];
            sum += diff * diff;
        }
        return System.Math.Sqrt(sum);
    }

    /// <summary>
    /// Returns indices that would sort the array in ascending order.
    /// </summary>
    /// <param name="values">Array of values.</param>
    /// <returns>Sorted indices.</returns>
    private static int[] GetSortedIndices(double[] values)
    {
        int n = values.Length;
        int[] indices = new int[n];
        for (int i = 0; i < n; i++)
            indices[i] = i;

        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                if (values[indices[j]] < values[indices[minIdx]])
                    minIdx = j;
            }

            if (minIdx != i)
            {
                int temp = indices[i];
                indices[i] = indices[minIdx];
                indices[minIdx] = temp;
            }
        }

        return indices;
    }
}
