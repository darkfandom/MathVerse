using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.QuickEvaluateCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (FocusManager?.GetFocusedElement() is TextBox tb && tb.Name == "ExpressionInput")
            {
                vm.AddGraphFromExpressionCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Delete && vm.Graph.SelectedGraph != null)
        {
            vm.Graph.RemoveGraphCommand.Execute(vm.Graph.SelectedGraph);
            e.Handled = true;
        }
        else if (e.Key == Key.G && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.Graph.ToggleGridCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.F && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.Graph.FitAllCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.H && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.Graph.HomeCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ExpressionInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not WorkspaceViewModel vm) return;

        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            vm.QuickEvaluateCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            vm.AddGraphFromExpressionCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void Toggle3D_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is WorkspaceViewModel vm)
            vm.Graph.Is3D = !vm.Graph.Is3D;
    }

    private void ExplorerItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.Tag is GraphEntry entry && DataContext is WorkspaceViewModel vm)
            vm.Graph.SelectedGraph = entry;
    }

    private void ExplorerDelete_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.Tag is GraphEntry entry && DataContext is WorkspaceViewModel vm)
            vm.Graph.RemoveGraphCommand.Execute(entry);
        e.Handled = true;
    }

    private void HistoryItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.Tag is string expr && DataContext is WorkspaceViewModel vm)
            vm.ExpressionInput = expr;
    }
}

public class ErrorToBrushConverter : IValueConverter
{
    public static readonly ErrorToBrushConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isError)
            return isError
                ? new SolidColorBrush(Color.Parse("#FF4444"))
                : new SolidColorBrush(Color.Parse("#E8E8F0"));
        return new SolidColorBrush(Color.Parse("#E8E8F0"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
