using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Rendering.Compilers;

public sealed class ExpressionCompiler : IRenderCompiler
{
    public string TypeTag => "Expression";
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Expression";

    public IRenderObject[] Compile(IWorkspaceObject obj)
    {
        var color = ParseColor(obj, 0x4A, 0x9E, 0xFF);
        var expr = obj.Metadata.TryGetValue("Expression", out var e) ? e?.ToString() ?? "" : "";
        return [new RenderText(obj.Id, expr, new Vertex2(0, 0), color, 12f)];
    }

    internal static Color4 ParseColor(IWorkspaceObject obj, byte defaultR, byte defaultG, byte defaultB)
    {
        if (obj.Metadata.TryGetValue("Color", out var c) && c is string hex && hex.Length >= 7)
        {
            if (byte.TryParse(hex[1..3], System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex[3..5], System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex[5..7], System.Globalization.NumberStyles.HexNumber, null, out var b))
                return new Color4(r, g, b, 255);
        }
        return new Color4(defaultR, defaultG, defaultB, 255);
    }
}

public sealed class GraphCompiler : IRenderCompiler
{
    public string TypeTag => "Graph";
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Graph";

    public IRenderObject[] Compile(IWorkspaceObject obj)
    {
        var color = ExpressionCompiler.ParseColor(obj, 0x4C, 0xAF, 0x50);
        var points = new Vertex2[200];
        for (int i = 0; i < 200; i++)
        {
            float t = (i / 199f) * 4f * (float)System.Math.PI - 2f * (float)System.Math.PI;
            points[i] = new Vertex2(t, (float)System.Math.Sin(t));
        }
        return
        [
            new RenderPolyline(obj.Id, points, color, false, 1.5f),
            new RenderText(obj.Id, obj.Name, new Vertex2(0, 1.5f), color, 11f),
        ];
    }
}

public sealed class SurfaceCompiler : IRenderCompiler
{
    public string TypeTag => "Surface";
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Surface";

    public IRenderObject[] Compile(IWorkspaceObject obj)
    {
        var color = ExpressionCompiler.ParseColor(obj, 0xFF, 0x98, 0x00);
        var list = new List<IRenderObject>();
        int resolution = 20;
        for (int i = 0; i <= resolution; i++)
        {
            float t = (i / (float)resolution) * 4f - 2f;
            var verts = new Vertex2[resolution + 1];
            for (int j = 0; j <= resolution; j++)
            {
                float u = (j / (float)resolution) * 4f - 2f;
                verts[j] = new Vertex2(t, u);
            }
            list.Add(new RenderPolyline(obj.Id, verts,
                new Color4((byte)(color.R / 2), (byte)(color.G / 2), (byte)(color.B / 2), 180), false, 0.5f));

            verts = new Vertex2[resolution + 1];
            for (int j = 0; j <= resolution; j++)
            {
                float u = (j / (float)resolution) * 4f - 2f;
                verts[j] = new Vertex2(u, t);
            }
            list.Add(new RenderPolyline(obj.Id, verts,
                new Color4((byte)(color.R / 2), (byte)(color.G / 2), (byte)(color.B / 2), 180), false, 0.5f));
        }
        list.Add(new RenderText(obj.Id, obj.Name, new Vertex2(0, 2.5f), color, 11f));
        return list.ToArray();
    }
}

public sealed class GeometryCompiler : IRenderCompiler
{
    public string TypeTag => "Geometry";
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Geometry";

    public IRenderObject[] Compile(IWorkspaceObject obj)
    {
        var color = ExpressionCompiler.ParseColor(obj, 0xE0, 0x40, 0x40);
        return
        [
            new RenderRectangle(obj.Id, new Vertex2(-0.5f, -0.5f), new Vertex2(0.5f, 0.5f),
                new Color4(color.R, color.G, color.B, 40), color, true, 1f),
            new RenderText(obj.Id, obj.Name, new Vertex2(0, 0.8f), color, 11f),
        ];
    }
}

public sealed class DatasetCompiler : IRenderCompiler
{
    public string TypeTag => "Dataset";
    public bool CanCompile(IWorkspaceObject obj) => obj.TypeTag == "Dataset";

    public IRenderObject[] Compile(IWorkspaceObject obj)
    {
        var color = ExpressionCompiler.ParseColor(obj, 0xAB, 0x47, 0xBC);
        var points = new Vertex2[50];
        var rng = new Random(obj.GetHashCode());
        for (int i = 0; i < 50; i++)
            points[i] = new Vertex2((i / 49f) * 4f - 2f, (float)(rng.NextDouble() - 0.5) * 2f);
        return
        [
            new RenderPolyline(obj.Id, points, color, false, 1f),
            new RenderText(obj.Id, obj.Name, new Vertex2(0, 1.2f), color, 11f),
        ];
    }
}
