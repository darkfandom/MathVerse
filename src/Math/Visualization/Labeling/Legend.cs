namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;

/// <summary>Defines the visual style of a legend item.</summary>
public enum LegendStyle
{
    /// <summary>Solid colored line.</summary>
    SolidLine,

    /// <summary>Dashed line.</summary>
    DashedLine,

    /// <summary>Dotted line.</summary>
    DottedLine,

    /// <summary>Filled circle marker.</summary>
    FilledCircle,

    /// <summary>Open circle marker.</summary>
    OpenCircle,

    /// <summary>Square marker.</summary>
    Square,

    /// <summary>Triangle marker.</summary>
    Triangle,

    /// <summary>Filled rectangle for area plots.</summary>
    FilledRectangle
}

/// <summary>Represents a rendered legend element (shape or text).</summary>
public sealed class LegendElement
{
    /// <summary>Gets the type of element.</summary>
    public LegendElementType ElementType { get; init; }

    /// <summary>Gets the position in world coordinates.</summary>
    public Vector2 Position { get; init; }

    /// <summary>Gets the size of the element.</summary>
    public Vector2 Size { get; init; }

    /// <summary>Gets the color hex string.</summary>
    public string Color { get; init; } = "#000000";

    /// <summary>Gets the text content for text elements.</summary>
    public string? Text { get; init; }

    /// <summary>Gets the style for shape elements.</summary>
    public LegendStyle? Style { get; init; }
}

/// <summary>Defines types of legend elements.</summary>
public enum LegendElementType
{
    /// <summary>Line or shape element.</summary>
    Shape,

    /// <summary>Text label element.</summary>
    Text
}

/// <summary>Creates legend entries for plot visualization.</summary>
public sealed class Legend
{
    private const double ItemSpacing = 5.0;
    private const double LineWidth = 20.0;
    private const double LineHeight = 2.0;
    private const double MarkerSize = 6.0;
    private const double TextOffset = 8.0;
    private const double FontSize = 11.0;
    private const double Padding = 8.0;

    /// <summary>Creates legend elements for the given items at the specified position.</summary>
    /// <param name="items">The legend items with labels, colors, and styles.</param>
    /// <param name="position">The top-left position of the legend.</param>
    /// <returns>A list of elements to render.</returns>
    public static System.Collections.Generic.List<LegendElement> Create(
        System.Collections.Generic.List<(string label, string color, LegendStyle style)> items,
        Vector2 position)
    {
        var elements = new System.Collections.Generic.List<LegendElement>();

        if (items.Count == 0)
            return elements;

        double currentY = position.Y;
        double maxWidth = 0.0;

        foreach (var item in items)
        {
            double shapeX = position.X + Padding;
            double shapeY = currentY + FontSize / 2.0;
            double textX = shapeX + LineWidth + TextOffset;

            AddLegendShape(elements, item.style, item.color, shapeX, shapeY);

            elements.Add(new LegendElement
            {
                ElementType = LegendElementType.Text,
                Position = new Vector2((float)textX, (float)currentY),
                Size = new Vector2(100.0f, (float)FontSize),
                Color = "#333333",
                Text = item.label
            });

            double itemWidth = textX - position.X + 100.0;
            if (itemWidth > maxWidth)
                maxWidth = itemWidth;

            currentY += FontSize + ItemSpacing;
        }

        elements.Insert(0, new LegendElement
        {
            ElementType = LegendElementType.Shape,
            Position = position,
            Size = new Vector2((float)(maxWidth + Padding * 2), (float)(currentY - position.Y + Padding)),
            Color = "#FFFFFF",
            Style = LegendStyle.FilledRectangle
        });

        return elements;
    }

    /// <summary>Calculates the bounding box for a legend.</summary>
    /// <param name="items">The legend items.</param>
    /// <param name="position">The top-left position.</param>
    /// <returns>The width and height of the legend.</returns>
    public static Vector2 MeasureLegend(
        System.Collections.Generic.List<(string label, string color, LegendStyle style)> items,
        Vector2 position)
    {
        if (items.Count == 0)
            return Vector2.Zero;

        double height = items.Count * (FontSize + ItemSpacing) + Padding * 2;
        double width = 150.0;

        return new Vector2((float)width, (float)height);
    }

    private static void AddLegendShape(System.Collections.Generic.List<LegendElement> elements, LegendStyle style, string color, double x, double y)
    {
        switch (style)
        {
            case LegendStyle.SolidLine:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)x, (float)(y - LineHeight / 2.0)),
                    Size = new Vector2((float)LineWidth, (float)LineHeight),
                    Color = color,
                    Style = style
                });
                break;

            case LegendStyle.DashedLine:
            case LegendStyle.DottedLine:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)x, (float)(y - LineHeight / 2.0)),
                    Size = new Vector2((float)LineWidth, (float)LineHeight),
                    Color = color,
                    Style = style
                });
                break;

            case LegendStyle.FilledCircle:
            case LegendStyle.OpenCircle:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)(x + LineWidth / 2.0), (float)y),
                    Size = new Vector2((float)MarkerSize, (float)MarkerSize),
                    Color = color,
                    Style = style
                });
                break;

            case LegendStyle.Square:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)(x + LineWidth / 2.0 - MarkerSize / 2.0), (float)(y - MarkerSize / 2.0)),
                    Size = new Vector2((float)MarkerSize, (float)MarkerSize),
                    Color = color,
                    Style = style
                });
                break;

            case LegendStyle.Triangle:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)(x + LineWidth / 2.0), (float)y),
                    Size = new Vector2((float)MarkerSize, (float)MarkerSize),
                    Color = color,
                    Style = style
                });
                break;

            case LegendStyle.FilledRectangle:
                elements.Add(new LegendElement
                {
                    ElementType = LegendElementType.Shape,
                    Position = new Vector2((float)x, (float)(y - MarkerSize / 2.0)),
                    Size = new Vector2((float)LineWidth, (float)MarkerSize),
                    Color = color,
                    Style = style
                });
                break;
        }
    }
}
