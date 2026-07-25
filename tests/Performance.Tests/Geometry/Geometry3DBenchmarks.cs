using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Transformations;

namespace MathVerse.Performance.Tests.Geometry;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Geometry3DBenchmarks
{
    private Point3D _p1;
    private Point3D _p2;
    private Point3D _p3;
    private Point3D _p4;
    private Point3D _p5;
    private Vector3D _v1;
    private Vector3D _v2;
    private Vector3D _v3;
    private Line3D _line1;
    private Line3D _line2;
    private Line3D _line3;
    private Plane3D _plane1;
    private Plane3D _plane2;
    private Plane3D _plane3;
    private Triangle3D _tri1;
    private Triangle3D _tri2;
    private Quad3D _quad1;
    private Sphere3D _sphere1;
    private Sphere3D _sphere2;
    private Cylinder3D _cylinder1;
    private Cone3D _cone1;
    private Capsule3D _capsule1;
    private Cube3D _cube1;
    private BoundingBox3D _box1;
    private BoundingBox3D _box2;
    private BoundingSphere3D _bsphere1;
    private BoundingSphere3D _bsphere2;
    private Transform3D _transform;
    private Point3D[] _points = null!;

    [GlobalSetup]
    public void Setup()
    {
        _p1 = new Point3D(1.0, 2.0, 3.0);
        _p2 = new Point3D(4.0, 5.0, 6.0);
        _p3 = new Point3D(7.0, 1.0, 2.0);
        _p4 = new Point3D(3.0, 6.0, 9.0);
        _p5 = new Point3D(-1.0, -2.0, -3.0);

        _v1 = new Vector3D(1.0, 2.0, 3.0);
        _v2 = new Vector3D(4.0, 5.0, 6.0);
        _v3 = new Vector3D(0.0, 1.0, 0.0);

        _line1 = new Line3D(_p1, _p2);
        _line2 = new Line3D(_p3, _p4);
        _line3 = new Line3D(_p5, new Point3D(2.0, 3.0, 4.0));

        _plane1 = new Plane3D(_p1, _v3.Normalize());
        _plane2 = new Plane3D(_p3, new Vector3D(0.0, 0.0, 1.0));
        _plane3 = new Plane3D(Point3D.Origin, new Vector3D(1.0, 1.0, 0.0).Normalize());

        _tri1 = new Triangle3D(_p1, _p2, _p3);
        _tri2 = new Triangle3D(Point3D.Origin, new Point3D(1.0, 0.0, 0.0), new Point3D(0.0, 1.0, 0.0));

        _quad1 = new Quad3D(_p1, _p2, _p3, _p4);

        _sphere1 = new Sphere3D(_p1, 5.0);
        _sphere2 = new Sphere3D(_p3, 3.0);

        _cylinder1 = new Cylinder3D(_p1, 3.0, 10.0);

        _cone1 = new Cone3D(_p1, _v3.Normalize(), 3.0, 10.0);

        _capsule1 = new Capsule3D(_p1, _p2, 2.0);

        _cube1 = new Cube3D(_p1, 5.0);

        _box1 = new BoundingBox3D(_p1, _p2);
        _box2 = new BoundingBox3D(_p3, _p4);

        _bsphere1 = new BoundingSphere3D(_p1, 5.0);
        _bsphere2 = new BoundingSphere3D(_p3, 3.0);

        _transform = Transform3D.Translation(1.0, 2.0, 3.0);

        _points = new[] { _p1, _p2, _p3, _p4, _p5, new Point3D(10.0, 10.0, 10.0) };
    }

    #region Point3D

    [BenchmarkCategory("Point3D"), Benchmark(Baseline = true)]
    public double Point3D_DistanceTo() => _p1.DistanceTo(_p2);

    [BenchmarkCategory("Point3D"), Benchmark]
    public double Point3D_DistanceSquaredTo() => _p1.DistanceSquaredTo(_p2);

    [BenchmarkCategory("Point3D"), Benchmark]
    public Point3D Point3D_Lerp() => _p1.Lerp(_p2, 0.5);

    [BenchmarkCategory("Point3D"), Benchmark]
    public Vector3D Point3D_ToVector3D() => _p1.ToVector3D();

    [BenchmarkCategory("Point3D"), Benchmark]
    public Point3D Point3D_Translate() => _p1.Translate(_v1);

    #endregion

    #region Vector3D

    [BenchmarkCategory("Vector3D"), Benchmark(Baseline = true)]
    public double Vector3D_Length() => _v1.Length;

    [BenchmarkCategory("Vector3D"), Benchmark]
    public double Vector3D_LengthSquared() => _v1.LengthSquared;

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Normalize() => _v1.Normalize();

    [BenchmarkCategory("Vector3D"), Benchmark]
    public double Vector3D_Dot() => _v1.Dot(_v2);

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Cross() => _v1.Cross(_v2);

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Add() => _v1.Add(_v2);

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Subtract() => _v1.Subtract(_v2);

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Scale() => _v1.Scale(2.5);

    [BenchmarkCategory("Vector3D"), Benchmark]
    public Vector3D Vector3D_Negate() => _v1.Negate();

    [BenchmarkCategory("Vector3D"), Benchmark]
    public double Vector3D_AngleTo() => _v1.AngleTo(_v2);

    #endregion

    #region Line3D

    [BenchmarkCategory("Line3D"), Benchmark(Baseline = true)]
    public Vector3D Line3D_Direction() => _line1.Direction;

    [BenchmarkCategory("Line3D"), Benchmark]
    public double Line3D_Length() => _line1.Length;

    [BenchmarkCategory("Line3D"), Benchmark]
    public Point3D Line3D_PointAt() => _line1.PointAt(0.5);

    [BenchmarkCategory("Line3D"), Benchmark]
    public double Line3D_DistanceTo() => _line1.DistanceTo(_p3);

    [BenchmarkCategory("Line3D"), Benchmark]
    public Point3D Line3D_ClosestPoint() => _line1.ClosestPoint(_p3);

    [BenchmarkCategory("Line3D"), Benchmark]
    public (bool hit, Point3D point) Line3D_IntersectPlane() => _line1.Intersect(_plane1);

    [BenchmarkCategory("Line3D"), Benchmark]
    public (bool hit, Point3D point, double distance) Line3D_IntersectLine() => _line1.Intersect(_line2);

    [BenchmarkCategory("Line3D"), Benchmark]
    public BoundingBox3D Line3D_ToBoundingBox() => _line1.ToBoundingBox();

    #endregion

    #region Plane3D

    [BenchmarkCategory("Plane3D"), Benchmark(Baseline = true)]
    public double Plane3D_SignedDistanceTo() => _plane1.SignedDistanceTo(_p2);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public double Plane3D_DistanceTo() => _plane1.DistanceTo(_p2);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public bool Plane3D_Contains() => _plane1.Contains(_p1);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public Point3D Plane3D_Project() => _plane1.Project(_p2);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public (bool hit, Point3D point) Plane3D_IntersectLine() => _plane1.Intersect(_line1);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public (bool hit, Line3D line) Plane3D_IntersectPlane() => _plane1.Intersect(_plane2);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public Plane3D Plane3D_Transform() => _plane1.Transform(_transform);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public Plane3D Plane3D_FromTriangle() => Plane3D.FromTriangle(_tri1);

    [BenchmarkCategory("Plane3D"), Benchmark]
    public Plane3D Plane3D_FromPoints() => Plane3D.FromPoints(_p1, _p2, _p3);

    #endregion

    #region Triangle3D

    [BenchmarkCategory("Triangle3D"), Benchmark(Baseline = true)]
    public Vector3D Triangle3D_Normal() => _tri1.Normal;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public double Triangle3D_Area() => _tri1.Area;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public double Triangle3D_Perimeter() => _tri1.Perimeter;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public Point3D Triangle3D_Centroid() => _tri1.Centroid;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public Point3D Triangle3D_Circumcenter() => _tri1.Circumcenter;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public Plane3D Triangle3D_Plane() => _tri1.Plane;

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public (double u, double v, double w) Triangle3D_BarycentricCoords() => _tri1.BarycentricCoords(_p3);

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public bool Triangle3D_Contains() => _tri1.Contains(_p3);

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public Point3D Triangle3D_ClosestPoint() => _tri1.ClosestPoint(_p5);

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public (bool hit, Point3D point) Triangle3D_Intersect() => _tri1.Intersect(_line1);

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public bool Triangle3D_IsDegenerate() => _tri1.IsDegenerate();

    [BenchmarkCategory("Triangle3D"), Benchmark]
    public BoundingBox3D Triangle3D_ToBoundingBox() => _tri1.ToBoundingBox();

    #endregion

    #region Quad3D

    [BenchmarkCategory("Quad3D"), Benchmark(Baseline = true)]
    public (Triangle3D tri1, Triangle3D tri2) Quad3D_Triangulate() => _quad1.Triangulate();

    [BenchmarkCategory("Quad3D"), Benchmark]
    public Vector3D Quad3D_Normal() => _quad1.Normal;

    [BenchmarkCategory("Quad3D"), Benchmark]
    public Point3D Quad3D_Centroid() => _quad1.Centroid;

    [BenchmarkCategory("Quad3D"), Benchmark]
    public double Quad3D_Area() => _quad1.Area;

    [BenchmarkCategory("Quad3D"), Benchmark]
    public bool Quad3D_Contains() => _quad1.Contains(_p3);

    #endregion

    #region Sphere3D

    [BenchmarkCategory("Sphere3D"), Benchmark(Baseline = true)]
    public double Sphere3D_Volume() => _sphere1.Volume;

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public double Sphere3D_SurfaceArea() => _sphere1.SurfaceArea;

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public bool Sphere3D_ContainsPoint() => _sphere1.Contains(_p2);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public bool Sphere3D_ContainsBox() => _sphere1.Contains(_box1);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public double Sphere3D_DistanceTo() => _sphere1.DistanceTo(_p5);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public (bool hit, ImmutableArray<Point3D> points) Sphere3D_IntersectLine() => _sphere1.Intersect(_line1);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public (bool hit, CircleOnPlane circle) Sphere3D_IntersectPlane() => _sphere1.Intersect(_plane1);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public Point3D Sphere3D_ClosestPointOnSurface() => _sphere1.ClosestPointOnSurface(_p5);

    [BenchmarkCategory("Sphere3D"), Benchmark]
    public BoundingBox3D Sphere3D_ToBoundingBox() => _sphere1.ToBoundingBox();

    #endregion

    #region Cylinder3D

    [BenchmarkCategory("Cylinder3D"), Benchmark(Baseline = true)]
    public double Cylinder3D_Volume() => _cylinder1.Volume;

    [BenchmarkCategory("Cylinder3D"), Benchmark]
    public double Cylinder3D_SurfaceArea() => _cylinder1.SurfaceArea;

    [BenchmarkCategory("Cylinder3D"), Benchmark]
    public Point3D Cylinder3D_PointAt() => _cylinder1.PointAt(0.5, 1.0);

    [BenchmarkCategory("Cylinder3D"), Benchmark]
    public BoundingBox3D Cylinder3D_ToBoundingBox() => _cylinder1.ToBoundingBox();

    #endregion

    #region Cone3D

    [BenchmarkCategory("Cone3D"), Benchmark(Baseline = true)]
    public double Cone3D_SlantHeight() => _cone1.SlantHeight;

    [BenchmarkCategory("Cone3D"), Benchmark]
    public double Cone3D_Volume() => _cone1.Volume;

    [BenchmarkCategory("Cone3D"), Benchmark]
    public double Cone3D_SurfaceArea() => _cone1.SurfaceArea;

    [BenchmarkCategory("Cone3D"), Benchmark]
    public BoundingBox3D Cone3D_ToBoundingBox() => _cone1.ToBoundingBox();

    #endregion

    #region Capsule3D

    [BenchmarkCategory("Capsule3D"), Benchmark(Baseline = true)]
    public double Capsule3D_Length() => _capsule1.Length;

    [BenchmarkCategory("Capsule3D"), Benchmark]
    public double Capsule3D_Volume() => _capsule1.Volume;

    [BenchmarkCategory("Capsule3D"), Benchmark]
    public double Capsule3D_SurfaceArea() => _capsule1.SurfaceArea;

    [BenchmarkCategory("Capsule3D"), Benchmark]
    public bool Capsule3D_Contains() => _capsule1.Contains(_p3);

    [BenchmarkCategory("Capsule3D"), Benchmark]
    public Point3D Capsule3D_ClosestPoint() => _capsule1.ClosestPoint(_p5);

    [BenchmarkCategory("Capsule3D"), Benchmark]
    public BoundingBox3D Capsule3D_ToBoundingBox() => _capsule1.ToBoundingBox();

    #endregion

    #region Cube3D

    [BenchmarkCategory("Cube3D"), Benchmark(Baseline = true)]
    public double Cube3D_Volume() => _cube1.Volume;

    [BenchmarkCategory("Cube3D"), Benchmark]
    public double Cube3D_SurfaceArea() => _cube1.SurfaceArea;

    [BenchmarkCategory("Cube3D"), Benchmark]
    public ImmutableArray<Point3D> Cube3D_Vertices() => _cube1.Vertices;

    [BenchmarkCategory("Cube3D"), Benchmark]
    public ImmutableArray<Quad3D> Cube3D_Faces() => _cube1.Faces;

    [BenchmarkCategory("Cube3D"), Benchmark]
    public bool Cube3D_Contains() => _cube1.Contains(_p3);

    [BenchmarkCategory("Cube3D"), Benchmark]
    public BoundingBox3D Cube3D_ToBoundingBox() => _cube1.ToBoundingBox();

    #endregion

    #region BoundingBox3D

    [BenchmarkCategory("BoundingBox3D"), Benchmark(Baseline = true)]
    public double BoundingBox3D_Width() => _box1.Width;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public double BoundingBox3D_Height() => _box1.Height;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public double BoundingBox3D_Depth() => _box1.Depth;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public Point3D BoundingBox3D_Center() => _box1.Center;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public double BoundingBox3D_Volume() => _box1.Volume;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public double BoundingBox3D_SurfaceArea() => _box1.SurfaceArea;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public ImmutableArray<Point3D> BoundingBox3D_Corners() => _box1.Corners;

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public bool BoundingBox3D_ContainsPoint() => _box1.Contains(_p3);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public bool BoundingBox3D_ContainsBox() => _box1.Contains(_box2);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public bool BoundingBox3D_Intersects() => _box1.Intersects(_box2);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public BoundingBox3D BoundingBox3D_Union() => _box1.Union(_box2);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public BoundingBox3D BoundingBox3D_Inflate() => _box1.Inflate(1.0);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public BoundingBox3D BoundingBox3D_Transform() => _box1.Transform(_transform);

    [BenchmarkCategory("BoundingBox3D"), Benchmark]
    public BoundingBox3D BoundingBox3D_FromPoints() => BoundingBox3D.FromPoints(_points);

    #endregion

    #region BoundingSphere3D

    [BenchmarkCategory("BoundingSphere3D"), Benchmark(Baseline = true)]
    public bool BoundingSphere3D_ContainsPoint() => _bsphere1.Contains(_p3);

    [BenchmarkCategory("BoundingSphere3D"), Benchmark]
    public bool BoundingSphere3D_ContainsSphere() => _bsphere1.Contains(_bsphere2);

    [BenchmarkCategory("BoundingSphere3D"), Benchmark]
    public bool BoundingSphere3D_Intersects() => _bsphere1.Intersects(_bsphere2);

    [BenchmarkCategory("BoundingSphere3D"), Benchmark]
    public BoundingSphere3D BoundingSphere3D_Union() => _bsphere1.Union(_bsphere2);

    [BenchmarkCategory("BoundingSphere3D"), Benchmark]
    public BoundingSphere3D BoundingSphere3D_FromPoints() => BoundingSphere3D.FromPoints(_points);

    #endregion

    #region Geometry3DOperations

    [BenchmarkCategory("Geometry3DOperations"), Benchmark(Baseline = true)]
    public double Ops_DistancePointPoint() => Geometry3DOperations.Distance(_p1, _p2);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_DistanceLinePoint() => Geometry3DOperations.Distance(_line1, _p3);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_DistancePlanePoint() => Geometry3DOperations.Distance(_plane1, _p2);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public (bool hit, Point3D point, double distance) Ops_IntersectLineLine() => Geometry3DOperations.Intersect(_line1, _line2);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public (bool hit, Point3D point) Ops_IntersectLinePlane() => Geometry3DOperations.Intersect(_line1, _plane1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public (bool hit, Line3D line) Ops_IntersectPlanePlane() => Geometry3DOperations.Intersect(_plane1, _plane2);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public (bool hit, ImmutableArray<Point3D> points) Ops_IntersectLineSphere() => Geometry3DOperations.Intersect(_line1, _sphere1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public (bool hit, Point3D point) Ops_IntersectTriangleLine() => Geometry3DOperations.Intersect(_tri1, _line1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public Point3D Ops_ProjectPointPlane() => Geometry3DOperations.Project(_p2, _plane1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public Point3D Ops_ProjectPointLine() => Geometry3DOperations.Project(_p3, _line1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public Point3D Ops_ClosestPointTriangle() => Geometry3DOperations.ClosestPoint(_tri1, _p5);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_VolumeSphere() => Geometry3DOperations.Volume(_sphere1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_VolumeCylinder() => Geometry3DOperations.Volume(_cylinder1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_VolumeCone() => Geometry3DOperations.Volume(_cone1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_SurfaceAreaSphere() => Geometry3DOperations.SurfaceArea(_sphere1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public double Ops_SurfaceAreaCylinder() => Geometry3DOperations.SurfaceArea(_cylinder1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public Vector3D Ops_NormalTriangle() => Geometry3DOperations.Normal(_tri1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public BoundingBox3D Ops_BoundingBoxTriangle() => Geometry3DOperations.BoundingBox(_tri1);

    [BenchmarkCategory("Geometry3DOperations"), Benchmark]
    public BoundingBox3D Ops_BoundingBoxSphere() => Geometry3DOperations.BoundingBox(_sphere1);

    #endregion
}
