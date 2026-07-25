namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    /// <summary>
    /// Contains the results of a one-way ANOVA test.
    /// </summary>
    public sealed class ANOVAResult
    {
        /// <summary>Gets or sets the F-test statistic (MSB / MSW).</summary>
        public double FStatistic { get; set; }

        /// <summary>Gets or sets the between-groups degrees of freedom (k - 1).</summary>
        public double DegreesOfFreedomBetween { get; set; }

        /// <summary>Gets or sets the within-groups degrees of freedom (N - k).</summary>
        public double DegreesOfFreedomWithin { get; set; }

        /// <summary>Gets or sets the between-groups mean square (SSB / df_between).</summary>
        public double MSB { get; set; }

        /// <summary>Gets or sets the within-groups mean square (SSW / df_within).</summary>
        public double MSW { get; set; }

        /// <summary>Gets or sets the between-groups sum of squares.</summary>
        public double SSB { get; set; }

        /// <summary>Gets or sets the within-groups sum of squares.</summary>
        public double SSW { get; set; }

        /// <summary>Gets or sets the p-value.</summary>
        public double PValue { get; set; }

        /// <summary>Gets or sets whether the result is statistically significant at alpha=0.05.</summary>
        public bool Significant { get; set; }
    }
}