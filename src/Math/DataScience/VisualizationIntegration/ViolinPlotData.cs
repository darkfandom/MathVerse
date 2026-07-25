namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a density estimation result for a violin plot visualization.
/// </summary>
public sealed class ViolinPlotData
{
    /// <summary>
    /// Gets or sets the label identifying this violin plot.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the x-axis positions for the density curve.
    /// </summary>
    public double[] Positions { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the density values at each position.
    /// </summary>
    public double[] Density { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the minimum value of the input data.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the input data.
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets the peak density value.
    /// </summary>
    public double PeakDensity { get; set; }

    /// <summary>
    /// Computes a kernel density estimation for a violin plot using a Gaussian kernel.
    /// </summary>
    /// <param name="data">The data values to estimate density from.</param>
    /// <param name="bins">The number of evaluation points for the density curve.</param>
    /// <param name="label">The label for this violin plot.</param>
    /// <returns>A new <see cref="ViolinPlotData"/> instance with the density estimation.</returns>
    public static ViolinPlotData Compute(double[] data, int bins = 50, string label = "")
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length < 2) throw new ArgumentException("Data must contain at least 2 values.", nameof(data));
        if (bins < 2) throw new ArgumentOutOfRangeException(nameof(bins), bins, "Bins must be at least 2.");

        double mean = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            mean += data[i];
        }
        mean /= data.Length;

        double variance = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            double diff = data[i] - mean;
            variance += diff * diff;
        }
        variance /= data.Length;
        double stdDev = System.Math.Sqrt(variance);

        double silverman = stdDev > 0.0
            ? 1.06 * stdDev * System.Math.Pow(data.Length, -0.2)
            : 1.0;

        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] < minVal) minVal = data[i];
            if (data[i] > maxVal) maxVal = data[i];
        }

        double padding = silverman * 2.0;
        double rangeMin = minVal - padding;
        double rangeMax = maxVal + padding;
        double step = (rangeMax - rangeMin) / (bins - 1);

        double[] positions = new double[bins];
        double[] density = new double[bins];
        double peakDensity = 0.0;

        for (int j = 0; j < bins; j++)
        {
            double x = rangeMin + j * step;
            positions[j] = x;

            double sum = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                double u = (x - data[i]) / silverman;
                sum += System.Math.Exp(-0.5 * u * u) / System.Math.Sqrt(2.0 * System.Math.PI);
            }
            density[j] = sum / (data.Length * silverman);

            if (density[j] > peakDensity)
            {
                peakDensity = density[j];
            }
        }

        return new ViolinPlotData
        {
            Label = label,
            Positions = positions,
            Density = density,
            Min = minVal,
            Max = maxVal,
            PeakDensity = peakDensity
        };
    }
}
