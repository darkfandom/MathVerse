using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Core;

public interface ICompiler
{
    bool CanCompile(IWorkspaceObject obj);
    CompiledObject? Compile(IWorkspaceObject obj);
}

public abstract class CompiledObject
{
    public Guid SourceObjectId { get; }
    public DateTime CompiledAt { get; } = DateTime.UtcNow;
    public bool IsValid { get; protected set; } = true;

    protected CompiledObject(Guid sourceObjectId)
    {
        SourceObjectId = sourceObjectId;
    }
}

public sealed class CompiledExpression : CompiledObject
{
    public string ExpressionString { get; }
    public bool ParseSucceeded { get; }

    public CompiledExpression(Guid sourceObjectId, string expression, bool parseSucceeded)
        : base(sourceObjectId)
    {
        ExpressionString = expression;
        ParseSucceeded = parseSucceeded;
        IsValid = parseSucceeded;
    }
}

public sealed class CompiledGraph : CompiledObject
{
    public string ExpressionString { get; }
    public string GraphType { get; }
    public float[] SamplePointsX { get; }
    public float[] SamplePointsY { get; }
    public int SampleCount { get; }

    public CompiledGraph(Guid sourceObjectId, string expression, string graphType,
        float[] sampleX, float[] sampleY) : base(sourceObjectId)
    {
        ExpressionString = expression;
        GraphType = graphType;
        SamplePointsX = sampleX;
        SamplePointsY = sampleY;
        SampleCount = System.Math.Min(sampleX.Length, sampleY.Length);
    }
}

public sealed class CompiledSurface : CompiledObject
{
    public string ExpressionString { get; }
    public float[] Vertices { get; }
    public int Width { get; }
    public int Height { get; }

    public CompiledSurface(Guid sourceObjectId, string expression,
        float[] vertices, int width, int height) : base(sourceObjectId)
    {
        ExpressionString = expression;
        Vertices = vertices;
        Width = width;
        Height = height;
    }
}

public sealed class ExpressionCompiler : ICompiler
{
    public bool CanCompile(IWorkspaceObject obj) =>
        obj.TypeTag is "Expression" or "Graph" or "Surface";

    public CompiledObject? Compile(IWorkspaceObject obj)
    {
        var expr = obj.Metadata.TryGetValue("Expression", out var e) ? e as string : obj.Name;
        if (string.IsNullOrEmpty(expr))
            return new CompiledExpression(obj.Id, "", false);

        return obj.TypeTag switch
        {
            "Expression" => new CompiledExpression(obj.Id, expr, true),
            "Graph" => CompileGraph(obj, expr),
            "Surface" => CompileSurface(obj, expr),
            _ => new CompiledExpression(obj.Id, expr, true)
        };
    }

    private static CompiledGraph CompileGraph(IWorkspaceObject obj, string expression)
    {
        var type = obj.Metadata.TryGetValue("GraphType", out var t) ? t as string ?? "Cartesian" : "Cartesian";
        var samples = 1000;
        var x = new float[samples];
        var y = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            x[i] = -10f + 20f * i / (samples - 1);
            y[i] = System.MathF.Sin(x[i]);
        }

        return new CompiledGraph(obj.Id, expression, type, x, y);
    }

    private static CompiledSurface CompileSurface(IWorkspaceObject obj, string expression)
    {
        var res = 50;
        var vertices = new float[res * res * 3];
        var idx = 0;

        for (int i = 0; i < res; i++)
        {
            for (int j = 0; j < res; j++)
            {
                var u = -5f + 10f * i / (res - 1);
                var v = -5f + 10f * j / (res - 1);
                vertices[idx++] = u;
                vertices[idx++] = v;
                vertices[idx++] = System.MathF.Sin(System.MathF.Sqrt(u * u + v * v));
            }
        }

        return new CompiledSurface(obj.Id, expression, vertices, res, res);
    }
}

public sealed class GraphCompiler : ICompiler
{
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Graph";

    public CompiledObject? Compile(IWorkspaceObject obj)
    {
        var expr = obj.Metadata.TryGetValue("Expression", out var e) ? e as string : obj.Name;
        if (string.IsNullOrEmpty(expr))
            return null;

        var type = obj.Metadata.TryGetValue("GraphType", out var t) ? t as string ?? "Cartesian" : "Cartesian";
        var samples = 1000;
        var x = new float[samples];
        var y = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            x[i] = -10f + 20f * i / (samples - 1);
            y[i] = System.MathF.Sin(x[i]);
        }

        return new CompiledGraph(obj.Id, expr, type, x, y);
    }
}

public sealed class SurfaceCompiler : ICompiler
{
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Surface";

    public CompiledObject? Compile(IWorkspaceObject obj)
    {
        var expr = obj.Metadata.TryGetValue("Expression", out var e) ? e as string : obj.Name;
        if (string.IsNullOrEmpty(expr))
            return null;

        var res = 50;
        var vertices = new float[res * res * 3];
        var idx = 0;

        for (int i = 0; i < res; i++)
        {
            for (int j = 0; j < res; j++)
            {
                var u = -5f + 10f * i / (res - 1);
                var v = -5f + 10f * j / (res - 1);
                vertices[idx++] = u;
                vertices[idx++] = v;
                vertices[idx++] = System.MathF.Sin(System.MathF.Sqrt(u * u + v * v));
            }
        }

        return new CompiledSurface(obj.Id, expr, vertices, res, res);
    }
}

public sealed class CompilerPipeline
{
    private readonly List<ICompiler> _compilers = [];
    private readonly Dictionary<Guid, CompiledObject> _cache = [];

    public IReadOnlyDictionary<Guid, CompiledObject> Cache => _cache;

    public CompilerPipeline()
    {
        Register(new ExpressionCompiler());
        Register(new GraphCompiler());
        Register(new SurfaceCompiler());
    }

    public void Register(ICompiler compiler) => _compilers.Add(compiler);

    public CompiledObject? Compile(IWorkspaceObject obj)
    {
        foreach (var compiler in _compilers)
        {
            if (compiler.CanCompile(obj))
            {
                var result = compiler.Compile(obj);
                if (result is not null)
                    _cache[obj.Id] = result;
                return result;
            }
        }
        return null;
    }

    public CompiledObject? GetCached(Guid objectId) =>
        _cache.TryGetValue(objectId, out var compiled) ? compiled : null;

    public void Invalidate(Guid objectId) => _cache.Remove(objectId);

    public void InvalidateAll() => _cache.Clear();
}
