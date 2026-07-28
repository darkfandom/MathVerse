namespace MathVerse.Desktop.Core;

public interface ITool
{
    string Name { get; }
    string Cursor { get; }
    void Activate();
    void Deactivate();
    bool OnMouseDown(float x, float y, int button);
    bool OnMouseMove(float x, float y);
    bool OnMouseUp(float x, float y, int button);
    bool OnWheel(float delta);
    bool OnKeyDown(string key);
    void DrawOverlay();
}

public sealed class ToolManager
{
    private readonly EventBus _eventBus;
    private readonly Dictionary<string, ITool> _tools = [];
    private readonly Stack<string> _history = new();
    private ITool? _activeTool;

    public ITool? ActiveTool => _activeTool;
    public string? ActiveToolName => _activeTool?.Name;

    public ToolManager(EventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void Register(ITool tool)
    {
        _tools[tool.Name] = tool;
    }

    public ITool? Get(string name) =>
        _tools.TryGetValue(name, out var tool) ? tool : null;

    public bool SetActive(string name)
    {
        if (!_tools.TryGetValue(name, out var tool))
            return false;

        if (_activeTool is not null)
        {
            _activeTool.Deactivate();
            _eventBus.Publish(new EventData(EventType.ToolDeactivated, null, _activeTool.Name));
        }

        if (_activeTool?.Name != name)
            _history.Push(_activeTool?.Name ?? "SelectTool");

        _activeTool = tool;
        _activeTool.Activate();
        _eventBus.Publish(new EventData(EventType.ToolActivated, null, name));
        return true;
    }

    public bool PreviousTool()
    {
        if (_history.Count == 0)
            return false;
        var prev = _history.Pop();
        return SetActive(prev);
    }

    public bool InvokeMouseDown(float x, float y, int button) =>
        _activeTool?.OnMouseDown(x, y, button) ?? false;

    public bool InvokeMouseMove(float x, float y) =>
        _activeTool?.OnMouseMove(x, y) ?? false;

    public bool InvokeMouseUp(float x, float y, int button) =>
        _activeTool?.OnMouseUp(x, y, button) ?? false;

    public bool InvokeWheel(float delta) =>
        _activeTool?.OnWheel(delta) ?? false;

    public bool InvokeKeyDown(string key) =>
        _activeTool?.OnKeyDown(key) ?? false;

    public void InvokeDrawOverlay() =>
        _activeTool?.DrawOverlay();

    public void Clear()
    {
        _activeTool?.Deactivate();
        _activeTool = null;
        _history.Clear();
        _tools.Clear();
    }
}
