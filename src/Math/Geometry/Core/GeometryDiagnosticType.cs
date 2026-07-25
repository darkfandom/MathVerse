namespace MathVerse.Math.Geometry;

/// <summary>
/// Specifies the type of geometry diagnostic detected during validation.
/// </summary>
public enum GeometryDiagnosticType
{
    /// <summary>A general or unspecified diagnostic.</summary>
    General,

    /// <summary>Geometry is degenerate (e.g. zero-area triangle, zero-radius circle).</summary>
    DegenerateGeometry,

    /// <summary>Geometry self-intersects.</summary>
    SelfIntersection,

    /// <summary>Geometry has invalid topology.</summary>
    InvalidTopology,

    /// <summary>Numerical instability detected during computation.</summary>
    NumericalInstability,

    /// <summary>A required input was null.</summary>
    NullInput,

    /// <summary>A value is outside the valid range.</summary>
    OutOfRange,

    /// <summary>Geometry has an invalid dimension for the requested operation.</summary>
    InvalidDimension,

    /// <summary>Vertices overlap within the configured tolerance.</summary>
    OverlappingVertices,

    /// <summary>Mesh topology is non-manifold.</summary>
    NonManifold,

    /// <summary>Mesh contains no faces.</summary>
    EmptyMesh,

    /// <summary>Mesh contains an invalid face definition.</summary>
    InvalidFace
}
