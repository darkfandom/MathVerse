using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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

        var workspace = this.GetLogicalAncestors()
            .OfType<Control>()
            .Select(c => c.DataContext as WorkspaceViewModel)
            .FirstOrDefault(vm => vm != null);
        workspace?.NavigateCommand.Execute(tag);
    }
}
