using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Collections.Immutable;
using System.Numerics;
using MathVerse.Math.Geometry.Geometry2D;
using Vector2D = MathVerse.Math.Geometry.Geometry2D.Vector2D;

namespace MathVerse.Performance.Tests.Geometry;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Geometry2DBenchmarks
{
    private const int Count = 1000;

    private Point2D[] _points = null!;
    private Vector2D[] _vectors = null!;
    private Line2D[] _lines = null!;
    private Segment2D[] _segments = null!;
    private Circle2D[] _circles = null!;
    private Ellipse2D[] _ellipses = null!;
    private Triangle2D[] _triangles = null!;
    private Rectangle2D[] _rectangles = null!;
    private Polygon2D[] _polygons = null!;
    private BoundingBox2D[] _boxes = null!;
    private Ray2D[] _rays = null!;
    private Arc2D[] _arcs = null!;

    private Point2D _pointA;
    private Point2D _pointB;
    private Vector2D _vecA;
    private Vector2D _vecB;
    private Line2D _lineA;
    private Line2D _lineB;
    private Segment2D _segA;
    private Segment2D _segB;
    private Circle2D _circleA;
    private Circle2D _circleB;
    private Triangle2D _triangle;
    private Rectangle2D _rect;
    private BoundingBox2D _boxA;
    private BoundingBox2D _boxB;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);

        _pointA = new Point2D(1.0, 2.0);
        _pointB = new Point2D(3.0, 4.0);
        _vecA = new Vector2D(1.0, 0.0);
        _vecB = new Vector2D(0.0, 1.0);
        _lineA = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        _lineB = new Line2D(new Point2D(1, 0), new Point2D(0, 1));
        _segA = new Segment2D(new Point2D(0, 0), new Point2D(5, 5));
        _segB = new Segment2D(new Point2D(5, 0), new Point2D(0, 5));
        _circleA = new Circle2D(new Point2D(0, 0), 5.0);
        _circleB = new Circle2D(new Point2D(8, 0), 3.0);
        _triangle = new Triangle2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(2, 3));
        _rect = new Rectangle2D(new Point2D(-2, -2), new Point2D(2, 2));
        _boxA = new BoundingBox2D(new Point2D(0, 0), new Point2D(5, 5));
        _boxB = new BoundingBox2D(new Point2D(3, 3), new Point2D(8, 8));

        _points = new Point2D[Count];
        _vectors = new Vector2D[Count];
        _lines = new Line2D[Count];
        _segments = new Segment2D[Count];
        _circles = new Circle2D[Count];
        _ellipses = new Ellipse2D[Count];
        _triangles = new Triangle2D[Count];
        _rectangles = new Rectangle2D[Count];
        _polygons = new Polygon2D[Count];
        _boxes = new BoundingBox2D[Count];
        _rays = new Ray2D[Count];
        _arcs = new Arc2D[Count];

        for (int i = 0; i < Count; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            _points[i] = new Point2D(x, y);

            double vx = rng.NextDouble() * 2 - 1;
            double vy = rng.NextDouble() * 2 - 1;
            _vectors[i] = new Vector2D(vx, vy);

            double x2 = rng.NextDouble() * 100;
            double y2 = rng.NextDouble() * 100;
            _lines[i] = new Line2D(new Point2D(x, y), new Point2D(x2, y2));

            double x3 = rng.NextDouble() * 100;
            double y3 = rng.NextDouble() * 100;
            _segments[i] = new Segment2D(new Point2D(x, y), new Point2D(x3, y3));

            _circles[i] = new Circle2D(new Point2D(x, y), rng.NextDouble() * 10 + 1);

            double semiMajor = rng.NextDouble() * 10 + 1;
            double semiMinor = rng.NextDouble() * 5 + 0.5;
            _ellipses[i] = new Ellipse2D(new Point2D(x, y), semiMajor, semiMinor, rng.NextDouble() * System.Math.PI);

            double tx2 = rng.NextDouble() * 50;
            double ty2 = rng.NextDouble() * 50;
            double tx3 = rng.NextDouble() * 50;
            double ty3 = rng.NextDouble() * 50;
            _triangles[i] = new Triangle2D(new Point2D(x, y), new Point2D(tx2, ty2), new Point2D(tx3, ty3));

            _rectangles[i] = new Rectangle2D(new Point2D(x, y), new Point2D(x + 5, y + 3));

            _boxes[i] = new BoundingBox2D(new Point2D(x, y), new Point2D(x + 10, y + 10));

            _rays[i] = new Ray2D(new Point2D(x, y), new Vector2D(vx, vy));

            _arcs[i] = new Arc2D(new Point2D(x, y), rng.NextDouble() * 10 + 1, rng.NextDouble() * System.Math.PI, rng.NextDouble() * System.Math.PI + System.Math.PI);
        }

        var polyVertices1 = ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10),
            new Point2D(5, 15), new Point2D(0, 10));
        var polyVertices2 = ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 5),
            new Point2D(0, 5));
        for (int i = 0; i < Count; i++)
        {
            _polygons[i] = new Polygon2D(polyVertices1);
        }
    }

    #region Point2D Benchmarks

    [BenchmarkCategory("Point2D_DistanceTo"), Benchmark(Baseline = true)]
    public double Point2D_DistanceTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _pointA.DistanceTo(_points[i]);
        return sum;
    }

    [BenchmarkCategory("Point2D_DistanceTo"), Benchmark]
    public double Point2D_DistanceSquaredTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _pointA.DistanceSquaredTo(_points[i]);
        return sum;
    }

    [BenchmarkCategory("Point2D_Lerp"), Benchmark(Baseline = true)]
    public Point2D Point2D_Lerp()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _pointA.Lerp(_points[i], 0.5);
        return result;
    }

    [BenchmarkCategory("Point2D_Transform"), Benchmark(Baseline = true)]
    public Vector2D Point2D_ToVector2D()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _points[i].ToVector2D();
        return result;
    }

    [BenchmarkCategory("Point2D_Transform"), Benchmark]
    public Point2D Point2D_Translate()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _points[i].Translate(_vectors[i]);
        return result;
    }

    #endregion

    #region Vector2D Benchmarks

    [BenchmarkCategory("Vector2D_Operations"), Benchmark(Baseline = true)]
    public Vector2D Vector2D_Normalize()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vectors[i].Normalize();
        return result;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public double Vector2D_Dot()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vecA.Dot(_vectors[i]);
        return sum;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public double Vector2D_Cross()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vecA.Cross(_vectors[i]);
        return sum;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public Vector2D Vector2D_Add()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vecA.Add(_vectors[i]);
        return result;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public Vector2D Vector2D_Subtract()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vecA.Subtract(_vectors[i]);
        return result;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public Vector2D Vector2D_Scale()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vectors[i].Scale(2.5);
        return result;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public Vector2D Vector2D_Negate()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vectors[i].Negate();
        return result;
    }

    [BenchmarkCategory("Vector2D_Operations"), Benchmark]
    public Vector2D Vector2D_Perpendicular()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _vectors[i].Perpendicular();
        return result;
    }

    [BenchmarkCategory("Vector2D_Angle"), Benchmark(Baseline = true)]
    public double Vector2D_AngleTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vecA.AngleTo(_vectors[i]);
        return sum;
    }

    [BenchmarkCategory("Vector2D_Angle"), Benchmark]
    public double Vector2D_AngleProperty()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vectors[i].Angle;
        return sum;
    }

    [BenchmarkCategory("Vector2D_Length"), Benchmark(Baseline = true)]
    public double Vector2D_Length()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vectors[i].Length;
        return sum;
    }

    [BenchmarkCategory("Vector2D_Length"), Benchmark]
    public double Vector2D_LengthSquared()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _vectors[i].LengthSquared;
        return sum;
    }

    #endregion

    #region Line2D Benchmarks

    [BenchmarkCategory("Line2D_Operations"), Benchmark(Baseline = true)]
    public double Line2D_DistanceTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _lines[i].DistanceTo(_points[i]);
        return sum;
    }

    [BenchmarkCategory("Line2D_Operations"), Benchmark]
    public Point2D Line2D_ClosestPoint()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _lines[i].ClosestPoint(_points[i]);
        return result;
    }

    [BenchmarkCategory("Line2D_Operations"), Benchmark]
    public Point2D Line2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _lines[i].PointAt(0.5);
        return result;
    }

    [BenchmarkCategory("Line2D_Intersect"), Benchmark(Baseline = true)]
    public Point2D Line2D_Intersect()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _lineA.Intersect(_lineB).point;
        return result;
    }

    [BenchmarkCategory("Line2D_Intersect"), Benchmark]
    public bool Line2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _lines[i].Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("Line2D_Properties"), Benchmark(Baseline = true)]
    public Vector2D Line2D_Direction()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _lines[i].Direction;
        return result;
    }

    [BenchmarkCategory("Line2D_Properties"), Benchmark]
    public double Line2D_Length()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _lines[i].Length;
        return sum;
    }

    #endregion

    #region Segment2D Benchmarks

    [BenchmarkCategory("Segment2D_Operations"), Benchmark(Baseline = true)]
    public double Segment2D_DistanceTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _segments[i].DistanceTo(_points[i]);
        return sum;
    }

    [BenchmarkCategory("Segment2D_Operations"), Benchmark]
    public Point2D Segment2D_ClosestPoint()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].ClosestPoint(_points[i]);
        return result;
    }

    [BenchmarkCategory("Segment2D_Operations"), Benchmark]
    public Point2D Segment2D_Midpoint()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].Midpoint;
        return result;
    }

    [BenchmarkCategory("Segment2D_Operations"), Benchmark]
    public Point2D Segment2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].PointAt(0.5);
        return result;
    }

    [BenchmarkCategory("Segment2D_Intersect"), Benchmark(Baseline = true)]
    public Point2D Segment2D_Intersect()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segA.Intersect(_segB).point;
        return result;
    }

    [BenchmarkCategory("Segment2D_Intersect"), Benchmark]
    public Point2D Segment2D_IntersectLine()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].IntersectLine(_lineA).point;
        return result;
    }

    [BenchmarkCategory("Segment2D_Intersect"), Benchmark]
    public BoundingBox2D Segment2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].ToBoundingBox();
        return result;
    }

    [BenchmarkCategory("Segment2D_Length"), Benchmark(Baseline = true)]
    public double Segment2D_Length()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _segments[i].Length;
        return sum;
    }

    [BenchmarkCategory("Segment2D_Length"), Benchmark]
    public Vector2D Segment2D_Direction()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _segments[i].Direction;
        return result;
    }

    #endregion

    #region Circle2D Benchmarks

    [BenchmarkCategory("Circle2D_Operations"), Benchmark(Baseline = true)]
    public double Circle2D_DistanceTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _circles[i].DistanceTo(_points[i]);
        return sum;
    }

    [BenchmarkCategory("Circle2D_Operations"), Benchmark]
    public bool Circle2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _circles[i].Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("Circle2D_Operations"), Benchmark]
    public Point2D Circle2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _circles[i].PointAt(i * 0.01);
        return result;
    }

    [BenchmarkCategory("Circle2D_Operations"), Benchmark]
    public Vector2D Circle2D_TangentAt()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _circles[i].TangentAt(i * 0.01);
        return result;
    }

    [BenchmarkCategory("Circle2D_Operations"), Benchmark]
    public Vector2D Circle2D_NormalAt()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _circles[i].NormalAt(i * 0.01);
        return result;
    }

    [BenchmarkCategory("Circle2D_Intersect"), Benchmark(Baseline = true)]
    public ImmutableArray<Point2D> Circle2D_IntersectCircle()
    {
        ImmutableArray<Point2D> result = default;
        for (int i = 0; i < Count; i++)
            result = _circleA.Intersect(_circleB).points;
        return result;
    }

    [BenchmarkCategory("Circle2D_Intersect"), Benchmark]
    public ImmutableArray<Point2D> Circle2D_IntersectLine()
    {
        ImmutableArray<Point2D> result = default;
        for (int i = 0; i < Count; i++)
            result = _circles[i].Intersect(_lineA).points;
        return result;
    }

    [BenchmarkCategory("Circle2D_Intersect"), Benchmark]
    public BoundingBox2D Circle2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _circles[i].ToBoundingBox();
        return result;
    }

    [BenchmarkCategory("Circle2D_Measures"), Benchmark(Baseline = true)]
    public double Circle2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _circles[i].Area;
        return sum;
    }

    [BenchmarkCategory("Circle2D_Measures"), Benchmark]
    public double Circle2D_Circumference()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _circles[i].Circumference;
        return sum;
    }

    #endregion

    #region Ellipse2D Benchmarks

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark(Baseline = true)]
    public double Ellipse2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _ellipses[i].Area;
        return sum;
    }

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark]
    public double Ellipse2D_Perimeter()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _ellipses[i].Perimeter();
        return sum;
    }

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark]
    public Point2D Ellipse2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _ellipses[i].PointAt(i * 0.01);
        return result;
    }

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark]
    public Vector2D Ellipse2D_TangentAt()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
            result = _ellipses[i].TangentAt(i * 0.01);
        return result;
    }

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark]
    public bool Ellipse2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _ellipses[i].Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("Ellipse2D_Operations"), Benchmark]
    public BoundingBox2D Ellipse2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _ellipses[i].ToBoundingBox();
        return result;
    }

    #endregion

    #region Triangle2D Benchmarks

    [BenchmarkCategory("Triangle2D_Operations"), Benchmark(Baseline = true)]
    public double Triangle2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _triangles[i].Area;
        return sum;
    }

    [BenchmarkCategory("Triangle2D_Operations"), Benchmark]
    public double Triangle2D_Perimeter()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _triangles[i].Perimeter;
        return sum;
    }

    [BenchmarkCategory("Triangle2D_Operations"), Benchmark]
    public Point2D Triangle2D_Centroid()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].Centroid;
        return result;
    }

    [BenchmarkCategory("Triangle2D_Operations"), Benchmark]
    public Point2D Triangle2D_Incenter()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].Incenter;
        return result;
    }

    [BenchmarkCategory("Triangle2D_Operations"), Benchmark]
    public Point2D Triangle2D_Circumcenter()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].Circumcenter;
        return result;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark(Baseline = true)]
    public double Triangle2D_Circumradius()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _triangles[i].Circumradius;
        return sum;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark]
    public double Triangle2D_Inradius()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _triangles[i].Inradius;
        return sum;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark]
    public bool Triangle2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark]
    public bool Triangle2D_IsDegenerate()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].IsDegenerate();
        return result;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark]
    public Vector2D Triangle2D_BarycentricCoords()
    {
        Vector2D result = default;
        for (int i = 0; i < Count; i++)
        {
            var bc = _triangles[i].BarycentricCoords(_points[i]);
            result = new Vector2D(bc.u, bc.v);
        }
        return result;
    }

    [BenchmarkCategory("Triangle2D_Measures"), Benchmark]
    public BoundingBox2D Triangle2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _triangles[i].ToBoundingBox();
        return result;
    }

    #endregion

    #region Rectangle2D Benchmarks

    [BenchmarkCategory("Rectangle2D_Operations"), Benchmark(Baseline = true)]
    public bool Rectangle2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _rect.Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Operations"), Benchmark]
    public bool Rectangle2D_Intersects()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _rect.Intersects(_rectangles[i]);
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Operations"), Benchmark]
    public Rectangle2D? Rectangle2D_Intersect()
    {
        Rectangle2D? result = default;
        for (int i = 0; i < Count; i++)
            result = _rect.Intersect(_rectangles[i]);
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Operations"), Benchmark]
    public Rectangle2D Rectangle2D_Inflate()
    {
        Rectangle2D result = default;
        for (int i = 0; i < Count; i++)
            result = _rectangles[i].Inflate(1.0);
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Operations"), Benchmark]
    public Rectangle2D Rectangle2D_Translate()
    {
        Rectangle2D result = default;
        for (int i = 0; i < Count; i++)
            result = _rectangles[i].Translate(new Vector2D(1.0, 1.0));
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Measures"), Benchmark(Baseline = true)]
    public double Rectangle2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _rectangles[i].Area;
        return sum;
    }

    [BenchmarkCategory("Rectangle2D_Measures"), Benchmark]
    public double Rectangle2D_Perimeter()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _rectangles[i].Perimeter;
        return sum;
    }

    [BenchmarkCategory("Rectangle2D_Measures"), Benchmark]
    public Point2D Rectangle2D_Center()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _rectangles[i].Center;
        return result;
    }

    [BenchmarkCategory("Rectangle2D_Measures"), Benchmark]
    public double Rectangle2D_Width()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _rectangles[i].Width;
        return sum;
    }

    [BenchmarkCategory("Rectangle2D_Measures"), Benchmark]
    public double Rectangle2D_Height()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _rectangles[i].Height;
        return sum;
    }

    #endregion

    #region Polygon2D Benchmarks

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark(Baseline = true)]
    public double Polygon2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _polygons[i].Area;
        return sum;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public double Polygon2D_Perimeter()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _polygons[i].Perimeter;
        return sum;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public Point2D Polygon2D_Centroid()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].Centroid;
        return result;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public bool Polygon2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].Contains(_points[i % _points.Length]);
        return result;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public bool Polygon2D_IsConvex()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].IsConvex;
        return result;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public bool Polygon2D_IsSimple()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].IsSimple;
        return result;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public ImmutableArray<Segment2D> Polygon2D_Edges()
    {
        ImmutableArray<Segment2D> result = default;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].Edges.ToImmutableArray();
        return result;
    }

    [BenchmarkCategory("Polygon2D_Operations"), Benchmark]
    public BoundingBox2D Polygon2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _polygons[i].ToBoundingBox();
        return result;
    }

    #endregion

    #region BoundingBox2D Benchmarks

    [BenchmarkCategory("BoundingBox2D_Operations"), Benchmark(Baseline = true)]
    public bool BoundingBox2D_Contains()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _boxA.Contains(_points[i]);
        return result;
    }

    [BenchmarkCategory("BoundingBox2D_Operations"), Benchmark]
    public bool BoundingBox2D_Intersects()
    {
        bool result = false;
        for (int i = 0; i < Count; i++)
            result = _boxA.Intersects(_boxes[i]);
        return result;
    }

    [BenchmarkCategory("BoundingBox2D_Operations"), Benchmark]
    public BoundingBox2D BoundingBox2D_Union()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _boxA.Union(_boxes[i]);
        return result;
    }

    [BenchmarkCategory("BoundingBox2D_Operations"), Benchmark]
    public BoundingBox2D BoundingBox2D_Inflate()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _boxes[i].Inflate(1.0);
        return result;
    }

    [BenchmarkCategory("BoundingBox2D_Measures"), Benchmark(Baseline = true)]
    public double BoundingBox2D_Area()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _boxes[i].Area;
        return sum;
    }

    [BenchmarkCategory("BoundingBox2D_Measures"), Benchmark]
    public double BoundingBox2D_Width()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _boxes[i].Width;
        return sum;
    }

    [BenchmarkCategory("BoundingBox2D_Measures"), Benchmark]
    public double BoundingBox2D_Height()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _boxes[i].Height;
        return sum;
    }

    [BenchmarkCategory("BoundingBox2D_Measures"), Benchmark]
    public Point2D BoundingBox2D_Center()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _boxes[i].Center;
        return result;
    }

    [BenchmarkCategory("BoundingBox2D_FromPoints"), Benchmark(Baseline = true)]
    public BoundingBox2D BoundingBox2D_FromPoints()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = BoundingBox2D.FromPoints(_points);
        return result;
    }

    #endregion

    #region Ray2D Benchmarks

    [BenchmarkCategory("Ray2D_Operations"), Benchmark(Baseline = true)]
    public Point2D Ray2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _rays[i].PointAt(5.0);
        return result;
    }

    [BenchmarkCategory("Ray2D_Operations"), Benchmark]
    public double Ray2D_IntersectLine()
    {
        double result = default;
        for (int i = 0; i < Count; i++)
            result = _rays[i].Intersect(_lineA).t;
        return result;
    }

    [BenchmarkCategory("Ray2D_Operations"), Benchmark]
    public ImmutableArray<Point2D> Ray2D_IntersectCircle()
    {
        ImmutableArray<Point2D> result = default;
        for (int i = 0; i < Count; i++)
            result = _rays[i].Intersect(_circleA).points;
        return result;
    }

    [BenchmarkCategory("Ray2D_Operations"), Benchmark]
    public double Ray2D_DistanceTo()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _rays[i].DistanceTo(_points[i]);
        return sum;
    }

    #endregion

    #region Arc2D Benchmarks

    [BenchmarkCategory("Arc2D_Operations"), Benchmark(Baseline = true)]
    public double Arc2D_Length()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += _arcs[i].Length;
        return sum;
    }

    [BenchmarkCategory("Arc2D_Operations"), Benchmark]
    public Point2D Arc2D_PointAt()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _arcs[i].PointAt(0.5);
        return result;
    }

    [BenchmarkCategory("Arc2D_Operations"), Benchmark]
    public BoundingBox2D Arc2D_ToBoundingBox()
    {
        BoundingBox2D result = default;
        for (int i = 0; i < Count; i++)
            result = _arcs[i].ToBoundingBox();
        return result;
    }

    #endregion

    #region Geometry2DOperations Static Benchmarks

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark(Baseline = true)]
    public double Geometry2DOperations_Distance()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += Geometry2DOperations.Distance(_pointA, _points[i]);
        return sum;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public Point2D Geometry2DOperations_Project()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = Geometry2DOperations.Project(_points[i], _lineA);
        return result;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public double Geometry2DOperations_Area_Triangle()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += Geometry2DOperations.Area(_triangles[i]);
        return sum;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public double Geometry2DOperations_Perimeter_Polygon()
    {
        double sum = 0;
        for (int i = 0; i < Count; i++)
            sum += Geometry2DOperations.Perimeter(_polygons[i]);
        return sum;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public Point2D Geometry2DOperations_Centroid_Polygon()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = Geometry2DOperations.Centroid(_polygons[i]);
        return result;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public Polygon2D Geometry2DOperations_ConvexHull()
    {
        Polygon2D result = default;
        for (int i = 0; i < 100; i++)
            result = Geometry2DOperations.ConvexHull(_points);
        return result;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public Point2D Geometry2DOperations_Translate()
    {
        Point2D result = default;
        for (int i = 0; i < Count; i++)
            result = _points[i].Translate(_vecA);
        return result;
    }

    [BenchmarkCategory("Geometry2DOperations_Distance"), Benchmark]
    public ImmutableArray<Point2D> Geometry2DOperations_Clip()
    {
        ImmutableArray<Point2D> result = default;
        for (int i = 0; i < Count; i++)
            result = Geometry2DOperations.Clip(_lines[i], _polygons[0]);
        return result;
    }

    #endregion
}
