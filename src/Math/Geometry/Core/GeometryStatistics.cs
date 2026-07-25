namespace MathVerse.Math.Geometry;

/// <summary>
/// Accumulates cumulative statistics for geometry operations within a session.
/// </summary>
public record GeometryStatistics
{
    /// <summary>Gets or sets the total number of points created.</summary>
    public int PointsCreated { get; set; }

    /// <summary>Gets or sets the total number of lines created.</summary>
    public int LinesCreated { get; set; }

    /// <summary>Gets or sets the total number of circles created.</summary>
    public int CirclesCreated { get; set; }

    /// <summary>Gets or sets the total number of triangles created.</summary>
    public int TrianglesCreated { get; set; }

    /// <summary>Gets or sets the total number of meshes created.</summary>
    public int MeshesCreated { get; set; }

    /// <summary>Gets or sets the total number of transformations applied.</summary>
    public int TransformationsApplied { get; set; }

    /// <summary>Gets or sets the total number of intersection computations performed.</summary>
    public int IntersectionsComputed { get; set; }

    /// <summary>Gets or sets the total number of curve evaluations performed.</summary>
    public int CurvesEvaluated { get; set; }

    /// <summary>Gets or sets the total number of surface evaluations performed.</summary>
    public int SurfacesEvaluated { get; set; }

    /// <summary>Gets or sets the cumulative memory allocated in bytes.</summary>
    public long TotalMemoryAllocated { get; set; }
}
