namespace MathVerse.Math.Geometry;

/// <summary>
/// Immutable configuration for geometry processing operations.
/// </summary>
public record GeometryOptions
{
    /// <summary>Gets the numerical tolerance used for geometric comparisons.</summary>
    public double Tolerance { get; init; } = 1e-10;

    /// <summary>Gets a value indicating whether result caching is enabled.</summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>Gets a value indicating whether parallel processing is enabled for suitable operations.</summary>
    public bool EnableParallelProcessing { get; init; } = true;

    /// <summary>Gets the maximum degree of parallelism for parallel operations.</summary>
    public int MaxParallelism { get; init; } = Environment.ProcessorCount;

    /// <summary>Gets a value indicating whether detailed diagnostic data is collected.</summary>
    public bool EnableDiagnostics { get; init; } = false;

    /// <summary>Gets a value indicating whether geometry is validated at creation time.</summary>
    public bool ValidateOnCreate { get; init; } = false;
}
