namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;

/// <summary>Defines the position of an axis label.</summary>
public enum AxisPosition
{
    /// <summary>Bottom axis (X-axis).</summary>
    Bottom,

    /// <summary>Top axis.</summary>
    Top,

    /// <summary>Left axis (Y-axis).</summary>
    Left,

    /// <summary>Right axis.</summary>
    Right
}

/// <summary>Creates axis labels for plot axes.</summary>
public sealed class AxisLabel
{
    /// <summary>Creates an axis label with the specified text and position.</summary>
    /// <param name="text">The label text.</param>
    /// <param name="position">The axis position.</param>
    /// <param name="offset">The offset from the axis in pixels.</param>
    /// <returns>A configured MathLabel for the axis.</returns>
    public static MathLabel CreateAxisLabel(string text, AxisPosition position, double offset = 0.0)
    {
        Vector2 labelPosition;
        double rotation = 0.0;
        TextAlignment alignment = TextAlignment.Center;

        switch (position)
        {
            case AxisPosition.Bottom:
                labelPosition = new Vector2(0.5f, (float)(1.0 + offset / 100.0));
                alignment = TextAlignment.Center;
                break;
            case AxisPosition.Top:
                labelPosition = new Vector2(0.5f, (float)(0.0 - offset / 100.0));
                alignment = TextAlignment.Center;
                break;
            case AxisPosition.Left:
                labelPosition = new Vector2((float)(0.0 - offset / 100.0), 0.5f);
                rotation = System.Math.PI / 2.0;
                alignment = TextAlignment.Center;
                break;
            case AxisPosition.Right:
                labelPosition = new Vector2((float)(1.0 + offset / 100.0), 0.5f);
                rotation = -System.Math.PI / 2.0;
                alignment = TextAlignment.Center;
                break;
            default:
                labelPosition = new Vector2(0.5f, 1.0f);
                break;
        }

        return new MathLabel
        {
            Text = text,
            Position = labelPosition,
            Rotation = rotation,
            FontFamily = "sans-serif",
            FontSize = 12.0,
            Color = "#333333",
            Alignment = alignment,
            IsLatex = false
        };
    }

    /// <summary>Creates a styled axis label with custom formatting.</summary>
    /// <param name="text">The label text.</param>
    /// <param name="position">The axis position.</param>
    /// <param name="offset">The offset from the axis.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="color">The color hex string.</param>
    /// <returns>A configured MathLabel.</returns>
    public static MathLabel CreateStyledAxisLabel(string text, AxisPosition position, double offset, double fontSize, string color)
    {
        var baseLabel = CreateAxisLabel(text, position, offset);
        return baseLabel with
        {
            FontSize = fontSize,
            Color = color
        };
    }
}
