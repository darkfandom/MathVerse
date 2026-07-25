namespace MathVerse.Math.DataScience.Diagnostics;

using System;
using System.Collections.Generic;
using Core;

/// <summary>
/// Performs comprehensive data quality diagnostics on a dataset.
/// </summary>
public sealed class DataDiagnostics
{
    /// <summary>
    /// Analyzes the dataset and produces a comprehensive quality report.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <returns>A <see cref="DataQualityReport"/> with quality scores and detected issues.</returns>
    public static DataQualityReport Analyze(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        DataQualityReport report = DataQualityReport.Create(ds.Name);
        report.RowCount = ds.Count;
        report.ColumnCount = ds.Schema.Columns.Count;

        if (ds.Count == 0 || ds.Schema.Columns.Count == 0)
        {
            report.Issues.Add("Dataset is empty.");
            report.OverallScore = 0.0;
            return report;
        }

        double completeness = ComputeCompleteness(ds, report);
        double consistency = ComputeConsistency(ds, report);
        double accuracy = ComputeAccuracy(ds, report);
        double timeliness = ComputeTimeliness(ds, report);

        report.CompletenessScore = completeness;
        report.ConsistencyScore = consistency;
        report.AccuracyScore = accuracy;
        report.TimelinessScore = timeliness;

        report.OverallScore = (completeness * 0.35) + (consistency * 0.25) + (accuracy * 0.25) + (timeliness * 0.15);

        return report;
    }

    private static double ComputeCompleteness(Dataset ds, DataQualityReport report)
    {
        int totalCells = 0;
        int nonNullCells = 0;

        for (int c = 0; c < ds.Schema.Columns.Count; c++)
        {
            string col = ds.Schema.Columns[c].Name;
            int colNonNull = 0;

            for (int r = 0; r < ds.Count; r++)
            {
                totalCells++;
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null)
                {
                    nonNullCells++;
                    colNonNull++;
                }
            }

            report.ColumnCompleteness[col] = ds.Count > 0 ? (double)colNonNull / ds.Count : 0.0;
        }

        if (totalCells == 0) return 100.0;

        double fraction = (double)nonNullCells / totalCells;
        if (fraction < 0.5)
        {
            report.Issues.Add($"Very low completeness: {fraction * 100:F1}% of cells are non-null.");
        }
        else if (fraction < 0.9)
        {
            report.Issues.Add($"Moderate completeness: {fraction * 100:F1}% of cells are non-null.");
        }

        return fraction * 100.0;
    }

    private static double ComputeConsistency(Dataset ds, DataQualityReport report)
    {
        int consistentColumns = 0;
        int totalColumns = ds.Schema.Columns.Count;

        for (int c = 0; c < ds.Schema.Columns.Count; c++)
        {
            string col = ds.Schema.Columns[c].Name;
            HashSet<Type> types = new();

            for (int r = 0; r < ds.Count; r++)
            {
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null)
                {
                    types.Add(val.GetType());
                }
            }

            if (types.Count <= 1)
            {
                consistentColumns++;
            }
            else
            {
                report.Issues.Add($"Column '{col}' has mixed types: {types.Count} different types found.");
            }
        }

        return totalColumns > 0 ? (double)consistentColumns / totalColumns * 100.0 : 100.0;
    }

    private static double ComputeAccuracy(Dataset ds, DataQualityReport report)
    {
        int totalNumeric = 0;
        int inRange = 0;

        for (int c = 0; c < ds.Schema.Columns.Count; c++)
        {
            string col = ds.Schema.Columns[c].Name;

            double colMin = double.MaxValue;
            double colMax = double.MinValue;
            bool hasNumeric = false;

            for (int r = 0; r < ds.Count; r++)
            {
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    if (d < colMin) colMin = d;
                    if (d > colMax) colMax = d;
                    hasNumeric = true;
                }
            }

            if (!hasNumeric) continue;

            for (int r = 0; r < ds.Count; r++)
            {
                if (ds.Rows[r].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    totalNumeric++;
                    double d = Convert.ToDouble(val);
                    if (!double.IsNaN(d) && !double.IsInfinity(d))
                    {
                        inRange++;
                    }
                    else
                    {
                        report.Issues.Add($"Column '{col}' contains {(double.IsNaN(d) ? "NaN" : "Infinity")} at row {r}.");
                    }
                }
            }
        }

        if (totalNumeric == 0) return 100.0;

        return (double)inRange / totalNumeric * 100.0;
    }

    private static double ComputeTimeliness(Dataset ds, DataQualityReport report)
    {
        bool hasTimestamp = false;
        DateTimeOffset latestTimestamp = DateTimeOffset.MinValue;
        DateTimeOffset earliestTimestamp = DateTimeOffset.MaxValue;

        for (int r = 0; r < ds.Count; r++)
        {
            foreach (KeyValuePair<string, object?> kvp in ds.Rows[r])
            {
                if (kvp.Value is DateTimeOffset dto)
                {
                    hasTimestamp = true;
                    if (dto > latestTimestamp) latestTimestamp = dto;
                    if (dto < earliestTimestamp) earliestTimestamp = dto;
                }
                else if (kvp.Value is DateTime dt)
                {
                    hasTimestamp = true;
                    DateTimeOffset converted = new(dt);
                    if (converted > latestTimestamp) latestTimestamp = converted;
                    if (converted < earliestTimestamp) earliestTimestamp = converted;
                }
            }
        }

        if (!hasTimestamp)
        {
            report.Issues.Add("No timestamp columns found; timeliness could not be assessed.");
            return 100.0;
        }

        TimeSpan age = DateTimeOffset.UtcNow - latestTimestamp;
        if (age.TotalDays > 365)
        {
            report.Issues.Add($"Data is over {age.TotalDays / 365:F1} years old.");
            return 50.0;
        }

        if (age.TotalDays > 30)
        {
            report.Issues.Add($"Data is {age.TotalDays / 30:F1} months old.");
            return 75.0;
        }

        return 100.0;
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
