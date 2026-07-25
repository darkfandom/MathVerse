namespace MathVerse.Math.Visualization.NumericalVisualization;

using System.Collections.Immutable;
using MathVerse.Math.Visualization._2DPlotting;

/// <summary>Creates heatmap visualizations of Jacobian matrices for analyzing numerical system behavior.</summary>
public sealed class JacobianHeatmap
{
    /// <summary>Creates a Plot2DResult showing a heatmap of a Jacobian matrix with cell values and color coding.</summary>
    /// <param name="jacobian">The Jacobian matrix as a 2D array [rows, cols].</param>
    /// <returns>A <see cref="Plot2DResult"/> containing the heatmap visualization.</returns>
    public static Plot2DResult Create(double[,] jacobian)
    {
        int rows = jacobian.GetLength(0);
        int cols = jacobian.GetLength(1);

        var result = new Plot2DResult
        {
            Title = "Jacobian Heatmap",
            XLabel = "Column",
            YLabel = "Row"
        };

        if (rows == 0 || cols == 0) return result;

        // Find value range
        double absMax = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                double absVal = System.Math.Abs(jacobian[i, j]);
                if (absVal > absMax) absMax = absVal;
            }
        }
        if (absMax < 1e-15) absMax = 1.0;

        // Create heatmap cells as scatter points with color-coded markers
        var xVals = new double[rows * cols];
        var yVals = new double[rows * cols];
        var colors = new string[rows * cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int idx = i * cols + j;
                xVals[idx] = j;
                yVals[idx] = rows - 1 - i; // flip Y so row 0 is at top
                colors[idx] = GetDivergingColor(jacobian[i, j] / absMax);
            }
        }

        // Plot cells as scatter points (using large markers to simulate heatmap cells)
        result.Points.Add(new Point2DSeries
        {
            Name = "Jacobian Values",
            X = ImmutableArray.Create(xVals),
            Y = ImmutableArray.Create(yVals),
            Color = "#3498DB",
            PointSize = 12.0,
            Marker = "square"
        });

        // Add value annotations
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result.Annotations.Add(new Annotation2D
                {
                    X = j,
                    Y = rows - 1 - i,
                    Text = jacobian[i, j].ToString("F3"),
                    Color = System.Math.Abs(jacobian[i, j]) / absMax > 0.5 ? "#FFFFFF" : "#2C3E50"
                });
            }
        }

        // Grid lines
        for (int i = 0; i <= rows; i++)
        {
            result.Lines.Add(new Line2DSeries
            {
                Name = $"H{i}",
                X = ImmutableArray.Create(-0.5, cols - 0.5),
                Y = ImmutableArray.Create(rows - i - 0.5, rows - i - 0.5),
                Color = "#BDC3C7",
                LineWidth = 0.5
            });
        }

        for (int j = 0; j <= cols; j++)
        {
            result.Lines.Add(new Line2DSeries
            {
                Name = $"V{j}",
                X = ImmutableArray.Create(j - 0.5, j - 0.5),
                Y = ImmutableArray.Create(-0.5, rows - 0.5),
                Color = "#BDC3C7",
                LineWidth = 0.5
            });
        }

        result.XMin = -1;
        result.XMax = cols;
        result.YMin = -1;
        result.YMax = rows;

        // Add matrix statistics annotation
        double frobenius = ComputeFrobeniusNorm(jacobian);
        double det = (rows == cols && rows <= 3) ? ComputeDeterminant(jacobian) : double.NaN;

        string stats = $"||J||_F = {frobenius:F4}";
        if (!double.IsNaN(det))
            stats += $"\ndet(J) = {det:F4}";

        result.Annotations.Add(new Annotation2D
        {
            X = cols * 0.6,
            Y = rows + 0.3,
            Text = stats,
            Color = "#2C3E50"
        });

        return result;
    }

    private static double ComputeFrobeniusNorm(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double sum = 0;

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sum += matrix[i, j] * matrix[i, j];

        return System.Math.Sqrt(sum);
    }

    private static double ComputeDeterminant(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        if (n != matrix.GetLength(1)) return double.NaN;
        if (n == 1) return matrix[0, 0];
        if (n == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
        if (n == 3)
        {
            return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
                 - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
                 + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        }

        // LU-based determinant for larger matrices
        var temp = new double[n, n];
        Array.Copy(matrix, temp, matrix.Length);
        double det = 1.0;

        for (int j = 0; j < n; j++)
        {
            int maxRow = j;
            double maxVal = System.Math.Abs(temp[j, j]);
            for (int i = j + 1; i < n; i++)
            {
                double val = System.Math.Abs(temp[i, j]);
                if (val > maxVal) { maxVal = val; maxRow = i; }
            }

            if (maxVal < 1e-15) return 0;

            if (maxRow != j)
            {
                for (int k = 0; k < n; k++)
                    (temp[j, k], temp[maxRow, k]) = (temp[maxRow, k], temp[j, k]);
                det = -det;
            }

            det *= temp[j, j];

            for (int i = j + 1; i < n; i++)
            {
                double factor = temp[i, j] / temp[j, j];
                for (int k = j + 1; k < n; k++)
                    temp[i, k] -= factor * temp[j, k];
            }
        }

        return det;
    }

    private static string GetDivergingColor(double normalizedValue)
    {
        double t = System.Math.Clamp(normalizedValue, -1.0, 1.0);

        double r, g, b;

        if (t < 0)
        {
            // Blue to white
            double s = -t;
            r = System.Math.Round(255 * (1 - s));
            g = System.Math.Round(255 * (1 - s));
            b = 255;
        }
        else
        {
            // White to red
            double s = t;
            r = 255;
            g = System.Math.Round(255 * (1 - s));
            b = System.Math.Round(255 * (1 - s));
        }

        return $"#{(int)r:X2}{(int)g:X2}{(int)b:X2}";
    }
}
