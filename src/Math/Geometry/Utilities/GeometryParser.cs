using System.Globalization;
using System.Text.RegularExpressions;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Utilities;

/// <summary>Parses geometric objects from string representations.</summary>
public static class GeometryParser
{
    /// <summary>Parses a 2D point from "(x, y)" format.</summary>
    public static Point2D ParsePoint2D(string s)
    {
        Match m = Regex.Match(s.Trim(), @"\(\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*\)");
        if (!m.Success) throw new System.FormatException($"Cannot parse Point2D from '{s}'.");
        return new Point2D(
            double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
            double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>Parses a 3D point from "(x, y, z)" format.</summary>
    public static Point3D ParsePoint3D(string s)
    {
        Match m = Regex.Match(s.Trim(), @"\(\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*\)");
        if (!m.Success) throw new System.FormatException($"Cannot parse Point3D from '{s}'.");
        return new Point3D(
            double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
            double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
            double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>Parses a 3D vector from "(x, y, z)" format.</summary>
    public static Vector3D ParseVector3D(string s)
    {
        Match m = Regex.Match(s.Trim(), @"\(\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*\)");
        if (!m.Success) throw new System.FormatException($"Cannot parse Vector3D from '{s}'.");
        return new Vector3D(
            double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
            double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
            double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>Parses a comma-separated list of 2D points.</summary>
    public static IReadOnlyList<Point2D> ParsePoints2D(string s)
    {
        string cleaned = s.Trim().Trim('[', ']').Trim();
        if (string.IsNullOrWhiteSpace(cleaned)) return Array.Empty<Point2D>();

        string[] parts = cleaned.Split(new[] { "),(" }, StringSplitOptions.RemoveEmptyEntries);
        List<Point2D> result = new();
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim().Trim('(', ')');
            string[] coords = part.Split(',');
            if (coords.Length >= 2)
            {
                result.Add(new Point2D(
                    double.Parse(coords[0].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(coords[1].Trim(), CultureInfo.InvariantCulture)));
            }
        }
        return result;
    }

    /// <summary>Parses a comma-separated list of 3D points.</summary>
    public static IReadOnlyList<Point3D> ParsePoints3D(string s)
    {
        string cleaned = s.Trim().Trim('[', ']').Trim();
        if (string.IsNullOrWhiteSpace(cleaned)) return Array.Empty<Point3D>();

        string[] parts = cleaned.Split(new[] { "),(" }, StringSplitOptions.RemoveEmptyEntries);
        List<Point3D> result = new();
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim().Trim('(', ')');
            string[] coords = part.Split(',');
            if (coords.Length >= 3)
            {
                result.Add(new Point3D(
                    double.Parse(coords[0].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(coords[1].Trim(), CultureInfo.InvariantCulture),
                    double.Parse(coords[2].Trim(), CultureInfo.InvariantCulture)));
            }
        }
        return result;
    }

    /// <summary>Parses a circle from "Circle(center_x, center_y, radius)" format.</summary>
    public static Circle2D ParseCircle2D(string s)
    {
        Match m = Regex.Match(s.Trim(), @"Circle\s*\(\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*\)");
        if (!m.Success) throw new System.FormatException($"Cannot parse Circle2D from '{s}'.");
        return new Circle2D(
            new Point2D(
                double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture)),
            double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>Parses a sphere from "Sphere(cx, cy, cz, radius)" format.</summary>
    public static Sphere3D ParseSphere3D(string s)
    {
        Match m = Regex.Match(s.Trim(), @"Sphere\s*\(\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*,\s*([-\d.E+]+)\s*\)");
        if (!m.Success) throw new System.FormatException($"Cannot parse Sphere3D from '{s}'.");
        return new Sphere3D(
            new Point3D(
                double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture)),
            double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture));
    }
}
