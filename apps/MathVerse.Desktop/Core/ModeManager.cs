namespace MathVerse.Desktop.Core;

public enum WorkspaceMode
{
    Mathematics,
    Visualization,
    Geometry,
    Simulation,
    Research,
    Publication,
    Teaching
}

public sealed class ModeManager
{
    private readonly EventBus _eventBus;
    private readonly ToolManager _toolManager;
    private WorkspaceMode _activeMode = WorkspaceMode.Mathematics;

    public WorkspaceMode ActiveMode => _activeMode;
    public event Action<WorkspaceMode>? ModeChanged;

    private static readonly Dictionary<WorkspaceMode, string> DefaultTools = new()
    {
        [WorkspaceMode.Mathematics] = "SelectTool",
        [WorkspaceMode.Visualization] = "PanTool",
        [WorkspaceMode.Geometry] = "SelectTool",
        [WorkspaceMode.Simulation] = "PanTool",
        [WorkspaceMode.Research] = "SelectTool",
        [WorkspaceMode.Publication] = "SelectTool",
        [WorkspaceMode.Teaching] = "SelectTool",
    };

    public ModeManager(EventBus eventBus, ToolManager toolManager)
    {
        _eventBus = eventBus;
        _toolManager = toolManager;
    }

    public void SetMode(WorkspaceMode mode)
    {
        if (_activeMode == mode)
            return;

        _activeMode = mode;

        if (DefaultTools.TryGetValue(mode, out var defaultTool))
            _toolManager.SetActive(defaultTool);

        ModeChanged?.Invoke(mode);
        _eventBus.Publish(new EventData(EventType.WorkspaceModeChanged, null, mode.ToString()));
    }

    public string GetDefaultTool(WorkspaceMode mode) =>
        DefaultTools.TryGetValue(mode, out var tool) ? tool : "SelectTool";
}
