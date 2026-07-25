namespace MathVerse.Math.AI.MachineLearning.DimensionalityReduction;
using System.Collections.Immutable;

/// <summary>Result of a dimensionality reduction operation.</summary>
public sealed class DimensionalityReductionResult
{
    /// <summary>The data projected into the reduced-dimensional space.</summary>
    public double[][] TransformedData { get; init; } = [];

    /// <summary>Proportion of variance explained by each principal component.</summary>
    public double[] ExplainedVarianceRatio { get; init; } = [];

    /// <summary>The principal component directions (eigenvectors).</summary>
    public double[][] Components { get; init; } = [];

    /// <summary>Number of dimensions in the original data.</summary>
    public int OriginalDimensions { get; init; }

    /// <summary>Number of dimensions after reduction.</summary>
    public int ReducedDimensions { get; init; }
}
