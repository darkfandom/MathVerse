namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Abstract base class for all scene elements.
/// </summary>
public abstract class SceneElement
{
    /// <summary>Gets or sets the fill color (CSS color string).</summary>
    public string FillColor { get; set; } = "#000000";

    /// <summary>Gets or sets the stroke color (CSS color string).</summary>
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>Gets or sets the stroke width.</summary>
    public double StrokeWidth { get; set; } = 1.0;

    /// <summary>Gets or sets the opacity (0.0 to 1.0).</summary>
    public double Opacity { get; set; } = 1.0;
}

/// <summary>
/// Represents a circle element in the scene.
/// </summary>
public sealed class CircleElement : SceneElement
{
    /// <summary>Gets or sets the center X coordinate.</summary>
    public double CX { get; set; }

    /// <summary>Gets or sets the center Y coordinate.</summary>
    public double CY { get; set; }

    /// <summary>Gets or sets the radius.</summary>
    public double Radius { get; set; }
}

/// <summary>
/// Represents a line element in the scene.
/// </summary>
public sealed class LineElement : SceneElement
{
    /// <summary>Gets or sets the start X coordinate.</summary>
    public double X1 { get; set; }

    /// <summary>Gets or sets the start Y coordinate.</summary>
    public double Y1 { get; set; }

    /// <summary>Gets or sets the end X coordinate.</summary>
    public double X2 { get; set; }

    /// <summary>Gets or sets the end Y coordinate.</summary>
    public double Y2 { get; set; }
}

/// <summary>
/// Represents a text element in the scene.
/// </summary>
public sealed class TextElement : SceneElement
{
    /// <summary>Gets or sets the text X coordinate.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the text Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the text content.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the font size in pixels.</summary>
    public double FontSize { get; set; } = 14.0;

    /// <summary>Gets or sets the font family.</summary>
    public string FontFamily { get; set; } = "sans-serif";

    /// <summary>Gets or sets the text anchor (start, middle, end).</summary>
    public string TextAnchor { get; set; } = "start";
}

/// <summary>
/// Represents an SVG path element in the scene.
/// </summary>
public sealed class PathElement : SceneElement
{
    /// <summary>Gets or sets the SVG path data string.</summary>
    public string PathData { get; set; } = string.Empty;
}

/// <summary>
/// Represents a complete visualization scene.
/// </summary>
public sealed class Scene
{
    /// <summary>Gets or sets the scene width in pixels.</summary>
    public double Width { get; set; } = 800.0;

    /// <summary>Gets or sets the scene height in pixels.</summary>
    public double Height { get; set; } = 600.0;

    /// <summary>Gets or sets the background color (CSS color string).</summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets the list of scene elements.
    /// </summary>
    public List<SceneElement> Elements { get; } = new();
}

/// <summary>
/// Exports visualizations to SVG format.
/// </summary>
public sealed class SVGExporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Exports a scene to an SVG string.
    /// </summary>
    /// <param name="scene">The scene to export.</param>
    /// <returns>An SVG XML string.</returns>
    public string Export(Scene scene)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));

        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{scene.Width.ToString(Inv)}\" height=\"{scene.Height.ToString(Inv)}\">");
        sb.AppendLine($"  <rect width=\"100%\" height=\"100%\" fill=\"{scene.BackgroundColor}\"/>");

        foreach (var elem in scene.Elements)
            AppendElement(sb, elem);

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Exports a scene to a stream as SVG.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="scene">The scene to export.</param>
    public void ExportToStream(Stream stream, Scene scene)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        string svg = Export(scene);
        byte[] bytes = Encoding.UTF8.GetBytes(svg);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void AppendElement(StringBuilder sb, SceneElement elem)
    {
        string opacity = elem.Opacity < 1.0 ? $" opacity=\"{elem.Opacity.ToString(Inv)}\"" : string.Empty;

        switch (elem)
        {
            case CircleElement circle:
                sb.AppendLine($"  <circle cx=\"{circle.CX.ToString(Inv)}\" cy=\"{circle.CY.ToString(Inv)}\" r=\"{circle.Radius.ToString(Inv)}\" fill=\"{circle.FillColor}\" stroke=\"{circle.StrokeColor}\" stroke-width=\"{circle.StrokeWidth.ToString(Inv)}\"{opacity}/>");
                break;
            case LineElement line:
                sb.AppendLine($"  <line x1=\"{line.X1.ToString(Inv)}\" y1=\"{line.Y1.ToString(Inv)}\" x2=\"{line.X2.ToString(Inv)}\" y2=\"{line.Y2.ToString(Inv)}\" stroke=\"{line.StrokeColor}\" stroke-width=\"{line.StrokeWidth.ToString(Inv)}\"{opacity}/>");
                break;
            case TextElement text:
                sb.AppendLine($"  <text x=\"{text.X.ToString(Inv)}\" y=\"{text.Y.ToString(Inv)}\" font-size=\"{text.FontSize.ToString(Inv)}\" font-family=\"{text.FontFamily}\" fill=\"{text.FillColor}\" text-anchor=\"{text.TextAnchor}\"{opacity}>{EscapeXml(text.Text)}</text>");
                break;
            case PathElement path:
                sb.AppendLine($"  <path d=\"{path.PathData}\" fill=\"{path.FillColor}\" stroke=\"{path.StrokeColor}\" stroke-width=\"{path.StrokeWidth.ToString(Inv)}\"{opacity}/>");
                break;
        }
    }

    private static string EscapeXml(string s)
    {
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
