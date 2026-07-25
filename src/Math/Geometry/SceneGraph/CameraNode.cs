namespace MathVerse.Math.Geometry.SceneGraph;

using Cameras;

/// <summary>A scene node that represents a camera.</summary>
public sealed class CameraNode : SceneNode
{
    /// <summary>Initializes a new camera node.</summary>
    /// <param name="name">The node name.</param>
    /// <param name="camera">An optional camera instance.</param>
    public CameraNode(string name = "", Camera? camera = null) : base(name)
    {
        Camera = camera ?? new PerspectiveCamera();
    }

    /// <summary>Camera attached to this node.</summary>
    public Camera Camera { get; set; }
}
