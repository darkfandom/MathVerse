namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides moving average (MA) model fitting and prediction.
    /// Uses Durbin's two-step method: fits a high-order AR model, computes residuals,
    /// then regresses the data on lagged residuals to estimate MA coefficients.
    /// </summary>
    public sealed class MAModel
    {
        private double[] _coefficients = Array.Empty<double>();
        private double[] _residuals = Array.Empty<double>();
        private double _noiseVariance;
        private double _mean;
        private double[] _data = Array.Empty<double>();
        private int _order;

        /// <summary>
        /// Gets the fitted MA coefficients [theta_1, theta_2, ..., theta_q].
        /// </summary>
        public double[] Coefficients => _coefficients;

        /// <summary>
        /// Gets the estimated noise variance.
        /// </summary>
        public double NoiseVariance => _noiseVariance;

        /// <summary>
        /// Fits an MA(q) model to the data using Durbin's method.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="order">The order of the moving average model (must be positive).</param>
        /// <returns>The fitted MA coefficients [theta_1, ..., theta_q].</returns>
        public double[] Fit(double[] data, int order)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (order <= 0) throw new ArgumentException("Order must be positive.");
            if (data.Length <= order + 1)
                throw new ArgumentException("Data length must be greater than order + 1.");

            _data = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                _data[i] = data[i];
            }
            _order = order;

            _mean = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                _mean += data[i];
            }
            _mean /= data.Length;

            int arOrder = System.Math.Max(2 * order + 5, order + 10);
            arOrder = System.Math.Min(arOrder, data.Length / 3);

            double[] arCoeffs = ARModel.Fit(data, arOrder);

            _residuals = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                double pred = 0.0;
                for (int j = 0; j < arOrder; j++)
                {
                    if (i - 1 - j >= 0)
                    {
                        pred += arCoeffs[j] * data[i - 1 - j];
                    }
                }
                _residuals[i] = data[i] - pred;
            }

            int startIdx = arOrder;
            int regressionLen = data.Length - startIdx;
            if (regressionLen <= order)
            {
                _coefficients = new double[order];
                _noiseVariance = 0.0;
                return _coefficients;
            }

            double[,] XtX = new double[order, order];
            double[] Xty = new double[order];

            for (int t = startIdx; t < data.Length; t++)
            {
                for (int j = 0; j < order; j++)
                {
                    double residLag = _residuals[t - 1 - j];
                    Xty[j] += (data[t] - _mean) * residLag;
                    for (int k = 0; k < order; k++)
                    {
                        XtX[j, k] += residLag * _residuals[t - 1 - k];
                    }
                }
            }

            _coefficients = SolveLinearSystem(XtX, Xty, order);

            double ssResid = 0.0;
            for (int t = startIdx; t < data.Length; t++)
            {
                double pred = _mean;
                for (int j = 0; j < order; j++)
                {
                    if (t - 1 - j >= 0)
                    {
                        pred += _coefficients[j] * _residuals[t - 1 - j];
                    }
                }
                double resid = data[t] - pred;
                ssResid += resid * resid;
            }
            _noiseVariance = ssResid / regressionLen;

            return _coefficients;
        }

        /// <summary>
        /// Forecasts future values using the fitted MA model.
        /// Requires a prior call to <see cref="Fit"/>.
        /// </summary>
        /// <param name="steps">The number of future steps to forecast.</param>
        /// <returns>An array of forecasted values. Future noise terms are assumed zero beyond the fitted residuals.</returns>
        public double[] Predict(int steps)
        {
            if (_coefficients.Length == 0)
                throw new InvalidOperationException("Model has not been fitted. Call Fit first.");
            if (steps <= 0) throw new ArgumentException("Steps must be positive.");

            int n = _data.Length;
            double[] predictions = new double[steps];

            for (int h = 0; h < steps; h++)
            {
                double pred = _mean;
                for (int j = 0; j < _order; j++)
                {
                    int idx = n + h - 1 - j;
                    if (idx >= n)
                    {
                        continue;
                    }
                    else if (idx >= 0)
                    {
                        pred += _coefficients[j] * _residuals[idx];
                    }
                }
                predictions[h] = pred;
            }

            return predictions;
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
