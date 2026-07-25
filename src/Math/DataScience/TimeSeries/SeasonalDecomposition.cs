namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Represents the result of an additive seasonal decomposition.
    /// </summary>
    public sealed class DecompositionResult
    {
        /// <summary>
        /// Gets the trend component of the decomposition.
        /// </summary>
        public double[] Trend { get; }

        /// <summary>
        /// Gets the seasonal component of the decomposition.
        /// </summary>
        public double[] Seasonal { get; }

        /// <summary>
        /// Gets the residual (irregular) component of the decomposition.
        /// </summary>
        public double[] Residual { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionResult"/> class.
        /// </summary>
        /// <param name="trend">The trend component.</param>
        /// <param name="seasonal">The seasonal component.</param>
        /// <param name="residual">The residual component.</param>
        public DecompositionResult(double[] trend, double[] seasonal, double[] residual)
        {
            Trend = trend ?? throw new ArgumentNullException(nameof(trend));
            Seasonal = seasonal ?? throw new ArgumentNullException(nameof(seasonal));
            Residual = residual ?? throw new ArgumentNullException(nameof(residual));
        }
    }

    /// <summary>
    /// Performs additive seasonal decomposition of a time series into trend, seasonal, and residual components.
    /// </summary>
    public sealed class SeasonalDecomposition
    {
        /// <summary>
        /// Decomposes a time series using an additive model: data = trend + seasonal + residual.
        /// Uses a centered moving average for trend estimation and averaging within seasonal periods for the seasonal component.
        /// </summary>
        /// <param name="data">The input time series data. Length must be at least 2 * period.</param>
        /// <param name="period">The length of the seasonal cycle (must be positive).</param>
        /// <returns>A <see cref="DecompositionResult"/> containing the trend, seasonal, and residual components.</returns>
        public static DecompositionResult Decompose(double[] data, int period)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (period <= 0) throw new ArgumentException("Period must be positive.");
            if (data.Length < 2 * period)
                throw new ArgumentException($"Data must have at least {2 * period} elements.");

            int n = data.Length;
            double[] trend = new double[n];
            double[] seasonal = new double[n];
            double[] residual = new double[n];

            int half = period / 2;

            if (period % 2 == 0)
            {
                for (int i = half; i < n - half; i++)
                {
                    double sum = 0.0;
                    for (int j = i - half; j < i + half; j++)
                    {
                        sum += data[j];
                    }
                    sum -= data[i - half] * 0.5;
                    sum -= data[i + half] * 0.5;
                    trend[i] = sum / period;
                }
            }
            else
            {
                for (int i = half; i < n - half; i++)
                {
                    double sum = 0.0;
                    for (int j = i - half; j <= i + half; j++)
                    {
                        sum += data[j];
                    }
                    trend[i] = sum / period;
                }
            }

            int startIdx = half + (period % 2 == 0 ? 0 : 0);
            int endIdx = n - half;
            for (int i = 0; i < startIdx; i++)
            {
                trend[i] = trend[startIdx];
            }
            for (int i = endIdx; i < n; i++)
            {
                trend[i] = trend[endIdx - 1];
            }

            double[] seasonalAvg = new double[period];
            int[] seasonalCount = new int[period];
            for (int i = 0; i < n; i++)
            {
                int idx = i % period;
                seasonalAvg[idx] += data[i] - trend[i];
                seasonalCount[idx]++;
            }
            for (int i = 0; i < period; i++)
            {
                seasonalAvg[i] /= seasonalCount[i];
            }

            double seasonalMean = 0.0;
            for (int i = 0; i < period; i++)
            {
                seasonalMean += seasonalAvg[i];
            }
            seasonalMean /= period;

            for (int i = 0; i < n; i++)
            {
                seasonal[i] = seasonalAvg[i % period] - seasonalMean;
            }

            for (int i = 0; i < n; i++)
            {
                residual[i] = data[i] - trend[i] - seasonal[i];
            }

            return new DecompositionResult(trend, seasonal, residual);
        }
    }
}
