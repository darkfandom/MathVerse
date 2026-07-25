namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the result of a time series forecast including bounds and metrics.
    /// </summary>
    public sealed class ForecastResult
    {
        /// <summary>
        /// Gets the forecasted values.
        /// </summary>
        public double[] Values { get; }

        /// <summary>
        /// Gets the lower confidence bound for each forecasted value.
        /// </summary>
        public double[] LowerBound { get; }

        /// <summary>
        /// Gets the upper confidence bound for each forecasted value.
        /// </summary>
        public double[] UpperBound { get; }

        /// <summary>
        /// Gets the name of the forecasting method used.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the performance metrics associated with the forecast.
        /// </summary>
        public Dictionary<string, double> Metrics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForecastResult"/> class.
        /// </summary>
        /// <param name="values">The forecasted values.</param>
        /// <param name="lowerBound">The lower confidence bounds.</param>
        /// <param name="upperBound">The upper confidence bounds.</param>
        /// <param name="method">The name of the forecasting method.</param>
        /// <param name="metrics">The performance metrics.</param>
        public ForecastResult(double[] values, double[] lowerBound, double[] upperBound, string method, Dictionary<string, double> metrics)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
            LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
            UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }
    }
}
