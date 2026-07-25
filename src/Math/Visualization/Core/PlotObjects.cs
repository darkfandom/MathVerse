namespace MathVerse.Math.Visualization.Core;
using System.Numerics;

/// <summary>Line plot visualization object.</summary>
public sealed class LinePlot : VisualizationObject
{
    public List<Vector2> Points { get; set; } = [];
    public new string Color { get; set; } = "#007ACC";
    public double LineWidth { get; set; } = 2.0;
    public int LineStyle { get; init; }
    public bool IsDashed { get; set; }
}

/// <summary>Point cloud visualization object.</summary>
public sealed class PointCloud : VisualizationObject
{
    public List<Vector3> Points { get; set; } = [];
    public List<Vector4> Colors { get; init; } = [];
    public double PointSize { get; set; } = 4.0;
    public new string? Color { get; set; }
}

/// <summary>Surface plot visualization object.</summary>
public sealed class SurfacePlot : VisualizationObject
{
    public List<Vector3> Vertices { get; init; } = [];
    public List<int[]> Faces { get; init; } = [];
    public List<Vector3> Normals { get; init; } = [];
    public List<Vector4> VertexColors { get; init; } = [];
    public string? FillColor { get; set; }
    public double Opacity { get; set; } = 0.7;
    public List<List<Vector3>>? Cells { get; set; }
}

/// <summary>Triangle mesh visualization object.</summary>
public sealed class MeshObject : VisualizationObject
{
    public List<Vector3> Vertices { get; set; } = [];
    public List<int[]> Faces { get; set; } = [];
    public List<Vector3> Normals { get; init; } = [];
    public List<Vector4> VertexColors { get; init; } = [];
    public string? WireframeColor { get; set; }
    public string? FillColor { get; set; }
}
