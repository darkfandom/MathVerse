namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    /// <summary>
    /// Contains the results of a chi-square test.
    /// </summary>
    public sealed class ChiSquareResult
    {
        /// <summary>Gets or sets the chi-square test statistic.</summary>
        public double ChiSquare { get; set; }

        /// <summary>Gets or sets the degrees of freedom.</summary>
        public int DegreesOfFreedom { get; set; }

        /// <summary>Gets or sets the p-value.</summary>
        public double PValue { get; set; }

        /// <summary>Gets or sets whether the result is statistically significant at alpha=0.05.</summary>
        public bool Significant { get; set; }
    }
}