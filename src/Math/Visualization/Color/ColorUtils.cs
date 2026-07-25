namespace MathVerse.Math.Visualization.Color;

/// <summary>Color utility functions for conversion, interpolation, and mapping.</summary>
public sealed class ColorUtils
{
    /// <summary>
    /// Parses a hex color string to RGBA components (0-1).
    /// Supports #RGB, #RGBA, #RRGGBB, #RRGGBBAA formats.
    /// </summary>
    /// <param name="hex">Hex color string (with or without # prefix).</param>
    /// <returns>RGBA components (0-1).</returns>
    public static (double R, double G, double B, double A) FromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return (0.0, 0.0, 0.0, 1.0);

        hex = hex.TrimStart('#');

        return hex.Length switch
        {
            3 => (
                ParseHexByte(hex[0], hex[0]) / 255.0,
                ParseHexByte(hex[1], hex[1]) / 255.0,
                ParseHexByte(hex[2], hex[2]) / 255.0,
                1.0),
            4 => (
                ParseHexByte(hex[0], hex[0]) / 255.0,
                ParseHexByte(hex[1], hex[1]) / 255.0,
                ParseHexByte(hex[2], hex[2]) / 255.0,
                ParseHexByte(hex[3], hex[3]) / 255.0),
            6 => (
                ParseHexByte(hex[0], hex[1]) / 255.0,
                ParseHexByte(hex[2], hex[3]) / 255.0,
                ParseHexByte(hex[4], hex[5]) / 255.0,
                1.0),
            8 => (
                ParseHexByte(hex[0], hex[1]) / 255.0,
                ParseHexByte(hex[2], hex[3]) / 255.0,
                ParseHexByte(hex[4], hex[5]) / 255.0,
                ParseHexByte(hex[6], hex[7]) / 255.0),
            _ => (0.0, 0.0, 0.0, 1.0)
        };
    }

    /// <summary>
    /// Converts RGBA components (0-1) to a hex color string.
    /// </summary>
    /// <param name="r">Red component (0-1).</param>
    /// <param name="g">Green component (0-1).</param>
    /// <param name="b">Blue component (0-1).</param>
    /// <param name="a">Alpha component (0-1).</param>
    /// <returns>Hex string in #RRGGBBAA format.</returns>
    public static string ToHex(double r, double g, double b, double a = 1.0)
    {
        int ri = System.Math.Clamp((int)System.Math.Round(r * 255.0), 0, 255);
        int gi = System.Math.Clamp((int)System.Math.Round(g * 255.0), 0, 255);
        int bi = System.Math.Clamp((int)System.Math.Round(b * 255.0), 0, 255);
        int ai = System.Math.Clamp((int)System.Math.Round(a * 255.0), 0, 255);

        if (ai == 255)
            return $"#{ri:X2}{gi:X2}{bi:X2}";
        return $"#{ri:X2}{gi:X2}{bi:X2}{ai:X2}";
    }

    /// <summary>
    /// Converts HSV color values to RGB.
    /// </summary>
    /// <param name="h">Hue (0-1, where 0=red, 0.33=green, 0.67=blue).</param>
    /// <param name="s">Saturation (0-1).</param>
    /// <param name="v">Value (brightness) (0-1).</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) FromHSV(double h, double s, double v)
    {
        h = h - System.Math.Floor(h);
        s = System.Math.Clamp(s, 0.0, 1.0);
        v = System.Math.Clamp(v, 0.0, 1.0);

        double c = v * s;
        double x = c * (1.0 - System.Math.Abs((h * 6.0) % 2.0 - 1.0));
        double m = v - c;

        double r1, g1, b1;
        int sector = (int)(h * 6.0);
        switch (sector)
        {
            case 0: r1 = c; g1 = x; b1 = 0.0; break;
            case 1: r1 = x; g1 = c; b1 = 0.0; break;
            case 2: r1 = 0.0; g1 = c; b1 = x; break;
            case 3: r1 = 0.0; g1 = x; b1 = c; break;
            case 4: r1 = x; g1 = 0.0; b1 = c; break;
            default: r1 = c; g1 = 0.0; b1 = x; break;
        }

        return (r1 + m, g1 + m, b1 + m);
    }

    /// <summary>
    /// Converts RGB color values to HSV.
    /// </summary>
    /// <param name="r">Red component (0-1).</param>
    /// <param name="g">Green component (0-1).</param>
    /// <param name="b">Blue component (0-1).</param>
    /// <returns>HSV values (h: 0-1, s: 0-1, v: 0-1).</returns>
    public static (double H, double S, double V) ToHSV(double r, double g, double b)
    {
        r = System.Math.Clamp(r, 0.0, 1.0);
        g = System.Math.Clamp(g, 0.0, 1.0);
        b = System.Math.Clamp(b, 0.0, 1.0);

        double max = System.Math.Max(r, System.Math.Max(g, b));
        double min = System.Math.Min(r, System.Math.Min(g, b));
        double delta = max - min;

        double v = max;
        double s = max > 1e-15 ? delta / max : 0.0;

        double h;
        if (delta < 1e-15)
        {
            h = 0.0;
        }
        else if (max == r)
        {
            h = (g - b) / delta;
            if (h < 0.0) h += 6.0;
        }
        else if (max == g)
        {
            h = (b - r) / delta + 2.0;
        }
        else
        {
            h = (r - g) / delta + 4.0;
        }

        h /= 6.0;
        h = h - System.Math.Floor(h);

        return (h, s, v);
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="a">Start color.</param>
    /// <param name="b">End color.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated color (R, G, B, A).</returns>
    public static (double R, double G, double B, double A) Lerp(
        (double R, double G, double B, double A) a,
        (double R, double G, double B, double A) b,
        double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        return (
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    /// <summary>
    /// Maps a value to 0-1 using logarithmic scaling.
    /// Useful for data with large dynamic range.
    /// </summary>
    /// <param name="value">Raw value (must be > 0).</param>
    /// <param name="min">Minimum of the data range (must be > 0).</param>
    /// <param name="max">Maximum of the data range (must be > 0).</param>
    /// <returns>Mapped value (0-1).</returns>
    public static double LogarithmicMap(double value, double min, double max)
    {
        if (min <= 0.0) min = 1e-15;
        if (max <= 0.0) max = 1e-15;
        if (value <= 0.0) value = min;

        double logMin = System.Math.Log(min);
        double logMax = System.Math.Log(max);
        double logVal = System.Math.Log(value);
        double range = logMax - logMin;

        if (range < 1e-15) return 0.0;

        double result = (logVal - logMin) / range;
        return System.Math.Clamp(result, 0.0, 1.0);
    }

    /// <summary>
    /// Normalizes a value to the 0-1 range with clamping.
    /// </summary>
    /// <param name="value">Raw value.</param>
    /// <param name="min">Minimum of the range.</param>
    /// <param name="max">Maximum of the range.</param>
    /// <returns>Normalized value (0-1).</returns>
    public static double Normalize(double value, double min, double max)
    {
        double range = max - min;
        if (range < 1e-15) return 0.0;

        double result = (value - min) / range;
        return System.Math.Clamp(result, 0.0, 1.0);
    }

    /// <summary>
    /// Maps a 0-1 value to a heat color: black → red → yellow → white.
    /// </summary>
    /// <param name="t">Value from 0 to 1.</param>
    /// <returns>RGB components (0-1).</returns>
    public static (double R, double G, double B) HeatColor(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        if (t < 0.25)
        {
            double s = t / 0.25;
            return (s * 0.8, 0.0, 0.0);
        }
        else if (t < 0.5)
        {
            double s = (t - 0.25) / 0.25;
            return (0.8 + s * 0.2, s * 0.8, 0.0);
        }
        else if (t < 0.75)
        {
            double s = (t - 0.5) / 0.25;
            return (1.0, 0.8 + s * 0.2, s * 0.6);
        }
        else
        {
            double s = (t - 0.75) / 0.25;
            return (1.0, 1.0, 0.6 + s * 0.4);
        }
    }

    private static int ParseHexByte(char high, char low)
    {
        return HexDigit(high) * 16 + HexDigit(low);
    }

    private static int HexDigit(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => 0
        };
    }
}
