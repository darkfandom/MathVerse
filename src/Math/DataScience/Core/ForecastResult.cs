namespace MathVerse.Math.DataScience.Core;

using System;

/// <summary>
/// Result of a forecasting operation.
/// </summary>
public sealed class ForecastResult
{
    /// <summary>
    /// Gets or sets the forecasted values.
    /// </summary>
    public double[] Values { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the lower bound of the confidence interval.
    /// </summary>
    public double[] LowerBound { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the upper bound of the confidence interval.
    /// </summary>
    public double[] UpperBound { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Gets or sets the forecast horizon.
    /// </summary>
    public int Horizon { get; set; }

    /// <summary>
    /// Gets or sets the method used for forecasting.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="ForecastResult"/> instance.
    /// </summary>
    /// <param name="values">The forecasted values.</param>
    /// <param name="horizon">The forecast horizon.</param>
    /// <returns>A new forecast result.</returns>
    public static ForecastResult Create(double[] values, int horizon)
    {
        return new ForecastResult
        {
            Values = values,
            Horizon = horizon,
            LowerBound = values,
            UpperBound = values
        };
    }
}