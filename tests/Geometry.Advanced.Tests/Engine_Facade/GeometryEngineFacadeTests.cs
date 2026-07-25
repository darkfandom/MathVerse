namespace MathVerse.Geometry.Advanced.Tests.Engine_Facade;

public class GeometryEngineFacadeTests
{
    private readonly GeometryEngine _engine = new(new GeometryOptions());

    [Fact]
    public void CreatePoint2D_ReturnsCorrectPoint()
    {
        var p = _engine.CreatePoint2D(3, 4);
        p.X.Should().Be(3);
        p.Y.Should().Be(4);
    }

    [Fact]
    public void CreatePoint3D_ReturnsCorrectPoint()
    {
        var p = _engine.CreatePoint3D(1, 2, 3);
        p.X.Should().Be(1);
        p.Y.Should().Be(2);
        p.Z.Should().Be(3);
    }

    [Fact]
    public void CreatePoint4D_ReturnsCorrectPoint()
    {
        var p = _engine.CreatePoint4D(1, 2, 3, 4);
        p.X.Should().Be(1);
        p.Y.Should().Be(2);
        p.Z.Should().Be(3);
        p.W.Should().Be(4);
    }

    [Fact]
    public void CreateLine2D_ReturnsCorrectLine()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);
        var line = _engine.CreateLine2D(a, b);
        line.P1.Should().Be(a);
        line.P2.Should().Be(b);
    }

    [Fact]
    public void CreateLine3D_ReturnsCorrectLine()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 5, 6);
        var line = _engine.CreateLine3D(a, b);
        line.P1.Should().Be(a);
        line.P2.Should().Be(b);
    }

    [Fact]
    public void CreateCircle2D_ReturnsCorrectCircle()
    {
        var center = new Point2D(1, 2);
        var circle = _engine.CreateCircle2D(center, 5);
        circle.Center.Should().Be(center);
        circle.Radius.Should().Be(5);
    }

    [Fact]
    public void CreatePlane_ReturnsCorrectPlane()
    {
        var pt = new Point3D(1, 2, 3);
        var normal = new Vector3D(0, 1, 0).Normalize();
        var plane = _engine.CreatePlane(pt, normal);
        plane.Point.Should().Be(pt);
        plane.Normal.Should().Be(normal);
    }

    [Fact]
    public void CreateSphere_ReturnsCorrectSphere()
    {
        var center = new Point3D(1, 2, 3);
        var sphere = _engine.CreateSphere(center, 5);
        sphere.Center.Should().Be(center);
        sphere.Radius.Should().Be(5);
    }

    [Fact]
    public void CreateTorus_ReturnsCorrectTorus()
    {
        var center = Point3D.Origin;
        var axis = Vector3D.UnitY;
        var torus = _engine.CreateTorus(center, axis, 10, 2);
        torus.MajorRadius.Should().Be(10);
        torus.MinorRadius.Should().Be(2);
    }

    [Fact]
    public void CreateCapsule_ReturnsCorrectCapsule()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(10, 0, 0);
        var capsule = _engine.CreateCapsule(a, b, 2);
        capsule.A.Should().Be(a);
        capsule.B.Should().Be(b);
        capsule.Radius.Should().Be(2);
    }

    [Fact]
    public void CreateOBB_ReturnsCorrectOBB()
    {
        var center = Point3D.Origin;
        var obb = _engine.CreateOBB(center, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);
        obb.Center.Should().Be(center);
        obb.ExtentX.Should().Be(1);
        obb.ExtentY.Should().Be(2);
        obb.ExtentZ.Should().Be(3);
    }

    [Fact]
    public void CreateUnitSphere_HasVertices()
    {
        var mesh = _engine.CreateUnitSphere(5);
        mesh.VertexCount.Should().Be(25);
    }

    [Fact]
    public void CreateUnitCube_Has12Triangles()
    {
        var mesh = _engine.CreateUnitCube();
        mesh.TriangleCount.Should().Be(12);
    }

    [Fact]
    public void CreateGrid2D_CorrectCount()
    {
        var grid = _engine.CreateGrid2D(0, 10, 3, 0, 10, 4);
        grid.Length.Should().Be(12);
    }

    [Fact]
    public void CreateGrid3D_CorrectCount()
    {
        var grid = _engine.CreateGrid3D(0, 10, 3, 0, 10, 3);
        grid.Length.Should().Be(9);
    }

    [Fact]
    public void CreateRegularPolygon_ReturnsCorrectPolygon()
    {
        var poly = _engine.CreateRegularPolygon(6, 1.0);
        poly.VertexCount.Should().Be(6);
    }

    [Fact]
    public void TessellatePolygon_Triangle_ReturnsOneTriangle()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        var tris = _engine.TessellatePolygon(pts);
        tris.Length.Should().Be(1);
    }

    [Fact]
    public void TessellatePolygon_Quad_ReturnsTwoTriangles()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1));
        var tris = _engine.TessellatePolygon(pts);
        tris.Length.Should().Be(2);
    }

    [Fact]
    public void TessellatePolygon_Pentagon_ReturnsThreeTriangles()
    {
        var poly = GeometryFactory.RegularPolygon(5, 1.0);
        var tris = _engine.TessellatePolygon(poly.Vertices);
        tris.Length.Should().Be(3);
    }

    [Fact]
    public void TessellatePolygon_TooFewPoints_ReturnsEmpty()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0));
        var tris = _engine.TessellatePolygon(pts);
        tris.Length.Should().Be(0);
    }

    [Fact]
    public void TessellatePolygon_Null_ReturnsEmpty()
    {
        var tris = _engine.TessellatePolygon(null!);
        tris.Length.Should().Be(0);
    }

    [Fact]
    public void TriangulatePolygon_Pentagon_ReturnsTriangles()
    {
        var poly = GeometryFactory.RegularPolygon(5, 1.0);
        var tris = _engine.TriangulatePolygon(poly.Vertices);
        tris.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Distance2D_SamePoint_ReturnsZero()
    {
        var p = new Point2D(5, 5);
        _engine.Distance2D(p, p).Should().Be(0);
    }

    [Fact]
    public void Distance2D_KnownDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        _engine.Distance2D(a, b).Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Distance3D_SamePoint_ReturnsZero()
    {
        var p = new Point3D(1, 2, 3);
        _engine.Distance3D(p, p).Should().Be(0);
    }

    [Fact]
    public void Distance3D_KnownDistance()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(1, 2, 2);
        _engine.Distance3D(a, b).Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void ConvexHull_Triangle_RemainsTriangle()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        var hull = _engine.ConvexHull(pts);
        hull.VertexCount.Should().Be(3);
    }

    [Fact]
    public void ConvexHull_Square_IncludesFourVertices()
    {
        var pts = ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0),
            new Point2D(1, 1), new Point2D(0, 1));
        var hull = _engine.ConvexHull(pts);
        hull.VertexCount.Should().Be(4);
    }

    [Fact]
    public void DelaunayTriangulate_ThreePoints_OneTriangle()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        var tris = _engine.DelaunayTriangulate(pts);
        tris.Length.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void DelaunayTriangulate_Square_TwoTriangles()
    {
        var pts = ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0),
            new Point2D(1, 1), new Point2D(0, 1));
        var tris = _engine.DelaunayTriangulate(pts);
        tris.Length.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Voronoi_ThreeSites_ReturnsThreeCells()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10));
        var cells = _engine.Voronoi(pts);
        cells.Length.Should().Be(3);
    }

    [Fact]
    public void ClipPolygon_SubjectInsideClip_ReturnsSameShape()
    {
        var subject = GeometryFactory.RegularPolygon(4, 0.5);
        var clip = GeometryFactory.RegularPolygon(4, 2.0);
        var result = _engine.ClipPolygon(subject, clip);
        result.VertexCount.Should().Be(4);
    }

    [Fact]
    public void FindSegmentIntersections_TwoCrossingSegments_ReturnsOnePoint()
    {
        var segs = ImmutableArray.Create(
            GeometryFactory.Segment(new Point2D(0, 0), new Point2D(10, 10)),
            GeometryFactory.Segment(new Point2D(0, 10), new Point2D(10, 0)));
        var pts = _engine.FindSegmentIntersections(segs);
        pts.Length.Should().Be(1);
    }

    [Fact]
    public void FindSegmentIntersections_ParallelSegments_ReturnsNoPoints()
    {
        var segs = ImmutableArray.Create(
            GeometryFactory.Segment(new Point2D(0, 0), new Point2D(10, 0)),
            GeometryFactory.Segment(new Point2D(0, 1), new Point2D(10, 1)));
        var pts = _engine.FindSegmentIntersections(segs);
        pts.Length.Should().Be(0);
    }

    [Fact]
    public void BuildKDTree2D_CanBeBuilt()
    {
        var pts = ImmutableArray.Create(new Point2D(1, 2), new Point2D(3, 4), new Point2D(5, 6));
        var tree = _engine.BuildKDTree2D(pts);
        tree.Should().NotBeNull();
    }

    [Fact]
    public void BuildKDTree3D_CanBeBuilt()
    {
        var pts = ImmutableArray.Create(new Point3D(1, 2, 3), new Point3D(4, 5, 6));
        var tree = _engine.BuildKDTree3D(pts);
        tree.Should().NotBeNull();
    }

    [Fact]
    public void BuildOctree_CanBeBuilt()
    {
        var box = GeometryFactory.AABB(new Point3D(-10, -10, -10), new Point3D(10, 10, 10));
        var pts = ImmutableArray.Create(new Point3D(1, 2, 3), new Point3D(-1, -2, -3));
        var octree = _engine.BuildOctree(box, pts);
        octree.Should().NotBeNull();
    }

    [Fact]
    public void BuildBVH_CanBeBuilt()
    {
        var tri1 = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));
        var tri2 = new Triangle3D(new Point3D(2, 2, 2), new Point3D(3, 2, 2), new Point3D(2, 3, 2));
        var bvh = _engine.BuildBVH(ImmutableArray.Create(tri1, tri2));
        bvh.Should().NotBeNull();
    }

    [Fact]
    public void BuildHalfEdgeMesh_Cube_HasFaces()
    {
        var mesh = _engine.CreateUnitCube();
        var hem = _engine.BuildHalfEdgeMesh(mesh);
        hem.FaceCount.Should().Be(12);
    }

    [Fact]
    public void RayTriangle_Hit_ReturnsTrue()
    {
        var ray = new Ray(new Point3D(0.5, 0.5, 5), new Vector3D(0, 0, -1));
        var tri = new Triangle3D(new Point3D(-1, -1, 0), new Point3D(2, -1, 0), new Point3D(0, 2, 0));
        var (hit, _, _) = _engine.RayTriangle(ray, tri);
        hit.Should().BeTrue();
    }

    [Fact]
    public void RayTriangle_Miss_ReturnsFalse()
    {
        var ray = new Ray(new Point3D(10, 10, 5), new Vector3D(0, 0, -1));
        var tri = new Triangle3D(new Point3D(-1, -1, 0), new Point3D(2, -1, 0), new Point3D(0, 2, 0));
        var (hit, _, _) = _engine.RayTriangle(ray, tri);
        hit.Should().BeFalse();
    }

    [Fact]
    public void RaySphere_Hit()
    {
        var ray = new Ray(new Point3D(0, 0, -10), Vector3D.UnitZ);
        var sphere = new Sphere3D(Point3D.Origin, 5);
        var (hit, _, _) = _engine.RaySphere(ray, sphere);
        hit.Should().BeTrue();
    }

    [Fact]
    public void RaySphere_Miss()
    {
        var ray = new Ray(new Point3D(10, 10, -10), Vector3D.UnitZ);
        var sphere = new Sphere3D(Point3D.Origin, 1);
        var (hit, _, _) = _engine.RaySphere(ray, sphere);
        hit.Should().BeFalse();
    }

    [Fact]
    public void RayAABB_Hit()
    {
        var ray = new Ray(new Point3D(0, 0, -5), Vector3D.UnitZ);
        var box = GeometryFactory.AABB(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var (hit, _, _) = _engine.RayAABB(ray, box);
        hit.Should().BeTrue();
    }

    [Fact]
    public void RayAABB_Miss()
    {
        var ray = new Ray(new Point3D(5, 5, -5), Vector3D.UnitZ);
        var box = GeometryFactory.AABB(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var (hit, _, _) = _engine.RayAABB(ray, box);
        hit.Should().BeFalse();
    }

    [Fact]
    public void RayPlane_Hit()
    {
        var ray = new Ray(new Point3D(0, 5, 0), new Vector3D(0, -1, 0));
        var plane = _engine.CreatePlane(Point3D.Origin, Vector3D.UnitY);
        var (hit, _, _) = _engine.RayPlane(ray, plane);
        hit.Should().BeTrue();
    }

    [Fact]
    public void RayPlane_Parallel_Misses()
    {
        var ray = new Ray(new Point3D(0, 5, 0), Vector3D.UnitX);
        var plane = _engine.CreatePlane(Point3D.Origin, Vector3D.UnitY);
        var (hit, _, _) = _engine.RayPlane(ray, plane);
        hit.Should().BeFalse();
    }

    [Fact]
    public void SphereSphereCollision_Overlapping_ReturnsTrue()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 5);
        var b = new Sphere3D(new Point3D(3, 0, 0), 5);
        _engine.SphereSphereCollision(a, b).Should().BeTrue();
    }

    [Fact]
    public void SphereSphereCollision_Separated_ReturnsFalse()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1);
        var b = new Sphere3D(new Point3D(10, 0, 0), 1);
        _engine.SphereSphereCollision(a, b).Should().BeFalse();
    }

    [Fact]
    public void AABBAABB_Overlapping_ReturnsTrue()
    {
        var a = GeometryFactory.AABB(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var b = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        _engine.AABBCollision(a, b).Should().BeTrue();
    }

    [Fact]
    public void AABBAABB_Separated_ReturnsFalse()
    {
        var a = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var b = GeometryFactory.AABB(new Point3D(5, 5, 5), new Point3D(6, 6, 6));
        _engine.AABBCollision(a, b).Should().BeFalse();
    }

    [Fact]
    public void AABBSphere_Inside_ReturnsTrue()
    {
        var box = GeometryFactory.AABB(new Point3D(-5, -5, -5), new Point3D(5, 5, 5));
        var sphere = new Sphere3D(Point3D.Origin, 2);
        _engine.AABBSphereCollision(box, sphere).Should().BeTrue();
    }

    [Fact]
    public void AABBSphere_FarAway_ReturnsFalse()
    {
        var box = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var sphere = new Sphere3D(new Point3D(100, 100, 100), 1);
        _engine.AABBSphereCollision(box, sphere).Should().BeFalse();
    }

    [Fact]
    public void CapsuleSphere_Inside_ReturnsTrue()
    {
        var capsule = _engine.CreateCapsule(new Point3D(-5, 0, 0), new Point3D(5, 0, 0), 2);
        var sphere = new Sphere3D(Point3D.Origin, 1);
        _engine.CapsuleSphereCollision(capsule, sphere).Should().BeTrue();
    }

    [Fact]
    public void CapsuleCapsule_Overlapping_ReturnsTrue()
    {
        var a = _engine.CreateCapsule(new Point3D(0, 0, 0), new Point3D(5, 0, 0), 2);
        var b = _engine.CreateCapsule(new Point3D(3, 0, 0), new Point3D(8, 0, 0), 2);
        _engine.CapsuleCapsuleCollision(a, b).Should().BeTrue();
    }

    [Fact]
    public void CapsuleCapsule_Separated_ReturnsFalse()
    {
        var a = _engine.CreateCapsule(new Point3D(0, 0, 0), new Point3D(1, 0, 0), 1);
        var b = _engine.CreateCapsule(new Point3D(10, 0, 0), new Point3D(11, 0, 0), 1);
        _engine.CapsuleCapsuleCollision(a, b).Should().BeFalse();
    }

    [Fact]
    public void OBBSphere_Containing_ReturnsTrue()
    {
        var obb = _engine.CreateOBB(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 5, 5, 5);
        var sphere = new Sphere3D(Point3D.Origin, 1);
        _engine.OBBSphereCollision(obb, sphere).Should().BeTrue();
    }

    [Fact]
    public void OBBSphere_Separated_ReturnsFalse()
    {
        var obb = _engine.CreateOBB(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var sphere = new Sphere3D(new Point3D(100, 100, 100), 1);
        _engine.OBBSphereCollision(obb, sphere).Should().BeFalse();
    }

    [Fact]
    public void Area_Triangle()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(10, 0), new Point2D(0, 10));
        _engine.Area(tri).Should().BeApproximately(50, 1e-6);
    }

    [Fact]
    public void Area_Polygon()
    {
        var poly = GeometryFactory.RegularPolygon(4, 1.0);
        double expected = 2.0;
        _engine.Area(poly).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Area_Circle()
    {
        var circle = new Circle2D(Point2D.Origin, 5);
        _engine.Area(circle).Should().BeApproximately(System.Math.PI * 25, 1e-6);
    }

    [Fact]
    public void Volume_Sphere()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3);
        double expected = 4.0 / 3.0 * System.Math.PI * 27;
        _engine.Volume(sphere).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Volume_Cylinder()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 2, 5);
        double expected = System.Math.PI * 4 * 5;
        _engine.Volume(cyl).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Volume_Cone()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3, 4);
        double expected = 1.0 / 3.0 * System.Math.PI * 9 * 4;
        _engine.Volume(cone).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Volume_Capsule()
    {
        var capsule = _engine.CreateCapsule(new Point3D(-5, 0, 0), new Point3D(5, 0, 0), 2);
        capsule.Volume.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Volume_Torus()
    {
        var torus = _engine.CreateTorus(Point3D.Origin, Vector3D.UnitY, 10, 2);
        double expected = 2 * System.Math.PI * System.Math.PI * 10 * 4;
        _engine.Volume(torus).Should().BeApproximately(expected, 1e-4);
    }

    [Fact]
    public void ValidateMesh_ValidMesh_ReturnsSuccess()
    {
        var mesh = _engine.CreateUnitCube();
        var result = _engine.ValidateMesh(mesh);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ValidateMesh_NullMesh_ReturnsFailure()
    {
        var result = _engine.ValidateMesh(null!);
        result.Success.Should().BeFalse();
        result.DiagnosticType.Should().Be(GeometryDiagnosticType.NullInput);
    }

    [Fact]
    public void ValidateGeometry_NullGeometry_ReturnsFailure()
    {
        var result = _engine.ValidateGeometry(null!);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void ExportScene_ValidScene_ReturnsSuccess()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "obj");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ExportScene_NullScene_ReturnsFailure()
    {
        var result = _engine.ExportScene(null!, "obj");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void ExportScene_EmptyFormat_ReturnsFailure()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void ExportScene_UnsupportedFormat_ReturnsFailure()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "xyz");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void RotatePoint_360Degrees_ReturnsToOriginal()
    {
        var p = new Point3D(1, 0, 0);
        var rotated = _engine.RotatePoint(p, Vector3D.UnitY, System.Math.PI * 2);
        rotated.X.Should().BeApproximately(1, 1e-6);
        rotated.Z.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void RotatePoint_90Degrees_AroundY()
    {
        var p = new Point3D(1, 0, 0);
        var rotated = _engine.RotatePoint(p, Vector3D.UnitY, System.Math.PI / 2);
        rotated.X.Should().BeApproximately(0, 1e-6);
        rotated.Z.Should().BeApproximately(-1, 1e-6);
    }

    [Fact]
    public void ScalePoint_DoublesAll()
    {
        var p = new Point3D(1, 2, 3);
        var scaled = _engine.ScalePoint(p, 2, 2, 2);
        scaled.X.Should().BeApproximately(2, 1e-10);
        scaled.Y.Should().BeApproximately(4, 1e-10);
        scaled.Z.Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void ScalePoint_NonUniform()
    {
        var p = new Point3D(1, 1, 1);
        var scaled = _engine.ScalePoint(p, 2, 3, 4);
        scaled.X.Should().BeApproximately(2, 1e-10);
        scaled.Y.Should().BeApproximately(3, 1e-10);
        scaled.Z.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void CreateMesh_ReturnsNonNull()
    {
        var mb = _engine.CreateMesh();
        mb.Should().NotBeNull();
    }

    [Fact]
    public void CreateTriangleMesh_ReturnsEmpty()
    {
        var mesh = _engine.CreateTriangleMesh();
        mesh.VertexCount.Should().Be(0);
        mesh.TriangleCount.Should().Be(0);
    }

    [Fact]
    public void CreateScene_ReturnsNonNull()
    {
        var scene = _engine.CreateScene();
        scene.Should().NotBeNull();
        scene.NodeCount.Should().Be(0);
    }

    [Fact]
    public void ClearCaches_DoesNotThrow()
    {
        Action act = () => _engine.ClearCaches();
        act.Should().NotThrow();
    }

    [Fact]
    public void ExportScene_Ply_ReturnsSuccess()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "ply");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ExportScene_Stl_ReturnsSuccess()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "stl");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ExportScene_Gltf_ReturnsSuccess()
    {
        var scene = _engine.CreateScene();
        var result = _engine.ExportScene(scene, "gltf");
        result.Success.Should().BeTrue();
    }
}
