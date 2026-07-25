namespace MathVerse.Math.DataScience.DistributedAnalytics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Configuration options for parallel feature engineering.
/// </summary>
public sealed class FeatureEngineeringOptions
{
    /// <summary>
    /// Gets or sets the numeric columns to extract features from.
    /// </summary>
    public string[] NumericColumns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the categorical columns to one-hot encode.
    /// </summary>
    public string[] CategoricalColumns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether to generate polynomial features.
    /// </summary>
    public bool GeneratePolynomials { get; set; }

    /// <summary>
    /// Gets or sets the polynomial degree (default 2).
    /// </summary>
    public int PolynomialDegree { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to generate interaction features between numeric columns.
    /// </summary>
    public bool GenerateInteractions { get; set; }

    /// <summary>
    /// Gets or sets whether to generate log-transformed features.
    /// </summary>
    public bool GenerateLogFeatures { get; set; }

    /// <summary>
    /// Gets or sets whether to generate square root features.
    /// </summary>
    public bool GenerateSqrtFeatures { get; set; }

    /// <summary>
    /// Gets or sets whether to generate binning features for numeric columns.
    /// </summary>
    public bool GenerateBinning { get; set; }

    /// <summary>
    /// Gets or sets the number of bins for binning features.
    /// </summary>
    public int BinCount { get; set; } = 10;
}

/// <summary>
/// Provides parallel feature engineering operations for datasets.
/// </summary>
public static class ParallelFeatureEngineering
{
    /// <summary>
    /// Generates features from a dataset based on the specified options, processing each feature type in parallel.
    /// </summary>
    /// <param name="ds">The source dataset.</param>
    /// <param name="options">The feature engineering options.</param>
    /// <returns>A new dataset with the generated features appended.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ds"/> or <paramref name="options"/> is null.</exception>
    public static Dataset GenerateFeatures(Dataset ds, FeatureEngineeringOptions options)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (options is null) throw new ArgumentNullException(nameof(options));

        var result = new Dataset
        {
            Name = ds.Name,
            Metadata = ds.Metadata,
            Schema = ds.Schema
        };

        foreach (var row in ds.Rows)
            result.Rows.Add(new Dictionary<string, object?>(row));

        var tasks = new List<Action>();

        if (options.NumericColumns.Length > 0)
        {
            tasks.Add(() => GenerateNumericStats(result, options.NumericColumns));

            if (options.GeneratePolynomials)
                tasks.Add(() => GeneratePolynomialFeatures(result, options.NumericColumns, options.PolynomialDegree));

            if (options.GenerateInteractions)
                tasks.Add(() => GenerateInteractionFeatures(result, options.NumericColumns));

            if (options.GenerateLogFeatures)
                tasks.Add(() => GenerateLogTransformedFeatures(result, options.NumericColumns));

            if (options.GenerateSqrtFeatures)
                tasks.Add(() => GenerateSqrtFeatures(result, options.NumericColumns));

            if (options.GenerateBinning)
                tasks.Add(() => GenerateBinnedFeatures(result, options.NumericColumns, options.BinCount));
        }

        if (options.CategoricalColumns.Length > 0)
        {
            tasks.Add(() => GenerateOneHotEncodedFeatures(result, options.CategoricalColumns));
        }

        Parallel.Invoke(
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            tasks.ToArray());

        return result;
    }

    private static void GenerateNumericStats(Dataset ds, string[] columns)
    {
        Parallel.ForEach(columns, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, col =>
        {
            var values = ExtractColumnValues(ds, col);
            if (values.Length == 0) return;

            string minCol = $"{col}_min";
            string maxCol = $"{col}_max";
            string rangeCol = $"{col}_range";

            ds.Schema.AddColumn(minCol, ColumnType.Double);
            ds.Schema.AddColumn(maxCol, ColumnType.Double);
            ds.Schema.AddColumn(rangeCol, ColumnType.Double);

            double min = double.MaxValue, max = double.MinValue;
            foreach (double v in values)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            foreach (var row in ds.Rows)
            {
                row[minCol] = min;
                row[maxCol] = max;
                row[rangeCol] = max - min;
            }
        });
    }

    private static void GeneratePolynomialFeatures(Dataset ds, string[] columns, int degree)
    {
        foreach (string col in columns)
        {
            for (int deg = 2; deg <= degree; deg++)
            {
                string newCol = $"{col}_pow{deg}";
                ds.Schema.AddColumn(newCol, ColumnType.Double);
                foreach (var row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                        row[newCol] = System.Math.Pow(Convert.ToDouble(val), deg);
                    else
                        row[newCol] = null;
                }
            }
        }
    }

    private static void GenerateInteractionFeatures(Dataset ds, string[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            for (int j = i + 1; j < columns.Length; j++)
            {
                string newCol = $"{columns[i]}_x_{columns[j]}";
                ds.Schema.AddColumn(newCol, ColumnType.Double);

                foreach (var row in ds.Rows)
                {
                    if (row.TryGetValue(columns[i], out object? v1) && v1 is not null && IsNumeric(v1)
                        && row.TryGetValue(columns[j], out object? v2) && v2 is not null && IsNumeric(v2))
                    {
                        row[newCol] = Convert.ToDouble(v1) * Convert.ToDouble(v2);
                    }
                    else
                    {
                        row[newCol] = null;
                    }
                }
            }
        }
    }

    private static void GenerateLogTransformedFeatures(Dataset ds, string[] columns)
    {
        Parallel.ForEach(columns, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, col =>
        {
            string newCol = $"{col}_log";
            ds.Schema.AddColumn(newCol, ColumnType.Double);
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    row[newCol] = d > 0.0 ? System.Math.Log(d) : (d == 0.0 ? double.NegativeInfinity : double.NaN);
                }
                else
                {
                    row[newCol] = null;
                }
            }
        });
    }

    private static void GenerateSqrtFeatures(Dataset ds, string[] columns)
    {
        Parallel.ForEach(columns, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, col =>
        {
            string newCol = $"{col}_sqrt";
            ds.Schema.AddColumn(newCol, ColumnType.Double);
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    row[newCol] = d >= 0.0 ? System.Math.Sqrt(d) : double.NaN;
                }
                else
                {
                    row[newCol] = null;
                }
            }
        });
    }

    private static void GenerateBinnedFeatures(Dataset ds, string[] columns, int binCount)
    {
        foreach (string col in columns)
        {
            double[] values = ExtractColumnValues(ds, col);
            if (values.Length == 0) continue;

            double min = double.MaxValue, max = double.MinValue;
            foreach (double v in values)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            double range = max - min;
            if (range < 1e-15) range = 1.0;

            string newCol = $"{col}_bin";
            ds.Schema.AddColumn(newCol, ColumnType.Int);

            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    int bin = (int)System.Math.Floor((d - min) / range * binCount);
                    if (bin >= binCount) bin = binCount - 1;
                    row[newCol] = bin;
                }
                else
                {
                    row[newCol] = null;
                }
            }
        }
    }

    private static void GenerateOneHotEncodedFeatures(Dataset ds, string[] columns)
    {
        foreach (string col in columns)
        {
            var distinctValues = new HashSet<string>();
            foreach (var row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                    distinctValues.Add(val.ToString() ?? "null");
            }

            foreach (string val in distinctValues)
            {
                string encodedCol = $"{col}_{val}";
                ds.Schema.AddColumn(encodedCol, ColumnType.Int);

                foreach (var row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? v) && v is not null)
                        row[encodedCol] = string.Equals(v.ToString(), val, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                    else
                        row[encodedCol] = 0;
                }
            }
        }
    }

    private static double[] ExtractColumnValues(Dataset ds, string column)
    {
        var bag = new ConcurrentBag<double>();
        foreach (var row in ds.Rows)
        {
            if (row.TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
                bag.Add(Convert.ToDouble(val));
        }
        return bag.ToArray();
    }

    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
