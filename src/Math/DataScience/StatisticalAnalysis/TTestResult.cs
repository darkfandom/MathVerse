namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    /// <summary>
    /// Contains the results of a t-test.
    /// </summary>
    public sealed class TTestResult
    {
        /// <summary>Gets or sets the t-test statistic.</summary>
        public double TStatistic { get; set; }

        /// <summary>Gets or sets the two-tailed p-value.</summary>
        public double PValue { get; set; }

        /// <summary>Gets or sets the degrees of freedom.</summary>
        public double DegreesOfFreedom { get; set; }

        /// <summary>Gets or sets whether the result is statistically significant at alpha=0.05.</summary>
        public bool Significant { get; set; }

        /// <summary>Gets or sets the mean of the first sample.</summary>
        public double Mean1 { get; set; }

        /// <summary>Gets or sets the mean of the second sample (if applicable).</summary>
        public double Mean2 { get; set; }

        /// <summary>Gets or sets the standard deviation of the first sample.</summary>
        public double StdDev1 { get; set; }

        /// <summary>Gets or sets the standard deviation of the second sample (if applicable).</summary>
        public double StdDev2 { get; set; }
    }
}