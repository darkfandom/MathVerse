namespace MathVerse.Math.Geometry.SceneGraph;

using Transformations;

/// <summary>A scene node that only provides a transform.</summary>
public sealed class TransformNode : SceneNode
{
    /// <summary>Initializes a new transform node.</summary>
    /// <param name="name">The node name.</param>
    /// <param name="transform">An optional initial local transform.</param>
    public TransformNode(string name = "", Transform3D? transform = null) : base(name)
    {
        if (transform.HasValue) LocalTransform = transform.Value;
    }
}
