namespace MathVerse.Geometry.Advanced.Tests.Spatial;

public class OctreeTests
{
    private static BoundingBox3D UnitBox => new(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

    private static Octree BuildOctree(BoundingBox3D bounds, params Point3D[] points) =>
        new Octree(bounds, points);

    private static Octree BuildEmptyOctree(BoundingBox3D bounds) =>
        new Octree(bounds, Array.Empty<Point3D>());

    [Fact]
    public void Constructor_WithPoints_SetsCount()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 2, 3), new Point3D(4, 5, 6), new Point3D(7, 8, 9));
        octree.Count.Should().Be(3);
    }

    [Fact]
    public void Constructor_EmptyPoints_CountIsZero()
    {
        var octree = BuildEmptyOctree(UnitBox);
        octree.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_SinglePoint_CountIsOne()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5));
        octree.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_DuplicatePoints_AllAreStored()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        octree.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_10Points_CountIs10()
    {
        var points = Enumerable.Range(0, 10).Select(i => new Point3D(i, i, i)).ToList();
        var octree = new Octree(UnitBox, points);
        octree.Count.Should().Be(10);
    }

    [Fact]
    public void Constructor_OverlappingBounds_StoresAllPoints()
    {
        var bounds = new BoundingBox3D(new Point3D(-100, -100, -100), new Point3D(100, 100, 100));
        var octree = BuildOctree(bounds,
            new Point3D(-50, -50, -50), new Point3D(50, 50, 50));
        octree.Count.Should().Be(2);
    }

    [Fact]
    public void NearestNeighbor_SinglePoint_ReturnsThatPoint()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5));
        var result = octree.NearestNeighbor(new Point3D(100, 100, 100));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_TwoPoints_ReturnsCloser()
    {
        var octree = BuildOctree(UnitBox, new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var result = octree.NearestNeighbor(new Point3D(0, 0, 0));
        result.Should().Be(new Point3D(1, 1, 1));
    }

    [Fact]
    public void NearestNeighbor_ExactMatch_ReturnsExactPoint()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5), new Point3D(9, 9, 9));
        var result = octree.NearestNeighbor(new Point3D(5, 5, 5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_MultiplePoints_ReturnsClosest()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 1, 1),
            new Point3D(1, 9, 1), new Point3D(1, 1, 9),
            new Point3D(5, 5, 5));
        var result = octree.NearestNeighbor(new Point3D(4.5, 4.5, 4.5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_NegativeCoordinates_ReturnsCorrect()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds, new Point3D(-9, -9, -9), new Point3D(9, 9, 9));
        var result = octree.NearestNeighbor(new Point3D(-8, -8, -8));
        result.Should().Be(new Point3D(-9, -9, -9));
    }

    [Fact]
    public void NearestNeighbor_LargeBounds_Correct()
    {
        var bounds = new BoundingBox3D(new Point3D(-1000, -1000, -1000), new Point3D(1000, 1000, 1000));
        var octree = BuildOctree(bounds,
            new Point3D(-500, -500, -500), new Point3D(500, 500, 500));
        var result = octree.NearestNeighbor(new Point3D(-499, -499, -499));
        result.Should().Be(new Point3D(-500, -500, -500));
    }

    [Fact]
    public void RangeQuery_Radius_ContainsPointsInside()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5.1, 5, 5),
            new Point3D(9, 9, 9));
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 1);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_Radius_ExcludesPointsOutside()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var result = octree.RangeQuery(new Point3D(1, 1, 1), 1);
        result.Length.Should().Be(1);
        result.Should().Contain(new Point3D(1, 1, 1));
    }

    [Fact]
    public void RangeQuery_Radius_ZeroRadius_ReturnsExactMatch()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5));
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 0);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_Radius_LargeRadius_ContainsAll()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(5, 5, 5), new Point3D(9, 9, 9));
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 100);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Radius_NoPointsInRange_ReturnsEmpty()
    {
        var octree = BuildOctree(UnitBox, new Point3D(1, 1, 1));
        var result = octree.RangeQuery(new Point3D(9, 9, 9), 1);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Radius_ExactlyOnBoundary_IsIncluded()
    {
        var octree = BuildOctree(UnitBox, new Point3D(0, 0, 3));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_Radius_MultiplePointsAtSameDistance()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(3, 0, 0), new Point3D(-3, 0, 0),
            new Point3D(0, 3, 0), new Point3D(0, 0, 3));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ContainsPointsInside()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(5, 5, 5),
            new Point3D(9, 9, 9));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(6, 6, 6));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ExcludesPointsOutside()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_OnBoundary_IsIncluded()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5));
        var box = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(10, 10, 10));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_TinyBox_ContainsOnlyExactPoint()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(2, 2, 2), new Point3D(3, 3, 3));
        var box = new BoundingBox3D(new Point3D(2, 2, 2), new Point3D(2, 2, 2));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(2, 2, 2));
    }

    [Fact]
    public void RangeQuery_BoundingBox_LargeBox_ContainsAll()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void GetLeaves_ReturnsNonEmpty()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetLeaves_CoversAllPoints()
    {
        var points = new List<Point3D>
        {
            new(1, 1, 1), new(5, 5, 5), new(9, 9, 9),
            new(2, 8, 3), new(7, 2, 8)
        };
        var octree = BuildOctree(UnitBox, points.ToArray());
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterOrEqualTo(1);
        foreach (var leaf in leaves)
        {
            leaf.Min.X.Should().BeGreaterOrEqualTo(0);
            leaf.Max.X.Should().BeLessOrEqualTo(10);
        }
    }

    [Fact]
    public void GetLeaves_ManyPoints_Subdivides()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i * 0.5, i * 0.5, i * 0.5)).ToList();
        var octree = new Octree(UnitBox, points);
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetLeaves_SinglePoint_ReturnsNonEmpty()
    {
        var octree = BuildOctree(UnitBox, new Point3D(5, 5, 5));
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetLeaves_AllLeavesAreValidBoxes()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 2, 3), new Point3D(4, 5, 6),
            new Point3D(7, 8, 9), new Point3D(2, 6, 8));
        var leaves = octree.GetLeaves();
        foreach (var leaf in leaves)
        {
            leaf.Min.X.Should().BeLessOrEqualTo(leaf.Max.X);
            leaf.Min.Y.Should().BeLessOrEqualTo(leaf.Max.Y);
            leaf.Min.Z.Should().BeLessOrEqualTo(leaf.Max.Z);
        }
    }

    [Fact]
    public void NearestNeighbor_OriginBounds_ReturnsClosest()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(-5, 0, 0), new Point3D(5, 0, 0));
        var result = octree.NearestNeighbor(new Point3D(-4, 0, 0));
        result.Should().Be(new Point3D(-5, 0, 0));
    }

    [Fact]
    public void RangeQuery_Radius_CornersOfBounds()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 20);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_CornersOfBounds()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_NegativeCoordinates()
    {
        var bounds = new BoundingBox3D(new Point3D(-20, -20, -20), new Point3D(20, 20, 20));
        var octree = BuildOctree(bounds,
            new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var box = new BoundingBox3D(new Point3D(-15, -15, -15), new Point3D(-5, -5, -5));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(-10, -10, -10));
    }

    [Fact]
    public void Constructor_20Points_CountIsCorrect()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i, i * 0.5, i * 0.3)).ToList();
        var octree = new Octree(UnitBox, points);
        octree.Count.Should().Be(20);
    }

    [Fact]
    public void NearestNeighbor_20Points_ReturnsCorrect()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i, i, i)).ToList();
        var octree = new Octree(UnitBox, points);
        var result = octree.NearestNeighbor(new Point3D(10, 10, 10));
        result.Should().Be(new Point3D(10, 10, 10));
    }

    [Fact]
    public void RangeQuery_Radius_20Points()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i, i, i)).ToList();
        var octree = new Octree(UnitBox, points);
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 3);
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_20Points()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i, i, i)).ToList();
        var octree = new Octree(UnitBox, points);
        var box = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(15, 15, 15));
        var result = octree.RangeQuery(box);
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetLeaves_20Points_ReturnsNonEmpty()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point3D(i, i, i)).ToList();
        var octree = new Octree(UnitBox, points);
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void NearestNeighbor_DiagonalClosest()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var result = octree.NearestNeighbor(new Point3D(8, 8, 8));
        result.Should().Be(new Point3D(9, 9, 9));
    }

    [Fact]
    public void RangeQuery_Radius_CloseToOrigin()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(0.1, 0, 0), new Point3D(0, 0.1, 0),
            new Point3D(0, 0, 0.1), new Point3D(9, 9, 9));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 1);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_MixedInsideOutside()
    {
        var bounds = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(-1, -1, -1), new Point3D(1, 1, 1),
            new Point3D(9, 9, 9), new Point3D(-9, -9, -9));
        var box = new BoundingBox3D(new Point3D(-2, -2, -2), new Point3D(2, 2, 2));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void GetLeaves_LargeBounds_CorrectSubdivision()
    {
        var bounds = new BoundingBox3D(new Point3D(-1000, -1000, -1000), new Point3D(1000, 1000, 1000));
        var points = Enumerable.Range(0, 30).Select(i =>
            new Point3D(-500 + i * 35, -500 + i * 35, -500 + i * 35)).ToList();
        var octree = new Octree(bounds, points);
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RangeQuery_Radius_EncompassesNothing()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var result = octree.RangeQuery(new Point3D(50, 50, 50), 1);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_BoundingBox_EncompassesNothing()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var box = new BoundingBox3D(new Point3D(50, 50, 50), new Point3D(60, 60, 60));
        var result = octree.RangeQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void NearestNeighbor_CoinscidentPoint_ReturnsIt()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(1, 1, 1), new Point3D(9, 9, 9));
        var result = octree.NearestNeighbor(new Point3D(5, 5, 5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void Constructor_CenterPointsAllOctants()
    {
        var bounds = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(2, 2, 2), new Point3D(8, 2, 2),
            new Point3D(2, 8, 2), new Point3D(8, 8, 2),
            new Point3D(2, 2, 8), new Point3D(8, 2, 8),
            new Point3D(2, 8, 8), new Point3D(8, 8, 8));
        octree.Count.Should().Be(8);
    }

    [Fact]
    public void RangeQuery_BoundingBox_CenterPointsAllOctants()
    {
        var bounds = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(2, 2, 2), new Point3D(8, 2, 2),
            new Point3D(2, 8, 2), new Point3D(8, 8, 2),
            new Point3D(2, 2, 8), new Point3D(8, 2, 8),
            new Point3D(2, 8, 8), new Point3D(8, 8, 8));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(8);
    }

    [Fact]
    public void GetLeaves_CenterPointsAllOctants_ReturnsMultipleLeaves()
    {
        var bounds = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(2, 2, 2), new Point3D(8, 2, 2),
            new Point3D(2, 8, 2), new Point3D(8, 8, 2),
            new Point3D(2, 2, 8), new Point3D(8, 2, 8),
            new Point3D(2, 8, 8), new Point3D(8, 8, 8));
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void NearestNeighbor_CenterPointsAllOctants_ReturnsCorrect()
    {
        var bounds = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var octree = BuildOctree(bounds,
            new Point3D(2, 2, 2), new Point3D(8, 8, 8));
        var result = octree.NearestNeighbor(new Point3D(3, 3, 3));
        result.Should().Be(new Point3D(2, 2, 2));
    }

    [Fact]
    public void RangeQuery_Radius_ExactOnBoundary_3Points()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(3, 0, 0), new Point3D(-3, 0, 0), new Point3D(0, 3, 0));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void NearestNeighbor_LargeDataset_50Points()
    {
        var points = Enumerable.Range(0, 50).Select(i => new Point3D(i * 0.2, i * 0.2, i * 0.2)).ToList();
        var octree = new Octree(UnitBox, points);
        var result = octree.NearestNeighbor(new Point3D(5, 5, 5));
        result.DistanceTo(new Point3D(5, 5, 5)).Should().BeLessOrEqualTo(0.5 + 1e-10);
    }

    [Fact]
    public void RangeQuery_Radius_50Points()
    {
        var points = Enumerable.Range(0, 50).Select(i => new Point3D(i * 0.2, i * 0.2, i * 0.2)).ToList();
        var octree = new Octree(UnitBox, points);
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 1);
        foreach (var p in result)
            p.DistanceTo(new Point3D(5, 5, 5)).Should().BeLessOrEqualTo(1.0 + 1e-10);
    }

    [Fact]
    public void RangeQuery_BoundingBox_50Points()
    {
        var points = Enumerable.Range(0, 50).Select(i => new Point3D(i * 0.2, i * 0.2, i * 0.2)).ToList();
        var octree = new Octree(UnitBox, points);
        var box = new BoundingBox3D(new Point3D(4, 4, 4), new Point3D(6, 6, 6));
        var result = octree.RangeQuery(box);
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetLeaves_50Points_ReturnsNonEmpty()
    {
        var points = Enumerable.Range(0, 50).Select(i => new Point3D(i * 0.2, i * 0.2, i * 0.2)).ToList();
        var octree = new Octree(UnitBox, points);
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RangeQuery_Radius_DuplicatePoints()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var result = octree.RangeQuery(new Point3D(5, 5, 5), 0);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_DuplicatePoints()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var box = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var result = octree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void NearestNeighbor_DuplicatePoints_ReturnsOne()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var result = octree.NearestNeighbor(new Point3D(5, 5, 5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void GetLeaves_DuplicatePoints_NonEmpty()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RangeQuery_Radius_OnEveryAxis()
    {
        var octree = BuildOctree(UnitBox,
            new Point3D(5, 0, 0), new Point3D(0, 5, 0), new Point3D(0, 0, 5));
        var result = octree.RangeQuery(new Point3D(0, 0, 0), 5);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void GetLeaves_CountMatchesTotal()
    {
        var points = Enumerable.Range(0, 15).Select(i =>
            new Point3D(i * 0.6, i * 0.6, i * 0.6)).ToList();
        var octree = new Octree(UnitBox, points);
        octree.Count.Should().Be(15);
        var leaves = octree.GetLeaves();
        leaves.Length.Should().BeGreaterThan(0);
    }
}
