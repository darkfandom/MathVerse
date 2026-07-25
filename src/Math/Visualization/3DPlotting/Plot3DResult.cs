namespace MathVerse.Math.Visualization._3DPlotting;

using System.Collections.Immutable;

/// <summary>Plot type constants for legacy API compatibility.</summary>
public static class Plot3DType
{
    public const string Surface = "Surface";
    public const string Wireframe = "Wireframe";
    public const string Mesh = "Mesh";
    public const string PointCloud = "PointCloud";
    public const string HeightMap = "HeightMap";
    public const string ParametricSurface = "ParametricSurface";
}

/// <summary>Represents a 3D visualization result containing lines, points, surfaces, and meshes.</summary>
public sealed class Plot3DResult
{
    /// <summary>Gets the list of line segments for wireframe rendering.</summary>
    public List<Line3DSeries> Lines { get; init; } = [];

    /// <summary>Gets the list of scatter point series.</summary>
    public List<Point3DSeries> Points { get; init; } = [];

    /// <summary>Gets the list of triangle mesh surfaces.</summary>
    public List<Mesh3DSeries> Meshes { get; init; } = [];

    /// <summary>Gets the list of polygon fills.</summary>
    public List<Polygon3DSeries> Polygons { get; init; } = [];

    /// <summary>Gets the list of edge connections for graph-like visualizations.</summary>
    public List<Edge3DSeries> Edges { get; init; } = [];

    /// <summary>Gets the bounding box min corner.</summary>
    public double[] BoundsMin { get; init; } = [double.MaxValue, double.MaxValue, double.MaxValue];

    /// <summary>Gets the bounding box max corner.</summary>
    public double[] BoundsMax { get; init; } = [double.MinValue, double.MinValue, double.MinValue];

    /// <summary>Gets or sets the raw vertex list.</summary>
    public List<System.Numerics.Vector3> Vertices { get; set; } = [];

    /// <summary>Gets or sets the raw face index arrays.</summary>
    public List<int[]> Faces { get; set; } = [];

    /// <summary>Gets or sets the raw normal list.</summary>
    public List<System.Numerics.Vector3> Normals { get; set; } = [];

    /// <summary>Gets or sets the per-vertex colors.</summary>
    public List<System.Numerics.Vector4> VertexColors { get; set; } = [];

    /// <summary>Gets or sets the bounding box.</summary>
    public Rendering.BoundingBox Bounds { get; set; }

    /// <summary>Gets or sets the plot type identifier.</summary>
    public string PlotType { get; set; } = "";

    /// <summary>Creates a new empty <see cref="Plot3DResult"/>.</summary>
    /// <returns>An empty 3D plot result.</returns>
    public static Plot3DResult Empty => new();
}

/// <summary>A series of connected 3D line segments.</summary>
public sealed class Line3DSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the X coordinates of the line vertices.</summary>
    public ImmutableArray<double> X { get; init; }

    /// <summary>Gets the Y coordinates of the line vertices.</summary>
    public ImmutableArray<double> Y { get; init; }

    /// <summary>Gets the Z coordinates of the line vertices.</summary>
    public ImmutableArray<double> Z { get; init; }

    /// <summary>Gets the color of the line.</summary>
    public string Color { get; init; } = "#007ACC";

    /// <summary>Gets the line width.</summary>
    public double LineWidth { get; init; } = 1.0;

    /// <summary>Gets whether the line is dashed.</summary>
    public bool IsDashed { get; init; }
}

/// <summary>A series of 3D scatter points.</summary>
public sealed class Point3DSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the X coordinates of the points.</summary>
    public ImmutableArray<double> X { get; init; }

    /// <summary>Gets the Y coordinates of the points.</summary>
    public ImmutableArray<double> Y { get; init; }

    /// <summary>Gets the Z coordinates of the points.</summary>
    public ImmutableArray<double> Z { get; init; }

    /// <summary>Gets the color of the points.</summary>
    public string Color { get; init; } = "#E74C3C";

    /// <summary>Gets the point size.</summary>
    public double PointSize { get; init; } = 5.0;

    /// <summary>Gets the marker shape.</summary>
    public string Marker { get; init; } = "circle";
}

/// <summary>A triangle mesh for surface rendering.</summary>
public sealed class Mesh3DSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the vertex positions as flat arrays.</summary>
    public ImmutableArray<double> VertexX { get; init; }

    /// <summary>Gets the vertex Y coordinates.</summary>
    public ImmutableArray<double> VertexY { get; init; }

    /// <summary>Gets the vertex Z coordinates.</summary>
    public ImmutableArray<double> VertexZ { get; init; }

    /// <summary>Gets the triangle face indices (groups of 3).</summary>
    public ImmutableArray<int> FaceIndices { get; init; }

    /// <summary>Gets optional vertex normals X component.</summary>
    public ImmutableArray<double>? NormalX { get; init; }

    /// <summary>Gets optional vertex normals Y component.</summary>
    public ImmutableArray<double>? NormalY { get; init; }

    /// <summary>Gets optional vertex normals Z component.</summary>
    public ImmutableArray<double>? NormalZ { get; init; }

    /// <summary>Gets the mesh color.</summary>
    public string Color { get; init; } = "#3498DB";

    /// <summary>Gets the opacity.</summary>
    public double Opacity { get; init; } = 1.0;

    /// <summary>Gets whether the mesh is wireframe only.</summary>
    public bool Wireframe { get; init; }
}

/// <summary>A filled 3D polygon.</summary>
public sealed class Polygon3DSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the vertex X coordinates.</summary>
    public ImmutableArray<double> X { get; init; }

    /// <summary>Gets the vertex Y coordinates.</summary>
    public ImmutableArray<double> Y { get; init; }

    /// <summary>Gets the vertex Z coordinates.</summary>
    public ImmutableArray<double> Z { get; init; }

    /// <summary>Gets the fill color.</summary>
    public string Color { get; init; } = "#2ECC71";

    /// <summary>Gets the opacity.</summary>
    public double Opacity { get; init; } = 0.6;

    /// <summary>Gets whether to draw the polygon outline.</summary>
    public bool ShowOutline { get; init; } = true;
}

/// <summary>A series of edges for graph or tree visualization.</summary>
public sealed class Edge3DSeries
{
    /// <summary>Gets the series name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the start X coordinates.</summary>
    public ImmutableArray<double> X1 { get; init; }

    /// <summary>Gets the start Y coordinates.</summary>
    public ImmutableArray<double> Y1 { get; init; }

    /// <summary>Gets the start Z coordinates.</summary>
    public ImmutableArray<double> Z1 { get; init; }

    /// <summary>Gets the end X coordinates.</summary>
    public ImmutableArray<double> X2 { get; init; }

    /// <summary>Gets the end Y coordinates.</summary>
    public ImmutableArray<double> Y2 { get; init; }

    /// <summary>Gets the end Z coordinates.</summary>
    public ImmutableArray<double> Z2 { get; init; }

    /// <summary>Gets the edge color.</summary>
    public string Color { get; init; } = "#95A5A6";

    /// <summary>Gets the edge width.</summary>
    public double Width { get; init; } = 1.0;

    /// <summary>Gets the edge labels.</summary>
    public ImmutableArray<string>? Labels { get; init; }
}
