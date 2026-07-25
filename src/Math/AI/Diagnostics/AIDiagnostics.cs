namespace MathVerse.Math.AI.Diagnostics;

using System.Collections.Concurrent;
using System.Collections.Immutable;

/// <summary>Comprehensive diagnostics for AI operations including convergence, training, and prediction analysis.</summary>
public sealed class AIDiagnostics
{
    private readonly ConcurrentBag<DiagnosticEntry> _entries = [];
    private readonly object _lock = new();

    /// <summary>Records a diagnostic message with the specified category and severity.</summary>
    /// <param name="category">The diagnostic category (e.g., "Training", "Inference", "Optimization").</param>
    /// <param name="message">The diagnostic message.</param>
    /// <param name="severity">The severity level.</param>
    public void Record(string category, string message, DiagnosticSeverity severity = DiagnosticSeverity.Info)
    {
        _entries.Add(new DiagnosticEntry
        {
            Timestamp = DateTime.UtcNow,
            Category = category,
            Message = message,
            Severity = severity
        });
    }

    /// <summary>Records a training step with epoch, loss, and learning rate.</summary>
    /// <param name="epoch">The current training epoch number.</param>
    /// <param name="loss">The loss value at this epoch.</param>
    /// <param name="learningRate">The learning rate used at this epoch.</param>
    public void RecordTraining(int epoch, double loss, double learningRate)
    {
        Record("Training", $"Epoch {epoch}: loss={loss:E6}, lr={learningRate:E4}", DiagnosticSeverity.Info);
    }

    /// <summary>Records a prediction comparison between predicted and actual values.</summary>
    /// <param name="predicted">The predicted output array.</param>
    /// <param name="actual">The actual target array.</param>
    public void RecordPrediction(double[] predicted, double[] actual)
    {
        if (predicted == null || actual == null || predicted.Length == 0 || actual.Length == 0)
        {
            Record("Prediction", "Empty or null prediction/actual arrays.", DiagnosticSeverity.Warning);
            return;
        }

        int len = System.Math.Min(predicted.Length, actual.Length);
        double sumAbsError = 0.0;
        double maxErr = 0.0;

        for (int i = 0; i < len; i++)
        {
            double err = System.Math.Abs(predicted[i] - actual[i]);
            sumAbsError += err;
            if (err > maxErr) maxErr = err;
        }

        double mae = sumAbsError / len;
        Record("Prediction", $"Prediction recorded: MAE={mae:E6}, MaxError={maxErr:E6}, Samples={len}", DiagnosticSeverity.Info);
    }

    /// <summary>Retrieves all diagnostic entries, optionally filtered by minimum severity.</summary>
    /// <param name="minSeverity">The minimum severity to include, or null for all entries.</param>
    /// <returns>A list of matching diagnostic entries sorted by timestamp.</returns>
    public List<DiagnosticEntry> GetEntries(DiagnosticSeverity? minSeverity = null)
    {
        var entries = new List<DiagnosticEntry>(_entries);

        if (minSeverity.HasValue)
        {
            int minVal = (int)minSeverity.Value;
            entries.RemoveAll(e => (int)e.Severity < minVal);
        }

        entries.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        return entries;
    }

    /// <summary>Analyzes a loss history to determine convergence characteristics.</summary>
    /// <param name="lossHistory">The sequence of loss values during training.</param>
    /// <returns>A <see cref="ConvergenceDiagnostics"/> with convergence analysis.</returns>
    public ConvergenceDiagnostics AnalyzeConvergence(double[] lossHistory)
    {
        if (lossHistory == null || lossHistory.Length < 2)
        {
            return new ConvergenceDiagnostics
            {
                Converged = false,
                ConvergenceEpoch = -1,
                FinalLoss = lossHistory?.Length > 0 ? lossHistory[0] : 0.0,
                LossReduction = 0.0,
                Oscillating = false,
                Assessment = "Insufficient loss history for convergence analysis."
            };
        }

        int len = lossHistory.Length;
        double initialLoss = lossHistory[0];
        double finalLoss = lossHistory[len - 1];
        double lossReduction = initialLoss > 1e-15 ? (initialLoss - finalLoss) / initialLoss : 0.0;

        int convergenceEpoch = -1;
        bool converged = false;
        double threshold = initialLoss * 0.01;

        for (int i = len - 1; i >= 1; i--)
        {
            if (System.Math.Abs(lossHistory[i] - lossHistory[i - 1]) < threshold && lossHistory[i] < initialLoss * 0.1)
            {
                convergenceEpoch = i;
                converged = true;
            }
            else
            {
                break;
            }
        }

        bool oscillating = DetectOscillation(lossHistory);

        string assessment;
        if (converged && !oscillating)
        {
            assessment = $"Converged at epoch {convergenceEpoch}. Loss reduced by {lossReduction * 100:F1}%.";
        }
        else if (converged && oscillating)
        {
            assessment = $"Converged at epoch {convergenceEpoch} but with oscillations. Consider reducing learning rate.";
        }
        else if (oscillating)
        {
            assessment = "Oscillating without convergence. Learning rate may be too high.";
        }
        else if (lossReduction < 0.01)
        {
            assessment = "Minimal loss reduction. Training may be stuck in a local minimum or learning rate is too low.";
        }
        else
        {
            assessment = $"Still converging. Loss reduced by {lossReduction * 100:F1}% after {len} epochs.";
        }

        return new ConvergenceDiagnostics
        {
            Converged = converged,
            ConvergenceEpoch = convergenceEpoch,
            FinalLoss = finalLoss,
            LossReduction = lossReduction,
            Oscillating = oscillating,
            Assessment = assessment
        };
    }

    /// <summary>Analyzes training performance from loss history and elapsed time.</summary>
    /// <param name="lossHistory">The sequence of loss values.</param>
    /// <param name="elapsed">Total training elapsed time.</param>
    /// <returns>A <see cref="TrainingDiagnostics"/> with training analysis.</returns>
    public TrainingDiagnostics AnalyzeTraining(double[] lossHistory, TimeSpan elapsed)
    {
        if (lossHistory == null || lossHistory.Length == 0)
        {
            return new TrainingDiagnostics
            {
                AverageLoss = 0.0,
                LossStdDev = 0.0,
                Overfitting = false,
                TrainingSpeed = 0.0,
                Assessment = "No training data available."
            };
        }

        int len = lossHistory.Length;
        double sum = 0.0;
        double minLoss = lossHistory[0];
        double maxLoss = lossHistory[0];

        for (int i = 0; i < len; i++)
        {
            sum += lossHistory[i];
            if (lossHistory[i] < minLoss) minLoss = lossHistory[i];
            if (lossHistory[i] > maxLoss) maxLoss = lossHistory[i];
        }

        double avgLoss = sum / len;

        double varianceSum = 0.0;
        for (int i = 0; i < len; i++)
        {
            double diff = lossHistory[i] - avgLoss;
            varianceSum += diff * diff;
        }
        double stdDev = System.Math.Sqrt(varianceSum / len);

        bool overfitting = false;
        if (len >= 10)
        {
            int quarter = len / 4;
            double firstQuarterAvg = 0.0;
            double lastQuarterAvg = 0.0;

            for (int i = 0; i < quarter; i++) firstQuarterAvg += lossHistory[i];
            for (int i = len - quarter; i < len; i++) lastQuarterAvg += lossHistory[i];

            firstQuarterAvg /= quarter;
            lastQuarterAvg /= quarter;

            if (lastQuarterAvg > firstQuarterAvg * 1.1)
            {
                overfitting = true;
            }
        }

        double epochsPerSecond = elapsed.TotalSeconds > 0 ? len / elapsed.TotalSeconds : 0.0;

        string assessment;
        if (overfitting)
        {
            assessment = $"Possible overfitting detected: loss increased in later epochs. Avg={avgLoss:E4}, StdDev={stdDev:E4}.";
        }
        else if (stdDev / System.Math.Max(System.Math.Abs(avgLoss), 1e-15) < 0.05)
        {
            assessment = $"Stable training: low variance in loss (StdDev/Avg={stdDev / System.Math.Max(System.Math.Abs(avgLoss), 1e-15):F4}).";
        }
        else
        {
            assessment = $"Training with moderate variance. Avg={avgLoss:E4}, Range=[{minLoss:E4}, {maxLoss:E4}].";
        }

        return new TrainingDiagnostics
        {
            AverageLoss = avgLoss,
            LossStdDev = stdDev,
            Overfitting = overfitting,
            TrainingSpeed = epochsPerSecond,
            Assessment = assessment
        };
    }

    /// <summary>Analyzes prediction quality by comparing predicted and actual arrays.</summary>
    /// <param name="predicted">The predicted values.</param>
    /// <param name="actual">The actual target values.</param>
    /// <returns>A <see cref="PredictionDiagnostics"/> with prediction quality metrics.</returns>
    public PredictionDiagnostics AnalyzePrediction(double[] predicted, double[] actual)
    {
        if (predicted == null || actual == null || predicted.Length == 0 || actual.Length == 0)
        {
            return new PredictionDiagnostics
            {
                MAE = 0.0,
                RMSE = 0.0,
                R2 = 0.0,
                MaxError = 0.0,
                MeanAbsolutePercentageError = 0.0,
                Assessment = "Empty or null arrays provided."
            };
        }

        int len = System.Math.Min(predicted.Length, actual.Length);
        double sumAbsError = 0.0;
        double sumSquaredError = 0.0;
        double maxErr = 0.0;
        double sumAbsPctError = 0.0;
        int pctCount = 0;

        double actualMean = 0.0;
        for (int i = 0; i < len; i++) actualMean += actual[i];
        actualMean /= len;

        double ssTot = 0.0;
        for (int i = 0; i < len; i++)
        {
            double err = predicted[i] - actual[i];
            double absErr = System.Math.Abs(err);
            sumAbsError += absErr;
            sumSquaredError += err * err;
            if (absErr > maxErr) maxErr = absErr;

            if (System.Math.Abs(actual[i]) > 1e-15)
            {
                sumAbsPctError += absErr / System.Math.Abs(actual[i]);
                pctCount++;
            }

            double dev = actual[i] - actualMean;
            ssTot += dev * dev;
        }

        double mae = sumAbsError / len;
        double rmse = System.Math.Sqrt(sumSquaredError / len);
        double r2 = ssTot > 1e-15 ? 1.0 - (sumSquaredError / ssTot) : 0.0;
        double mape = pctCount > 0 ? (sumAbsPctError / pctCount) * 100.0 : 0.0;

        string assessment;
        if (r2 > 0.95)
        {
            assessment = $"Excellent prediction quality. R²={r2:F4}, MAE={mae:E4}.";
        }
        else if (r2 > 0.8)
        {
            assessment = $"Good prediction quality. R²={r2:F4}, MAE={mae:E4}.";
        }
        else if (r2 > 0.5)
        {
            assessment = $"Moderate prediction quality. R²={r2:F4}. Consider model improvements.";
        }
        else
        {
            assessment = $"Poor prediction quality. R²={r2:F4}, RMSE={rmse:E4}. Model may need redesign.";
        }

        return new PredictionDiagnostics
        {
            MAE = mae,
            RMSE = rmse,
            R2 = r2,
            MaxError = maxErr,
            MeanAbsolutePercentageError = mape,
            Assessment = assessment
        };
    }

    /// <summary>Generates an explainability report summarizing feature importance and model behavior.</summary>
    /// <param name="modelType">The type of model being explained.</param>
    /// <param name="features">Feature names mapped to their importance or magnitude values.</param>
    /// <returns>An <see cref="ExplainabilityReport"/> with feature importance ranking and insights.</returns>
    public ExplainabilityReport GenerateExplainabilityReport(string modelType, ImmutableDictionary<string, double> features)
    {
        var importanceBuilder = ImmutableDictionary<string, double>.Empty.ToBuilder();
        double totalMagnitude = 0.0;

        foreach (var kvp in features)
        {
            double absVal = System.Math.Abs(kvp.Value);
            totalMagnitude += absVal;
            importanceBuilder[kvp.Key] = absVal;
        }

        if (totalMagnitude > 1e-15)
        {
            foreach (var key in importanceBuilder.Keys.ToList())
            {
                importanceBuilder[key] = importanceBuilder[key] / totalMagnitude;
            }
        }

        var sorted = features.OrderByDescending(kvp => System.Math.Abs(kvp.Value)).ToList();

        var insights = new List<string>();

        if (sorted.Count > 0)
        {
            insights.Add($"Most influential feature: '{sorted[0].Key}' (magnitude={sorted[0].Value:E4}).");
        }

        if (sorted.Count > 1)
        {
            insights.Add($"Second most influential: '{sorted[1].Key}' (magnitude={sorted[1].Value:E4}).");
        }

        int dominantCount = 0;
        foreach (var kvp in sorted)
        {
            if (totalMagnitude > 1e-15 && System.Math.Abs(kvp.Value) / totalMagnitude > 0.5)
            {
                dominantCount++;
            }
        }

        if (dominantCount == 1)
        {
            insights.Add("Single dominant feature; model may be oversimplified.");
        }
        else if (dominantCount == 0 && sorted.Count > 0)
        {
            insights.Add("No single dominant feature; balanced feature usage.");
        }

        string confidenceLevel;
        if (sorted.Count > 0 && totalMagnitude > 1e-15)
        {
            double topRatio = System.Math.Abs(sorted[0].Value) / totalMagnitude;
            confidenceLevel = topRatio switch
            {
                > 0.7 => "High (strong single feature dominance)",
                > 0.4 => "Medium (distributed importance)",
                _ => "Low (uniform importance, possible noise sensitivity)"
            };
        }
        else
        {
            confidenceLevel = "N/A (no features)";
        }

        return new ExplainabilityReport
        {
            ModelType = modelType,
            FeatureImportance = importanceBuilder.ToImmutable(),
            ConfidenceLevel = confidenceLevel,
            KeyInsights = insights
        };
    }

    /// <summary>Clears all recorded diagnostic entries.</summary>
    public void Clear()
    {
        while (_entries.TryTake(out _)) { }
    }

    /// <summary>Detects oscillation in a loss sequence by counting sign changes in consecutive differences.</summary>
    /// <param name="lossHistory">The loss history array.</param>
    /// <returns>true if oscillation is detected; otherwise, false.</returns>
    private static bool DetectOscillation(double[] lossHistory)
    {
        if (lossHistory.Length < 4) return false;

        int signChanges = 0;
        double lastDiff = lossHistory[1] - lossHistory[0];

        for (int i = 2; i < lossHistory.Length; i++)
        {
            double diff = lossHistory[i] - lossHistory[i - 1];
            if ((lastDiff > 1e-15 && diff < -1e-15) || (lastDiff < -1e-15 && diff > 1e-15))
            {
                signChanges++;
            }
            lastDiff = diff;
        }

        return signChanges > lossHistory.Length / 3;
    }
}

/// <summary>Severity level for diagnostic entries.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Detailed diagnostic information for debugging.</summary>
    Debug,

    /// <summary>General informational diagnostic messages.</summary>
    Info,

    /// <summary>Warning messages indicating potential issues.</summary>
    Warning,

    /// <summary>Error messages indicating failures.</summary>
    Error,

    /// <summary>Critical messages indicating severe problems.</summary>
    Critical
}

/// <summary>Represents a single diagnostic entry with timestamp, category, message, and severity.</summary>
public sealed class DiagnosticEntry
{
    /// <summary>Gets the UTC timestamp when the entry was recorded.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Gets the diagnostic category.</summary>
    public string Category { get; init; } = "";

    /// <summary>Gets the diagnostic message.</summary>
    public string Message { get; init; } = "";

    /// <summary>Gets the severity level.</summary>
    public DiagnosticSeverity Severity { get; init; }
}

/// <summary>Results of convergence analysis on a loss history.</summary>
public sealed class ConvergenceDiagnostics
{
    /// <summary>Gets whether the training converged.</summary>
    public bool Converged { get; init; }

    /// <summary>Gets the epoch at which convergence was detected, or -1 if not converged.</summary>
    public int ConvergenceEpoch { get; init; }

    /// <summary>Gets the final loss value.</summary>
    public double FinalLoss { get; init; }

    /// <summary>Gets the fractional loss reduction from initial to final.</summary>
    public double LossReduction { get; init; }

    /// <summary>Gets whether the loss sequence exhibits oscillation.</summary>
    public bool Oscillating { get; init; }

    /// <summary>Gets a human-readable convergence assessment.</summary>
    public string Assessment { get; init; } = "";
}

/// <summary>Results of training performance analysis.</summary>
public sealed class TrainingDiagnostics
{
    /// <summary>Gets the average loss across all epochs.</summary>
    public double AverageLoss { get; init; }

    /// <summary>Gets the standard deviation of the loss values.</summary>
    public double LossStdDev { get; init; }

    /// <summary>Gets whether overfitting is suspected.</summary>
    public bool Overfitting { get; init; }

    /// <summary>Gets the training speed in epochs per second.</summary>
    public double TrainingSpeed { get; init; }

    /// <summary>Gets a human-readable training assessment.</summary>
    public string Assessment { get; init; } = "";
}

/// <summary>Results of prediction quality analysis.</summary>
public sealed class PredictionDiagnostics
{
    /// <summary>Gets the mean absolute error.</summary>
    public double MAE { get; init; }

    /// <summary>Gets the root mean square error.</summary>
    public double RMSE { get; init; }

    /// <summary>Gets the R-squared (coefficient of determination) value.</summary>
    public double R2 { get; init; }

    /// <summary>Gets the maximum absolute error.</summary>
    public double MaxError { get; init; }

    /// <summary>Gets the mean absolute percentage error.</summary>
    public double MeanAbsolutePercentageError { get; init; }

    /// <summary>Gets a human-readable prediction quality assessment.</summary>
    public string Assessment { get; init; } = "";
}

/// <summary>Report summarizing model explainability with feature importance.</summary>
public sealed class ExplainabilityReport
{
    /// <summary>Gets the model type being explained.</summary>
    public string ModelType { get; init; } = "";

    /// <summary>Gets feature importance scores normalized to sum to 1.</summary>
    public ImmutableDictionary<string, double> FeatureImportance { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Gets the confidence level of the explanation.</summary>
    public string ConfidenceLevel { get; init; } = "";

    /// <summary>Gets key insights extracted from the analysis.</summary>
    public List<string> KeyInsights { get; init; } = [];
}
