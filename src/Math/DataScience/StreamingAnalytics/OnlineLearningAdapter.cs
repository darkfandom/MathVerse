namespace MathVerse.Math.DataScience.StreamingAnalytics;

using System;

/// <summary>
/// Provides online learning capability using stochastic gradient descent.
/// Weights are updated incrementally as new training examples arrive.
/// </summary>
public sealed class OnlineLearningAdapter
{
    private double[] _weights;
    private readonly int _featureCount;
    private long _updateCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnlineLearningAdapter"/> class.
    /// </summary>
    /// <param name="featureCount">The number of input features (including bias).</param>
    public OnlineLearningAdapter(int featureCount)
    {
        if (featureCount < 1) throw new ArgumentOutOfRangeException(nameof(featureCount), featureCount, "Must be at least 1.");
        _featureCount = featureCount;
        _weights = new double[featureCount];
        _updateCount = 0;
    }

    /// <summary>
    /// Initializes a new instance with specified initial weights.
    /// </summary>
    /// <param name="initialWeights">The initial weight values.</param>
    public OnlineLearningAdapter(double[] initialWeights)
    {
        if (initialWeights is null) throw new ArgumentNullException(nameof(initialWeights));
        if (initialWeights.Length == 0) throw new ArgumentException("Weights array cannot be empty.", nameof(initialWeights));
        _featureCount = initialWeights.Length;
        _weights = new double[_featureCount];
        System.Array.Copy(initialWeights, _weights, _featureCount);
        _updateCount = 0;
    }

    /// <summary>
    /// Gets the current weight values as a read-only span.
    /// </summary>
    public ReadOnlySpan<double> Weights => _weights;

    /// <summary>
    /// Gets the number of weight updates performed.
    /// </summary>
    public long UpdateCount => _updateCount;

    /// <summary>
    /// Gets the number of features (weights).
    /// </summary>
    public int FeatureCount => _featureCount;

    /// <summary>
    /// Performs a single stochastic gradient descent update step.
    /// Uses the squared error loss: L = 0.5 * (y - prediction)^2.
    /// </summary>
    /// <param name="features">The input feature vector.</param>
    /// <param name="label">The true target value.</param>
    /// <param name="learningRate">The learning rate for this update.</param>
    public void UpdateWeights(double[] features, double label, double learningRate)
    {
        if (features is null) throw new ArgumentNullException(nameof(features));
        if (features.Length != _featureCount)
            throw new ArgumentException(
                $"Feature count ({features.Length}) must match weight count ({_featureCount}).",
                nameof(features));

        double prediction = Predict(features);
        double error = label - prediction;

        for (int j = 0; j < _featureCount; j++)
        {
            _weights[j] += learningRate * error * features[j];
        }

        _updateCount++;
    }

    /// <summary>
    /// Makes a prediction using the current weights.
    /// Computes the dot product of features and weights.
    /// </summary>
    /// <param name="features">The input feature vector.</param>
    /// <returns>The predicted value.</returns>
    public double Predict(double[] features)
    {
        if (features is null) throw new ArgumentNullException(nameof(features));
        if (features.Length != _featureCount)
            throw new ArgumentException(
                $"Feature count ({features.Length}) must match weight count ({_featureCount}).",
                nameof(features));

        double result = 0.0;
        for (int j = 0; j < _featureCount; j++)
        {
            result += features[j] * _weights[j];
        }
        return result;
    }

    /// <summary>
    /// Performs a batch update over multiple training examples.
    /// </summary>
    /// <param name="featureMatrix">The matrix of input features (each row is an example).</param>
    /// <param name="labels">The target values.</param>
    /// <param name="learningRate">The learning rate for each update.</param>
    public void UpdateBatch(double[][] featureMatrix, double[] labels, double learningRate)
    {
        if (featureMatrix is null) throw new ArgumentNullException(nameof(featureMatrix));
        if (labels is null) throw new ArgumentNullException(nameof(labels));
        if (featureMatrix.Length != labels.Length)
            throw new ArgumentException("Feature matrix rows must match labels length.");

        for (int i = 0; i < featureMatrix.Length; i++)
        {
            UpdateWeights(featureMatrix[i], labels[i], learningRate);
        }
    }

    /// <summary>
    /// Resets all weights to zero.
    /// </summary>
    public void Reset()
    {
        for (int j = 0; j < _featureCount; j++)
        {
            _weights[j] = 0.0;
        }
        _updateCount = 0;
    }
}
