using Avalonia.Controls;
using Avalonia.Input;
using MathVerse.Desktop.ViewModels;

namespace MathVerse.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not WorkspaceViewModel vm) return;

        if (e.Key == Key.Left && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            vm.GoBackCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Right && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            vm.GoForwardCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Back && !e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            vm.GoBackCommand.Execute(null);
            e.Handled = true;
        }
    }
}
