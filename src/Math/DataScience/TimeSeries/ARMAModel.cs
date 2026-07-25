namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides ARMA (Autoregressive Moving Average) model fitting and prediction
    /// using the Hannan-Rissanen two-stage estimation method.
    /// </summary>
    public sealed class ARMAModel
    {
        private double[] _arCoefficients = Array.Empty<double>();
        private double[] _maCoefficients = Array.Empty<double>();
        private double[] _residuals = Array.Empty<double>();
        private double _noiseVariance;
        private double _mean;
        private double[] _data = Array.Empty<double>();
        private int _arOrder;
        private int _maOrder;

        /// <summary>
        /// Gets the fitted AR coefficients [phi_1, ..., phi_p].
        /// </summary>
        public double[] ARCoefficients => _arCoefficients;

        /// <summary>
        /// Gets the fitted MA coefficients [theta_1, ..., theta_q].
        /// </summary>
        public double[] MACoefficients => _maCoefficients;

        /// <summary>
        /// Gets the estimated noise variance.
        /// </summary>
        public double NoiseVariance => _noiseVariance;

        /// <summary>
        /// Fits an ARMA(p, q) model to the data using the Hannan-Rissanen method.
        /// Stage 1: Fit a high-order AR model to obtain consistent residual estimates.
        /// Stage 2: Regress the data on both AR lags and residual lags simultaneously.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="arOrder">The autoregressive order p (must be non-negative).</param>
        /// <param name="maOrder">The moving average order q (must be non-negative).</param>
        /// <returns>A tuple containing AR coefficients and MA coefficients.</returns>
        public (double[] AR, double[] MA) Fit(double[] data, int arOrder, int maOrder)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (arOrder < 0) throw new ArgumentException("AR order must be non-negative.");
            if (maOrder < 0) throw new ArgumentException("MA order must be non-negative.");
            if (arOrder + maOrder == 0)
                throw new ArgumentException("At least one of AR or MA order must be positive.");
            if (data.Length <= arOrder + maOrder + 10)
                throw new ArgumentException("Data length is too short for the specified model orders.");

            _data = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                _data[i] = data[i];
            }
            _arOrder = arOrder;
            _maOrder = maOrder;

            _mean = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                _mean += data[i];
            }
            _mean /= data.Length;

            int n = data.Length;
            int arLarge = System.Math.Max(System.Math.Max(arOrder, maOrder) + 5, 10);
            arLarge = System.Math.Min(arLarge, n / 4);

            double[] arLargeCoeffs = ARModel.Fit(data, arLarge);

            double[] initResiduals = new double[n];
            for (int i = 0; i < n; i++)
            {
                double pred = 0.0;
                for (int j = 0; j < arLarge; j++)
                {
                    if (i - 1 - j >= 0)
                    {
                        pred += arLargeCoeffs[j] * data[i - 1 - j];
                    }
                }
                initResiduals[i] = data[i] - pred;
            }

            int totalParams = arOrder + maOrder;
            int startIdx = System.Math.Max(arLarge, arOrder) + 1;

            double[,] XtX = new double[totalParams, totalParams];
            double[] Xty = new double[totalParams];

            for (int t = startIdx; t < n; t++)
            {
                double y = data[t] - _mean;
                double[] row = new double[totalParams];

                for (int j = 0; j < arOrder; j++)
                {
                    row[j] = data[t - 1 - j] - _mean;
                }
                for (int j = 0; j < maOrder; j++)
                {
                    row[arOrder + j] = initResiduals[t - 1 - j];
                }

                for (int j = 0; j < totalParams; j++)
                {
                    Xty[j] += y * row[j];
                    for (int k = 0; k < totalParams; k++)
                    {
                        XtX[j, k] += row[j] * row[k];
                    }
                }
            }

            double[] allCoeffs = SolveLinearSystem(XtX, Xty, totalParams);

            _arCoefficients = new double[arOrder];
            for (int i = 0; i < arOrder; i++)
            {
                _arCoefficients[i] = allCoeffs[i];
            }

            _maCoefficients = new double[maOrder];
            for (int i = 0; i < maOrder; i++)
            {
                _maCoefficients[i] = allCoeffs[arOrder + i];
            }

            _residuals = new double[n];
            for (int i = 0; i < n; i++)
            {
                double pred = _mean;
                for (int j = 0; j < arOrder; j++)
                {
                    if (i - 1 - j >= 0)
                    {
                        pred += _arCoefficients[j] * (data[i - 1 - j] - _mean);
                    }
                }
                for (int j = 0; j < maOrder; j++)
                {
                    if (i - 1 - j >= 0)
                    {
                        pred += _maCoefficients[j] * _residuals[i - 1 - j];
                    }
                }
                _residuals[i] = data[i] - pred;
            }

            double ssResid = 0.0;
            for (int t = startIdx; t < n; t++)
            {
                ssResid += _residuals[t] * _residuals[t];
            }
            _noiseVariance = ssResid / (n - startIdx);

            return (_arCoefficients, _maCoefficients);
        }

        /// <summary>
        /// Forecasts future values using the fitted ARMA model.
        /// Requires a prior call to <see cref="Fit"/>.
        /// </summary>
        /// <param name="steps">The number of future steps to forecast.</param>
        /// <returns>An array of forecasted values of length <paramref name="steps"/>.</returns>
        public double[] Predict(int steps)
        {
            if (_arCoefficients.Length == 0 && _maCoefficients.Length == 0)
                throw new InvalidOperationException("Model has not been fitted. Call Fit first.");
            if (steps <= 0) throw new ArgumentException("Steps must be positive.");

            int n = _data.Length;
            double[] extended = new double[n + steps];
            double[] extendedResiduals = new double[n + steps];

            for (int i = 0; i < n; i++)
            {
                extended[i] = _data[i];
                extendedResiduals[i] = _residuals[i];
            }

            for (int h = 0; h < steps; h++)
            {
                double pred = _mean;
                for (int j = 0; j < _arOrder; j++)
                {
                    pred += _arCoefficients[j] * (extended[n + h - 1 - j] - _mean);
                }
                for (int j = 0; j < _maOrder; j++)
                {
                    int resIdx = n + h - 1 - j;
                    if (resIdx >= 0 && resIdx < n)
                    {
                        pred += _maCoefficients[j] * extendedResiduals[resIdx];
                    }
                }
                extended[n + h] = pred;
                extendedResiduals[n + h] = 0.0;
            }

            double[] result = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                result[i] = extended[n + i];
            }
            return result;
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
                    return new double[n];

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
