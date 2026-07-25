namespace MathVerse.Math.Geometry.SceneGraph;

using Geometry3D;

/// <summary>Represents a complete scene graph.</summary>
public sealed class Scene
{
    private readonly List<SceneNode> _rootNodes = [];

    /// <summary>Initializes a new empty scene.</summary>
    /// <param name="name">The scene name.</param>
    public Scene(string name = "Scene")
    {
        Name = name;
    }

    /// <summary>Scene name.</summary>
    public string Name { get; set; }

    /// <summary>Root nodes of the scene.</summary>
    public IReadOnlyList<SceneNode> RootNodes => _rootNodes;

    /// <summary>Adds a root node.</summary>
    /// <param name="node">The node to add as a root.</param>
    public void AddRootNode(SceneNode node) => _rootNodes.Add(node);

    /// <summary>Removes a root node.</summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was found and removed; otherwise, false.</returns>
    public bool RemoveRootNode(SceneNode node) => _rootNodes.Remove(node);

    /// <summary>Removes all root nodes.</summary>
    public void Clear() => _rootNodes.Clear();

    /// <summary>Traverses all nodes in the scene (depth-first).</summary>
    /// <returns>A sequence of all nodes in the scene.</returns>
    public IEnumerable<SceneNode> TraverseAll() => _rootNodes.SelectMany(n => n.Traverse());

    /// <summary>Returns all geometry nodes.</summary>
    /// <returns>A sequence of all geometry nodes in the scene.</returns>
    public IEnumerable<GeometryNode> GetGeometryNodes() => TraverseAll().OfType<GeometryNode>();

    /// <summary>Returns all camera nodes.</summary>
    /// <returns>A sequence of all camera nodes in the scene.</returns>
    public IEnumerable<CameraNode> GetCameraNodes() => TraverseAll().OfType<CameraNode>();

    /// <summary>Returns all light nodes.</summary>
    /// <returns>A sequence of all light nodes in the scene.</returns>
    public IEnumerable<LightNode> GetLightNodes() => TraverseAll().OfType<LightNode>();

    /// <summary>Computes the scene bounding box.</summary>
    /// <returns>The axis-aligned bounding box enclosing all geometry in the scene.</returns>
    public BoundingBox3D ComputeBoundingBox()
    {
        Geometry3D.Point3D min = new(double.MaxValue, double.MaxValue, double.MaxValue);
        Geometry3D.Point3D max = new(double.MinValue, double.MinValue, double.MinValue);
        bool found = false;

        foreach (GeometryNode geo in GetGeometryNodes())
        {
            if (geo.Mesh == null) continue;
            BoundingBox3D bb = geo.Mesh.BoundingBox();
            if (!found)
            {
                min = bb.Min;
                max = bb.Max;
                found = true;
            }
            else
            {
                min = new Geometry3D.Point3D(
                    System.Math.Min(min.X, bb.Min.X),
                    System.Math.Min(min.Y, bb.Min.Y),
                    System.Math.Min(min.Z, bb.Min.Z));
                max = new Geometry3D.Point3D(
                    System.Math.Max(max.X, bb.Max.X),
                    System.Math.Max(max.Y, bb.Max.Y),
                    System.Math.Max(max.Z, bb.Max.Z));
            }
        }

        return found
            ? new BoundingBox3D(min, max)
            : new BoundingBox3D(Geometry3D.Point3D.Origin, Geometry3D.Point3D.Origin);
    }

    /// <summary>Number of root nodes.</summary>
    public int NodeCount => _rootNodes.Count;

    /// <summary>Total node count (including descendants).</summary>
    public int TotalNodeCount => TraverseAll().Count();
}
