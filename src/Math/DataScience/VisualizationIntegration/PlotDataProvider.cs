namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;
using Core;

/// <summary>
/// Provides data formatted for the Visualization module's chart components.
/// </summary>
public sealed class PlotDataProvider
{
    /// <summary>
    /// Represents a single data point for an XY plot series.
    /// </summary>
    public sealed class XYPoint
    {
        /// <summary>
        /// Gets or sets the X value.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y value.
        /// </summary>
        public double Y { get; set; }
    }

    /// <summary>
    /// Represents a data point for a bar or pie chart.
    /// </summary>
    public sealed class CategoryValue
    {
        /// <summary>
        /// Gets or sets the category label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the numeric value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Converts two columns from a dataset into XY plot series data.
    /// </summary>
    /// <param name="ds">The dataset to extract data from.</param>
    /// <param name="xCol">The column name for the X axis.</param>
    /// <param name="yCol">The column name for the Y axis.</param>
    /// <returns>A list of <see cref="XYPoint"/> values suitable for line or scatter plots.</returns>
    public static List<XYPoint> ToPlotSeries(Dataset ds, string xCol, string yCol)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(xCol)) throw new ArgumentException("X column name cannot be null or empty.", nameof(xCol));
        if (string.IsNullOrEmpty(yCol)) throw new ArgumentException("Y column name cannot be null or empty.", nameof(yCol));

        List<XYPoint> points = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            if (row.TryGetValue(xCol, out object? xVal) && xVal is not null && IsNumeric(xVal) &&
                row.TryGetValue(yCol, out object? yVal) && yVal is not null && IsNumeric(yVal))
            {
                points.Add(new XYPoint
                {
                    X = Convert.ToDouble(xVal),
                    Y = Convert.ToDouble(yVal)
                });
            }
        }

        return points;
    }

    /// <summary>
    /// Converts label and value columns into bar chart data.
    /// </summary>
    /// <param name="ds">The dataset to extract data from.</param>
    /// <param name="labelCol">The column name for bar labels.</param>
    /// <param name="valueCol">The column name for bar values.</param>
    /// <returns>A list of <see cref="CategoryValue"/> entries suitable for bar charts.</returns>
    public static List<CategoryValue> ToBarData(Dataset ds, string labelCol, string valueCol)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(labelCol)) throw new ArgumentException("Label column name cannot be null or empty.", nameof(labelCol));
        if (string.IsNullOrEmpty(valueCol)) throw new ArgumentException("Value column name cannot be null or empty.", nameof(valueCol));

        List<CategoryValue> bars = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            string label = row.TryGetValue(labelCol, out object? lbl) ? lbl?.ToString() ?? string.Empty : string.Empty;
            if (row.TryGetValue(valueCol, out object? val) && val is not null && IsNumeric(val))
            {
                bars.Add(new CategoryValue
                {
                    Label = label,
                    Value = Convert.ToDouble(val)
                });
            }
        }

        return bars;
    }

    /// <summary>
    /// Converts label and value columns into pie chart data.
    /// </summary>
    /// <param name="ds">The dataset to extract data from.</param>
    /// <param name="labelCol">The column name for slice labels.</param>
    /// <param name="valueCol">The column name for slice values.</param>
    /// <returns>A list of <see cref="CategoryValue"/> entries suitable for pie charts.</returns>
    public static List<CategoryValue> ToPieData(Dataset ds, string labelCol, string valueCol)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(labelCol)) throw new ArgumentException("Label column name cannot be null or empty.", nameof(labelCol));
        if (string.IsNullOrEmpty(valueCol)) throw new ArgumentException("Value column name cannot be null or empty.", nameof(valueCol));

        Dictionary<string, double> aggregated = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            string label = row.TryGetValue(labelCol, out object? lbl) ? lbl?.ToString() ?? string.Empty : string.Empty;
            if (row.TryGetValue(valueCol, out object? val) && val is not null && IsNumeric(val))
            {
                double d = Convert.ToDouble(val);
                if (aggregated.ContainsKey(label))
                    aggregated[label] += d;
                else
                    aggregated[label] = d;
            }
        }

        List<CategoryValue> slices = new(aggregated.Count);
        foreach (KeyValuePair<string, double> kvp in aggregated)
        {
            slices.Add(new CategoryValue { Label = kvp.Key, Value = kvp.Value });
        }

        return slices;
    }

    /// <summary>
    /// Converts two columns from a dataset into area chart data.
    /// </summary>
    /// <param name="ds">The dataset to extract data from.</param>
    /// <param name="xCol">The column name for the X axis (horizontal).</param>
    /// <param name="yCol">The column name for the Y axis (height).</param>
    /// <returns>A list of <see cref="XYPoint"/> values suitable for area charts.</returns>
    public static List<XYPoint> ToAreaData(Dataset ds, string xCol, string yCol)
    {
        if (ds is null) throw new ArgumentNullException(nameof(ds));
        if (string.IsNullOrEmpty(xCol)) throw new ArgumentException("X column name cannot be null or empty.", nameof(xCol));
        if (string.IsNullOrEmpty(yCol)) throw new ArgumentException("Y column name cannot be null or empty.", nameof(yCol));

        List<XYPoint> points = new();
        for (int i = 0; i < ds.Count; i++)
        {
            Dictionary<string, object?> row = ds.Rows[i];
            if (row.TryGetValue(xCol, out object? xVal) && xVal is not null && IsNumeric(xVal) &&
                row.TryGetValue(yCol, out object? yVal) && yVal is not null && IsNumeric(yVal))
            {
                points.Add(new XYPoint
                {
                    X = Convert.ToDouble(xVal),
                    Y = Convert.ToDouble(yVal)
                });
            }
        }

        return points;
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
