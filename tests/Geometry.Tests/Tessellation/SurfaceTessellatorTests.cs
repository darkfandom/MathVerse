namespace MathVerse.Geometry.Tests.Tessellation;

/// <summary>Tests for the <see cref="SurfaceTessellator"/> static class.</summary>
public class SurfaceTessellatorTests
{
    private const double Tolerance = 1e-6;

    /// <summary>Verifies TessellateSphere produces the expected vertex count.</summary>
    [Fact]
    public void TessellateSphere_CorrectVertexCount()
    {
        int latDiv = 8;
        int lonDiv = 12;
        var sphere = new Sphere3D(Point3D.Origin, 1.0);

        var mesh = SurfaceTessellator.TessellateSphere(sphere, latDiv, lonDiv);

        int expectedVertices = (latDiv + 1) * (lonDiv + 1);
        mesh.VertexCount.Should().Be(expectedVertices);
    }

    /// <summary>Verifies TessellateSphere produces triangles.</summary>
    [Fact]
    public void TessellateSphere_ProducesTriangles()
    {
        var sphere = new Sphere3D(Point3D.Origin, 1.0);

        var mesh = SurfaceTessellator.TessellateSphere(sphere, 4, 8);

        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    /// <summary>Verifies TessellateCylinder produces the expected vertex count.</summary>
    [Fact]
    public void TessellateCylinder_ProducesCorrectVertexCount()
    {
        int radialDiv = 8;
        int heightDiv = 4;
        var cylinder = new Cylinder3D(Point3D.Origin, 1.0, 2.0);

        var mesh = SurfaceTessellator.TessellateCylinder(cylinder, radialDiv, heightDiv);

        int expectedVertices = (heightDiv + 1) * (radialDiv + 1) + 2;
        mesh.VertexCount.Should().Be(expectedVertices);
    }

    /// <summary>Verifies TessellateCylinder produces triangles.</summary>
    [Fact]
    public void TessellateCylinder_ProducesTriangles()
    {
        var cylinder = new Cylinder3D(Point3D.Origin, 1.0, 2.0);

        var mesh = SurfaceTessellator.TessellateCylinder(cylinder, 8, 2);

        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    /// <summary>Verifies Tessellate(BezierSurface) produces expected grid dimensions.</summary>
    [Fact]
    public void Tessellate_BezierSurface_ProducesGridVertices()
    {
        int uRes = 4;
        int vRes = 4;
        var controlPoints = ImmutableArray.Create(
            ImmutableArray.Create(
                new Point3D(0, 0, 0), new Point3D(1, 0, 0),
                new Point3D(2, 0, 0)),
            ImmutableArray.Create(
                new Point3D(0, 1, 0), new Point3D(1, 1, 1),
                new Point3D(2, 1, 0)),
            ImmutableArray.Create(
                new Point3D(0, 2, 0), new Point3D(1, 2, 0),
                new Point3D(2, 2, 0)));
        var surface = new BezierSurface(controlPoints);

        var mesh = SurfaceTessellator.Tessellate(surface, uRes, vRes);

        int expectedVertices = (uRes + 1) * (vRes + 1);
        mesh.VertexCount.Should().Be(expectedVertices);
    }

    /// <summary>Verifies Tessellate(BezierSurface) produces triangles.</summary>
    [Fact]
    public void Tessellate_BezierSurface_ProducesTriangles()
    {
        var controlPoints = ImmutableArray.Create(
            ImmutableArray.Create(
                new Point3D(0, 0, 0), new Point3D(1, 0, 0)),
            ImmutableArray.Create(
                new Point3D(0, 1, 0), new Point3D(1, 1, 1)));
        var surface = new BezierSurface(controlPoints);

        var mesh = SurfaceTessellator.Tessellate(surface, 4, 4);

        mesh.TriangleCount.Should().Be(4 * 4 * 2);
    }

    /// <summary>Verifies TessellateSphere vertex positions are approximately on the sphere surface.</summary>
    [Fact]
    public void TessellateSphere_VerticesOnSphereSurface()
    {
        double radius = 2.0;
        var sphere = new Sphere3D(Point3D.Origin, radius);

        var mesh = SurfaceTessellator.TessellateSphere(sphere, 8, 12);

        foreach (Vertex v in mesh.Vertices)
        {
            double dist = v.Position.DistanceTo(Point3D.Origin);
            dist.Should().BeApproximately(radius, Tolerance);
        }
    }

    /// <summary>Verifies TessellateCylinder with minimal divisions still produces geometry.</summary>
    [Fact]
    public void TessellateCylinder_MinimalDivisions_ProducesGeometry()
    {
        var cylinder = new Cylinder3D(Point3D.Origin, 1.0, 1.0);

        var mesh = SurfaceTessellator.TessellateCylinder(cylinder, 3, 1);

        mesh.VertexCount.Should().BeGreaterThan(0);
        mesh.TriangleCount.Should().BeGreaterThan(0);
    }
}
