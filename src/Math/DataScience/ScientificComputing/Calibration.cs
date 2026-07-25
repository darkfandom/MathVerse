namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Represents the result of a linear calibration.
    /// </summary>
    public sealed class CalibrationResult
    {
        /// <summary>
        /// Gets the slope of the calibration line.
        /// </summary>
        public double Slope { get; }

        /// <summary>
        /// Gets the intercept of the calibration line.
        /// </summary>
        public double Intercept { get; }

        /// <summary>
        /// Gets the coefficient of determination R^2.
        /// </summary>
        public double R2 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationResult"/> class.
        /// </summary>
        public CalibrationResult(double slope, double intercept, double r2)
        {
            Slope = slope;
            Intercept = intercept;
            R2 = r2;
        }
    }

    /// <summary>
    /// Provides linear and polynomial calibration methods for relating measured values to reference standards.
    /// </summary>
    public sealed class Calibration
    {
        /// <summary>
        /// Performs a linear calibration: measured = slope * reference + intercept.
        /// Uses ordinary least squares regression on reference vs. measured values.
        /// </summary>
        /// <param name="referenceValues">The known reference (true) values.</param>
        /// <param name="measuredValues">The instrument/measured values corresponding to each reference.</param>
        /// <returns>A <see cref="CalibrationResult"/> with the slope, intercept, and R^2.</returns>
        public static CalibrationResult LinearCalibration(double[] referenceValues, double[] measuredValues)
        {
            if (referenceValues == null) throw new ArgumentNullException(nameof(referenceValues));
            if (measuredValues == null) throw new ArgumentNullException(nameof(measuredValues));
            if (referenceValues.Length != measuredValues.Length)
                throw new ArgumentException("Reference and measured arrays must have the same length.");
            if (referenceValues.Length < 2)
                throw new ArgumentException("At least 2 data points are required for linear calibration.");

            int n = referenceValues.Length;

            double sumX = 0.0, sumY = 0.0, sumXY = 0.0, sumX2 = 0.0;
            for (int i = 0; i < n; i++)
            {
                sumX += referenceValues[i];
                sumY += measuredValues[i];
                sumXY += referenceValues[i] * measuredValues[i];
                sumX2 += referenceValues[i] * referenceValues[i];
            }

            double denom = n * sumX2 - sumX * sumX;
            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            double meanY = sumY / n;
            double ssTot = 0.0, ssRes = 0.0;
            for (int i = 0; i < n; i++)
            {
                double predicted = slope * referenceValues[i] + intercept;
                ssRes += (measuredValues[i] - predicted) * (measuredValues[i] - predicted);
                ssTot += (measuredValues[i] - meanY) * (measuredValues[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;

            return new CalibrationResult(slope, intercept, r2);
        }

        /// <summary>
        /// Performs a polynomial calibration of the specified degree using least squares regression.
        /// </summary>
        /// <param name="referenceValues">The known reference (true) values.</param>
        /// <param name="measuredValues">The instrument/measured values corresponding to each reference.</param>
        /// <param name="degree">The polynomial degree (must be at least 1).</param>
        /// <returns>An array of polynomial coefficients [c0, c1, ..., c_degree] such that measured = c0 + c1*x + c2*x^2 + ...</returns>
        public static double[] PolynomialCalibration(double[] referenceValues, double[] measuredValues, int degree)
        {
            if (referenceValues == null) throw new ArgumentNullException(nameof(referenceValues));
            if (measuredValues == null) throw new ArgumentNullException(nameof(measuredValues));
            if (referenceValues.Length != measuredValues.Length)
                throw new ArgumentException("Reference and measured arrays must have the same length.");
            if (degree < 1) throw new ArgumentException("Degree must be at least 1.");
            if (referenceValues.Length < degree + 1)
                throw new ArgumentException($"At least {degree + 1} data points are required for degree {degree} polynomial calibration.");

            int n = referenceValues.Length;
            int cols = degree + 1;

            double[,] XtX = new double[cols, cols];
            double[] Xty = new double[cols];

            for (int i = 0; i < n; i++)
            {
                double xPow = 1.0;
                for (int j = 0; j < cols; j++)
                {
                    Xty[j] += xPow * measuredValues[i];
                    double xPow2 = 1.0;
                    for (int k = 0; k < cols; k++)
                    {
                        XtX[j, k] += xPow * xPow2;
                        xPow2 *= referenceValues[i];
                    }
                    xPow *= referenceValues[i];
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
                    throw new InvalidOperationException("Singular matrix encountered in calibration.");

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
