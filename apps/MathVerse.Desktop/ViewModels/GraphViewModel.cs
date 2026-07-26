using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.Core;
using MathVerse.Math.Expressions;
using MathVerse.Math.Parsing;
using MathVerse.Math.Visualization._2DPlotting;
using MathVerse.Math.Visualization.Export;
using MathVerse.Math.Visualization.FunctionVisualization;

namespace MathVerse.Desktop.ViewModels;

public partial class GraphViewModel : ObservableObject
{
    [ObservableProperty] private double _viewportCenterX;
    [ObservableProperty] private double _viewportCenterY;
    [ObservableProperty] private double _viewportScale = 40;
    [ObservableProperty] private bool _is3D;
    [ObservableProperty] private double _cameraAzimuth = 45;
    [ObservableProperty] private double _cameraElevation = 30;
    [ObservableProperty] private double _cameraDistance = 15;

    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private bool _showAxes = true;
    [ObservableProperty] private bool _showLabels = true;

    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _cursorCoords = string.Empty;
    [ObservableProperty] private string _hoverCoords = string.Empty;

    [ObservableProperty] private bool _isAnimating;
    [ObservableProperty] private double _currentTime;
    [ObservableProperty] private double _animationSpeed = 1.0;
    [ObservableProperty] private bool _loopAnimation = true;
    [ObservableProperty] private int _fps = 60;
    [ObservableProperty] private double _animationDuration = 10;

    [ObservableProperty] private GraphEntry? _selectedGraph;
    [ObservableProperty] private Bitmap? _viewportBitmap;

    [ObservableProperty] private string _newExpression = string.Empty;
    [ObservableProperty] private int _selectedGraphTypeIndex;

    public ObservableCollection<GraphEntry> Graphs { get; } = new();

    private GraphRenderer? _renderer;
    private CancellationTokenSource? _renderCts;
    private int _viewportWidth = 800;
    private int _viewportHeight = 600;

    private static readonly string[] GraphTypeNames =
    [
        "Cartesian", "Polar", "Parametric", "Surface",
        "Vector Field", "Contour", "Heatmap", "Scatter", "Histogram"
    ];
    public string[] GraphTypes => GraphTypeNames;

    partial void OnViewportScaleChanged(double value) => RequestRender();
    partial void OnViewportCenterXChanged(double value) => RequestRender();
    partial void OnViewportCenterYChanged(double value) => RequestRender();
    partial void OnShowGridChanged(bool value) => RequestRender();
    partial void OnShowAxesChanged(bool value) => RequestRender();

    partial void OnSelectedGraphChanged(GraphEntry? value)
    {
        foreach (var g in Graphs) g.IsSelected = g == value;
    }

    [RelayCommand]
    private void AddGraph()
    {
        var expr = NewExpression?.Trim();
        if (string.IsNullOrEmpty(expr)) return;
        var type = SelectedGraphTypeIndex switch
        {
            0 => GraphType.Cartesian,
            1 => GraphType.Polar,
            2 => GraphType.Parametric2D,
            3 => GraphType.Surface,
            4 => GraphType.VectorField,
            5 => GraphType.Contour,
            6 => GraphType.Heatmap,
            7 => GraphType.Scatter,
            8 => GraphType.Histogram,
            _ => GraphType.Cartesian
        };
        var entry = new GraphEntry
        {
            Expression = expr, Type = type,
            Color = GraphEntry.PresetColors[Graphs.Count % GraphEntry.PresetColors.Length],
            Label = expr
        };
        var parameters = GraphRenderer.DetectParameters(expr);
        foreach (var p in parameters)
        {
            var slider = new ParameterSlider(p, 1.0, -10, 10, 0.1);
            slider.PropertyChanged += (_, _) => RequestRender();
            entry.ParameterSliders.Add(slider);
        }
        Graphs.Add(entry);
        NewExpression = string.Empty;
        SelectedGraph = entry;
        RequestRender();
    }

    [RelayCommand]
    private void RemoveGraph(GraphEntry? entry)
    {
        if (entry == null) return;
        Graphs.Remove(entry);
        if (SelectedGraph == entry) SelectedGraph = Graphs.FirstOrDefault();
        RequestRender();
    }

    [RelayCommand]
    private void ClearGraphs() { Graphs.Clear(); SelectedGraph = null; RequestRender(); }

    [RelayCommand]
    private void ToggleVisibility(GraphEntry? entry)
    {
        if (entry == null) return;
        entry.IsVisible = !entry.IsVisible;
        RequestRender();
    }

    [RelayCommand]
    private void Home() { ViewportCenterX = 0; ViewportCenterY = 0; ViewportScale = 40; }

    [RelayCommand]
    private void ZoomIn() => ViewportScale *= 1.3;

    [RelayCommand]
    private void ZoomOut() => ViewportScale /= 1.3;

    [RelayCommand]
    private void FitAll()
    {
        if (Graphs.Count == 0) { Home(); return; }
        double xMin = double.MaxValue, xMax = double.MinValue;
        double yMin = double.MaxValue, yMax = double.MinValue;
        foreach (var g in Graphs.Where(g => g.IsVisible))
        {
            xMin = System.Math.Min(xMin, g.XMin); xMax = System.Math.Max(xMax, g.XMax);
            yMin = System.Math.Min(yMin, g.YMin); yMax = System.Math.Max(yMax, g.YMax);
        }
        ViewportCenterX = (xMin + xMax) / 2;
        ViewportCenterY = (yMin + yMax) / 2;
        double rangeX = (xMax - xMin) * 1.2;
        double rangeY = (yMax - yMin) * 1.2;
        ViewportScale = System.Math.Min(_viewportWidth / rangeX, _viewportHeight / rangeY);
    }

    [RelayCommand]
    private void ResetView()
    {
        ViewportCenterX = 0; ViewportCenterY = 0; ViewportScale = 40;
        CameraAzimuth = 45; CameraElevation = 30; CameraDistance = 15;
        RequestRender();
    }

    [RelayCommand] private void ToggleGrid() => ShowGrid = !ShowGrid;

    [RelayCommand]
    private void ToggleAnimation()
    {
        IsAnimating = !IsAnimating;
        StatusText = IsAnimating ? "Playing" : "Paused";
    }

    [RelayCommand]
    private void StopAnimation() { IsAnimating = false; CurrentTime = 0; StatusText = "Stopped"; }

    [RelayCommand]
    private async Task ExportPng()
    {
        if (_renderer == null) return;
        var desktop = App.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d
            ? d.MainWindow : null;
        if (desktop == null) return;
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(desktop);
        if (topLevel == null) return;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Export PNG", AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("PNG Image") { Patterns = ["*.png"] }]
        });
        if (files.Count > 0)
        {
            _renderer.SetViewport(ViewportCenterX, ViewportCenterY, ViewportScale);
            _renderer.Clear();
            RenderAll(_renderer);
            var pngBytes = PNGExporter.ToPNG(_renderer.Buffer);
            await File.WriteAllBytesAsync(files[0].Path.LocalPath, pngBytes);
            StatusText = $"Exported to {Path.GetFileName(files[0].Path.LocalPath)}";
        }
    }

    [RelayCommand]
    private void AddPresetExample(string? expression)
    {
        if (string.IsNullOrEmpty(expression)) return;
        NewExpression = expression;
        SelectedGraphTypeIndex = 0;
        AddGraph();
    }

    public void HandleMouseMove(double canvasX, double canvasY)
    {
        double mathX = (canvasX - _viewportWidth / 2.0) / ViewportScale + ViewportCenterX;
        double mathY = (_viewportHeight / 2.0 - canvasY) / ViewportScale + ViewportCenterY;
        HoverCoords = $"({mathX:F3}, {mathY:F3})";
    }

    public void HandleMouseWheel(double delta)
    {
        ViewportScale *= delta > 0 ? 1.15 : 1.0 / 1.15;
    }

    public void HandlePan(double dx, double dy)
    {
        ViewportCenterX -= dx / ViewportScale;
        ViewportCenterY += dy / ViewportScale;
    }

    public void HandleRotate(double dx, double dy)
    {
        CameraAzimuth += dx * 0.5;
        CameraElevation = double.Clamp(CameraElevation + dy * 0.5, -90, 90);
        RequestRender();
    }

    public void UpdateViewportSize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        _viewportWidth = width;
        _viewportHeight = height;
        RequestRender();
    }

    public void TickAnimation(double deltaTime)
    {
        if (!IsAnimating) return;
        CurrentTime += deltaTime * AnimationSpeed;
        if (CurrentTime >= AnimationDuration)
        {
            CurrentTime = LoopAnimation ? 0 : AnimationDuration;
            if (!LoopAnimation) IsAnimating = false;
        }
        foreach (var g in Graphs)
            foreach (var s in g.ParameterSliders)
            {
                double range = s.Max - s.Min;
                s.Value = s.Min + (System.Math.Sin(CurrentTime * 0.5 + s.Name.GetHashCode() * 0.1) + 1) / 2 * range;
            }
        RequestRender();
    }

    public void RequestRender()
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0) return;
        _renderer ??= new GraphRenderer(_viewportWidth, _viewportHeight);
        if (_renderer.Width != _viewportWidth || _renderer.Height != _viewportHeight)
            _renderer = new GraphRenderer(_viewportWidth, _viewportHeight);

        _renderCts?.Cancel();
        _renderCts = new CancellationTokenSource();
        var token = _renderCts.Token;
        int w = _viewportWidth, h = _viewportHeight;
        double cx = ViewportCenterX, cy = ViewportCenterY, scale = ViewportScale;
        bool grid = ShowGrid, labels = ShowLabels;
        var graphs = Graphs.ToList();
        var camAz = CameraAzimuth;
        var camEl = CameraElevation;

        Task.Run(() =>
        {
            try
            {
                var r = new GraphRenderer(w, h);
                r.SetViewport(cx, cy, scale);
                r.Clear();
                if (grid) r.DrawGrid(labels);
                RenderGraphs(r, graphs, camAz, camEl);
                if (token.IsCancellationRequested) return;

                var pngBytes = PNGExporter.ToPNG(r.Buffer);
                if (token.IsCancellationRequested) return;
                using var stream = new MemoryStream(pngBytes);
                var bmp = new Bitmap(stream);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var old = ViewportBitmap;
                    ViewportBitmap = bmp;
                    old?.Dispose();
                }, Avalonia.Threading.DispatcherPriority.Default);
            }
            catch { }
        }, token);
    }

    private void RenderAll(GraphRenderer r)
    {
        foreach (var g in Graphs.Where(g => g.IsVisible))
            RenderSingleGraph(r, g, CameraAzimuth, CameraElevation);
    }

    private static void RenderGraphs(GraphRenderer r, List<GraphEntry> graphs, double camAz, double camEl)
    {
        foreach (var g in graphs.Where(g => g.IsVisible))
            RenderSingleGraph(r, g, camAz, camEl);
    }

    private static void RenderSingleGraph(GraphRenderer r, GraphEntry g, double camAz, double camEl)
    {
        try
        {
            var parameters = new Dictionary<string, double>();
            foreach (var s in g.ParameterSliders) parameters[s.Name] = s.Value;

            switch (g.Type)
            {
                case GraphType.Cartesian:
                    var fn = GraphRenderer.BuildFunctionFromExpression(g.Expression, "x", parameters);
                    r.DrawCurve(fn, g.Color, g.LineWidth, g.ShowFill);
                    break;
                case GraphType.Polar:
                    var polarFn = GraphRenderer.BuildFunctionFromExpression(g.Expression, "theta", parameters);
                    r.DrawPolarCurve(polarFn, g.Color, g.LineWidth, g.ThetaMin, g.ThetaMax);
                    break;
                case GraphType.Parametric2D:
                    var parts = g.Expression.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var xFn = GraphRenderer.BuildFunctionFromExpression(parts[0].Trim(), "t", parameters);
                        var yFn = GraphRenderer.BuildFunctionFromExpression(parts[1].Trim(), "t", parameters);
                        r.DrawParametric(t => (xFn(t), yFn(t)), g.Color, g.LineWidth, 0, 6.283185);
                    }
                    break;
                case GraphType.VectorField:
                    var vFn = GraphRenderer.Build2DFunctionFromExpression(g.Expression, "x", "y", parameters);
                    r.DrawVectorField((x, y) =>
                    {
                        double mag = vFn(x, y);
                        double angle = mag * System.Math.PI;
                        return (System.Math.Cos(angle), System.Math.Sin(angle));
                    }, g.Color, 20, false);
                    break;
                case GraphType.Contour:
                    var cFn = GraphRenderer.Build2DFunctionFromExpression(g.Expression, "x", "y", parameters);
                    r.DrawContour(cFn, 10, g.Color, g.LineWidth);
                    break;
                case GraphType.Heatmap:
                    var hFn = GraphRenderer.Build2DFunctionFromExpression(g.Expression, "x", "y", parameters);
                    r.DrawHeatmap(hFn, g.HeatmapResolution, -2, 2);
                    break;
                case GraphType.Scatter:
                    var scatterData = GenerateScatterData();
                    r.DrawScatterPlot(scatterData.x, scatterData.y, g.Color, g.LineWidth);
                    break;
                case GraphType.Histogram:
                    var histData = GenerateHistogramData();
                    r.DrawHistogram(histData, 20, g.Color);
                    break;
                case GraphType.Surface:
                    DrawSurface3D(r, g, parameters, camAz, camEl);
                    break;
                case GraphType.Fractal:
                    DrawMandelbrot(r, g);
                    break;
                default:
                    var defFn = GraphRenderer.BuildFunctionFromExpression(g.Expression, "x", parameters);
                    r.DrawCurve(defFn, g.Color, g.LineWidth);
                    break;
            }
        }
        catch { }
    }

    private static void DrawSurface3D(GraphRenderer r, GraphEntry g, Dictionary<string, double> parameters,
        double camAzDeg, double camElDeg)
    {
        var fn = GraphRenderer.Build2DFunctionFromExpression(g.Expression, "x", "y", parameters);
        int res = 50;
        double dx = (g.XMax - g.XMin) / res;
        double dy = (g.YMax - g.YMin) / res;
        double az = camAzDeg * System.Math.PI / 180;
        double el = camElDeg * System.Math.PI / 180;
        double cosAz = System.Math.Cos(az), sinAz = System.Math.Sin(az);
        double cosEl = System.Math.Cos(el), sinEl = System.Math.Sin(el);
        var tris = new List<(int x0, int y0, int x1, int y1, int x2, int y2, double zAvg, byte cr, byte cg, byte cb)>();

        for (int i = 0; i < res; i++)
        {
            for (int j = 0; j < res; j++)
            {
                double[] vx = [g.XMin + i * dx, g.XMin + (i + 1) * dx, g.XMin + i * dx, g.XMin + (i + 1) * dx];
                double[] vy = [g.YMin + j * dy, g.YMin + j * dy, g.YMin + (j + 1) * dy, g.YMin + (j + 1) * dy];
                double[] vz = new double[4];
                bool valid = true;
                for (int k = 0; k < 4; k++)
                {
                    vz[k] = fn(vx[k], vy[k]);
                    if (double.IsNaN(vz[k]) || double.IsInfinity(vz[k])) { valid = false; break; }
                }
                if (!valid) continue;
                double avgZ = (vz[0] + vz[1] + vz[2] + vz[3]) / 4;
                double t = double.Clamp((avgZ - g.ZMin) / (g.ZMax - g.ZMin + 1e-10), 0, 1);
                byte cr = (byte)(t * 200 + 55);
                byte cg = (byte)((1 - t) * 150 + 50);
                byte cb = (byte)(200 - t * 150);
                double[] projX = new double[4], projY = new double[4];
                for (int k = 0; k < 4; k++)
                {
                    double rx = vx[k] * cosAz - vy[k] * sinAz;
                    double ry = -(vx[k] * sinAz + vy[k] * cosAz) * sinEl + vz[k] * cosEl;
                    double scale2 = r.Width * 0.04;
                    projX[k] = r.Width / 2.0 + rx * scale2;
                    projY[k] = r.Height / 2.0 - ry * scale2;
                }
                tris.Add(((int)projX[0], (int)projY[0], (int)projX[1], (int)projY[1],
                          (int)projX[2], (int)projY[2], avgZ, cr, cg, cb));
                tris.Add(((int)projX[1], (int)projY[1], (int)projX[2], (int)projY[2],
                          (int)projX[3], (int)projY[3], avgZ, cr, cg, cb));
            }
        }
        foreach (var tri in tris.OrderByDescending(t => t.zAvg))
            DrawTriangleFilled(r, tri.x0, tri.y0, tri.x1, tri.y1, tri.x2, tri.y2, tri.cr, tri.cg, tri.cb);
    }

    private static void DrawTriangleFilled(GraphRenderer r, int x0, int y0, int x1, int y1, int x2, int y2,
        byte cr, byte cg, byte cb)
    {
        int minX = System.Math.Max(0, System.Math.Min(x0, System.Math.Min(x1, x2)));
        int maxX = System.Math.Min(r.Width - 1, System.Math.Max(x0, System.Math.Max(x1, x2)));
        int minY = System.Math.Max(0, System.Math.Min(y0, System.Math.Min(y1, y2)));
        int maxY = System.Math.Min(r.Height - 1, System.Math.Max(y0, System.Math.Max(y1, y2)));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (PointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                    r.SetPixel(x, y, cb, cg, cr, 255);
    }

    private static bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
    {
        int d1 = (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
        int d2 = (px - x1) * (y0 - y1) - (x0 - x1) * (py - y1);
        int d3 = (px - x0) * (y2 - y0) - (x2 - x0) * (py - y0);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static void DrawMandelbrot(GraphRenderer r, GraphEntry g)
    {
        int maxIter = g.FractalMaxIter;
        for (int px = 0; px < r.Width; px++)
        {
            for (int py = 0; py < r.Height; py++)
            {
                double cx = g.XMin + (double)px / r.Width * (g.XMax - g.XMin);
                double cy = g.YMin + (double)py / r.Height * (g.YMax - g.YMin);
                double zx = 0, zy = 0;
                int iter = 0;
                while (zx * zx + zy * zy < 4 && iter < maxIter)
                {
                    double tmp = zx * zx - zy * zy + cx;
                    zy = 2 * zx * zy + cy;
                    zx = tmp;
                    iter++;
                }
                if (iter < maxIter)
                {
                    double t = (double)iter / maxIter;
                    r.SetPixel(px, py,
                        (byte)(8.5 * (1 - t) * (1 - t) * (1 - t) * t * 255),
                        (byte)(15 * (1 - t) * (1 - t) * t * t * 255),
                        (byte)(9 * (1 - t) * t * t * t * 255), 255);
                }
            }
        }
    }

    private static (double[] x, double[] y) GenerateScatterData()
    {
        int n = 200;
        var xData = new double[n]; var yData = new double[n];
        var rng = new Random(42);
        for (int i = 0; i < n; i++)
        {
            xData[i] = rng.NextDouble() * 10 - 5;
            yData[i] = System.Math.Sin(xData[i]) + rng.NextDouble() * 0.5 - 0.25;
        }
        return (xData, yData);
    }

    private static double[] GenerateHistogramData()
    {
        int n = 500;
        var data = new double[n];
        var rng = new Random(42);
        for (int i = 0; i < n; i++)
            data[i] = rng.NextDouble() * 4 - 2 + System.Math.Sin(rng.NextDouble() * 3);
        return data;
    }
}
