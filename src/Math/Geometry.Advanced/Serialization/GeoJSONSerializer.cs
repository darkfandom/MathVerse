using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of 2D geometric objects to and from the GeoJSON format
/// as defined by RFC 7946. Supports Point, LineString, and Polygon geometry types, as well as
/// FeatureCollection wrappers for grouping multiple features.
/// </summary>
public static class GeoJSONSerializer
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Serializes a 2D point into a GeoJSON Feature containing a Point geometry.
    /// The GeoJSON output includes a "type": "Feature" wrapper with a "geometry" property
    /// holding the Point coordinates in [longitude, latitude] format (as GeoJSON specifies).
    /// </summary>
    /// <param name="point">The 2D point to serialize.</param>
    /// <returns>A GeoJSON Feature string containing the Point geometry.</returns>
    public static string SerializePoint(Point2D point)
    {
        return string.Format(CultureInfo.InvariantCulture,
            "{{\"type\":\"Feature\",\"geometry\":{{\"type\":\"Point\",\"coordinates\":[{0}, {1}]}}}}",
            point.X, point.Y);
    }

    /// <summary>
    /// Serializes a 2D polygon into a GeoJSON Feature containing a Polygon geometry.
    /// The polygon is represented as a single linear ring in the coordinates array,
    /// following GeoJSON's requirement that the first and last positions are identical.
    /// </summary>
    /// <param name="polygon">The ordered vertices defining the polygon boundary.</param>
    /// <returns>A GeoJSON Feature string containing the Polygon geometry.</returns>
    public static string SerializePolygon(ImmutableArray<Point2D> polygon)
    {
        var sb = new StringBuilder();
        sb.Append("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[");
        for (int i = 0; i < polygon.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "[{0}, {1}]", polygon[i].X, polygon[i].Y);
        }
        if (polygon.Length > 0)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, ", [{0}, {1}]",
                polygon[0].X, polygon[0].Y);
        }
        sb.Append("]]}}");
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a sequence of 2D points into a GeoJSON Feature containing a LineString geometry.
    /// Each point is represented as a coordinate pair in the GeoJSON coordinates array.
    /// </summary>
    /// <param name="line">The ordered points defining the line string.</param>
    /// <returns>A GeoJSON Feature string containing the LineString geometry.</returns>
    public static string SerializeLineString(ImmutableArray<Point2D> line)
    {
        var sb = new StringBuilder();
        sb.Append("{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[");
        for (int i = 0; i < line.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "[{0}, {1}]", line[i].X, line[i].Y);
        }
        sb.Append("]}}");
        return sb.ToString();
    }

    /// <summary>
    /// Wraps multiple GeoJSON Feature strings into a single GeoJSON FeatureCollection.
    /// Each feature string should be a valid GeoJSON Feature object.
    /// </summary>
    /// <param name="features">An array of GeoJSON Feature strings to include in the collection.</param>
    /// <returns>A GeoJSON FeatureCollection string containing all provided features.</returns>
    public static string SerializeFeatureCollection(ImmutableArray<string> features)
    {
        var sb = new StringBuilder("{\"type\":\"FeatureCollection\",\"features\":[");
        for (int i = 0; i < features.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(features[i]);
        }
        sb.Append("]}");
        return sb.ToString();
    }

    /// <summary>
    /// Parses a GeoJSON Feature string containing a Point geometry and extracts the 2D coordinates.
    /// The parser expects a JSON object with a "geometry" property containing a "type": "Point" entry.
    /// </summary>
    /// <param name="geojson">The GeoJSON Feature string to parse.</param>
    /// <returns>The parsed 2D point.</returns>
    /// <exception cref="FormatException">Thrown when the GeoJSON string does not contain a valid Point geometry.</exception>
    public static Point2D DeserializePoint(string geojson)
    {
        string coords = ExtractPointCoordinates(geojson);
        string[] parts = coords.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new FormatException("Invalid GeoJSON Point: expected at least 2 coordinates.");

        double x = double.Parse(parts[0], CultureInfo.InvariantCulture);
        double y = double.Parse(parts[1], CultureInfo.InvariantCulture);
        return new Point2D(x, y);
    }

    /// <summary>
    /// Parses a GeoJSON Feature string containing a Polygon geometry and extracts the 2D coordinates.
    /// The parser expects a JSON object with a "geometry" property containing a "type": "Polygon" entry
    /// and reads the first linear ring from the coordinates array.
    /// </summary>
    /// <param name="geojson">The GeoJSON Feature string to parse.</param>
    /// <returns>An immutable array of 2D points defining the polygon boundary (excluding the closing duplicate).</returns>
    /// <exception cref="FormatException">Thrown when the GeoJSON string does not contain a valid Polygon geometry.</exception>
    public static ImmutableArray<Point2D> DeserializePolygon(string geojson)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        string content = geojson;

        int coordIdx = content.IndexOf("\"coordinates\"");
        if (coordIdx < 0)
            throw new FormatException("Invalid GeoJSON Polygon: missing 'coordinates' property.");

        int firstBracket = content.IndexOf('[', coordIdx);
        if (firstBracket < 0)
            throw new FormatException("Invalid GeoJSON Polygon: missing coordinate array.");

        int depth = 0;
        int start = -1;
        for (int i = firstBracket; i < content.Length; i++)
        {
            if (content[i] == '[')
            {
                depth++;
                if (depth == 2 && start < 0)
                    start = i;
            }
            else if (content[i] == ']')
            {
                depth--;
                if (depth == 1 && start >= 0)
                {
                    string ringContent = content.Substring(start + 1, i - start - 1);
                    return ParseCoordinateArray(ringContent);
                }
            }
        }

        return result.ToImmutable();
    }

    private static string ExtractPointCoordinates(string geojson)
    {
        int coordIdx = geojson.IndexOf("\"coordinates\"");
        if (coordIdx < 0)
            throw new FormatException("Invalid GeoJSON Point: missing 'coordinates' property.");

        int bracketStart = geojson.IndexOf('[', coordIdx);
        if (bracketStart < 0)
            throw new FormatException("Invalid GeoJSON Point: missing coordinate array.");

        int depth = 0;
        for (int i = bracketStart; i < geojson.Length; i++)
        {
            if (geojson[i] == '[') depth++;
            else if (geojson[i] == ']')
            {
                depth--;
                if (depth == 0)
                    return geojson.Substring(bracketStart + 1, i - bracketStart - 1);
            }
        }

        throw new FormatException("Invalid GeoJSON Point: unclosed coordinate array.");
    }

    private static ImmutableArray<Point2D> ParseCoordinateArray(string arrayContent)
    {
        var result = ImmutableArray.CreateBuilder<Point2D>();
        string[] pairs = arrayContent.Split(new[] { ']' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < pairs.Length; i++)
        {
            string cleaned = pairs[i].TrimStart(',', ' ', '[');
            string[] coords = cleaned.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length >= 2)
            {
                double x = double.Parse(coords[0], CultureInfo.InvariantCulture);
                double y = double.Parse(coords[1], CultureInfo.InvariantCulture);
                result.Add(new Point2D(x, y));
            }
        }

        if (result.Count >= 2)
        {
            Point2D first = result[0];
            Point2D last = result[result.Count - 1];
            double dx = System.Math.Abs(first.X - last.X);
            double dy = System.Math.Abs(first.Y - last.Y);
            if (dx < Tolerance && dy < Tolerance)
                result.RemoveAt(result.Count - 1);
        }

        return result.ToImmutable();
    }
}
