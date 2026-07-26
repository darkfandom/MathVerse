using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MathVerse.Desktop.ViewModels;

namespace MathVerse.Desktop.Views;

public partial class GraphView : UserControl
{
    private bool _isPanning;
    private bool _isRotating;
    private Point _lastMousePos;

    public GraphView() => InitializeComponent();

    private GraphViewModel? Vm => DataContext as GraphViewModel;

    private void ExpressionInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Vm != null)
        {
            Vm.AddGraphCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void DeleteGraph_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock tb && tb.Tag is GraphEntry entry && Vm != null)
            Vm.RemoveGraphCommand.Execute(entry);
    }

    private void ViewportImage_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        Vm?.HandleMouseWheel(e.Delta.Y);
        e.Handled = true;
    }

    private void ViewportImage_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control ctrl) return;
        var props = e.GetCurrentPoint(ctrl).Properties;
        _lastMousePos = e.GetPosition(ctrl);
        if (props.IsMiddleButtonPressed) { _isPanning = true; e.Handled = true; }
        else if (props.IsRightButtonPressed) { _isRotating = true; e.Handled = true; }
    }

    private void ViewportImage_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        _isRotating = false;
    }

    private void ViewportImage_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not Control ctrl) return;
        var pos = e.GetPosition(ctrl);
        Vm?.HandleMouseMove(pos.X, pos.Y);
        if (_isPanning)
        {
            Vm?.HandlePan(pos.X - _lastMousePos.X, pos.Y - _lastMousePos.Y);
            _lastMousePos = pos;
        }
        else if (_isRotating)
        {
            Vm?.HandleRotate(pos.X - _lastMousePos.X, pos.Y - _lastMousePos.Y);
            _lastMousePos = pos;
        }
    }

    private void ViewportImage_DoubleTapped(object? sender, TappedEventArgs e)
    {
        Vm?.HomeCommand.Execute(null);
    }
}
