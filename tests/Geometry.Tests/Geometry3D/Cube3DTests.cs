namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Cube3D"/> struct.</summary>
public class Cube3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Volume equals sideLength cubed.</summary>
    [Fact]
    public void Volume_EqualsSideCubed()
    {
        var cube = new Cube3D(Point3D.Origin, 3.0);

        cube.Volume.Should().BeApproximately(27.0, Tolerance);
    }

    /// <summary>Verifies Volume with unit cube equals 1.</summary>
    [Fact]
    public void Volume_UnitCube_ReturnsOne()
    {
        var cube = new Cube3D(Point3D.Origin, 1.0);

        cube.Volume.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies SurfaceArea equals 6 * side^2.</summary>
    [Fact]
    public void SurfaceArea_Equals6TimesSideSquared()
    {
        var cube = new Cube3D(Point3D.Origin, 2.0);

        cube.SurfaceArea.Should().BeApproximately(24.0, Tolerance);
    }

    /// <summary>Verifies Vertices returns exactly 8 points.</summary>
    [Fact]
    public void Vertices_ReturnsEightPoints()
    {
        var cube = new Cube3D(Point3D.Origin, 2.0);

        cube.Vertices.Length.Should().Be(8);
    }

    /// <summary>Verifies all vertices are at the correct distance from center.</summary>
    [Fact]
    public void Vertices_AllAtCorrectDistanceFromCenter()
    {
        var center = new Point3D(1, 2, 3);
        var cube = new Cube3D(center, 2.0);
        double expectedDist = System.Math.Sqrt(3);

        foreach (var v in cube.Vertices)
        {
            v.DistanceTo(center).Should().BeApproximately(expectedDist, Tolerance);
        }
    }

    /// <summary>Verifies Faces returns exactly 6 quads.</summary>
    [Fact]
    public void Faces_ReturnsSixQuads()
    {
        var cube = new Cube3D(Point3D.Origin, 2.0);

        cube.Faces.Length.Should().Be(6);
    }

    /// <summary>Verifies Contains returns true for the center.</summary>
    [Fact]
    public void Contains_Center_ReturnsTrue()
    {
        var cube = new Cube3D(new Point3D(5, 5, 5), 4.0);

        cube.Contains(new Point3D(5, 5, 5)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns true for a point on the face.</summary>
    [Fact]
    public void Contains_FacePoint_ReturnsTrue()
    {
        var cube = new Cube3D(Point3D.Origin, 2.0);

        cube.Contains(new Point3D(1, 0, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for an exterior point.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ReturnsFalse()
    {
        var cube = new Cube3D(Point3D.Origin, 2.0);

        cube.Contains(new Point3D(5, 0, 0)).Should().BeFalse();
    }

    /// <summary>Verifies ToBoundingBox encloses the cube.</summary>
    [Fact]
    public void ToBoundingBox_EnclosesCube()
    {
        var cube = new Cube3D(Point3D.Origin, 4.0);

        var bbox = cube.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Z.Should().BeApproximately(-2.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(2.0, Tolerance);
        bbox.Max.Y.Should().BeApproximately(2.0, Tolerance);
        bbox.Max.Z.Should().BeApproximately(2.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox center matches the cube center.</summary>
    [Fact]
    public void ToBoundingBox_CenterMatchesCubeCenter()
    {
        var cube = new Cube3D(new Point3D(3, 4, 5), 6.0);

        var bbox = cube.ToBoundingBox();

        bbox.Center.X.Should().BeApproximately(3.0, Tolerance);
        bbox.Center.Y.Should().BeApproximately(4.0, Tolerance);
        bbox.Center.Z.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies all vertices are inside the bounding box.</summary>
    [Fact]
    public void Vertices_AllInsideBoundingBox()
    {
        var cube = new Cube3D(Point3D.Origin, 3.0);
        var bbox = cube.ToBoundingBox();

        foreach (var v in cube.Vertices)
        {
            bbox.Contains(v).Should().BeTrue();
        }
    }

    /// <summary>Verifies a non-centered cube contains its center.</summary>
    [Fact]
    public void Contains_NonCenteredCube_ContainsCenter()
    {
        var cube = new Cube3D(new Point3D(10, 20, 30), 4.0);

        cube.Contains(new Point3D(10, 20, 30)).Should().BeTrue();
    }

    /// <summary>Verifies SurfaceArea with side 1 equals 6.</summary>
    [Fact]
    public void SurfaceArea_UnitCube_ReturnsSix()
    {
        var cube = new Cube3D(Point3D.Origin, 1.0);

        cube.SurfaceArea.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies volume of a cube at non-origin is the same.</summary>
    [Fact]
    public void Volume_NonOriginCenter_SameVolume()
    {
        var cube = new Cube3D(new Point3D(100, 200, 300), 5.0);

        cube.Volume.Should().BeApproximately(125.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox width equals the side length.</summary>
    [Fact]
    public void ToBoundingBox_WidthEqualsSideLength()
    {
        var cube = new Cube3D(Point3D.Origin, 7.0);

        var bbox = cube.ToBoundingBox();

        bbox.Width.Should().BeApproximately(7.0, Tolerance);
        bbox.Height.Should().BeApproximately(7.0, Tolerance);
        bbox.Depth.Should().BeApproximately(7.0, Tolerance);
    }
}
