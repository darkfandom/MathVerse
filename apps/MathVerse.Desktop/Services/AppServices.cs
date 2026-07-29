using MathVerse.Desktop.Core;
using MathVerse.Desktop.Commands;
using MathVerse.Desktop.Rendering;

namespace MathVerse.Desktop.Services;

public static class AppServices
{
    public static EventBus EventBus { get; } = new();
    public static ObjectRegistry Registry { get; } = new();
    public static SelectionManager SelectionManager { get; }
    public static CommandRegistry CommandRegistry { get; } = new();
    public static Workspace Workspace { get; } = new();
    public static CommandManager CommandManager { get; }
    public static UndoManager UndoManager { get; }
    public static ModeManager ModeManager { get; }
    public static ToolManager ToolManager { get; }
    public static CompilerPipeline CompilerPipeline { get; } = new();
    public static ViewportRenderer ViewportRenderer { get; private set; } = new();

    static AppServices()
    {
        ToolManager = new ToolManager(EventBus);
        SelectionManager = new SelectionManager(Registry, EventBus);
        CommandManager = new CommandManager(CommandRegistry, Workspace, Registry, EventBus, SelectionManager);
        UndoManager = new UndoManager(EventBus, CommandManager);
        ModeManager = new ModeManager(EventBus, ToolManager);

        RegisterInitialCommands();
        RegisterInitialTools();
    }

    private static void RegisterInitialCommands()
    {
        CommandRegistry.Register(new CreateObjectCommand());
        CommandRegistry.Register(new DeleteObjectCommand());
        CommandRegistry.Register(new SetObjectPropertyCommand());
        CommandRegistry.Register(new SetObjectVisibilityCommand());
        CommandRegistry.Register(new RenameObjectCommand());
    }

    private static void RegisterInitialTools()
    {
        ToolManager.Register(new SelectTool());
        ToolManager.Register(new PanTool());
        ToolManager.Register(new ZoomTool());
    }
}
