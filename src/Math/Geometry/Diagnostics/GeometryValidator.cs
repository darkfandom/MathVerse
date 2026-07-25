namespace MathVerse.Math.Geometry.Diagnostics;

using Geometry2D;
using Geometry3D;
using Meshes;

/// <summary>Validates geometric objects for correctness.</summary>
public static class GeometryValidator
{
    /// <summary>Validates a triangle mesh.</summary>
    public static GeometryResult Validate(TriangleMesh mesh)
    {
        if (mesh == null) return GeometryResult.Failure("Mesh is null");
        
        var vertices = mesh.GetVertices();
        var faces = mesh.GetTriangles();
        
        if (vertices.Count == 0) return GeometryResult.Failure("Mesh has no vertices", GeometryDiagnosticType.EmptyMesh);
        if (faces.Count == 0) return GeometryResult.Failure("Mesh has no faces", GeometryDiagnosticType.EmptyMesh);
        
        for (int i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            if (face.V0 < 0 || face.V0 >= vertices.Count ||
                face.V1 < 0 || face.V1 >= vertices.Count ||
                face.V2 < 0 || face.V2 >= vertices.Count)
                return GeometryResult.Failure($"Face {i} has invalid vertex indices", GeometryDiagnosticType.InvalidTopology);
        }
        
        return GeometryResult.Ok();
    }
    
    /// <summary>Validates a 2D triangle.</summary>
    public static GeometryResult Validate(Triangle2D triangle)
    {
        if (double.IsNaN(triangle.A.X) || double.IsNaN(triangle.A.Y) ||
            double.IsNaN(triangle.B.X) || double.IsNaN(triangle.B.Y) ||
            double.IsNaN(triangle.C.X) || double.IsNaN(triangle.C.Y))
            return GeometryResult.Failure("Triangle contains NaN", GeometryDiagnosticType.NumericalInstability);
        
        if (triangle.IsDegenerate())
            return GeometryResult.Failure("Triangle is degenerate (zero area)", GeometryDiagnosticType.DegenerateGeometry);
        
        return GeometryResult.Ok();
    }
    
    /// <summary>Validates a 3D triangle.</summary>
    public static GeometryResult Validate(Triangle3D triangle)
    {
        if (triangle.IsDegenerate())
            return GeometryResult.Failure("Triangle is degenerate (zero area)", GeometryDiagnosticType.DegenerateGeometry);
        
        return GeometryResult.Ok();
    }
    
    /// <summary>Validates a circle.</summary>
    public static GeometryResult Validate(Circle2D circle)
    {
        if (double.IsNaN(circle.Center.X) || double.IsNaN(circle.Center.Y) || double.IsNaN(circle.Radius))
            return GeometryResult.Failure("Circle contains NaN", GeometryDiagnosticType.NumericalInstability);
        if (circle.Radius <= 0)
            return GeometryResult.Failure("Circle radius must be positive", GeometryDiagnosticType.DegenerateGeometry);
        return GeometryResult.Ok();
    }
    
    /// <summary>Validates a sphere.</summary>
    public static GeometryResult Validate(Sphere3D sphere)
    {
        if (sphere.Radius <= 0)
            return GeometryResult.Failure("Sphere radius must be positive", GeometryDiagnosticType.DegenerateGeometry);
        return GeometryResult.Ok();
    }
    
    /// <summary>Validates a polygon.</summary>
    public static GeometryResult Validate(Polygon2D polygon)
    {
        if (polygon.VertexCount < 3)
            return GeometryResult.Failure("Polygon must have at least 3 vertices", GeometryDiagnosticType.DegenerateGeometry);
        
        if (!polygon.IsSimple)
            return GeometryResult.Failure("Polygon is self-intersecting", GeometryDiagnosticType.SelfIntersection);
        
        return GeometryResult.Ok();
    }
}
