namespace MathVerse.Math.DataScience.Core;

/// <summary>
/// Options for cleaning operations on datasets.
/// </summary>
public sealed class CleaningOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to remove duplicate rows.
    /// </summary>
    public bool RemoveDuplicates { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to fill missing values with column defaults.
    /// </summary>
    public bool FillMissingValues { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trim whitespace from string values.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to remove rows with missing values.
    /// </summary>
    public bool RemoveRowsWithMissing { get; set; }

    /// <summary>
    /// Gets or sets the maximum percentage of missing values allowed per column before removal.
    /// </summary>
    public double MaxMissingPercentage { get; set; } = 50.0;
}

/// <summary>
/// Options for feature engineering operations.
/// </summary>
public sealed class FeatureEngineeringOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to create polynomial features.
    /// </summary>
    public bool CreatePolynomialFeatures { get; set; }

    /// <summary>
    /// Gets or sets the degree of polynomial features.
    /// </summary>
    public int PolynomialDegree { get; set; } = 2;

    /// <summary>
    /// Gets or sets a value indicating whether to create interaction features.
    /// </summary>
    public bool CreateInteractionFeatures { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create log-transformed features.
    /// </summary>
    public bool CreateLogFeatures { get; set; }

    /// <summary>
    /// Gets or sets the columns to apply feature engineering to.
    /// </summary>
    public string[] TargetColumns { get; set; } = System.Array.Empty<string>();
}