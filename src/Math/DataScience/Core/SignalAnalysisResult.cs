namespace MathVerse.Math.DataScience.Core;

using System;

/// <summary>
/// Result of a signal analysis operation.
/// </summary>
public sealed class SignalAnalysisResult
{
    /// <summary>
    /// Gets or sets the magnitude spectrum.
    /// </summary>
    public double[] Magnitude { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the phase spectrum.
    /// </summary>
    public double[] Phase { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the frequency axis values.
    /// </summary>
    public double[] Frequencies { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the power spectral density.
    /// </summary>
    public double[] PowerSpectralDensity { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the dominant frequency.
    /// </summary>
    public double DominantFrequency { get; set; }

    /// <summary>
    /// Gets or sets the total signal energy.
    /// </summary>
    public double Energy { get; set; }

    /// <summary>
    /// Gets or sets the root mean square value.
    /// </summary>
    public double Rms { get; set; }

    /// <summary>
    /// Creates a new <see cref="SignalAnalysisResult"/> instance.
    /// </summary>
    /// <param name="magnitude">The magnitude spectrum.</param>
    /// <param name="phase">The phase spectrum.</param>
    /// <param name="frequencies">The frequency axis values.</param>
    /// <returns>A new signal analysis result.</returns>
    public static SignalAnalysisResult Create(double[] magnitude, double[] phase, double[] frequencies)
    {
        return new SignalAnalysisResult
        {
            Magnitude = magnitude,
            Phase = phase,
            Frequencies = frequencies,
            PowerSpectralDensity = magnitude
        };
    }
}