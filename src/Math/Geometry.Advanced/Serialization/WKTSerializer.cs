using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of geometric objects to and from the Well-Known Text (WKT)
/// format as defined by the Open Geospatial Consortium (OGC). Supports POINT, POLYGON, and LINESTRING
/// representations in both 2D and 3D (with Z coordinates).
/// </summary>
public static class WKTSerializer
{
    /// <summary>
    /// Serializes a 2D point into the WKT POINT representation.
    /// The output format is "POINT(x y)" where coordinates are formatted with invariant culture.
    /// </summary>
    /// <param name="point">The 2D point to serialize.</param>
    /// <returns>A WKT string representing the point, e.g. "POINT(1 2)".</returns>
    public static string SerializePoint2D(Point2D point)
    {
        return string.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", point.X, point.Y);
    }

    /// <summary>
    /// Serializes a 2D polygon into the WKT POLYGON representation.
    /// The output format is "POLYGON((x1 y1, x2 y2, ...))" with coordinates listed in order.
    /// </summary>
    /// <param name="polygon">The ordered vertices defining the polygon boundary.</param>
    /// <returns>A WKT string representing the polygon.</returns>
    public static string SerializePolygon2D(ImmutableArray<Point2D> polygon)
    {
        var sb = new StringBuilder("POLYGON((");
        for (int i = 0; i < polygon.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", polygon[i].X, polygon[i].Y);
        }
        sb.Append("))");
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a sequence of 2D points into the WKT LINESTRING representation.
    /// The output format is "LINESTRING(x1 y1, x2 y2, ...)" with coordinates listed in order.
    /// </summary>
    /// <param name="points">The ordered points defining the line string.</param>
    /// <returns>A WKT string representing the line string.</returns>
    public static string SerializeLineString2D(ImmutableArray<Point2D> points)
    {
        var sb = new StringBuilder("LINESTRING(");
        for (int i = 0; i < points.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", points[i].X, points[i].Y);
        }
        sb.Append(")");
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a 3D point into the WKT POINT Z representation.
    /// The output format is "POINT Z(x y z)" where the Z keyword indicates a three-dimensional coordinate.
    /// </summary>
    /// <param name="point">The 3D point to serialize.</param>
    /// <returns>A WKT string representing the 3D point, e.g. "POINT Z(1 2 3)".</returns>
    public static string SerializePoint3D(Point3D point)
    {
        return string.Format(CultureInfo.InvariantCulture, "POINT Z({0} {1} {2})", point.X, point.Y, point.Z);
    }

    /// <summary>
    /// Parses a WKT POINT string and extracts the 2D coordinates.
    /// Accepts formats "POINT(x y)" or "POINT (x y)" with flexible whitespace.
    /// </summary>
    /// <param name="wkt">The WKT POINT string to parse.</param>
    /// <returns>The parsed 2D point.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not a valid WKT POINT representation.</exception>
    public static Point2D DeserializePoint2D(string wkt)
    {
        string content = ExtractContent(wkt, "POINT");
        string[] coords = content.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (coords.Length < 2)
            throw new FormatException("Invalid WKT POINT format: expected at least 2 coordinates.");

        double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
        double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
        return new Point2D(x, y);
    }

    /// <summary>
    /// Parses a WKT POLYGON string and extracts the ordered 2D vertex coordinates.
    /// Accepts formats "POLYGON((x1 y1, x2 y2, ...))" or "POLYGON ((x1 y1, x2 y2, ...))" with flexible whitespace.
    /// </summary>
    /// <param name="wkt">The WKT POLYGON string to parse.</param>
    /// <returns>An immutable array of 2D points defining the polygon boundary.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not a valid WKT POLYGON representation.</exception>
    public static ImmutableArray<Point2D> DeserializePolygon2D(string wkt)
    {
        string content = ExtractContent(wkt, "POLYGON");
        content = content.Trim('(', ')', ' ');
        return ParseCoordinatePairs(content);
    }

    private static string ExtractContent(string wkt, string type)
    {
        string trimmed = wkt.Trim();
        int parenIdx = trimmed.IndexOf('(');
        if (parenIdx < 0)
            throw new FormatException($"Invalid WKT format: missing parenthesis for {type}.");

        string typePart = trimmed.Substring(0, parenIdx).Trim();
        if (!typePart.Equals(type, StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"Expected WKT type '{type}' but found '{typePart}'.");

        string inside = trimmed.Substring(parenIdx);
        int lastParen = inside.LastIndexOf(')');
        if (lastParen >= 0)
            inside = inside.Substring(0, lastParen + 1);

        int firstParen = inside.IndexOf('(');
        return firstParen >= 0 ? inside.Substring(firstParen + 1, inside.Length - firstParen - 2) : inside;
    }

    private static ImmutableArray<Point2D> ParseCoordinatePairs(string coordinateString)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        string[] pairs = coordinateString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < pairs.Length; i++)
        {
            string[] coords = pairs[i].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length >= 2)
            {
                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                result.Add(new Point2D(x, y));
            }
        }

        return result.ToImmutable();
    }
}
