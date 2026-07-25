namespace MathVerse.Math.Geometry.Picking;

using Geometry3D;
using SceneGraph;

/// <summary>Result of a hit test / picking operation.</summary>
public sealed record HitTestResult
{
    /// <summary>Whether the ray hit something.</summary>
    public bool Hit { get; init; }
    
    /// <summary>Distance from ray origin.</summary>
    public double Distance { get; init; }
    
    /// <summary>World-space hit point.</summary>
    public Point3D HitPoint { get; init; }
    
    /// <summary>Surface normal at hit point.</summary>
    public Vector3D Normal { get; init; }
    
    /// <summary>Triangle index in the mesh.</summary>
    public int TriangleIndex { get; init; }
    
    /// <summary>Barycentric coordinates.</summary>
    public (double U, double V, double W) BarycentricCoords { get; init; }
    
    /// <summary>The scene node that was hit.</summary>
    public SceneNode? Node { get; init; }
    
    /// <summary>No-hit result.</summary>
    public static HitTestResult Miss() => new() { Hit = false, Distance = double.MaxValue };
}
