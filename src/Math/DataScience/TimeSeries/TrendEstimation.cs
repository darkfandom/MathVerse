namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides methods for estimating trends in time series data.
    /// </summary>
    public sealed class TrendEstimation
    {
        /// <summary>
        /// Estimates a linear trend using ordinary least squares regression.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <returns>A tuple containing the slope and intercept of the fitted linear trend.</returns>
        public static (double Slope, double Intercept) LinearTrend(double[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 2) throw new ArgumentException("Data must have at least 2 elements.");

            int n = data.Length;
            double sumX = 0.0;
            double sumY = 0.0;
            double sumXY = 0.0;
            double sumX2 = 0.0;

            for (int i = 0; i < n; i++)
            {
                double x = i;
                double y = data[i];
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double denom = n * sumX2 - sumX * sumX;
            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        /// <summary>
        /// Estimates a polynomial trend of a given degree using least squares regression with a Vandermonde matrix.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="degree">The degree of the polynomial (must be at least 1).</param>
        /// <returns>The fitted polynomial trend values at each data point.</returns>
        public static double[] PolynomialTrend(double[] data, int degree)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 2) throw new ArgumentException("Data must have at least 2 elements.");
            if (degree < 1) throw new ArgumentException("Degree must be at least 1.");

            int n = data.Length;
            int cols = degree + 1;

            double[] coefficients = FitPolynomialLeastSquares(data, degree);

            double[] result = new double[n];
            for (int i = 0; i < n; i++)
            {
                double val = 0.0;
                double xPow = 1.0;
                for (int d = 0; d <= degree; d++)
                {
                    val += coefficients[d] * xPow;
                    xPow *= i;
                }
                result[i] = val;
            }

            return result;
        }

        /// <summary>
        /// Estimates the trend using a centered moving average with the specified window size.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="window">The window size for the moving average (must be positive and odd for symmetric centering).</param>
        /// <returns>The moving average trend values. Edges are filled with the nearest computed trend value.</returns>
        public static double[] MovingAverageTrend(double[] data, int window)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.");
            if (window <= 0) throw new ArgumentException("Window must be positive.");
            if (window > data.Length) throw new ArgumentException("Window must not exceed data length.");

            int n = data.Length;
            double[] result = new double[n];
            int half = window / 2;

            double[] ma = MovingAverage.SMA(data, window);

            for (int i = 0; i < half && i < n; i++)
            {
                result[i] = ma[0];
            }

            for (int i = half; i < half + ma.Length && i < n; i++)
            {
                result[i] = ma[i - half];
            }

            int fillStart = half + ma.Length;
            if (fillStart < n && ma.Length > 0)
            {
                double lastVal = ma[ma.Length - 1];
                for (int i = fillStart; i < n; i++)
                {
                    result[i] = lastVal;
                }
            }

            return result;
        }

        private static double[] FitPolynomialLeastSquares(double[] data, int degree)
        {
            int n = data.Length;
            int cols = degree + 1;

            double[,] XtX = new double[cols, cols];
            double[] Xty = new double[cols];

            for (int i = 0; i < n; i++)
            {
                double[] row = new double[cols];
                double xPow = 1.0;
                for (int d = 0; d <= degree; d++)
                {
                    row[d] = xPow;
                    xPow *= i;
                }

                for (int j = 0; j < cols; j++)
                {
                    Xty[j] += row[j] * data[i];
                    for (int k = 0; k < cols; k++)
                    {
                        XtX[j, k] += row[j] * row[k];
                    }
                }
            }

            return SolveLinearSystem(XtX, Xty, cols);
        }

        private static double[] SolveLinearSystem(double[,] A, double[] b, int n)
        {
            double[,] augmented = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = A[i, j];
                }
                augmented[i, n] = b[i];
            }

            for (int col = 0; col < n; col++)
            {
                int maxRow = col;
                double maxVal = System.Math.Abs(augmented[col, col]);
                for (int row = col + 1; row < n; row++)
                {
                    double absVal = System.Math.Abs(augmented[row, col]);
                    if (absVal > maxVal)
                    {
                        maxVal = absVal;
                        maxRow = row;
                    }
                }

                if (maxVal < 1e-15)
                    throw new InvalidOperationException("Singular matrix encountered in polynomial fitting.");

                if (maxRow != col)
                {
                    for (int j = 0; j <= n; j++)
                    {
                        double temp = augmented[col, j];
                        augmented[col, j] = augmented[maxRow, j];
                        augmented[maxRow, j] = temp;
                    }
                }

                double pivot = augmented[col, col];
                for (int j = col; j <= n; j++)
                {
                    augmented[col, j] /= pivot;
                }

                for (int row = 0; row < n; row++)
                {
                    if (row == col) continue;
                    double factor = augmented[row, col];
                    for (int j = col; j <= n; j++)
                    {
                        augmented[row, j] -= factor * augmented[col, j];
                    }
                }
            }

            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                x[i] = augmented[i, n];
            }
            return x;
        }
    }
}
