using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MathVerse.Desktop.Views.Controls;

public partial class NavButton : Button
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<NavButton, string>(nameof(Glyph));

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<NavButton, string>(nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<NavButton, bool>(nameof(IsActive));

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty)
            UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        var border = this.FindControl<Border>("RootBorder");
        var glyph = this.FindControl<TextBlock>("GlyphText");
        var label = this.FindControl<TextBlock>("LabelText");

        if (IsActive)
        {
            if (border is not null) border.Background = new SolidColorBrush(Color.Parse("#0D4A9EFF"));
            if (glyph is not null) glyph.Foreground = new SolidColorBrush(Color.Parse("#4A9EFF"));
            if (label is not null) label.Foreground = new SolidColorBrush(Color.Parse("#4A9EFF"));
        }
        else
        {
            if (border is not null) border.Background = Brushes.Transparent;
            if (glyph is not null) glyph.Foreground = new SolidColorBrush(Color.Parse("#4A4A64"));
            if (label is not null) label.Foreground = new SolidColorBrush(Color.Parse("#4A4A64"));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateVisuals();
    }

    public void SetActive(bool active) => IsActive = active;
}
