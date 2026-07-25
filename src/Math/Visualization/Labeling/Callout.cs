namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;

/// <summary>Represents a callout element with text and an arrow.</summary>
public sealed class CalloutElement
{
    /// <summary>Gets the text content.</summary>
    public string Text { get; init; } = "";

    /// <summary>Gets the text box position.</summary>
    public Vector2 TextPosition { get; init; }

    /// <summary>Gets the arrow tip position.</summary>
    public Vector2 ArrowTip { get; init; }

    /// <summary>Gets the color hex string.</summary>
    public string Color { get; init; } = "#000000";

    /// <summary>Gets the background color.</summary>
    public string BackgroundColor { get; init; } = "#FFFFFF";

    /// <summary>Gets the border width.</summary>
    public double BorderWidth { get; init; } = 1.0;

    /// <summary>Gets the corner radius for rounded corners.</summary>
    public double CornerRadius { get; init; } = 4.0;

    /// <summary>Gets the font size.</summary>
    public double FontSize { get; init; } = 12.0;

    /// <summary>Gets the padding around the text.</summary>
    public double Padding { get; init; } = 6.0;

    /// <summary>Gets the arrow line width.</summary>
    public double ArrowLineWidth { get; init; } = 1.5;
}

/// <summary>Creates callout elements with text boxes and arrows.</summary>
public sealed class Callout
{
    /// <summary>Creates a callout with text and an arrow pointing to a target.</summary>
    /// <param name="text">The callout text.</param>
    /// <param name="textPosition">The text box position.</param>
    /// <param name="arrowTip">The arrow tip position.</param>
    /// <param name="color">The color hex string.</param>
    /// <returns>A callout element.</returns>
    public static CalloutElement Create(string text, Vector2 textPosition, Vector2 arrowTip, string color = "#000000")
    {
        return new CalloutElement
        {
            Text = text,
            TextPosition = textPosition,
            ArrowTip = arrowTip,
            Color = color,
            BackgroundColor = "#FFFFFF",
            BorderWidth = 1.0,
            CornerRadius = 4.0,
            FontSize = 12.0,
            Padding = 6.0,
            ArrowLineWidth = 1.5
        };
    }

    /// <summary>Creates a styled callout with custom appearance.</summary>
    /// <param name="text">The callout text.</param>
    /// <param name="textPosition">The text box position.</param>
    /// <param name="arrowTip">The arrow tip position.</param>
    /// <param name="color">The border and text color.</param>
    /// <param name="backgroundColor">The background color.</param>
    /// <param name="fontSize">The font size.</param>
    /// <returns>A styled callout element.</returns>
    public static CalloutElement CreateStyled(string text, Vector2 textPosition, Vector2 arrowTip,
        string color, string backgroundColor, double fontSize)
    {
        return new CalloutElement
        {
            Text = text,
            TextPosition = textPosition,
            ArrowTip = arrowTip,
            Color = color,
            BackgroundColor = backgroundColor,
            BorderWidth = 1.0,
            CornerRadius = 4.0,
            FontSize = fontSize,
            Padding = 6.0,
            ArrowLineWidth = 1.5
        };
    }

    /// <summary>Calculates the bounding box for the callout text box.</summary>
    /// <param name="callout">The callout element.</param>
    /// <param name="textWidth">The measured text width.</param>
    /// <returns>The bounding box as (min, max) points.</returns>
    public static (Vector2 Min, Vector2 Max) CalculateTextBoxBounds(CalloutElement callout, double textWidth)
    {
        double halfWidth = (textWidth + callout.Padding * 2.0) / 2.0;
        double halfHeight = (callout.FontSize + callout.Padding * 2.0) / 2.0;

        Vector2 min = new Vector2(
            callout.TextPosition.X - (float)halfWidth,
            callout.TextPosition.Y - (float)halfHeight);
        Vector2 max = new Vector2(
            callout.TextPosition.X + (float)halfWidth,
            callout.TextPosition.Y + (float)halfHeight);

        return (min, max);
    }

    /// <summary>Generates line segments for rendering the callout.</summary>
    /// <param name="callout">The callout element.</param>
    /// <param name="textWidth">The measured text width.</param>
    /// <returns>Line segments for the arrow and border.</returns>
    public static System.Collections.Generic.List<(Vector2 Start, Vector2 End)> GenerateRenderLines(
        CalloutElement callout, double textWidth)
    {
        var lines = new System.Collections.Generic.List<(Vector2, Vector2)>();

        var (min, max) = CalculateTextBoxBounds(callout, textWidth);

        lines.Add((new Vector2(min.X, min.Y), new Vector2(max.X, min.Y)));
        lines.Add((new Vector2(max.X, min.Y), new Vector2(max.X, max.Y)));
        lines.Add((new Vector2(max.X, max.Y), new Vector2(min.X, max.Y)));
        lines.Add((new Vector2(min.X, max.Y), new Vector2(min.X, min.Y)));

        Vector2 boxCenter = callout.TextPosition;
        Vector2 toTip = callout.ArrowTip - boxCenter;
        float distToTip = Vector2.Distance(Vector2.Zero, toTip);

        if (distToTip > 0.001f)
        {
            Vector2 exitPoint = FindBoxEdgePoint(min, max, boxCenter, callout.ArrowTip);

            lines.Add((exitPoint, callout.ArrowTip));

            Vector2 direction = Vector2.Normalize(callout.ArrowTip - exitPoint);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            double arrowSize = 8.0;

            Vector2 leftWing = callout.ArrowTip - direction * (float)arrowSize + perpendicular * (float)(arrowSize * 0.4);
            Vector2 rightWing = callout.ArrowTip - direction * (float)arrowSize - perpendicular * (float)(arrowSize * 0.4);

            lines.Add((callout.ArrowTip, leftWing));
            lines.Add((callout.ArrowTip, rightWing));
        }

        return lines;
    }

    private static Vector2 FindBoxEdgePoint(Vector2 min, Vector2 max, Vector2 center, Vector2 target)
    {
        Vector2 toTarget = target - center;
        float dx = toTarget.X;
        float dy = toTarget.Y;

        float halfW = (max.X - min.X) / 2.0f;
        float halfH = (max.Y - min.Y) / 2.0f;

        if (System.Math.Abs(dx) < 0.0001f && System.Math.Abs(dy) < 0.0001f)
            return center;

        float scaleX = dx != 0 ? halfW / System.Math.Abs(dx) : float.MaxValue;
        float scaleY = dy != 0 ? halfH / System.Math.Abs(dy) : float.MaxValue;
        float scale = System.Math.Min(scaleX, scaleY);

        return center + toTarget * scale * 0.99f;
    }
}
