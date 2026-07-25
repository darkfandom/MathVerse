namespace MathVerse.Math.Geometry.Colors;

/// <summary>Represents an RGBA color with components in [0,1].</summary>
public readonly record struct Color(double R, double G, double B, double A = 1.0)
{
    /// <summary>Black color (0, 0, 0, 1).</summary>
    public static readonly Color Black = new(0, 0, 0);

    /// <summary>White color (1, 1, 1, 1).</summary>
    public static readonly Color White = new(1, 1, 1);

    /// <summary>Red color (1, 0, 0, 1).</summary>
    public static readonly Color Red = new(1, 0, 0);

    /// <summary>Green color (0, 1, 0, 1).</summary>
    public static readonly Color Green = new(0, 1, 0);

    /// <summary>Blue color (0, 0, 1, 1).</summary>
    public static readonly Color Blue = new(0, 0, 1);

    /// <summary>Yellow color (1, 1, 0, 1).</summary>
    public static readonly Color Yellow = new(1, 1, 0);

    /// <summary>Cyan color (0, 1, 1, 1).</summary>
    public static readonly Color Cyan = new(0, 1, 1);

    /// <summary>Magenta color (1, 0, 1, 1).</summary>
    public static readonly Color Magenta = new(1, 0, 1);

    /// <summary>Transparent color (0, 0, 0, 0).</summary>
    public static readonly Color Transparent = new(0, 0, 0, 0);

    /// <summary>Red component in [0,1].</summary>
    public double R { get; } = System.Math.Clamp(R, 0.0, 1.0);

    /// <summary>Green component in [0,1].</summary>
    public double G { get; } = System.Math.Clamp(G, 0.0, 1.0);

    /// <summary>Blue component in [0,1].</summary>
    public double B { get; } = System.Math.Clamp(B, 0.0, 1.0);

    /// <summary>Alpha component in [0,1].</summary>
    public double A { get; } = System.Math.Clamp(A, 0.0, 1.0);

    /// <summary>Creates a color from 8-bit RGB values.</summary>
    /// <param name="r">Red channel (0-255).</param>
    /// <param name="g">Green channel (0-255).</param>
    /// <param name="b">Blue channel (0-255).</param>
    /// <returns>A new <see cref="Color"/> with the specified RGB components.</returns>
    public static Color FromRgb(byte r, byte g, byte b) => new(r / 255.0, g / 255.0, b / 255.0);

    /// <summary>Creates a color from 8-bit RGBA values.</summary>
    /// <param name="r">Red channel (0-255).</param>
    /// <param name="g">Green channel (0-255).</param>
    /// <param name="b">Blue channel (0-255).</param>
    /// <param name="a">Alpha channel (0-255).</param>
    /// <returns>A new <see cref="Color"/> with the specified RGBA components.</returns>
    public static Color FromRgba(byte r, byte g, byte b, byte a) => new(r / 255.0, g / 255.0, b / 255.0, a / 255.0);

    /// <summary>Parses a hex color string (e.g., "#FF00AA" or "#FF00AAFF").</summary>
    /// <param name="hex">The hex color string to parse.</param>
    /// <returns>A new <see cref="Color"/> parsed from the hex string.</returns>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return White;
        }

        string h = hex.TrimStart('#');

        if (h.Length == 6)
        {
            byte r = System.Convert.ToByte(h.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(h.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(h.Substring(4, 2), 16);
            return FromRgb(r, g, b);
        }

        if (h.Length == 8)
        {
            byte r = System.Convert.ToByte(h.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(h.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(h.Substring(4, 2), 16);
            byte a = System.Convert.ToByte(h.Substring(6, 2), 16);
            return FromRgba(r, g, b, a);
        }

        return White;
    }

    /// <summary>Returns a copy of this color with a different alpha value.</summary>
    /// <param name="alpha">The new alpha component in [0,1].</param>
    /// <returns>A new <see cref="Color"/> with the specified alpha.</returns>
    public Color WithAlpha(double alpha) => new(R, G, B, alpha);

    /// <summary>Linearly interpolates between this color and another.</summary>
    /// <param name="other">The target color.</param>
    /// <param name="t">Interpolation parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public Color Lerp(Color other, double t) => new(
        R + (other.R - R) * t,
        G + (other.G - G) * t,
        B + (other.B - B) * t,
        A + (other.A - A) * t);

    /// <inheritdoc />
    public override string ToString() => $"rgba({R:F3}, {G:F3}, {B:F3}, {A:F3})";
}
