namespace MathVerse.Math.Visualization.Labeling;
using System.Numerics;

/// <summary>Label for mathematical annotations on plots.</summary>
public sealed record class MathLabel
{
    /// <summary>Gets the text content of the label.</summary>
    public string Text { get; init; } = "";

    /// <summary>Gets the position of the label in world coordinates.</summary>
    public Vector2 Position { get; init; }

    /// <summary>Gets the rotation angle in radians.</summary>
    public double Rotation { get; init; }

    /// <summary>Gets the font family name.</summary>
    public string FontFamily { get; init; } = "serif";

    /// <summary>Gets the font size in points.</summary>
    public double FontSize { get; init; } = 14.0;

    /// <summary>Gets the color as a hex string.</summary>
    public string Color { get; init; } = "#000000";

    /// <summary>Gets the text alignment.</summary>
    public TextAlignment Alignment { get; init; }

    /// <summary>Gets whether the text is LaTeX formatted.</summary>
    public bool IsLatex { get; init; }
}

/// <summary>Defines text alignment options.</summary>
public enum TextAlignment
{
    /// <summary>Left-aligned text.</summary>
    Left,

    /// <summary>Center-aligned text.</summary>
    Center,

    /// <summary>Right-aligned text.</summary>
    Right
}
