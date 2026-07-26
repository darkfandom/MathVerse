using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MathVerse.Desktop.ViewModels;

namespace MathVerse.Desktop.Views;

public partial class GraphView : UserControl
{
    private bool _isPanning;
    private bool _isRotating;
    private Point _lastMousePos;
    private DispatcherTimer? _animationTimer;
    private DateTime _lastTick = DateTime.UtcNow;

    public GraphView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(GraphViewModel.IsAnimating))
                    SyncAnimationTimer(vm);
            };
            SyncAnimationTimer(vm);
        }
    }

    private void SyncAnimationTimer(GraphViewModel vm)
    {
        if (vm.IsAnimating)
        {
            if (_animationTimer == null)
            {
                _animationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(16)
                };
                _animationTimer.Tick += AnimationTimer_Tick;
            }
            _lastTick = DateTime.UtcNow;
            _animationTimer.Start();
        }
        else
        {
            _animationTimer?.Stop();
        }
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (DataContext is not GraphViewModel vm) return;
        var now = DateTime.UtcNow;
        double dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;
        vm.TickAnimation(dt);
    }

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
        if (props.IsMiddleButtonPressed || props.IsLeftButtonPressed)
        {
            _isPanning = true;
            e.Handled = true;
        }
        else if (props.IsRightButtonPressed)
        {
            _isRotating = true;
            e.Handled = true;
        }
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

    private void Toggle3D_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Vm == null) return;
        Vm.Is3D = !Vm.Is3D;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _animationTimer?.Stop();
        _animationTimer = null;
        base.OnDetachedFromVisualTree(e);
    }
}
