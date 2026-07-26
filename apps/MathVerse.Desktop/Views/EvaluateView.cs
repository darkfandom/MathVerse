using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MathVerse.Desktop.ViewModels;

namespace MathVerse.Desktop.Views;

public partial class EvaluateView : UserControl
{
    public EvaluateView() => InitializeComponent();

    private void ExpressionTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is EvaluateViewModel vm)
        {
            vm.EvaluateCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void HistoryItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.Tag is HistoryItem item && DataContext is EvaluateViewModel vm)
        {
            vm.LoadHistoryCommand.Execute(item);
        }
    }

    private void HistoryDelete_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control control && control.Tag is HistoryItem item && DataContext is EvaluateViewModel vm)
        {
            vm.DeleteHistoryCommand.Execute(item);
        }
        e.Handled = true;
    }
}
