namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single point on the loss curve.</summary>
public sealed record LossCurvePoint
{
    /// <summary>Epoch number.</summary>
    public required double Epoch { get; init; }

    /// <summary>Training loss value.</summary>
    public required double TrainLoss { get; init; }

    /// <summary>Validation loss value (if available).</summary>
    public double? ValidationLoss { get; init; }
}

/// <summary>Complete data for loss curve visualization.</summary>
public sealed record LossCurveData
{
    /// <summary>Loss curve points.</summary>
    public required IReadOnlyList<LossCurvePoint> Points { get; init; }

    /// <summary>Minimum training loss achieved.</summary>
    public required double MinTrainLoss { get; init; }

    /// <summary>Minimum validation loss achieved (if available).</summary>
    public double? MinValidationLoss { get; init; }

    /// <summary>Epoch at which minimum validation loss occurred.</summary>
    public double? BestEpoch { get; init; }

    /// <summary>Whether the model appears to be overfitting.</summary>
    public required bool IsOverfitting { get; init; }
}

/// <summary>Visualizes training and validation loss over epochs.</summary>
public sealed class LossCurveVisualizer
{
    /// <summary>
    /// Creates a loss curve visualization showing train/validation loss over training epochs.
    /// </summary>
    /// <param name="epochs">Epoch numbers.</param>
    /// <param name="trainLoss">Training loss values.</param>
    /// <param name="valLoss">Optional validation loss values.</param>
    /// <returns>Loss curve data with minimum loss and overfitting detection.</returns>
    public LossCurveData Create(double[] epochs, double[] trainLoss, double[]? valLoss = null)
    {
        if (epochs == null || trainLoss == null)
        {
            return new LossCurveData
            {
                Points = [],
                MinTrainLoss = 0.0,
                MinValidationLoss = null,
                BestEpoch = null,
                IsOverfitting = false
            };
        }

        int count = System.Math.Min(epochs.Length, trainLoss.Length);
        var points = new List<LossCurvePoint>();
        double minTrainLoss = double.MaxValue;
        double minValLoss = double.MaxValue;
        double bestEpoch = 0.0;
        bool hasVal = valLoss != null && valLoss.Length >= count;

        for (int i = 0; i < count; i++)
        {
            if (trainLoss[i] < minTrainLoss) minTrainLoss = trainLoss[i];

            double? vl = hasVal ? valLoss![i] : null;

            if (vl.HasValue && vl.Value < minValLoss)
            {
                minValLoss = vl.Value;
                bestEpoch = epochs[i];
            }

            points.Add(new LossCurvePoint
            {
                Epoch = epochs[i],
                TrainLoss = trainLoss[i],
                ValidationLoss = vl
            });
        }

        bool overfitting = false;
        if (hasVal && points.Count > 10)
        {
            int lastTen = points.Count - 10;
            double earlyValMean = 0.0;
            double lateValMean = 0.0;
            for (int i = lastTen; i < lastTen + 5; i++)
                earlyValMean += points[i].ValidationLoss!.Value;
            for (int i = points.Count - 5; i < points.Count; i++)
                lateValMean += points[i].ValidationLoss!.Value;
            earlyValMean /= 5.0;
            lateValMean /= 5.0;

            double earlyTrainMean = 0.0;
            double lateTrainMean = 0.0;
            for (int i = lastTen; i < lastTen + 5; i++)
                earlyTrainMean += points[i].TrainLoss;
            for (int i = points.Count - 5; i < points.Count; i++)
                lateTrainMean += points[i].TrainLoss;
            earlyTrainMean /= 5.0;
            lateTrainMean /= 5.0;

            overfitting = lateTrainMean < earlyTrainMean - 0.001 && lateValMean > earlyValMean + 0.001;
        }

        return new LossCurveData
        {
            Points = points,
            MinTrainLoss = minTrainLoss,
            MinValidationLoss = hasVal ? minValLoss : null,
            BestEpoch = hasVal ? bestEpoch : null,
            IsOverfitting = overfitting
        };
    }
}
