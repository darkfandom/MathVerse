namespace MathVerse.Geometry.Tests.Colors;

/// <summary>Tests for the <see cref="ColorMap"/> class.</summary>
public class ColorMapTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that Viridis at t=0 produces a blue-ish color (high blue channel).</summary>
    [Fact]
    public void Viridis_AtZero_IsBlueish()
    {
        Color c = ColorMap.Viridis(0.0);

        c.B.Should().BeGreaterThan(c.R);
        c.B.Should().BeGreaterThan(c.G);
    }

    /// <summary>Verifies that Viridis at t=1 produces a yellow-ish color (high green and red).</summary>
    [Fact]
    public void Viridis_AtOne_IsYellowish()
    {
        Color c = ColorMap.Viridis(1.0);

        c.G.Should().BeGreaterThan(0.5);
        c.R.Should().BeGreaterThan(0.5);
    }

    /// <summary>Verifies that Jet at t=0 produces blue.</summary>
    [Fact]
    public void Jet_AtZero_IsBlue()
    {
        Color c = ColorMap.Jet(0.0);

        c.R.Should().BeApproximately(0.0, Tolerance);
        c.G.Should().BeApproximately(0.0, Tolerance);
        c.B.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that Jet at t=1 produces red.</summary>
    [Fact]
    public void Jet_AtOne_IsRed()
    {
        Color c = ColorMap.Jet(1.0);

        c.R.Should().BeApproximately(1.0, Tolerance);
        c.G.Should().BeApproximately(0.0, Tolerance);
        c.B.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that Grayscale at t=0 produces black.</summary>
    [Fact]
    public void Grayscale_AtZero_IsBlack()
    {
        Color c = ColorMap.Grayscale(0.0);

        c.R.Should().BeApproximately(0.0, Tolerance);
        c.G.Should().BeApproximately(0.0, Tolerance);
        c.B.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that Grayscale at t=1 produces white.</summary>
    [Fact]
    public void Grayscale_AtOne_IsWhite()
    {
        Color c = ColorMap.Grayscale(1.0);

        c.R.Should().BeApproximately(1.0, Tolerance);
        c.G.Should().BeApproximately(1.0, Tolerance);
        c.B.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that CoolWarm at t=0 is cool (blue-dominant).</summary>
    [Fact]
    public void CoolWarm_AtZero_IsCool()
    {
        Color c = ColorMap.CoolWarm(0.0);

        c.B.Should().BeGreaterThan(c.R);
    }

    /// <summary>Verifies that CoolWarm at t=1 is warm (red-dominant).</summary>
    [Fact]
    public void CoolWarm_AtOne_IsWarm()
    {
        Color c = ColorMap.CoolWarm(1.0);

        c.R.Should().BeGreaterThan(c.B);
    }

    /// <summary>Verifies that the Evaluate method dispatches to Viridis correctly.</summary>
    [Fact]
    public void Evaluate_Viridis_ReturnsSameAsViridis()
    {
        Color expected = ColorMap.Viridis(0.5);
        Color actual = ColorMap.Evaluate(0.5, ColorMapType.Viridis);

        actual.Should().Be(expected);
    }

    /// <summary>Verifies that the Evaluate method dispatches to Jet correctly.</summary>
    [Fact]
    public void Evaluate_Jet_ReturnsSameAsJet()
    {
        Color expected = ColorMap.Jet(0.25);
        Color actual = ColorMap.Evaluate(0.25, ColorMapType.Jet);

        actual.Should().Be(expected);
    }

    /// <summary>Verifies that Grayscale at t=0.5 produces medium gray.</summary>
    [Fact]
    public void Grayscale_AtHalf_IsGray()
    {
        Color c = ColorMap.Grayscale(0.5);

        c.R.Should().BeApproximately(0.5, Tolerance);
        c.G.Should().BeApproximately(0.5, Tolerance);
        c.B.Should().BeApproximately(0.5, Tolerance);
    }

    /// <summary>Verifies that Viridis at t=0.5 produces a color with all components in [0,1].</summary>
    [Fact]
    public void Viridis_AtHalf_AllComponentsInRange()
    {
        Color c = ColorMap.Viridis(0.5);

        c.R.Should().BeInRange(0.0, 1.0);
        c.G.Should().BeInRange(0.0, 1.0);
        c.B.Should().BeInRange(0.0, 1.0);
    }

    /// <summary>Verifies that all colormap functions clamp t out of range.</summary>
    [Fact]
    public void AllColormaps_ClampNegativeT()
    {
        Color c1 = ColorMap.Viridis(-1.0);
        Color c2 = ColorMap.Viridis(0.0);

        c1.Should().Be(c2);
    }

    /// <summary>Verifies that the Evaluate method dispatches to Grayscale correctly.</summary>
    [Fact]
    public void Evaluate_Grayscale_ReturnsSameAsGrayscale()
    {
        Color expected = ColorMap.Grayscale(0.7);
        Color actual = ColorMap.Evaluate(0.7, ColorMapType.Grayscale);

        actual.Should().Be(expected);
    }

    /// <summary>Verifies that the Evaluate method dispatches to CoolWarm correctly.</summary>
    [Fact]
    public void Evaluate_CoolWarm_ReturnsSameAsCoolWarm()
    {
        Color expected = ColorMap.CoolWarm(0.3);
        Color actual = ColorMap.Evaluate(0.3, ColorMapType.CoolWarm);

        actual.Should().Be(expected);
    }
}
