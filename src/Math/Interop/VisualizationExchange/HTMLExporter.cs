namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.IO;
using System.Text;

/// <summary>
/// Exports visualizations as self-contained HTML files with embedded SVG.
/// </summary>
public sealed class HTMLExporter
{
    private readonly SVGExporter _svgExporter = new();

    /// <summary>
    /// Exports a scene to a self-contained HTML string.
    /// </summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="title">The HTML page title.</param>
    /// <returns>A complete HTML string with embedded SVG.</returns>
    public string Export(Scene scene, string title)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));
        if (title is null)
            title = "MathVerse Visualization";

        string svg = _svgExporter.Export(scene);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.Append("  <title>").Append(EscapeHtml(title)).AppendLine("</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { margin: 0; padding: 20px; display: flex; justify-content: center; align-items: center; min-height: 100vh; background: #f5f5f5; }");
        sb.AppendLine("    .container { background: white; box-shadow: 0 2px 8px rgba(0,0,0,0.1); border-radius: 4px; padding: 10px; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"container\">");
        sb.Append("    ").AppendLine(svg.Trim());
        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Exports a scene to a stream as a self-contained HTML file.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="scene">The scene to export.</param>
    /// <param name="title">The HTML page title.</param>
    public void ExportToStream(Stream stream, Scene scene, string title)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        string html = Export(scene, title);
        byte[] bytes = Encoding.UTF8.GetBytes(html);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string EscapeHtml(string s)
    {
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
