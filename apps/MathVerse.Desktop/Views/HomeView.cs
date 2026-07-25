using Avalonia.Controls;
using Avalonia.Interactivity;
using MathVerse.Desktop.ViewModels;

namespace MathVerse.Desktop.Views;

public partial class HomeView : UserControl
{
    public HomeView() => InitializeComponent();

    private void OnCardClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control control) return;
        var tag = control.Tag?.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        var workspace = DataContext as WorkspaceViewModel
            ?? (VisualRoot as Control)?.DataContext as WorkspaceViewModel;
        workspace?.NavigateCommand.Execute(tag);
    }
}
