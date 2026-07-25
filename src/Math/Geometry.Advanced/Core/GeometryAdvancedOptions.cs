namespace MathVerse.Math.Geometry.Advanced;

/// <summary>
/// Defines the configuration options for the advanced geometry processing engine.
/// Controls tolerance thresholds, parallelism settings, and performance optimization flags
/// used across all geometry operations.
/// </summary>
/// <param name="Tolerance">The floating-point comparison tolerance for geometric predicates and equality tests. Default is 1e-10.</param>
/// <param name="MaxParallelism">The maximum degree of parallelism for concurrent operations. Use -1 for unlimited parallelism based on available cores.</param>
/// <param name="UseSIMD">When true, enables SIMD-accelerated geometric computations where available for improved throughput.</param>
/// <param name="UseObjectPooling">When true, enables object pooling for frequently allocated geometry data structures to reduce GC pressure.</param>
public readonly record struct GeometryAdvancedOptions(
    double Tolerance = 1e-10,
    int MaxParallelism = -1,
    bool UseSIMD = true,
    bool UseObjectPooling = true)
{
    /// <summary>The floating-point comparison tolerance for geometric predicates.</summary>
    public double Tolerance { get; } = Tolerance;

    /// <summary>The maximum degree of parallelism for concurrent operations. -1 indicates unlimited parallelism.</summary>
    public int MaxParallelism { get; } = MaxParallelism;

    /// <summary>Whether SIMD-accelerated computations are enabled.</summary>
    public bool UseSIMD { get; } = UseSIMD;

    /// <summary>Whether object pooling for geometry buffers is enabled.</summary>
    public bool UseObjectPooling { get; } = UseObjectPooling;
}
