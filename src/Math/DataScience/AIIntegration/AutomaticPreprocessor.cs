namespace MathVerse.Math.DataScience.AIIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Automatically preprocesses datasets by handling missing values, encoding categoricals, and scaling numerics.
/// </summary>
public static class AutomaticPreprocessor
{
    /// <summary>
    /// Preprocesses a dataset in-place: fills missing values, encodes categorical columns, and scales numerics.
    /// </summary>
    /// <param name="ds">The dataset to preprocess.</param>
    /// <returns>The modified dataset.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    public static Dataset Preprocess(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (ds.Count == 0) return ds;

        HandleMissingValues(ds);
        EncodeCategoricalColumns(ds);
        ScaleNumericColumns(ds);

        return ds;
    }

    /// <summary>
    /// Fills missing values in a dataset using the specified strategy.
    /// </summary>
    /// <param name="ds">The dataset to modify.</param>
    /// <param name="strategy">The fill strategy: "mean", "median", "zero", or "drop".</param>
    /// <returns>The modified dataset.</returns>
    public static Dataset FillMissingValues(Dataset ds, string strategy = "mean")
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (ds.Count == 0) return ds;

        switch (strategy.ToLowerInvariant())
        {
            case "mean":
                FillWithMean(ds);
                break;
            case "median":
                FillWithMedian(ds);
                break;
            case "zero":
                FillWithZero(ds);
                break;
            case "drop":
                DropRowsWithMissing(ds);
                break;
            default:
                throw new ArgumentException($"Unknown fill strategy: {strategy}. Use 'mean', 'median', 'zero', or 'drop'.");
        }

        return ds;
    }

    private static void HandleMissingValues(Dataset ds)
    {
        foreach (string col in GetAllColumnNames(ds))
        {
            bool hasMissing = false;
            foreach (var row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                {
                    hasMissing = true;
                    break;
                }
            }

            if (!hasMissing) continue;

            bool isNumeric = IsColumnNumeric(ds, col);

            if (isNumeric)
            {
                double median = ComputeColumnMedian(ds, col);
                foreach (var row in ds.Rows)
                {
                    if (!row.ContainsKey(col) || row[col] is null)
                        row[col] = median;
                }
            }
            else
            {
                string mode = ComputeColumnMode(ds, col);
                foreach (var row in ds.Rows)
                {
                    if (!row.ContainsKey(col) || row[col] is null)
                        row[col] = mode;
                }
            }
        }
    }

    private static void EncodeCategoricalColumns(Dataset ds)
    {
        var categoricalCols = new List<string>();
        foreach (string col in GetAllColumnNames(ds))
        {
            if (!IsColumnNumeric(ds, col))
                categoricalCols.Add(col);
        }

        foreach (string col in categoricalCols)
        {
            var distinctValues = new HashSet<string>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                    distinctValues.Add(val.ToString() ?? "null");
            }

            var valueIndex = new Dictionary<string, int>();
            int idx = 0;
            foreach (string v in distinctValues)
            {
                valueIndex[v] = idx++;
            }

            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                {
                    string key = val.ToString() ?? "null";
                    row[col] = valueIndex.TryGetValue(key, out int vi) ? vi : -1;
                }
                else
                {
                    row[col] = -1;
                }
            }
        }
    }

    private static void ScaleNumericColumns(Dataset ds)
    {
        foreach (string col in GetAllColumnNames(ds))
        {
            if (!IsColumnNumeric(ds, col)) continue;

            double min = double.MaxValue, max = double.MinValue;
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumericType(val))
                {
                    double d = Convert.ToDouble(val);
                    if (d < min) min = d;
                    if (d > max) max = d;
                }
            }

            double range = max - min;
            if (range < 1e-15) continue;

            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumericType(val))
                {
                    row[col] = (Convert.ToDouble(val) - min) / range;
                }
            }
        }
    }

    private static void FillWithMean(Dataset ds)
    {
        foreach (string col in GetAllColumnNames(ds))
        {
            if (!IsColumnNumeric(ds, col)) continue;

            double sum = 0.0;
            int count = 0;
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumericType(val))
                {
                    sum += Convert.ToDouble(val);
                    count++;
                }
            }

            double mean = count > 0 ? sum / count : 0.0;

            foreach (var row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                    row[col] = mean;
            }
        }
    }

    private static void FillWithMedian(Dataset ds)
    {
        foreach (string col in GetAllColumnNames(ds))
        {
            if (!IsColumnNumeric(ds, col)) continue;

            double median = ComputeColumnMedian(ds, col);

            foreach (var row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                    row[col] = median;
            }
        }
    }

    private static void FillWithZero(Dataset ds)
    {
        foreach (string col in GetAllColumnNames(ds))
        {
            foreach (var row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                    row[col] = IsColumnNumeric(ds, col) ? 0.0 : "";
            }
        }
    }

    private static void DropRowsWithMissing(Dataset ds)
    {
        ds.Rows.RemoveAll(row =>
        {
            foreach (string col in GetAllColumnNames(ds))
            {
                if (!row.ContainsKey(col) || row[col] is null)
                    return true;
            }
            return false;
        });
    }

    private static double ComputeColumnMedian(Dataset ds, string col)
    {
        var values = new List<double>();
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(col, out object? val) && val is not null && IsNumericType(val))
                values.Add(Convert.ToDouble(val));
        }

        if (values.Count == 0) return 0.0;
        values.Sort();
        int mid = values.Count / 2;
        return values.Count % 2 == 0 ? (values[mid - 1] + values[mid]) / 2.0 : values[mid];
    }

    private static string ComputeColumnMode(Dataset ds, string col)
    {
        var counts = new Dictionary<string, int>();
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(col, out object? val) && val is not null)
            {
                string key = val.ToString() ?? "null";
                counts[key] = counts.TryGetValue(key, out int c) ? c + 1 : 1;
            }
        }

        string mode = "";
        int maxCount = 0;
        foreach (var kvp in counts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mode = kvp.Key;
            }
        }

        return mode;
    }

    private static bool IsColumnNumeric(Dataset ds, string col)
    {
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(col, out object? val) && val is not null)
            {
                return IsNumericType(val);
            }
        }
        return false;
    }

    private static List<string> GetAllColumnNames(Dataset ds)
    {
        var names = new HashSet<string>();
        foreach (var row in ds.Rows)
        {
            foreach (string key in row.Keys)
                names.Add(key);
        }
        return new List<string>(names);
    }

    private static bool IsNumericType(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
