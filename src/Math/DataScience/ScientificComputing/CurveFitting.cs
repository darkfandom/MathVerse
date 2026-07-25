namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Represents the result of a curve fit including coefficients, R^2, and residuals.
    /// </summary>
    public sealed class CurveFitResult
    {
        /// <summary>
        /// Gets the fitted coefficients.
        /// </summary>
        public double[] Coefficients { get; }

        /// <summary>
        /// Gets the coefficient of determination R^2.
        /// </summary>
        public double R2 { get; }

        /// <summary>
        /// Gets the residual values (observed - predicted).
        /// </summary>
        public double[] Residuals { get; }

        /// <summary>
        /// Gets the root mean square error.
        /// </summary>
        public double RMSE { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveFitResult"/> class.
        /// </summary>
        public CurveFitResult(double[] coefficients, double r2, double[] residuals, double rmse)
        {
            Coefficients = coefficients;
            R2 = r2;
            Residuals = residuals;
            RMSE = rmse;
        }
    }

    /// <summary>
    /// Provides curve fitting methods for linear, polynomial, exponential, power, and logarithmic models.
    /// All fits use least squares regression (direct or linearized).
    /// </summary>
    public sealed class CurveFitting
    {
        /// <summary>
        /// Fits a linear model y = slope * x + intercept to the data using ordinary least squares.
        /// </summary>
        /// <param name="x">The independent variable values.</param>
        /// <param name="y">The dependent variable values.</param>
        /// <returns>A <see cref="CurveFitResult"/> with slope (first coefficient), intercept (second), R^2, and residuals.</returns>
        public static CurveFitResult LinearFit(double[] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length) throw new ArgumentException("x and y must have the same length.");
            if (x.Length < 2) throw new ArgumentException("At least 2 data points are required.");

            int n = x.Length;
            double sumX = 0.0, sumY = 0.0, sumXY = 0.0, sumX2 = 0.0;
            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
            }

            double denom = n * sumX2 - sumX * sumX;
            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double meanY = sumY / n;
            for (int i = 0; i < n; i++)
            {
                double predicted = slope * x[i] + intercept;
                residuals[i] = y[i] - predicted;
                ssRes += residuals[i] * residuals[i];
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);

            return new CurveFitResult(new double[] { slope, intercept }, r2, residuals, rmse);
        }

        /// <summary>
        /// Fits a polynomial model of the specified degree to the data using least squares with a Vandermonde matrix.
        /// </summary>
        /// <param name="x">The independent variable values.</param>
        /// <param name="y">The dependent variable values.</param>
        /// <param name="degree">The degree of the polynomial (must be at least 1).</param>
        /// <returns>A <see cref="CurveFitResult"/> with polynomial coefficients in ascending order of degree.</returns>
        public static CurveFitResult PolynomialFit(double[] x, double[] y, int degree)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length) throw new ArgumentException("x and y must have the same length.");
            if (degree < 1) throw new ArgumentException("Degree must be at least 1.");
            if (x.Length < degree + 1)
                throw new ArgumentException($"At least {degree + 1} data points are required for degree {degree}.");

            int n = x.Length;
            int cols = degree + 1;

            double[,] XtX = new double[cols, cols];
            double[] Xty = new double[cols];

            for (int i = 0; i < n; i++)
            {
                double xPow = 1.0;
                for (int j = 0; j < cols; j++)
                {
                    Xty[j] += xPow * y[i];
                    double xPow2 = 1.0;
                    for (int k = 0; k < cols; k++)
                    {
                        XtX[j, k] += xPow * xPow2;
                        xPow2 *= x[i];
                    }
                    xPow *= x[i];
                }
            }

            double[] coefficients = SolveLinearSystem(XtX, Xty, cols);

            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double meanY = 0.0;
            for (int i = 0; i < n; i++) meanY += y[i];
            meanY /= n;

            for (int i = 0; i < n; i++)
            {
                double predicted = 0.0;
                double xPow = 1.0;
                for (int j = 0; j < cols; j++)
                {
                    predicted += coefficients[j] * xPow;
                    xPow *= x[i];
                }
                residuals[i] = y[i] - predicted;
                ssRes += residuals[i] * residuals[i];
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);

            return new CurveFitResult(coefficients, r2, residuals, rmse);
        }

        /// <summary>
        /// Fits an exponential model y = a * exp(b * x) by linearizing as ln(y) = ln(a) + b*x.
        /// All y values must be positive.
        /// </summary>
        /// <param name="x">The independent variable values.</param>
        /// <param name="y">The dependent variable values (must all be positive).</param>
        /// <returns>A <see cref="CurveFitResult"/> with coefficients [a, b].</returns>
        public static CurveFitResult ExponentialFit(double[] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length) throw new ArgumentException("x and y must have the same length.");
            if (x.Length < 2) throw new ArgumentException("At least 2 data points are required.");

            double[] logY = new double[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                if (y[i] <= 0)
                    throw new ArgumentException($"y[{i}] must be positive for exponential fit.");
                logY[i] = System.Math.Log(y[i]);
            }

            var linearResult = LinearFit(x, logY);

            double a = System.Math.Exp(linearResult.Coefficients[1]);
            double b = linearResult.Coefficients[0];

            int n = x.Length;
            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double meanY = 0.0;
            for (int i = 0; i < n; i++) meanY += y[i];
            meanY /= n;

            for (int i = 0; i < n; i++)
            {
                double predicted = a * System.Math.Exp(b * x[i]);
                residuals[i] = y[i] - predicted;
                ssRes += residuals[i] * residuals[i];
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);

            return new CurveFitResult(new double[] { a, b }, r2, residuals, rmse);
        }

        /// <summary>
        /// Fits a power model y = a * x^b by linearizing as ln(y) = ln(a) + b*ln(x).
        /// All x and y values must be positive.
        /// </summary>
        /// <param name="x">The independent variable values (must all be positive).</param>
        /// <param name="y">The dependent variable values (must all be positive).</param>
        /// <returns>A <see cref="CurveFitResult"/> with coefficients [a, b].</returns>
        public static CurveFitResult PowerFit(double[] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length) throw new ArgumentException("x and y must have the same length.");
            if (x.Length < 2) throw new ArgumentException("At least 2 data points are required.");

            double[] logX = new double[x.Length];
            double[] logY = new double[y.Length];
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] <= 0) throw new ArgumentException($"x[{i}] must be positive for power fit.");
                if (y[i] <= 0) throw new ArgumentException($"y[{i}] must be positive for power fit.");
                logX[i] = System.Math.Log(x[i]);
                logY[i] = System.Math.Log(y[i]);
            }

            var linearResult = LinearFit(logX, logY);

            double a = System.Math.Exp(linearResult.Coefficients[1]);
            double b = linearResult.Coefficients[0];

            int n = x.Length;
            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double meanY = 0.0;
            for (int i = 0; i < n; i++) meanY += y[i];
            meanY /= n;

            for (int i = 0; i < n; i++)
            {
                double predicted = a * System.Math.Pow(x[i], b);
                residuals[i] = y[i] - predicted;
                ssRes += residuals[i] * residuals[i];
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);

            return new CurveFitResult(new double[] { a, b }, r2, residuals, rmse);
        }

        /// <summary>
        /// Fits a logarithmic model y = a + b * ln(x) using least squares on the transformed variable.
        /// All x values must be positive.
        /// </summary>
        /// <param name="x">The independent variable values (must all be positive).</param>
        /// <param name="y">The dependent variable values.</param>
        /// <returns>A <see cref="CurveFitResult"/> with coefficients [a, b].</returns>
        public static CurveFitResult LogarithmicFit(double[] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length) throw new ArgumentException("x and y must have the same length.");
            if (x.Length < 2) throw new ArgumentException("At least 2 data points are required.");

            double[] logX = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] <= 0) throw new ArgumentException($"x[{i}] must be positive for logarithmic fit.");
                logX[i] = System.Math.Log(x[i]);
            }

            var linearResult = LinearFit(logX, y);

            double a = linearResult.Coefficients[1];
            double b = linearResult.Coefficients[0];

            int n = x.Length;
            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double meanY = 0.0;
            for (int i = 0; i < n; i++) meanY += y[i];
            meanY /= n;

            for (int i = 0; i < n; i++)
            {
                double predicted = a + b * System.Math.Log(x[i]);
                residuals[i] = y[i] - predicted;
                ssRes += residuals[i] * residuals[i];
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);

            return new CurveFitResult(new double[] { a, b }, r2, residuals, rmse);
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
                    throw new InvalidOperationException("Singular matrix encountered in curve fitting.");

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
