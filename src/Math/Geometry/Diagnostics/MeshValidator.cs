namespace MathVerse.Math.Geometry.Diagnostics;

using System.Collections.Immutable;
using Meshes;
using Geometry3D;

/// <summary>Comprehensive mesh validation.</summary>
public static class MeshValidator
{
    /// <summary>Validates topology (index bounds, manifold edges).</summary>
    public static GeometryResult ValidateTopology(TriangleMesh mesh)
    {
        var result = GeometryValidator.Validate(mesh);
        if (!result.Success) return result;
        
        var vertices = mesh.GetVertices();
        var faces = mesh.GetTriangles();
        
        for (int i = 0; i < faces.Count; i++)
        {
            var a = vertices[faces[i].V0].Position;
            var b = vertices[faces[i].V1].Position;
            var c = vertices[faces[i].V2].Position;
            double area = new Triangle3D(a, b, c).Area;
            if (area < 1e-15)
                return GeometryResult.Failure($"Face {i} is degenerate", GeometryDiagnosticType.DegenerateGeometry);
        }
        
        return GeometryResult.Ok();
    }
    
    /// <summary>Detects degenerate triangles.</summary>
    public static ImmutableArray<int> FindDegenerateTriangles(TriangleMesh mesh, double tolerance = 1e-10)
    {
        var result = ImmutableArray.CreateBuilder<int>();
        var vertices = mesh.GetVertices();
        var faces = mesh.GetTriangles();
        
        for (int i = 0; i < faces.Count; i++)
        {
            var a = vertices[faces[i].V0].Position;
            var b = vertices[faces[i].V1].Position;
            var c = vertices[faces[i].V2].Position;
            double area = new Triangle3D(a, b, c).Area;
            if (area < tolerance) result.Add(i);
        }
        
        return result.ToImmutable();
    }
    
    /// <summary>Finds non-manifold edges.</summary>
    public static ImmutableArray<Edge> FindNonManifoldEdges(TriangleMesh mesh)
    {
        var edgeCount = new Dictionary<Edge, int>();
        var faces = mesh.GetTriangles();
        
        foreach (var face in faces)
        {
            var edges = face.Edges;
            var e0 = edges.E0.Canonical();
            var e1 = edges.E1.Canonical();
            var e2 = edges.E2.Canonical();
            
            edgeCount[e0] = edgeCount.GetValueOrDefault(e0) + 1;
            edgeCount[e1] = edgeCount.GetValueOrDefault(e1) + 1;
            edgeCount[e2] = edgeCount.GetValueOrDefault(e2) + 1;
        }
        
        return edgeCount.Where(kvp => kvp.Value > 2).Select(kvp => kvp.Key).ToImmutableArray();
    }
}
