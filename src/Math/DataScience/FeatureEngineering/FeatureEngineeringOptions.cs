namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System.Collections.Generic;

    /// <summary>
    /// Options for configuring feature engineering operations.
    /// </summary>
    public sealed class FeatureEngineeringOptions
    {
        /// <summary>
        /// Gets or sets the polynomial degree for polynomial feature generation (default 2).
        /// </summary>
        public int PolynomialDegree { get; set; } = 2;

        /// <summary>
        /// Gets or sets whether to generate interaction features.
        /// </summary>
        public bool GenerateInteractions { get; set; }

        /// <summary>
        /// Gets or sets the column names to one-hot encode.
        /// </summary>
        public List<string> OneHotColumns { get; set; } = new();

        /// <summary>
        /// Gets or sets the scaling method to apply to features.
        /// </summary>
        public ScaleMethod ScaleMethod { get; set; } = ScaleMethod.Standard;
    }
}