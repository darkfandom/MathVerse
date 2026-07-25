namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides simple, weighted, and exponential moving average computations.
    /// </summary>
    public sealed class MovingAverage
    {
        /// <summary>
        /// Computes the Simple Moving Average (SMA) using a fixed-size sliding window.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="window">The size of the moving average window.</param>
        /// <returns>An array of SMA values of length <c>data.Length - window + 1</c>.</returns>
        public static double[] SMA(double[] data, int window)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.");
            if (window <= 0) throw new ArgumentException("Window must be positive.");
            if (window > data.Length) throw new ArgumentException("Window must not exceed data length.");

            int resultLength = data.Length - window + 1;
            double[] result = new double[resultLength];

            double sum = 0.0;
            for (int i = 0; i < window; i++)
            {
                sum += data[i];
            }
            result[0] = sum / window;

            for (int i = 1; i < resultLength; i++)
            {
                sum += data[i + window - 1] - data[i - 1];
                result[i] = sum / window;
            }

            return result;
        }

        /// <summary>
        /// Computes the Weighted Moving Average (WMA) using linearly decreasing weights.
        /// The most recent observation receives the highest weight.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="window">The size of the moving average window.</param>
        /// <returns>An array of WMA values of length <c>data.Length - window + 1</c>.</returns>
        public static double[] WMA(double[] data, int window)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.");
            if (window <= 0) throw new ArgumentException("Window must be positive.");
            if (window > data.Length) throw new ArgumentException("Window must not exceed data length.");

            int resultLength = data.Length - window + 1;
            double[] result = new double[resultLength];
            double weightSum = window * (window + 1.0) / 2.0;

            for (int i = 0; i < resultLength; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < window; j++)
                {
                    sum += (j + 1.0) * data[i + j];
                }
                result[i] = sum / weightSum;
            }

            return result;
        }

        /// <summary>
        /// Computes the Exponential Moving Average (EMA) with a given smoothing factor.
        /// The first value of the output equals the first input value.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="alpha">The smoothing factor in the range (0, 1]. Values closer to 1 give more weight to recent observations.</param>
        /// <returns>An array of EMA values with the same length as the input data.</returns>
        public static double[] EMA(double[] data, double alpha)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must not be empty.");
            if (alpha <= 0.0 || alpha > 1.0)
                throw new ArgumentException("Alpha must be in the range (0, 1].");

            double[] result = new double[data.Length];
            result[0] = data[0];

            for (int i = 1; i < data.Length; i++)
            {
                result[i] = alpha * data[i] + (1.0 - alpha) * result[i - 1];
            }

            return result;
        }
    }
}
