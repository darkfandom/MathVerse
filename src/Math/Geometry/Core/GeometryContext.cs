namespace MathVerse.Math.Geometry;

/// <summary>
/// Holds mutable state for a geometry processing session, including options and statistics.
/// </summary>
public class GeometryContext
{
    /// <summary>Gets the geometry options governing this context.</summary>
    public GeometryOptions Options { get; }

    /// <summary>Gets the cumulative statistics for this context.</summary>
    public GeometryStatistics Statistics { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryContext"/> class using default options.
    /// </summary>
    public GeometryContext() : this(new GeometryOptions()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The geometry options to use for this context.</param>
    public GeometryContext(GeometryOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Statistics = new GeometryStatistics();
    }

    /// <summary>
    /// Records that a geometry creation event occurred, incrementing the relevant counter.
    /// </summary>
    public void TrackCreation()
    {
        Statistics = Statistics with { PointsCreated = Statistics.PointsCreated + 1 };
    }

    /// <summary>
    /// Resets all accumulated statistics to their default values.
    /// </summary>
    public void Reset()
    {
        Statistics = new GeometryStatistics();
    }
}
