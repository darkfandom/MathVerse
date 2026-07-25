namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;
using Core;

/// <summary>
/// Generates visualization data from datasets for histogram, scatter, box plot, heatmap, and time series charts.
/// </summary>
public sealed class DataVisualizer
{
    /// <summary>
    /// Represents a single bin in a histogram.
    /// </summary>
    public sealed class HistogramBin
    {
        /// <summary>
        /// Gets or sets the lower bound of the bin.
        /// </summary>
        public double LowerBound { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the bin.
        /// </summary>
        public double UpperBound { get; set; }

        /// <summary>
        /// Gets or sets the count of values in the bin.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the relative frequency (count / total).
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Gets or sets the label for this bin.
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the result of a histogram computation.
    /// </summary>
    public sealed class HistogramData
    {
        /// <summary>
        /// Gets or sets the column name the histogram was computed from.
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the histogram bins.
        /// </summary>
        public List<HistogramBin> Bins { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of values counted.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in the data.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in the data.
        /// </summary>
        public double Max { get; set; }
    }

    /// <summary>
    /// Represents a single data point in a scatter plot.
    /// </summary>
    public sealed class ScatterPoint
    {
        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the original row index.
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// Represents the result of a scatter plot computation.
    /// </summary>
    public sealed class ScatterData
    {
        /// <summary>
        /// Gets or sets the X column name.
        /// </summary>
        public string XColumn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Y column name.
        /// </summary>
        public string YColumn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data points.
        /// </summary>
        public List<ScatterPoint> Points { get; set; } = new();

        /// <summary>
        /// Gets or sets the Pearson correlation coefficient.
        /// </summary>
        public double Correlation { get; set; }
    }

    /// <summary>
    /// Represents the result of a box plot computation for multiple columns.
    /// </summary>
    public sealed class BoxPlotResult
    {
        /// <summary>
        /// Gets or sets the box plot data for each column.
        /// </summary>
        public List<BoxPlotData> BoxPlots { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of a heatmap computation.
    /// </summary>
    public sealed class HeatmapResult
    {
        /// <summary>
        /// Gets or sets the heatmap labels.
        /// </summary>
        public string[] Labels { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the 2D correlation values.
        /// </summary>
        public double[,] Values { get; set; } = new double[0, 0];
    }

    /// <summary>
    /// Represents a single time series data point.
    /// </summary>
    public sealed class TimeSeriesPoint
    {
        /// <summary>
        /// Gets or sets the time value.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Gets or sets the values at this time point.
        /// </summary>
        public Dictionary<string, double> Values { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of a time series computation.
    /// </summary>
    public sealed class TimeSeriesResult
    {
        /// <summary>
        /// Gets or sets the time column name.
        /// </summary>
        public string TimeColumn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value column names.
        /// </summary>
        public string[] ValueColumns { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the time series data points.
        /// </summary>
        public List<TimeSeriesPoint> Points { get; set; } = new();
    }

    /// <summary>
    /// Generates histogram data for a numeric column in the dataset.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <param name="column">The column name to create the histogram for.</param>
    /// <param name="bins">The number of bins (default 10).</param>
    /// <returns>A <see cref="HistogramData"/> containing the bin data.</returns>
    public static HistogramData Histogram(Dataset ds, string column, int bins = 10)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));
        if (bins < 1) throw new ArgumentOutOfRangeException(nameof(bins), bins, "Bins must be at least 1.");

        List<double> values = GetNumericColumn(ds, column);
        if (values.Count == 0)
            return new HistogramData { ColumnName = column, TotalCount = 0 };

        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] < minVal) minVal = values[i];
            if (values[i] > maxVal) maxVal = values[i];
        }

        if (System.Math.Abs(maxVal - minVal) < 1e-15)
        {
            double mid = minVal;
            return new HistogramData
            {
                ColumnName = column,
                TotalCount = values.Count,
                Min = minVal,
                Max = maxVal,
                Bins = new List<HistogramBin>
                {
                    new()
                    {
                        LowerBound = mid - 0.5,
                        UpperBound = mid + 0.5,
                        Count = values.Count,
                        Frequency = 1.0,
                        Label = $"{mid:G}"
                    }
                }
            };
        }

        double binWidth = (maxVal - minVal) / bins;
        List<HistogramBin> histBins = new(bins);
        for (int b = 0; b < bins; b++)
        {
            double lower = minVal + b * binWidth;
            double upper = lower + binWidth;
            histBins.Add(new HistogramBin
            {
                LowerBound = lower,
                UpperBound = upper,
                Label = $"{lower:G} - {upper:G}"
            });
        }

        for (int i = 0; i < values.Count; i++)
        {
            int binIndex = (int)((values[i] - minVal) / binWidth);
            if (binIndex >= bins) binIndex = bins - 1;
            if (binIndex < 0) binIndex = 0;
            histBins[binIndex].Count++;
        }

        for (int b = 0; b < histBins.Count; b++)
        {
            histBins[b].Frequency = values.Count > 0 ? (double)histBins[b].Count / values.Count : 0.0;
        }

        return new HistogramData
        {
            ColumnName = column,
            Bins = histBins,
            TotalCount = values.Count,
            Min = minVal,
            Max = maxVal
        };
    }

    /// <summary>
    /// Generates scatter plot data from two numeric columns.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <param name="xCol">The column name for the X axis.</param>
    /// <param name="yCol">The column name for the Y axis.</param>
    /// <returns>A <see cref="ScatterData"/> containing the points and correlation.</returns>
    public static ScatterData Scatter(Dataset ds, string xCol, string yCol)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(xCol)) throw new ArgumentException("X column name cannot be null or empty.", nameof(xCol));
        if (string.IsNullOrEmpty(yCol)) throw new ArgumentException("Y column name cannot be null or empty.", nameof(yCol));

        List<ScatterPoint> points = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            if (row.TryGetValue(xCol, out object? xVal) && xVal is not null && IsNumeric(xVal) &&
                row.TryGetValue(yCol, out object? yVal) && yVal is not null && IsNumeric(yVal))
            {
                points.Add(new ScatterPoint
                {
                    X = Convert.ToDouble(xVal),
                    Y = Convert.ToDouble(yVal),
                    Index = i
                });
            }
        }

        double correlation = 0.0;
        if (points.Count >= 2)
        {
            double[] xArr = new double[points.Count];
            double[] yArr = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                xArr[i] = points[i].X;
                yArr[i] = points[i].Y;
            }
            correlation = CorrelationMatrixVisualizer.PearsonCorrelation(xArr, yArr);
        }

        return new ScatterData
        {
            XColumn = xCol,
            YColumn = yCol,
            Points = points,
            Correlation = correlation
        };
    }

    /// <summary>
    /// Generates box plot data for multiple numeric columns.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <param name="columns">The column names to create box plots for.</param>
    /// <returns>A <see cref="BoxPlotResult"/> containing box plot data for each column.</returns>
    public static BoxPlotResult BoxPlot(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("Columns array cannot be null or empty.", nameof(columns));

        List<BoxPlotData> boxPlots = new(columns.Length);
        for (int c = 0; c < columns.Length; c++)
        {
            List<double> values = GetNumericColumn(ds, columns[c]);
            if (values.Count > 0)
            {
                boxPlots.Add(BoxPlotData.Compute(values.ToArray(), columns[c]));
            }
        }

        return new BoxPlotResult { BoxPlots = boxPlots };
    }

    /// <summary>
    /// Generates correlation heatmap data for the specified numeric columns.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <param name="columns">The column names to compute correlations for.</param>
    /// <returns>A <see cref="HeatmapResult"/> containing the correlation matrix.</returns>
    public static HeatmapResult Heatmap(Dataset ds, string[] columns)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("Columns array cannot be null or empty.", nameof(columns));

        int n = columns.Length;
        double[][] columnArrays = new double[n][];
        for (int c = 0; c < n; c++)
        {
            List<double> vals = GetNumericColumn(ds, columns[c]);
            columnArrays[c] = vals.ToArray();
        }

        double[,] matrix = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    matrix[i, j] = 1.0;
                }
                else if (j < i)
                {
                    matrix[i, j] = matrix[j, i];
                }
                else
                {
                    int len = System.Math.Min(columnArrays[i].Length, columnArrays[j].Length);
                    double[] x = new double[len];
                    double[] y = new double[len];
                    System.Array.Copy(columnArrays[i], x, len);
                    System.Array.Copy(columnArrays[j], y, len);
                    matrix[i, j] = CorrelationMatrixVisualizer.PearsonCorrelation(x, y);
                }
            }
        }

        return new HeatmapResult
        {
            Labels = columns,
            Values = matrix
        };
    }

    /// <summary>
    /// Generates time series data from a dataset with a time column and value columns.
    /// </summary>
    /// <param name="ds">The dataset to analyze.</param>
    /// <param name="timeCol">The column name containing time values.</param>
    /// <param name="valueCols">The column names containing the values to plot.</param>
    /// <returns>A <see cref="TimeSeriesResult"/> containing the time series data points.</returns>
    public static TimeSeriesResult TimeSeries(Dataset ds, string timeCol, string[] valueCols)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(timeCol)) throw new ArgumentException("Time column name cannot be null or empty.", nameof(timeCol));
        if (valueCols is null || valueCols.Length == 0)
            throw new ArgumentException("Value columns array cannot be null or empty.", nameof(valueCols));

        HashSet<string> valueColSet = new(valueCols);
        List<TimeSeriesPoint> points = new();

        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            if (!row.TryGetValue(timeCol, out object? timeVal) || timeVal is null || !IsNumeric(timeVal))
                continue;

            TimeSeriesPoint point = new() { Time = Convert.ToDouble(timeVal) };
            bool hasAny = false;
            for (int c = 0; c < valueCols.Length; c++)
            {
                if (row.TryGetValue(valueCols[c], out object? val) && val is not null && IsNumeric(val))
                {
                    point.Values[valueCols[c]] = Convert.ToDouble(val);
                    hasAny = true;
                }
            }

            if (hasAny)
            {
                points.Add(point);
            }
        }

        return new TimeSeriesResult
        {
            TimeColumn = timeCol,
            ValueColumns = valueCols,
            Points = points
        };
    }

    /// <summary>
    /// Extracts all numeric values from a column in the dataset.
    /// </summary>
    /// <param name="ds">The dataset.</param>
    /// <param name="column">The column name.</param>
    /// <returns>A list of numeric values from the specified column.</returns>
    private static List<double> GetNumericColumn(Dataset ds, string column)
    {
        List<double> values = new();
        for (int i = 0; i < ds.Count; i++)
        {
            if (ds.Rows[i].TryGetValue(column, out object? val) && val is not null && IsNumeric(val))
            {
                values.Add(Convert.ToDouble(val));
            }
        }
        return values;
    }

    /// <summary>
    /// Determines whether a value is a numeric type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>true if the value is numeric; otherwise, false.</returns>
    private static bool IsNumeric(object value)
    {
        return value is int or long or float or double or decimal or short or byte;
    }
}
