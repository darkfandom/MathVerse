namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Interface for exporting scenes as PDF documents.
/// </summary>
public interface IPDFExporter
{
    /// <summary>
    /// Exports a scene to a PDF byte array.
    /// </summary>
    /// <param name="scene">The scene to export.</param>
    /// <returns>A byte array containing the PDF document.</returns>
    byte[] Export(Scene scene);
}

/// <summary>
/// PDF export adapter that generates a minimal PDF with vector content.
/// </summary>
public sealed class PDFExportAdapter : IPDFExporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly PDFExporter PdfHelper = new();

    /// <inheritdoc/>
    public byte[] Export(Scene scene)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));

        return PdfHelper.GeneratePdf(scene);
    }
}

/// <summary>
/// Internal minimal PDF generator that produces PDFs with vector drawing commands.
/// </summary>
internal sealed class PDFExporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public byte[] GeneratePdf(Scene scene)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, 1024, leaveOpen: true);

        var objects = new StringBuilder();
        int objCount = 0;

        int catalogId = ++objCount;
        int pagesId = ++objCount;
        int pageId = ++objCount;
        int contentId = ++objCount;

        var content = new StringBuilder();
        content.AppendLine("q");
        AppendPdfColor(content, scene.BackgroundColor);
        content.AppendLine($"0 0 {scene.Width.ToString(Inv)} {scene.Height.ToString(Inv)} re f");

        foreach (var elem in scene.Elements)
            AppendElement(content, elem);

        content.AppendLine("Q");

        objects.AppendLine($"{catalogId} 0 obj");
        objects.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
        objects.AppendLine("endobj");

        objects.AppendLine($"{pagesId} 0 obj");
        objects.AppendLine($"<< /Type /Pages /Kids [{pageId} 0 R] /Count 1 >>");
        objects.AppendLine("endobj");

        objects.AppendLine($"{pageId} 0 obj");
        objects.AppendLine($"<< /Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 {scene.Width.ToString(Inv)} {scene.Height.ToString(Inv)}] /Contents {contentId} 0 R >>");
        objects.AppendLine("endobj");

        objects.AppendLine($"{contentId} 0 obj");
        objects.AppendLine($"<< /Length {content.Length} >>");
        objects.AppendLine("stream");
        objects.Append(content);
        objects.AppendLine("endstream");
        objects.AppendLine("endobj");

        long contentOffset = ms.Position;
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        ms.Position = 0;
        using (var finalWriter = new StreamWriter(ms, Encoding.ASCII, 1024, leaveOpen: true))
        {
            // Rewrite with proper offsets
            ms.SetLength(0);
            WritePdf(ms, scene, content.ToString(), catalogId, pagesId, pageId, contentId);
        }

        return ms.ToArray();
    }

    private void WritePdf(MemoryStream ms, Scene scene, string contentStream,
        int catalogId, int pagesId, int pageId, int contentId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");

        int[] objOffsets = new int[5];
        int objNum = 1;

        objOffsets[1] = sb.Length;
        sb.AppendLine($"{catalogId} 0 obj");
        sb.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
        sb.AppendLine("endobj");

        objOffsets[2] = sb.Length;
        sb.AppendLine($"{pagesId} 0 obj");
        sb.AppendLine($"<< /Type /Pages /Kids [{pageId} 0 R] /Count 1 >>");
        sb.AppendLine("endobj");

        objOffsets[3] = sb.Length;
        sb.AppendLine($"{pageId} 0 obj");
        sb.AppendLine($"<< /Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 {scene.Width.ToString(Inv)} {scene.Height.ToString(Inv)}] /Contents {contentId} 0 R >>");
        sb.AppendLine("endobj");

        objOffsets[4] = sb.Length;
        sb.AppendLine($"{contentId} 0 obj");
        sb.AppendLine($"<< /Length {contentStream.Length} >>");
        sb.AppendLine("stream");
        sb.Append(contentStream);
        if (!contentStream.EndsWith("\n")) sb.AppendLine();
        sb.AppendLine("endstream");
        sb.AppendLine("endobj");

        int xrefOffset = sb.Length;
        sb.AppendLine("xref");
        sb.AppendLine($"0 {objNum}");
        sb.AppendLine("0000000000 65535 f ");
        for (int i = 1; i < objNum; i++)
            sb.AppendLine($"{objOffsets[i]:D10} 00000 n ");

        sb.AppendLine("trailer");
        sb.AppendLine($"<< /Size {objNum} /Root {catalogId} 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine($"{xrefOffset}");
        sb.AppendLine("%%EOF");

        byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
        ms.Write(bytes, 0, bytes.Length);
    }

    private static void AppendElement(StringBuilder sb, SceneElement elem)
    {
        switch (elem)
        {
            case CircleElement circle:
                AppendPdfColor(sb, circle.FillColor);
                DrawPdfCircle(sb, circle.CX, circle.CY, circle.Radius, true);
                break;

            case LineElement line:
                AppendPdfColor(sb, line.StrokeColor);
                sb.AppendLine($"w {line.StrokeWidth.ToString(Inv)}");
                sb.AppendLine($"{line.X1.ToString(Inv)} {line.Y1.ToString(Inv)} m {line.X2.ToString(Inv)} {line.Y2.ToString(Inv)} l S");
                break;

            case TextElement text:
                AppendPdfColor(sb, text.FillColor);
                sb.AppendLine($"BT /F1 {text.FontSize.ToString(Inv)} Tf {text.X.ToString(Inv)} {text.Y.ToString(Inv)} Td");
                sb.AppendLine($"({EscapePdf(text.Text)}) Tj ET");
                break;

            case PathElement path:
                AppendPdfColor(sb, path.StrokeColor);
                sb.AppendLine($"w {path.StrokeWidth.ToString(Inv)}");
                string pdfPath = ConvertSvgPathToPdf(path.PathData);
                sb.AppendLine(pdfPath);
                sb.AppendLine("S");
                break;
        }
    }

    private static void DrawPdfCircle(StringBuilder sb, double cx, double cy, double r, bool fill)
    {
        double k = 0.5522847498;
        double kr = k * r;
        sb.AppendLine($"{(cx + r).ToString(Inv)} {cy.ToString(Inv)} m");
        sb.AppendLine($"{(cx + r).ToString(Inv)} {(cy + kr).ToString(Inv)} {(cx + kr).ToString(Inv)} {(cy + r).ToString(Inv)} {cx.ToString(Inv)} {(cy + r).ToString(Inv)} c");
        sb.AppendLine($"{(cx - kr).ToString(Inv)} {(cy + r).ToString(Inv)} {(cx - r).ToString(Inv)} {(cy + kr).ToString(Inv)} {(cx - r).ToString(Inv)} {cy.ToString(Inv)} c");
        sb.AppendLine($"{(cx - r).ToString(Inv)} {(cy - kr).ToString(Inv)} {(cx - kr).ToString(Inv)} {(cy - r).ToString(Inv)} {cx.ToString(Inv)} {(cy - r).ToString(Inv)} c");
        sb.AppendLine($"{(cx + kr).ToString(Inv)} {(cy - r).ToString(Inv)} {(cx + r).ToString(Inv)} {(cy - kr).ToString(Inv)} {(cx + r).ToString(Inv)} {cy.ToString(Inv)} c");
        sb.AppendLine(fill ? "f" : "S");
    }

    private static void AppendPdfColor(StringBuilder sb, string cssColor)
    {
        if (string.IsNullOrEmpty(cssColor) || cssColor[0] != '#') return;
        string hex = cssColor.Substring(1);
        if (hex.Length < 6) return;
        if (int.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, Inv, out int r) &&
            int.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, Inv, out int g) &&
            int.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, Inv, out int b))
        {
            sb.AppendLine($"{(r / 255.0).ToString(Inv)} {(g / 255.0).ToString(Inv)} {(b / 255.0).ToString(Inv)} rg");
        }
    }

    private static string ConvertSvgPathToPdf(string pathData)
    {
        if (string.IsNullOrEmpty(pathData)) return string.Empty;
        var sb = new StringBuilder();
        int pos = 0;

        while (pos < pathData.Length)
        {
            while (pos < pathData.Length && (pathData[pos] == ' ' || pathData[pos] == ',')) pos++;
            if (pos >= pathData.Length) break;

            char cmd = pathData[pos];
            pos++;

            switch (cmd)
            {
                case 'M':
                case 'm':
                    double mx = ParseNext(pathData, ref pos);
                    double my = ParseNext(pathData, ref pos);
                    sb.AppendLine($"{mx.ToString(Inv)} {my.ToString(Inv)} m");
                    break;
                case 'L':
                case 'l':
                    double lx = ParseNext(pathData, ref pos);
                    double ly = ParseNext(pathData, ref pos);
                    sb.AppendLine($"{lx.ToString(Inv)} {ly.ToString(Inv)} l");
                    break;
                case 'Z':
                case 'z':
                    sb.AppendLine("h");
                    break;
            }
        }

        return sb.ToString();
    }

    private static double ParseNext(string s, ref int pos)
    {
        while (pos < s.Length && (s[pos] == ' ' || s[pos] == ',')) pos++;
        int start = pos;
        while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.' || s[pos] == '-' || s[pos] == '+'))
            pos++;
        if (double.TryParse(s.Substring(start, pos - start), NumberStyles.Float, Inv, out double val))
            return val;
        return 0;
    }

    private static string EscapePdf(string s)
    {
        return s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
