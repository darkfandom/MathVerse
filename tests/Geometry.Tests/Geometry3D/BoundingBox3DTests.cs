namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="BoundingBox3D"/> struct.</summary>
public class BoundingBox3DTests
{
    private const double Tolerance = 1e-10;

    private static readonly BoundingBox3D UnitBox = new(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

    /// <summary>Verifies Width returns the X extent.</summary>
    [Fact]
    public void Width_ReturnsXExtent()
    {
        var box = new BoundingBox3D(new Point3D(-1, 0, 0), new Point3D(3, 0, 0));

        box.Width.Should().BeApproximately(4.0, Tolerance);
    }

    /// <summary>Verifies Height returns the Y extent.</summary>
    [Fact]
    public void Height_ReturnsYExtent()
    {
        var box = new BoundingBox3D(new Point3D(0, -2, 0), new Point3D(0, 5, 0));

        box.Height.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies Depth returns the Z extent.</summary>
    [Fact]
    public void Depth_ReturnsZExtent()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, -3), new Point3D(0, 0, 4));

        box.Depth.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies Center is the midpoint of min and max.</summary>
    [Fact]
    public void Center_ReturnsMidpoint()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 20, 30));

        var center = box.Center;

        center.X.Should().BeApproximately(5.0, Tolerance);
        center.Y.Should().BeApproximately(10.0, Tolerance);
        center.Z.Should().BeApproximately(15.0, Tolerance);
    }

    /// <summary>Verifies Volume equals width * height * depth.</summary>
    [Fact]
    public void Volume_EqualsWidthTimesHeightTimesDepth()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 3, 4));

        box.Volume.Should().BeApproximately(24.0, Tolerance);
    }

    /// <summary>Verifies SurfaceArea matches the formula.</summary>
    [Fact]
    public void SurfaceArea_MatchesFormula()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 3, 4));

        double expected = 2.0 * (2 * 3 + 3 * 4 + 4 * 2);

        box.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Contains(Point) for an interior point.</summary>
    [Fact]
    public void ContainsPoint_InteriorPoint_ReturnsTrue()
    {
        UnitBox.Contains(new Point3D(0.5, 0.5, 0.5)).Should().BeTrue();
    }

    /// <summary>Verifies Contains(Point) for a point on the boundary.</summary>
    [Fact]
    public void ContainsPoint_BoundaryPoint_ReturnsTrue()
    {
        UnitBox.Contains(new Point3D(0, 0, 0)).Should().BeTrue();
        UnitBox.Contains(new Point3D(1, 1, 1)).Should().BeTrue();
    }

    /// <summary>Verifies Contains(Point) for an exterior point.</summary>
    [Fact]
    public void ContainsPoint_ExteriorPoint_ReturnsFalse()
    {
        UnitBox.Contains(new Point3D(2, 2, 2)).Should().BeFalse();
    }

    /// <summary>Verifies Contains(BoundingBox) for a smaller box inside.</summary>
    [Fact]
    public void ContainsBoundingBox_SmallerBoxInside_ReturnsTrue()
    {
        var inner = new BoundingBox3D(new Point3D(0.1, 0.1, 0.1), new Point3D(0.9, 0.9, 0.9));

        UnitBox.Contains(inner).Should().BeTrue();
    }

    /// <summary>Verifies Contains(BoundingBox) for a larger box returns false.</summary>
    [Fact]
    public void ContainsBoundingBox_LargerBox_ReturnsFalse()
    {
        var outer = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(2, 2, 2));

        UnitBox.Contains(outer).Should().BeFalse();
    }

    /// <summary>Verifies Intersects for overlapping boxes.</summary>
    [Fact]
    public void Intersects_OverlappingBoxes_ReturnsTrue()
    {
        var a = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var b = new BoundingBox3D(new Point3D(1, 1, 1), new Point3D(3, 3, 3));

        a.Intersects(b).Should().BeTrue();
    }

    /// <summary>Verifies Intersects for non-overlapping boxes.</summary>
    [Fact]
    public void Intersects_NonOverlappingBoxes_ReturnsFalse()
    {
        var a = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var b = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(6, 6, 6));

        a.Intersects(b).Should().BeFalse();
    }

    /// <summary>Verifies Union encloses both boxes.</summary>
    [Fact]
    public void Union_EnclosesBothBoxes()
    {
        var a = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var b = new BoundingBox3D(new Point3D(1, 1, 1), new Point3D(4, 4, 4));

        var result = a.Union(b);

        result.Contains(a.Min).Should().BeTrue();
        result.Contains(b.Max).Should().BeTrue();
        result.Min.X.Should().BeApproximately(0.0, Tolerance);
        result.Max.X.Should().BeApproximately(4.0, Tolerance);
    }

    /// <summary>Verifies Inflate expands the box on all sides.</summary>
    [Fact]
    public void Inflate_ExpandsOnAllSides()
    {
        var box = new BoundingBox3D(new Point3D(1, 1, 1), new Point3D(2, 2, 2));

        var inflated = box.Inflate(1.0);

        inflated.Min.X.Should().BeApproximately(0.0, Tolerance);
        inflated.Min.Y.Should().BeApproximately(0.0, Tolerance);
        inflated.Min.Z.Should().BeApproximately(0.0, Tolerance);
        inflated.Max.X.Should().BeApproximately(3.0, Tolerance);
        inflated.Max.Y.Should().BeApproximately(3.0, Tolerance);
        inflated.Max.Z.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies Transform with identity returns the same box.</summary>
    [Fact]
    public void Transform_Identity_ReturnsSameBox()
    {
        var box = new BoundingBox3D(new Point3D(1, 2, 3), new Point3D(4, 5, 6));

        var result = box.Transform(Transform3D.Identity);

        result.Min.Should().Be(box.Min);
        result.Max.Should().Be(box.Max);
    }

    /// <summary>Verifies Transform with translation shifts the box.</summary>
    [Fact]
    public void Transform_Translation_ShiftsBox()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var t = Transform3D.Translation(5, 5, 5);

        var result = box.Transform(t);

        result.Min.X.Should().BeApproximately(5.0, Tolerance);
        result.Min.Y.Should().BeApproximately(5.0, Tolerance);
        result.Min.Z.Should().BeApproximately(5.0, Tolerance);
        result.Max.X.Should().BeApproximately(6.0, Tolerance);
        result.Max.Y.Should().BeApproximately(6.0, Tolerance);
        result.Max.Z.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies Corners returns exactly 8 points.</summary>
    [Fact]
    public void Corners_ReturnsEightPoints()
    {
        UnitBox.Corners.Length.Should().Be(8);
    }

    /// <summary>Verifies Corners all lie inside the box.</summary>
    [Fact]
    public void Corners_AllInsideBox()
    {
        foreach (var corner in UnitBox.Corners)
        {
            UnitBox.Contains(corner).Should().BeTrue();
        }
    }

    /// <summary>Verifies FromPoints creates a box enclosing all points.</summary>
    [Fact]
    public void FromPoints_EnclosesAllPoints()
    {
        var points = new[]
        {
            new Point3D(-1, 2, 3),
            new Point3D(5, -2, 7),
            new Point3D(3, 8, -1)
        };

        var box = BoundingBox3D.FromPoints(points);

        box.Min.X.Should().BeApproximately(-1.0, Tolerance);
        box.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        box.Min.Z.Should().BeApproximately(-1.0, Tolerance);
        box.Max.X.Should().BeApproximately(5.0, Tolerance);
        box.Max.Y.Should().BeApproximately(8.0, Tolerance);
        box.Max.Z.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies FromPoints with empty collection returns origin box.</summary>
    [Fact]
    public void FromPoints_EmptyCollection_ReturnsOriginBox()
    {
        var box = BoundingBox3D.FromPoints(Array.Empty<Point3D>());

        box.Min.Should().Be(Point3D.Origin);
        box.Max.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies Volume is zero for a flat box.</summary>
    [Fact]
    public void Volume_FlatBox_ReturnsZero()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(5, 5, 0));

        box.Volume.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies indexer returns correct corners.</summary>
    [Fact]
    public void Indexer_ReturnsCorrectCorners()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 2, 3));

        box[0].Should().Be(new Point3D(0, 0, 0));
        box[6].Should().Be(new Point3D(1, 2, 3));
    }
}
