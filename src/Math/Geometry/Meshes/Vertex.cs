namespace MathVerse.Math.Geometry.Meshes;

using Geometry3D;

/// <summary>Represents a mesh vertex with position, normal, and UV coordinates.</summary>
public readonly record struct Vertex
{
    /// <summary>Vertex position.</summary>
    public Point3D Position { get; init; }

    /// <summary>Vertex normal.</summary>
    public Vector3D Normal { get; init; }

    /// <summary>Texture coordinate.</summary>
    public (double U, double V) UV { get; init; }

    /// <summary>Initializes a new vertex.</summary>
    /// <param name="position">The vertex position.</param>
    /// <param name="normal">The vertex normal.</param>
    /// <param name="uv">The texture coordinate.</param>
    public Vertex(Point3D position, Vector3D normal, (double U, double V) uv)
    {
        Position = position;
        Normal = normal;
        UV = uv;
    }

    /// <summary>Linearly interpolates between two vertices.</summary>
    /// <param name="other">The target vertex.</param>
    /// <param name="t">Interpolation parameter in [0, 1].</param>
    /// <returns>The interpolated vertex.</returns>
    public Vertex Lerp(Vertex other, double t) => new(
        Position.Lerp(other.Position, t),
        new Vector3D(
            Normal.X + (other.Normal.X - Normal.X) * t,
            Normal.Y + (other.Normal.Y - Normal.Y) * t,
            Normal.Z + (other.Normal.Z - Normal.Z) * t),
        (UV.U + (other.UV.U - UV.U) * t, UV.V + (other.UV.V - UV.V) * t));
}
