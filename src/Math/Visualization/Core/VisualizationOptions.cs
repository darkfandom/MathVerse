namespace MathVerse.Math.Visualization.Core;
using System.Numerics;

/// <summary>Options for visualization rendering.</summary>
public sealed class VisualizationOptions
{
    public int Width { get; init; } = 1920;
    public int Height { get; init; } = 1080;
    public double DPI { get; init; } = 96.0;
    public bool AntiAliasing { get; init; } = true;
    public int MSAA { get; init; } = 4;
    public bool EnableDepthTest { get; init; } = true;
    public bool EnableBlending { get; init; } = true;
    public bool EnableShadows { get; init; }
    public int MaxLights { get; init; } = 8;
    public int MaxDrawCalls { get; init; } = 10000;
    public bool EnableFrustumCulling { get; init; } = true;
    public bool EnableLOD { get; init; } = true;
    public double FieldOfView { get; init; } = 60.0;
    public double NearPlane { get; init; } = 0.1;
    public double FarPlane { get; init; } = 1000.0;
    public string BackgroundColor { get; init; } = "#FFFFFF";
    public static VisualizationOptions Default => new();
}
