namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;
using System.Collections.Generic;

/// <summary>
/// Implements a Random Forest classifier using bootstrap aggregation and random feature subsets.
/// </summary>
public sealed class RandomForest
{
    private DecisionTree[] _trees = [];
    private readonly int _treeCount;
    private readonly int _maxDepth;
    private double[] _featureImportances = [];
    private int _featureCount;

    /// <summary>
    /// Gets the number of trees in the forest.
    /// </summary>
    public int TreeCount => _treeCount;

    /// <summary>
    /// Gets the maximum depth of each tree.
    /// </summary>
    public int MaxDepth => _maxDepth;

    /// <summary>
    /// Gets the feature importance scores after training.
    /// </summary>
    public double[] FeatureImportances => _featureImportances ?? Array.Empty<double>();

    /// <summary>
    /// Initializes a new instance of the RandomForest class.
    /// </summary>
    /// <param name="treeCount">Number of trees in the forest.</param>
    /// <param name="maxDepth">Maximum depth of each tree.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public RandomForest(int treeCount = 100, int maxDepth = 10)
    {
        if (treeCount <= 0)
            throw new ArgumentException("Tree count must be positive.", nameof(treeCount));
        if (maxDepth <= 0)
            throw new ArgumentException("Max depth must be positive.", nameof(maxDepth));

        _treeCount = treeCount;
        _maxDepth = maxDepth;
    }

    /// <summary>
    /// Trains the Random Forest classifier.
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
        _featureImportances = new double[_featureCount];

        _trees = new DecisionTree[_treeCount];
        Random random = new Random(42);

        for (int t = 0; t < _treeCount; t++)
        {
            int[] bootstrapIndices = GenerateBootstrapIndices(n, random);
            double[][] bootstrapX = new double[bootstrapIndices.Length][];
            int[] bootstrapY = new int[bootstrapIndices.Length];

            for (int i = 0; i < bootstrapIndices.Length; i++)
            {
                bootstrapX[i] = X[bootstrapIndices[i]];
                bootstrapY[i] = y[bootstrapIndices[i]];
            }

            _trees[t] = new DecisionTree(_maxDepth);
            _trees[t].Train(bootstrapX, bootstrapY);

            UpdateFeatureImportance(_trees[t], X, y);
        }

        NormalizeFeatureImportances();
    }

    /// <summary>
    /// Predicts class labels for multiple samples using majority vote.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted class labels.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public int[] Predict(double[][] X)
    {
        if (_trees == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int[] predictions = new int[X.Length];

        for (int i = 0; i < X.Length; i++)
        {
            Dictionary<int, int> votes = new Dictionary<int, int>();

            for (int t = 0; t < _treeCount; t++)
            {
                int vote = _trees[t].Predict(X[i]);
                if (votes.ContainsKey(vote))
                    votes[vote]++;
                else
                    votes[vote] = 1;
            }

            predictions[i] = GetMajorityVote(votes);
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
    /// Generates bootstrap sample indices.
    /// </summary>
    /// <param name="n">Total number of samples.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>Array of sampled indices.</returns>
    private static int[] GenerateBootstrapIndices(int n, Random random)
    {
        int[] indices = new int[n];
        for (int i = 0; i < n; i++)
        {
            indices[i] = random.Next(n);
        }
        return indices;
    }

    /// <summary>
    /// Gets the majority vote from a dictionary of votes.
    /// </summary>
    /// <param name="votes">Dictionary mapping class labels to vote counts.</param>
    /// <returns>Class label with most votes.</returns>
    private static int GetMajorityVote(Dictionary<int, int> votes)
    {
        int majorityClass = 0;
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
    /// Updates feature importance based on a trained tree.
    /// </summary>
    /// <param name="tree">Trained decision tree.</param>
    /// <param name="X">Feature matrix.</param>
    /// <param name="y">Label array.</param>
    private void UpdateFeatureImportance(DecisionTree tree, double[][] X, int[] y)
    {
        int n = X.Length;
        double baselineAccuracy = CalculateTreeAccuracy(tree, X, y);

        for (int f = 0; f < _featureCount; f++)
        {
            double[][] permutedX = new double[n][];
            for (int i = 0; i < n; i++)
            {
                permutedX[i] = (double[])X[i].Clone();
            }

            Random random = new Random(f);
            for (int i = 0; i < n; i++)
            {
                int j = random.Next(n);
                double temp = permutedX[i][f];
                permutedX[i][f] = permutedX[j][f];
                permutedX[j][f] = temp;
            }

            double permutedAccuracy = CalculateTreeAccuracy(tree, permutedX, y);
            _featureImportances[f] += System.Math.Max(0, baselineAccuracy - permutedAccuracy);
        }
    }

    /// <summary>
    /// Calculates accuracy of a tree on given data.
    /// </summary>
    /// <param name="tree">Decision tree.</param>
    /// <param name="X">Feature matrix.</param>
    /// <param name="y">Label array.</param>
    /// <returns>Accuracy score.</returns>
    private double CalculateTreeAccuracy(DecisionTree tree, double[][] X, int[] y)
    {
        int correct = 0;
        for (int i = 0; i < X.Length; i++)
        {
            if (tree.Predict(X[i]) == y[i])
                correct++;
        }
        return (double)correct / X.Length;
    }

    /// <summary>
    /// Normalizes feature importances to sum to 1.
    /// </summary>
    private void NormalizeFeatureImportances()
    {
        double sum = 0.0;
        for (int i = 0; i < _featureImportances.Length; i++)
        {
            sum += _featureImportances[i];
        }

        if (sum > 0)
        {
            for (int i = 0; i < _featureImportances.Length; i++)
            {
                _featureImportances[i] /= sum;
            }
        }
    }
}
