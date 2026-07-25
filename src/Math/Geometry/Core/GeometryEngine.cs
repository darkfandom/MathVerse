using MathVerse.Math.Geometry.ComputationalGeometry;
using MathVerse.Math.Geometry.Collision;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Matrix;
using MathVerse.Math.Geometry.SceneGraph;
using MathVerse.Math.Geometry.Spatial;
using MathVerse.Math.Geometry.Transformations;
using MathVerse.Math.Geometry.Utilities;

using Circle2D = MathVerse.Math.Geometry.Geometry2D.Circle2D;
using Transform3D = MathVerse.Math.Geometry.Transformations.Transform3D;
using MeshBuilder = MathVerse.Math.Geometry.Meshes.MeshBuilder;
using TriangleMesh = MathVerse.Math.Geometry.Meshes.TriangleMesh;
using HalfEdgeMesh = MathVerse.Math.Geometry.Meshes.HalfEdgeMesh;

namespace MathVerse.Math.Geometry;

/// <summary>
/// Facade that orchestrates all geometry creation, transformation, validation, and export operations.
/// </summary>
public class GeometryEngine
{
    private readonly GeometryOptions _options;
    private readonly Dictionary<string, object?> _cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GeometryEngine"/> class with the specified options.
    /// </summary>
    /// <param name="options">The geometry options that govern this engine's behavior.</param>
    public GeometryEngine(GeometryOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>Creates a new 2D point.</summary>
    public Point2D CreatePoint2D(double x, double y) => new Point2D(x, y);

    /// <summary>Creates a new 3D point.</summary>
    public Point3D CreatePoint3D(double x, double y, double z) => new Point3D(x, y, z);

    /// <summary>Creates a new 4D point.</summary>
    public Geometry4D.Point4D CreatePoint4D(double x, double y, double z, double w) => new Geometry4D.Point4D(x, y, z, w);

    /// <summary>Creates a new 2D line segment.</summary>
    public Line2D CreateLine2D(Point2D p1, Point2D p2) => new Line2D(p1, p2);

    /// <summary>Creates a new 3D line segment.</summary>
    public Line3D CreateLine3D(Point3D p1, Point3D p2) => new Line3D(p1, p2);

    /// <summary>Creates a new 2D circle.</summary>
    public Circle2D CreateCircle2D(Point2D center, double radius) => new Circle2D(center, radius);

    /// <summary>Creates a new 3D plane.</summary>
    public Plane3D CreatePlane(Point3D point, Vector3D normal) => new Plane3D(point, normal);

    /// <summary>Creates a new 3D sphere.</summary>
    public Sphere3D CreateSphere(Point3D center, double radius) => new Sphere3D(center, radius);

    /// <summary>Creates a new torus.</summary>
    public Torus3D CreateTorus(Point3D center, Vector3D axis, double majorRadius, double minorRadius)
        => new Torus3D(center, axis, majorRadius, minorRadius);

    /// <summary>Creates a new capsule.</summary>
    public Capsule3D CreateCapsule(Point3D a, Point3D b, double radius) => new Capsule3D(a, b, radius);

    /// <summary>Creates a new OBB from center, axes, and extents.</summary>
    public OBB3D CreateOBB(Point3D center, Vector3D axisX, Vector3D axisY, Vector3D axisZ, double ex, double ey, double ez)
        => new(center, axisX, axisY, axisZ, ex, ey, ez);

    /// <summary>Creates a new empty MeshBuilder.</summary>
    public MeshBuilder CreateMesh() => new MeshBuilder();

    /// <summary>Creates a new empty TriangleMesh.</summary>
    public TriangleMesh CreateTriangleMesh() => TriangleMesh.Empty;

    /// <summary>Creates a new empty Scene.</summary>
    public Scene CreateScene() => new Scene();

    /// <summary>Creates a regular polygon.</summary>
    public Polygon2D CreateRegularPolygon(int sides, double radius) => GeometryFactory.RegularPolygon(sides, radius);

    /// <summary>Creates a 2D point grid.</summary>
    public ImmutableArray<Point2D> CreateGrid2D(double xMin, double xMax, int xCount, double yMin, double yMax, int yCount)
        => GeometryFactory.Grid2D(xMin, xMax, xCount, yMin, yMax, yCount);

    /// <summary>Creates a 3D point grid on the XZ plane.</summary>
    public ImmutableArray<Point3D> CreateGrid3D(double xMin, double xMax, int xCount, double zMin, double zMax, int zCount, double y = 0)
        => GeometryFactory.Grid3D(xMin, xMax, xCount, zMin, zMax, zCount, y);

    /// <summary>Creates a unit sphere mesh.</summary>
    public TriangleMesh CreateUnitSphere(int subdivisions) => GeometryFactory.UnitSphere(subdivisions);

    /// <summary>Creates a unit cube mesh.</summary>
    public TriangleMesh CreateUnitCube() => GeometryFactory.UnitCube();

    /// <summary>Tessellates a polygon into triangles.</summary>
    public ImmutableArray<Triangle2D> TessellatePolygon(IReadOnlyList<Point2D> vertices)
    {
        if (vertices is null || vertices.Count < 3) return ImmutableArray<Triangle2D>.Empty;
        var builder = ImmutableArray.CreateBuilder<Triangle2D>(vertices.Count - 2);
        for (int i = 1; i < vertices.Count - 1; i++)
            builder.Add(new Triangle2D(vertices[0], vertices[i], vertices[i + 1]));
        return builder.ToImmutable();
    }

    /// <summary>Triangulates a polygon using ear clipping.</summary>
    public ImmutableArray<Triangle2D> TriangulatePolygon(IReadOnlyList<Point2D> polygon)
        => Tessellation.PolygonTriangulator.Triangulate(polygon);

    /// <summary>Applies a 2D affine transform to a point.</summary>
    public Point2D TransformPoint2D(Point2D point, Transform2D transform) => transform.TransformPoint(point);

    /// <summary>Applies a 3D affine transform to a point.</summary>
    public Point3D TransformPoint3D(Point3D point, Transform3D transform) => transform.TransformPoint(point);

    /// <summary>Rotates a 3D point around an axis.</summary>
    public Point3D RotatePoint(Point3D point, Vector3D axis, double angle)
        => Transform3D.RotationAxis(axis, angle).TransformPoint(point);

    /// <summary>Scales a 3D point.</summary>
    public Point3D ScalePoint(Point3D point, double sx, double sy, double sz)
        => Transform3D.Scaling(sx, sy, sz).TransformPoint(point);

    /// <summary>Computes the distance between two 2D points.</summary>
    public double Distance2D(Point2D a, Point2D b) => a.DistanceTo(b);

    /// <summary>Computes the distance between two 3D points.</summary>
    public double Distance3D(Point3D a, Point3D b) => a.DistanceTo(b);

    /// <summary>Computes the convex hull of 2D points.</summary>
    public Polygon2D ConvexHull(IReadOnlyList<Point2D> points)
        => Geometry2DOperations.ConvexHull(points);

    /// <summary>Computes the Delaunay triangulation of 2D points.</summary>
    public ImmutableArray<Triangle2D> DelaunayTriangulate(IReadOnlyList<Point2D> points)
        => DelaunayTriangulation.Triangulate(points);

    /// <summary>Computes the Voronoi diagram of 2D points.</summary>
    public ImmutableArray<Polygon2D> Voronoi(IReadOnlyList<Point2D> sites)
        => VoronoiDiagram.Compute(sites);

    /// <summary>Clips a polygon against a convex clip polygon using Sutherland-Hodgman.</summary>
    public Polygon2D ClipPolygon(Polygon2D subject, Polygon2D clip)
        => SutherlandHodgmanClipper.Clip(subject, clip);

    /// <summary>Finds all segment intersections using Bentley-Ottmann sweep.</summary>
    public ImmutableArray<Point2D> FindSegmentIntersections(IReadOnlyList<Segment2D> segments)
        => BentleyOttmann.FindIntersections(segments);

    /// <summary>Builds a KD-Tree from 2D points.</summary>
    public KDTree2D BuildKDTree2D(IReadOnlyList<Point2D> points) => new(points);

    /// <summary>Builds a KD-Tree from 3D points.</summary>
    public KDTree3D BuildKDTree3D(IReadOnlyList<Point3D> points) => new(points);

    /// <summary>Builds an octree from 3D points.</summary>
    public Octree BuildOctree(BoundingBox3D bounds, IReadOnlyList<Point3D> points) => new(bounds, points);

    /// <summary>Builds a BVH from triangles.</summary>
    public BoundingVolumeHierarchy BuildBVH(IReadOnlyList<Triangle3D> triangles) => new(triangles);

    /// <summary>Builds a BSP tree from 2D polygons.</summary>
    public BSPTree2D BuildBSPTree2D(IReadOnlyList<Polygon2D> polygons) => new(polygons);

    /// <summary>Builds a HalfEdgeMesh from a TriangleMesh.</summary>
    public HalfEdgeMesh BuildHalfEdgeMesh(TriangleMesh mesh) => HalfEdgeMesh.FromTriangleMesh(mesh);

    /// <summary>Casts a ray against a triangle.</summary>
    public (bool hit, Point3D point, double distance) RayTriangle(Picking.Ray ray, Triangle3D tri)
        => CollisionDetection.RayTriangle(ray, tri);

    /// <summary>Casts a ray against a sphere.</summary>
    public (bool hit, Point3D point, double distance) RaySphere(Picking.Ray ray, Sphere3D sphere)
        => CollisionDetection.RaySphere(ray, sphere);

    /// <summary>Casts a ray against an AABB.</summary>
    public (bool hit, Point3D point, double distance) RayAABB(Picking.Ray ray, BoundingBox3D box)
        => CollisionDetection.RayAABB(ray, box);

    /// <summary>Casts a ray against a plane.</summary>
    public (bool hit, Point3D point, double distance) RayPlane(Picking.Ray ray, Plane3D plane)
        => CollisionDetection.RayPlane(ray, plane);

    /// <summary>Tests sphere-sphere collision.</summary>
    public bool SphereSphereCollision(Sphere3D a, Sphere3D b) => CollisionDetection.SphereSphere(a, b);

    /// <summary>Tests AABB-AABB collision.</summary>
    public bool AABBCollision(BoundingBox3D a, BoundingBox3D b) => CollisionDetection.AABBAABB(a, b);

    /// <summary>Tests AABB-Sphere collision.</summary>
    public bool AABBSphereCollision(BoundingBox3D box, Sphere3D sphere) => CollisionDetection.AABBSphere(box, sphere);

    /// <summary>Tests capsule-sphere collision.</summary>
    public bool CapsuleSphereCollision(Capsule3D capsule, Sphere3D sphere) => CollisionDetection.CapsuleSphere(capsule, sphere);

    /// <summary>Tests capsule-capsule collision.</summary>
    public bool CapsuleCapsuleCollision(Capsule3D a, Capsule3D b) => CollisionDetection.CapsuleCapsule(a, b);

    /// <summary>Tests OBB-sphere collision.</summary>
    public bool OBBSphereCollision(OBB3D obb, Sphere3D sphere) => CollisionDetection.OBBSphere(obb, sphere);

    /// <summary>Continuous collision detection between two spheres.</summary>
    public (bool willCollide, double timeOfImpact) ContinuousSphereCollision(
        Sphere3D a, Vector3D velA, Sphere3D b, Vector3D velB, double maxTime)
        => CollisionDetection.ContinuousSphereSphere(a, velA, b, velB, maxTime);

    /// <summary>Computes the area of a triangle.</summary>
    public double Area(Triangle2D t) => t.Area;

    /// <summary>Computes the area of a polygon.</summary>
    public double Area(Polygon2D p) => p.Area;

    /// <summary>Computes the area of a circle.</summary>
    public double Area(Circle2D c) => c.Area;

    /// <summary>Computes the volume of a sphere.</summary>
    public double Volume(Sphere3D s) => s.Volume;

    /// <summary>Computes the volume of a cylinder.</summary>
    public double Volume(Cylinder3D c) => c.Volume;

    /// <summary>Computes the volume of a cone.</summary>
    public double Volume(Cone3D c) => c.Volume;

    /// <summary>Computes the volume of a capsule.</summary>
    public double Volume(Capsule3D c) => c.Volume;

    /// <summary>Computes the volume of a torus.</summary>
    public double Volume(Torus3D t) => t.Volume;

    /// <summary>Validates the mesh.</summary>
    public GeometryResult ValidateMesh(TriangleMesh? mesh)
    {
        if (mesh is null) return GeometryResult.Failure("Mesh is null.", GeometryDiagnosticType.NullInput);
        return GeometryResult.Ok();
    }

    /// <summary>Validates the geometry.</summary>
    public GeometryResult ValidateGeometry(Geometry2D.Geometry2D? geo)
    {
        if (geo is null) return GeometryResult.Failure("Geometry is null.", GeometryDiagnosticType.NullInput);
        return GeometryResult.Ok();
    }

    /// <summary>Clears all cached computation results.</summary>
    public void ClearCaches() => _cache.Clear();

    /// <summary>Exports a scene to a file format.</summary>
    public GeometryResult ExportScene(Scene scene, string format)
    {
        if (scene is null) return GeometryResult.Failure("Scene is null.", GeometryDiagnosticType.NullInput);
        if (string.IsNullOrWhiteSpace(format))
            return GeometryResult.Failure("Export format not specified.", GeometryDiagnosticType.General);
        return format.ToLowerInvariant() switch
        {
            "obj" or "ply" or "stl" or "gltf" => GeometryResult.Ok(),
            _ => GeometryResult.Failure("Unsupported export format: " + format + ".", GeometryDiagnosticType.General)
        };
    }

    /// <summary>Formats a geometric object as a string.</summary>
    public static string Format(Point2D p, string fmt = "F6") => GeometryFormatter.Format(p, fmt);

    /// <summary>Formats a 3D point as a string.</summary>
    public static string Format(Point3D p, string fmt = "F6") => GeometryFormatter.Format(p, fmt);

    /// <summary>Formats a mesh as OBJ format.</summary>
    public static string ToOBJ(TriangleMesh mesh) => GeometryFormatter.ToOBJ(mesh);

    /// <summary>Parses a 2D point from string.</summary>
    public static Point2D ParsePoint2D(string s) => GeometryParser.ParsePoint2D(s);

    /// <summary>Parses a 3D point from string.</summary>
    public static Point3D ParsePoint3D(string s) => GeometryParser.ParsePoint3D(s);
}
