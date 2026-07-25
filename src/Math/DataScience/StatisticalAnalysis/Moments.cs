namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Computes statistical moments of various orders for datasets.
    /// </summary>
    public sealed class Moments
    {
        /// <summary>
        /// Computes the k-th central moment of the data.
        /// Central moment = E[(X - mean)^k].
        /// </summary>
        /// <param name="data">The data array to analyze.</param>
        /// <param name="order">The order of the central moment (must be >= 1).</param>
        /// <returns>The k-th central moment.</returns>
        public static double CentralMoment(double[] data, int order)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (order < 1) throw new ArgumentException("Order must be at least 1.", nameof(order));

            int n = data.Length;
            if (n == 0) return double.NaN;

            double sum = 0.0;
            foreach (double v in data) sum += v;
            double mean = sum / n;

            double moment = 0.0;
            foreach (double v in data)
            {
                double diff = v - mean;
                moment += System.Math.Pow(diff, order);
            }

            return moment / n;
        }

        /// <summary>
        /// Computes the k-th raw moment of the data.
        /// Raw moment = E[X^k].
        /// </summary>
        /// <param name="data">The data array to analyze.</param>
        /// <param name="order">The order of the raw moment (must be >= 1).</param>
        /// <returns>The k-th raw moment.</returns>
        public static double RawMoment(double[] data, int order)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (order < 1) throw new ArgumentException("Order must be at least 1.", nameof(order));

            int n = data.Length;
            if (n == 0) return double.NaN;

            double moment = 0.0;
            foreach (double v in data)
            {
                moment += System.Math.Pow(v, order);
            }

            return moment / n;
        }

        /// <summary>
        /// Computes the k-th standardized moment of the data.
        /// Standardized moment = E[((X - mean) / stddev)^k].
        /// </summary>
        /// <param name="data">The data array to analyze.</param>
        /// <param name="order">The order of the standardized moment (must be >= 1).</param>
        /// <returns>The k-th standardized moment.</returns>
        public static double StandardizedMoment(double[] data, int order)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (order < 1) throw new ArgumentException("Order must be at least 1.", nameof(order));

            int n = data.Length;
            if (n == 0) return double.NaN;

            double sum = 0.0;
            foreach (double v in data) sum += v;
            double mean = sum / n;

            double variance = 0.0;
            foreach (double v in data)
            {
                double diff = v - mean;
                variance += diff * diff;
            }
            double stdDev = System.Math.Sqrt(variance / n);

            if (stdDev == 0.0) return double.NaN;

            double moment = 0.0;
            double invStd = 1.0 / stdDev;
            foreach (double v in data)
            {
                double z = (v - mean) * invStd;
                moment += System.Math.Pow(z, order);
            }

            return moment / n;
        }
    }
}