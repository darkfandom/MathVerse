namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides ARIMA (Autoregressive Integrated Moving Average) model fitting and forecasting.
    /// The ARIMA(p, d, q) model differences the data d times to achieve stationarity,
    /// then fits an ARMA(p, q) model on the differenced series.
    /// </summary>
    public sealed class ARIMAModel
    {
        private double[] _arCoefficients = Array.Empty<double>();
        private double[] _maCoefficients = Array.Empty<double>();
        private double[] _differencedData = Array.Empty<double>();
        private double[] _residuals = Array.Empty<double>();
        private double _noiseVariance;
        private double _mean;
        private double[] _originalData = Array.Empty<double>();
        private int _p;
        private int _d;
        private int _q;

        /// <summary>
        /// Gets the fitted AR coefficients.
        /// </summary>
        public double[] ARCoefficients => _arCoefficients;

        /// <summary>
        /// Gets the fitted MA coefficients.
        /// </summary>
        public double[] MACoefficients => _maCoefficients;

        /// <summary>
        /// Gets the differenced data used for fitting the ARMA model.
        /// </summary>
        public double[] DifferencedData => _differencedData;

        /// <summary>
        /// Gets the estimated noise variance.
        /// </summary>
        public double NoiseVariance => _noiseVariance;

        /// <summary>
        /// Fits an ARIMA(p, d, q) model to the data.
        /// The data is differenced d times, then an ARMA(p, q) model is fitted on the differenced series.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="p">The autoregressive order (must be non-negative).</param>
        /// <param name="d">The degree of differencing (must be non-negative).</param>
        /// <param name="q">The moving average order (must be non-negative).</param>
        /// <returns>A tuple containing the AR coefficients, MA coefficients, and the last d differences used for integration.</returns>
        public (double[] AR, double[] MA, double[] LastDifferences) Fit(double[] data, int p, int d, int q)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (p < 0) throw new ArgumentException("AR order must be non-negative.");
            if (d < 0) throw new ArgumentException("Differencing order must be non-negative.");
            if (q < 0) throw new ArgumentException("MA order must be non-negative.");
            if (p + q == 0 && d == 0)
                throw new ArgumentException("At least one of p, d, or q must be positive.");

            _originalData = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                _originalData[i] = data[i];
            }
            _p = p;
            _d = d;
            _q = q;

            double[] differenced = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                differenced[i] = data[i];
            }

            for (int diff = 0; diff < d; diff++)
            {
                differenced = Difference(differenced);
            }

            _differencedData = new double[differenced.Length];
            for (int i = 0; i < differenced.Length; i++)
            {
                _differencedData[i] = differenced[i];
            }

            if (p + q > 0 && differenced.Length > p + q + 10)
            {
                var arma = new ARMAModel();
                var (ar, ma) = arma.Fit(differenced, p, q);
                _arCoefficients = ar;
                _maCoefficients = ma;

                int n = differenced.Length;
                _mean = 0.0;
                for (int i = 0; i < n; i++)
                {
                    _mean += differenced[i];
                }
                _mean /= n;

                _residuals = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double pred = _mean;
                    for (int j = 0; j < p; j++)
                    {
                        if (i - 1 - j >= 0)
                        {
                            pred += _arCoefficients[j] * (differenced[i - 1 - j] - _mean);
                        }
                    }
                    for (int j = 0; j < q; j++)
                    {
                        if (i - 1 - j >= 0)
                        {
                            pred += _maCoefficients[j] * _residuals[i - 1 - j];
                        }
                    }
                    _residuals[i] = differenced[i] - pred;
                }

                double ssResid = 0.0;
                int startIdx = System.Math.Max(p, q) + 1;
                for (int t = startIdx; t < n; t++)
                {
                    ssResid += _residuals[t] * _residuals[t];
                }
                _noiseVariance = ssResid / (n - startIdx);
            }
            else
            {
                _mean = 0.0;
                for (int i = 0; i < differenced.Length; i++)
                {
                    _mean += differenced[i];
                }
                _mean /= differenced.Length;
                _residuals = new double[differenced.Length];
                _noiseVariance = 0.0;
            }

            return (_arCoefficients, _maCoefficients, GetLastDifferences(data, d));
        }

        /// <summary>
        /// Forecasts future values using the fitted ARIMA model.
        /// Generates ARMA forecasts on the differenced series, then integrates back to the original scale.
        /// Requires a prior call to <see cref="Fit"/>.
        /// </summary>
        /// <param name="steps">The number of future steps to forecast.</param>
        /// <returns>An array of forecasted values in the original scale.</returns>
        public double[] Forecast(int steps)
        {
            if (_originalData.Length == 0)
                throw new InvalidOperationException("Model has not been fitted. Call Fit first.");
            if (steps <= 0) throw new ArgumentException("Steps must be positive.");

            double[] armaForecast;

            if (_p + _q > 0 && _differencedData.Length > _p + _q + 10)
            {
                int n = _differencedData.Length;
                armaForecast = new double[steps];

                for (int h = 0; h < steps; h++)
                {
                    double pred = _mean;
                    for (int j = 0; j < _p; j++)
                    {
                        int idx = n + h - 1 - j;
                        if (idx >= n)
                        {
                            continue;
                        }
                        else if (idx >= 0)
                        {
                            pred += _arCoefficients[j] * (_differencedData[idx] - _mean);
                        }
                    }
                    for (int j = 0; j < _q; j++)
                    {
                        int idx = n + h - 1 - j;
                        if (idx >= n)
                        {
                            continue;
                        }
                        else if (idx >= 0)
                        {
                            pred += _maCoefficients[j] * _residuals[idx];
                        }
                    }
                    armaForecast[h] = pred;
                }
            }
            else
            {
                armaForecast = new double[steps];
                for (int h = 0; h < steps; h++)
                {
                    armaForecast[h] = _mean;
                }
            }

            double[] integrated = Integrate(armaForecast, _originalData, _d);

            return integrated;
        }

        private static double[] Difference(double[] data)
        {
            if (data.Length < 2) return Array.Empty<double>();

            double[] result = new double[data.Length - 1];
            for (int i = 0; i < data.Length - 1; i++)
            {
                result[i] = data[i + 1] - data[i];
            }
            return result;
        }

        private static double[] GetLastDifferences(double[] data, int d)
        {
            double[] current = data;
            for (int i = 0; i < d; i++)
            {
                current = Difference(current);
            }
            return current;
        }

        private static double[] Integrate(double[] forecast, double[] originalData, int d)
        {
            if (d == 0)
            {
                double[] result = new double[forecast.Length];
                for (int i = 0; i < forecast.Length; i++)
                {
                    result[i] = forecast[i];
                }
                return result;
            }

            double[] current = new double[forecast.Length];
            for (int i = 0; i < forecast.Length; i++)
            {
                current[i] = forecast[i];
            }

            for (int diff = 0; diff < d; diff++)
            {
                int nOrig = originalData.Length - diff;
                double baseValue = 0.0;
                if (nOrig > 0)
                {
                    baseValue = originalData[nOrig - 1];
                }

                double[] integrated = new double[current.Length];
                integrated[0] = baseValue + current[0];
                for (int i = 1; i < current.Length; i++)
                {
                    integrated[i] = integrated[i - 1] + current[i];
                }
                current = integrated;
            }

            return current;
        }
    }
}
