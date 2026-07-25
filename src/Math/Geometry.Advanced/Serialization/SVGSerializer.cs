using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization of 2D geometric primitives to SVG (Scalable Vector Graphics) format.
/// Supports polygon paths, polyline paths, and circle elements, all enclosed within an
/// SVG viewport of the specified width and height.
/// </summary>
public static class SVGSerializer
{
    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    /// <summary>
    /// Serializes a 2D polygon into an SVG document with the polygon rendered as a closed path element.
    /// The polygon is filled with light blue and stroked with a blue border for visual clarity.
    /// The coordinate system is flipped vertically to match SVG's top-down Y-axis convention.
    /// </summary>
    /// <param name="polygon">The ordered vertices defining the polygon boundary.</param>
    /// <param name="width">The width of the SVG viewport in user units.</param>
    /// <param name="height">The height of the SVG viewport in user units.</param>
    /// <returns>A complete SVG document string containing the polygon path element.</returns>
    public static string SerializePolygon2D(ImmutableArray<Point2D> polygon, double width, double height)
    {
        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        if (polygon.Length > 0)
        {
            sb.Append("  <path d=\"M ");
            for (int i = 0; i < polygon.Length; i++)
            {
                if (i > 0) sb.Append(" L ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:G} {1:G}",
                    polygon[i].X, height - polygon[i].Y);
            }
            sb.AppendLine(" Z\" fill=\"#ADD8E6\" stroke=\"#0000FF\" stroke-width=\"1\" />");
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a 2D polyline into an SVG document with the polyline rendered as an open path element.
    /// The polyline is stroked with a black border and has no fill.
    /// The coordinate system is flipped vertically to match SVG's top-down Y-axis convention.
    /// </summary>
    /// <param name="polyline">The ordered vertices defining the polyline.</param>
    /// <param name="width">The width of the SVG viewport in user units.</param>
    /// <param name="height">The height of the SVG viewport in user units.</param>
    /// <returns>A complete SVG document string containing the polyline path element.</returns>
    public static string SerializePolyline2D(ImmutableArray<Point2D> polyline, double width, double height)
    {
        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        if (polyline.Length > 0)
        {
            sb.Append("  <polyline points=\"");
            for (int i = 0; i < polyline.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:G},{1:G}",
                    polyline[i].X, height - polyline[i].Y);
            }
            sb.AppendLine("\" fill=\"none\" stroke=\"#000000\" stroke-width=\"1\" />");
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a collection of 2D circles into an SVG document with each circle rendered as an SVG circle element.
    /// All circles are filled with light coral and stroked with a dark red border.
    /// The coordinate system is flipped vertically to match SVG's top-down Y-axis convention.
    /// </summary>
    /// <param name="circles">An array of tuples, each containing a center point and radius for a circle.</param>
    /// <param name="width">The width of the SVG viewport in user units.</param>
    /// <param name="height">The height of the SVG viewport in user units.</param>
    /// <returns>A complete SVG document string containing all circle elements.</returns>
    public static string SerializeCircles2D(ImmutableArray<(Point2D Center, double Radius)> circles, double width, double height)
    {
        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        for (int i = 0; i < circles.Length; i++)
        {
            (Point2D center, double radius) = circles[i];
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "  <circle cx=\"{0:G}\" cy=\"{1:G}\" r=\"{2:G}\" fill=\"#F08080\" stroke=\"#8B0000\" stroke-width=\"1\" />",
                center.X, height - center.Y, radius);
            sb.AppendLine();
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    private static void AppendSvgHeader(StringBuilder sb, double width, double height)
    {
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendFormat("<svg xmlns=\"{0}\" width=\"{1:G}\" height=\"{2:G}\" viewBox=\"0 0 {1:G} {2:G}\">",
            SvgNamespace, width, height);
        sb.AppendLine();
    }

    private static void AppendSvgFooter(StringBuilder sb)
    {
        sb.AppendLine("</svg>");
    }
}
