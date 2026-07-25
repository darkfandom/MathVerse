using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Geometry.Curves;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Transformations;
using System;
using System.Numerics;
using Vector2D = MathVerse.Math.Geometry.Geometry2D.Vector2D;

namespace MathVerse.Performance.Tests.Geometry;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class TransformCurveBenchmarks
{
    // ── Transform2D fields ──
    private Transform2D _t2dA;
    private Transform2D _t2dB;
    private Point2D _point2D;
    private Vector2D _vector2D;
    private Vector2D _reflectionAxis2D;

    // ── Transform3D fields ──
    private Transform3D _t3dA;
    private Transform3D _t3dB;
    private Point3D _point3D;
    private Vector3D _vector3D;
    private Vector3D _reflectionAxis3D;
    private Point3D _eye;
    private Point3D _target;
    private Vector3D _up;
    private double[][] _rowMajorMatrix = null!;
    private Matrix4x4 _systemMatrix;
    private Transform3D _invertible3D;

    // ── Quaternion fields ──
    private QuaternionRotation _quatA;
    private QuaternionRotation _quatB;
    private Vector3D _quatAxis;
    private double _quatAngle;

    // ── Bezier2D fields ──
    private BezierCurve2D _bezier2D_Cubic;
    private BezierCurve2D _bezier2D_Sextic;

    // ── Bezier3D fields ──
    private BezierCurve3D _bezier3D_Cubic;
    private BezierCurve3D _bezier3D_Sextic;

    // ── Parametric2D fields ──
    private ParametricCurve2D _param2D = null!;

    // ── Parametric3D fields ──
    private ParametricCurve3D _param3D = null!;

    // ── Implicit2D fields ──
    private ImplicitCurve2D _implicit2D = null!;

    // ── Hermite fields ──
    private HermiteCurve _hermite;

    // ── CatmullRom fields ──
    private CatmullRomCurve _catmullRom = null!;

    // ── BSpline fields ──
    private BSplineCurve _bSpline = null!;

    [GlobalSetup]
    public void Setup()
    {
        double angle = 0.5;
        double cos = System.Math.Cos(angle);
        double sin = System.Math.Sin(angle);

        _t2dA = Transform2D.Translation(1.0, 2.0).Multiply(Transform2D.Rotation(angle));
        _t2dB = Transform2D.Scaling(1.5, 0.75).Multiply(Transform2D.Shearing(0.1, 0.2));
        _point2D = new Point2D(3.0, 4.0);
        _vector2D = new Vector2D(1.0, 1.0);
        _reflectionAxis2D = new Vector2D(1.0, 1.0);

        _t3dA = Transform3D.Translation(1.0, 2.0, 3.0)
            .Multiply(Transform3D.RotationX(angle))
            .Multiply(Transform3D.RotationY(angle * 0.7))
            .Multiply(Transform3D.RotationZ(angle * 0.3));
        _t3dB = Transform3D.Scaling(1.5, 0.75, 2.0)
            .Multiply(Transform3D.Shearing(0.1, 0.05, 0.2, 0.03, 0.07, 0.11));
        _point3D = new Point3D(3.0, 4.0, 5.0);
        _vector3D = new Vector3D(1.0, 2.0, 3.0);
        _reflectionAxis3D = new Vector3D(1.0, 1.0, 1.0);
        _eye = new Point3D(0.0, 0.0, 5.0);
        _target = new Point3D(0.0, 0.0, 0.0);
        _up = new Vector3D(0.0, 1.0, 0.0);
        _rowMajorMatrix = new double[4][]
        {
            new double[] { 1.0, 0.0, 0.0, 5.0 },
            new double[] { 0.0, 1.0, 0.0, -3.0 },
            new double[] { 0.0, 0.0, 1.0, 2.0 },
            new double[] { 0.0, 0.0, 0.0, 1.0 }
        };
        _systemMatrix = Matrix4x4.CreateRotationY((float)angle) * Matrix4x4.CreateTranslation(1, 2, 3);
        _invertible3D = Transform3D.Translation(1, 2, 3)
            .Multiply(Transform3D.RotationX(0.5))
            .Multiply(Transform3D.Scaling(2.0, 0.5, 1.5));

        _quatA = QuaternionRotation.FromAxisAngle(new Vector3D(0, 1, 0), angle);
        _quatB = QuaternionRotation.FromAxisAngle(new Vector3D(1, 0, 0), angle * 0.6);
        _quatAxis = new Vector3D(1.0, 1.0, 0.0);
        _quatAngle = 0.75;

        _bezier2D_Cubic = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 3), new Point2D(2, -1), new Point2D(3, 0)));
        _bezier2D_Sextic = new BezierCurve2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 4), new Point2D(2, -2),
            new Point2D(3, 5), new Point2D(4, -1), new Point2D(5, 3), new Point2D(6, 0)));

        _bezier3D_Cubic = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(0, 0, 0), new Point3D(1, 3, 1), new Point3D(2, -1, 2), new Point3D(3, 0, -1)));
        _bezier3D_Sextic = new BezierCurve3D(ImmutableArray.Create(
            new Point3D(0, 0, 0), new Point3D(1, 4, 1), new Point3D(2, -2, 3),
            new Point3D(3, 5, -1), new Point3D(4, -1, 2), new Point3D(5, 3, 0), new Point3D(6, 0, 1)));

        _param2D = new ParametricCurve2D(
            (ParametricCurveFunc2D)(t => new Point2D(System.Math.Cos(t), System.Math.Sin(t))),
            0.0,
            2.0 * System.Math.PI);

        _param3D = new ParametricCurve3D(
            (ParametricCurveFunc3D)(t => new Point3D(System.Math.Cos(t), System.Math.Sin(t), t / (2.0 * System.Math.PI))),
            0.0,
            2.0 * System.Math.PI);

        _implicit2D = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);

        _hermite = new HermiteCurve(
            new Point3D(0, 0, 0),
            new Vector3D(1, 0, 0),
            new Point3D(3, 4, 1),
            new Vector3D(-1, 2, 0));

        _catmullRom = new CatmullRomCurve(ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(1, 3, 1),
            new Point3D(2, -1, 2),
            new Point3D(3, 4, -1),
            new Point3D(5, 2, 3),
            new Point3D(6, 0, 0)), 0.5);

        var bsKnots = ImmutableArray.Create(
            0.0, 0.0, 0.0, 0.0,
            1.0, 2.0, 3.0,
            4.0, 4.0, 4.0, 4.0);
        var bsPoints = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(1, 2, 1),
            new Point3D(2, -1, 3),
            new Point3D(3, 4, 0),
            new Point3D(4, 1, 2),
            new Point3D(5, 3, -1),
            new Point3D(6, 0, 1));
        _bSpline = new BSplineCurve(bsKnots, bsPoints, 3);
    }

    // ───────────────────────────────────────────────────────────
    //  Transform2D
    // ───────────────────────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Point2D Transform2D_TransformPoint() => _t2dA.TransformPoint(_point2D);

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Vector2D Transform2D_TransformVector() => _t2dA.TransformVector(_vector2D);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Transform2D")]
    public Transform2D Transform2D_Multiply() => _t2dA.Multiply(_t2dB);

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Transform2D Transform2D_Inverse() => _t2dA.Inverse();

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public double Transform2D_Determinant() => _t2dA.Determinant();

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Transform2D Transform2D_Compose() => _t2dA.Compose(_t2dB);

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Transform2D Transform2D_OperatorMultiply() => _t2dA * _t2dB;

    [Benchmark]
    [BenchmarkCategory("Transform2D")]
    public Point2D Transform2D_OperatorMultiplyPoint() => _t2dA * _point2D;

    // ───────────────────────────────────────────────────────────
    //  Transform2D_Creation
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateTranslation() => Transform2D.Translation(1.5, 2.5);

    [Benchmark]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateRotation() => Transform2D.Rotation(0.75);

    [Benchmark]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateScalingNonUniform() => Transform2D.Scaling(1.5, 0.75);

    [Benchmark]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateScalingUniform() => Transform2D.Scaling(2.0);

    [Benchmark]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateReflection() => Transform2D.Reflection(_reflectionAxis2D);

    [Benchmark]
    [BenchmarkCategory("Transform2D_Creation")]
    public Transform2D Transform2D_CreateShearing() => Transform2D.Shearing(0.3, 0.1);

    // ───────────────────────────────────────────────────────────
    //  Transform3D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Transform3D")]
    public Point3D Transform3D_TransformPoint() => _t3dA.TransformPoint(_point3D);

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Vector3D Transform3D_TransformVector() => _t3dA.TransformVector(_vector3D);

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Vector3D Transform3D_TransformNormal() => _t3dA.TransformNormal(_vector3D);

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Transform3D Transform3D_Transpose() => _t3dA.Transpose();

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Transform3D Transform3D_Multiply() => _t3dA.Multiply(_t3dB);

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Transform3D Transform3D_Inverse() => _invertible3D.Inverse();

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public double Transform3D_Determinant() => _t3dA.Determinant();

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Matrix4x4 Transform3D_ToSystemNumerics() => _t3dA.ToSystemNumerics();

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Transform3D Transform3D_InverseTranspose3x3() => _invertible3D.InverseTranspose3x3();

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Transform3D Transform3D_OperatorMultiply() => _t3dA * _t3dB;

    [Benchmark]
    [BenchmarkCategory("Transform3D")]
    public Point3D Transform3D_OperatorMultiplyPoint() => _t3dA * _point3D;

    // ───────────────────────────────────────────────────────────
    //  Transform3D_Creation
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateTranslation() => Transform3D.Translation(1.5, 2.5, 3.5);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateRotationX() => Transform3D.RotationX(0.75);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateRotationY() => Transform3D.RotationY(0.75);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateRotationZ() => Transform3D.RotationZ(0.75);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateRotationAxis() => Transform3D.RotationAxis(_reflectionAxis3D, 0.75);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateRotationEuler() => Transform3D.RotationEuler(0.5, 0.3, 0.1);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateScalingNonUniform() => Transform3D.Scaling(1.5, 0.75, 2.0);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateScalingUniform() => Transform3D.Scaling(2.0);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateReflection() => Transform3D.Reflection(_reflectionAxis3D);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateShearing() => Transform3D.Shearing(0.1, 0.05, 0.2, 0.03, 0.07, 0.11);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateLookAt() => Transform3D.LookAt(_eye, _target, _up);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateFromRowMajor() => Transform3D.FromRowMajor(_rowMajorMatrix);

    [Benchmark]
    [BenchmarkCategory("Transform3D_Creation")]
    public Transform3D Transform3D_CreateFromSystemNumerics() => Transform3D.FromSystemNumerics(_systemMatrix);

    // ───────────────────────────────────────────────────────────
    //  Quaternion
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_FromAxisAngle() => QuaternionRotation.FromAxisAngle(_quatAxis, _quatAngle);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_FromEuler() => QuaternionRotation.FromEuler(0.5, 0.3, 0.1);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_FromRotationMatrix() => QuaternionRotation.FromRotationMatrix(_t3dA);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_Normalize() => _quatA.Normalize();

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_Conjugate() => _quatA.Conjugate();

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_Inverse() => _quatA.Inverse();

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public Vector3D Quaternion_Rotate() => _quatA.Rotate(_vector3D);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public Transform3D Quaternion_ToTransform() => _quatA.ToTransform();

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_Multiply() => _quatA.Multiply(_quatB);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_Slerp() => _quatA.Slerp(_quatB, 0.5);

    [Benchmark]
    [BenchmarkCategory("Quaternion")]
    public QuaternionRotation Quaternion_OperatorMultiply() => _quatA * _quatB;

    // ───────────────────────────────────────────────────────────
    //  Bezier2D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Bezier2D")]
    public Point2D Bezier2D_PointAt_Cubic() => _bezier2D_Cubic.PointAt(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public Point2D Bezier2D_PointAt_Sextic() => _bezier2D_Sextic.PointAt(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public Vector2D Bezier2D_Derivative_Cubic() => _bezier2D_Cubic.Derivative(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public Vector2D Bezier2D_Derivative_Sextic() => _bezier2D_Sextic.Derivative(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public ImmutableArray<Point2D> Bezier2D_Sample10_Cubic() => _bezier2D_Cubic.Sample(10);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public ImmutableArray<Point2D> Bezier2D_Sample100_Cubic() => _bezier2D_Cubic.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public ImmutableArray<Point2D> Bezier2D_Sample100_Sextic() => _bezier2D_Sextic.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Bezier2D")]
    public BezierCurve2D Bezier2D_HermiteToBezier() =>
        BezierCurve2D.HermiteToBezier(new Point2D(0, 0), new Vector2D(1, 0), new Point2D(3, 4), new Vector2D(-1, 2));

    // ───────────────────────────────────────────────────────────
    //  Bezier3D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Bezier3D")]
    public Point3D Bezier3D_PointAt_Cubic() => _bezier3D_Cubic.PointAt(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public Point3D Bezier3D_PointAt_Sextic() => _bezier3D_Sextic.PointAt(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public Vector3D Bezier3D_Derivative_Cubic() => _bezier3D_Cubic.Derivative(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public Vector3D Bezier3D_Derivative_Sextic() => _bezier3D_Sextic.Derivative(0.5);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public ImmutableArray<Point3D> Bezier3D_Sample10_Cubic() => _bezier3D_Cubic.Sample(10);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public ImmutableArray<Point3D> Bezier3D_Sample100_Cubic() => _bezier3D_Cubic.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public ImmutableArray<Point3D> Bezier3D_Sample100_Sextic() => _bezier3D_Sextic.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Bezier3D")]
    public BezierCurve3D Bezier3D_HermiteToBezier() =>
        BezierCurve3D.HermiteToBezier(new Point3D(0, 0, 0), new Vector3D(1, 0, 0), new Point3D(3, 4, 1), new Vector3D(-1, 2, 0));

    // ───────────────────────────────────────────────────────────
    //  Parametric2D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Parametric2D")]
    public Point2D Parametric2D_Evaluate() => _param2D.Evaluate(1.5);

    [Benchmark]
    [BenchmarkCategory("Parametric2D")]
    public IReadOnlyList<Point2D> Parametric2D_Sample10() => _param2D.Sample(10);

    [Benchmark]
    [BenchmarkCategory("Parametric2D")]
    public IReadOnlyList<Point2D> Parametric2D_Sample100() => _param2D.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Parametric2D")]
    public Vector2D Parametric2D_Tangent() => _param2D.Tangent(1.5);

    [Benchmark]
    [BenchmarkCategory("Parametric2D")]
    public double Parametric2D_Curvature() => _param2D.Curvature(1.5);

    // ───────────────────────────────────────────────────────────
    //  Parametric3D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Parametric3D")]
    public Point3D Parametric3D_Evaluate() => _param3D.Evaluate(1.5);

    [Benchmark]
    [BenchmarkCategory("Parametric3D")]
    public IReadOnlyList<Point3D> Parametric3D_Sample10() => _param3D.Sample(10);

    [Benchmark]
    [BenchmarkCategory("Parametric3D")]
    public IReadOnlyList<Point3D> Parametric3D_Sample100() => _param3D.Sample(100);

    [Benchmark]
    [BenchmarkCategory("Parametric3D")]
    public Vector3D Parametric3D_Tangent() => _param3D.Tangent(1.5);

    // ───────────────────────────────────────────────────────────
    //  Implicit2D
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Implicit2D")]
    public double Implicit2D_Evaluate() => _implicit2D.Evaluate(0.5, 0.5);

    [Benchmark]
    [BenchmarkCategory("Implicit2D")]
    public ImmutableArray<Segment2D> Implicit2D_Contour50() => _implicit2D.Contour(-2, 2, -2, 2, 50);

    [Benchmark]
    [BenchmarkCategory("Implicit2D")]
    public ImmutableArray<Segment2D> Implicit2D_Contour200() => _implicit2D.Contour(-2, 2, -2, 2, 200);

    // ───────────────────────────────────────────────────────────
    //  Hermite
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Hermite")]
    public Point3D Hermite_PointAt() => _hermite.PointAt(0.5);

    [Benchmark]
    [BenchmarkCategory("Hermite")]
    public BezierCurve3D Hermite_ToBezier() => _hermite.ToBezier();

    // ───────────────────────────────────────────────────────────
    //  CatmullRom
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CatmullRom")]
    public Point3D CatmullRom_PointAt() => _catmullRom.PointAt(2.5);

    [Benchmark]
    [BenchmarkCategory("CatmullRom")]
    public IReadOnlyList<Point3D> CatmullRom_Sample10() => _catmullRom.Sample(10);

    [Benchmark]
    [BenchmarkCategory("CatmullRom")]
    public IReadOnlyList<Point3D> CatmullRom_Sample100() => _catmullRom.Sample(100);

    // ───────────────────────────────────────────────────────────
    //  BSpline
    // ───────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BSpline")]
    public Point3D BSpline_PointAt() => _bSpline.PointAt(2.0);

    [Benchmark]
    [BenchmarkCategory("BSpline")]
    public IReadOnlyList<Point3D> BSpline_Sample10() => _bSpline.Sample(10);

    [Benchmark]
    [BenchmarkCategory("BSpline")]
    public IReadOnlyList<Point3D> BSpline_Sample100() => _bSpline.Sample(100);

    [Benchmark]
    [BenchmarkCategory("BSpline")]
    public BSplineCurve BSpline_InsertKnot() => _bSpline.InsertKnot(1.5);

    [Benchmark]
    [BenchmarkCategory("BSpline")]
    public BSplineCurve BSpline_Derivative() => _bSpline.Derivative();
}
