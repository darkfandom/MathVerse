namespace MathVerse.Geometry.Tests.CoordinateSystems;

/// <summary>Tests for CartesianCoordinate struct.</summary>
public class CartesianCoordinateTests
{
    private const double Precision = 1e-10;

    /// <summary>ToPolar on (1, 0, 0) should give r=1 and theta=0.</summary>
    [Fact]
    public void ToPolar_OnXAxis_ShouldHaveR1Theta0()
    {
        var c = new CartesianCoordinate(1, 0, 0);
        var polar = c.ToPolar();
        polar.R.Should().BeApproximately(1.0, Precision);
        polar.Theta.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToPolar on (0, 1, 0) should give r=1 and theta=PI/2.</summary>
    [Fact]
    public void ToPolar_OnYAxis_ShouldHaveCorrectTheta()
    {
        var c = new CartesianCoordinate(0, 1, 0);
        var polar = c.ToPolar();
        polar.R.Should().BeApproximately(1.0, Precision);
        polar.Theta.Should().BeApproximately(System.Math.PI / 2, Precision);
    }

    /// <summary>ToSpherical on (0, 0, 1) should give phi=0 (north pole).</summary>
    [Fact]
    public void ToSpherical_OnZAxis_ShouldHavePhiZero()
    {
        var c = new CartesianCoordinate(0, 0, 1);
        var s = c.ToSpherical();
        s.R.Should().BeApproximately(1.0, Precision);
        s.Phi.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>ToSpherical round trip should recover original coordinates.</summary>
    [Fact]
    public void ToSpherical_RoundTrip_ShouldRecoverOriginal()
    {
        var original = new CartesianCoordinate(1, 2, 3);
        var s = original.ToSpherical();
        var back = CartesianCoordinate.FromSpherical(s);
        back.X.Should().BeApproximately(original.X, Precision);
        back.Y.Should().BeApproximately(original.Y, Precision);
        back.Z.Should().BeApproximately(original.Z, Precision);
    }

    /// <summary>ToCylindrical on (1, 0, 5) should give r=1 and z=5.</summary>
    [Fact]
    public void ToCylindrical_ShouldPreserveHeight()
    {
        var c = new CartesianCoordinate(1, 0, 5);
        var cyl = c.ToCylindrical();
        cyl.R.Should().BeApproximately(1.0, Precision);
        cyl.Z.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>ToCylindrical round trip should recover original coordinates.</summary>
    [Fact]
    public void ToCylindrical_RoundTrip_ShouldRecoverOriginal()
    {
        var original = new CartesianCoordinate(3, 4, 7);
        var cyl = original.ToCylindrical();
        var back = CartesianCoordinate.FromCylindrical(cyl);
        back.X.Should().BeApproximately(original.X, Precision);
        back.Y.Should().BeApproximately(original.Y, Precision);
        back.Z.Should().BeApproximately(original.Z, Precision);
    }

    /// <summary>ToHomogeneous should set W=1.</summary>
    [Fact]
    public void ToHomogeneous_ShouldSetWToOne()
    {
        var c = new CartesianCoordinate(1, 2, 3);
        var h = c.ToHomogeneous();
        h.W.Should().BeApproximately(1.0, Precision);
        h.X.Should().BeApproximately(1.0, Precision);
        h.Y.Should().BeApproximately(2.0, Precision);
        h.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Origin should have all zero coordinates.</summary>
    [Fact]
    public void Origin_ShouldHaveAllZeros()
    {
        var c = new CartesianCoordinate(0, 0, 0);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Origin toSpherical should give R=0.</summary>
    [Fact]
    public void Origin_ToSpherical_ShouldHaveR0()
    {
        var c = new CartesianCoordinate(0, 0, 0);
        var s = c.ToSpherical();
        s.R.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>X axis point should have polar theta=0.</summary>
    [Fact]
    public void PositiveX_ShouldHaveThetaZero()
    {
        var c = new CartesianCoordinate(5, 0, 0);
        var polar = c.ToPolar();
        polar.Theta.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Negative Y point should have polar theta=-PI/2.</summary>
    [Fact]
    public void NegativeY_ShouldHaveThetaNegPiOver2()
    {
        var c = new CartesianCoordinate(0, -1, 0);
        var polar = c.ToPolar();
        polar.Theta.Should().BeApproximately(-System.Math.PI / 2, Precision);
    }

    /// <summary>Cartesian to spherical at equator should have phi=PI/2.</summary>
    [Fact]
    public void OnXYPlane_ShouldHavePhiPiOver2()
    {
        var c = new CartesianCoordinate(1, 0, 0);
        var s = c.ToSpherical();
        s.Phi.Should().BeApproximately(System.Math.PI / 2, Precision);
    }

    /// <summary>FromSpherical with phi=0 should give point on Z axis.</summary>
    [Fact]
    public void FromSpherical_PhiZero_ShouldBeOnZAxis()
    {
        var s = new SphericalCoordinate(5, 0, 0);
        var c = CartesianCoordinate.FromSpherical(s);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>FromCylindrical with theta=0 should give point on X axis.</summary>
    [Fact]
    public void FromCylindrical_ThetaZero_ShouldBeOnXAxis()
    {
        var cyl = new CylindricalCoordinate(3, 0, 7);
        var c = CartesianCoordinate.FromCylindrical(cyl);
        c.X.Should().BeApproximately(3.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>Polar round trip through Cartesian should recover original.</summary>
    [Fact]
    public void Polar_RoundTrip_ShouldRecoverOriginal()
    {
        var original = new CartesianCoordinate(3, 4, 0);
        var polar = original.ToPolar();
        var back = polar.ToCartesian();
        back.X.Should().BeApproximately(original.X, Precision);
        back.Y.Should().BeApproximately(original.Y, Precision);
        back.Z.Should().BeApproximately(0.0, Precision);
    }
}
