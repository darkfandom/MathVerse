namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single point on the ROC curve.</summary>
public sealed record ROCPoint
{
    /// <summary>False Positive Rate at this threshold.</summary>
    public required double FPR { get; init; }

    /// <summary>True Positive Rate (recall) at this threshold.</summary>
    public required double TPR { get; init; }

    /// <summary>The threshold value.</summary>
    public required double Threshold { get; init; }
}

/// <summary>Performance metrics from ROC analysis.</summary>
public sealed record ROCCurveMetrics
{
    /// <summary>Area Under the Curve (AUC).</summary>
    public required double AUC { get; init; }

    /// <summary>Optimal threshold (maximizes TPR - FPR).</summary>
    public required double OptimalThreshold { get; init; }

    /// <summary>TPR at the optimal threshold.</summary>
    public required double OptimalTPR { get; init; }

    /// <summary>FPR at the optimal threshold.</summary>
    public required double OptimalFPR { get; init; }
}

/// <summary>Complete data for ROC curve visualization.</summary>
public sealed record ROCCurveData
{
    /// <summary>Points along the ROC curve.</summary>
    public required IReadOnlyList<ROCPoint> Points { get; init; }

    /// <summary>Computed ROC metrics.</summary>
    public required ROCCurveMetrics Metrics { get; init; }
}

/// <summary>Visualizes ROC curves by computing TPR/FPR at various thresholds.</summary>
public sealed class ROCCurveVisualizer
{
    /// <summary>
    /// Creates a ROC curve from true labels and predicted scores.
    /// </summary>
    /// <param name="trueLabels">True binary labels (0 or 1).</param>
    /// <param name="predictedScores">Predicted scores/probabilities.</param>
    /// <returns>ROC curve points and AUC metrics.</returns>
    public ROCCurveData Create(double[] trueLabels, double[] predictedScores)
    {
        if (trueLabels == null || predictedScores == null ||
            trueLabels.Length == 0 || predictedScores.Length == 0 ||
            trueLabels.Length != predictedScores.Length)
        {
            return new ROCCurveData
            {
                Points = [],
                Metrics = new ROCCurveMetrics
                {
                    AUC = 0.0,
                    OptimalThreshold = 0.0,
                    OptimalTPR = 0.0,
                    OptimalFPR = 0.0
                }
            };
        }

        int n = trueLabels.Length;
        int positiveCount = 0;
        int negativeCount = 0;

        for (int i = 0; i < n; i++)
        {
            if (trueLabels[i] > 0.5) positiveCount++;
            else negativeCount++;
        }

        var sortedIndices = new int[n];
        for (int i = 0; i < n; i++)
            sortedIndices[i] = i;

        System.Array.Sort(sortedIndices, (a, b) => predictedScores[b].CompareTo(predictedScores[a]));

        var points = new List<ROCPoint>();
        points.Add(new ROCPoint { FPR = 0.0, TPR = 0.0, Threshold = predictedScores[sortedIndices[0]] + 1.0 });

        int tp = 0;
        int fp = 0;
        double prevScore = double.MaxValue;

        for (int i = 0; i < n; i++)
        {
            int idx = sortedIndices[i];

            if (predictedScores[idx] < prevScore)
            {
                double fpr = negativeCount > 0 ? (double)fp / (double)negativeCount : 0.0;
                double tpr = positiveCount > 0 ? (double)tp / (double)positiveCount : 0.0;
                points.Add(new ROCPoint { FPR = fpr, TPR = tpr, Threshold = predictedScores[idx] });
                prevScore = predictedScores[idx];
            }

            if (trueLabels[idx] > 0.5) tp++;
            else fp++;
        }

        double finalFpr = negativeCount > 0 ? (double)fp / (double)negativeCount : 0.0;
        double finalTpr = positiveCount > 0 ? (double)tp / (double)positiveCount : 0.0;
        points.Add(new ROCPoint { FPR = finalFpr, TPR = finalTpr, Threshold = double.MinValue });

        double auc = 0.0;
        for (int i = 1; i < points.Count; i++)
        {
            double dFpr = points[i].FPR - points[i - 1].FPR;
            double avgTpr = (points[i].TPR + points[i - 1].TPR) * 0.5;
            auc += dFpr * avgTpr;
        }

        double bestDiff = double.MinValue;
        double bestThreshold = 0.0;
        double bestTpr = 0.0;
        double bestFpr = 0.0;

        for (int i = 0; i < points.Count; i++)
        {
            double diff = points[i].TPR - points[i].FPR;
            if (diff > bestDiff)
            {
                bestDiff = diff;
                bestThreshold = points[i].Threshold;
                bestTpr = points[i].TPR;
                bestFpr = points[i].FPR;
            }
        }

        return new ROCCurveData
        {
            Points = points,
            Metrics = new ROCCurveMetrics
            {
                AUC = auc,
                OptimalThreshold = bestThreshold,
                OptimalTPR = bestTpr,
                OptimalFPR = bestFpr
            }
        };
    }
}
