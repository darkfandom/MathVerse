using System.Numerics;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Core;

public interface IRenderObject
{
    Guid WorkspaceObjectId { get; }
    Matrix4x4 Transform { get; set; }
    bool IsVisible { get; set; }
    int Layer { get; set; }
    BoundingBox? BoundingBox { get; set; }
    MeshData? MeshData { get; }
    RenderMaterial Material { get; }
    void UpdateFrom(IWorkspaceObject obj);
}

public sealed class RenderMaterial
{
    public float R { get; set; } = 0.27f;
    public float G { get; set; } = 0.53f;
    public float B { get; set; } = 1.0f;
    public float A { get; set; } = 1.0f;
    public float Roughness { get; set; } = 0.5f;
    public float Metalness { get; set; }
    public float Opacity { get; set; } = 1.0f;
    public float EmissiveR { get; set; }
    public float EmissiveG { get; set; }
    public float EmissiveB { get; set; }
}

public sealed class MeshData
{
    public float[] Vertices { get; set; } = [];
    public int[] Indices { get; set; } = [];
    public float[] Normals { get; set; } = [];
    public float[] UVs { get; set; } = [];
}

public abstract class RenderObject : IRenderObject
{
    public Guid WorkspaceObjectId { get; }
    public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
    public bool IsVisible { get; set; } = true;
    public int Layer { get; set; }
    public BoundingBox? BoundingBox { get; set; }
    public MeshData? MeshData { get; protected set; }
    public RenderMaterial Material { get; } = new();

    protected RenderObject(Guid workspaceObjectId)
    {
        WorkspaceObjectId = workspaceObjectId;
    }

    public abstract void UpdateFrom(IWorkspaceObject obj);
}

public sealed class GraphRenderObject : RenderObject
{
    public float[] SamplePointsX { get; set; } = [];
    public float[] SamplePointsY { get; set; } = [];
    public string GraphType { get; set; } = "Cartesian";

    public GraphRenderObject(Guid workspaceObjectId) : base(workspaceObjectId)
    {
    }

    public override void UpdateFrom(IWorkspaceObject obj)
    {
        IsVisible = obj.IsVisible;
        Layer = obj.Layer;
        BoundingBox = obj.BoundingBox;

        if (obj.Metadata.TryGetValue("Color", out var color) && color is string hex && hex.Length >= 6)
        {
            if (byte.TryParse(hex[1..3], System.Globalization.NumberStyles.HexNumber, null, out var r))
                Material.R = r / 255f;
            if (byte.TryParse(hex[3..5], System.Globalization.NumberStyles.HexNumber, null, out var g))
                Material.G = g / 255f;
            if (byte.TryParse(hex[5..7], System.Globalization.NumberStyles.HexNumber, null, out var b))
                Material.B = b / 255f;
        }
    }
}

public sealed class SurfaceRenderObject : RenderObject
{
    public float[] Vertices { get; set; } = [];
    public int Width { get; set; }
    public int Height { get; set; }

    public SurfaceRenderObject(Guid workspaceObjectId) : base(workspaceObjectId)
    {
    }

    public override void UpdateFrom(IWorkspaceObject obj)
    {
        IsVisible = obj.IsVisible;
        Layer = obj.Layer;
        BoundingBox = obj.BoundingBox;
    }
}

public sealed class GeometryRenderObject : RenderObject
{
    public float[] Points { get; set; } = [];
    public int[] Indices { get; set; } = [];
    public string ShapeType { get; set; } = "Points";

    public GeometryRenderObject(Guid workspaceObjectId) : base(workspaceObjectId)
    {
    }

    public override void UpdateFrom(IWorkspaceObject obj)
    {
        IsVisible = obj.IsVisible;
        Layer = obj.Layer;
        BoundingBox = obj.BoundingBox;
    }
}

public sealed class MeshRenderObject : RenderObject
{
    public MeshRenderObject(Guid workspaceObjectId) : base(workspaceObjectId)
    {
    }

    public override void UpdateFrom(IWorkspaceObject obj)
    {
        IsVisible = obj.IsVisible;
        Layer = obj.Layer;
        BoundingBox = obj.BoundingBox;
    }
}
