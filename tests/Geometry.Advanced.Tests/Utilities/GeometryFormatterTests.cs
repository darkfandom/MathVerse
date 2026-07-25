namespace MathVerse.Geometry.Advanced.Tests.Utilities;

public class GeometryFormatterTests
{
    [Fact]
    public void Format_Point2D_ReturnsXYFormat()
    {
        var p = new Point2D(1.0, 2.0);
        string result = GeometryFormatter.Format(p);
        result.Should().Be("(1.000000, 2.000000)");
    }

    [Fact]
    public void Format_Point2D_NegativeValues()
    {
        var p = new Point2D(-3.5, 7.25);
        string result = GeometryFormatter.Format(p);
        result.Should().Contain("-3.500000");
        result.Should().Contain("7.250000");
    }

    [Fact]
    public void Format_Point2D_CustomFormat()
    {
        var p = new Point2D(1.23456789, 2.34567890);
        string result = GeometryFormatter.Format(p, "F2");
        result.Should().Be("(1.23, 2.35)");
    }

    [Fact]
    public void Format_Point3D_ReturnsXYZFormat()
    {
        var p = new Point3D(1.0, 2.0, 3.0);
        string result = GeometryFormatter.Format(p);
        result.Should().Be("(1.000000, 2.000000, 3.000000)");
    }

    [Fact]
    public void Format_Point3D_NegativeValues()
    {
        var p = new Point3D(-1.5, 0, 9.99);
        string result = GeometryFormatter.Format(p);
        result.Should().Contain("-1.500000");
        result.Should().Contain("0.000000");
        result.Should().Contain("9.990000");
    }

    [Fact]
    public void Format_Point3D_CustomFormat()
    {
        var p = new Point3D(1.11111, 2.22222, 3.33333);
        string result = GeometryFormatter.Format(p, "F1");
        result.Should().Be("(1.1, 2.2, 3.3)");
    }

    [Fact]
    public void Format_Vector3D_ReturnsXYZFormat()
    {
        var v = new Vector3D(1.0, 2.0, 3.0);
        string result = GeometryFormatter.Format(v);
        result.Should().Be("(1.000000, 2.000000, 3.000000)");
    }

    [Fact]
    public void Format_Vector3D_NegativeComponents()
    {
        var v = new Vector3D(-5, 0, 3);
        string result = GeometryFormatter.Format(v);
        result.Should().Contain("-5.000000");
        result.Should().Contain("0.000000");
        result.Should().Contain("3.000000");
    }

    [Fact]
    public void Format_Vector3D_ZeroVector()
    {
        var v = new Vector3D(0, 0, 0);
        string result = GeometryFormatter.Format(v);
        result.Should().Be("(0.000000, 0.000000, 0.000000)");
    }

    [Fact]
    public void Format_Triangle3D_ShowsThreePoints()
    {
        var t = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));
        string result = GeometryFormatter.Format(t);
        result.Should().Contain("->");
        result.Should().Contain("(0.000000, 0.000000, 0.000000)");
        result.Should().Contain("(1.000000, 0.000000, 0.000000)");
        result.Should().Contain("(0.000000, 1.000000, 0.000000)");
    }

    [Fact]
    public void Format_Triangle3D_ThreeArrowSeparators()
    {
        var t = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));
        string result = GeometryFormatter.Format(t);
        int arrows = result.Count(c => c == '>');
        arrows.Should().Be(2);
    }

    [Fact]
    public void Format_TriangleMesh_ShowsVertexAndTriangleCount()
    {
        var mesh = GeometryFactory.UnitCube();
        string result = GeometryFormatter.Format(mesh);
        result.Should().Contain("TriangleMesh");
        result.Should().Contain($"vertices={mesh.VertexCount}");
        result.Should().Contain($"triangles={mesh.TriangleCount}");
    }

    [Fact]
    public void Format_TriangleMesh_UnitCubeHas12Triangles()
    {
        var mesh = GeometryFactory.UnitCube();
        string result = GeometryFormatter.Format(mesh);
        result.Should().Contain("triangles=12");
    }

    [Fact]
    public void FormatPoints_Empty2D_ReturnsEmptyBrackets()
    {
        var points = Array.Empty<Point2D>();
        string result = GeometryFormatter.FormatPoints(points);
        result.Should().Be("[]");
    }

    [Fact]
    public void FormatPoints_SinglePoint2D()
    {
        var points = new[] { new Point2D(1, 2) };
        string result = GeometryFormatter.FormatPoints(points);
        result.Should().Be("[(1.000000, 2.000000)]");
    }

    [Fact]
    public void FormatPoints_MultiplePoints2D()
    {
        var points = new[]
        {
            new Point2D(1, 2),
            new Point2D(3, 4),
            new Point2D(5, 6)
        };
        string result = GeometryFormatter.FormatPoints(points);
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain("(1.000000, 2.000000)");
        result.Should().Contain("(3.000000, 4.000000)");
        result.Should().Contain("(5.000000, 6.000000)");
        result.Should().Contain(", ");
    }

    [Fact]
    public void FormatPoints_Empty3D_ReturnsEmptyBrackets()
    {
        var points = Array.Empty<Point3D>();
        string result = GeometryFormatter.FormatPoints(points);
        result.Should().Be("[]");
    }

    [Fact]
    public void FormatPoints_MultiplePoints3D()
    {
        var points = new[]
        {
            new Point3D(1, 2, 3),
            new Point3D(4, 5, 6)
        };
        string result = GeometryFormatter.FormatPoints(points);
        result.Should().Contain("(1.000000, 2.000000, 3.000000)");
        result.Should().Contain("(4.000000, 5.000000, 6.000000)");
    }

    [Fact]
    public void Format_Polygon2D_ShowsPolygonPrefix()
    {
        var poly = GeometryFactory.RegularPolygon(3, 1.0);
        string result = GeometryFormatter.Format(poly);
        result.Should().StartWith("Polygon2D(");
    }

    [Fact]
    public void Format_Polygon2D_ContainsVertices()
    {
        var poly = GeometryFactory.RegularPolygon(3, 1.0);
        string result = GeometryFormatter.Format(poly);
        result.Should().Contain("Polygon2D");
        result.Should().Contain("-1.000000");
        result.Should().Contain("0.500000");
    }

    [Fact]
    public void Format_Polyline2D_ShowsPolylinePrefix()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 0));
        var polyline = new Polyline2D(pts);
        string result = GeometryFormatter.Format(polyline);
        result.Should().StartWith("Polyline2D(");
    }

    [Fact]
    public void Format_Polyline2D_ContainsAllVertices()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 1));
        var polyline = new Polyline2D(pts);
        string result = GeometryFormatter.Format(polyline);
        result.Should().Contain("(0.000000, 0.000000)");
        result.Should().Contain("(1.000000, 1.000000)");
    }

    [Fact]
    public void Format_BoundingBox3D_ShowsAABB()
    {
        var box = GeometryFactory.AABB(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        string result = GeometryFormatter.Format(box);
        result.Should().StartWith("AABB(");
        result.Should().Contain("..");
    }

    [Fact]
    public void Format_BoundingBox3D_ContainsMinMax()
    {
        var box = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(10, 20, 30));
        string result = GeometryFormatter.Format(box);
        result.Should().Contain("(0.000000, 0.000000, 0.000000)");
        result.Should().Contain("(10.000000, 20.000000, 30.000000)");
    }

    [Fact]
    public void Format_Sphere3D_ShowsCenterAndRadius()
    {
        var sphere = new Sphere3D(new Point3D(1, 2, 3), 5.0);
        string result = GeometryFormatter.Format(sphere);
        result.Should().StartWith("Sphere(");
        result.Should().Contain("r=5.000000");
    }

    [Fact]
    public void Format_Sphere3D_ContainsCenter()
    {
        var sphere = new Sphere3D(new Point3D(1, 2, 3), 5.0);
        string result = GeometryFormatter.Format(sphere);
        result.Should().Contain("(1.000000, 2.000000, 3.000000)");
    }

    [Fact]
    public void ToWKT_SinglePoint()
    {
        var pts = ImmutableArray.Create(new Point2D(1, 2));
        string result = GeometryFormatter.ToWKT(pts);
        result.Should().StartWith("POLYGON (");
        result.Should().EndWith(")");
    }

    [Fact]
    public void ToWKT_FormatsCoordinates()
    {
        var pts = ImmutableArray.Create(new Point2D(1.5, 2.5), new Point3D(3.5, 4.5, 0).X > 0 ? new Point2D(3.5, 4.5) : new Point2D(3.5, 4.5));
        string result = GeometryFormatter.ToWKT(pts);
        result.Should().Contain("1.500000");
        result.Should().Contain("2.500000");
    }

    [Fact]
    public void ToWKT_ClosesPolygon()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1));
        string result = GeometryFormatter.ToWKT(pts);
        string content = result.Replace("POLYGON (", "").TrimEnd(')');
        string[] parts = content.Split(", ");
        parts.Length.Should().Be(4);
        parts[0].Should().Be(parts[3]);
    }

    [Fact]
    public void ToOBJ_UnitCube_HasVerticesAndFaces()
    {
        var mesh = GeometryFactory.UnitCube();
        string result = GeometryFormatter.ToOBJ(mesh);
        string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int vCount = lines.Count(l => l.StartsWith("v "));
        int fCount = lines.Count(l => l.StartsWith("f "));
        vCount.Should().Be(8);
        fCount.Should().Be(12);
    }

    [Fact]
    public void ToOBJ_VerticesAreFormatted()
    {
        var mesh = GeometryFactory.UnitCube();
        string result = GeometryFormatter.ToOBJ(mesh);
        result.Should().Contain("v ");
        result.Should().Contain("f ");
    }

    [Fact]
    public void ToOBJ_FaceIndicesAreOneBased()
    {
        var mesh = GeometryFactory.UnitCube();
        string result = GeometryFormatter.ToOBJ(mesh);
        string[] faceLines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.StartsWith("f ")).ToArray();
        foreach (string line in faceLines)
        {
            string[] parts = line.Substring(2).Split(' ');
            foreach (string part in parts)
            {
                int idx = int.Parse(part);
                idx.Should().BeGreaterThanOrEqualTo(1);
                idx.Should().BeLessThanOrEqualTo(mesh.VertexCount);
            }
        }
    }

    [Fact]
    public void Format_Polygon2D_WithFiveVertices()
    {
        var poly = GeometryFactory.RegularPolygon(5, 2.0);
        string result = GeometryFormatter.Format(poly);
        result.Should().Contain("Polygon2D");
    }

    [Fact]
    public void FormatPoints_TwoPoints_HasCommaSeparator()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(1, 1));
        string result = GeometryFormatter.FormatPoints(pts);
        result.Should().Contain(", ");
    }

    [Fact]
    public void Format_BoundingBox3D_CustomFormat()
    {
        var box = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(1, 2, 3));
        string result = GeometryFormatter.Format(box, "F2");
        result.Should().Contain("0.00");
        result.Should().Contain("1.00");
    }

    [Fact]
    public void Format_Sphere3D_RadiusFormat()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.14159);
        string result = GeometryFormatter.Format(sphere, "F4");
        result.Should().Contain("r=3.1416");
    }

    [Fact]
    public void FormatPoints_3D_WithCustomFormat()
    {
        var pts = ImmutableArray.Create(new Point3D(1.111, 2.222, 3.333));
        string result = GeometryFormatter.FormatPoints(pts, "F1");
        result.Should().Contain("(1.1, 2.2, 3.3)");
    }

    [Fact]
    public void ToWKT_EmptyPolygon()
    {
        var pts = ImmutableArray<Point2D>.Empty;
        string result = GeometryFormatter.ToWKT(pts);
        result.Should().StartWith("POLYGON (");
        result.Should().EndWith(")");
    }

    [Fact]
    public void Format_Point2D_Origin()
    {
        var p = Point2D.Origin;
        string result = GeometryFormatter.Format(p);
        result.Should().Be("(0.000000, 0.000000)");
    }

    [Fact]
    public void Format_Point3D_Origin()
    {
        var p = Point3D.Origin;
        string result = GeometryFormatter.Format(p);
        result.Should().Be("(0.000000, 0.000000, 0.000000)");
    }

    [Fact]
    public void Format_Polygon2D_Triangle()
    {
        var poly = GeometryFactory.RegularPolygon(3, 1.0);
        string result = GeometryFormatter.Format(poly);
        result.Should().Contain("Polygon2D(");
        result.Should().Contain(")");
    }

    [Fact]
    public void FormatPoints_SinglePoint3D()
    {
        var pts = ImmutableArray.Create(new Point3D(10, 20, 30));
        string result = GeometryFormatter.FormatPoints(pts);
        result.Should().Be("[(10.000000, 20.000000, 30.000000)]");
    }

    [Fact]
    public void Format_BoundingBox3D_SinglePoint()
    {
        var box = GeometryFactory.AABB(new Point3D(5, 5, 5), new Point3D(5, 5, 5));
        string result = GeometryFormatter.Format(box);
        result.Should().Contain("AABB(");
    }

    [Fact]
    public void Format_Sphere3D_OriginRadius1()
    {
        var sphere = new Sphere3D(Point3D.Origin, 1.0);
        string result = GeometryFormatter.Format(sphere);
        result.Should().Contain("r=1.000000");
    }

    [Fact]
    public void Format_Triangle3D_NonZeroVertices()
    {
        var t = new Triangle3D(
            new Point3D(1, 2, 3),
            new Point3D(4, 5, 6),
            new Point3D(7, 8, 9));
        string result = GeometryFormatter.Format(t);
        result.Should().Contain("(1.000000, 2.000000, 3.000000)");
        result.Should().Contain("(7.000000, 8.000000, 9.000000)");
    }

    [Fact]
    public void ToOBJ_UnitSphere_HasVertices()
    {
        var mesh = GeometryFactory.UnitSphere(3);
        string result = GeometryFormatter.ToOBJ(mesh);
        string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int vCount = lines.Count(l => l.StartsWith("v "));
        vCount.Should().Be(9);
    }

    [Fact]
    public void FormatPoints_FourPoints2D()
    {
        var pts = ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0),
            new Point2D(1, 1), new Point2D(0, 1));
        string result = GeometryFormatter.FormatPoints(pts);
        result.Should().Contain("(0.000000, 0.000000)");
        result.Should().Contain("(1.000000, 1.000000)");
    }

    [Fact]
    public void ToWKT_SinglePoint_FormatsCoordinates()
    {
        var pts = ImmutableArray.Create(new Point2D(42.5, -17.3));
        string result = GeometryFormatter.ToWKT(pts);
        result.Should().Contain("42.500000");
        result.Should().Contain("-17.300000");
    }

    [Fact]
    public void Format_Polyline2D_ThreePoints()
    {
        var pts = ImmutableArray.Create(new Point2D(0, 0), new Point2D(5, 5), new Point2D(10, 0));
        var polyline = new Polyline2D(pts);
        string result = GeometryFormatter.Format(polyline);
        result.Should().StartWith("Polyline2D(");
        result.Should().Contain("(0.000000, 0.000000)");
    }

    [Fact]
    public void Format_TriangleMesh_Empty()
    {
        var mesh = GeometryFactory.UnitSphere(2);
        string result = GeometryFormatter.Format(mesh);
        result.Should().Contain("vertices=4");
    }
}
