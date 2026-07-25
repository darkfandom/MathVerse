namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;
using System.Collections.Generic;

/// <summary>
/// Implements Gaussian Naive Bayes classifier for multi-class classification.
/// </summary>
public sealed class NaiveBayes
{
    private Dictionary<int, double> _classPriors;
    private Dictionary<int, double[]> _means;
    private Dictionary<int, double[]> _variances;
    private int _featureCount;
    private int _classCount;

    /// <summary>
    /// Gets the number of classes encountered during training.
    /// </summary>
    public int ClassCount => _classCount;

    /// <summary>
    /// Gets the number of features.
    /// </summary>
    public int FeatureCount => _featureCount;

    /// <summary>
    /// Initializes a new instance of the NaiveBayes class.
    /// </summary>
    public NaiveBayes()
    {
        _classPriors = new Dictionary<int, double>();
        _means = new Dictionary<int, double[]>();
        _variances = new Dictionary<int, double[]>();
    }

    /// <summary>
    /// Trains the Gaussian Naive Bayes classifier.
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

        int n = X.Length;
        _featureCount = X[0].Length;

        Dictionary<int, List<int>> classIndices = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int label = y[i];
            if (!classIndices.ContainsKey(label))
                classIndices[label] = new List<int>();
            classIndices[label].Add(i);
        }

        _classCount = classIndices.Count;
        _classPriors.Clear();
        _means.Clear();
        _variances.Clear();

        foreach (var kvp in classIndices)
        {
            int classLabel = kvp.Key;
            List<int> indices = kvp.Value;

            _classPriors[classLabel] = (double)indices.Count / n;

            double[] mean = new double[_featureCount];
            double[] variance = new double[_featureCount];

            for (int f = 0; f < _featureCount; f++)
            {
                double sum = 0.0;
                for (int i = 0; i < indices.Count; i++)
                {
                    sum += X[indices[i]][f];
                }
                mean[f] = sum / indices.Count;

                double sumSq = 0.0;
                for (int i = 0; i < indices.Count; i++)
                {
                    double diff = X[indices[i]][f] - mean[f];
                    sumSq += diff * diff;
                }
                variance[f] = sumSq / indices.Count + 1e-10;
            }

            _means[classLabel] = mean;
            _variances[classLabel] = variance;
        }
    }

    /// <summary>
    /// Predicts class probabilities for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Dictionary mapping class labels to probability arrays.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public Dictionary<int, double[]> PredictProbabilities(double[][] X)
    {
        if (_classPriors.Count == 0)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        Dictionary<int, double[]> allProbs = new Dictionary<int, double[]>();

        foreach (int classLabel in _classPriors.Keys)
        {
            allProbs[classLabel] = new double[X.Length];
        }

        for (int i = 0; i < X.Length; i++)
        {
            Dictionary<int, double> logProbs = new Dictionary<int, double>();

            foreach (int classLabel in _classPriors.Keys)
            {
                double logProb = System.Math.Log(_classPriors[classLabel]);

                for (int f = 0; f < _featureCount; f++)
                {
                    logProb += LogGaussianPDF(X[i][f], _means[classLabel][f], _variances[classLabel][f]);
                }

                logProbs[classLabel] = logProb;
            }

            double maxLogProb = double.MinValue;
            foreach (double lp in logProbs.Values)
            {
                if (lp > maxLogProb)
                    maxLogProb = lp;
            }

            double sumExp = 0.0;
            foreach (double lp in logProbs.Values)
            {
                sumExp += System.Math.Exp(lp - maxLogProb);
            }

            double logSumExp = maxLogProb + System.Math.Log(sumExp);

            foreach (int classLabel in logProbs.Keys)
            {
                allProbs[classLabel][i] = System.Math.Exp(logProbs[classLabel] - logSumExp);
            }
        }

        return allProbs;
    }

    /// <summary>
    /// Predicts class labels for the given feature matrix.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted class labels.</returns>
    public int[] Predict(double[][] X)
    {
        Dictionary<int, double[]> probabilities = PredictProbabilities(X);
        int[] predictions = new int[X.Length];

        for (int i = 0; i < X.Length; i++)
        {
            int bestClass = -1;
            double bestProb = double.MinValue;

            foreach (int classLabel in probabilities.Keys)
            {
                if (probabilities[classLabel][i] > bestProb)
                {
                    bestProb = probabilities[classLabel][i];
                    bestClass = classLabel;
                }
            }

            predictions[i] = bestClass;
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
    /// Computes the log of the Gaussian probability density function.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <param name="mean">Mean of the distribution.</param>
    /// <param name="variance">Variance of the distribution.</param>
    /// <returns>Log probability density.</returns>
    private static double LogGaussianPDF(double x, double mean, double variance)
    {
        double diff = x - mean;
        return -0.5 * (System.Math.Log(2.0 * System.Math.PI * variance) + (diff * diff) / variance);
    }
}
