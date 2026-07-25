namespace MathVerse.Geometry.Advanced.Tests.Utilities;

public class GeometryParserTests
{
    [Fact]
    public void ParsePoint2D_Basic()
    {
        var p = GeometryParser.ParsePoint2D("(1.0, 2.0)");
        p.X.Should().BeApproximately(1.0, 1e-10);
        p.Y.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void ParsePoint2D_NegativeValues()
    {
        var p = GeometryParser.ParsePoint2D("(-3.5, 7.25)");
        p.X.Should().BeApproximately(-3.5, 1e-10);
        p.Y.Should().BeApproximately(7.25, 1e-10);
    }

    [Fact]
    public void ParsePoint2D_WithSpaces()
    {
        var p = GeometryParser.ParsePoint2D("( 1.0 , 2.0 )");
        p.X.Should().BeApproximately(1.0, 1e-10);
        p.Y.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void ParsePoint2D_ZeroOrigin()
    {
        var p = GeometryParser.ParsePoint2D("(0, 0)");
        p.X.Should().BeApproximately(0, 1e-10);
        p.Y.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void ParsePoint2D_LargeValues()
    {
        var p = GeometryParser.ParsePoint2D("(1234567.89, -9876543.21)");
        p.X.Should().BeApproximately(1234567.89, 0.01);
        p.Y.Should().BeApproximately(-9876543.21, 0.01);
    }

    [Fact]
    public void ParsePoint2D_InvalidFormat_Throws()
    {
        Action act = () => GeometryParser.ParsePoint2D("not a point");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsePoint2D_MissingParen_Throws()
    {
        Action act = () => GeometryParser.ParsePoint2D("1.0, 2.0");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsePoint3D_Basic()
    {
        var p = GeometryParser.ParsePoint3D("(1.0, 2.0, 3.0)");
        p.X.Should().BeApproximately(1.0, 1e-10);
        p.Y.Should().BeApproximately(2.0, 1e-10);
        p.Z.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParsePoint3D_NegativeValues()
    {
        var p = GeometryParser.ParsePoint3D("(-1.5, 0, 9.99)");
        p.X.Should().BeApproximately(-1.5, 1e-10);
        p.Y.Should().BeApproximately(0, 1e-10);
        p.Z.Should().BeApproximately(9.99, 1e-10);
    }

    [Fact]
    public void ParsePoint3D_WithSpaces()
    {
        var p = GeometryParser.ParsePoint3D("( 1.0 , 2.0 , 3.0 )");
        p.X.Should().BeApproximately(1.0, 1e-10);
        p.Y.Should().BeApproximately(2.0, 1e-10);
        p.Z.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParsePoint3D_InvalidFormat_Throws()
    {
        Action act = () => GeometryParser.ParsePoint3D("invalid");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsePoint3D_TwoComponents_Throws()
    {
        Action act = () => GeometryParser.ParsePoint3D("(1.0, 2.0)");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseVector3D_Basic()
    {
        var v = GeometryParser.ParseVector3D("(1.0, 2.0, 3.0)");
        v.X.Should().BeApproximately(1.0, 1e-10);
        v.Y.Should().BeApproximately(2.0, 1e-10);
        v.Z.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParseVector3D_NegativeValues()
    {
        var v = GeometryParser.ParseVector3D("(-5.5, 0, 3.3)");
        v.X.Should().BeApproximately(-5.5, 1e-10);
        v.Y.Should().BeApproximately(0, 1e-10);
        v.Z.Should().BeApproximately(3.3, 1e-10);
    }

    [Fact]
    public void ParseVector3D_InvalidFormat_Throws()
    {
        Action act = () => GeometryParser.ParseVector3D("garbage");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsePoints2D_EmptyList()
    {
        var pts = GeometryParser.ParsePoints2D("[]");
        pts.Should().BeEmpty();
    }

    [Fact]
    public void ParsePoints2D_SinglePoint()
    {
        var pts = GeometryParser.ParsePoints2D("[(1.0, 2.0)]");
        pts.Count.Should().Be(1);
        pts[0].X.Should().BeApproximately(1.0, 1e-10);
        pts[0].Y.Should().BeApproximately(2.0, 1e-10);
    }

    [Fact]
    public void ParsePoints2D_MultiplePoints()
    {
        var pts = GeometryParser.ParsePoints2D("[(1.0, 2.0),(3.0, 4.0)]");
        pts.Count.Should().Be(2);
        pts[0].X.Should().BeApproximately(1.0, 1e-10);
        pts[1].X.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParsePoints2D_ThreePoints()
    {
        var pts = GeometryParser.ParsePoints2D("[(0, 0),(1, 0),(0, 1)]");
        pts.Count.Should().Be(3);
    }

    [Fact]
    public void ParsePoints3D_EmptyList()
    {
        var pts = GeometryParser.ParsePoints3D("[]");
        pts.Should().BeEmpty();
    }

    [Fact]
    public void ParsePoints3D_SinglePoint()
    {
        var pts = GeometryParser.ParsePoints3D("[(1.0, 2.0, 3.0)]");
        pts.Count.Should().Be(1);
        pts[0].Z.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParsePoints3D_MultiplePoints()
    {
        var pts = GeometryParser.ParsePoints3D("[(1.0, 2.0, 3.0),(4.0, 5.0, 6.0)]");
        pts.Count.Should().Be(2);
        pts[1].X.Should().BeApproximately(4.0, 1e-10);
        pts[1].Z.Should().BeApproximately(6.0, 1e-10);
    }

    [Fact]
    public void ParseCircle2D_Basic()
    {
        var c = GeometryParser.ParseCircle2D("Circle(1.0, 2.0, 3.0)");
        c.Center.X.Should().BeApproximately(1.0, 1e-10);
        c.Center.Y.Should().BeApproximately(2.0, 1e-10);
        c.Radius.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParseCircle2D_NegativeCenter()
    {
        var c = GeometryParser.ParseCircle2D("Circle(-5.5, 0, 1.5)");
        c.Center.X.Should().BeApproximately(-5.5, 1e-10);
        c.Center.Y.Should().BeApproximately(0, 1e-10);
        c.Radius.Should().BeApproximately(1.5, 1e-10);
    }

    [Fact]
    public void ParseCircle2D_InvalidFormat_Throws()
    {
        Action act = () => GeometryParser.ParseCircle2D("not a circle");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseSphere3D_Basic()
    {
        var s = GeometryParser.ParseSphere3D("Sphere(1.0, 2.0, 3.0, 5.0)");
        s.Center.X.Should().BeApproximately(1.0, 1e-10);
        s.Center.Y.Should().BeApproximately(2.0, 1e-10);
        s.Center.Z.Should().BeApproximately(3.0, 1e-10);
        s.Radius.Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void ParseSphere3D_InvalidFormat_Throws()
    {
        Action act = () => GeometryParser.ParseSphere3D("Sphere(1, 2)");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void RoundTrip_ParsePoint2D_FormatPoint2D()
    {
        var original = new Point2D(1.23456, 7.89012);
        string formatted = GeometryFormatter.Format(original);
        var parsed = GeometryParser.ParsePoint2D(formatted);
        parsed.X.Should().BeApproximately(original.X, 1e-6);
        parsed.Y.Should().BeApproximately(original.Y, 1e-6);
    }

    [Fact]
    public void RoundTrip_ParsePoint3D_FormatPoint3D()
    {
        var original = new Point3D(1.23456, 7.89012, 3.45678);
        string formatted = GeometryFormatter.Format(original);
        var parsed = GeometryParser.ParsePoint3D(formatted);
        parsed.X.Should().BeApproximately(original.X, 1e-6);
        parsed.Y.Should().BeApproximately(original.Y, 1e-6);
        parsed.Z.Should().BeApproximately(original.Z, 1e-6);
    }

    [Fact]
    public void RoundTrip_ParseVector3D_FormatVector3D()
    {
        var original = new Vector3D(1.111, 2.222, 3.333);
        string formatted = GeometryFormatter.Format(original);
        var parsed = GeometryParser.ParseVector3D(formatted);
        parsed.X.Should().BeApproximately(original.X, 1e-3);
        parsed.Y.Should().BeApproximately(original.Y, 1e-3);
        parsed.Z.Should().BeApproximately(original.Z, 1e-3);
    }

    [Fact]
    public void ParsePoint2D_ScientificNotation()
    {
        var p = GeometryParser.ParsePoint2D("(1.5E2, -2.5E-1)");
        p.X.Should().BeApproximately(150, 1e-10);
        p.Y.Should().BeApproximately(-0.25, 1e-10);
    }

    [Fact]
    public void ParsePoint3D_ScientificNotation()
    {
        var p = GeometryParser.ParsePoint3D("(1E1, 2E2, 3E3)");
        p.X.Should().BeApproximately(10, 1e-10);
        p.Y.Should().BeApproximately(200, 1e-10);
        p.Z.Should().BeApproximately(3000, 1e-10);
    }

    [Fact]
    public void ParsePoints2D_EmptyString()
    {
        var pts = GeometryParser.ParsePoints2D("");
        pts.Should().BeEmpty();
    }

    [Fact]
    public void ParsePoints3D_EmptyString()
    {
        var pts = GeometryParser.ParsePoints3D("");
        pts.Should().BeEmpty();
    }

    [Fact]
    public void ParseCircle2D_WithSpaces()
    {
        var c = GeometryParser.ParseCircle2D("Circle( 1.0 , 2.0 , 3.0 )");
        c.Radius.Should().BeApproximately(3.0, 1e-10);
    }

    [Fact]
    public void ParseSphere3D_NegativeValues()
    {
        var s = GeometryParser.ParseSphere3D("Sphere(-1, -2, -3, 4)");
        s.Center.X.Should().BeApproximately(-1, 1e-10);
        s.Center.Y.Should().BeApproximately(-2, 1e-10);
        s.Center.Z.Should().BeApproximately(-3, 1e-10);
        s.Radius.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void ParsePoint2D_Zero()
    {
        var p = GeometryParser.ParsePoint2D("(0, 0)");
        p.X.Should().Be(0);
        p.Y.Should().Be(0);
    }

    [Fact]
    public void ParsePoints2D_TwoPointsFormatted()
    {
        string formatted = "[(1.5, 2.5),(-3.5, 4.5)]";
        var parsed = GeometryParser.ParsePoints2D(formatted);
        parsed.Count.Should().Be(2);
        parsed[0].X.Should().BeApproximately(1.5, 1e-6);
        parsed[1].Y.Should().BeApproximately(4.5, 1e-6);
    }

    [Fact]
    public void ParsePoints3D_ThreePointsFormatted()
    {
        string formatted = "[(1, 2, 3),(4, 5, 6),(7, 8, 9)]";
        var parsed = GeometryParser.ParsePoints3D(formatted);
        parsed.Count.Should().Be(3);
    }

    [Fact]
    public void ParsePoint2D_EmptyString_Throws()
    {
        Action act = () => GeometryParser.ParsePoint2D("");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseVector3D_EmptyString_Throws()
    {
        Action act = () => GeometryParser.ParseVector3D("");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseCircle2D_EmptyString_Throws()
    {
        Action act = () => GeometryParser.ParseCircle2D("");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParseSphere3D_EmptyString_Throws()
    {
        Action act = () => GeometryParser.ParseSphere3D("");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsePoint2D_VerySmallValues()
    {
        var p = GeometryParser.ParsePoint2D("(1E-15, 2E-15)");
        p.X.Should().BeApproximately(1e-15, 1e-20);
        p.Y.Should().BeApproximately(2e-15, 1e-20);
    }

    [Fact]
    public void ParsePoint3D_VeryLargeValues()
    {
        var p = GeometryParser.ParsePoint3D("(1E15, 2E15, 3E15)");
        p.X.Should().BeApproximately(1e15, 1e5);
        p.Y.Should().BeApproximately(2e15, 1e5);
        p.Z.Should().BeApproximately(3e15, 1e5);
    }

    [Fact]
    public void ParseCircle2D_UnitCircle()
    {
        var c = GeometryParser.ParseCircle2D("Circle(0, 0, 1)");
        c.Center.X.Should().BeApproximately(0, 1e-10);
        c.Center.Y.Should().BeApproximately(0, 1e-10);
        c.Radius.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void ParseSphere3D_OriginRadiusOne()
    {
        var s = GeometryParser.ParseSphere3D("Sphere(0, 0, 0, 1)");
        s.Center.Should().Be(Point3D.Origin);
        s.Radius.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void ParsePoints2D_FourPoints()
    {
        var pts = GeometryParser.ParsePoints2D("[(0,0),(1,0),(1,1),(0,1)]");
        pts.Count.Should().Be(4);
    }

    [Fact]
    public void ParsePoints3D_TwoPoints()
    {
        var pts = GeometryParser.ParsePoints3D("[(1, 2, 3),(4, 5, 6)]");
        pts.Count.Should().Be(2);
        pts[0].Z.Should().BeApproximately(3, 1e-10);
        pts[1].Z.Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void ParseCircle2D_LargeRadius()
    {
        var c = GeometryParser.ParseCircle2D("Circle(0, 0, 999.999)");
        c.Radius.Should().BeApproximately(999.999, 1e-6);
    }
}
