namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;

/// <summary>Represents a free-form annotation element.</summary>
public sealed class AnnotationElement
{
    /// <summary>Gets the text content.</summary>
    public string Text { get; init; } = "";

    /// <summary>Gets the position of the annotation.</summary>
    public Vector2 Position { get; init; }

    /// <summary>Gets the optional arrow target point.</summary>
    public Vector2? ArrowTarget { get; init; }

    /// <summary>Gets the color hex string.</summary>
    public string Color { get; init; } = "#000000";

    /// <summary>Gets the font size.</summary>
    public double FontSize { get; init; } = 12.0;

    /// <summary>Gets whether the annotation has a background.</summary>
    public bool HasBackground { get; init; }

    /// <summary>Gets the background color if HasBackground is true.</summary>
    public string BackgroundColor { get; init; } = "#FFFFFF";

    /// <summary>Gets the arrow line width.</summary>
    public double ArrowLineWidth { get; init; } = 1.5;
}

/// <summary>Creates free-form annotations for plots.</summary>
public sealed class Annotation
{
    /// <summary>Creates a simple text annotation at the specified position.</summary>
    /// <param name="text">The annotation text.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <param name="color">The color hex string.</param>
    /// <returns>An annotation element.</returns>
    public static AnnotationElement CreateSimple(string text, Vector2 position, string color = "#000000")
    {
        return new AnnotationElement
        {
            Text = text,
            Position = position,
            Color = color,
            FontSize = 12.0,
            HasBackground = false
        };
    }

    /// <summary>Creates an annotation with an arrow pointing to a target.</summary>
    /// <param name="text">The annotation text.</param>
    /// <param name="position">The text position in world coordinates.</param>
    /// <param name="arrowTarget">The arrow tip target point.</param>
    /// <param name="color">The color hex string.</param>
    /// <returns>An annotation element with an arrow.</returns>
    public static AnnotationElement CreateAnnotation(string text, Vector2 position, Vector2? arrowTarget = null, string color = "#000000")
    {
        return new AnnotationElement
        {
            Text = text,
            Position = position,
            ArrowTarget = arrowTarget,
            Color = color,
            FontSize = 12.0,
            HasBackground = arrowTarget.HasValue,
            BackgroundColor = "#FFFFFF",
            ArrowLineWidth = 1.5
        };
    }

    /// <summary>Creates a boxed annotation with background.</summary>
    /// <param name="text">The annotation text.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <param name="boxColor">The border color.</param>
    /// <param name="backgroundColor">The background color.</param>
    /// <returns>A boxed annotation element.</returns>
    public static AnnotationElement CreateBoxed(string text, Vector2 position, string boxColor = "#000000", string backgroundColor = "#FFFFFF")
    {
        return new AnnotationElement
        {
            Text = text,
            Position = position,
            Color = boxColor,
            FontSize = 12.0,
            HasBackground = true,
            BackgroundColor = backgroundColor
        };
    }

    /// <summary>Calculates the bounding rectangle for an annotation.</summary>
    /// <param name="element">The annotation element.</param>
    /// <param name="textWidth">The measured text width.</param>
    /// <returns>The bounding box as (position, size).</returns>
    public static (Vector2 Position, Vector2 Size) MeasureAnnotation(AnnotationElement element, double textWidth)
    {
        double padding = 4.0;
        double width = textWidth + padding * 2.0;
        double height = element.FontSize + padding * 2.0;

        return (element.Position, new Vector2((float)width, (float)height));
    }

    /// <summary>Generates line segments for rendering an annotation arrow.</summary>
    /// <param name="from">The start point (text position).</param>
    /// <param name="to">The end point (arrow tip).</param>
    /// <param name="arrowHeadSize">The size of the arrowhead.</param>
    /// <returns>Line segments for the arrow.</returns>
    public static System.Collections.Generic.List<(Vector2 Start, Vector2 End)> GenerateArrowLines(
        Vector2 from, Vector2 to, double arrowHeadSize = 8.0)
    {
        var lines = new System.Collections.Generic.List<(Vector2, Vector2)>();

        lines.Add((from, to));

        Vector2 direction = Vector2.Normalize(to - from);
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        Vector2 leftWing = to - direction * (float)arrowHeadSize + perpendicular * (float)(arrowHeadSize * 0.4);
        Vector2 rightWing = to - direction * (float)arrowHeadSize - perpendicular * (float)(arrowHeadSize * 0.4);

        lines.Add((to, leftWing));
        lines.Add((to, rightWing));

        return lines;
    }
}
