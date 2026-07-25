using Avalonia;
using Avalonia.Controls;

namespace MathVerse.Desktop.Views.Controls;

public partial class ToolbarButton : Button
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<ToolbarButton, string>(nameof(Glyph));

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public ToolbarButton()
    {
        InitializeComponent();
    }
}
