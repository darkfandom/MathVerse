namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides utility methods for evaluating forecast accuracy.
    /// </summary>
    public sealed class ForecastUtilities
    {
        /// <summary>
        /// Computes the Mean Absolute Error between actual and forecast values.
        /// </summary>
        /// <param name="actual">The actual observed values.</param>
        /// <param name="forecast">The forecasted values.</param>
        /// <returns>The mean absolute error.</returns>
        public static double MeanAbsoluteError(double[] actual, double[] forecast)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));
            if (actual.Length != forecast.Length)
                throw new ArgumentException("Arrays must have the same length.");
            if (actual.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double sum = 0.0;
            for (int i = 0; i < actual.Length; i++)
            {
                sum += System.Math.Abs(actual[i] - forecast[i]);
            }
            return sum / actual.Length;
        }

        /// <summary>
        /// Computes the Mean Squared Error between actual and forecast values.
        /// </summary>
        /// <param name="actual">The actual observed values.</param>
        /// <param name="forecast">The forecasted values.</param>
        /// <returns>The mean squared error.</returns>
        public static double MeanSquaredError(double[] actual, double[] forecast)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));
            if (actual.Length != forecast.Length)
                throw new ArgumentException("Arrays must have the same length.");
            if (actual.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double sum = 0.0;
            for (int i = 0; i < actual.Length; i++)
            {
                double diff = actual[i] - forecast[i];
                sum += diff * diff;
            }
            return sum / actual.Length;
        }

        /// <summary>
        /// Computes the Mean Absolute Percentage Error between actual and forecast values.
        /// Skips entries where the actual value is zero.
        /// </summary>
        /// <param name="actual">The actual observed values.</param>
        /// <param name="forecast">The forecasted values.</param>
        /// <returns>The mean absolute percentage error as a percentage.</returns>
        public static double MeanAbsolutePercentageError(double[] actual, double[] forecast)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));
            if (actual.Length != forecast.Length)
                throw new ArgumentException("Arrays must have the same length.");
            if (actual.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double sum = 0.0;
            int count = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != 0.0)
                {
                    sum += System.Math.Abs((actual[i] - forecast[i]) / actual[i]);
                    count++;
                }
            }

            if (count == 0)
                throw new ArgumentException("All actual values are zero; MAPE is undefined.");

            return (sum / count) * 100.0;
        }

        /// <summary>
        /// Computes the Symmetric Mean Absolute Percentage Error between actual and forecast values.
        /// </summary>
        /// <param name="actual">The actual observed values.</param>
        /// <param name="forecast">The forecasted values.</param>
        /// <returns>The symmetric mean absolute percentage error as a percentage.</returns>
        public static double SymmetricMeanAbsolutePercentageError(double[] actual, double[] forecast)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (forecast == null) throw new ArgumentNullException(nameof(forecast));
            if (actual.Length != forecast.Length)
                throw new ArgumentException("Arrays must have the same length.");
            if (actual.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double sum = 0.0;
            int count = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                double denom = System.Math.Abs(actual[i]) + System.Math.Abs(forecast[i]);
                if (denom != 0.0)
                {
                    sum += System.Math.Abs(actual[i] - forecast[i]) / denom;
                    count++;
                }
            }

            if (count == 0)
                throw new ArgumentException("All values are zero; SMAPE is undefined.");

            return (sum / count) * 200.0;
        }
    }
}
