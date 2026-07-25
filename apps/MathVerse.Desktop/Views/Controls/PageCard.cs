using Avalonia;
using Avalonia.Controls;

namespace MathVerse.Desktop.Views.Controls;

public partial class PageCard : Button
{
    public static readonly StyledProperty<string> CardTitleProperty =
        AvaloniaProperty.Register<PageCard, string>(nameof(CardTitle));

    public string CardTitle
    {
        get => GetValue(CardTitleProperty);
        set => SetValue(CardTitleProperty, value);
    }

    public static readonly StyledProperty<string> CardSubtitleProperty =
        AvaloniaProperty.Register<PageCard, string>(nameof(CardSubtitle));

    public string CardSubtitle
    {
        get => GetValue(CardSubtitleProperty);
        set => SetValue(CardSubtitleProperty, value);
    }

    public PageCard()
    {
        InitializeComponent();
    }
}
