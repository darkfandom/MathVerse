namespace MathVerse.Math.Geometry.Advanced;

/// <summary>
/// Provides a centralized configuration object for the advanced geometry engine,
/// encapsulating a set of <see cref="GeometryAdvancedOptions"/> and exposing predefined
/// presets for common use cases such as default precision and high-precision computation.
/// </summary>
public sealed class GeometryAdvancedConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryAdvancedConfiguration"/> class
    /// with the specified options.
    /// </summary>
    /// <param name="options">The configuration options governing engine behavior.</param>
    public GeometryAdvancedConfiguration(GeometryAdvancedOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Gets the configuration options governing engine behavior.
    /// </summary>
    public GeometryAdvancedOptions Options { get; }

    /// <summary>
    /// Gets the default configuration with standard tolerance (1e-10), unlimited parallelism,
    /// and both SIMD and object pooling enabled.
    /// </summary>
    public static GeometryAdvancedConfiguration Default { get; } = new(new GeometryAdvancedOptions());

    /// <summary>
    /// Gets a high-precision configuration with tolerance set to 1e-15,
    /// suitable for applications requiring maximum numerical accuracy at the cost of
    /// potentially stricter comparison thresholds.
    /// </summary>
    public static GeometryAdvancedConfiguration HighPrecision { get; } = new(new GeometryAdvancedOptions(Tolerance: 1e-15));
}
