namespace MathVerse.Geometry.Tests.Colors;

/// <summary>Tests for the <see cref="Color"/> struct.</summary>
public class ColorTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that constructing a color stores the provided RGBA values.</summary>
    [Fact]
    public void Constructor_StoresValues()
    {
        var c = new Color(0.5, 0.6, 0.7, 0.8);

        c.R.Should().BeApproximately(0.5, Tolerance);
        c.G.Should().BeApproximately(0.6, Tolerance);
        c.B.Should().BeApproximately(0.7, Tolerance);
        c.A.Should().BeApproximately(0.8, Tolerance);
    }

    /// <summary>Verifies that default alpha is 1.0 when omitted.</summary>
    [Fact]
    public void Constructor_DefaultAlpha_IsOne()
    {
        var c = new Color(0.1, 0.2, 0.3);

        c.A.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that R is clamped to [0,1] when exceeding upper bound.</summary>
    [Fact]
    public void R_ClampedUpper()
    {
        var c = new Color(2.0, 0.5, 0.5);

        c.R.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that G is clamped to [0,1] when below lower bound.</summary>
    [Fact]
    public void G_ClampedLower()
    {
        var c = new Color(0.5, -1.0, 0.5);

        c.G.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that B is clamped to [0,1] when exceeding upper bound.</summary>
    [Fact]
    public void B_ClampedUpper()
    {
        var c = new Color(0.5, 0.5, 3.0);

        c.B.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that A is clamped to [0,1] when below lower bound.</summary>
    [Fact]
    public void A_ClampedLower()
    {
        var c = new Color(0.5, 0.5, 0.5, -0.5);

        c.A.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that FromRgb converts 8-bit values to [0,1] range.</summary>
    [Fact]
    public void FromRgb_ConvertsCorrectly()
    {
        var c = Color.FromRgb(128, 64, 255);

        c.R.Should().BeApproximately(128.0 / 255.0, Tolerance);
        c.G.Should().BeApproximately(64.0 / 255.0, Tolerance);
        c.B.Should().BeApproximately(1.0, Tolerance);
        c.A.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that FromRgba converts 8-bit RGBA values correctly.</summary>
    [Fact]
    public void FromRgba_ConvertsCorrectly()
    {
        var c = Color.FromRgba(255, 0, 128, 64);

        c.R.Should().BeApproximately(1.0, Tolerance);
        c.G.Should().BeApproximately(0.0, Tolerance);
        c.B.Should().BeApproximately(128.0 / 255.0, Tolerance);
        c.A.Should().BeApproximately(64.0 / 255.0, Tolerance);
    }

    /// <summary>Verifies that the Black constant has correct values.</summary>
    [Fact]
    public void Black_IsCorrect()
    {
        Color.Black.R.Should().BeApproximately(0.0, Tolerance);
        Color.Black.G.Should().BeApproximately(0.0, Tolerance);
        Color.Black.B.Should().BeApproximately(0.0, Tolerance);
        Color.Black.A.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that the White constant has correct values.</summary>
    [Fact]
    public void White_IsCorrect()
    {
        Color.White.R.Should().BeApproximately(1.0, Tolerance);
        Color.White.G.Should().BeApproximately(1.0, Tolerance);
        Color.White.B.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that the Red constant has correct values.</summary>
    [Fact]
    public void Red_IsCorrect()
    {
        Color.Red.R.Should().BeApproximately(1.0, Tolerance);
        Color.Red.G.Should().BeApproximately(0.0, Tolerance);
        Color.Red.B.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that the Green constant has correct values.</summary>
    [Fact]
    public void Green_IsCorrect()
    {
        Color.Green.R.Should().BeApproximately(0.0, Tolerance);
        Color.Green.G.Should().BeApproximately(1.0, Tolerance);
        Color.Green.B.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that the Blue constant has correct values.</summary>
    [Fact]
    public void Blue_IsCorrect()
    {
        Color.Blue.R.Should().BeApproximately(0.0, Tolerance);
        Color.Blue.G.Should().BeApproximately(0.0, Tolerance);
        Color.Blue.B.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies that WithAlpha returns a color with the specified alpha.</summary>
    [Fact]
    public void WithAlpha_ReturnsNewColor()
    {
        var c = Color.Red.WithAlpha(0.5);

        c.R.Should().BeApproximately(1.0, Tolerance);
        c.G.Should().BeApproximately(0.0, Tolerance);
        c.B.Should().BeApproximately(0.0, Tolerance);
        c.A.Should().BeApproximately(0.5, Tolerance);
    }

    /// <summary>Verifies that Lerp at t=0 returns the start color.</summary>
    [Fact]
    public void Lerp_AtZero_ReturnsStart()
    {
        var result = Color.Black.Lerp(Color.White, 0.0);

        result.Should().Be(Color.Black);
    }

    /// <summary>Verifies that Lerp at t=1 returns the end color.</summary>
    [Fact]
    public void Lerp_AtOne_ReturnsEnd()
    {
        var result = Color.Black.Lerp(Color.White, 1.0);

        result.Should().Be(Color.White);
    }

    /// <summary>Verifies that Lerp at t=0.5 returns the midpoint color.</summary>
    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var result = Color.Black.Lerp(Color.White, 0.5);

        result.R.Should().BeApproximately(0.5, Tolerance);
        result.G.Should().BeApproximately(0.5, Tolerance);
        result.B.Should().BeApproximately(0.5, Tolerance);
    }

    /// <summary>Verifies that two colors with the same values are equal.</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new Color(0.5, 0.5, 0.5);
        var b = new Color(0.5, 0.5, 0.5);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    /// <summary>Verifies that ToString produces the expected format.</summary>
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var c = new Color(1.0, 0.0, 0.5, 0.8);

        string result = c.ToString();

        result.Should().Be("rgba(1.000, 0.000, 0.500, 0.800)");
    }

    /// <summary>Verifies that the Transparent constant has zero alpha.</summary>
    [Fact]
    public void Transparent_IsCorrect()
    {
        Color.Transparent.A.Should().BeApproximately(0.0, Tolerance);
        Color.Transparent.R.Should().BeApproximately(0.0, Tolerance);
        Color.Transparent.G.Should().BeApproximately(0.0, Tolerance);
        Color.Transparent.B.Should().BeApproximately(0.0, Tolerance);
    }
}
