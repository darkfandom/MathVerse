namespace MathVerse.Math.Visualization.Export;
using System.Text;
using System.Collections.Generic;

/// <summary>Exports visualization scenes to SVG format.</summary>
public sealed class SVGExporter
{
    private const double DefaultWidth = 1920;
    private const double DefaultHeight = 1080;

    /// <summary>Exports a visualization scene to an SVG file.</summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="width">The SVG width in pixels.</param>
    /// <param name="height">The SVG height in pixels.</param>
    public static void Export(Core.VisualizationScene scene, string filePath, int width = 1920, int height = 1080)
    {
        string svg = GenerateSVG(scene, width, height);
        System.IO.File.WriteAllText(filePath, svg, Encoding.UTF8);
    }

    /// <summary>Generates SVG XML string from a visualization scene.</summary>
    /// <param name="scene">The scene to convert.</param>
    /// <param name="width">The SVG width.</param>
    /// <param name="height">The SVG height.</param>
    /// <returns>The SVG XML string.</returns>
    public static string GenerateSVG(Core.VisualizationScene scene, int width, int height)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">");

        sb.AppendLine("  <defs>");
        sb.AppendLine("    <style>");
        sb.AppendLine("      .axis-line { stroke: #333333; stroke-width: 1; }");
        sb.AppendLine("      .grid-line { stroke: #E0E0E0; stroke-width: 0.5; }");
        sb.AppendLine("      .label { font-family: serif; font-size: 14px; fill: #333333; }");
        sb.AppendLine("      .title { font-family: sans-serif; font-size: 18px; font-weight: bold; fill: #000000; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("  </defs>");

        sb.AppendLine($"  <rect width=\"{width}\" height=\"{height}\" fill=\"white\" />");

        if (scene != null)
        {
            AppendSceneElements(sb, scene, width, height);
        }

        sb.AppendLine("</svg>");

        return sb.ToString();
    }

    /// <summary>Converts a color hex string to SVG format.</summary>
    /// <param name="hex">The hex color string.</param>
    /// <returns>The SVG-compatible color string.</returns>
    public static string ConvertColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return "#000000";

        if (hex.StartsWith("#"))
            return hex;

        return "#" + hex;
    }

    /// <summary>Escapes special characters for SVG text elements.</summary>
    /// <param name="text">The input text.</param>
    /// <returns>Escaped text safe for SVG.</returns>
    public static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static void AppendSceneElements(StringBuilder sb, Core.VisualizationScene scene, int width, int height)
    {
        sb.AppendLine("  <g id=\"scene\">");

        if (scene.Objects != null)
        {
            foreach (var obj in scene.Objects)
            {
                AppendObject(sb, obj);
            }
        }

        sb.AppendLine("  </g>");
    }

    private static void AppendObject(StringBuilder sb, Core.VisualizationObject obj)
    {
        if (obj == null)
            return;

        string id = EscapeXml(obj.Id ?? "obj-" + System.Math.Abs(obj.GetHashCode()).ToString());
        string transform = FormatTransform(obj.Transform);

        sb.AppendLine($"    <g id=\"{id}\"{transform}>");

        switch (obj)
        {
            case Core.LinePlot linePlot:
                AppendLinePlot(sb, linePlot);
                break;
            case Core.SurfacePlot surfacePlot:
                AppendSurfacePlot(sb, surfacePlot);
                break;
            case Core.MeshObject meshObj:
                AppendMeshObject(sb, meshObj);
                break;
            case Core.PointCloud pointCloud:
                AppendPointCloud(sb, pointCloud);
                break;
            default:
                AppendGenericObject(sb, obj);
                break;
        }

        sb.AppendLine("    </g>");
    }

    private static void AppendLinePlot(StringBuilder sb, Core.LinePlot linePlot)
    {
        if (linePlot.Points == null || linePlot.Points.Count < 2)
            return;

        var sbPath = new StringBuilder();
        sbPath.Append("M ");

        for (int i = 0; i < linePlot.Points.Count; i++)
        {
            var pt = linePlot.Points[i];
            sbPath.Append($"{pt.X:F4} {pt.Y:F4}");

            if (i < linePlot.Points.Count - 1)
                sbPath.Append(" L ");
        }

        string color = ConvertColor(linePlot.Color ?? "#0000FF");
        double lineWidth = linePlot.LineWidth > 0 ? linePlot.LineWidth : 2.0;
        string dashArray = linePlot.IsDashed ? " stroke-dasharray=\"5,3\"" : "";

        sb.AppendLine($"      <path d=\"{sbPath}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"{lineWidth:F1}\"{dashArray} />");
    }

    private static void AppendSurfacePlot(StringBuilder sb, Core.SurfacePlot surfacePlot)
    {
        if (surfacePlot.Cells == null || surfacePlot.Cells.Count == 0)
            return;

        string fillColor = ConvertColor(surfacePlot.FillColor ?? "#4488CC");
        double opacity = surfacePlot.Opacity > 0 ? System.Math.Min(1.0, surfacePlot.Opacity) : 0.7;

        foreach (var cell in surfacePlot.Cells)
        {
            if (cell == null || cell.Count < 3)
                continue;

            var sbPath = new StringBuilder();
            sbPath.Append($"M {cell[0].X:F4} {cell[0].Y:F4}");

            for (int i = 1; i < cell.Count; i++)
            {
                sbPath.Append($" L {cell[i].X:F4} {cell[i].Y:F4}");
            }

            sbPath.Append(" Z");
            sb.AppendLine($"      <path d=\"{sbPath}\" fill=\"{fillColor}\" fill-opacity=\"{opacity:F2}\" stroke=\"{fillColor}\" stroke-width=\"0.5\" />");
        }
    }

    private static void AppendMeshObject(StringBuilder sb, Core.MeshObject meshObj)
    {
        if (meshObj.Faces == null || meshObj.Vertices == null)
            return;

        string strokeColor = ConvertColor(meshObj.WireframeColor ?? "#333333");
        string fillColor = ConvertColor(meshObj.FillColor ?? "#CCCCCC");

        foreach (var face in meshObj.Faces)
        {
            if (face == null || face.Length < 3)
                continue;

            var sbPath = new StringBuilder();
            sbPath.Append($"M {meshObj.Vertices[face[0]].X:F4} {meshObj.Vertices[face[0]].Y:F4}");

            for (int i = 1; i < face.Length; i++)
            {
                sbPath.Append($" L {meshObj.Vertices[face[i]].X:F4} {meshObj.Vertices[face[i]].Y:F4}");
            }

            sbPath.Append(" Z");
            sb.AppendLine($"      <path d=\"{sbPath}\" fill=\"{fillColor}\" stroke=\"{strokeColor}\" stroke-width=\"0.5\" />");
        }
    }

    private static void AppendPointCloud(StringBuilder sb, Core.PointCloud pointCloud)
    {
        if (pointCloud.Points == null)
            return;

        string color = ConvertColor(pointCloud.Color ?? "#FF0000");
        double size = pointCloud.PointSize > 0 ? pointCloud.PointSize : 3.0;

        foreach (var pt in pointCloud.Points)
        {
            sb.AppendLine($"      <circle cx=\"{pt.X:F4}\" cy=\"{pt.Y:F4}\" r=\"{size / 2.0:F2}\" fill=\"{color}\" />");
        }
    }

    private static void AppendGenericObject(StringBuilder sb, Core.VisualizationObject obj)
    {
        if (obj.Position.HasValue)
        {
            string color = ConvertColor(obj.Color ?? "#000000");
            sb.AppendLine($"      <circle cx=\"{obj.Position.Value.X:F4}\" cy=\"{obj.Position.Value.Y:F4}\" r=\"5\" fill=\"{color}\" />");
        }
    }

    private static string FormatTransform(object? transform)
    {
        if (transform == null)
            return "";

        return $" transform=\"{transform}\"";
    }
}
