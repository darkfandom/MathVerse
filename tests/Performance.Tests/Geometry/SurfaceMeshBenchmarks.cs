using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Geometry.Surfaces;
using MathVerse.Math.Geometry.Meshes;
using MeshTriangleMesh = MathVerse.Math.Geometry.Mesh.TriangleMesh;
using MathVerse.Math.Geometry.Tessellation;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Transformations;
using MathVerse.Math.Geometry;

namespace MathVerse.Performance.Tests.Geometry;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SurfaceMeshBenchmarks
{
    private BezierSurface _bezier = null!;
    private BSplineSurface _bspline = null!;
    private ParametricSurface _parametricSphere = null!;
    private ImplicitSurface _implicitSphere = null!;
    private HeightMap _heightMap = null!;

    private TriangleMesh _smallMesh = null!;
    private TriangleMesh _mediumMesh = null!;
    private QuadMesh _smallQuadMesh = null!;

    private MeshBuilder _builderForGet = null!;
    private MeshBuilder _builderForSet = null!;
    private MeshBuilder _builderForClear = null!;

    private Vertex _vertexA;
    private Vertex _vertexB;
    private TriangleFace _triFace;
    private QuadFace _quadFace;
    private Edge _edge;

    private Transform3D _transform;

    private IReadOnlyList<Point2D> _poly2D3 = null!;
    private IReadOnlyList<Point2D> _poly2D4 = null!;
    private IReadOnlyList<Point2D> _poly2D5 = null!;
    private IReadOnlyList<Point2D> _poly2D8 = null!;
    private IReadOnlyList<Point2D> _poly2D16 = null!;
    private IReadOnlyList<Point3D> _poly3D4 = null!;
    private IReadOnlyList<Point3D> _poly3D8 = null!;

    private IReadOnlyList<Point2D> _polyline2D = null!;
    private IReadOnlyList<Point3D> _polyline3D = null!;

    private Sphere3D _sphere;
    private Cylinder3D _cylinder;

    [GlobalSetup]
    public void Setup()
    {
        var cp = CreateBezierControlPoints();
        _bezier = new BezierSurface(cp);

        var knots = ImmutableArray.Create(0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0);
        _bspline = new BSplineSurface(knots, knots, cp, 3, 3);

        _parametricSphere = new ParametricSurface(
            (u, v) => new Point3D(
                System.Math.Sin(v) * System.Math.Cos(u),
                System.Math.Sin(v) * System.Math.Sin(u),
                System.Math.Cos(v)),
            0.0, 2.0 * System.Math.PI, 0.0, System.Math.PI);

        _implicitSphere = new ImplicitSurface((x, y, z) => x * x + y * y + z * z - 1.0);

        var heights = new double[21, 21];
        for (int i = 0; i < 21; i++)
            for (int j = 0; j < 21; j++)
                heights[i, j] = System.Math.Sin(i * 0.3) * System.Math.Cos(j * 0.3);
        _heightMap = new HeightMap(heights, -3.0, 3.0, -3.0, 3.0);

        _smallMesh = BuildTriangleMesh(10);
        _mediumMesh = BuildTriangleMesh(30);
        _smallQuadMesh = BuildQuadMesh(10);

        _builderForGet = new MeshBuilder();
        for (int i = 0; i < 100; i++)
            _builderForGet.AddVertex(new Point3D(i * 0.1, 0, 0));

        _builderForSet = new MeshBuilder();
        for (int i = 0; i < 100; i++)
            _builderForSet.AddVertex(new Point3D(i * 0.1, 0, 0));

        _builderForClear = new MeshBuilder();
        for (int i = 0; i < 100; i++)
        {
            _builderForClear.AddVertex(new Point3D(i * 0.1, 0, 0));
            if (i >= 2)
                _builderForClear.AddTriangle(i, i - 1, i - 2);
        }

        _vertexA = new Vertex(new Point3D(0, 0, 0), Vector3D.UnitX, (0.0, 0.0));
        _vertexB = new Vertex(new Point3D(1, 1, 1), Vector3D.UnitY, (1.0, 1.0));
        _triFace = new TriangleFace(0, 1, 2);
        _quadFace = new QuadFace(0, 1, 2, 3);
        _edge = new Edge(0, 5);

        _transform = Transform3D.RotationZ(0.5) * Transform3D.Translation(1, 2, 3);

        _poly2D3 = new List<Point2D> { new(0, 0), new(1, 0), new(0.5, 1) };
        _poly2D4 = new List<Point2D> { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        _poly2D5 = CreateRegularPolygon(5, 1.0);
        _poly2D8 = CreateRegularPolygon(8, 1.0);
        _poly2D16 = CreateRegularPolygon(16, 1.0);
        _poly3D4 = new List<Point3D>
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        _poly3D8 = CreateRegularPolygon3D(8, 1.0);

        _polyline2D = CreatePolyline2D(20);
        _polyline3D = CreatePolyline3D(20);

        _sphere = new Sphere3D(Point3D.Origin, 1.0);
        _cylinder = new Cylinder3D(Point3D.Origin, 1.0, 2.0);
    }

    #region Helpers

    private static ImmutableArray<ImmutableArray<Point3D>> CreateBezierControlPoints()
    {
        var rows = ImmutableArray.CreateBuilder<ImmutableArray<Point3D>>(4);
        for (int j = 0; j < 4; j++)
        {
            var row = ImmutableArray.CreateBuilder<Point3D>(4);
            for (int i = 0; i < 4; i++)
                row.Add(new Point3D(
                    (double)i / 3.0,
                    (double)j / 3.0,
                    System.Math.Sin((double)i / 3.0 * System.Math.PI) * System.Math.Sin((double)j / 3.0 * System.Math.PI)));
            rows.Add(row.MoveToImmutable());
        }
        return rows.MoveToImmutable();
    }

    private static TriangleMesh BuildTriangleMesh(int gridSize)
    {
        var builder = new MeshBuilder();
        int n = gridSize + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                builder.AddVertex(new Point3D(
                    i * 0.1, j * 0.1,
                    System.Math.Sin(i * 0.1) * System.Math.Cos(j * 0.1)));
        for (int j = 0; j < gridSize; j++)
            for (int i = 0; i < gridSize; i++)
            {
                int tl = j * n + i;
                builder.AddTriangle(tl, tl + 1, tl + n);
                builder.AddTriangle(tl + 1, tl + n + 1, tl + n);
            }
        return builder.Build();
    }

    private static QuadMesh BuildQuadMesh(int gridSize)
    {
        var builder = new MeshBuilder();
        int n = gridSize + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                builder.AddVertex(new Point3D(
                    i * 0.1, j * 0.1,
                    System.Math.Sin(i * 0.1) * System.Math.Cos(j * 0.1)));
        for (int j = 0; j < gridSize; j++)
            for (int i = 0; i < gridSize; i++)
            {
                int tl = j * n + i;
                builder.AddQuad(tl, tl + 1, tl + n + 1, tl + n);
            }
        return builder.BuildQuadMesh();
    }

    private static IReadOnlyList<Point2D> CreateRegularPolygon(int sides, double radius)
    {
        var pts = new List<Point2D>(sides);
        for (int i = 0; i < sides; i++)
        {
            double a = 2.0 * System.Math.PI * i / sides;
            pts.Add(new Point2D(radius * System.Math.Cos(a), radius * System.Math.Sin(a)));
        }
        return pts;
    }

    private static IReadOnlyList<Point3D> CreateRegularPolygon3D(int sides, double radius)
    {
        var pts = new List<Point3D>(sides);
        for (int i = 0; i < sides; i++)
        {
            double a = 2.0 * System.Math.PI * i / sides;
            pts.Add(new Point3D(radius * System.Math.Cos(a), radius * System.Math.Sin(a), 0));
        }
        return pts;
    }

    private static IReadOnlyList<Point2D> CreatePolyline2D(int count)
    {
        var pts = new List<Point2D>(count);
        for (int i = 0; i < count; i++)
            pts.Add(new Point2D(i * 0.5, System.Math.Sin(i * 0.3)));
        return pts;
    }

    private static IReadOnlyList<Point3D> CreatePolyline3D(int count)
    {
        var pts = new List<Point3D>(count);
        for (int i = 0; i < count; i++)
            pts.Add(new Point3D(i * 0.5, System.Math.Sin(i * 0.3), System.Math.Cos(i * 0.2)));
        return pts;
    }

    #endregion

    #region Surfaces - BezierSurface

    [BenchmarkCategory("Surfaces"), Benchmark(Baseline = true)]
    public Point3D BezierSurface_PointAt() => _bezier.PointAt(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D BezierSurface_Normal() => _bezier.Normal(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BezierSurface_Sample_5x5() => _bezier.Sample(5, 5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BezierSurface_Sample_10x10() => _bezier.Sample(10, 10);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BezierSurface_Sample_20x20() => _bezier.Sample(20, 20);

    #endregion

    #region Surfaces - BSplineSurface

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Point3D BSplineSurface_PointAt() => _bspline.PointAt(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D BSplineSurface_Normal() => _bspline.Normal(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BSplineSurface_Sample_5x5() => _bspline.Sample(5, 5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BSplineSurface_Sample_10x10() => _bspline.Sample(10, 10);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> BSplineSurface_Sample_20x20() => _bspline.Sample(20, 20);

    #endregion

    #region Surfaces - ParametricSurface

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Point3D ParametricSurface_Evaluate() => _parametricSphere.Evaluate(1.0, 1.0);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D ParametricSurface_Normal() => _parametricSphere.Normal(1.0, 1.0);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> ParametricSurface_Sample_10x10() => _parametricSphere.Sample(10, 10);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> ParametricSurface_Sample_20x20() => _parametricSphere.Sample(20, 20);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D ParametricSurface_TangentU() => _parametricSphere.TangentU(1.0, 1.0);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D ParametricSurface_TangentV() => _parametricSphere.TangentV(1.0, 1.0);

    #endregion

    #region Surfaces - ImplicitSurface

    [BenchmarkCategory("Surfaces"), Benchmark]
    public double ImplicitSurface_Evaluate() => _implicitSphere.Evaluate(0.5, 0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public MeshTriangleMesh ImplicitSurface_MarchingCubes_10() =>
        _implicitSphere.MarchingCubes(-2, 2, -2, 2, -2, 2, 10, 0.0);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public MeshTriangleMesh ImplicitSurface_MarchingCubes_20() =>
        _implicitSphere.MarchingCubes(-2, 2, -2, 2, -2, 2, 20, 0.0);

    #endregion

    #region Surfaces - HeightMap

    [BenchmarkCategory("Surfaces"), Benchmark]
    public double HeightMap_Evaluate() => _heightMap.Evaluate(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public Vector3D HeightMap_Normal() => _heightMap.Normal(0.5, 0.5);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public double HeightMap_Min() => _heightMap.Min;

    [BenchmarkCategory("Surfaces"), Benchmark]
    public double HeightMap_Max() => _heightMap.Max;

    [BenchmarkCategory("Surfaces"), Benchmark]
    public MeshTriangleMesh HeightMap_ToMesh_10() => _heightMap.ToMesh(10);

    [BenchmarkCategory("Surfaces"), Benchmark]
    public MeshTriangleMesh HeightMap_ToMesh_20() => _heightMap.ToMesh(20);

    #endregion

    #region Meshes - MeshBuilder

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh MeshBuilder_Build_Small()
    {
        var b = new MeshBuilder();
        int gs = 10, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddTriangle(tl, tl + 1, tl + n);
                b.AddTriangle(tl + 1, tl + n + 1, tl + n);
            }
        return b.Build();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh MeshBuilder_Build_Medium()
    {
        var b = new MeshBuilder();
        int gs = 30, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddTriangle(tl, tl + 1, tl + n);
                b.AddTriangle(tl + 1, tl + n + 1, tl + n);
            }
        return b.Build();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh MeshBuilder_Build_Large()
    {
        var b = new MeshBuilder();
        int gs = 50, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddTriangle(tl, tl + 1, tl + n);
                b.AddTriangle(tl + 1, tl + n + 1, tl + n);
            }
        return b.Build();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public QuadMesh MeshBuilder_BuildQuadMesh_Small()
    {
        var b = new MeshBuilder();
        int gs = 10, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddQuad(tl, tl + 1, tl + n + 1, tl + n);
            }
        return b.BuildQuadMesh();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public QuadMesh MeshBuilder_BuildQuadMesh_Medium()
    {
        var b = new MeshBuilder();
        int gs = 30, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddQuad(tl, tl + 1, tl + n + 1, tl + n);
            }
        return b.BuildQuadMesh();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public QuadMesh MeshBuilder_BuildQuadMesh_Large()
    {
        var b = new MeshBuilder();
        int gs = 50, n = gs + 1;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                b.AddVertex(new Point3D(i * 0.1, j * 0.1, 0));
        for (int j = 0; j < gs; j++)
            for (int i = 0; i < gs; i++)
            {
                int tl = j * n + i;
                b.AddQuad(tl, tl + 1, tl + n + 1, tl + n);
            }
        return b.BuildQuadMesh();
    }

    [BenchmarkCategory("Meshes"), Benchmark]
    public Vertex MeshBuilder_GetVertex() => _builderForGet.GetVertex(50);

    [BenchmarkCategory("Meshes"), Benchmark]
    public void MeshBuilder_SetVertexNormal() =>
        _builderForSet.SetVertexNormal(50, new Vector3D(1, 1, 1).Normalize());

    [BenchmarkCategory("Meshes"), Benchmark]
    public void MeshBuilder_SetVertexUV() => _builderForSet.SetVertexUV(50, 0.5, 0.5);

    [BenchmarkCategory("Meshes"), Benchmark]
    public void MeshBuilder_Clear() => _builderForClear.Clear();

    #endregion

    #region Meshes - Record Types

    [BenchmarkCategory("Meshes"), Benchmark]
    public Vertex Vertex_Lerp() => _vertexA.Lerp(_vertexB, 0.5);

    [BenchmarkCategory("Meshes"), Benchmark]
    public (Edge, Edge, Edge) TriangleFace_Edges() => _triFace.Edges;

    [BenchmarkCategory("Meshes"), Benchmark]
    public int[] TriangleFace_Indices() => _triFace.Indices;

    [BenchmarkCategory("Meshes"), Benchmark]
    public (TriangleFace, TriangleFace) QuadFace_Triangulate() => _quadFace.Triangulate();

    [BenchmarkCategory("Meshes"), Benchmark]
    public Edge Edge_Reversed() => _edge.Reversed();

    [BenchmarkCategory("Meshes"), Benchmark]
    public Edge Edge_Canonical() => _edge.Canonical();

    #endregion

    #region Meshes - TriangleMesh Operations

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<Edge> TriangleMesh_GetEdges() => _mediumMesh.GetEdges();

    [BenchmarkCategory("Meshes"), Benchmark]
    public BoundingBox3D TriangleMesh_BoundingBox() => _mediumMesh.BoundingBox();

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh TriangleMesh_CalculateNormals() => _mediumMesh.CalculateNormals();

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh TriangleMesh_Transform() => _mediumMesh.Transform(_transform);

    [BenchmarkCategory("Meshes"), Benchmark]
    public GeometryResult TriangleMesh_Validate() => _mediumMesh.Validate();

    #endregion

    #region Meshes - QuadMesh Operations

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh QuadMesh_Triangulate() => _smallQuadMesh.Triangulate();

    [BenchmarkCategory("Meshes"), Benchmark]
    public GeometryResult QuadMesh_Validate() => _smallQuadMesh.Validate();

    #endregion

    #region Meshes - NormalGenerator

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<Vector3D> NormalGenerator_ComputeVertexNormals() =>
        NormalGenerator.ComputeVertexNormals(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<Vector3D> NormalGenerator_ComputeFaceNormals() =>
        NormalGenerator.ComputeFaceNormals(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<Vector3D> NormalGenerator_ComputeSmoothNormals() =>
        NormalGenerator.ComputeSmoothNormals(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<Vector3D> NormalGenerator_ComputeTangents() =>
        NormalGenerator.ComputeTangents(_mediumMesh);

    #endregion

    #region Meshes - MeshOptimizer

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh MeshOptimizer_WeldVertices() =>
        MeshOptimizer.WeldVertices(_mediumMesh, 1e-10);

    [BenchmarkCategory("Meshes"), Benchmark]
    public TriangleMesh MeshOptimizer_RemoveDegenerateTriangles() =>
        MeshOptimizer.RemoveDegenerateTriangles(_mediumMesh, 1e-12);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<double> MeshOptimizer_ComputeEdgeLengths() =>
        MeshOptimizer.ComputeEdgeLengths(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<int> MeshOptimizer_ComputeVertexValences() =>
        MeshOptimizer.ComputeVertexValences(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public ImmutableArray<double> MeshOptimizer_ComputeTriangleAreas() =>
        MeshOptimizer.ComputeTriangleAreas(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public double MeshOptimizer_ComputeMeshVolume() =>
        MeshOptimizer.ComputeMeshVolume(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public double MeshOptimizer_ComputeSurfaceArea() =>
        MeshOptimizer.ComputeSurfaceArea(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public bool MeshOptimizer_IsManifold() => MeshOptimizer.IsManifold(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public bool MeshOptimizer_IsWatertight() => MeshOptimizer.IsWatertight(_mediumMesh);

    [BenchmarkCategory("Meshes"), Benchmark]
    public int MeshOptimizer_ComputeEulerCharacteristic() =>
        MeshOptimizer.ComputeEulerCharacteristic(_mediumMesh);

    #endregion

    #region Tessellation - SurfaceTessellator

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateBezier_10x10() =>
        SurfaceTessellator.Tessellate(_bezier, 10, 10);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateBezier_20x20() =>
        SurfaceTessellator.Tessellate(_bezier, 20, 20);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateBSpline_10x10() =>
        SurfaceTessellator.Tessellate(_bspline, 10, 10);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateBSpline_20x20() =>
        SurfaceTessellator.Tessellate(_bspline, 20, 20);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateSphere_10x20() =>
        SurfaceTessellator.TessellateSphere(_sphere, 10, 20);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateSphere_20x40() =>
        SurfaceTessellator.TessellateSphere(_sphere, 20, 40);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateCylinder_10x5() =>
        SurfaceTessellator.TessellateCylinder(_cylinder, 10, 5);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public TriangleMesh SurfaceTessellator_TessellateCylinder_20x10() =>
        SurfaceTessellator.TessellateCylinder(_cylinder, 20, 10);

    #endregion

    #region Tessellation - PolygonTriangulator

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle2D> PolygonTriangulator_Triangulate2D_Triangle() =>
        PolygonTriangulator.Triangulate(_poly2D3);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle2D> PolygonTriangulator_Triangulate2D_Square() =>
        PolygonTriangulator.Triangulate(_poly2D4);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle2D> PolygonTriangulator_Triangulate2D_Pentagon() =>
        PolygonTriangulator.Triangulate(_poly2D5);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle2D> PolygonTriangulator_Triangulate2D_Octagon() =>
        PolygonTriangulator.Triangulate(_poly2D8);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle3D> PolygonTriangulator_Triangulate3D_Square() =>
        PolygonTriangulator.Triangulate(_poly3D4, Vector3D.UnitZ);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Triangle3D> PolygonTriangulator_Triangulate3D_Octagon() =>
        PolygonTriangulator.Triangulate(_poly3D8, Vector3D.UnitZ);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public bool PolygonTriangulator_IsEar() =>
        PolygonTriangulator.IsEar(_poly2D5, 4, 0, 1);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public double PolygonTriangulator_SignedArea_Square() =>
        PolygonTriangulator.SignedArea(_poly2D4);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public double PolygonTriangulator_SignedArea_Octagon() =>
        PolygonTriangulator.SignedArea(_poly2D8);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public IReadOnlyList<Point2D> PolygonTriangulator_EnsureWinding() =>
        PolygonTriangulator.EnsureWinding(_poly2D8, WindingOrder.CounterClockwise);

    #endregion

    #region Tessellation - CurveSubdivider

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> CurveSubdivider_Subdivide2D_OnePass() =>
        CurveSubdivider.Subdivide(_polyline2D, 1);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> CurveSubdivider_Subdivide2D_ThreePasses() =>
        CurveSubdivider.Subdivide(_polyline2D, 3);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point3D> CurveSubdivider_Subdivide3D_OnePass() =>
        CurveSubdivider.Subdivide(_polyline3D, 1);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point3D> CurveSubdivider_Subdivide3D_ThreePasses() =>
        CurveSubdivider.Subdivide(_polyline3D, 3);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> CurveSubdivider_ChaikinSubdivide_OneIter() =>
        CurveSubdivider.ChaikinSubdivide(_polyline2D, 1);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> CurveSubdivider_ChaikinSubdivide_ThreeIters() =>
        CurveSubdivider.ChaikinSubdivide(_polyline2D, 3);

    #endregion

    #region Tessellation - AdaptiveTessellator

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> AdaptiveTessellator_TessellateCurve() =>
        AdaptiveTessellator.TessellateCurve(
            t => new Point2D(System.Math.Cos(t), System.Math.Sin(t)),
            0.0, 2.0 * System.Math.PI, 4, 64, 0.01);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<ImmutableArray<Point3D>> AdaptiveTessellator_TessellateSurface() =>
        AdaptiveTessellator.TessellateSurface(
            (u, v) => new Point3D(
                System.Math.Sin(v) * System.Math.Cos(u),
                System.Math.Sin(v) * System.Math.Sin(u),
                System.Math.Cos(v)),
            0.0, 2.0 * System.Math.PI, 0.0, System.Math.PI, 4, 32, 0.1);

    [BenchmarkCategory("Tessellation"), Benchmark]
    public ImmutableArray<Point2D> AdaptiveTessellator_SubdivideEdge() =>
        AdaptiveTessellator.SubdivideEdge(
            new Point2D(0, 0), new Point2D(10, 0),
            param => System.Math.Abs(System.Math.Sin(param)) > 0.1, 8);

    #endregion
}
