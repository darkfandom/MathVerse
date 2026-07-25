namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates complex function visualizations using domain coloring.</summary>
public sealed class ComplexFunctionPlot
{
    /// <summary>Creates a domain coloring visualization of a complex function.</summary>
    /// <param name="func">The complex function f(z) -> w to visualize.</param>
    /// <param name="realMin">The minimum real axis value.</param>
    /// <param name="realMax">The maximum real axis value.</param>
    /// <param name="imagMin">The minimum imaginary axis value.</param>
    /// <param name="imagMax">The maximum imaginary axis value.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A ComplexFunctionResult containing the pixel colors and metadata.</returns>
    public static ComplexFunctionResult Create(
        Func<Complex, Complex> func,
        double realMin, double realMax,
        double imagMin, double imagMax,
        int resolution = 100)
    {
        ArgumentNullException.ThrowIfNull(func);

        Vector4[,] pixels = new Vector4[resolution + 1, resolution + 1];
        double realStep = (realMax - realMin) / resolution;
        double imagStep = (imagMax - imagMin) / resolution;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double re = realMin + i * realStep;
                double im = imagMin + j * imagStep;
                Complex z = new(re, im);

                try
                {
                    Complex w = func(z);
                    pixels[j, i] = DomainColoring(w);
                }
                catch
                {
                    pixels[j, i] = new Vector4(0f, 0f, 0f, 1f);
                }
            }
        }

        return new ComplexFunctionResult
        {
            Pixels = pixels,
            RealMin = realMin,
            RealMax = realMax,
            ImagMin = imagMin,
            ImagMax = imagMax,
            Resolution = resolution
        };
    }

    /// <summary>Applies domain coloring to map a complex number to an RGBA color.</summary>
    /// <param name="w">The complex number output from the function.</param>
    /// <returns>An RGBA color where hue encodes phase and brightness encodes magnitude.</returns>
    private static Vector4 DomainColoring(Complex w)
    {
        double magnitude = w.Magnitude;
        double phase = w.Phase;

        if (double.IsNaN(magnitude) || double.IsInfinity(magnitude))
            return new Vector4(0f, 0f, 0f, 1f);

        double hue = (phase + System.Math.PI) / (2.0 * System.Math.PI);
        hue = System.Math.Clamp(hue, 0.0, 0.9999);

        double saturation = 1.0;
        double lightness = 1.0 - 1.0 / (1.0 + magnitude * 0.3);

        (double r, double g, double b) = HslToRgb(hue, saturation, lightness);

        double stripes = 1.0;
        double phaseNorm = phase / (2.0 * System.Math.PI);
        double frac = phaseNorm - System.Math.Floor(phaseNorm);
        if (frac < 0.05 || frac > 0.95)
        {
            stripes = 0.7;
        }

        r *= stripes;
        g *= stripes;
        b *= stripes;

        return new Vector4(
            (float)System.Math.Clamp(r, 0.0, 1.0),
            (float)System.Math.Clamp(g, 0.0, 1.0),
            (float)System.Math.Clamp(b, 0.0, 1.0),
            1f);
    }

    /// <summary>Converts HSL color values to RGB.</summary>
    private static (double R, double G, double B) HslToRgb(double h, double s, double l)
    {
        if (s < 1e-12)
        {
            return (l, l, l);
        }

        double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
        double p = 2.0 * l - q;

        double r = HueToRgb(p, q, h + 1.0 / 3.0);
        double g = HueToRgb(p, q, h);
        double b = HueToRgb(p, q, h - 1.0 / 3.0);

        return (r, g, b);
    }

    /// <summary>Helper for HSL to RGB conversion.</summary>
    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0.0) t += 1.0;
        if (t > 1.0) t -= 1.0;
        if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
        if (t < 0.5) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
        return p;
    }
}

/// <summary>Result of a complex function domain coloring visualization.</summary>
public sealed class ComplexFunctionResult
{
    /// <summary>Gets the 2D pixel color array indexed as [row, col] where row is the imaginary axis and col is the real axis.</summary>
    public Vector4[,] Pixels { get; init; } = new Vector4[0, 0];

    /// <summary>Gets the minimum real axis value.</summary>
    public double RealMin { get; init; }

    /// <summary>Gets the maximum real axis value.</summary>
    public double RealMax { get; init; }

    /// <summary>Gets the minimum imaginary axis value.</summary>
    public double ImagMin { get; init; }

    /// <summary>Gets the maximum imaginary axis value.</summary>
    public double ImagMax { get; init; }

    /// <summary>Gets the resolution (number of pixels per axis).</summary>
    public int Resolution { get; init; }
}
