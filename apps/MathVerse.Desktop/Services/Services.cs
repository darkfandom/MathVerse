using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Services;

public sealed class ExpressionCompilerService
{
    private readonly CompilerPipeline _pipeline;

    public ExpressionCompilerService(CompilerPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public CompiledExpression? Compile(IWorkspaceObject obj)
    {
        var result = _pipeline.Compile(obj);
        return result as CompiledExpression;
    }
}

public sealed class GraphCompilerService
{
    private readonly CompilerPipeline _pipeline;

    public GraphCompilerService(CompilerPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public CompiledGraph? Compile(IWorkspaceObject obj)
    {
        var result = _pipeline.Compile(obj);
        return result as CompiledGraph;
    }
}

public sealed class MeshGeneratorService
{
    private readonly CompilerPipeline _pipeline;

    public MeshGeneratorService(CompilerPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public CompiledSurface? Generate(IWorkspaceObject obj)
    {
        var result = _pipeline.Compile(obj);
        return result as CompiledSurface;
    }
}

public sealed class ExportService
{
    public byte[] ExportToPng(byte[] pixelData, int width, int height)
    {
        // Placeholder: return raw pixel data
        return pixelData;
    }
}

public sealed class ScreenshotService
{
    public byte[] CaptureViewport(int width, int height)
    {
        return new byte[width * height * 4];
    }
}

public sealed class ClipboardService
{
    private readonly List<IWorkspaceObject> _clipboard = [];

    public void Copy(IEnumerable<IWorkspaceObject> objects)
    {
        _clipboard.Clear();
        foreach (var obj in objects)
            _clipboard.Add(obj.Clone());
    }

    public void Cut(IEnumerable<IWorkspaceObject> objects)
    {
        _clipboard.Clear();
        foreach (var obj in objects)
            _clipboard.Add(obj.Clone());
    }

    public IReadOnlyList<IWorkspaceObject> Paste()
    {
        var copies = _clipboard.Select(o => o.Clone()).ToList();
        return copies;
    }

    public void Clear() => _clipboard.Clear();
    public bool HasData => _clipboard.Count > 0;
}
