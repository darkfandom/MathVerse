namespace MathVerse.Math.Visualization.Integration;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Represents a trained machine learning model for visualization.</summary>
public sealed class TrainedModel
{
    /// <summary>Gets the model name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the model type.</summary>
    public string ModelType { get; init; } = "";

    /// <summary>Gets the model weights or parameters.</summary>
    public List<double> Weights { get; init; } = new();

    /// <summary>Gets the input feature names.</summary>
    public List<string> FeatureNames { get; init; } = new();
}

/// <summary>Represents a data point for ML visualization.</summary>
public sealed class DataPoint
{
    /// <summary>Gets the feature values.</summary>
    public double[] Features { get; init; } = new double[0];

    /// <summary>Gets the label or target value.</summary>
    public double Label { get; init; }

    /// <summary>Gets the predicted value.</summary>
    public double Prediction { get; init; }

    /// <summary>Gets the cluster assignment.</summary>
    public int Cluster { get; init; }
}

/// <summary>Integrates with AI module for ML visualization.</summary>
public sealed class AIIntegration
{
    private static readonly string[] s_clusterColors = {
        "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7",
        "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9"
    };

    /// <summary>Creates a scatter plot of data points colored by class label.</summary>
    /// <param name="dataPoints">The data points.</param>
    /// <param name="featureIndexX">The X feature index.</param>
    /// <param name="featureIndexY">The Y feature index.</param>
    /// <returns>Point clouds grouped by class.</returns>
    public static Dictionary<int, Core.PointCloud> CreateClassScatterPlot(
        List<DataPoint> dataPoints, int featureIndexX = 0, int featureIndexY = 1)
    {
        var grouped = new Dictionary<int, Core.PointCloud>();

        foreach (var dp in dataPoints)
        {
            int label = (int)dp.Label;

            if (!grouped.ContainsKey(label))
            {
                grouped[label] = new Core.PointCloud
                {
                    Id = $"ml-class-{label}",
                    Color = s_clusterColors[label % s_clusterColors.Length],
                    PointSize = 4.0,
                    Points = new List<Vector3>()
                };
            }

            if (dp.Features.Length > System.Math.Max(featureIndexX, featureIndexY))
            {
                grouped[label].Points.Add(new Vector3(
                    (float)dp.Features[featureIndexX],
                    (float)dp.Features[featureIndexY],
                    0));
            }
        }

        return grouped;
    }

    /// <summary>Creates a decision boundary visualization by evaluating the model over a grid.</summary>
    /// <param name="model">The trained model.</param>
    /// <param name="xMin">The X range minimum.</param>
    /// <param name="xMax">The X range maximum.</param>
    /// <param name="yMin">The Y range minimum.</param>
    /// <param name="yMax">The Y range maximum.</param>
    /// <param name="resolution">The grid resolution.</param>
    /// <returns>Grid points with predictions.</returns>
    public static (double[,] XGrid, double[,] YGrid, double[,] Predictions) CreateDecisionBoundary(
        TrainedModel model, double xMin, double xMax, double yMin, double yMax, int resolution = 50)
    {
        double xStep = (xMax - xMin) / System.Math.Max(1, resolution - 1);
        double yStep = (yMax - yMin) / System.Math.Max(1, resolution - 1);

        double[,] xGrid = new double[resolution, resolution];
        double[,] yGrid = new double[resolution, resolution];
        double[,] predictions = new double[resolution, resolution];

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                double x = xMin + i * xStep;
                double y = yMin + j * yStep;

                xGrid[j, i] = x;
                yGrid[j, i] = y;
                predictions[j, i] = EvaluateSimpleModel(model, new double[] { x, y });
            }
        }

        return (xGrid, yGrid, predictions);
    }

    /// <summary>Creates a confusion matrix visualization.</summary>
    /// <param name="actual">The actual labels.</param>
    /// <param name="predicted">The predicted labels.</param>
    /// <param name="numClasses">The number of classes.</param>
    /// <returns>The confusion matrix.</returns>
    public static int[,] CreateConfusionMatrix(int[] actual, int[] predicted, int numClasses)
    {
        int[,] matrix = new int[numClasses, numClasses];
        int count = System.Math.Min(actual.Length, predicted.Length);

        for (int i = 0; i < count; i++)
        {
            int a = System.Math.Max(0, System.Math.Min(numClasses - 1, actual[i]));
            int p = System.Math.Max(0, System.Math.Min(numClasses - 1, predicted[i]));
            matrix[a, p]++;
        }

        return matrix;
    }

    /// <summary>Computes ROC curve points from scores and labels.</summary>
    /// <param name="scores">The prediction scores.</param>
    /// <param name="labels">The true labels (0 or 1).</param>
    /// <param name="numThresholds">The number of threshold points.</param>
    /// <returns>TPR and FPR arrays.</returns>
    public static (double[] FPR, double[] TPR) ComputeROCCurve(double[] scores, int[] labels, int numThresholds = 100)
    {
        int count = System.Math.Min(scores.Length, labels.Length);

        int positives = 0;
        int negatives = 0;
        for (int i = 0; i < count; i++)
        {
            if (labels[i] == 1) positives++;
            else negatives++;
        }

        if (positives == 0 || negatives == 0)
            return (new double[] { 0, 1 }, new double[] { 0, 1 });

        double[] fpr = new double[numThresholds + 1];
        double[] tpr = new double[numThresholds + 1];

        fpr[0] = 0;
        tpr[0] = 0;

        for (int t = 1; t <= numThresholds; t++)
        {
            double threshold = (double)t / numThresholds;

            int tp = 0, fp = 0;
            for (int i = 0; i < count; i++)
            {
                if (scores[i] >= threshold)
                {
                    if (labels[i] == 1) tp++;
                    else fp++;
                }
            }

            fpr[t] = (double)fp / negatives;
            tpr[t] = (double)tp / positives;
        }

        return (fpr, tpr);
    }

    /// <summary>Creates a loss curve visualization.</summary>
    /// <param name="trainLosses">The training losses per epoch.</param>
    /// <param name="valLosses">The validation losses per epoch.</param>
    /// <returns>Line plots for the loss curves.</returns>
    public static List<Core.LinePlot> CreateLossCurves(double[] trainLosses, double[]? valLosses = null)
    {
        var plots = new List<Core.LinePlot>();

        var trainPlot = new Core.LinePlot
        {
            Id = "ml-train-loss",
            Color = "#FF6B6B",
            LineWidth = 2.0,
            Points = new List<Vector2>()
        };

        for (int i = 0; i < trainLosses.Length; i++)
        {
            trainPlot.Points.Add(new Vector2(i, (float)trainLosses[i]));
        }

        plots.Add(trainPlot);

        if (valLosses != null)
        {
            var valPlot = new Core.LinePlot
            {
                Id = "ml-val-loss",
                Color = "#4ECDC4",
                LineWidth = 2.0,
                Points = new List<Vector2>()
            };

            for (int i = 0; i < valLosses.Length; i++)
            {
                valPlot.Points.Add(new Vector2(i, (float)valLosses[i]));
            }

            plots.Add(valPlot);
        }

        return plots;
    }

    /// <summary>Creates a feature importance bar chart.</summary>
    /// <param name="importance">The feature importance scores.</param>
    /// <param name="featureNames">The feature names.</param>
    /// <returns>Visualization data for the importance chart.</returns>
    public static List<(string Feature, double Importance, string Color)> CreateFeatureImportance(
        double[] importance, string[]? featureNames = null)
    {
        var result = new List<(string, double, string)>();

        int count = importance.Length;
        double[] sorted = new double[count];
        System.Array.Copy(importance, sorted, count);
        System.Array.Sort(sorted);
        System.Array.Reverse(sorted);

        double maxImportance = sorted.Length > 0 ? sorted[0] : 1.0;
        if (maxImportance <= 0) maxImportance = 1.0;

        for (int i = 0; i < count; i++)
        {
            string name = featureNames != null && i < featureNames.Length
                ? featureNames[i]
                : $"Feature {i}";

            double normalizedImportance = importance[i] / maxImportance;
            string color = GetImportanceColor(normalizedImportance);

            result.Add((name, importance[i], color));
        }

        result.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        return result;
    }

    /// <summary>Creates a cluster visualization from k-means results.</summary>
    /// <param name="dataPoints">The data points with cluster assignments.</param>
    /// <returns>Point clouds grouped by cluster.</returns>
    public static Dictionary<int, Core.PointCloud> CreateClusterVisualization(List<DataPoint> dataPoints)
    {
        var clusters = new Dictionary<int, Core.PointCloud>();

        foreach (var dp in dataPoints)
        {
            if (!clusters.ContainsKey(dp.Cluster))
            {
                clusters[dp.Cluster] = new Core.PointCloud
                {
                    Id = $"ml-cluster-{dp.Cluster}",
                    Color = s_clusterColors[dp.Cluster % s_clusterColors.Length],
                    PointSize = 4.0,
                    Points = new List<Vector3>()
                };
            }

            if (dp.Features.Length >= 2)
            {
                clusters[dp.Cluster].Points.Add(new Vector3(
                    (float)dp.Features[0],
                    (float)dp.Features[1],
                    0));
            }
        }

        return clusters;
    }

    /// <summary>Creates a neural network layer visualization.</summary>
    /// <param name="layerSizes">The number of neurons per layer.</param>
    /// <param name="spacing">The horizontal spacing between layers.</param>
    /// <returns>Node positions and edge connections.</returns>
    public static (List<Vector2> Nodes, List<(int From, int To)> Edges) CreateNeuralNetworkLayout(
        int[] layerSizes, double spacing = 2.0)
    {
        var nodes = new List<Vector2>();
        var edges = new List<(int, int)>();

        int nodeIndex = 0;
        int prevLayerStart = -1;
        int prevLayerSize = 0;

        for (int layer = 0; layer < layerSizes.Length; layer++)
        {
            int layerSize = layerSizes[layer];
            double layerX = layer * spacing;
            double startY = -(layerSize - 1) / 2.0;

            int currentLayerStart = nodeIndex;

            for (int neuron = 0; neuron < layerSize; neuron++)
            {
                double y = startY + neuron;
                nodes.Add(new Vector2((float)layerX, (float)y));
                nodeIndex++;
            }

            if (prevLayerStart >= 0)
            {
                for (int prev = 0; prev < prevLayerSize; prev++)
                {
                    for (int curr = 0; curr < layerSize; curr++)
                    {
                        edges.Add((prevLayerStart + prev, currentLayerStart + curr));
                    }
                }
            }

            prevLayerStart = currentLayerStart;
            prevLayerSize = layerSize;
        }

        return (nodes, edges);
    }

    /// <summary>Computes precision-recall curve points.</summary>
    /// <param name="scores">The prediction scores.</param>
    /// <param name="labels">The true labels.</param>
    /// <param name="numThresholds">The number of threshold points.</param>
    /// <returns>Precision and recall arrays.</returns>
    public static (double[] Recall, double[] Precision) ComputePrecisionRecallCurve(
        double[] scores, int[] labels, int numThresholds = 100)
    {
        int count = System.Math.Min(scores.Length, labels.Length);

        int positives = 0;
        for (int i = 0; i < count; i++)
        {
            if (labels[i] == 1) positives++;
        }

        if (positives == 0)
            return (new double[] { 0 }, new double[] { 1 });

        double[] recall = new double[numThresholds + 1];
        double[] precision = new double[numThresholds + 1];

        for (int t = 0; t <= numThresholds; t++)
        {
            double threshold = (double)t / numThresholds;
            int tp = 0, fp = 0, fn = 0;

            for (int i = 0; i < count; i++)
            {
                bool predicted = scores[i] >= threshold;

                if (predicted && labels[i] == 1) tp++;
                else if (predicted && labels[i] == 0) fp++;
                else if (!predicted && labels[i] == 1) fn++;
            }

            recall[t] = tp + fn > 0 ? (double)tp / (tp + fn) : 0;
            precision[t] = tp + fp > 0 ? (double)tp / (tp + fp) : 0;
        }

        return (recall, precision);
    }

    private static double EvaluateSimpleModel(TrainedModel model, double[] features)
    {
        if (model.Weights.Count < features.Length + 1)
            return 0;

        double sum = model.Weights[0];
        for (int i = 0; i < features.Length && i + 1 < model.Weights.Count; i++)
        {
            sum += features[i] * model.Weights[i + 1];
        }

        return 1.0 / (1.0 + System.Math.Exp(-sum));
    }

    private static string GetImportanceColor(double normalizedImportance)
    {
        if (normalizedImportance > 0.75) return "#FF6B6B";
        if (normalizedImportance > 0.5) return "#FFEAA7";
        if (normalizedImportance > 0.25) return "#4ECDC4";
        return "#85C1E9";
    }
}
