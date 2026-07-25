namespace MathVerse.Math.DataScience.DataCleaning
{
    using System.Collections.Generic;

    /// <summary>
    /// Specifies which cleaning operations to apply during data cleaning.
    /// </summary>
    public sealed class CleaningOptions
    {
        /// <summary>
        /// Gets or sets the strategy for handling missing values.
        /// </summary>
        public MissingValueStrategy HandleMissing { get; set; } = MissingValueStrategy.None;

        /// <summary>
        /// Gets or sets whether to remove duplicate rows.
        /// </summary>
        public bool RemoveDuplicates { get; set; }

        /// <summary>
        /// Gets or sets whether to detect and clip outliers.
        /// </summary>
        public bool DetectOutliers { get; set; }

        /// <summary>
        /// Gets or sets the column names to normalize.
        /// </summary>
        public List<string> NormalizeColumns { get; set; } = new();

        /// <summary>
        /// Gets or sets the column names to standardize.
        /// </summary>
        public List<string> StandardizeColumns { get; set; } = new();

        /// <summary>
        /// Gets or sets the column names to winsorize.
        /// </summary>
        public List<string> WinsorizeColumns { get; set; } = new();

        /// <summary>
        /// Gets or sets the constant value to use for constant imputation.
        /// </summary>
        public object? ConstantImputeValue { get; set; }
    }

    /// <summary>
    /// Defines strategies for handling missing values.
    /// </summary>
    public enum MissingValueStrategy
    {
        /// <summary>No imputation is performed.</summary>
        None,

        /// <summary>Impute missing values with the column mean.</summary>
        Mean,

        /// <summary>Impute missing values with the column median.</summary>
        Median,

        /// <summary>Impute missing values with the column mode.</summary>
        Mode,

        /// <summary>Impute missing values with a constant value.</summary>
        Constant,

        /// <summary>Drop rows with missing values.</summary>
        Drop,

        /// <summary>Interpolate missing values linearly.</summary>
        Interpolate
    }
}