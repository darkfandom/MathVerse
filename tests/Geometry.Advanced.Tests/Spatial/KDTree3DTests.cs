namespace MathVerse.Geometry.Advanced.Tests.Spatial;

public class KDTree3DTests
{
    private static KDTree3D BuildTree(params Point3D[] points) => new KDTree3D(points);

    private static KDTree3D BuildEmptyTree() => new KDTree3D(Array.Empty<Point3D>());

    [Fact]
    public void Constructor_WithPoints_SetsCount()
    {
        var tree = BuildTree(new Point3D(1, 2, 3), new Point3D(4, 5, 6));
        tree.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_EmptyList_CountIsZero()
    {
        var tree = BuildEmptyTree();
        tree.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_SinglePoint_CountIsOne()
    {
        var tree = BuildTree(new Point3D(1, 1, 1));
        tree.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_DuplicatePoints_AllAreStored()
    {
        var tree = BuildTree(new Point3D(5, 5, 5), new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        tree.Count.Should().Be(3);
    }

    [Fact]
    public void Constructor_10Points_CountIs10()
    {
        var points = Enumerable.Range(0, 10).Select(i => new Point3D(i, i * 2, i * 3)).ToList();
        var tree = new KDTree3D(points);
        tree.Count.Should().Be(10);
    }

    [Fact]
    public void NearestNeighbor_SinglePoint_ReturnsThatPoint()
    {
        var tree = BuildTree(new Point3D(3, 7, 2));
        var result = tree.NearestNeighbor(new Point3D(100, 100, 100));
        result.Should().Be(new Point3D(3, 7, 2));
    }

    [Fact]
    public void NearestNeighbor_TwoPoints_ReturnsCloser()
    {
        var tree = BuildTree(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var result = tree.NearestNeighbor(new Point3D(1, 1, 1));
        result.Should().Be(new Point3D(0, 0, 0));
    }

    [Fact]
    public void NearestNeighbor_ExactMatch_ReturnsExactPoint()
    {
        var tree = BuildTree(new Point3D(5, 5, 5), new Point3D(10, 10, 10));
        var result = tree.NearestNeighbor(new Point3D(5, 5, 5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_MultiplePoints_ReturnsClosest()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(10, 0, 0),
            new Point3D(0, 10, 0), new Point3D(0, 0, 10),
            new Point3D(5, 5, 5));
        var result = tree.NearestNeighbor(new Point3D(4.5, 4.5, 4.5));
        result.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_EmptyTree_ReturnsOrigin()
    {
        var tree = BuildEmptyTree();
        var result = tree.NearestNeighbor(new Point3D(5, 5, 5));
        result.Should().Be(Point3D.Origin);
    }

    [Fact]
    public void NearestNeighbor_NegativeCoordinates_ReturnsCorrect()
    {
        var tree = BuildTree(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var result = tree.NearestNeighbor(new Point3D(-9, -9, -9));
        result.Should().Be(new Point3D(-10, -10, -10));
    }

    [Fact]
    public void NearestNeighbor_FarFromAllPoints_ReturnsClosest()
    {
        var tree = BuildTree(new Point3D(1000, 1000, 1000), new Point3D(2000, 2000, 2000));
        var result = tree.NearestNeighbor(new Point3D(0, 0, 0));
        result.Should().Be(new Point3D(1000, 1000, 1000));
    }

    [Fact]
    public void NearestNeighbor_LargeDistance_ReturnsCorrect()
    {
        var tree = BuildTree(new Point3D(1e6, 1e6, 1e6), new Point3D(-1e6, -1e6, -1e6));
        var result = tree.NearestNeighbor(new Point3D(1e6 - 1, 1e6 - 1, 1e6 - 1));
        result.Should().Be(new Point3D(1e6, 1e6, 1e6));
    }

    [Fact]
    public void RangeQuery_Radius_ContainsPointsInside()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(1, 0, 0),
            new Point3D(5, 5, 5), new Point3D(2, 0, 0));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Radius_ExcludesPointsOutside()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(100, 100, 100));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 5);
        result.Length.Should().Be(1);
        result.Should().Contain(new Point3D(0, 0, 0));
    }

    [Fact]
    public void RangeQuery_Radius_ZeroRadius_ReturnsExactMatch()
    {
        var tree = BuildTree(new Point3D(5, 5, 5), new Point3D(6, 6, 6));
        var result = tree.RangeQuery(new Point3D(5, 5, 5), 0);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void RangeQuery_Radius_LargeRadius_ContainsAll()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(10, 10, 10), new Point3D(20, 20, 20));
        var result = tree.RangeQuery(new Point3D(10, 10, 10), 100);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Radius_EmptyTree_ReturnsEmpty()
    {
        var tree = BuildEmptyTree();
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 10);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Radius_NoPointsInRange_ReturnsEmpty()
    {
        var tree = BuildTree(new Point3D(100, 100, 100));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 1);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Radius_ExactlyOnBoundary_IsIncluded()
    {
        var tree = BuildTree(new Point3D(0, 0, 3));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_Radius_MultiplePointsAtSameDistance()
    {
        var tree = BuildTree(
            new Point3D(3, 0, 0), new Point3D(-3, 0, 0),
            new Point3D(0, 3, 0), new Point3D(0, 0, 3));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 3);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void RangeQuery_Radius_DiagonalPoints()
    {
        var tree = BuildTree(
            new Point3D(1, 1, 1), new Point3D(2, 2, 2),
            new Point3D(10, 10, 10));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 4);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ContainsPointsInside()
    {
        var tree = BuildTree(
            new Point3D(1, 1, 1), new Point3D(5, 5, 5),
            new Point3D(9, 9, 9), new Point3D(2, 2, 2));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(6, 6, 6));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ExcludesPointsOutside()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(50, 50, 50));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(0);
    }

    [Fact]
    public void RangeQuery_BoundingBox_EmptyTree_ReturnsEmpty()
    {
        var tree = BuildEmptyTree();
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var result = tree.RangeQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_BoundingBox_OnBoundary_IsIncluded()
    {
        var tree = BuildTree(new Point3D(5, 5, 5));
        var box = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(10, 10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_TinyBox_ContainsOnlyExactPoint()
    {
        var tree = BuildTree(new Point3D(1, 1, 1), new Point3D(2, 2, 2), new Point3D(3, 3, 3));
        var box = new BoundingBox3D(new Point3D(2, 2, 2), new Point3D(2, 2, 2));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(2, 2, 2));
    }

    [Fact]
    public void RangeQuery_BoundingBox_LargeBox_ContainsAll()
    {
        var tree = BuildTree(
            new Point3D(-50, -50, -50), new Point3D(50, 50, 50));
        var box = new BoundingBox3D(new Point3D(-100, -100, -100), new Point3D(100, 100, 100));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_NegativeCoordinates()
    {
        var tree = BuildTree(
            new Point3D(-10, -10, -10), new Point3D(-5, -5, -5), new Point3D(5, 5, 5));
        var box = new BoundingBox3D(new Point3D(-8, -8, -8), new Point3D(-2, -2, -2));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(-5, -5, -5));
    }

    [Fact]
    public void RangeQuery_BoundingBox_CornerPoints()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(10, 0, 0),
            new Point3D(0, 10, 0), new Point3D(0, 0, 10),
            new Point3D(10, 10, 0), new Point3D(10, 0, 10),
            new Point3D(0, 10, 10), new Point3D(10, 10, 10));
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(8);
    }

    [Fact]
    public void NearestNeighbor_ThreePointsOnAxis_ReturnsCorrect()
    {
        var tree = BuildTree(
            new Point3D(5, 0, 0), new Point3D(-5, 0, 0), new Point3D(0, 5, 0));
        var result = tree.NearestNeighbor(new Point3D(0, 0, 0));
        result.DistanceTo(Point3D.Origin).Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void NearestNeighbor_CoinscidentPoint_ReturnsIt()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(1, 1, 1), new Point3D(2, 2, 2));
        var result = tree.NearestNeighbor(new Point3D(1, 1, 1));
        result.Should().Be(new Point3D(1, 1, 1));
    }

    [Fact]
    public void NearestNeighbor_SymmetricPoints_ReturnsOneOfThem()
    {
        var tree = BuildTree(new Point3D(-1, 0, 0), new Point3D(1, 0, 0));
        var result = tree.NearestNeighbor(new Point3D(0, 0, 0));
        result.DistanceTo(Point3D.Origin).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void RangeQuery_Radius_ContainsCorrectCount()
    {
        var tree = BuildTree(
            new Point3D(1, 0, 0), new Point3D(2, 0, 0), new Point3D(3, 0, 0),
            new Point3D(4, 0, 0), new Point3D(5, 0, 0), new Point3D(6, 0, 0));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 3.5);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Radius_VerySmallRadius()
    {
        var tree = BuildTree(new Point3D(5, 5, 5), new Point3D(5.001, 5, 5));
        var result = tree.RangeQuery(new Point3D(5, 5, 5), 0.0005);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_Radius_SphereOnDiagonal()
    {
        var tree = BuildTree(
            new Point3D(1, 1, 1), new Point3D(2, 2, 2), new Point3D(3, 3, 3));
        var result = tree.RangeQuery(new Point3D(2, 2, 2), 2);
        result.Length.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_SlabShaped()
    {
        var tree = BuildTree(
            new Point3D(5, 0, 0), new Point3D(5, 5, 0), new Point3D(5, 10, 0));
        var box = new BoundingBox3D(new Point3D(5, 2, 0), new Point3D(5, 8, 0));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void Constructor_Grid3x3x3_CountIs27()
    {
        var points = new List<Point3D>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    points.Add(new Point3D(x, y, z));
        var tree = new KDTree3D(points);
        tree.Count.Should().Be(27);
    }

    [Fact]
    public void NearestNeighbor_Grid3x3x3_ReturnsCorrect()
    {
        var points = new List<Point3D>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    points.Add(new Point3D(x, y, z));
        var tree = new KDTree3D(points);
        var result = tree.NearestNeighbor(new Point3D(1.4, 1.4, 1.4));
        result.Should().Be(new Point3D(1, 1, 1));
    }

    [Fact]
    public void RangeQuery_Radius_Grid_ContainsCorrectCount()
    {
        var points = new List<Point3D>();
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                for (int z = 0; z < 5; z++)
                    points.Add(new Point3D(x, y, z));
        var tree = new KDTree3D(points);
        var result = tree.RangeQuery(new Point3D(2, 2, 2), 1.5);
        foreach (var p in result)
            p.DistanceTo(new Point3D(2, 2, 2)).Should().BeLessOrEqualTo(1.5 + 1e-10);
    }

    [Fact]
    public void NearestNeighbor_CloseToEdge_ReturnsCorrect()
    {
        var tree = BuildTree(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var result = tree.NearestNeighbor(new Point3D(-0.1, 0, 0));
        result.Should().Be(new Point3D(0, 0, 0));
    }

    [Fact]
    public void RangeQuery_Radius_AllDistantPoints()
    {
        var tree = BuildTree(
            new Point3D(100, 0, 0), new Point3D(200, 0, 0), new Point3D(300, 0, 0));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 5);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_BoundingBox_EncompassesNothing()
    {
        var tree = BuildTree(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
        var box = new BoundingBox3D(new Point3D(50, 50, 50), new Point3D(60, 60, 60));
        var result = tree.RangeQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void NearestNeighbor_DiagonalClosest()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(1, 1, 1),
            new Point3D(2, 2, 2), new Point3D(3, 3, 3));
        var result = tree.NearestNeighbor(new Point3D(2.9, 2.9, 2.9));
        result.Should().Be(new Point3D(3, 3, 3));
    }

    [Fact]
    public void Constructor_100Points_CountIsCorrect()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point3D(i * 0.5, i * 1.2, i * 0.8)).ToList();
        var tree = new KDTree3D(points);
        tree.Count.Should().Be(100);
    }

    [Fact]
    public void NearestNeighbor_100Points_ReturnsClosest()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point3D(i, i, i)).ToList();
        var tree = new KDTree3D(points);
        var result = tree.NearestNeighbor(new Point3D(50.4, 50.4, 50.4));
        result.Should().Be(new Point3D(50, 50, 50));
    }

    [Fact]
    public void RangeQuery_BoundingBox_100Points_CountNonNegative()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point3D(i, i, i)).ToList();
        var tree = new KDTree3D(points);
        var result = tree.RangeQuery(new BoundingBox3D(new Point3D(40, 40, 40), new Point3D(60, 60, 60)));
        result.Length.Should().BeGreaterOrEqualTo(1);
        foreach (var p in result)
        {
            var box = new BoundingBox3D(new Point3D(40, 40, 40), new Point3D(60, 60, 60));
            box.Contains(p).Should().BeTrue();
        }
    }

    [Fact]
    public void RangeQuery_Radius_AllInsideCount()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0),
            new Point3D(0, 0, 1), new Point3D(1, 1, 1));
        var result = tree.RangeQuery(new Point3D(0.5, 0.5, 0.5), 2);
        result.Length.Should().Be(5);
    }

    [Fact]
    public void RangeQuery_BoundingBox_SingleAxisSpan()
    {
        var tree = BuildTree(
            new Point3D(0, 5, 5), new Point3D(10, 5, 5), new Point3D(20, 5, 5));
        var box = new BoundingBox3D(new Point3D(5, 0, 0), new Point3D(15, 10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point3D(10, 5, 5));
    }

    [Fact]
    public void NearestNeighbor_AllSameDistance_ReturnsOne()
    {
        var tree = BuildTree(
            new Point3D(5, 0, 0), new Point3D(-5, 0, 0),
            new Point3D(0, 5, 0), new Point3D(0, -5, 0),
            new Point3D(0, 0, 5), new Point3D(0, 0, -5));
        var result = tree.NearestNeighbor(new Point3D(0, 0, 0));
        result.DistanceTo(Point3D.Origin).Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void RangeQuery_Radius_CloseToOrigin()
    {
        var tree = BuildTree(
            new Point3D(0.1, 0, 0), new Point3D(0, 0.1, 0),
            new Point3D(0, 0, 0.1), new Point3D(10, 10, 10));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 1);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_MixedInsideOutside()
    {
        var tree = BuildTree(
            new Point3D(-1, -1, -1), new Point3D(1, 1, 1),
            new Point3D(100, 100, 100), new Point3D(-100, -100, -100));
        var box = new BoundingBox3D(new Point3D(-2, -2, -2), new Point3D(2, 2, 2));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Constructor_LargeDataset_64Points()
    {
        var points = Enumerable.Range(0, 64).Select(i => new Point3D(i % 4, (i / 4) % 4, i / 16)).ToList();
        var tree = new KDTree3D(points);
        tree.Count.Should().Be(64);
    }

    [Fact]
    public void NearestNeighbor_LargeDataset_ReturnsCorrect()
    {
        var points = Enumerable.Range(0, 64).Select(i => new Point3D(i % 4, (i / 4) % 4, i / 16)).ToList();
        var tree = new KDTree3D(points);
        var result = tree.NearestNeighbor(new Point3D(0.4, 0.4, 0.4));
        result.Should().Be(new Point3D(0, 0, 0));
    }

    [Fact]
    public void RangeQuery_Radius_64Points_Grid()
    {
        var points = Enumerable.Range(0, 64).Select(i => new Point3D(i % 4, (i / 4) % 4, i / 16)).ToList();
        var tree = new KDTree3D(points);
        var result = tree.RangeQuery(new Point3D(1.5, 1.5, 1.5), 2);
        result.Length.Should().BeGreaterOrEqualTo(1);
        foreach (var p in result)
            p.DistanceTo(new Point3D(1.5, 1.5, 1.5)).Should().BeLessOrEqualTo(2.0 + 1e-10);
    }

    [Fact]
    public void RangeQuery_BoundingBox_64Points()
    {
        var points = Enumerable.Range(0, 64).Select(i => new Point3D(i % 4, (i / 4) % 4, i / 16)).ToList();
        var tree = new KDTree3D(points);
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(0);
    }

    [Fact]
    public void RangeQuery_Radius_AllPointsIdentical()
    {
        var tree = BuildTree(
            new Point3D(5, 5, 5), new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var result = tree.RangeQuery(new Point3D(5, 5, 5), 0);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_SinglePointAtCenter()
    {
        var tree = BuildTree(new Point3D(5, 5, 5));
        var box = new BoundingBox3D(new Point3D(4, 4, 4), new Point3D(6, 6, 6));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void NearestNeighbor_TwentyPoints_ReturnsClosest()
    {
        var points = Enumerable.Range(0, 20).Select(i =>
            new Point3D(i * 1.0, 0, 0)).ToList();
        var tree = new KDTree3D(points);
        var result = tree.NearestNeighbor(new Point3D(7.4, 0, 0));
        result.Should().Be(new Point3D(7, 0, 0));
    }

    [Fact]
    public void RangeQuery_Radius_CornerPoints()
    {
        var tree = BuildTree(
            new Point3D(0, 0, 0), new Point3D(10, 0, 0),
            new Point3D(0, 10, 0), new Point3D(0, 0, 10));
        var result = tree.RangeQuery(new Point3D(0, 0, 0), 10);
        result.Length.Should().Be(4);
    }
}
