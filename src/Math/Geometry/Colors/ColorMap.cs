namespace MathVerse.Math.Geometry.Colors;

/// <summary>Provides scientific colormaps for data visualization.</summary>
public sealed class ColorMap
{
    /// <summary>Evaluates the Viridis colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Viridis(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r = System.Math.Clamp(0.267 + 2.05 * clamped - 4.36 * clamped * clamped + 3.79 * clamped * clamped * clamped, 0.0, 1.0);
        double g = System.Math.Clamp(0.004 + 1.27 * clamped + 0.48 * clamped * clamped - 0.82 * clamped * clamped * clamped, 0.0, 1.0);
        double b = System.Math.Clamp(0.329 + 2.44 * clamped - 7.13 * clamped * clamped + 5.72 * clamped * clamped * clamped, 0.0, 1.0);
        return new Color(r, g, b);
    }

    /// <summary>Evaluates the Inferno colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Inferno(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r = System.Math.Clamp(0.05 + 1.8 * clamped - 0.6 * clamped * clamped + 0.05 * clamped * clamped * clamped, 0.0, 1.0);
        double g = System.Math.Clamp(0.02 + 0.15 * clamped + 1.6 * clamped * clamped - 1.4 * clamped * clamped * clamped, 0.0, 1.0);
        double b = System.Math.Clamp(0.3 + 1.5 * clamped - 4.2 * clamped * clamped + 3.2 * clamped * clamped * clamped, 0.0, 1.0);
        return new Color(r, g, b);
    }

    /// <summary>Evaluates the Plasma colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Plasma(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r = System.Math.Clamp(0.05 + 1.5 * clamped + 0.2 * clamped * clamped - 0.5 * clamped * clamped * clamped, 0.0, 1.0);
        double g = System.Math.Clamp(0.02 - 0.7 * clamped + 3.5 * clamped * clamped - 2.5 * clamped * clamped * clamped, 0.0, 1.0);
        double b = System.Math.Clamp(0.5 + 1.5 * clamped - 5.0 * clamped * clamped + 4.0 * clamped * clamped * clamped, 0.0, 1.0);
        return new Color(r, g, b);
    }

    /// <summary>Evaluates the Magma colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Magma(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r = System.Math.Clamp(0.05 + 1.8 * clamped - 0.8 * clamped * clamped + 0.1 * clamped * clamped * clamped, 0.0, 1.0);
        double g = System.Math.Clamp(0.01 + 0.1 * clamped + 1.2 * clamped * clamped - 1.0 * clamped * clamped * clamped, 0.0, 1.0);
        double b = System.Math.Clamp(0.15 + 1.6 * clamped - 3.5 * clamped * clamped + 2.5 * clamped * clamped * clamped, 0.0, 1.0);
        return new Color(r, g, b);
    }

    /// <summary>Evaluates the Jet colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Jet(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r;
        double g;
        double b;

        if (clamped < 0.25)
        {
            r = 0.0;
            g = clamped * 4.0;
            b = 1.0;
        }
        else if (clamped < 0.5)
        {
            r = 0.0;
            g = 1.0;
            b = 2.0 - clamped * 4.0;
        }
        else if (clamped < 0.75)
        {
            r = clamped * 4.0 - 2.0;
            g = 1.0;
            b = 0.0;
        }
        else
        {
            r = 1.0;
            g = 4.0 - clamped * 4.0;
            b = 0.0;
        }

        return new Color(r, g, b);
    }

    /// <summary>Evaluates the Grayscale colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Grayscale(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        return new Color(clamped, clamped, clamped);
    }

    /// <summary>Evaluates the CoolWarm diverging colormap at the specified parameter.</summary>
    /// <param name="t">Parameter in [0,1] where 0 is cool (blue) and 1 is warm (red).</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color CoolWarm(double t)
    {
        double clamped = System.Math.Clamp(t, 0.0, 1.0);
        double r;
        double g;
        double b;

        if (clamped < 0.5)
        {
            double u = clamped * 2.0;
            r = System.Math.Clamp(0.59 + 0.4 * u, 0.0, 1.0);
            g = System.Math.Clamp(0.58 - 0.18 * u, 0.0, 1.0);
            b = System.Math.Clamp(0.88 - 0.2 * u, 0.0, 1.0);
        }
        else
        {
            double u = (clamped - 0.5) * 2.0;
            r = System.Math.Clamp(0.99 - 0.15 * u, 0.0, 1.0);
            g = System.Math.Clamp(0.4 + 0.18 * u, 0.0, 1.0);
            b = System.Math.Clamp(0.68 - 0.56 * u, 0.0, 1.0);
        }

        return new Color(r, g, b);
    }

    /// <summary>Evaluates the specified colormap at the given parameter.</summary>
    /// <param name="t">Parameter in [0,1].</param>
    /// <param name="type">The colormap type to evaluate.</param>
    /// <returns>The interpolated <see cref="Color"/>.</returns>
    public static Color Evaluate(double t, ColorMapType type)
    {
        return type switch
        {
            ColorMapType.Viridis => Viridis(t),
            ColorMapType.Inferno => Inferno(t),
            ColorMapType.Plasma => Plasma(t),
            ColorMapType.Magma => Magma(t),
            ColorMapType.Jet => Jet(t),
            ColorMapType.Grayscale => Grayscale(t),
            ColorMapType.CoolWarm => CoolWarm(t),
            _ => Grayscale(t)
        };
    }
}
