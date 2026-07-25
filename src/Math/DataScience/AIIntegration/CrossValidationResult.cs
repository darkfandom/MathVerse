namespace MathVerse.Math.DataScience.AIIntegration;

using System;

/// <summary>
/// Represents the result of cross-validation evaluation.
/// </summary>
public sealed class CrossValidationResult
{
    /// <summary>
    /// Gets or sets the mean score across all folds.
    /// </summary>
    public double MeanScore { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation of scores across all folds.
    /// </summary>
    public double StdScore { get; set; }

    /// <summary>
    /// Gets or sets the individual fold scores.
    /// </summary>
    public double[] FoldScores { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the name of the scoring metric used.
    /// </summary>
    public string Metric { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum score across all folds.
    /// </summary>
    public double MinScore { get; set; }

    /// <summary>
    /// Gets or sets the maximum score across all folds.
    /// </summary>
    public double MaxScore { get; set; }

    /// <summary>
    /// Creates a new <see cref="CrossValidationResult"/> from fold scores.
    /// </summary>
    /// <param name="scores">The array of fold scores.</param>
    /// <param name="metric">The metric name.</param>
    /// <returns>A new cross-validation result with computed statistics.</returns>
    public static CrossValidationResult Create(double[] scores, string metric)
    {
        if (scores is null || scores.Length == 0)
            throw new ArgumentException("Scores cannot be null or empty.", nameof(scores));

        double mean = 0.0;
        foreach (double s in scores) mean += s;
        mean /= scores.Length;

        double variance = 0.0;
        double min = double.MaxValue;
        double max = double.MinValue;
        foreach (double s in scores)
        {
            double d = s - mean;
            variance += d * d;
            if (s < min) min = s;
            if (s > max) max = s;
        }

        double stdDev = scores.Length > 1 ? System.Math.Sqrt(variance / (scores.Length - 1)) : 0.0;

        return new CrossValidationResult
        {
            MeanScore = mean,
            StdScore = stdDev,
            FoldScores = scores,
            Metric = metric,
            MinScore = min,
            MaxScore = max
        };
    }

    /// <summary>
    /// Returns a string summary of the cross-validation result.
    /// </summary>
    /// <returns>A summary string.</returns>
    public override string ToString()
    {
        return $"{Metric}: {MeanScore:G4} +/- {StdScore:G4} (min={MinScore:G4}, max={MaxScore:G4}, folds={FoldScores.Length})";
    }
}
