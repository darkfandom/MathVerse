namespace MathVerse.Geometry.Advanced.Tests.Spatial;

public class KDTree2DTests
{
    private static readonly Point2D Origin = new(0, 0);

    private static KDTree2D BuildTree(params Point2D[] points) => new KDTree2D(points);

    private static KDTree2D BuildEmptyTree() => new KDTree2D(Array.Empty<Point2D>());

    [Fact]
    public void Constructor_WithPoints_SetsCount()
    {
        var tree = BuildTree(new Point2D(1, 2), new Point2D(3, 4), new Point2D(5, 6));
        tree.Count.Should().Be(3);
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
        var tree = BuildTree(new Point2D(5, 5));
        tree.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_DuplicatePoints_AllAreStored()
    {
        var tree = BuildTree(new Point2D(1, 1), new Point2D(1, 1), new Point2D(1, 1));
        tree.Count.Should().Be(3);
    }

    [Fact]
    public void NearestNeighbor_SinglePoint_ReturnsThatPoint()
    {
        var tree = BuildTree(new Point2D(3, 7));
        var result = tree.NearestNeighbor(new Point2D(10, 10));
        result.Should().Be(new Point2D(3, 7));
    }

    [Fact]
    public void NearestNeighbor_TwoPoints_ReturnsCloser()
    {
        var tree = BuildTree(new Point2D(0, 0), new Point2D(10, 10));
        var result = tree.NearestNeighbor(new Point2D(1, 1));
        result.Should().Be(new Point2D(0, 0));
    }

    [Fact]
    public void NearestNeighbor_ExactMatch_ReturnsExactPoint()
    {
        var tree = BuildTree(new Point2D(5, 5), new Point2D(10, 10));
        var result = tree.NearestNeighbor(new Point2D(5, 5));
        result.Should().Be(new Point2D(5, 5));
    }

    [Fact]
    public void NearestNeighbor_MultiplePoints_ReturnsClosest()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 10),
            new Point2D(10, 10), new Point2D(5, 5));
        var result = tree.NearestNeighbor(new Point2D(4.5, 4.5));
        result.Should().Be(new Point2D(5, 5));
    }

    [Fact]
    public void NearestNeighbor_EmptyTree_ReturnsOrigin()
    {
        var tree = BuildEmptyTree();
        var result = tree.NearestNeighbor(new Point2D(5, 5));
        result.Should().Be(Point2D.Origin);
    }

    [Fact]
    public void NearestNeighbor_NegativeCoordinates_ReturnsCorrect()
    {
        var tree = BuildTree(new Point2D(-10, -10), new Point2D(10, 10));
        var result = tree.NearestNeighbor(new Point2D(-9, -9));
        result.Should().Be(new Point2D(-10, -10));
    }

    [Fact]
    public void NearestNeighbor_LargeCoordinates_ReturnsCorrect()
    {
        var tree = BuildTree(new Point2D(100000, 100000), new Point2D(-100000, -100000));
        var result = tree.NearestNeighbor(new Point2D(99999, 99999));
        result.Should().Be(new Point2D(100000, 100000));
    }

    [Fact]
    public void KNearest_RequestMoreThanCount_ReturnsAllPoints()
    {
        var tree = BuildTree(new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2));
        var result = tree.KNearest(new Point2D(0, 0), 10);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void KNearest_RequestOne_ReturnsSingleClosest()
    {
        var tree = BuildTree(new Point2D(0, 0), new Point2D(10, 10), new Point2D(5, 5));
        var result = tree.KNearest(new Point2D(1, 1), 1);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point2D(0, 0));
    }

    [Fact]
    public void KNearest_RequestTwo_ReturnsTwoClosest()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 10),
            new Point2D(10, 10));
        var result = tree.KNearest(new Point2D(1, 1), 2);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void KNearest_RequestTwo_ReturnsClosestByDistance()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(3, 4), new Point2D(10, 10),
            new Point2D(6, 8));
        var result = tree.KNearest(new Point2D(0, 0), 2);
        result.Should().Contain(new Point2D(0, 0));
        result.Should().Contain(new Point2D(3, 4));
    }

    [Fact]
    public void KNearest_EmptyTree_ReturnsEmpty()
    {
        var tree = BuildEmptyTree();
        var result = tree.KNearest(new Point2D(0, 0), 5);
        result.Should().BeEmpty();
    }

    [Fact]
    public void KNearest_ZeroK_ReturnsEmpty()
    {
        var tree = BuildTree(new Point2D(1, 1), new Point2D(2, 2));
        var result = tree.KNearest(new Point2D(0, 0), 0);
        result.Should().BeEmpty();
    }

    [Fact]
    public void KNearest_NegativeK_ReturnsEmpty()
    {
        var tree = BuildTree(new Point2D(1, 1), new Point2D(2, 2));
        var result = tree.KNearest(new Point2D(0, 0), -1);
        result.Should().BeEmpty();
    }

    [Fact]
    public void KNearest_SinglePoint_ReturnsIt()
    {
        var tree = BuildTree(new Point2D(42, 42));
        var result = tree.KNearest(new Point2D(0, 0), 1);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point2D(42, 42));
    }

    [Fact]
    public void RangeQuery_Circle_ContainsPointsInsideRadius()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(5, 5),
            new Point2D(2, 0), new Point2D(10, 10));
        var result = tree.RangeQuery(new Point2D(0, 0), 3);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Circle_ExcludesPointsOutsideRadius()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(100, 100), new Point2D(200, 200));
        var result = tree.RangeQuery(new Point2D(0, 0), 5);
        result.Length.Should().Be(1);
        result.Should().Contain(new Point2D(0, 0));
    }

    [Fact]
    public void RangeQuery_Circle_ZeroRadius_ReturnsExactMatch()
    {
        var tree = BuildTree(new Point2D(5, 5), new Point2D(6, 6));
        var result = tree.RangeQuery(new Point2D(5, 5), 0);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point2D(5, 5));
    }

    [Fact]
    public void RangeQuery_Circle_LargeRadius_ContainsAll()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(10, 10), new Point2D(20, 20));
        var result = tree.RangeQuery(new Point2D(10, 10), 100);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Circle_EmptyTree_ReturnsEmpty()
    {
        var tree = BuildEmptyTree();
        var result = tree.RangeQuery(new Point2D(0, 0), 10);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Circle_NoPointsInRange_ReturnsEmpty()
    {
        var tree = BuildTree(new Point2D(100, 100), new Point2D(200, 200));
        var result = tree.RangeQuery(new Point2D(0, 0), 1);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Circle_ExactlyOnBoundary_IsIncluded()
    {
        var tree = BuildTree(new Point2D(3, 4));
        var result = tree.RangeQuery(new Point2D(0, 0), 5);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ContainsPointsInsideBox()
    {
        var tree = BuildTree(
            new Point2D(1, 1), new Point2D(5, 5), new Point2D(9, 9),
            new Point2D(2, 2), new Point2D(8, 8));
        var box = new BoundingBox2D(new Point2D(0, 0), new Point2D(6, 6));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_ExcludesPointsOutsideBox()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(50, 50), new Point2D(100, 100));
        var box = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(0);
    }

    [Fact]
    public void RangeQuery_BoundingBox_EmptyTree_ReturnsEmpty()
    {
        var tree = BuildEmptyTree();
        var box = new BoundingBox2D(new Point2D(-10, -10), new Point2D(10, 10));
        var result = tree.RangeQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_BoundingBox_OnBoundary_IsIncluded()
    {
        var tree = BuildTree(new Point2D(5, 5));
        var box = new BoundingBox2D(new Point2D(5, 5), new Point2D(10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_TinyBox_ContainsOnlyExactPoint()
    {
        var tree = BuildTree(new Point2D(1, 1), new Point2D(2, 2), new Point2D(3, 3));
        var box = new BoundingBox2D(new Point2D(2, 2), new Point2D(2, 2));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point2D(2, 2));
    }

    [Fact]
    public void RangeQuery_BoundingBox_LargeBox_ContainsAll()
    {
        var tree = BuildTree(
            new Point2D(-50, -50), new Point2D(50, 50), new Point2D(0, 0));
        var box = new BoundingBox2D(new Point2D(-100, -100), new Point2D(100, 100));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void NearestNeighbor_AllPointsSameDistance_ReturnsOne()
    {
        var tree = BuildTree(
            new Point2D(5, 0), new Point2D(-5, 0),
            new Point2D(0, 5), new Point2D(0, -5));
        var result = tree.NearestNeighbor(new Point2D(0, 0));
        result.DistanceTo(Origin).Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void KNearest_FivePoints_RequestThree_ReturnsThree()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1),
            new Point2D(10, 10), new Point2D(20, 20));
        var result = tree.KNearest(new Point2D(0, 0), 3);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_Circle_MultiplePointsAtSameDistance()
    {
        var tree = BuildTree(
            new Point2D(3, 0), new Point2D(-3, 0),
            new Point2D(0, 3), new Point2D(0, -3));
        var result = tree.RangeQuery(new Point2D(0, 0), 3);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void Constructor_TenPoints_CountIsCorrect()
    {
        var points = Enumerable.Range(0, 10).Select(i => new Point2D(i, i * 2)).ToList();
        var tree = new KDTree2D(points);
        tree.Count.Should().Be(10);
    }

    [Fact]
    public void NearestNeighbor_DiagonalPoints_ReturnsCorrect()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2),
            new Point2D(3, 3), new Point2D(4, 4));
        var result = tree.NearestNeighbor(new Point2D(3.9, 3.9));
        result.Should().Be(new Point2D(4, 4));
    }

    [Fact]
    public void KNearest_DuplicatePoints_ReturnsDuplicates()
    {
        var tree = BuildTree(
            new Point2D(5, 5), new Point2D(5, 5), new Point2D(5, 5));
        var result = tree.KNearest(new Point2D(5, 5), 2);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void RangeQuery_BoundingBox_NegativeCoordinates()
    {
        var tree = BuildTree(
            new Point2D(-10, -10), new Point2D(-5, -5), new Point2D(5, 5));
        var box = new BoundingBox2D(new Point2D(-8, -8), new Point2D(-2, -2));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
        result[0].Should().Be(new Point2D(-5, -5));
    }

    [Fact]
    public void RangeQuery_Circle_NegativeCoordinates()
    {
        var tree = BuildTree(
            new Point2D(-10, -10), new Point2D(-5, -5), new Point2D(0, 0));
        var result = tree.RangeQuery(new Point2D(-5, -5), 3);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void NearestNeighbor_FarFromAllPoints_ReturnsClosest()
    {
        var tree = BuildTree(new Point2D(1000, 1000), new Point2D(2000, 2000));
        var result = tree.NearestNeighbor(new Point2D(0, 0));
        result.Should().Be(new Point2D(1000, 1000));
    }

    [Fact]
    public void KNearest_RequestAllPoints_ReturnsAll()
    {
        var points = Enumerable.Range(0, 20).Select(i => new Point2D(i, i)).ToList();
        var tree = new KDTree2D(points);
        var result = tree.KNearest(new Point2D(0, 0), 20);
        result.Length.Should().Be(20);
    }

    [Fact]
    public void RangeQuery_Circle_MixedDistances()
    {
        var tree = BuildTree(
            new Point2D(1, 0), new Point2D(2, 0), new Point2D(3, 0),
            new Point2D(4, 0), new Point2D(5, 0), new Point2D(6, 0));
        var result = tree.RangeQuery(new Point2D(0, 0), 3.5);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void RangeQuery_BoundingBox_CornerPoints()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(10, 0),
            new Point2D(0, 10), new Point2D(10, 10));
        var box = new BoundingBox2D(new Point2D(0, 0), new Point2D(10, 10));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void NearestNeighbor_CoinscidentPoint_ReturnsIt()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0));
        var result = tree.NearestNeighbor(new Point2D(1, 0));
        result.Should().Be(new Point2D(1, 0));
    }

    [Fact]
    public void KNearest_SixPoints_RequestFour_ClosestFour()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0),
            new Point2D(10, 0), new Point2D(20, 0), new Point2D(30, 0));
        var result = tree.KNearest(new Point2D(0, 0), 4);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void RangeQuery_BoundingBox_WidthZero_ContainsAlignedPoints()
    {
        var tree = BuildTree(
            new Point2D(5, 0), new Point2D(5, 5), new Point2D(5, 10));
        var box = new BoundingBox2D(new Point2D(5, 2), new Point2D(5, 8));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void NearestNeighbor_NonAxisAlignedClosest()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 10));
        var result = tree.NearestNeighbor(new Point2D(9, 1));
        result.Should().Be(new Point2D(10, 0));
    }

    [Fact]
    public void RangeQuery_Circle_ExactOnMultipleBoundaries()
    {
        var tree = BuildTree(
            new Point2D(0, 3), new Point2D(0, -3),
            new Point2D(3, 0), new Point2D(-3, 0));
        var result = tree.RangeQuery(new Point2D(0, 0), 3);
        result.Length.Should().Be(4);
    }

    [Fact]
    public void KNearest_AllDistantPoints_ReturnsKClosest()
    {
        var tree = BuildTree(
            new Point2D(100, 0), new Point2D(200, 0),
            new Point2D(300, 0), new Point2D(400, 0));
        var result = tree.KNearest(new Point2D(0, 0), 2);
        result.Length.Should().Be(2);
        result.Should().Contain(new Point2D(100, 0));
        result.Should().Contain(new Point2D(200, 0));
    }

    [Fact]
    public void RangeQuery_BoundingBox_IntersectsOnlyCorners()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(100, 100));
        var box = new BoundingBox2D(new Point2D(0, 0), new Point2D(1, 1));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(0);
    }

    [Fact]
    public void Constructor_Grid25Points_CountIs25()
    {
        var points = new List<Point2D>();
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                points.Add(new Point2D(x, y));
        var tree = new KDTree2D(points);
        tree.Count.Should().Be(25);
    }

    [Fact]
    public void NearestNeighbor_Grid_ReturnsCorrect()
    {
        var points = new List<Point2D>();
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                points.Add(new Point2D(x, y));
        var tree = new KDTree2D(points);
        var result = tree.NearestNeighbor(new Point2D(2.4, 2.4));
        result.Should().Be(new Point2D(2, 2));
    }

    [Fact]
    public void RangeQuery_Circle_Grid_ContainsCorrectCount()
    {
        var points = new List<Point2D>();
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                points.Add(new Point2D(x, y));
        var tree = new KDTree2D(points);
        var result = tree.RangeQuery(new Point2D(4.5, 4.5), 2.0);
        result.Length.Should().Be(12);
    }

    [Fact]
    public void NearestNeighbor_CloseToEdge_ReturnsCorrect()
    {
        var tree = BuildTree(new Point2D(0, 0), new Point2D(10, 0));
        var result = tree.NearestNeighbor(new Point2D(-0.1, 0));
        result.Should().Be(new Point2D(0, 0));
    }

    [Fact]
    public void KNearest_RequestAllSorted()
    {
        var tree = BuildTree(
            new Point2D(0, 0), new Point2D(3, 4), new Point2D(6, 8),
            new Point2D(1, 0), new Point2D(5, 0));
        var result = tree.KNearest(new Point2D(0, 0), 5);
        result.Length.Should().Be(5);
        for (int i = 0; i < result.Length - 1; i++)
        {
            double d1 = result[i].DistanceSquaredTo(new Point2D(0, 0));
            double d2 = result[i + 1].DistanceSquaredTo(new Point2D(0, 0));
            d1.Should().BeLessOrEqualTo(d2);
        }
    }

    [Fact]
    public void RangeQuery_BoundingBox_ExactMinMax()
    {
        var tree = BuildTree(new Point2D(5, 5));
        var box = new BoundingBox2D(new Point2D(5, 5), new Point2D(5, 5));
        var result = tree.RangeQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void NearestNeighbor_SymmetricPoints_ReturnsOneOfThem()
    {
        var tree = BuildTree(new Point2D(-1, 0), new Point2D(1, 0));
        var result = tree.NearestNeighbor(new Point2D(0, 0));
        result.DistanceTo(new Point2D(0, 0)).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void KNearest_RequestNegative_ReturnsEmpty()
    {
        var tree = BuildTree(new Point2D(1, 1));
        var result = tree.KNearest(new Point2D(0, 0), -5);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RangeQuery_Circle_VerySmallRadius()
    {
        var tree = BuildTree(new Point2D(5, 5), new Point2D(5.001, 5));
        var result = tree.RangeQuery(new Point2D(5, 5), 0.0005);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void RangeQuery_BoundingBox_EncompassesNothing()
    {
        var tree = BuildTree(new Point2D(0, 0), new Point2D(10, 10));
        var box = new BoundingBox2D(new Point2D(50, 50), new Point2D(60, 60));
        var result = tree.RangeQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_100Points_CountIsCorrect()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point2D(i * 0.7, i * 1.3)).ToList();
        var tree = new KDTree2D(points);
        tree.Count.Should().Be(100);
    }

    [Fact]
    public void NearestNeighbor_100Points_ReturnsClosest()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point2D(i, i)).ToList();
        var tree = new KDTree2D(points);
        var result = tree.NearestNeighbor(new Point2D(50.4, 50.4));
        result.Should().Be(new Point2D(50, 50));
    }

    [Fact]
    public void RangeQuery_Circle_100Points_WithinRadius()
    {
        var points = Enumerable.Range(0, 100).Select(i => new Point2D(i, i)).ToList();
        var tree = new KDTree2D(points);
        var result = tree.RangeQuery(new Point2D(50, 50), 5);
        result.Length.Should().BeGreaterOrEqualTo(1);
        foreach (var p in result)
            p.DistanceTo(new Point2D(50, 50)).Should().BeLessOrEqualTo(5.0 + 1e-10);
    }
}
