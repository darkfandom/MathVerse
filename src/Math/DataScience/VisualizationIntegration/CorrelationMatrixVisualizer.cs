namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Generates correlation matrix data formatted for heatmap visualization.
/// </summary>
public sealed class CorrelationMatrixVisualizer
{
    /// <summary>
    /// Represents a single cell in the correlation matrix heatmap.
    /// </summary>
    public sealed class CorrelationCell
    {
        /// <summary>
        /// Gets or sets the row label.
        /// </summary>
        public string RowLabel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the column label.
        /// </summary>
        public string ColumnLabel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the correlation coefficient.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the row index in the matrix.
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the column index in the matrix.
        /// </summary>
        public int ColumnIndex { get; set; }
    }

    /// <summary>
    /// Represents the complete heatmap data for a correlation matrix.
    /// </summary>
    public sealed class HeatmapData
    {
        /// <summary>
        /// Gets or sets the labels for each axis.
        /// </summary>
        public string[] Labels { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the 2D correlation values.
        /// </summary>
        public double[,] Values { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the flattened list of all cells for rendering.
        /// </summary>
        public List<CorrelationCell> Cells { get; set; } = new();

        /// <summary>
        /// Gets or sets the minimum correlation value in the matrix.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum correlation value in the matrix.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the mean absolute correlation value.
        /// </summary>
        public double MeanAbsoluteCorrelation { get; set; }
    }

    /// <summary>
    /// Generates heatmap data from a correlation matrix and corresponding labels.
    /// </summary>
    /// <param name="correlationMatrix">The 2D array of correlation coefficients.</param>
    /// <param name="labels">The labels for each variable.</param>
    /// <returns>A new <see cref="HeatmapData"/> instance containing the heatmap data.</returns>
    public static HeatmapData Generate(double[,] correlationMatrix, string[] labels)
    {
        if (correlationMatrix is null) throw new ArgumentNullException(nameof(correlationMatrix));
        if (labels is null) throw new ArgumentNullException(nameof(labels));

        int rows = correlationMatrix.GetLength(0);
        int cols = correlationMatrix.GetLength(1);

        if (rows != cols)
            throw new ArgumentException("Correlation matrix must be square.", nameof(correlationMatrix));
        if (labels.Length != rows)
            throw new ArgumentException("Labels length must match matrix dimensions.", nameof(labels));

        List<CorrelationCell> cells = new(rows * cols);
        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        double absSum = 0.0;
        int cellCount = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double val = correlationMatrix[r, c];
                cells.Add(new CorrelationCell
                {
                    RowLabel = labels[r],
                    ColumnLabel = labels[c],
                    Value = val,
                    RowIndex = r,
                    ColumnIndex = c
                });

                if (val < minVal) minVal = val;
                if (val > maxVal) maxVal = val;
                absSum += System.Math.Abs(val);
                cellCount++;
            }
        }

        return new HeatmapData
        {
            Labels = labels,
            Values = correlationMatrix,
            Cells = cells,
            MinValue = cellCount > 0 ? minVal : 0.0,
            MaxValue = cellCount > 0 ? maxVal : 0.0,
            MeanAbsoluteCorrelation = cellCount > 0 ? absSum / cellCount : 0.0
        };
    }

    /// <summary>
    /// Computes the Pearson correlation coefficient between two arrays.
    /// </summary>
    /// <param name="x">The first data array.</param>
    /// <param name="y">The second data array.</param>
    /// <returns>The Pearson correlation coefficient in the range [-1, 1].</returns>
    public static double PearsonCorrelation(double[] x, double[] y)
    {
        if (x is null) throw new ArgumentNullException(nameof(x));
        if (y is null) throw new ArgumentNullException(nameof(y));
        if (x.Length != y.Length)
            throw new ArgumentException("Arrays must have the same length.");
        if (x.Length < 2) throw new ArgumentException("Arrays must contain at least 2 elements.");

        int n = x.Length;
        double sumX = 0.0;
        double sumY = 0.0;
        for (int i = 0; i < n; i++)
        {
            sumX += x[i];
            sumY += y[i];
        }
        double meanX = sumX / n;
        double meanY = sumY / n;

        double sumXY = 0.0;
        double sumX2 = 0.0;
        double sumY2 = 0.0;
        for (int i = 0; i < n; i++)
        {
            double dx = x[i] - meanX;
            double dy = y[i] - meanY;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        double denom = System.Math.Sqrt(sumX2 * sumY2);
        if (denom < 1e-15) return 0.0;

        return sumXY / denom;
    }
}
