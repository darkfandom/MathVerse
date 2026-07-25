using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using MathVerse.Math.Geometry;
using MathVerse.Math.Geometry.Cameras;
using MathVerse.Math.Geometry.Charts;
using MathVerse.Math.Geometry.Colors;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Lighting;
using MathVerse.Math.Geometry.Plotting;
using MathVerse.Math.Geometry.Rendering;
using MathVerse.Math.Geometry.SceneGraph;
using MathVerse.Math.Geometry.Transformations;

using MeshBuilder = MathVerse.Math.Geometry.Meshes.MeshBuilder;
using MeshTriangleMesh = MathVerse.Math.Geometry.Meshes.TriangleMesh;
using MeshVertex = MathVerse.Math.Geometry.Meshes.Vertex;
using Vector2D = MathVerse.Math.Geometry.Geometry2D.Vector2D;

namespace MathVerse.Performance.Tests.Geometry;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SceneCameraPlotBenchmarks
{
    // ── Scene Graph ──

    [BenchmarkCategory("SceneGraph")]
    [Benchmark(Baseline = true)]
    public Scene Scene_Creation() => new("BenchmarkScene");

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Scene Scene_AddRootNode()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new TransformNode("t0"));
        scene.AddRootNode(new TransformNode("t1"));
        return scene;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Scene Scene_RemoveRootNode()
    {
        var scene = new Scene("S");
        var n0 = new TransformNode("t0");
        var n1 = new TransformNode("t1");
        scene.AddRootNode(n0);
        scene.AddRootNode(n1);
        scene.RemoveRootNode(n0);
        return scene;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Scene Scene_Clear()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new TransformNode("a"));
        scene.AddRootNode(new TransformNode("b"));
        scene.AddRootNode(new TransformNode("c"));
        scene.Clear();
        return scene;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_TraverseAll_Shallow()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new TransformNode("a"));
        scene.AddRootNode(new TransformNode("b"));
        scene.AddRootNode(new TransformNode("c"));
        return scene.TraverseAll().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_TraverseAll_Deep()
    {
        var scene = new Scene("S");
        SceneNode current = new TransformNode("root");
        scene.AddRootNode(current);
        for (int i = 1; i < 50; i++)
        {
            var child = new TransformNode($"n{i}");
            current.AddChild(child);
            current = child;
        }
        return scene.TraverseAll().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_GetGeometryNodes()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new GeometryNode("g0", CreateSmallMesh()));
        scene.AddRootNode(new TransformNode("t0"));
        scene.AddRootNode(new GeometryNode("g1", CreateSmallMesh()));
        return scene.GetGeometryNodes().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_GetCameraNodes()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new CameraNode("c0"));
        scene.AddRootNode(new TransformNode("t0"));
        scene.AddRootNode(new CameraNode("c1"));
        return scene.GetCameraNodes().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_GetLightNodes()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new LightNode("l0", new PointLight()));
        scene.AddRootNode(new TransformNode("t0"));
        scene.AddRootNode(new LightNode("l1", new DirectionalLight()));
        return scene.GetLightNodes().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public BoundingBox3D Scene_ComputeBoundingBox_Empty()
    {
        var scene = new Scene("S");
        scene.AddRootNode(new TransformNode("t0"));
        return scene.ComputeBoundingBox();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public BoundingBox3D Scene_ComputeBoundingBox_WithGeometry()
    {
        var scene = new Scene("S");
        var mesh = CreateSmallMesh();
        var g0 = new GeometryNode("g0", mesh);
        var g1 = new GeometryNode("g1", mesh);
        g1.LocalTransform = Transform3D.Translation(5, 5, 5);
        scene.AddRootNode(g0);
        scene.AddRootNode(g1);
        return scene.ComputeBoundingBox();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_TotalNodeCount()
    {
        var scene = BuildWideScene(20);
        return scene.TotalNodeCount;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int Scene_NodeCount() => BuildWideScene(10).NodeCount;

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public SceneNode SceneNode_AddChild()
    {
        var parent = new TransformNode("parent");
        parent.AddChild(new TransformNode("child0"));
        parent.AddChild(new TransformNode("child1"));
        return parent;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public SceneNode SceneNode_RemoveChild()
    {
        var parent = new TransformNode("parent");
        var c0 = new TransformNode("c0");
        var c1 = new TransformNode("c1");
        parent.AddChild(c0);
        parent.AddChild(c1);
        parent.RemoveChild(c0);
        return parent;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public SceneNode SceneNode_ClearChildren()
    {
        var parent = new TransformNode("parent");
        parent.AddChild(new TransformNode("c0"));
        parent.AddChild(new TransformNode("c1"));
        parent.AddChild(new TransformNode("c2"));
        parent.ClearChildren();
        return parent;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int SceneNode_Traverse_Shallow()
    {
        var root = new TransformNode("root");
        root.AddChild(new TransformNode("a"));
        root.AddChild(new TransformNode("b"));
        return root.Traverse().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public int SceneNode_Traverse_Deep()
    {
        var root = new TransformNode("root");
        SceneNode current = root;
        for (int i = 0; i < 30; i++)
        {
            var child = new TransformNode($"n{i}");
            current.AddChild(child);
            current = child;
        }
        return root.Traverse().Count();
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Transform3D SceneNode_WorldTransform_Root()
    {
        var node = new TransformNode("r");
        node.LocalTransform = Transform3D.Translation(1, 2, 3);
        return node.WorldTransform;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Transform3D SceneNode_WorldTransform_Deep()
    {
        var root = new TransformNode("root");
        root.LocalTransform = Transform3D.Translation(1, 0, 0);
        SceneNode current = root;
        for (int i = 0; i < 10; i++)
        {
            var child = new TransformNode($"n{i}");
            child.LocalTransform = Transform3D.Translation(0, 1, 0);
            current.AddChild(child);
            current = child;
        }
        return current.WorldTransform;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public TransformNode TransformNode_Creation() => new("tn", Transform3D.RotationX(0.5));

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public GeometryNode GeometryNode_Creation() => new("gn", CreateSmallMesh());

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public CameraNode CameraNode_Creation() => new("cn", new PerspectiveCamera());

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public LightNode LightNode_Creation() => new("ln", new SpotLight());

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public bool SceneNode_Visible()
    {
        var n = new TransformNode("v");
        n.Visible = false;
        n.Visible = true;
        return n.Visible;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public Scene BuildComplexScene()
    {
        var scene = new Scene("Complex");
        for (int i = 0; i < 10; i++)
        {
            var geo = new GeometryNode($"geo{i}", CreateSmallMesh());
            geo.LocalTransform = Transform3D.Translation(i, 0, 0);
            scene.AddRootNode(geo);
        }
        scene.AddRootNode(new CameraNode("cam", new PerspectiveCamera()));
        scene.AddRootNode(new LightNode("light", new PointLight()));
        return scene;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public MeshTriangleMesh Scene_GeometryNode_MeshAccess()
    {
        var mesh = CreateSmallMesh();
        var node = new GeometryNode("g", mesh);
        return node.Mesh!;
    }

    [BenchmarkCategory("SceneGraph")]
    [Benchmark]
    public string Scene_GeometryNode_MaterialAccess()
    {
        var node = new GeometryNode("g", CreateSmallMesh()) { MaterialName = "steel" };
        return node.MaterialName;
    }

    // ── Cameras ──

    [BenchmarkCategory("Cameras")]
    [Benchmark(Baseline = true)]
    public Transform3D PerspectiveCamera_ViewMatrix()
    {
        var cam = new PerspectiveCamera
        {
            Position = new Point3D(0, 5, 10),
            Target = Point3D.Origin,
            Up = Vector3D.UnitY
        };
        return cam.GetViewMatrix();
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Transform3D PerspectiveCamera_ProjectionMatrix()
    {
        var cam = new PerspectiveCamera { FieldOfView = 60.0, AspectRatio = 16.0 / 9.0 };
        return cam.GetProjectionMatrix();
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Transform3D OrthographicCamera_ViewMatrix()
    {
        var cam = new OrthographicCamera
        {
            Position = new Point3D(0, 5, 10),
            Target = Point3D.Origin
        };
        return cam.GetViewMatrix();
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Transform3D OrthographicCamera_ProjectionMatrix()
    {
        var cam = new OrthographicCamera { HalfWidth = 10.0, HalfHeight = 7.5 };
        return cam.GetProjectionMatrix();
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Vector3D PerspectiveCamera_Forward()
    {
        var cam = new PerspectiveCamera
        {
            Position = new Point3D(1, 2, 3),
            Target = new Point3D(4, 5, 6)
        };
        return cam.Forward;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Vector3D PerspectiveCamera_Right()
    {
        var cam = new PerspectiveCamera
        {
            Position = new Point3D(1, 2, 3),
            Target = new Point3D(4, 5, 6)
        };
        return cam.Right;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_MoveForward()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.MoveForward(2.5);
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_MoveRight()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.MoveRight(1.0);
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_MoveUp()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.MoveUp(3.0);
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_Rotate()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.Rotate(0.5, 0.3);
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_LookAt()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.LookAt(new Point3D(10, 5, -3));
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_Reset()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.MoveForward(5);
        ctrl.Rotate(1.0, 0.5);
        ctrl.Reset();
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public PerspectiveCamera PerspectiveCamera_RecordWith()
    {
        var cam = new PerspectiveCamera { FieldOfView = 45.0 };
        return cam with { FieldOfView = 90.0, AspectRatio = 2.0 };
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public OrthographicCamera OrthographicCamera_RecordWith()
    {
        var cam = new OrthographicCamera { HalfWidth = 5.0 };
        return cam with { HalfWidth = 20.0, HalfHeight = 15.0 };
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Camera CameraController_MultipleMoves()
    {
        var cam = new PerspectiveCamera();
        var ctrl = new CameraController(cam);
        ctrl.MoveForward(1.0);
        ctrl.MoveRight(0.5);
        ctrl.MoveUp(0.3);
        ctrl.Rotate(0.1, 0.05);
        return ctrl.Camera;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public double PerspectiveCamera_FieldOfViewAccess()
    {
        var cam = new PerspectiveCamera { FieldOfView = 75.0 };
        return cam.FieldOfView;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public double OrthographicCamera_HalfWidthAccess()
    {
        var cam = new OrthographicCamera { HalfWidth = 12.0 };
        return cam.HalfWidth;
    }

    [BenchmarkCategory("Cameras")]
    [Benchmark]
    public Transform3D PerspectiveCamera_NearFarPlanes()
    {
        var cam = new PerspectiveCamera { NearPlane = 0.01, FarPlane = 500.0 };
        return cam.GetProjectionMatrix();
    }

    // ── Colors ──

    [BenchmarkCategory("Colors")]
    [Benchmark(Baseline = true)]
    public Color Color_Creation() => new(0.5, 0.3, 0.8, 1.0);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_FromRgb() => Color.FromRgb(128, 64, 200);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_FromRgba() => Color.FromRgba(255, 128, 0, 200);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_FromHex_6() => Color.FromHex("#FF80C0");

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_FromHex_8() => Color.FromHex("#FF80C0AA");

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_WithAlpha() => Color.Red.WithAlpha(0.5);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_Lerp() => Color.Red.Lerp(Color.Blue, 0.5);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_Black() => Color.Black;

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_White() => Color.White;

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color Color_Transparent() => Color.Transparent;

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Viridis() => ColorMap.Viridis(0.5);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Inferno() => ColorMap.Inferno(0.75);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Plasma() => ColorMap.Plasma(0.25);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Magma() => ColorMap.Magma(0.9);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Jet() => ColorMap.Jet(0.6);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Grayscale() => ColorMap.Grayscale(0.42);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_CoolWarm() => ColorMap.CoolWarm(0.5);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Evaluate_Viridis() => ColorMap.Evaluate(0.5, ColorMapType.Viridis);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_Evaluate_Jet() => ColorMap.Evaluate(0.5, ColorMapType.Jet);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorPalette_Default_GetColor() => ColorPalette.Default.GetColor(5);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorPalette_Pastel_GetColor() => ColorPalette.Pastel.GetColor(3);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorPalette_Bold_GetColor() => ColorPalette.Bold.GetColor(7);

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public int ColorPalette_Default_Count() => ColorPalette.Default.Count;

    [BenchmarkCategory("Colors")]
    [Benchmark]
    public Color ColorMap_BatchEvaluate()
    {
        Color c = default;
        for (int i = 0; i < 10; i++)
        {
            double t = i / 9.0;
            c = ColorMap.Evaluate(t, ColorMapType.CoolWarm);
        }
        return c;
    }

    // ── Plotting ──

    [BenchmarkCategory("Plotting")]
    [Benchmark(Baseline = true)]
    public PlotResult PlotEngine_PlotFunction_Sin()
    {
        var engine = new PlotEngine();
        return engine.PlotFunction(System.Math.Sin, -3.14, 3.14);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotFunction_Polynomial()
    {
        var engine = new PlotEngine();
        return engine.PlotFunction(x => x * x * x - 3 * x + 1, -5, 5);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotFunction_Exp()
    {
        var engine = new PlotEngine();
        return engine.PlotFunction(System.Math.Exp, -2, 4);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotParametric_Circle()
    {
        var engine = new PlotEngine();
        return engine.PlotParametric(t => (System.Math.Cos(t), System.Math.Sin(t)), 0, 2 * System.Math.PI);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotParametric_Lissajous()
    {
        var engine = new PlotEngine();
        return engine.PlotParametric(t => (System.Math.Sin(3 * t), System.Math.Sin(2 * t)), 0, 2 * System.Math.PI);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotPolar_Rose()
    {
        var engine = new PlotEngine();
        return engine.PlotPolar(theta => System.Math.Cos(3 * theta), 0, 2 * System.Math.PI);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotScatter_Small()
    {
        var engine = new PlotEngine();
        var pts = new List<(double, double)>();
        for (int i = 0; i < 50; i++)
            pts.Add((i * 0.1, System.Math.Sin(i * 0.1)));
        return engine.PlotScatter(pts);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotScatter_Large()
    {
        var engine = new PlotEngine();
        var pts = new List<(double, double)>();
        for (int i = 0; i < 500; i++)
            pts.Add((i * 0.01, System.Math.Sin(i * 0.01) + i * 0.001));
        return engine.PlotScatter(pts);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotHistogram()
    {
        var engine = new PlotEngine();
        var vals = new List<double>();
        for (int i = 0; i < 200; i++)
            vals.Add(System.Math.Sin(i * 0.1) + i * 0.01);
        return engine.PlotHistogram(vals, 20);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotContour()
    {
        var engine = new PlotEngine();
        return engine.PlotContour((x, y) => System.Math.Sin(x) * System.Math.Cos(y), -3, 3, -3, 3, 5);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotVectorField()
    {
        var engine = new PlotEngine();
        return engine.PlotVectorField((x, y) => (-y, x), -3, 3, -3, 3, 10);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotSurface()
    {
        var engine = new PlotEngine();
        return engine.PlotSurface((x, y) => System.Math.Sin(x) * System.Math.Cos(y), -3, 3, -3, 3, 10);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotLine()
    {
        var engine = new PlotEngine();
        var pts = new List<(double, double)>();
        for (int i = 0; i < 100; i++)
            pts.Add((i * 0.1, System.Math.Sin(i * 0.1)));
        return engine.PlotLine(pts, label: "sin(x)");
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotFunction_CustomConfig()
    {
        var engine = new PlotEngine();
        var cfg = new PlotConfiguration
        {
            Title = "Custom",
            Width = 1200,
            Height = 800,
            ShowGrid = false
        };
        return engine.PlotFunction(x => x * x, -10, 10, cfg);
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotConfiguration PlotConfiguration_Default() => PlotConfiguration.Default;

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotConfiguration PlotConfiguration_Custom()
    {
        return new PlotConfiguration
        {
            Title = "Benchmark",
            XLabel = "x",
            YLabel = "y",
            Width = 640,
            Height = 480,
            ShowGrid = false,
            ShowLegend = false,
            AxisLimits = (-5.0, 5.0, -5.0, 5.0)
        };
    }

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotResult_Failed() => PlotResult.Failed("test error");

    [BenchmarkCategory("Plotting")]
    [Benchmark]
    public PlotResult PlotEngine_PlotFunction_LargeRange()
    {
        var engine = new PlotEngine();
        return engine.PlotFunction(x => System.Math.Sin(x) / (x + 1), -50, 50);
    }

    // ── Charts ──

    [BenchmarkCategory("Charts")]
    [Benchmark(Baseline = true)]
    public ChartResult ChartEngine_CreateLineChart()
    {
        var engine = new ChartEngine();
        var pts = ImmutableArray.CreateRange(Enumerable.Range(0, 50).Select(i => ((double)i, System.Math.Sin(i * 0.1))));
        var series = new List<Series> { new("sin", Color.Blue, pts) };
        return engine.CreateLineChart(series);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateAreaChart()
    {
        var engine = new ChartEngine();
        var pts = ImmutableArray.CreateRange(Enumerable.Range(0, 30).Select(i => ((double)i, i * 0.5)));
        var series = new List<Series> { new("area", Color.Green, pts) };
        return engine.CreateAreaChart(series);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateBarChart()
    {
        var engine = new ChartEngine();
        var pts = ImmutableArray.Create((0.0, 5.0), (1.0, 8.0), (2.0, 3.0), (3.0, 7.0));
        var series = new List<Series> { new("bars", Color.Red, pts) };
        return engine.CreateBarChart(series);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreatePieChart()
    {
        var engine = new ChartEngine();
        var slices = new List<PieSlice>
        {
            new("A", 30, Color.Red),
            new("B", 50, Color.Green),
            new("C", 20, Color.Blue)
        };
        return engine.CreatePieChart(slices);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateHistogramChart()
    {
        var engine = new ChartEngine();
        var vals = Enumerable.Range(0, 100).Select(i => System.Math.Sin(i * 0.1)).ToList();
        return engine.CreateHistogramChart(vals, 15);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateBoxPlot()
    {
        var engine = new ChartEngine();
        var data = new List<BoxPlotData>
        {
            new("set1", 1.0, 3.0, 5.0, 7.0, 9.0, ImmutableArray<double>.Empty),
            new("set2", 2.0, 4.0, 6.0, 8.0, 10.0, ImmutableArray<double>.Empty)
        };
        return engine.CreateBoxPlot(data);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateCandlestickChart()
    {
        var engine = new ChartEngine();
        var data = new List<CandlestickData>
        {
            new(new DateTime(2024, 1, 1), 100, 110, 95, 105),
            new(new DateTime(2024, 1, 2), 105, 115, 100, 112),
            new(new DateTime(2024, 1, 3), 112, 120, 108, 118),
            new(new DateTime(2024, 1, 4), 118, 125, 110, 115)
        };
        return engine.CreateCandlestickChart(data);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateLineChart_MultipleSeries()
    {
        var engine = new ChartEngine();
        var pts1 = ImmutableArray.CreateRange(Enumerable.Range(0, 30).Select(i => ((double)i, System.Math.Sin(i * 0.2))));
        var pts2 = ImmutableArray.CreateRange(Enumerable.Range(0, 30).Select(i => ((double)i, System.Math.Cos(i * 0.2))));
        var series = new List<Series>
        {
            new("sin", Color.Blue, pts1),
            new("cos", Color.Red, pts2)
        };
        return engine.CreateLineChart(series);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartConfiguration ChartConfiguration_Default() => ChartConfiguration.Default;

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartConfiguration ChartConfiguration_Custom()
    {
        return new ChartConfiguration
        {
            Title = "My Chart",
            Width = 1024,
            Height = 768,
            ShowLegend = false,
            ShowGrid = false
        };
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public Series Series_Creation()
    {
        var pts = ImmutableArray.Create((0.0, 1.0), (1.0, 2.0), (2.0, 3.0));
        return new Series("test", Color.Blue, pts);
    }

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public PieSlice PieSlice_Creation() => new("slice", 42.0, Color.Green);

    [BenchmarkCategory("Charts")]
    [Benchmark]
    public ChartResult ChartEngine_CreateHistogramChart_Large()
    {
        var engine = new ChartEngine();
        var vals = Enumerable.Range(0, 1000).Select(i => (double)i).ToList();
        return engine.CreateHistogramChart(vals, 50);
    }

    // ── Rendering ──

    [BenchmarkCategory("Rendering")]
    [Benchmark(Baseline = true)]
    public RenderBatch RenderBatch_AddCommand()
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        batch.AddCommand(new RenderCommand
        {
            Mesh = mesh,
            Transform = Transform3D.Identity,
            MaterialName = "default"
        });
        return batch;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public IReadOnlyDictionary<string, IReadOnlyList<RenderCommand>> RenderBatch_GetGroupedCommands_Small()
    {
        var batch = CreateSmallRenderBatch(5, 3);
        return batch.GetGroupedCommands();
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public IReadOnlyDictionary<string, IReadOnlyList<RenderCommand>> RenderBatch_GetGroupedCommands_Large()
    {
        var batch = CreateSmallRenderBatch(50, 10);
        return batch.GetGroupedCommands();
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public int RenderBatch_CommandCount_Accumulate()
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        for (int i = 0; i < 20; i++)
        {
            batch.AddCommand(new RenderCommand
            {
                Mesh = mesh,
                MaterialName = $"mat{i % 5}"
            });
        }
        return batch.CommandCount;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public int RenderBatch_MaterialCount_Accumulate()
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        for (int i = 0; i < 20; i++)
        {
            batch.AddCommand(new RenderCommand
            {
                Mesh = mesh,
                MaterialName = $"mat{i % 5}"
            });
        }
        return batch.MaterialCount;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public RenderBatch RenderBatch_Clear()
    {
        var batch = CreateSmallRenderBatch(10, 5);
        batch.Clear();
        return batch;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public RenderCommand RenderCommand_Creation()
    {
        return new RenderCommand
        {
            Mesh = CreateRenderMesh(),
            Transform = Transform3D.Translation(1, 2, 3),
            MaterialName = "wood",
            Priority = 5
        };
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public RenderBatch RenderBatch_AddMultipleSameMaterial()
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        for (int i = 0; i < 100; i++)
        {
            batch.AddCommand(new RenderCommand
            {
                Mesh = mesh,
                MaterialName = "same_material",
                Priority = i
            });
        }
        return batch;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public RenderBatch RenderBatch_AddMultipleDifferentMaterials()
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        for (int i = 0; i < 100; i++)
        {
            batch.AddCommand(new RenderCommand
            {
                Mesh = mesh,
                MaterialName = $"material_{i}",
                Priority = i
            });
        }
        return batch;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public RenderCommand RenderCommand_RecordWith()
    {
        var cmd = new RenderCommand { MaterialName = "old" };
        return cmd with { MaterialName = "new", Priority = 10 };
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public int RenderBatch_CommandCount_Empty()
    {
        var batch = new RenderBatch();
        return batch.CommandCount;
    }

    [BenchmarkCategory("Rendering")]
    [Benchmark]
    public int RenderBatch_MaterialCount_Empty()
    {
        var batch = new RenderBatch();
        return batch.MaterialCount;
    }

    // ── Diagnostics ──

    [BenchmarkCategory("Diagnostics")]
    [Benchmark(Baseline = true)]
    public bool Diagnostics_IsValid_Point3D()
    {
        var p = new Point3D(1.5, 2.3, -0.7);
        return GeometryDiagnostics.IsValid(p);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Point2D()
    {
        var p = new Point2D(1.5, 2.3);
        return GeometryDiagnostics.IsValid(p);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Vector3D()
    {
        var v = new Vector3D(1.0, 0.0, 0.0);
        return GeometryDiagnostics.IsValid(v);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Vector2D()
    {
        var v = new Vector2D(1.0, 2.0);
        return GeometryDiagnostics.IsValid(v);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Triangle3D()
    {
        var t = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));
        return GeometryDiagnostics.IsValid(t);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Triangle2D()
    {
        var t = new Triangle2D(
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(0, 1));
        return GeometryDiagnostics.IsValid(t);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public double Diagnostics_DegeneracyScore_Triangle3D_Equilateral()
    {
        double h = System.Math.Sqrt(3) / 2.0;
        var t = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0.5, h, 0));
        return GeometryDiagnostics.DegeneracyScore(t);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public double Diagnostics_DegeneracyScore_Triangle2D_Equilateral()
    {
        double h = System.Math.Sqrt(3) / 2.0;
        var t = new Triangle2D(
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(0.5, h));
        return GeometryDiagnostics.DegeneracyScore(t);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsConvex_Convex()
    {
        var polygon = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };
        return GeometryDiagnostics.IsConvex(polygon);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public WindingOrder Diagnostics_ComputeWindingOrder()
    {
        var polygon = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };
        return GeometryDiagnostics.ComputeWindingOrder(polygon);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public GeometryResult Diagnostics_GeometryResult_Ok() => GeometryResult.Ok();

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public GeometryResult Diagnostics_GeometryResult_Failure() =>
        GeometryResult.Failure("test error", GeometryDiagnosticType.DegenerateGeometry);

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_IsValid_Sphere3D()
    {
        var s = new Sphere3D(new Point3D(1, 2, 3), 5.0);
        return GeometryDiagnostics.IsValid(s);
    }

    [BenchmarkCategory("Diagnostics")]
    [Benchmark]
    public bool Diagnostics_DegeneracyScore_Degenerate()
    {
        var t = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(2, 0, 0));
        return GeometryDiagnostics.DegeneracyScore(t) > 1e-10;
    }

    // ── Helpers ──

    private static MeshTriangleMesh CreateSmallMesh()
    {
        var builder = new MeshBuilder();
        int v0 = builder.AddVertex(new Point3D(0, 0, 0));
        int v1 = builder.AddVertex(new Point3D(1, 0, 0));
        int v2 = builder.AddVertex(new Point3D(0, 1, 0));
        int v3 = builder.AddVertex(new Point3D(1, 1, 0));
        builder.AddTriangle(v0, v1, v2);
        builder.AddTriangle(v1, v3, v2);
        return builder.Build();
    }

    private static MeshTriangleMesh CreateRenderMesh()
    {
        var builder = new MeshBuilder();
        int v0 = builder.AddVertex(new Point3D(0, 0, 0));
        int v1 = builder.AddVertex(new Point3D(1, 0, 0));
        int v2 = builder.AddVertex(new Point3D(0, 1, 0));
        int v3 = builder.AddVertex(new Point3D(1, 1, 0));
        builder.AddTriangle(v0, v1, v2);
        builder.AddTriangle(v1, v3, v2);
        return builder.Build();
    }

    private static Scene BuildWideScene(int rootCount)
    {
        var scene = new Scene("Wide");
        for (int i = 0; i < rootCount; i++)
        {
            var node = new TransformNode($"r{i}");
            node.AddChild(new TransformNode($"c{i}a"));
            node.AddChild(new TransformNode($"c{i}b"));
            scene.AddRootNode(node);
        }
        return scene;
    }

    private static RenderBatch CreateSmallRenderBatch(int commandCount, int materialCount)
    {
        var batch = new RenderBatch();
        var mesh = CreateRenderMesh();
        for (int i = 0; i < commandCount; i++)
        {
            batch.AddCommand(new RenderCommand
            {
                Mesh = mesh,
                MaterialName = $"mat{i % materialCount}",
                Priority = i
            });
        }
        return batch;
    }
}
