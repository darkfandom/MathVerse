using Avalonia.Controls;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppServices.EventBus.Subscribe(EventType.ObjectCreated, _ => UpdateStatusBar());
        AppServices.EventBus.Subscribe(EventType.ObjectDeleted, _ => UpdateStatusBar());
        AppServices.EventBus.Subscribe(EventType.ToolActivated, OnToolChanged);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        var count = AppServices.Registry.Count;
        StatusObjectCount.Text = $"Objects: {count}";
    }

    private void OnToolChanged(EventData data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var name = AppServices.ToolManager.ActiveToolName ?? "SelectTool";
            StatusToolName.Text = $"Tool: {name}";
            AppServices.ViewportRenderer.SetStatus($"Tool: {name}");
        });
    }
}
