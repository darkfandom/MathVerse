namespace MathVerse.Geometry.Tests.Colors;

/// <summary>Tests for the <see cref="ColorPalette"/> class.</summary>
public class ColorPaletteTests
{
    /// <summary>Verifies that the Default palette contains 10 colors.</summary>
    [Fact]
    public void Default_HasTenColors()
    {
        ColorPalette.Default.Count.Should().Be(10);
    }

    /// <summary>Verifies that the Pastel palette contains 10 colors.</summary>
    [Fact]
    public void Pastel_HasTenColors()
    {
        ColorPalette.Pastel.Count.Should().Be(10);
    }

    /// <summary>Verifies that the Bold palette contains 10 colors.</summary>
    [Fact]
    public void Bold_HasTenColors()
    {
        ColorPalette.Bold.Count.Should().Be(10);
    }

    /// <summary>Verifies that GetColor with an in-range index returns a valid color.</summary>
    [Fact]
    public void GetColor_InRange_ReturnsColor()
    {
        Color c = ColorPalette.Default.GetColor(0);

        c.R.Should().BeInRange(0.0, 1.0);
        c.G.Should().BeInRange(0.0, 1.0);
        c.B.Should().BeInRange(0.0, 1.0);
    }

    /// <summary>Verifies that GetColor returns the first color at index 0.</summary>
    [Fact]
    public void GetColor_FirstIndex_ReturnsFirstColor()
    {
        Color c = ColorPalette.Default.GetColor(0);

        c.R.Should().BeApproximately(0.12, 1e-10);
        c.G.Should().BeApproximately(0.47, 1e-10);
        c.B.Should().BeApproximately(0.71, 1e-10);
    }

    /// <summary>Verifies that GetColor returns the last color at index Count-1.</summary>
    [Fact]
    public void GetColor_LastIndex_ReturnsLastColor()
    {
        Color c = ColorPalette.Default.GetColor(9);

        c.R.Should().BeApproximately(0.83, 1e-10);
        c.G.Should().BeApproximately(0.33, 1e-10);
        c.B.Should().BeApproximately(0.36, 1e-10);
    }

    /// <summary>Verifies that GetColor wraps around when index exceeds Count.</summary>
    [Fact]
    public void GetColor_OutOfRange_WrapsAround()
    {
        Color c = ColorPalette.Default.GetColor(10);

        Color first = ColorPalette.Default.GetColor(0);
        c.Should().Be(first);
    }

    /// <summary>Verifies that GetColor handles negative indices by wrapping.</summary>
    [Fact]
    public void GetColor_NegativeIndex_WrapsAround()
    {
        Color c = ColorPalette.Default.GetColor(-1);

        Color last = ColorPalette.Default.GetColor(9);
        c.Should().Be(last);
    }
}
