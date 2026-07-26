using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MathVerse.Desktop.ViewModels;

public enum GraphType
{
    Cartesian,
    Polar,
    Parametric2D,
    Parametric3D,
    Surface,
    Implicit,
    Contour,
    Heatmap,
    Histogram,
    Scatter,
    VectorField,
    ComplexPlane,
    DomainColoring,
    Fractal,
    PointCloud
}

public partial class GraphEntry : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public GraphType Type { get; set; }

    [ObservableProperty] private string _expression = string.Empty;
    [ObservableProperty] private string _expressionY = string.Empty;
    [ObservableProperty] private string _color = "#4A9EFF";
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private double _lineWidth = 2.0;
    [ObservableProperty] private int _samples = 500;
    [ObservableProperty] private string _label = string.Empty;

    [ObservableProperty] private double _xMin = -10;
    [ObservableProperty] private double _xMax = 10;
    [ObservableProperty] private double _yMin = -10;
    [ObservableProperty] private double _yMax = 10;
    [ObservableProperty] private double _zMin = -5;
    [ObservableProperty] private double _zMax = 5;

    [ObservableProperty] private bool _showFill;
    [ObservableProperty] private bool _showWireframe;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private bool _showAxes = true;
    [ObservableProperty] private bool _showLabels = true;

    [ObservableProperty] private double _thetaMin = 0;
    [ObservableProperty] private double _thetaMax = 6.283185;

    [ObservableProperty] private int _fractalMaxIter = 100;
    [ObservableProperty] private int _heatmapResolution = 50;

    [ObservableProperty] private ObservableCollection<string> _detectedParameters = new();
    [ObservableProperty] private ObservableCollection<ParameterSlider> _parameterSliders = new();

    public string DisplayName => string.IsNullOrEmpty(Label)
        ? $"{Type}: {Expression}"
        : Label;

    public static string GraphTypeToIcon(GraphType type) => type switch
    {
        GraphType.Cartesian => "\u2212",
        GraphType.Polar => "\u2218",
        GraphType.Parametric2D => "\u223F",
        GraphType.Parametric3D => "\u223F",
        GraphType.Surface => "\u25A6",
        GraphType.Implicit => "\u2261",
        GraphType.Contour => "\u2237",
        GraphType.Heatmap => "\u25A3",
        GraphType.Histogram => "\u2587",
        GraphType.Scatter => "\u25CF",
        GraphType.VectorField => "\u2197",
        GraphType.ComplexPlane => "\u2102",
        GraphType.DomainColoring => "\u2207",
        GraphType.Fractal => "\u2726",
        GraphType.PointCloud => "\u2022",
        _ => "\u00B7"
    };

    public static string[] PresetColors { get; } =
    [
        "#4A9EFF", "#8B5CF6", "#06D6A0", "#FF6B35",
        "#FF4444", "#FFD700", "#E879F9", "#22D3EE",
        "#84CC16", "#FB923C"
    ];
}

public sealed partial class ParameterSlider : ObservableObject
{
    public string Name { get; } = string.Empty;

    [ObservableProperty] private double _value;
    [ObservableProperty] private double _min = 0;
    [ObservableProperty] private double _max = 10;
    [ObservableProperty] private double _step = 0.1;

    public ParameterSlider() { }

    public ParameterSlider(string name, double value, double min, double max, double step = 0.1)
    {
        Name = name;
        _value = value;
        _min = min;
        _max = max;
        _step = step;
    }
}
