namespace MathVerse.Math.Visualization.Export;
using System.Text;
using System.Collections.Generic;

/// <summary>Native MathVerse scene serialization format.</summary>
public sealed class MathVerseSceneFormat
{
    private const string SceneHeader = "MATHVERSE_SCENE_V1";
    private const string ObjectSeparator = "---OBJECT---";
    private const string PropertySeparator = "=";

    /// <summary>Serializes a visualization scene to the native format string.</summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>The serialized scene data.</returns>
    public static string Serialize(Core.VisualizationScene scene)
    {
        var sb = new StringBuilder();

        sb.AppendLine(SceneHeader);
        sb.AppendLine($"Name={scene.Name ?? ""}");
        sb.AppendLine($"ObjectCount={scene.Objects?.Count ?? 0}");
        sb.AppendLine();

        if (scene.Objects != null)
        {
            foreach (var obj in scene.Objects)
            {
                SerializeObject(sb, obj);
                sb.AppendLine(ObjectSeparator);
            }
        }

        return sb.ToString();
    }

    /// <summary>Deserializes a visualization scene from the native format string.</summary>
    /// <param name="data">The serialized scene data.</param>
    /// <returns>The deserialized visualization scene.</returns>
    public static Core.VisualizationScene Deserialize(string data)
    {
        var scene = new Core.VisualizationScene();
        var lines = data.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0 || lines[0] != SceneHeader)
            throw new System.FormatException("Invalid MathVerse scene format.");

        int currentIndex = 1;

        while (currentIndex < lines.Length)
        {
            var (key, value) = ParseProperty(lines[currentIndex]);
            currentIndex++;

            switch (key)
            {
                case "Name":
                    scene.Name = value;
                    break;
                case "ObjectCount":
                    break;
            }

            if (currentIndex < lines.Length && lines[currentIndex] == ObjectSeparator)
            {
                currentIndex++;
            }

            if (currentIndex < lines.Length && lines[currentIndex] == ObjectSeparator)
            {
                break;
            }
        }

        scene.Objects = DeserializeObjects(data);
        return scene;
    }

    /// <summary>Validates whether a string is a valid MathVerse scene.</summary>
    /// <param name="data">The data to validate.</param>
    /// <returns>True if the data is a valid scene.</returns>
    public static bool IsValidScene(string data)
    {
        if (string.IsNullOrEmpty(data))
            return false;

        var lines = data.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0 && lines[0] == SceneHeader;
    }

    /// <summary>Gets the file extension for MathVerse scene files.</summary>
    /// <returns>The file extension.</returns>
    public static string GetFileExtension()
    {
        return ".mvscene";
    }

    private static void SerializeObject(StringBuilder sb, Core.VisualizationObject obj)
    {
        sb.AppendLine($"Type={obj.GetType().Name}");
        sb.AppendLine($"Id={obj.Id ?? ""}");
        sb.AppendLine($"Color={obj.Color ?? "#000000"}");

        if (obj.Position.HasValue)
        {
            sb.AppendLine($"Pos={obj.Position.Value.X},{obj.Position.Value.Y},{obj.Position.Value.Z}");
        }

        if (obj is Core.LinePlot linePlot)
        {
            SerializeLinePlot(sb, linePlot);
        }
        else if (obj is Core.SurfacePlot surfacePlot)
        {
            SerializeSurfacePlot(sb, surfacePlot);
        }
        else if (obj is Core.MeshObject meshObj)
        {
            SerializeMeshObject(sb, meshObj);
        }
        else if (obj is Core.PointCloud pointCloud)
        {
            SerializePointCloud(sb, pointCloud);
        }
    }

    private static void SerializeLinePlot(StringBuilder sb, Core.LinePlot linePlot)
    {
        sb.AppendLine($"LineWidth={linePlot.LineWidth}");
        sb.AppendLine($"IsDashed={linePlot.IsDashed}");

        if (linePlot.Points != null)
        {
            sb.AppendLine($"PointCount={linePlot.Points.Count}");
            foreach (var pt in linePlot.Points)
            {
                sb.AppendLine($"P={pt.X},{pt.Y}");
            }
        }
    }

    private static void SerializeSurfacePlot(StringBuilder sb, Core.SurfacePlot surfacePlot)
    {
        sb.AppendLine($"FillColor={surfacePlot.FillColor ?? "#4488CC"}");
        sb.AppendLine($"Opacity={surfacePlot.Opacity}");

        if (surfacePlot.Cells != null)
        {
            sb.AppendLine($"CellCount={surfacePlot.Cells.Count}");
            foreach (var cell in surfacePlot.Cells)
            {
                if (cell != null)
                {
                    sb.AppendLine($"CellVertices={cell.Count}");
                    foreach (var pt in cell)
                    {
                        sb.AppendLine($"CV={pt.X},{pt.Y},{pt.Z}");
                    }
                }
            }
        }
    }

    private static void SerializeMeshObject(StringBuilder sb, Core.MeshObject meshObj)
    {
        sb.AppendLine($"WireframeColor={meshObj.WireframeColor ?? "#333333"}");
        sb.AppendLine($"FillColor={meshObj.FillColor ?? "#CCCCCC"}");

        if (meshObj.Vertices != null)
        {
            sb.AppendLine($"VertexCount={meshObj.Vertices.Count}");
            foreach (var v in meshObj.Vertices)
            {
                sb.AppendLine($"V={v.X},{v.Y},{v.Z}");
            }
        }

        if (meshObj.Faces != null)
        {
            sb.AppendLine($"FaceCount={meshObj.Faces.Count}");
            foreach (var face in meshObj.Faces)
            {
                sb.AppendLine($"F={string.Join(",", face)}");
            }
        }
    }

    private static void SerializePointCloud(StringBuilder sb, Core.PointCloud pointCloud)
    {
        sb.AppendLine($"PointSize={pointCloud.PointSize}");

        if (pointCloud.Points != null)
        {
            sb.AppendLine($"PointCount={pointCloud.Points.Count}");
            foreach (var pt in pointCloud.Points)
            {
                sb.AppendLine($"P={pt.X},{pt.Y},{pt.Z}");
            }
        }
    }

    private static List<Core.VisualizationObject> DeserializeObjects(string data)
    {
        var objects = new List<Core.VisualizationObject>();
        var objectBlocks = data.Split(new[] { ObjectSeparator }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in objectBlocks)
        {
            var lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                continue;

            var properties = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var (key, value) = ParseProperty(line);
                if (key != null)
                    properties[key] = value;
            }

            if (!properties.TryGetValue("Type", out var type))
                continue;

            Core.VisualizationObject obj = type switch
            {
                "LinePlot" => DeserializeLinePlot(properties),
                "SurfacePlot" => DeserializeSurfacePlot(properties),
                "MeshObject" => DeserializeMeshObject(properties),
                "PointCloud" => DeserializePointCloud(properties),
                _ => new Core.VisualizationObject()
            };

            if (properties.TryGetValue("Id", out var id))
                obj.Id = id;
            if (properties.TryGetValue("Color", out var color))
                obj.Color = color;

            objects.Add(obj);
        }

        return objects;
    }

    private static Core.LinePlot DeserializeLinePlot(Dictionary<string, string> props)
    {
        var linePlot = new Core.LinePlot();

        if (props.TryGetValue("LineWidth", out var lw))
            linePlot.LineWidth = double.Parse(lw);
        if (props.TryGetValue("IsDashed", out var dashed))
            linePlot.IsDashed = bool.Parse(dashed);

        if (props.TryGetValue("PointCount", out var countStr) && int.TryParse(countStr, out int count))
        {
            linePlot.Points = new List<System.Numerics.Vector2>();
            var lines = new List<string>();

            foreach (var kvp in props)
            {
                if (kvp.Key == "P")
                {
                    var parts = kvp.Value.Split(',');
                    if (parts.Length >= 2)
                    {
                        linePlot.Points.Add(new System.Numerics.Vector2(
                            float.Parse(parts[0]),
                            float.Parse(parts[1])));
                    }
                }
            }
        }

        return linePlot;
    }

    private static Core.SurfacePlot DeserializeSurfacePlot(Dictionary<string, string> props)
    {
        var surfacePlot = new Core.SurfacePlot();

        if (props.TryGetValue("FillColor", out var fc))
            surfacePlot.FillColor = fc;
        if (props.TryGetValue("Opacity", out var op))
            surfacePlot.Opacity = double.Parse(op);

        return surfacePlot;
    }

    private static Core.MeshObject DeserializeMeshObject(Dictionary<string, string> props)
    {
        var meshObj = new Core.MeshObject();

        if (props.TryGetValue("WireframeColor", out var wc))
            meshObj.WireframeColor = wc;
        if (props.TryGetValue("FillColor", out var fc))
            meshObj.FillColor = fc;

        return meshObj;
    }

    private static Core.PointCloud DeserializePointCloud(Dictionary<string, string> props)
    {
        var pointCloud = new Core.PointCloud();

        if (props.TryGetValue("PointSize", out var ps))
            pointCloud.PointSize = double.Parse(ps);

        return pointCloud;
    }

    private static (string? key, string value) ParseProperty(string line)
    {
        int separatorIndex = line.IndexOf(PropertySeparator[0]);
        if (separatorIndex < 0)
            return (null, line);

        string key = line.Substring(0, separatorIndex);
        string value = line.Substring(separatorIndex + 1);
        return (key, value);
    }
}
