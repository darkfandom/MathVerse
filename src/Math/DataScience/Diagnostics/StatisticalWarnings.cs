namespace MathVerse.Math.DataScience.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Detects statistical issues and anomalies in data arrays.
/// </summary>
public sealed class StatisticalWarnings
{
    /// <summary>
    /// Represents a single statistical warning.
    /// </summary>
    public sealed class Warning
    {
        /// <summary>
        /// Gets or sets the category of the warning (e.g., "Skewness", "Outliers", "Normality").
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the descriptive message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity level.
        /// </summary>
        public WarningSeverity Severity { get; set; }
    }

    /// <summary>
    /// Severity levels for statistical warnings.
    /// </summary>
    public enum WarningSeverity
    {
        /// <summary>Informational notice.</summary>
        Info,

        /// <summary>Potential issue requiring attention.</summary>
        Warning,

        /// <summary>Critical issue that may invalidate results.</summary>
        Error
    }

    /// <summary>
    /// Analyzes a data array for common statistical issues.
    /// </summary>
    /// <param name="data">The data values to check.</param>
    /// <returns>A list of <see cref="Warning"/> instances describing detected issues.</returns>
    public static List<Warning> Check(double[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));

        List<Warning> warnings = new();

        if (data.Length == 0)
        {
            warnings.Add(new Warning
            {
                Category = "Empty",
                Message = "Data array is empty.",
                Severity = WarningSeverity.Error
            });
            return warnings;
        }

        if (data.Length < 3)
        {
            warnings.Add(new Warning
            {
                Category = "SampleSize",
                Message = $"Sample size ({data.Length}) is too small for reliable statistical analysis.",
                Severity = WarningSeverity.Warning
            });
            return warnings;
        }

        CheckSkewness(data, warnings);
        CheckKurtosis(data, warnings);
        CheckOutliers(data, warnings);
        CheckNormality(data, warnings);
        CheckConstant(data, warnings);

        return warnings;
    }

    /// <summary>
    /// Computes the skewness of a data array.
    /// </summary>
    /// <param name="data">The data array.</param>
    /// <returns>The Pearson skewness coefficient.</returns>
    public static double Skewness(double[] data)
    {
        if (data is null || data.Length < 3) return 0.0;

        int n = data.Length;
        double sum = 0.0;
        for (int i = 0; i < n; i++) sum += data[i];
        double mean = sum / n;

        double m2 = 0.0;
        double m3 = 0.0;
        for (int i = 0; i < n; i++)
        {
            double diff = data[i] - mean;
            m2 += diff * diff;
            m3 += diff * diff * diff;
        }
        double variance = m2 / n;
        double stdDev = System.Math.Sqrt(variance);

        if (stdDev < 1e-15) return 0.0;
        double skew = (m3 / n) / (stdDev * stdDev * stdDev);
        return skew;
    }

    /// <summary>
    /// Computes the excess kurtosis of a data array.
    /// </summary>
    /// <param name="data">The data array.</param>
    /// <returns>The excess kurtosis (0 for normal distribution).</returns>
    public static double Kurtosis(double[] data)
    {
        if (data is null || data.Length < 4) return 0.0;

        int n = data.Length;
        double sum = 0.0;
        for (int i = 0; i < n; i++) sum += data[i];
        double mean = sum / n;

        double m2 = 0.0;
        double m4 = 0.0;
        for (int i = 0; i < n; i++)
        {
            double diff = data[i] - mean;
            double diff2 = diff * diff;
            m2 += diff2;
            m4 += diff2 * diff2;
        }
        double variance = m2 / n;
        double stdDev = System.Math.Sqrt(variance);

        if (stdDev < 1e-15) return 0.0;
        double kurt = (m4 / n) / (variance * variance) - 3.0;
        return kurt;
    }

    private static void CheckSkewness(double[] data, List<Warning> warnings)
    {
        double skew = Skewness(data);
        double absSkew = System.Math.Abs(skew);

        if (absSkew > 2.0)
        {
            warnings.Add(new Warning
            {
                Category = "Skewness",
                Message = $"High skewness detected ({skew:F3}). Data is strongly {(skew > 0 ? "right" : "left")}-skewed.",
                Severity = WarningSeverity.Warning
            });
        }
        else if (absSkew > 1.0)
        {
            warnings.Add(new Warning
            {
                Category = "Skewness",
                Message = $"Moderate skewness detected ({skew:F3}).",
                Severity = WarningSeverity.Info
            });
        }
    }

    private static void CheckKurtosis(double[] data, List<Warning> warnings)
    {
        double kurt = Kurtosis(data);
        double absKurt = System.Math.Abs(kurt);

        if (absKurt > 7.0)
        {
            warnings.Add(new Warning
            {
                Category = "Kurtosis",
                Message = $"Extreme kurtosis detected ({kurt:F3}). Distribution has heavy tails or is very peaked.",
                Severity = WarningSeverity.Warning
            });
        }
        else if (absKurt > 3.0)
        {
            warnings.Add(new Warning
            {
                Category = "Kurtosis",
                Message = $"Elevated kurtosis detected ({kurt:F3}).",
                Severity = WarningSeverity.Info
            });
        }
    }

    private static void CheckOutliers(double[] data, List<Warning> warnings)
    {
        if (data.Length < 4) return;

        double[] sorted = (double[])data.Clone();
        System.Array.Sort(sorted);

        double q1 = Percentile(sorted, 25.0);
        double q3 = Percentile(sorted, 75.0);
        double iqr = q3 - q1;

        double lowerBound = q1 - 1.5 * iqr;
        double upperBound = q3 + 1.5 * iqr;

        int outlierCount = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] < lowerBound || data[i] > upperBound)
            {
                outlierCount++;
            }
        }

        if (outlierCount > 0)
        {
            double pct = (double)outlierCount / data.Length * 100.0;
            warnings.Add(new Warning
            {
                Category = "Outliers",
                Message = $"{outlierCount} outlier(s) detected using IQR method ({pct:F1}% of data).",
                Severity = pct > 5.0 ? WarningSeverity.Warning : WarningSeverity.Info
            });
        }
    }

    private static void CheckNormality(double[] data, List<Warning> warnings)
    {
        if (data.Length < 8) return;

        double skew = Skewness(data);
        double kurt = Kurtosis(data);

        double skewPct = System.Math.Abs(skew) / System.Math.Sqrt(6.0 / data.Length);
        double kurtPct = System.Math.Abs(kurt) / System.Math.Sqrt(24.0 / data.Length);

        if (skewPct > 2.0 || kurtPct > 2.0)
        {
            warnings.Add(new Warning
            {
                Category = "Normality",
                Message = $"Data may not be normally distributed (skewness z-score: {skewPct:F2}, kurtosis z-score: {kurtPct:F2}).",
                Severity = WarningSeverity.Info
            });
        }
    }

    private static void CheckConstant(double[] data, List<Warning> warnings)
    {
        if (data.Length < 2) return;

        double first = data[0];
        bool allSame = true;
        for (int i = 1; i < data.Length; i++)
        {
            if (System.Math.Abs(data[i] - first) > 1e-15)
            {
                allSame = false;
                break;
            }
        }

        if (allSame)
        {
            warnings.Add(new Warning
            {
                Category = "Constant",
                Message = "All values are identical. Variance is zero.",
                Severity = WarningSeverity.Warning
            });
        }
    }

    private static double Percentile(double[] sortedValues, double percentile)
    {
        double index = (percentile / 100.0) * (sortedValues.Length - 1);
        int lower = (int)System.Math.Floor(index);
        int upper = (int)System.Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        double fraction = index - lower;
        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }
}
