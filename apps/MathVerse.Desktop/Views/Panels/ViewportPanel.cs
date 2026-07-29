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
        AppServices.EventBus.Subscribe(EventType.ToolActivated, _ => UpdateCursor());
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
        var normX = (float)(pos.X / Bounds.Width);
        var normY = (float)(pos.Y / Bounds.Height);
        var props = e.GetCurrentPoint(this).Properties;
        var button = props.IsRightButtonPressed ? 2 : props.IsMiddleButtonPressed ? 1 : 0;
        AppServices.ToolManager.InvokeMouseDown(normX, normY, button);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(this);
        var normX = (float)(pos.X / Bounds.Width);
        var normY = (float)(pos.Y / Bounds.Height);

        var world = ScreenToWorld(normX, normY);
        CoordDisplay.Text = $"({world.X:F2}, {world.Y:F2})";

        AppServices.ToolManager.InvokeMouseMove(normX, normY);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var pos = e.GetPosition(this);
        var normX = (float)(pos.X / Bounds.Width);
        var normY = (float)(pos.Y / Bounds.Height);
        var props = e.GetCurrentPoint(this).Properties;
        var button = props.IsRightButtonPressed ? 2 : props.IsMiddleButtonPressed ? 1 : 0;
        AppServices.ToolManager.InvokeMouseUp(normX, normY, button);
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        AppServices.ToolManager.InvokeWheel((float)e.Delta.Y);
        e.Handled = true;
    }

    private Vector2 ScreenToWorld(float nx, float ny)
    {
        float ndcX = nx * 2f - 1f;
        float ndcY = 1f - ny * 2f;
        if (Matrix4x4.Invert(AppServices.ViewportRenderer.Camera.ViewProjectionMatrix, out var inv))
        {
            var clip = Vector4.Transform(new Vector4(ndcX, ndcY, 0, 1), inv);
            if (System.Math.Abs(clip.W) > 0.0001f)
            {
                clip.X /= clip.W;
                clip.Y /= clip.W;
            }
            return new Vector2(clip.X, clip.Y);
        }
        return Vector2.Zero;
    }

    private void UpdateCursor()
    {
        var tool = AppServices.ToolManager.ActiveTool;
        Cursor = tool?.Name switch
        {
            "PanTool" => new Cursor(StandardCursorType.Hand),
            "ZoomTool" => new Cursor(StandardCursorType.SizeAll),
            _ => new Cursor(StandardCursorType.Arrow),
        };
    }

    private void OnSelectionChanged(EventData data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var count = AppServices.SelectionManager.Count;
            SelectionRect.IsVisible = count > 0;
        });
    }
}
