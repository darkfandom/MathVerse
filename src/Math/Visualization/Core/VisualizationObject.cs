namespace MathVerse.Math.Visualization.Core;
using System.Numerics;

/// <summary>Base class for all objects that can be visualized in the rendering system.</summary>
public class VisualizationObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; init; } = "";
    public Rendering.BoundingBox Bounds { get; init; } = new(Vector3.Zero, Vector3.Zero);
    public string MaterialId { get; init; } = "default";
    public bool IsVisible { get; set; } = true;
    public Matrix4x4 Transform { get; init; } = Matrix4x4.Identity;
    public int RenderOrder { get; init; }
    public string? Color { get; set; }
    public Vector3? Position { get; set; }
}
