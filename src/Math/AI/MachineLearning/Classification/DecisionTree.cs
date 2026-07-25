namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;
using System.Collections.Generic;

/// <summary>
/// Implements a decision tree classifier using ID3-like algorithm with information gain splitting.
/// </summary>
public sealed class DecisionTree
{
    private TreeNode _root = null!;
    private int _maxDepth;

    /// <summary>
    /// Gets the maximum depth of the tree.
    /// </summary>
    public int MaxDepth => _maxDepth;

    /// <summary>
    /// Initializes a new instance of the DecisionTree class.
    /// </summary>
    /// <param name="maxDepth">Maximum depth of the tree.</param>
    /// <exception cref="ArgumentException">Thrown when maxDepth is less than 1.</exception>
    public DecisionTree(int maxDepth = 10)
    {
        if (maxDepth < 1)
            throw new ArgumentException("Max depth must be at least 1.", nameof(maxDepth));

        _maxDepth = maxDepth;
    }

    /// <summary>
    /// Trains the decision tree classifier.
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

        int[] sampleIndices = new int[y.Length];
        for (int i = 0; i < y.Length; i++)
            sampleIndices[i] = i;

        int[] featureIndices = new int[X[0].Length];
        for (int i = 0; i < featureIndices.Length; i++)
            featureIndices[i] = i;

        _root = BuildTree(X, y, sampleIndices, featureIndices, 0);
    }

    /// <summary>
    /// Predicts the class label for a single sample.
    /// </summary>
    /// <param name="x">Feature vector to classify.</param>
    /// <returns>Predicted class label.</returns>
    /// <exception cref="InvalidOperationException">Thrown when model has not been trained.</exception>
    public int Predict(double[] x)
    {
        if (_root == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        return TraverseTree(_root, x);
    }

    /// <summary>
    /// Predicts class labels for multiple samples.
    /// </summary>
    /// <param name="X">Feature matrix to predict on.</param>
    /// <returns>Array of predicted class labels.</returns>
    public int[] Predict(double[][] X)
    {
        int[] predictions = new int[X.Length];
        for (int i = 0; i < X.Length; i++)
        {
            predictions[i] = Predict(X[i]);
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
    /// Recursively builds the decision tree.
    /// </summary>
    /// <param name="X">Feature matrix.</param>
    /// <param name="y">Label array.</param>
    /// <param name="sampleIndices">Indices of current samples.</param>
    /// <param name="featureIndices">Indices of available features.</param>
    /// <param name="currentDepth">Current depth in the tree.</param>
    /// <returns>Root node of the subtree.</returns>
    private TreeNode BuildTree(double[][] X, int[] y, int[] sampleIndices, int[] featureIndices, int currentDepth)
    {
        Dictionary<int, int> classCounts = new Dictionary<int, int>();
        for (int i = 0; i < sampleIndices.Length; i++)
        {
            int label = y[sampleIndices[i]];
            if (classCounts.ContainsKey(label))
                classCounts[label]++;
            else
                classCounts[label] = 1;
        }

        int majorityClass = GetMajorityClass(classCounts);

        if (sampleIndices.Length <= 1 || featureIndices.Length == 0 || currentDepth >= _maxDepth)
        {
            return new TreeNode { ClassLabel = majorityClass };
        }

        bool allSameClass = true;
        int firstLabel = y[sampleIndices[0]];
        for (int i = 1; i < sampleIndices.Length; i++)
        {
            if (y[sampleIndices[i]] != firstLabel)
            {
                allSameClass = false;
                break;
            }
        }

        if (allSameClass)
        {
            return new TreeNode { ClassLabel = majorityClass };
        }

        int bestFeature = -1;
        double bestThreshold = 0.0;
        double bestGain = double.NegativeInfinity;

        for (int f = 0; f < featureIndices.Length; f++)
        {
            int featureIdx = featureIndices[f];
            double[] featureValues = new double[sampleIndices.Length];
            for (int i = 0; i < sampleIndices.Length; i++)
            {
                featureValues[i] = X[sampleIndices[i]][featureIdx];
            }

            double[] thresholds = GetUniqueThresholds(featureValues);

            for (int t = 0; t < thresholds.Length; t++)
            {
                double threshold = thresholds[t];
                double gain = CalculateInformationGain(y, sampleIndices, featureIdx, threshold);

                if (gain > bestGain)
                {
                    bestGain = gain;
                    bestFeature = featureIdx;
                    bestThreshold = threshold;
                }
            }
        }

        if (bestGain <= 0)
        {
            return new TreeNode { ClassLabel = majorityClass };
        }

        List<int> leftIndices = new List<int>();
        List<int> rightIndices = new List<int>();

        for (int i = 0; i < sampleIndices.Length; i++)
        {
            if (X[sampleIndices[i]][bestFeature] <= bestThreshold)
                leftIndices.Add(sampleIndices[i]);
            else
                rightIndices.Add(sampleIndices[i]);
        }

        List<int> remainingFeatures = new List<int>(featureIndices);
        remainingFeatures.Remove(bestFeature);

        TreeNode leftChild = leftIndices.Count > 0
            ? BuildTree(X, y, leftIndices.ToArray(), remainingFeatures.ToArray(), currentDepth + 1)
            : new TreeNode { ClassLabel = majorityClass };

        TreeNode rightChild = rightIndices.Count > 0
            ? BuildTree(X, y, rightIndices.ToArray(), remainingFeatures.ToArray(), currentDepth + 1)
            : new TreeNode { ClassLabel = majorityClass };

        return new TreeNode
        {
            FeatureIndex = bestFeature,
            Threshold = bestThreshold,
            Left = leftChild,
            Right = rightChild
        };
    }

    /// <summary>
    /// Traverses the tree to make a prediction for a single sample.
    /// </summary>
    /// <param name="node">Current tree node.</param>
    /// <param name="x">Feature vector.</param>
    /// <returns>Predicted class label.</returns>
    private int TraverseTree(TreeNode node, double[] x)
    {
        if (node.Left == null || node.Right == null)
            return node.ClassLabel;

        if (x[node.FeatureIndex] <= node.Threshold)
            return TraverseTree(node.Left, x);
        else
            return TraverseTree(node.Right, x);
    }

    /// <summary>
    /// Gets the majority class from a dictionary of class counts.
    /// </summary>
    /// <param name="classCounts">Dictionary mapping class labels to counts.</param>
    /// <returns>Majority class label.</returns>
    private static int GetMajorityClass(Dictionary<int, int> classCounts)
    {
        int majorityClass = 0;
        int maxCount = 0;

        foreach (var kvp in classCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                majorityClass = kvp.Key;
            }
        }

        return majorityClass;
    }

    /// <summary>
    /// Calculates information gain for a binary split.
    /// </summary>
    /// <param name="y">Label array.</param>
    /// <param name="sampleIndices">Indices of current samples.</param>
    /// <param name="featureIndex">Feature index to split on.</param>
    /// <param name="threshold">Split threshold.</param>
    /// <returns>Information gain value.</returns>
    private double CalculateInformationGain(int[] y, int[] sampleIndices, int featureIndex, double threshold)
    {
        double parentEntropy = CalculateEntropy(y, sampleIndices);

        List<int> leftIndices = new List<int>();
        List<int> rightIndices = new List<int>();

        double[][] dummyX = new double[1][];

        for (int i = 0; i < sampleIndices.Length; i++)
        {
            leftIndices.Add(sampleIndices[i]);
        }

        double leftWeight = (double)leftIndices.Count / sampleIndices.Length;
        double rightWeight = (double)rightIndices.Count / sampleIndices.Length;

        double childEntropy = leftWeight * CalculateEntropy(y, leftIndices.ToArray()) +
                              rightWeight * CalculateEntropy(y, rightIndices.ToArray());

        return parentEntropy - childEntropy;
    }

    /// <summary>
    /// Calculates the Gini impurity for a set of samples.
    /// </summary>
    /// <param name="y">Label array.</param>
    /// <param name="sampleIndices">Indices of current samples.</param>
    /// <returns>Gini impurity value.</returns>
    private double CalculateGini(int[] y, int[] sampleIndices)
    {
        if (sampleIndices.Length == 0)
            return 0.0;

        Dictionary<int, int> classCounts = new Dictionary<int, int>();
        for (int i = 0; i < sampleIndices.Length; i++)
        {
            int label = y[sampleIndices[i]];
            if (classCounts.ContainsKey(label))
                classCounts[label]++;
            else
                classCounts[label] = 1;
        }

        double gini = 1.0;
        int n = sampleIndices.Length;

        foreach (var kvp in classCounts)
        {
            double p = (double)kvp.Value / n;
            gini -= p * p;
        }

        return gini;
    }

    /// <summary>
    /// Calculates the Shannon entropy for a set of samples.
    /// </summary>
    /// <param name="y">Label array.</param>
    /// <param name="sampleIndices">Indices of current samples.</param>
    /// <returns>Entropy value.</returns>
    private double CalculateEntropy(int[] y, int[] sampleIndices)
    {
        if (sampleIndices.Length == 0)
            return 0.0;

        Dictionary<int, int> classCounts = new Dictionary<int, int>();
        for (int i = 0; i < sampleIndices.Length; i++)
        {
            int label = y[sampleIndices[i]];
            if (classCounts.ContainsKey(label))
                classCounts[label]++;
            else
                classCounts[label] = 1;
        }

        double entropy = 0.0;
        int n = sampleIndices.Length;

        foreach (var kvp in classCounts)
        {
            double p = (double)kvp.Value / n;
            if (p > 0)
                entropy -= p * System.Math.Log(p) / System.Math.Log(2.0);
        }

        return entropy;
    }

    /// <summary>
    /// Gets unique threshold values for a feature.
    /// </summary>
    /// <param name="values">Feature values.</param>
    /// <returns>Array of unique threshold values.</returns>
    private static double[] GetUniqueThresholds(double[] values)
    {
        HashSet<double> uniqueValues = new HashSet<double>();
        for (int i = 0; i < values.Length; i++)
        {
            uniqueValues.Add(values[i]);
        }

        double[] sorted = new double[uniqueValues.Count];
        uniqueValues.CopyTo(sorted);
        Array.Sort(sorted);

        if (sorted.Length <= 10)
            return sorted;

        double[] thresholds = new double[System.Math.Min(10, sorted.Length)];
        for (int i = 0; i < thresholds.Length; i++)
        {
            int idx = i * (sorted.Length - 1) / (thresholds.Length - 1);
            thresholds[i] = sorted[idx];
        }

        return thresholds;
    }

    /// <summary>
    /// Represents a node in the decision tree.
    /// </summary>
    private sealed class TreeNode
    {
        /// <summary>
        /// Feature index for splitting (leaf nodes have -1).
        /// </summary>
        public int FeatureIndex { get; set; } = -1;

        /// <summary>
        /// Split threshold value.
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Left child node (values &lt;= threshold).
        /// </summary>
        public TreeNode? Left { get; set; }

        /// <summary>
        /// Right child node (values > threshold).
        /// </summary>
        public TreeNode? Right { get; set; }

        /// <summary>
        /// Class label for leaf nodes.
        /// </summary>
        public int ClassLabel { get; set; }
    }
}
