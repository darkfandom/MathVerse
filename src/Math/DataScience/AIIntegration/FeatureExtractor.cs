namespace MathVerse.Math.DataScience.AIIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Extracts numerical and categorical features from datasets for machine learning.
/// </summary>
public static class FeatureExtractor
{
    /// <summary>
    /// Extracts numerical features from the dataset as a 2D array.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <returns>A 2D array where each row is a feature vector.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no numeric columns are found.</exception>
    public static double[][] ExtractNumericalFeatures(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (ds.Count == 0) throw new ArgumentException("Dataset is empty.", nameof(ds));

        List<string> numericCols = GetNumericColumnNames(ds);
        if (numericCols.Count == 0)
            throw new ArgumentException("No numeric columns found in the dataset.");

        double[][] result = new double[ds.Count][];
        for (int i = 0; i < ds.Count; i++)
        {
            result[i] = new double[numericCols.Count];
            for (int j = 0; j < numericCols.Count; j++)
            {
                if (ds.Rows[i].TryGetValue(numericCols[j], out object? val) && val is not null && IsNumeric(val))
                    result[i][j] = Convert.ToDouble(val);
                else
                    result[i][j] = 0.0;
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts numerical features along with their column names.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <returns>A tuple of feature matrix and column names.</returns>
    public static (double[][] Features, string[] ColumnNames) ExtractNumericalFeaturesWithNames(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (ds.Count == 0) throw new ArgumentException("Dataset is empty.", nameof(ds));

        List<string> numericCols = GetNumericColumnNames(ds);
        if (numericCols.Count == 0)
            throw new ArgumentException("No numeric columns found in the dataset.");

        double[][] features = ExtractNumericalFeatures(ds);
        return (features, numericCols.ToArray());
    }

    /// <summary>
    /// Extracts categorical features as one-hot encoded arrays.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <returns>A 2D array of one-hot encoded values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> is null.</exception>
    public static double[][] ExtractCategoricalFeatures(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (ds.Count == 0) throw new ArgumentException("Dataset is empty.", nameof(ds));

        List<string> categoricalCols = GetCategoricalColumnNames(ds);
        if (categoricalCols.Count == 0)
            return new double[ds.Count][];

        var columnMappings = new List<Dictionary<string, int>>();
        int totalFeatures = 0;

        foreach (string col in categoricalCols)
        {
            var distinctValues = new SortedSet<string>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                    distinctValues.Add(val.ToString() ?? "null");
            }

            var mapping = new Dictionary<string, int>();
            int idx = 0;
            foreach (string v in distinctValues)
            {
                mapping[v] = idx++;
            }

            columnMappings.Add(mapping);
            totalFeatures += mapping.Count;
        }

        double[][] result = new double[ds.Count][];
        for (int i = 0; i < ds.Count; i++)
        {
            result[i] = new double[totalFeatures];
            int offset = 0;

            for (int j = 0; j < categoricalCols.Count; j++)
            {
                string col = categoricalCols[j];
                var mapping = columnMappings[j];

                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null)
                {
                    string key = val.ToString() ?? "null";
                    if (mapping.TryGetValue(key, out int vi))
                        result[i][offset + vi] = 1.0;
                }

                offset += mapping.Count;
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts categorical features along with the encoding mappings.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <returns>A tuple of encoded matrix and per-column mappings.</returns>
    public static (double[][] Features, Dictionary<string, string>[] Mappings) ExtractCategoricalFeaturesWithMappings(Dataset ds)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));

        List<string> categoricalCols = GetCategoricalColumnNames(ds);
        var mappingsList = new List<Dictionary<string, string>>();

        foreach (string col in categoricalCols)
        {
            var distinctValues = new SortedSet<string>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                    distinctValues.Add(val.ToString() ?? "null");
            }

            var mapping = new Dictionary<string, string>();
            int idx = 0;
            foreach (string v in distinctValues)
            {
                mapping[v] = $"{col}_{idx}";
                idx++;
            }

            mappingsList.Add(mapping);
        }

        double[][] features = ExtractCategoricalFeatures(ds);
        return (features, mappingsList.ToArray());
    }

    /// <summary>
    /// Gets the names of numeric columns in the dataset.
    /// </summary>
    /// <param name="ds">The dataset.</param>
    /// <returns>A list of numeric column names.</returns>
    public static List<string> GetNumericColumnNames(Dataset ds)
    {
        var names = new List<string>();
        if (ds.Count == 0) return names;

        foreach (string col in ds.Rows[0].Keys)
        {
            bool isNumeric = true;
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && !IsNumeric(val))
                {
                    isNumeric = false;
                    break;
                }
            }
            if (isNumeric)
                names.Add(col);
        }

        return names;
    }

    /// <summary>
    /// Gets the names of categorical columns in the dataset.
    /// </summary>
    /// <param name="ds">The dataset.</param>
    /// <returns>A list of categorical column names.</returns>
    public static List<string> GetCategoricalColumnNames(Dataset ds)
    {
        var names = new List<string>();
        if (ds.Count == 0) return names;

        foreach (string col in ds.Rows[0].Keys)
        {
            bool isCategorical = false;
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && !IsNumeric(val))
                {
                    isCategorical = true;
                    break;
                }
            }
            if (isCategorical)
                names.Add(col);
        }

        return names;
    }

    private static bool IsNumeric(object? value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
