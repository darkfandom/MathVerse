using System.Collections.Immutable;

namespace MathVerse.Math.Geometry.Colors;

/// <summary>Provides a collection of named colors for use in charts and plots.</summary>
public sealed class ColorPalette
{
    private readonly ImmutableArray<Color> _colors;

    /// <summary>Initializes a new instance of the <see cref="ColorPalette"/> class.</summary>
    /// <param name="colors">The collection of colors in the palette.</param>
    public ColorPalette(ImmutableArray<Color> colors)
    {
        _colors = colors;
    }

    /// <summary>Gets the default palette with 10 distinguishable colors.</summary>
    public static ColorPalette Default { get; } = new(ImmutableArray.Create(
        new Color(0.12, 0.47, 0.71),
        new Color(0.84, 0.15, 0.16),
        new Color(0.16, 0.50, 0.34),
        new Color(0.58, 0.40, 0.74),
        new Color(1.00, 0.50, 0.05),
        new Color(0.17, 0.63, 0.79),
        new Color(0.87, 0.55, 0.13),
        new Color(0.55, 0.23, 0.51),
        new Color(0.34, 0.69, 0.31),
        new Color(0.83, 0.33, 0.36)
    ));

    /// <summary>Gets a pastel palette with soft, muted colors.</summary>
    public static ColorPalette Pastel { get; } = new(ImmutableArray.Create(
        new Color(0.78, 0.85, 0.93),
        new Color(0.98, 0.80, 0.80),
        new Color(0.80, 0.92, 0.82),
        new Color(0.88, 0.82, 0.93),
        new Color(1.00, 0.92, 0.75),
        new Color(0.76, 0.90, 0.93),
        new Color(0.97, 0.87, 0.75),
        new Color(0.87, 0.78, 0.87),
        new Color(0.80, 0.93, 0.80),
        new Color(0.95, 0.82, 0.82)
    ));

    /// <summary>Gets a bold palette with strong, saturated colors.</summary>
    public static ColorPalette Bold { get; } = new(ImmutableArray.Create(
        new Color(0.00, 0.45, 0.70),
        new Color(0.84, 0.15, 0.16),
        new Color(0.00, 0.62, 0.45),
        new Color(0.58, 0.40, 0.74),
        new Color(0.99, 0.55, 0.01),
        new Color(0.12, 0.47, 0.71),
        new Color(0.65, 0.35, 0.00),
        new Color(0.40, 0.00, 0.60),
        new Color(0.00, 0.55, 0.20),
        new Color(0.80, 0.00, 0.20)
    ));

    /// <summary>Gets the number of colors in the palette.</summary>
    public int Count => _colors.Length;

    /// <summary>Gets the color at the specified index.</summary>
    /// <param name="index">The zero-based index of the color to retrieve.</param>
    /// <returns>The <see cref="Color"/> at the specified index.</returns>
    public Color GetColor(int index)
    {
        int wrappedIndex = index % Count;
        if (wrappedIndex < 0)
        {
            wrappedIndex += Count;
        }
        return _colors[wrappedIndex];
    }
}
