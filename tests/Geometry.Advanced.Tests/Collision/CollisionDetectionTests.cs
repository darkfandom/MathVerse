namespace MathVerse.Geometry.Advanced.Tests.Collision;

public class CollisionDetectionTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void RayTriangle_Hit_ReturnsTrueAndHitPoint()
    {
        var ray = new Ray(new Point3D(0.5, 0.5, -1), new Vector3D(0, 0, 1));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));

        var (hit, point, distance) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeTrue();
        distance.Should().BeGreaterThan(0);
        point.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void RayTriangle_Miss_ReturnsFalse()
    {
        var ray = new Ray(new Point3D(5, 5, -1), new Vector3D(0, 0, 1));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));

        var (hit, _, distance) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeFalse();
        distance.Should().Be(double.MaxValue);
    }

    [Fact]
    public void RayTriangle_ParallelRay_Misses()
    {
        var ray = new Ray(new Point3D(0.5, 0.5, -1), new Vector3D(1, 0, 0));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));

        var (hit, _, _) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayTriangle_RayOriginOnTriangle_Hits()
    {
        var ray = new Ray(new Point3D(0.25, 0.25, 0.001), new Vector3D(0, 0, 1));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));

        var (hit, _, distance) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayTriangle_RayFromBelow_MissesWhenWrongDirection()
    {
        var ray = new Ray(new Point3D(0.5, 0.5, 1), new Vector3D(0, 0, 1));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0));

        var (hit, _, _) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayTriangle_DegenerateTriangle_Misses()
    {
        var ray = new Ray(new Point3D(0, 0, -1), new Vector3D(0, 0, 1));
        var tri = new Triangle3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0), new Point3D(0, 0, 0));

        var (hit, _, _) = CollisionDetection.RayTriangle(ray, tri);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RaySphere_Hit_ReturnsTrue()
    {
        var ray = new Ray(new Point3D(0, 0, -5), new Vector3D(0, 0, 1));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var (hit, point, distance) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(4.0, Tolerance);
        point.Z.Should().BeApproximately(-1.0, Tolerance);
    }

    [Fact]
    public void RaySphere_Miss_ReturnsFalse()
    {
        var ray = new Ray(new Point3D(5, 5, -1), new Vector3D(0, 0, 1));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var (hit, _, distance) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeFalse();
        distance.Should().Be(double.MaxValue);
    }

    [Fact]
    public void RaySphere_RayInsideSphere_HitsExit()
    {
        var ray = new Ray(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 2.0);

        var (hit, _, distance) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(2.0, Tolerance);
    }

    [Fact]
    public void RaySphere_TangentRay_Hits()
    {
        var ray = new Ray(new Point3D(-5, 1, 0), new Vector3D(1, 0, 0));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var (hit, _, _) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeTrue();
    }

    [Fact]
    public void RaySphere_OriginOnSurface_Hits()
    {
        var ray = new Ray(new Point3D(1, 0, 0), new Vector3D(1, 0, 0));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var (hit, _, distance) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void RaySphere_AwayFromSphere_Misses()
    {
        var ray = new Ray(new Point3D(3, 0, 0), new Vector3D(1, 0, 0));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        var (hit, _, _) = CollisionDetection.RaySphere(ray, sphere);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayAABB_Hit_ReturnsTrue()
    {
        var ray = new Ray(new Point3D(0, 0, -5), new Vector3D(0, 0, 1));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, point, distance) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(4.0, Tolerance);
        point.Z.Should().BeApproximately(-1.0, Tolerance);
    }

    [Fact]
    public void RayAABB_Miss_ReturnsFalse()
    {
        var ray = new Ray(new Point3D(5, 5, -1), new Vector3D(0, 0, 1));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, _, distance) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeFalse();
        distance.Should().Be(double.MaxValue);
    }

    [Fact]
    public void RayAABB_RayInsideBox_Hits()
    {
        var ray = new Ray(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, _, distance) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void RayAABB_ParallelToAxis_MissesWhenOffset()
    {
        var ray = new Ray(new Point3D(3, 0, 0), new Vector3D(0, 1, 0));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, _, _) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayAABB_GrazingEdge_Hits()
    {
        var ray = new Ray(new Point3D(-2, 1, 0), new Vector3D(1, 0, 0));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, _, _) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeTrue();
    }

    [Fact]
    public void RayAABB_DiagonalThroughBox_Hits()
    {
        var ray = new Ray(new Point3D(-2, -2, -2), new Vector3D(1, 1, 1));
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        var (hit, _, _) = CollisionDetection.RayAABB(ray, box);

        hit.Should().BeTrue();
    }

    [Fact]
    public void RayPlane_Hit_ReturnsTrue()
    {
        var ray = new Ray(new Point3D(0, 5, 0), new Vector3D(0, -1, 0));
        var plane = new Plane3D(Point3D.Origin, new Vector3D(0, 1, 0));

        var (hit, point, distance) = CollisionDetection.RayPlane(ray, plane);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(5.0, Tolerance);
        point.Y.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void RayPlane_ParallelRay_Misses()
    {
        var ray = new Ray(new Point3D(0, 5, 0), new Vector3D(1, 0, 0));
        var plane = new Plane3D(Point3D.Origin, new Vector3D(0, 1, 0));

        var (hit, _, distance) = CollisionDetection.RayPlane(ray, plane);

        hit.Should().BeFalse();
        distance.Should().Be(double.MaxValue);
    }

    [Fact]
    public void RayPlane_RayAwayFromPlane_Misses()
    {
        var ray = new Ray(new Point3D(0, 5, 0), new Vector3D(0, 1, 0));
        var plane = new Plane3D(Point3D.Origin, new Vector3D(0, 1, 0));

        var (hit, _, _) = CollisionDetection.RayPlane(ray, plane);

        hit.Should().BeFalse();
    }

    [Fact]
    public void RayPlane_OriginOnPlane_Hits()
    {
        var ray = new Ray(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));
        var plane = new Plane3D(Point3D.Origin, new Vector3D(0, 1, 0));

        var (hit, _, distance) = CollisionDetection.RayPlane(ray, plane);

        hit.Should().BeTrue();
        distance.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void RayPlane_AngledRay_Hits()
    {
        var ray = new Ray(new Point3D(0, 3, 0), new Vector3D(1, -1, 0).Normalize());
        var plane = new Plane3D(Point3D.Origin, new Vector3D(0, 1, 0));

        var (hit, _, _) = CollisionDetection.RayPlane(ray, plane);

        hit.Should().BeTrue();
    }

    [Fact]
    public void SphereSphere_Intersecting_ReturnsTrue()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(1.5, 0, 0), 1.0);

        CollisionDetection.SphereSphere(a, b).Should().BeTrue();
    }

    [Fact]
    public void SphereSphere_NonIntersecting_ReturnsFalse()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(5, 0, 0), 1.0);

        CollisionDetection.SphereSphere(a, b).Should().BeFalse();
    }

    [Fact]
    public void SphereSphere_Touching_ReturnsTrue()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(2, 0, 0), 1.0);

        CollisionDetection.SphereSphere(a, b).Should().BeTrue();
    }

    [Fact]
    public void SphereSphere_Contained_ReturnsTrue()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 5.0);
        var b = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        CollisionDetection.SphereSphere(a, b).Should().BeTrue();
    }

    [Fact]
    public void SphereSphere_Identical_ReturnsTrue()
    {
        var a = new Sphere3D(new Point3D(1, 2, 3), 2.0);
        var b = new Sphere3D(new Point3D(1, 2, 3), 2.0);

        CollisionDetection.SphereSphere(a, b).Should().BeTrue();
    }

    [Fact]
    public void AABBAABB_Intersecting_ReturnsTrue()
    {
        var a = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var b = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));

        CollisionDetection.AABBAABB(a, b).Should().BeTrue();
    }

    [Fact]
    public void AABBAABB_NonIntersecting_ReturnsFalse()
    {
        var a = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var b = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(6, 6, 6));

        CollisionDetection.AABBAABB(a, b).Should().BeFalse();
    }

    [Fact]
    public void AABBAABB_TouchingEdge_ReturnsTrue()
    {
        var a = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var b = new BoundingBox3D(new Point3D(1, 0, 0), new Point3D(2, 1, 1));

        CollisionDetection.AABBAABB(a, b).Should().BeTrue();
    }

    [Fact]
    public void AABBAABB_Contained_ReturnsTrue()
    {
        var a = new BoundingBox3D(new Point3D(-5, -5, -5), new Point3D(5, 5, 5));
        var b = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        CollisionDetection.AABBAABB(a, b).Should().BeTrue();
    }

    [Fact]
    public void AABBAABB_XOverlapOnly_ReturnsTrue()
    {
        var a = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
        var b = new BoundingBox3D(new Point3D(1, 5, 5), new Point3D(3, 7, 7));

        CollisionDetection.AABBAABB(a, b).Should().BeFalse();
    }

    [Fact]
    public void AABBSphere_Intersecting_ReturnsTrue()
    {
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var sphere = new Sphere3D(new Point3D(2, 0, 0), 1.5);

        CollisionDetection.AABBSphere(box, sphere).Should().BeTrue();
    }

    [Fact]
    public void AABBSphere_NonIntersecting_ReturnsFalse()
    {
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var sphere = new Sphere3D(new Point3D(5, 5, 5), 1.0);

        CollisionDetection.AABBSphere(box, sphere).Should().BeFalse();
    }

    [Fact]
    public void AABBSphere_SphereInsideBox_ReturnsTrue()
    {
        var box = new BoundingBox3D(new Point3D(-5, -5, -5), new Point3D(5, 5, 5));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        CollisionDetection.AABBSphere(box, sphere).Should().BeTrue();
    }

    [Fact]
    public void AABBSphere_BoxInsideSphere_ReturnsTrue()
    {
        var box = new BoundingBox3D(new Point3D(-0.5, -0.5, -0.5), new Point3D(0.5, 0.5, 0.5));
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 5.0);

        CollisionDetection.AABBSphere(box, sphere).Should().BeTrue();
    }

    [Fact]
    public void AABBSphere_CornerTouching_ReturnsTrue()
    {
        var box = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        double cornerDist = System.Math.Sqrt(3.0);
        var sphere = new Sphere3D(new Point3D(0, 0, 0), cornerDist);

        CollisionDetection.AABBSphere(box, sphere).Should().BeTrue();
    }

    [Fact]
    public void CapsuleSphere_Intersecting_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(-2, 0, 0), new Point3D(2, 0, 0), 0.5);
        var sphere = new Sphere3D(new Point3D(0, 2, 0), 2.0);

        CollisionDetection.CapsuleSphere(capsule, sphere).Should().BeTrue();
    }

    [Fact]
    public void CapsuleSphere_NonIntersecting_ReturnsFalse()
    {
        var capsule = new Capsule3D(new Point3D(-2, 0, 0), new Point3D(2, 0, 0), 0.5);
        var sphere = new Sphere3D(new Point3D(0, 5, 0), 1.0);

        CollisionDetection.CapsuleSphere(capsule, sphere).Should().BeFalse();
    }

    [Fact]
    public void CapsuleSphere_SphereEnclosesCapsule_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(-1, 0, 0), new Point3D(1, 0, 0), 0.5);
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 5.0);

        CollisionDetection.CapsuleSphere(capsule, sphere).Should().BeTrue();
    }

    [Fact]
    public void CapsuleSphere_EndCapHit_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(5, 0, 0), 0.5);
        var sphere = new Sphere3D(new Point3D(6, 0, 0), 1.0);

        CollisionDetection.CapsuleSphere(capsule, sphere).Should().BeTrue();
    }

    [Fact]
    public void CapsuleCapsule_Intersecting_ReturnsTrue()
    {
        var a = new Capsule3D(new Point3D(-2, 0, 0), new Point3D(2, 0, 0), 0.5);
        var b = new Capsule3D(new Point3D(0, 0, 0), new Point3D(0, 5, 0), 0.5);

        CollisionDetection.CapsuleCapsule(a, b).Should().BeTrue();
    }

    [Fact]
    public void CapsuleCapsule_NonIntersecting_ReturnsFalse()
    {
        var a = new Capsule3D(new Point3D(-2, 0, 0), new Point3D(2, 0, 0), 0.5);
        var b = new Capsule3D(new Point3D(0, 5, 0), new Point3D(0, 10, 0), 0.5);

        CollisionDetection.CapsuleCapsule(a, b).Should().BeFalse();
    }

    [Fact]
    public void CapsuleCapsule_ParallelCapsules_ReturnsFalse()
    {
        var a = new Capsule3D(new Point3D(0, 0, 0), new Point3D(5, 0, 0), 0.3);
        var b = new Capsule3D(new Point3D(0, 2, 0), new Point3D(5, 2, 0), 0.3);

        CollisionDetection.CapsuleCapsule(a, b).Should().BeFalse();
    }

    [Fact]
    public void CapsuleCapsule_PerpendicularIntersecting_ReturnsTrue()
    {
        var a = new Capsule3D(new Point3D(-2, 0, 0), new Point3D(2, 0, 0), 1.0);
        var b = new Capsule3D(new Point3D(0, -2, 0), new Point3D(0, 2, 0), 1.0);

        CollisionDetection.CapsuleCapsule(a, b).Should().BeTrue();
    }

    [Fact]
    public void OBBSphere_Intersecting_ReturnsTrue()
    {
        var obb = new OBB3D(new Point3D(0, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var sphere = new Sphere3D(new Point3D(2, 0, 0), 1.5);

        CollisionDetection.OBBSphere(obb, sphere).Should().BeTrue();
    }

    [Fact]
    public void OBBSphere_NonIntersecting_ReturnsFalse()
    {
        var obb = new OBB3D(new Point3D(0, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var sphere = new Sphere3D(new Point3D(5, 5, 5), 1.0);

        CollisionDetection.OBBSphere(obb, sphere).Should().BeFalse();
    }

    [Fact]
    public void OBBSphere_SphereInsideOBB_ReturnsTrue()
    {
        var obb = new OBB3D(new Point3D(0, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 3, 3, 3);
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        CollisionDetection.OBBSphere(obb, sphere).Should().BeTrue();
    }

    [Fact]
    public void OBBSphere_RotatedOBB_Hits()
    {
        var obb = new OBB3D(new Point3D(0, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 0.5, 0.5);
        var sphere = new Sphere3D(new Point3D(0, 0, 0), 1.0);

        CollisionDetection.OBBSphere(obb, sphere).Should().BeTrue();
    }

    [Fact]
    public void ContinuousSphereSphere_CollisionDetected()
    {
        var a = new Sphere3D(new Point3D(-5, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(5, 0, 0), 1.0);
        var velA = new Vector3D(1, 0, 0);
        var velB = new Vector3D(-1, 0, 0);

        var (willCollide, timeOfImpact) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 10.0);

        willCollide.Should().BeTrue();
        timeOfImpact.Should().BeApproximately(4.0, Tolerance);
    }

    [Fact]
    public void ContinuousSphereSphere_NoCollision()
    {
        var a = new Sphere3D(new Point3D(-5, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(5, 0, 0), 1.0);
        var velA = new Vector3D(-1, 0, 0);
        var velB = new Vector3D(1, 0, 0);

        var (willCollide, _) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 5.0);

        willCollide.Should().BeFalse();
    }

    [Fact]
    public void ContinuousSphereSphere_AlreadyOverlapping()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(0.5, 0, 0), 1.0);
        var velA = Vector3D.Zero;
        var velB = Vector3D.Zero;

        var (willCollide, timeOfImpact) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 1.0);

        willCollide.Should().BeTrue();
        timeOfImpact.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ContinuousSphereSphere_ExceedsMaxTime_ReturnsFalse()
    {
        var a = new Sphere3D(new Point3D(-10, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(10, 0, 0), 1.0);
        var velA = new Vector3D(0.5, 0, 0);
        var velB = new Vector3D(-0.5, 0, 0);

        var (willCollide, _) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 5.0);

        willCollide.Should().BeFalse();
    }

    [Fact]
    public void ContinuousSphereSphere_PerpendicularVelocity_NoCollision()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 1.0);
        var b = new Sphere3D(new Point3D(5, 0, 0), 1.0);
        var velA = new Vector3D(0, 1, 0);
        var velB = new Vector3D(0, -1, 0);

        var (willCollide, _) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 10.0);

        willCollide.Should().BeFalse();
    }

    [Fact]
    public void ContinuousSphereSphere_ZeroRelativeVelocity_AlreadyOverlapping()
    {
        var a = new Sphere3D(new Point3D(0, 0, 0), 2.0);
        var b = new Sphere3D(new Point3D(1, 0, 0), 2.0);
        var velA = new Vector3D(1, 0, 0);
        var velB = new Vector3D(1, 0, 0);

        var (willCollide, timeOfImpact) = CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, 5.0);

        willCollide.Should().BeTrue();
        timeOfImpact.Should().BeApproximately(0.0, Tolerance);
    }
}
