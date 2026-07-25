namespace MathVerse.Math.Geometry.SceneGraph;

using Lighting;

/// <summary>A scene node that represents a light source.</summary>
public sealed class LightNode : SceneNode
{
    /// <summary>Initializes a new light node.</summary>
    /// <param name="name">The node name.</param>
    /// <param name="light">An optional light instance.</param>
    public LightNode(string name = "", Light? light = null) : base(name)
    {
        Light = light ?? new AmbientLight();
    }

    /// <summary>Light attached to this node.</summary>
    public Light Light { get; set; }
}
