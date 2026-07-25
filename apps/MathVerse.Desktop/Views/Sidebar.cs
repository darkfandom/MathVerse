using Avalonia.Controls;
using Avalonia.Interactivity;
using MathVerse.Desktop.ViewModels;
using MathVerse.Desktop.Views.Controls;

namespace MathVerse.Desktop.Views;

public partial class Sidebar : UserControl
{
    private NavButton? _activeButton;

    public Sidebar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _activeButton = BtnHome;
    }

    private void OnNavClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not NavButton button) return;
        if (DataContext is not WorkspaceViewModel vm) return;

        var page = button.Tag?.ToString();
        if (string.IsNullOrEmpty(page)) return;

        if (_activeButton is not null)
            _activeButton.SetActive(false);

        button.SetActive(true);
        _activeButton = button;

        vm.NavigateCommand.Execute(page);
    }
}
