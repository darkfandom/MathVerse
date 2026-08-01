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
        AppServices.EventBus.Subscribe(EventType.ObjectSelectionChanged, _ => UpdateStatusBar());
        AppServices.EventBus.Subscribe(EventType.ActiveObjectChanged, _ => UpdateStatusBar());
        AppServices.EventBus.Subscribe(EventType.HoveredObjectChanged, _ => UpdateStatusBar());
        AppServices.EventBus.Subscribe(EventType.ToolActivated, OnToolChanged);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var count = AppServices.Registry.Count;
            StatusObjectCount.Text = $"Objects: {count}";

            var sel = AppServices.SelectionService;
            var parts = new System.Collections.Generic.List<string>();

            if (sel.Count > 0)
                parts.Add($"Selected: {sel.Count}");

            if (sel.ActiveObject is { } active)
                parts.Add($"Active: {active.Name}");

            if (sel.HoveredObject is { } hovered)
                parts.Add($"Hovered: {hovered.Name}");

            var toolName = AppServices.ToolManager.ActiveToolName ?? "SelectTool";
            parts.Add($"Mode: {toolName.Replace("Tool", "")}");

            StatusMessage.Text = string.Join("  |  ", parts);
        });
    }

    private void OnToolChanged(EventData data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var name = AppServices.ToolManager.ActiveToolName ?? "SelectTool";
            StatusToolName.Text = $"Tool: {name}";
            AppServices.ViewportRenderer.SetStatus($"Tool: {name}");
            UpdateStatusBar();
        });
    }
}
