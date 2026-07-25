namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single cell in the confusion matrix.</summary>
public sealed record ConfusionCell
{
    /// <summary>True class index (row).</summary>
    public required int TrueClass { get; init; }

    /// <summary>Predicted class index (column).</summary>
    public required int PredictedClass { get; init; }

    /// <summary>Count of predictions for this cell.</summary>
    public required int Count { get; init; }

    /// <summary>Normalized value (0-1) for color mapping.</summary>
    public required double NormalizedValue { get; init; }

    /// <summary>Label for the true class.</summary>
    public string TrueLabel { get; init; } = "";

    /// <summary>Label for the predicted class.</summary>
    public string PredictedLabel { get; init; } = "";
}

/// <summary>Performance metrics derived from the confusion matrix.</summary>
public sealed record ConfusionMatrixMetrics
{
    /// <summary>Per-class accuracy.</summary>
    public required IReadOnlyList<double> ClassAccuracy { get; init; }

    /// <summary>Overall accuracy.</summary>
    public required double OverallAccuracy { get; init; }

    /// <summary>Per-class precision.</summary>
    public required IReadOnlyList<double> Precision { get; init; }

    /// <summary>Per-class recall.</summary>
    public required IReadOnlyList<double> Recall { get; init; }
}

/// <summary>Complete data for confusion matrix visualization.</summary>
public sealed record ConfusionMatrixData
{
    /// <summary>Matrix cells with values and colors.</summary>
    public required IReadOnlyList<IReadOnlyList<ConfusionCell>> Cells { get; init; }

    /// <summary>Number of classes.</summary>
    public required int ClassCount { get; init; }

    /// <summary>Class labels.</summary>
    public required IReadOnlyList<string> Labels { get; init; }

    /// <summary>Computed metrics.</summary>
    public required ConfusionMatrixMetrics Metrics { get; init; }
}

/// <summary>Visualizes a confusion matrix as a color-coded heatmap with metrics.</summary>
public sealed class ConfusionMatrixVisualizer
{
    /// <summary>
    /// Creates a confusion matrix visualization with color-coded cells and computed metrics.
    /// </summary>
    /// <param name="matrix">Confusion matrix (rows = true, cols = predicted).</param>
    /// <param name="labels">Optional class labels.</param>
    /// <returns>Complete confusion matrix data with colors and metrics.</returns>
    public ConfusionMatrixData Create(int[,] matrix, string[]? labels = null)
    {
        int classes = matrix.GetLength(0);

        string[] classLabels = labels ?? new string[classes];
        for (int i = 0; i < classes; i++)
        {
            if (classLabels.Length <= i || string.IsNullOrEmpty(classLabels[i]))
                classLabels[i] = $"Class {i}";
        }

        int maxVal = 0;
        for (int r = 0; r < classes; r++)
            for (int c = 0; c < classes; c++)
                if (matrix[r, c] > maxVal)
                    maxVal = matrix[r, c];

        var cells = new List<IReadOnlyList<ConfusionCell>>();
        int totalCorrect = 0;
        int totalSamples = 0;

        int[] rowSums = new int[classes];
        int[] colSums = new int[classes];

        for (int r = 0; r < classes; r++)
        {
            var row = new List<ConfusionCell>();
            for (int c = 0; c < classes; c++)
            {
                int count = matrix[r, c];
                totalSamples += count;
                rowSums[r] += count;
                colSums[c] += count;
                if (r == c) totalCorrect += count;

                double normalized = maxVal > 0 ? (double)count / (double)maxVal : 0.0;

                row.Add(new ConfusionCell
                {
                    TrueClass = r,
                    PredictedClass = c,
                    Count = count,
                    NormalizedValue = normalized,
                    TrueLabel = classLabels[r],
                    PredictedLabel = classLabels[c]
                });
            }
            cells.Add(row);
        }

        double overallAccuracy = totalSamples > 0 ? (double)totalCorrect / (double)totalSamples : 0.0;

        var classAccuracy = new List<double>();
        var precision = new List<double>();
        var recall = new List<double>();

        for (int i = 0; i < classes; i++)
        {
            double rec = rowSums[i] > 0 ? (double)matrix[i, i] / (double)rowSums[i] : 0.0;
            double prec = colSums[i] > 0 ? (double)matrix[i, i] / (double)colSums[i] : 0.0;

            recall.Add(rec);
            precision.Add(prec);
            classAccuracy.Add(rec);
        }

        return new ConfusionMatrixData
        {
            Cells = cells,
            ClassCount = classes,
            Labels = classLabels,
            Metrics = new ConfusionMatrixMetrics
            {
                ClassAccuracy = classAccuracy,
                OverallAccuracy = overallAccuracy,
                Precision = precision,
                Recall = recall
            }
        };
    }
}
