namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Computes kernel density estimates and fits distributions for visualization.
/// </summary>
public sealed class DistributionVisualizer
{
    /// <summary>
    /// Represents the parameters of a fitted normal distribution.
    /// </summary>
    public sealed class NormalFit
    {
        /// <summary>
        /// Gets or sets the estimated mean.
        /// </summary>
        public double Mean { get; set; }

        /// <summary>
        /// Gets or sets the estimated standard deviation.
        /// </summary>
        public double StdDev { get; set; }

        /// <summary>
        /// Gets or sets the number of data points used.
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// Gets or sets the variance.
        /// </summary>
        public double Variance { get; set; }
    }

    /// <summary>
    /// Represents a kernel density estimation result.
    /// </summary>
    public sealed class KDEResult
    {
        /// <summary>
        /// Gets or sets the x-axis evaluation points.
        /// </summary>
        public double[] X { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the density values at each evaluation point.
        /// </summary>
        public double[] Density { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the bandwidth used for the kernel.
        /// </summary>
        public double Bandwidth { get; set; }

        /// <summary>
        /// Gets or sets the number of data points used.
        /// </summary>
        public int SampleCount { get; set; }
    }

    /// <summary>
    /// Fits a normal distribution to the data by estimating mean and standard deviation.
    /// </summary>
    /// <param name="data">The data values to fit.</param>
    /// <returns>A <see cref="NormalFit"/> containing the estimated parameters.</returns>
    public static NormalFit FitNormal(double[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length < 1) throw new ArgumentException("Data must contain at least 1 value.", nameof(data));

        double sum = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }
        double mean = sum / data.Length;

        double m2 = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            double diff = data[i] - mean;
            m2 += diff * diff;
        }

        double variance = data.Length > 1 ? m2 / (data.Length - 1) : 0.0;
        double stdDev = System.Math.Sqrt(variance);

        return new NormalFit
        {
            Mean = mean,
            StdDev = stdDev,
            SampleCount = data.Length,
            Variance = variance
        };
    }

    /// <summary>
    /// Computes a kernel density estimation using a Gaussian kernel.
    /// Silverman's rule is used for automatic bandwidth selection if bandwidth is 0.
    /// </summary>
    /// <param name="data">The data values to estimate density from.</param>
    /// <param name="bandwidth">The kernel bandwidth. If 0, Silverman's rule is applied.</param>
    /// <param name="evaluationPoints">The number of evaluation points (default 200).</param>
    /// <returns>A <see cref="KDEResult"/> containing the density estimation.</returns>
    public static KDEResult KDE(double[] data, double bandwidth = 0, int evaluationPoints = 200)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length < 2) throw new ArgumentException("Data must contain at least 2 values for KDE.", nameof(data));
        if (evaluationPoints < 2) throw new ArgumentOutOfRangeException(nameof(evaluationPoints), evaluationPoints, "Must be at least 2.");

        NormalFit fit = FitNormal(data);
        double stdDev = fit.StdDev;

        if (bandwidth <= 0.0)
        {
            double iqr = ComputeIQR(data);
            double spread = System.Math.Min(stdDev, iqr / 1.34);
            bandwidth = 1.06 * spread * System.Math.Pow(data.Length, -0.2);
            if (bandwidth < 1e-10) bandwidth = 1.0;
        }

        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] < minVal) minVal = data[i];
            if (data[i] > maxVal) maxVal = data[i];
        }

        double padding = bandwidth * 4.0;
        double rangeMin = minVal - padding;
        double rangeMax = maxVal + padding;
        double step = (rangeMax - rangeMin) / (evaluationPoints - 1);

        double[] x = new double[evaluationPoints];
        double[] density = new double[evaluationPoints];

        double sqrt2Pi = System.Math.Sqrt(2.0 * System.Math.PI);

        for (int j = 0; j < evaluationPoints; j++)
        {
            double xi = rangeMin + j * step;
            x[j] = xi;

            double sum = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                double u = (xi - data[i]) / bandwidth;
                sum += System.Math.Exp(-0.5 * u * u);
            }
            density[j] = sum / (data.Length * bandwidth * sqrt2Pi);
        }

        return new KDEResult
        {
            X = x,
            Density = density,
            Bandwidth = bandwidth,
            SampleCount = data.Length
        };
    }

    /// <summary>
    /// Computes the interquartile range of a data array.
    /// </summary>
    /// <param name="data">The data array.</param>
    /// <returns>The interquartile range (Q3 - Q1).</returns>
    private static double ComputeIQR(double[] data)
    {
        double[] sorted = (double[])data.Clone();
        Array.Sort(sorted);

        double q1 = Interpolate(sorted, 0.25);
        double q3 = Interpolate(sorted, 0.75);
        return q3 - q1;
    }

    /// <summary>
    /// Interpolates a quantile value from a sorted array.
    /// </summary>
    /// <param name="sortedValues">The sorted data array.</param>
    /// <param name="quantile">The quantile as a fraction (0-1).</param>
    /// <returns>The interpolated value.</returns>
    private static double Interpolate(double[] sortedValues, double quantile)
    {
        double index = quantile * (sortedValues.Length - 1);
        int lower = (int)System.Math.Floor(index);
        int upper = (int)System.Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        double fraction = index - lower;
        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }
}
