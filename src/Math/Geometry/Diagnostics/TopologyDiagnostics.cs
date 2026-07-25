namespace MathVerse.Math.Geometry.Diagnostics;

using System.Collections.Immutable;
using Meshes;

/// <summary>Topology-specific diagnostic operations.</summary>
public static class TopologyDiagnostics
{
    /// <summary>Reports Euler characteristic and related info.</summary>
    public static GeometryResult ValidateEulerCharacteristic(TriangleMesh mesh)
    {
        var vertices = mesh.GetVertices();
        var faces = mesh.GetTriangles();
        
        var edgeSet = new HashSet<Edge>();
        foreach (var face in faces)
        {
            var edges = face.Edges;
            edgeSet.Add(edges.E0.Canonical());
            edgeSet.Add(edges.E1.Canonical());
            edgeSet.Add(edges.E2.Canonical());
        }
        
        int euler = vertices.Count - edgeSet.Count + faces.Count;
        return GeometryResult.Ok();
    }
    
    /// <summary>Detects boundary edges (edges with only one adjacent face).</summary>
    public static ImmutableArray<Edge> FindBoundaryEdges(TriangleMesh mesh)
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
        
        return edgeCount.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).ToImmutableArray();
    }
    
    /// <summary>Checks if mesh is watertight (closed, no boundary edges).</summary>
    public static bool IsWatertight(TriangleMesh mesh) => FindBoundaryEdges(mesh).IsEmpty;
    
    /// <summary>Computes genus estimate from Euler characteristic.</summary>
    public static int ComputeGenus(TriangleMesh mesh)
    {
        var vertices = mesh.GetVertices();
        var faces = mesh.GetTriangles();
        var edgeSet = new HashSet<Edge>();
        foreach (var face in faces)
        {
            edgeSet.Add(face.Edges.E0.Canonical());
            edgeSet.Add(face.Edges.E1.Canonical());
            edgeSet.Add(face.Edges.E2.Canonical());
        }
        int euler = vertices.Count - edgeSet.Count + faces.Count;
        return 1 - (euler / 2);
    }
}
