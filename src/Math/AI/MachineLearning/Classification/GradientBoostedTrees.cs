namespace MathVerse.Math.AI.MachineLearning.Classification;

using System;

/// <summary>
/// Implements Gradient Boosted Trees for classification using sequential ensemble of shallow trees.
/// </summary>
public sealed class GradientBoostedTrees
{
    private RegressionTree[] _trees = [];
    private double[] _treeWeights = [];
    private double _initialPrediction;
    private readonly int _nEstimators;
    private readonly double _learningRate;
    private readonly int _maxDepth;

    /// <summary>
    /// Gets the number of estimators (trees) in the ensemble.
    /// </summary>
    public int NEstimators => _nEstimators;

    /// <summary>
    /// Gets the learning rate for shrinkage.
    /// </summary>
    public double LearningRate => _learningRate;

    /// <summary>
    /// Gets the maximum depth of each tree.
    /// </summary>
    public int MaxDepth => _maxDepth;

    /// <summary>
    /// Initializes a new instance of the GradientBoostedTrees class.
    /// </summary>
    /// <param name="nEstimators">Number of boosting stages.</param>
    /// <param name="learningRate">Learning rate for shrinkage.</param>
    /// <param name="maxDepth">Maximum depth of each tree.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public GradientBoostedTrees(int nEstimators = 100, double learningRate = 0.1, int maxDepth = 3)
    {
        if (nEstimators <= 0)
            throw new ArgumentException("Number of estimators must be positive.", nameof(nEstimators));
        if (learningRate <= 0 || learningRate > 1)
            throw new ArgumentException("Learning rate must be between 0 and 1.", nameof(learningRate));
        if (maxDepth <= 0)
            throw new ArgumentException("Max depth must be positive.", nameof(maxDepth));

        _nEstimators = nEstimators;
        _learningRate = learningRate;
        _maxDepth = maxDepth;
    }

    /// <summary>
    /// Trains the Gradient Boosted Trees classifier.
    /// </summary>
    /// <param name="X">Training feature matrix (samples x features).</param>
    /// <param name="y">Class labels (0 or 1).</param>
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

        double sum = 0.0;
        for (int i = 0; i < n; i++)
            sum += y[i];
        _initialPrediction = System.Math.Log((sum / n) / (1.0 - sum / n) + 1e-15);

        _trees = new RegressionTree[_nEstimators];
        _treeWeights = new double[_nEstimators];

        double[] currentPredictions = new double[n];
        for (int i = 0; i < n; i++)
            currentPredictions[i] = _initialPrediction;

        for (int t = 0; t < _nEstimators; t++)
        {
            double[] probabilities = new double[n];
            for (int i = 0; i < n; i++)
                probabilities[i] = Sigmoid(currentPredictions[i]);

            double[] residuals = new double[n];
            for (int i = 0; i < n; i++)
                residuals[i] = y[i] - probabilities[i];

            _trees[t] = new RegressionTree(_maxDepth);
            _trees[t].Train(X, residuals);

            double[] treePredictions = _trees[t].Predict(X);

            double bestWeight = FindBestWeight(y, currentPredictions, treePredictions);
            _treeWeights[t] = bestWeight * _learningRate;

            for (int i = 0; i < n; i++)
            {
                currentPredictions[i] += _treeWeights[t] * treePredictions[i];
            }
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
        if (_trees == null)
            throw new InvalidOperationException("Model has not been trained. Call Train() first.");

        int n = X.Length;
        double[] predictions = new double[n];

        for (int i = 0; i < n; i++)
        {
            double score = _initialPrediction;
            for (int t = 0; t < _nEstimators; t++)
            {
                double[] treePred = _trees[t].Predict(new double[][] { X[i] });
                score += _treeWeights[t] * treePred[0];
            }
            predictions[i] = Sigmoid(score);
        }

        return predictions;
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
    /// Computes the sigmoid function.
    /// </summary>
    /// <param name="z">Input value.</param>
    /// <returns>Sigmoid output.</returns>
    private static double Sigmoid(double z)
    {
        if (z >= 0)
            return 1.0 / (1.0 + System.Math.Exp(-z));
        double expZ = System.Math.Exp(z);
        return expZ / (1.0 + expZ);
    }

    /// <summary>
    /// Finds the best weight for a new tree using line search.
    /// </summary>
    /// <param name="y">True labels.</param>
    /// <param name="currentPredictions">Current predictions.</param>
    /// <param name="treePredictions">Predictions from new tree.</param>
    /// <returns>Optimal weight.</returns>
    private double FindBestWeight(double[] y, double[] currentPredictions, double[] treePredictions)
    {
        double bestWeight = 1.0;
        double bestLoss = double.MaxValue;

        double[] testWeights = { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };

        for (int w = 0; w < testWeights.Length; w++)
        {
            double weight = testWeights[w];
            double totalLoss = 0.0;

            for (int i = 0; i < y.Length; i++)
            {
                double newPred = currentPredictions[i] + weight * treePredictions[i];
                double prob = Sigmoid(newPred);
                prob = System.Math.Max(1e-15, System.Math.Min(1.0 - 1e-15, prob));
                totalLoss += -y[i] * System.Math.Log(prob) - (1.0 - y[i]) * System.Math.Log(1.0 - prob);
            }

            if (totalLoss < bestLoss)
            {
                bestLoss = totalLoss;
                bestWeight = weight;
            }
        }

        return bestWeight;
    }

    /// <summary>
    /// Represents a simple regression tree for predicting continuous values.
    /// </summary>
    private sealed class RegressionTree
    {
        private readonly int _maxDepth;
        private RegressionTreeNode _root = null!;

        /// <summary>
        /// Initializes a new regression tree.
        /// </summary>
        /// <param name="maxDepth">Maximum tree depth.</param>
        public RegressionTree(int maxDepth)
        {
            _maxDepth = maxDepth;
        }

        /// <summary>
        /// Trains the regression tree on residuals.
        /// </summary>
        /// <param name="X">Feature matrix.</param>
        /// <param name="y">Target values (residuals).</param>
        public void Train(double[][] X, double[] y)
        {
            int n = X.Length;
            int[] indices = new int[n];
            for (int i = 0; i < n; i++)
                indices[i] = i;

            _root = BuildTree(X, y, indices, 0);
        }

        /// <summary>
        /// Predicts values for the given feature matrix.
        /// </summary>
        /// <param name="X">Feature matrix.</param>
        /// <returns>Predicted values.</returns>
        public double[] Predict(double[][] X)
        {
            double[] predictions = new double[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                predictions[i] = TraverseTree(_root, X[i]);
            }
            return predictions;
        }

        private RegressionTreeNode BuildTree(double[][] X, double[] y, int[] indices, int depth)
        {
            double mean = 0.0;
            for (int i = 0; i < indices.Length; i++)
                mean += y[indices[i]];
            mean /= indices.Length;

            if (indices.Length <= 1 || depth >= _maxDepth)
            {
                return new RegressionTreeNode { Value = mean };
            }

            int bestFeature = -1;
            double bestThreshold = 0.0;
            double bestMSE = double.MaxValue;

            for (int f = 0; f < X[0].Length; f++)
            {
                double[] featureValues = new double[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                    featureValues[i] = X[indices[i]][f];

                double[] thresholds = GetThresholds(featureValues);

                for (int t = 0; t < thresholds.Length; t++)
                {
                    double threshold = thresholds[t];
                    double mse = CalculateSplitMSE(y, indices, f, threshold);

                    if (mse < bestMSE)
                    {
                        bestMSE = mse;
                        bestFeature = f;
                        bestThreshold = threshold;
                    }
                }
            }

            if (bestFeature == -1)
                return new RegressionTreeNode { Value = mean };

            System.Collections.Generic.List<int> leftIndices = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> rightIndices = new System.Collections.Generic.List<int>();

            for (int i = 0; i < indices.Length; i++)
            {
                if (X[indices[i]][bestFeature] <= bestThreshold)
                    leftIndices.Add(indices[i]);
                else
                    rightIndices.Add(indices[i]);
            }

            if (leftIndices.Count == 0 || rightIndices.Count == 0)
                return new RegressionTreeNode { Value = mean };

            return new RegressionTreeNode
            {
                FeatureIndex = bestFeature,
                Threshold = bestThreshold,
                Left = BuildTree(X, y, leftIndices.ToArray(), depth + 1),
                Right = BuildTree(X, y, rightIndices.ToArray(), depth + 1)
            };
        }

        private double TraverseTree(RegressionTreeNode node, double[] x)
        {
            if (node.Left == null || node.Right == null)
                return node.Value;

            if (x[node.FeatureIndex] <= node.Threshold)
                return TraverseTree(node.Left, x);
            else
                return TraverseTree(node.Right, x);
        }

        private double CalculateSplitMSE(double[] y, int[] indices, int featureIndex, double threshold)
        {
            System.Collections.Generic.List<int> leftIndices = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> rightIndices = new System.Collections.Generic.List<int>();

            double[][] dummyX = new double[1][];

            for (int i = 0; i < indices.Length; i++)
            {
                leftIndices.Add(indices[i]);
            }

            double leftMSE = CalculateMSE(y, leftIndices.ToArray());
            double rightMSE = CalculateMSE(y, rightIndices.ToArray());

            double leftWeight = (double)leftIndices.Count / indices.Length;
            double rightWeight = (double)rightIndices.Count / indices.Length;

            return leftWeight * leftMSE + rightWeight * rightMSE;
        }

        private double CalculateMSE(double[] y, int[] indices)
        {
            if (indices.Length == 0)
                return 0.0;

            double mean = 0.0;
            for (int i = 0; i < indices.Length; i++)
                mean += y[indices[i]];
            mean /= indices.Length;

            double mse = 0.0;
            for (int i = 0; i < indices.Length; i++)
            {
                double diff = y[indices[i]] - mean;
                mse += diff * diff;
            }

            return mse / indices.Length;
        }

        private static double[] GetThresholds(double[] values)
        {
            System.Collections.Generic.HashSet<double> uniqueValues = new System.Collections.Generic.HashSet<double>();
            for (int i = 0; i < values.Length; i++)
                uniqueValues.Add(values[i]);

            double[] sorted = new double[uniqueValues.Count];
            uniqueValues.CopyTo(sorted);
            Array.Sort(sorted);

            if (sorted.Length <= 5)
                return sorted;

            double[] thresholds = new double[5];
            for (int i = 0; i < thresholds.Length; i++)
            {
                int idx = i * (sorted.Length - 1) / (thresholds.Length - 1);
                thresholds[i] = sorted[idx];
            }

            return thresholds;
        }

        private sealed class RegressionTreeNode
        {
            public int FeatureIndex { get; set; } = -1;
            public double Threshold { get; set; }
            public RegressionTreeNode? Left { get; set; }
            public RegressionTreeNode? Right { get; set; }
            public double Value { get; set; }
        }
    }
}
