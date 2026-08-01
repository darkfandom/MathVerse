using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Rendering;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop;

public partial class ViewportPanel : UserControl
{
    private DispatcherTimer? _renderTimer;

    public ViewportRenderer Renderer => AppServices.ViewportRenderer;

    public ViewportPanel()
    {
        InitializeComponent();
        AppServices.EventBus.Subscribe(EventType.ObjectSelectionChanged, OnSelectionChanged);
        AppServices.EventBus.Subscribe(EventType.ActiveObjectChanged, OnSelectionChanged);
        AppServices.EventBus.Subscribe(EventType.ToolActivated, OnToolActivated);

        // Set up overlay pass to update UI controls
        AppServices.ViewportRenderer.SetOverlayPass(new OverlayPass(UpdateOverlay));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        StartRenderLoop();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var w = (int)e.NewSize.Width;
        var h = (int)e.NewSize.Height;
        if (w > 0 && h > 0)
        {
            AppServices.ViewportRenderer.Resize(w, h);
            AppServices.ViewportRenderer.Invalidate();
            RenderFrame();
        }
    }

    private void StartRenderLoop()
    {
        _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _renderTimer.Tick += (_, _) => RenderFrame();
        _renderTimer.Start();
    }

    private void RenderFrame()
    {
        var bitmap = AppServices.ViewportRenderer.Render();
        if (bitmap is not null)
            RenderImage.Source = bitmap;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        var pos = e.GetPosition(this);
        var nx = (float)(pos.X / Bounds.Width);
        var ny = (float)(pos.Y / Bounds.Height);
        AppServices.ViewportRenderer.SetCursorWorld(nx, ny);
        var props = e.GetCurrentPoint(this).Properties;
        var button = props.IsRightButtonPressed ? 2 : props.IsMiddleButtonPressed ? 1 : 0;

        // Check modifiers for Ctrl+Click
        var modifiers = e.KeyModifiers;
        if (modifiers.HasFlag(KeyModifiers.Control))
            AppServices.ToolManager.InvokeKeyDown("Control");

        AppServices.ToolManager.InvokeMouseDown(nx, ny, button);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(this);
        var nx = (float)(pos.X / Bounds.Width);
        var ny = (float)(pos.Y / Bounds.Height);
        AppServices.ViewportRenderer.SetCursorWorld(nx, ny);
        AppServices.ToolManager.InvokeMouseMove(nx, ny);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var pos = e.GetPosition(this);
        var nx = (float)(pos.X / Bounds.Width);
        var ny = (float)(pos.Y / Bounds.Height);
        AppServices.ViewportRenderer.SetCursorWorld(nx, ny);
        var props = e.GetCurrentPoint(this).Properties;
        var button = props.IsRightButtonPressed ? 2 : props.IsMiddleButtonPressed ? 1 : 0;
        AppServices.ToolManager.InvokeMouseUp(nx, ny, button);
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var pos = e.GetPosition(this);
        var nx = (float)(pos.X / Bounds.Width);
        var ny = (float)(pos.Y / Bounds.Height);
        AppServices.ViewportRenderer.ZoomOnCursor((float)e.Delta.Y, nx, ny);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Global keyboard shortcuts
        switch (e.Key)
        {
            case Key.Escape:
                AppServices.CommandManager.Execute("ClearSelection");
                e.Handled = true;
                break;
            case Key.A when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                AppServices.CommandManager.Execute("SelectAll");
                e.Handled = true;
                break;
            case Key.A when !e.KeyModifiers.HasFlag(KeyModifiers.Control):
                AppServices.CommandManager.Execute("SelectAll");
                e.Handled = true;
                break;
            default:
                // Forward to active tool
                var keyStr = e.Key switch
                {
                    Key.LeftCtrl or Key.RightCtrl => "Control",
                    Key.LeftShift or Key.RightShift => "Shift",
                    _ => e.Key.ToString(),
                };
                AppServices.ToolManager.InvokeKeyDown(keyStr);
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            AppServices.ToolManager.InvokeKeyDown(""); // Reset Ctrl state in tool
    }

    private void OnToolActivated(EventData data)
    {
        Dispatcher.UIThread.Post(() => UpdateCursor());
    }

    private void UpdateCursor()
    {
        var tool = AppServices.ToolManager.ActiveTool;
        var name = tool?.Name ?? "SelectTool";
        AppServices.ViewportRenderer.SetToolName(name);
        Cursor = name switch
        {
            "PanTool" => new Cursor(StandardCursorType.Hand),
            "ZoomTool" => new Cursor(StandardCursorType.SizeAll),
            _ => new Cursor(StandardCursorType.Arrow),
        };
    }

    private void UpdateOverlay(OverlayPass.OverlayData data)
    {
        CoordDisplay.Text = data.Coordinates;
        ZoomDisplay.Text = data.ZoomLevel;
        ToolDisplay.Text = data.ActiveTool;
        FpsDisplay.Text = data.Fps + " FPS";
        CamDisplay.Text = data.CameraPos;
        SelectionDisplay.Text = data.SelectionInfo;
    }

    private void OnSelectionChanged(EventData data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var count = AppServices.SelectionService.Count;
            SelectionDisplay.Text = count > 0 ? $"{count} selected" : "";
        });
    }
}
