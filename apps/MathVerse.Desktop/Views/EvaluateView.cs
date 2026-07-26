using Avalonia.Controls;
using Avalonia.Input;
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
}
