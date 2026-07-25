namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides simple, double, and triple (Holt-Winters) exponential smoothing methods.
    /// </summary>
    public sealed class ExponentialSmoothing
    {
        /// <summary>
        /// Applies Simple Exponential Smoothing (SES) to the data.
        /// Best suited for data with no trend or seasonality.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="alpha">The smoothing parameter in the range (0, 1).</param>
        /// <returns>The smoothed level values.</returns>
        public static double[] SimpleExponential(double[] data, double alpha)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.");
            if (alpha <= 0.0 || alpha >= 1.0)
                throw new ArgumentException("Alpha must be in the range (0, 1).");

            double[] result = new double[data.Length];
            result[0] = data[0];

            for (int i = 1; i < data.Length; i++)
            {
                result[i] = alpha * data[i] + (1.0 - alpha) * result[i - 1];
            }

            return result;
        }

        /// <summary>
        /// Applies Double Exponential Smoothing (Holt's linear method) to the data.
        /// Handles data with a trend component.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="alpha">The level smoothing parameter in the range (0, 1).</param>
        /// <param name="beta">The trend smoothing parameter in the range (0, 1).</param>
        /// <returns>An array of smoothed values (level + trend) for each time step.</returns>
        public static double[] DoubleExponential(double[] data, double alpha, double beta)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 2)
                throw new ArgumentException("Data must have at least 2 elements for double exponential smoothing.");
            if (alpha <= 0.0 || alpha >= 1.0)
                throw new ArgumentException("Alpha must be in the range (0, 1).");
            if (beta <= 0.0 || beta >= 1.0)
                throw new ArgumentException("Beta must be in the range (0, 1).");

            double[] result = new double[data.Length];
            double level = data[0];
            double trend = data[1] - data[0];

            result[0] = level + trend;

            for (int i = 1; i < data.Length; i++)
            {
                double prevLevel = level;
                level = alpha * data[i] + (1.0 - alpha) * (prevLevel + trend);
                trend = beta * (level - prevLevel) + (1.0 - beta) * trend;
                result[i] = level + trend;
            }

            return result;
        }

        /// <summary>
        /// Applies Triple Exponential Smoothing (Holt-Winters additive method) to the data.
        /// Handles data with both trend and seasonality.
        /// </summary>
        /// <param name="data">The input time series data. Must have at least 2 * period elements.</param>
        /// <param name="alpha">The level smoothing parameter in the range (0, 1).</param>
        /// <param name="beta">The trend smoothing parameter in the range (0, 1).</param>
        /// <param name="gamma">The seasonal smoothing parameter in the range (0, 1).</param>
        /// <param name="period">The length of the seasonal cycle (must be positive).</param>
        /// <returns>An array of smoothed values for each time step.</returns>
        public static double[] TripleExponential(double[] data, double alpha, double beta, double gamma, int period)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (period <= 0) throw new ArgumentException("Period must be positive.");
            if (data.Length < 2 * period)
                throw new ArgumentException($"Data must have at least {2 * period} elements for triple exponential smoothing.");
            if (alpha <= 0.0 || alpha >= 1.0)
                throw new ArgumentException("Alpha must be in the range (0, 1).");
            if (beta <= 0.0 || beta >= 1.0)
                throw new ArgumentException("Beta must be in the range (0, 1).");
            if (gamma <= 0.0 || gamma >= 1.0)
                throw new ArgumentException("Gamma must be in the range (0, 1).");

            double[] result = new double[data.Length];

            double level = 0.0;
            for (int i = 0; i < period; i++)
            {
                level += data[i];
            }
            level /= period;

            double trend = 0.0;
            for (int i = 0; i < period; i++)
            {
                trend += data[period + i] - data[i];
            }
            trend /= (period * period);

            double[] seasonal = new double[data.Length + period];
            for (int i = 0; i < period; i++)
            {
                seasonal[i] = data[i] - level;
            }

            for (int i = 0; i < System.Math.Min(period, data.Length); i++)
            {
                result[i] = level + trend + seasonal[i];
            }

            for (int t = period; t < data.Length; t++)
            {
                double prevLevel = level;
                level = alpha * (data[t] - seasonal[t - period]) + (1.0 - alpha) * (prevLevel + trend);
                trend = beta * (level - prevLevel) + (1.0 - beta) * trend;
                seasonal[t] = gamma * (data[t] - level) + (1.0 - gamma) * seasonal[t - period];
                result[t] = level + trend + seasonal[t];
            }

            return result;
        }
    }
}
