namespace MathVerse.Math.Geometry.Rendering;

using System.Collections.Immutable;
using Meshes;

/// <summary>GPU-ready buffer data for a mesh.</summary>
public sealed record MeshBuffer
{
    /// <summary>Vertex positions as flat array (x,y,z,x,y,z,...).</summary>
    public ImmutableArray<float> Positions { get; init; }
    
    /// <summary>Vertex normals as flat array.</summary>
    public ImmutableArray<float> Normals { get; init; }
    
    /// <summary>Texture coordinates as flat array (u,v,u,v,...).</summary>
    public ImmutableArray<float> UVs { get; init; }
    
    /// <summary>Index buffer.</summary>
    public ImmutableArray<int> Indices { get; init; }
    
    /// <summary>Creates a MeshBuffer from a TriangleMesh.</summary>
    public static MeshBuffer FromMesh(TriangleMesh mesh)
    {
        var positions = ImmutableArray.CreateBuilder<float>();
        var normals = ImmutableArray.CreateBuilder<float>();
        var uvs = ImmutableArray.CreateBuilder<float>();
        var indices = ImmutableArray.CreateBuilder<int>();
        
        foreach (var v in mesh.GetVertices())
        {
            positions.Add((float)v.Position.X);
            positions.Add((float)v.Position.Y);
            positions.Add((float)v.Position.Z);
            normals.Add((float)v.Normal.X);
            normals.Add((float)v.Normal.Y);
            normals.Add((float)v.Normal.Z);
            uvs.Add((float)v.UV.U);
            uvs.Add((float)v.UV.V);
        }
        
        foreach (var face in mesh.GetTriangles())
        {
            indices.Add(face.V0);
            indices.Add(face.V1);
            indices.Add(face.V2);
        }
        
        return new MeshBuffer
        {
            Positions = positions.ToImmutable(),
            Normals = normals.ToImmutable(),
            UVs = uvs.ToImmutable(),
            Indices = indices.ToImmutable()
        };
    }
}
