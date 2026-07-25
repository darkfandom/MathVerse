namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a data point on the learning curve.</summary>
public sealed record LearningCurvePoint
{
    /// <summary>Training set size.</summary>
    public required double TrainSize { get; init; }

    /// <summary>Mean training score.</summary>
    public required double TrainScore { get; init; }

    /// <summary>Mean validation score.</summary>
    public required double ValidationScore { get; init; }

    /// <summary>Standard deviation of training scores (if available).</summary>
    public double TrainScoreStd { get; init; }

    /// <summary>Standard deviation of validation scores (if available).</summary>
    public double ValidationScoreStd { get; init; }
}

/// <summary>Complete data for learning curve visualization.</summary>
public sealed record LearningCurveData
{
    /// <summary>Learning curve points.</summary>
    public required IReadOnlyList<LearningCurvePoint> Points { get; init; }

    /// <summary>Best training score achieved.</summary>
    public required double BestTrainScore { get; init; }

    /// <summary>Best validation score achieved.</summary>
    public required double BestValidationScore { get; init; }

    /// <summary>Final gap between train and validation (overfitting indicator).</summary>
    public required double FinalGap { get; init; }
}

/// <summary>Visualizes training and validation learning curves over training set sizes.</summary>
public sealed class LearningCurveVisualizer
{
    /// <summary>
    /// Creates a learning curve visualization showing training/validation performance vs training size.
    /// </summary>
    /// <param name="trainSizes">Training set sizes.</param>
    /// <param name="trainScores">Mean training scores at each size.</param>
    /// <param name="valScores">Mean validation scores at each size.</param>
    /// <returns>Learning curve data with metrics.</returns>
    public LearningCurveData Create(double[] trainSizes, double[] trainScores, double[] valScores)
    {
        if (trainSizes == null || trainScores == null || valScores == null)
        {
            return new LearningCurveData
            {
                Points = [],
                BestTrainScore = 0.0,
                BestValidationScore = 0.0,
                FinalGap = 0.0
            };
        }

        int count = System.Math.Min(trainSizes.Length, System.Math.Min(trainScores.Length, valScores.Length));
        var points = new List<LearningCurvePoint>();

        double bestTrain = double.MinValue;
        double bestVal = double.MinValue;

        for (int i = 0; i < count; i++)
        {
            if (trainScores[i] > bestTrain) bestTrain = trainScores[i];
            if (valScores[i] > bestVal) bestVal = valScores[i];

            points.Add(new LearningCurvePoint
            {
                TrainSize = trainSizes[i],
                TrainScore = trainScores[i],
                ValidationScore = valScores[i]
            });
        }

        double finalGap = 0.0;
        if (points.Count > 0)
        {
            var lastPoint = points[^1];
            finalGap = lastPoint.TrainScore - lastPoint.ValidationScore;
        }

        return new LearningCurveData
        {
            Points = points,
            BestTrainScore = bestTrain,
            BestValidationScore = bestVal,
            FinalGap = finalGap
        };
    }
}
