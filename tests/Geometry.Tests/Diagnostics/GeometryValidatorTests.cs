using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Geometry.Tests.Diagnostics;

/// <summary>Tests for the <see cref="GeometryValidator"/> class.</summary>
public class GeometryValidatorTests
{
    /// <summary>Verifies that Validate returns success for a valid mesh.</summary>
    [Fact]
    public void Validate_ValidMesh_ReturnsSuccess()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var result = GeometryValidator.Validate(mesh);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Validate returns failure for a null mesh.</summary>
    [Fact]
    public void Validate_NullMesh_ReturnsFailure()
    {
        var result = GeometryValidator.Validate((TriangleMesh)null!);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for an empty mesh.</summary>
    [Fact]
    public void Validate_EmptyMesh_ReturnsFailure()
    {
        var result = GeometryValidator.Validate(TriangleMesh.Empty);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for a degenerate triangle (zero area).</summary>
    [Fact]
    public void Validate_DegenerateTriangle_ReturnsFailure()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 0));

        var result = GeometryValidator.Validate(tri);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns success for a valid triangle.</summary>
    [Fact]
    public void Validate_ValidTriangle_ReturnsSuccess()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));

        var result = GeometryValidator.Validate(tri);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Validate returns failure for a circle with NaN center.</summary>
    [Fact]
    public void Validate_CircleWithNaN_ReturnsFailure()
    {
        var circle = new Circle2D(new Point2D(double.NaN, 0), 1.0);

        var result = GeometryValidator.Validate(circle);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for a circle with negative radius.</summary>
    [Fact]
    public void Validate_CircleNegativeRadius_ReturnsFailure()
    {
        var circle = new Circle2D(new Point2D(0, 0), -1.0);

        var result = GeometryValidator.Validate(circle);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for a sphere with negative radius.</summary>
    [Fact]
    public void Validate_SphereNegativeRadius_ReturnsFailure()
    {
        var sphere = new Sphere3D(new Point3D(0, 0, 0), -1.0);

        var result = GeometryValidator.Validate(sphere);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for a polygon with fewer than 3 vertices.</summary>
    [Fact]
    public void Validate_PolygonLessThan3_ReturnsFailure()
    {
        var polygon = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 0)));

        var result = GeometryValidator.Validate(polygon);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns failure for a self-intersecting polygon.</summary>
    [Fact]
    public void Validate_SelfIntersectingPolygon_ReturnsFailure()
    {
        var polygon = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 2),
            new Point2D(2, 0),
            new Point2D(0, 2)));

        var result = GeometryValidator.Validate(polygon);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns success for a valid 3D triangle.</summary>
    [Fact]
    public void Validate_ValidTriangle3D_ReturnsSuccess()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));

        var result = GeometryValidator.Validate(tri);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Validate returns failure for a degenerate 3D triangle.</summary>
    [Fact]
    public void Validate_DegenerateTriangle3D_ReturnsFailure()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(2, 0, 0));

        var result = GeometryValidator.Validate(tri);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that Validate returns success for a valid sphere.</summary>
    [Fact]
    public void Validate_ValidSphere_ReturnsSuccess()
    {
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var result = GeometryValidator.Validate(sphere);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Validate returns success for a valid circle.</summary>
    [Fact]
    public void Validate_ValidCircle_ReturnsSuccess()
    {
        var circle = new Circle2D(new Point2D(0, 0), 1.0);

        var result = GeometryValidator.Validate(circle);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that Validate for null mesh reports the correct error message.</summary>
    [Fact]
    public void Validate_NullMesh_ErrorMessage()
    {
        var result = GeometryValidator.Validate((TriangleMesh)null!);

        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
