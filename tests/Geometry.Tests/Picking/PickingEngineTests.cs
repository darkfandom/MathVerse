using System.Collections.Immutable;

namespace MathVerse.Geometry.Tests.Picking;

/// <summary>Tests for the <see cref="PickingEngine"/> class.</summary>
public class PickingEngineTests
{
    private static TriangleMesh CreateTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(2, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 2, 0), Vector3D.UnitZ, (0.5, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    /// <summary>Verifies that PickMesh returns a hit when the ray intersects the mesh.</summary>
    [Fact]
    public void PickMesh_Hit_ReturnsHit()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(1, 1, -5), new Vector3D(0, 0, 1));

        var result = engine.PickMesh(ray, mesh);

        result.Hit.Should().BeTrue();
    }

    /// <summary>Verifies that PickMesh returns a miss when the ray misses the mesh.</summary>
    [Fact]
    public void PickMesh_Miss_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(10, 10, -5), new Vector3D(0, 0, 1));

        var result = engine.PickMesh(ray, mesh);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickBoundingBox returns a hit when the ray intersects the box.</summary>
    [Fact]
    public void PickBoundingBox_Hit_ReturnsHit()
    {
        var engine = new PickingEngine();
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var ray = new Ray(new Point3D(0.5, 0.5, -1), new Vector3D(0, 0, 1));

        var result = engine.PickBoundingBox(ray, box);

        result.Hit.Should().BeTrue();
    }

    /// <summary>Verifies that PickBoundingBox returns a miss when the ray misses the box.</summary>
    [Fact]
    public void PickBoundingBox_Miss_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var ray = new Ray(new Point3D(5, 5, -1), new Vector3D(0, 0, 1));

        var result = engine.PickBoundingBox(ray, box);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickScene returns the closest hit when multiple geometry nodes exist.</summary>
    [Fact]
    public void PickScene_MultipleNodes_ReturnsClosest()
    {
        var engine = new PickingEngine();
        var scene = new Scene();
        var mesh = CreateTriangleMesh();

        var node1 = new GeometryNode("near", mesh);
        node1.LocalTransform = Transform3D.Translation(0, 0, 0);
        var node2 = new GeometryNode("far", mesh);
        node2.LocalTransform = Transform3D.Translation(0, 0, 10);

        scene.AddRootNode(node1);
        scene.AddRootNode(node2);

        var ray = new Ray(new Point3D(1, 1, -5), new Vector3D(0, 0, 1));

        var result = engine.PickScene(ray, scene);

        result.Hit.Should().BeTrue();
        result.Distance.Should().BeLessThan(10.0);
    }

    /// <summary>Verifies that PickScene returns a miss when no geometry is hit.</summary>
    [Fact]
    public void PickScene_NoHits_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var scene = new Scene();
        var mesh = CreateTriangleMesh();

        var node = new GeometryNode("mesh", mesh);
        scene.AddRootNode(node);

        var ray = new Ray(new Point3D(100, 100, -5), new Vector3D(0, 0, 1));

        var result = engine.PickScene(ray, scene);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickMesh returns correct hit point.</summary>
    [Fact]
    public void PickMesh_Hit_ReturnsCorrectHitPoint()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(1, 1, -5), new Vector3D(0, 0, 1));

        var result = engine.PickMesh(ray, mesh);

        result.HitPoint.Z.Should().BeApproximately(0.0, 1e-5);
    }

    /// <summary>Verifies that PickMesh with ray parallel to triangle returns miss.</summary>
    [Fact]
    public void PickMesh_RayParallelToTriangle_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(1, 1, 0), new Vector3D(1, 0, 0));

        var result = engine.PickMesh(ray, mesh);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickBoundingBox returns correct distance for a hit.</summary>
    [Fact]
    public void PickBoundingBox_Hit_ReturnsCorrectDistance()
    {
        var engine = new PickingEngine();
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var ray = new Ray(new Point3D(0.5, 0.5, -2), new Vector3D(0, 0, 1));

        var result = engine.PickBoundingBox(ray, box);

        result.Distance.Should().BeApproximately(2.0, 1e-5);
    }

    /// <summary>Verifies that PickMesh returns miss when ray direction is away from mesh.</summary>
    [Fact]
    public void PickMesh_RayDirectionAway_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(1, 1, 5), new Vector3D(0, 0, 1));

        var result = engine.PickMesh(ray, mesh);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickScene with empty scene returns miss.</summary>
    [Fact]
    public void PickScene_EmptyScene_ReturnsMiss()
    {
        var engine = new PickingEngine();
        var scene = new Scene();
        var ray = new Ray(Point3D.Origin, Vector3D.UnitZ);

        var result = engine.PickScene(ray, scene);

        result.Hit.Should().BeFalse();
    }

    /// <summary>Verifies that PickMesh sets the triangle index on hit.</summary>
    [Fact]
    public void PickMesh_Hit_SetsTriangleIndex()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var ray = new Ray(new Point3D(1, 1, -5), new Vector3D(0, 0, 1));

        var result = engine.PickMesh(ray, mesh);

        result.TriangleIndex.Should().Be(0);
    }

    /// <summary>Verifies that PickScene returns farthest hit distance greater than closest.</summary>
    [Fact]
    public void PickScene_ClosestIsCloserThanFarthest()
    {
        var engine = new PickingEngine();
        var mesh = CreateTriangleMesh();
        var scene = new Scene();

        var nearNode = new GeometryNode("near", mesh);
        nearNode.LocalTransform = Transform3D.Translation(0, 0, 2);
        var farNode = new GeometryNode("far", mesh);
        farNode.LocalTransform = Transform3D.Translation(0, 0, 8);

        scene.AddRootNode(nearNode);
        scene.AddRootNode(farNode);

        var ray = new Ray(new Point3D(1, 1, -5), new Vector3D(0, 0, 1));

        var result = engine.PickScene(ray, scene);

        result.Hit.Should().BeTrue();
        result.Distance.Should().BeApproximately(5.0, 0.1);
    }

    /// <summary>Verifies that PickBoundingBox from inside the box returns a hit.</summary>
    [Fact]
    public void PickBoundingBox_FromInside_ReturnsHit()
    {
        var engine = new PickingEngine();
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var ray = new Ray(new Point3D(1, 1, 1), new Vector3D(0, 0, 1));

        var result = engine.PickBoundingBox(ray, box);

        result.Hit.Should().BeTrue();
    }
}
