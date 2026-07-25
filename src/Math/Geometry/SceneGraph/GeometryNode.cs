namespace MathVerse.Math.Geometry.SceneGraph;

using Meshes;

/// <summary>A scene node that holds mesh geometry data.</summary>
public sealed class GeometryNode : SceneNode
{
    /// <summary>Initializes a new geometry node.</summary>
    /// <param name="name">The node name.</param>
    /// <param name="mesh">An optional triangle mesh.</param>
    public GeometryNode(string name = "", TriangleMesh? mesh = null) : base(name)
    {
        Mesh = mesh;
        MaterialName = "default";
    }

    /// <summary>The mesh geometry for this node.</summary>
    public TriangleMesh? Mesh { get; set; }

    /// <summary>Name of the material applied to this geometry.</summary>
    public string MaterialName { get; set; }
}
