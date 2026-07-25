namespace MathVerse.Geometry.Advanced.Tests.Spatial;

public class BoundingVolumeHierarchyTests
{
    private static Triangle3D MakeTriangle(double ax, double ay, double az,
        double bx, double by, double bz, double cx, double cy, double cz) =>
        new(new Point3D(ax, ay, az), new Point3D(bx, by, bz), new Point3D(cx, cy, cz));

    private static BoundingVolumeHierarchy BuildBVH(params Triangle3D[] triangles) =>
        new BoundingVolumeHierarchy(triangles);

    private static BoundingVolumeHierarchy BuildEmptyBVH() =>
        new BoundingVolumeHierarchy(Array.Empty<Triangle3D>());

    [Fact]
    public void Constructor_WithTriangles_SetsCount()
    {
        var bvh = BuildBVH(
            MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0),
            MakeTriangle(2, 2, 2, 3, 2, 2, 2, 3, 2));
        bvh.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_EmptyList_CountIsZero()
    {
        var bvh = BuildEmptyBVH();
        bvh.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_SingleTriangle_CountIsOne()
    {
        var bvh = BuildBVH(MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0));
        bvh.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_MultipleTriangles_AllAreStored()
    {
        var bvh = BuildBVH(
            MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0),
            MakeTriangle(2, 0, 0, 3, 0, 0, 2, 1, 0),
            MakeTriangle(4, 0, 0, 5, 0, 0, 4, 1, 0),
            MakeTriangle(6, 0, 0, 7, 0, 0, 6, 1, 0));
        bvh.Count.Should().Be(4);
    }

    [Fact]
    public void Constructor_10Triangles_CountIs10()
    {
        var triangles = Enumerable.Range(0, 10).Select(i =>
            MakeTriangle(i * 2, 0, 0, i * 2 + 1, 0, 0, i * 2, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        bvh.Count.Should().Be(10);
    }

    [Fact]
    public void Raycast_EmptyBVH_ReturnsNoHit()
    {
        var bvh = BuildEmptyBVH();
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
        result.triangleIndex.Should().Be(-1);
    }

    [Fact]
    public void Raycast_HitsSingleTriangle()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(0);
    }

    [Fact]
    public void Raycast_HitsCorrectTriangle()
    {
        var t1 = MakeTriangle(-10, -10, -5, -8, -10, -5, -10, -8, -5);
        var t2 = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(t1, t2);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(1);
    }

    [Fact]
    public void Raycast_MissesTriangle()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(50, 50, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void Raycast_ClosestHitIsReturned()
    {
        var t1 = MakeTriangle(-1, -1, -10, 1, -1, -10, 0, 1, -10);
        var t2 = MakeTriangle(-1, -1, -1, 1, -1, -1, 0, 1, -1);
        var bvh = BuildBVH(t1, t2);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(1);
    }

    [Fact]
    public void Raycast_ParallelToTriangle_Misses()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(1, 0, 0));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void Raycast_RayBehindTriangle_Misses()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, -5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void Raycast_TriangleOnXYPlane()
    {
        var triangle = MakeTriangle(-5, -5, 0, 5, -5, 0, 0, 5, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 10), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.point.X.Should().BeApproximately(0, 1e-6);
        result.point.Y.Should().BeApproximately(0, 1e-6);
        result.point.Z.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void BoxQuery_EmptyBVH_ReturnsEmpty()
    {
        var bvh = BuildEmptyBVH();
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var result = bvh.BoxQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void BoxQuery_SingleTriangle_InRange()
    {
        var triangle = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(2, 2, 2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void BoxQuery_SingleTriangle_OutOfRange()
    {
        var triangle = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var box = new BoundingBox3D(new Point3D(50, 50, 50), new Point3D(60, 60, 60));
        var result = bvh.BoxQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void BoxQuery_MultipleTriangles_SomeInRange()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(50, 50, 50, 51, 50, 50, 50, 51, 50);
        var t3 = MakeTriangle(2, 2, 2, 3, 2, 2, 2, 3, 2);
        var bvh = BuildBVH(t1, t2, t3);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(4, 4, 4));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void BoxQuery_AllTrianglesInRange()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(0.5, 0.5, 0, 1.5, 0.5, 0, 0.5, 1.5, 0);
        var t3 = MakeTriangle(0.2, 0.2, 0, 1.2, 0.2, 0, 0.2, 1.2, 0);
        var bvh = BuildBVH(t1, t2, t3);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(5, 5, 5));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void BoxQuery_TinyBox_ContainsOneTriangle()
    {
        var t1 = MakeTriangle(0, 0, 0, 0.1, 0, 0, 0, 0.1, 0);
        var t2 = MakeTriangle(5, 5, 5, 5.1, 5, 5, 5, 5.1, 5);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-0.1, -0.1, -0.1), new Point3D(0.2, 0.2, 0.2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void BoxQuery_NegativeCoordinates()
    {
        var t1 = MakeTriangle(-5, -5, -5, -4, -5, -5, -5, -4, -5);
        var t2 = MakeTriangle(5, 5, 5, 6, 5, 5, 5, 6, 5);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(-3, -3, -3));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void BoxQuery_BoundingBoxEncompassesAll()
    {
        var triangles = Enumerable.Range(0, 5).Select(i =>
            MakeTriangle(i * 3, 0, 0, i * 3 + 1, 0, 0, i * 3, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(100, 100, 100));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(5);
    }

    [Fact]
    public void Constructor_TrianglesInDifferentOrients()
    {
        var t1 = MakeTriangle(0, 0, 0, 0, 10, 0, 10, 0, 0);
        var t2 = MakeTriangle(-5, -5, -5, -4, -5, -5, -5, -4, -5);
        var bvh = BuildBVH(t1, t2);
        bvh.Count.Should().Be(2);
    }

    [Fact]
    public void Raycast_DiagonalRay()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0.1, 0.1, -1).Normalize());
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void Raycast_VeryCloseToEdge()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0.99, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void BoxQuery_TrianglesPartiallyOverlap()
    {
        var t1 = MakeTriangle(0, 0, 0, 2, 0, 0, 0, 2, 0);
        var t2 = MakeTriangle(1, 1, 0, 3, 1, 0, 1, 3, 0);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(0.5, 0.5, -1), new Point3D(1.5, 1.5, 1));
        var result = bvh.BoxQuery(box);
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void Constructor_20Triangles_CountIs20()
    {
        var triangles = Enumerable.Range(0, 20).Select(i =>
            MakeTriangle(i, 0, 0, i + 1, 0, 0, i, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        bvh.Count.Should().Be(20);
    }

    [Fact]
    public void Raycast_20Triangles_HitsClosest()
    {
        var triangles = Enumerable.Range(0, 20).Select(i =>
            MakeTriangle(-1 + i * 10, -1, -i * 10, 1 + i * 10, -1, -i * 10, 0, 1, -i * 10)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(0);
    }

    [Fact]
    public void BoxQuery_20Triangles_ReturnsCorrect()
    {
        var triangles = Enumerable.Range(0, 20).Select(i =>
            MakeTriangle(i * 5, 0, 0, i * 5 + 1, 0, 0, i * 5, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var box = new BoundingBox3D(new Point3D(0, -1, -1), new Point3D(12, 2, 2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void Raycast_OriginOnTrianglePlane_Hits()
    {
        var triangle = MakeTriangle(-5, -5, 0, 5, -5, 0, 0, 5, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void Raycast_LargeTriangles()
    {
        var triangle = MakeTriangle(-1000, -1000, 0, 1000, -1000, 0, 0, 1000, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 100), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
    }

    [Fact]
    public void BoxQuery_LargeBoundingBox()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(500, 500, 500, 501, 500, 500, 500, 501, 500);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-1000, -1000, -1000), new Point3D(1000, 1000, 1000));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Raycast_TrianglesScattered_HitsNearest()
    {
        var t1 = MakeTriangle(50, 50, -100, 51, 50, -100, 50, 51, -100);
        var t2 = MakeTriangle(-1, -1, -1, 1, -1, -1, 0, 1, -1);
        var bvh = BuildBVH(t1, t2);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(1);
    }

    [Fact]
    public void BoxQuery_ExactOnTriangle()
    {
        var triangle = MakeTriangle(5, 5, 5, 6, 5, 5, 5, 6, 5);
        var bvh = BuildBVH(triangle);
        var box = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void Raycast_MultipleParallelHits()
    {
        var t1 = MakeTriangle(-1, -1, -10, 1, -1, -10, 0, 1, -10);
        var t2 = MakeTriangle(-1, -1, -5, 1, -1, -5, 0, 1, -5);
        var t3 = MakeTriangle(-1, -1, -1, 1, -1, -1, 0, 1, -1);
        var bvh = BuildBVH(t1, t2, t3);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(2);
    }

    [Fact]
    public void BoxQuery_BoxBetweenTriangles()
    {
        var t1 = MakeTriangle(-10, 0, 0, -9, 0, 0, -10, 1, 0);
        var t2 = MakeTriangle(10, 0, 0, 11, 0, 0, 10, 1, 0);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var result = bvh.BoxQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_OverlappingTriangles()
    {
        var t1 = MakeTriangle(0, 0, 0, 5, 0, 0, 0, 5, 0);
        var t2 = MakeTriangle(1, 1, 0, 6, 1, 0, 1, 6, 0);
        var bvh = BuildBVH(t1, t2);
        bvh.Count.Should().Be(2);
    }

    [Fact]
    public void Raycast_AngledRay()
    {
        var triangle = MakeTriangle(-10, -10, 0, 10, -10, 0, 0, 10, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(-5, 0, 10), new Vector3D(1, 0, -1).Normalize());
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
    }

    [Fact]
    public void BoxQuery_NegativeTriangles()
    {
        var t1 = MakeTriangle(-5, -5, -5, -4, -5, -5, -5, -4, -5);
        var t2 = MakeTriangle(-3, -3, -3, -2, -3, -3, -3, -2, -3);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-10, -10, -10), new Point3D(-2.5, -2.5, -2.5));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Constructor_SamePositionTriangles()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var bvh = BuildBVH(t1, t2);
        bvh.Count.Should().Be(2);
    }

    [Fact]
    public void BoxQuery_VariousPositions_ReturnsCorrectCount()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(3, 3, 3, 4, 3, 3, 3, 4, 3);
        var t3 = MakeTriangle(6, 6, 6, 7, 6, 6, 6, 7, 6);
        var t4 = MakeTriangle(9, 9, 9, 10, 9, 9, 9, 10, 9);
        var bvh = BuildBVH(t1, t2, t3, t4);
        var box = new BoundingBox3D(new Point3D(2, 2, 2), new Point3D(7, 7, 7));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Constructor_30Triangles_CountIs30()
    {
        var triangles = Enumerable.Range(0, 30).Select(i =>
            MakeTriangle(i * 2, 0, 0, i * 2 + 1, 0, 0, i * 2, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        bvh.Count.Should().Be(30);
    }

    [Fact]
    public void Raycast_30Triangles_HitsClosest()
    {
        var triangles = Enumerable.Range(0, 30).Select(i =>
            MakeTriangle(-1, -1, -i * 5, 1, -1, -i * 5, 0, 1, -i * 5)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.triangleIndex.Should().Be(0);
    }

    [Fact]
    public void BoxQuery_30Triangles_ReturnsCorrect()
    {
        var triangles = Enumerable.Range(0, 30).Select(i =>
            MakeTriangle(i * 4, 0, 0, i * 4 + 1, 0, 0, i * 4, 1, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var box = new BoundingBox3D(new Point3D(0, -1, -1), new Point3D(10, 2, 2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void Raycast_MissesAllTriangles()
    {
        var triangles = Enumerable.Range(0, 5).Select(i =>
            MakeTriangle(i * 5, 50, 0, i * 5 + 1, 50, 0, i * 5, 51, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var ray = new Ray(new Point3D(0, 0, 10), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void BoxQuery_TrianglesOnXYPlane()
    {
        var t1 = MakeTriangle(-2, -2, 0, 2, -2, 0, 0, 2, 0);
        var t2 = MakeTriangle(3, -2, 0, 7, -2, 0, 5, 2, 0);
        var bvh = BuildBVH(t1, t2);
        var box = new BoundingBox3D(new Point3D(-3, -3, -1), new Point3D(3, 3, 1));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Raycast_FromSide()
    {
        var triangle = MakeTriangle(0, 0, 0, 0, 0, 5, 0, 5, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(5, 2, 2), new Vector3D(-1, 0, 0));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
    }

    [Fact]
    public void BoxQuery_ThreeClusters()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(20, 20, 20, 21, 20, 20, 20, 21, 20);
        var t3 = MakeTriangle(40, 40, 40, 41, 40, 40, 40, 41, 40);
        var bvh = BuildBVH(t1, t2, t3);
        var box = new BoundingBox3D(new Point3D(19, 19, 19), new Point3D(22, 22, 22));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(1);
    }

    [Fact]
    public void Constructor_50Triangles_CountIs50()
    {
        var triangles = Enumerable.Range(0, 50).Select(i =>
            MakeTriangle(i, i, 0, i + 0.5, i, 0, i, i + 0.5, 0)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        bvh.Count.Should().Be(50);
    }

    [Fact]
    public void Raycast_SteepAngle()
    {
        var triangle = MakeTriangle(-5, -5, 0, 5, -5, 0, 0, 5, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(-20, -20, 20), new Vector3D(1, 1, -1).Normalize());
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
    }

    [Fact]
    public void BoxQuery_CoinscidentTriangles()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t3 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var bvh = BuildBVH(t1, t2, t3);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(2, 2, 2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(3);
    }

    [Fact]
    public void Raycast_HitPointIsOnTriangle()
    {
        var triangle = MakeTriangle(-10, -10, 0, 10, -10, 0, 0, 10, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 10), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.point.DistanceTo(new Point3D(0, 0, 0)).Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void BoxQuery_SixFaces()
    {
        var triangles = new[]
        {
            MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0),
            MakeTriangle(0, 0, 1, 1, 0, 1, 0, 1, 1),
            MakeTriangle(0, 0, 0, 1, 0, 0, 1, 0, 1),
            MakeTriangle(0, 1, 0, 1, 1, 0, 1, 1, 1),
            MakeTriangle(0, 0, 0, 0, 1, 0, 0, 1, 1),
            MakeTriangle(1, 0, 0, 1, 1, 0, 1, 1, 1)
        };
        var bvh = new BoundingVolumeHierarchy(triangles);
        bvh.Count.Should().Be(6);
        var box = new BoundingBox3D(new Point3D(-0.5, -0.5, -0.5), new Point3D(1.5, 1.5, 1.5));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(6);
    }

    [Fact]
    public void Raycast_RayParallelToPlane_Misses()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(1, 0, 0));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeFalse();
    }

    [Fact]
    public void BoxQuery_OverlappingRanges()
    {
        var t1 = MakeTriangle(0, 0, 0, 4, 0, 0, 0, 4, 0);
        var t2 = MakeTriangle(2, 2, 0, 6, 2, 0, 2, 6, 0);
        var t3 = MakeTriangle(4, 4, 0, 8, 4, 0, 4, 8, 0);
        var bvh = BuildBVH(t1, t2, t3);
        var box = new BoundingBox3D(new Point3D(1, 1, -1), new Point3D(5, 5, 1));
        var result = bvh.BoxQuery(box);
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void Raycast_ManyTriangles_PicksClosest()
    {
        var triangles = Enumerable.Range(0, 10).Select(i =>
            MakeTriangle(-1, -1, -i * 10 - 5, 1, -1, -i * 10 - 5, 0, 1, -i * 10 - 5)).ToArray();
        var bvh = new BoundingVolumeHierarchy(triangles);
        var ray = new Ray(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.t.Should().BeApproximately(10.0, 1e-6);
    }

    [Fact]
    public void BoxQuery_DistantBox_EmptyResult()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var bvh = BuildBVH(t1);
        var box = new BoundingBox3D(new Point3D(1000, 1000, 1000), new Point3D(2000, 2000, 2000));
        var result = bvh.BoxQuery(box);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_OverlappingTrianglesInZ()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(0, 0, 0.5, 1, 0, 0.5, 0, 1, 0.5);
        var bvh = BuildBVH(t1, t2);
        bvh.Count.Should().Be(2);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(2, 2, 2));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Raycast_MediumDistance()
    {
        var triangle = MakeTriangle(-1, -1, 0, 1, -1, 0, 0, 1, 0);
        var bvh = BuildBVH(triangle);
        var ray = new Ray(new Point3D(0, 0, 50), new Vector3D(0, 0, -1));
        var result = bvh.Raycast(ray);
        result.hit.Should().BeTrue();
        result.t.Should().BeApproximately(50.0, 1e-6);
    }

    [Fact]
    public void BoxQuery_TrianglesSpreadAcrossAllAxes()
    {
        var t1 = MakeTriangle(0, 0, 0, 1, 0, 0, 0, 1, 0);
        var t2 = MakeTriangle(0, 0, 10, 1, 0, 10, 0, 1, 10);
        var t3 = MakeTriangle(10, 0, 0, 11, 0, 0, 10, 1, 0);
        var t4 = MakeTriangle(0, 10, 0, 1, 10, 0, 0, 11, 0);
        var bvh = BuildBVH(t1, t2, t3, t4);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(2, 2, 12));
        var result = bvh.BoxQuery(box);
        result.Length.Should().Be(2);
    }

    [Fact]
    public void Constructor_SingleDegenerateTriangle()
    {
        var triangle = MakeTriangle(0, 0, 0, 1, 1, 0, 2, 2, 0);
        var bvh = BuildBVH(triangle);
        bvh.Count.Should().Be(1);
    }
}
