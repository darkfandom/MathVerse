namespace MathVerse.Math.Visualization._3DPlotting;

/// <summary>Options for configuring 3D plot rendering.</summary>
public sealed class Plot3DOptions
{
    /// <summary>Gets or sets the title of the plot.</summary>
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the background color as RGBA in [0,1].</summary>
    public System.Numerics.Vector4 BackgroundColor { get; set; } = new(1f, 1f, 1f, 1f);

    /// <summary>Gets or sets the color map name for surface coloring.</summary>
    public string ColorMap { get; set; } = "Viridis";

    /// <summary>Gets or sets whether to show the wireframe overlay.</summary>
    public bool ShowWireframe { get; set; }

    /// <summary>Gets or sets whether to show axis labels.</summary>
    public bool ShowAxes { get; set; } = true;

    /// <summary>Gets or sets the camera azimuth angle in degrees.</summary>
    public double CameraAzimuth { get; set; } = 45.0;

    /// <summary>Gets or sets the camera elevation angle in degrees.</summary>
    public double CameraElevation { get; set; } = 30.0;

    /// <summary>Gets or sets the camera distance from the origin.</summary>
    public double CameraDistance { get; set; } = 5.0;

    /// <summary>Gets or sets the X-axis label.</summary>
    public string XLabel { get; set; } = "X";

    /// <summary>Gets or sets the Y-axis label.</summary>
    public string YLabel { get; set; } = "Y";

    /// <summary>Gets or sets the Z-axis label.</summary>
    public string ZLabel { get; set; } = "Z";

    /// <summary>Gets or sets the point size for point cloud plots.</summary>
    public float PointSize { get; set; } = 5.0f;

    /// <summary>Gets or sets the line width for wireframe plots.</summary>
    public float LineWidth { get; set; } = 1.0f;

    /// <summary>Gets or sets the opacity of the surface in [0,1].</summary>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>Gets or sets the minimum value for color mapping.</summary>
    public double ColorMin { get; set; } = double.NaN;

    /// <summary>Gets or sets the maximum value for color mapping.</summary>
    public double ColorMax { get; set; } = double.NaN;
}
