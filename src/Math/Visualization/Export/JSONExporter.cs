namespace MathVerse.Math.Visualization.Export;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>JSON serialization options for scene export.</summary>
public sealed class SceneJsonOptions
{
    /// <summary>Gets or sets whether to indent the JSON output.</summary>
    public bool Indented { get; set; } = true;

    /// <summary>Gets or sets the maximum depth for serialization.</summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>Gets or sets whether to write comments.</summary>
    public bool WriteComments { get; set; }
}

/// <summary>Exports and imports visualization scenes to/from JSON format.</summary>
public sealed class JSONExporter
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Exports a visualization scene to a JSON file.</summary>
    /// <param name="scene">The scene to export.</param>
    /// <param name="filePath">The output file path.</param>
    public static void Export(Core.VisualizationScene scene, string filePath)
    {
        string json = SerializeScene(scene);
        System.IO.File.WriteAllText(filePath, json, Encoding.UTF8);
    }

    /// <summary>Imports a visualization scene from a JSON file.</summary>
    /// <param name="filePath">The input file path.</param>
    /// <returns>The deserialized visualization scene.</returns>
    public static Core.VisualizationScene Import(string filePath)
    {
        string json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
        return DeserializeScene(json);
    }

    /// <summary>Serializes a scene to JSON string.</summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string SerializeScene(Core.VisualizationScene scene)
    {
        var data = SceneToData(scene);
        return JsonSerializer.Serialize(data, s_options);
    }

    /// <summary>Deserializes a scene from JSON string.</summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized scene.</returns>
    public static Core.VisualizationScene DeserializeScene(string json)
    {
        var data = JsonSerializer.Deserialize<SceneData>(json, s_options);
        if (data == null)
            return new Core.VisualizationScene();

        return DataToScene(data);
    }

    /// <summary>Serializes scene to JSON bytes.</summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>The JSON bytes.</returns>
    public static byte[] SerializeToBytes(Core.VisualizationScene scene)
    {
        var data = SceneToData(scene);
        return JsonSerializer.SerializeToUtf8Bytes(data, s_options);
    }

    /// <summary>Deserializes scene from JSON bytes.</summary>
    /// <param name="bytes">The JSON bytes.</param>
    /// <returns>The deserialized scene.</returns>
    public static Core.VisualizationScene DeserializeFromBytes(byte[] bytes)
    {
        var data = JsonSerializer.Deserialize<SceneData>(bytes, s_options);
        if (data == null)
            return new Core.VisualizationScene();

        return DataToScene(data);
    }

    private static SceneData SceneToData(Core.VisualizationScene scene)
    {
        var data = new SceneData
        {
            Name = scene.Name ?? "",
            Version = "1.0",
            Objects = new List<ObjectData>()
        };

        if (scene.Objects != null)
        {
            foreach (var obj in scene.Objects)
            {
                data.Objects.Add(ObjectToData(obj));
            }
        }

        return data;
    }

    private static Core.VisualizationScene DataToScene(SceneData data)
    {
        var scene = new Core.VisualizationScene
        {
            Name = data.Name ?? ""
        };

        if (data.Objects != null)
        {
            scene.Objects = new List<Core.VisualizationObject>();
            foreach (var objData in data.Objects)
            {
                scene.Objects.Add(DataToObject(objData));
            }
        }

        return scene;
    }

    private static ObjectData ObjectToData(Core.VisualizationObject obj)
    {
        var data = new ObjectData
        {
            Type = obj.GetType().Name,
            Id = obj.Id ?? "",
            Color = obj.Color ?? "#000000",
            PositionX = obj.Position?.X ?? 0,
            PositionY = obj.Position?.Y ?? 0,
            PositionZ = obj.Position?.Z ?? 0
        };

        if (obj is Core.LinePlot linePlot)
        {
            data.ObjectType = "LinePlot";
            data.Points = new List<PointData>();

            if (linePlot.Points != null)
            {
                foreach (var pt in linePlot.Points)
                {
                    data.Points.Add(new PointData { X = pt.X, Y = pt.Y, Z = 0 });
                }
            }

            data.LineWidth = linePlot.LineWidth;
            data.IsDashed = linePlot.IsDashed;
        }
        else if (obj is Core.SurfacePlot surfacePlot)
        {
            data.ObjectType = "SurfacePlot";
            data.FillColor = surfacePlot.FillColor ?? "#4488CC";
            data.Opacity = surfacePlot.Opacity;
            data.Cells = new List<List<PointData>>();

            if (surfacePlot.Cells != null)
            {
                foreach (var cell in surfacePlot.Cells)
                {
                    var cellData = new List<PointData>();
                    if (cell != null)
                    {
                        foreach (var pt in cell)
                        {
                            cellData.Add(new PointData { X = pt.X, Y = pt.Y, Z = pt.Z });
                        }
                    }
                    data.Cells.Add(cellData);
                }
            }
        }
        else if (obj is Core.MeshObject meshObj)
        {
            data.ObjectType = "MeshObject";
            data.WireframeColor = meshObj.WireframeColor ?? "#333333";
            data.FillColor = meshObj.FillColor ?? "#CCCCCC";

            data.Vertices = new List<PointData>();
            if (meshObj.Vertices != null)
            {
                foreach (var v in meshObj.Vertices)
                {
                    data.Vertices.Add(new PointData { X = v.X, Y = v.Y, Z = v.Z });
                }
            }

            data.Faces = new List<List<int>>();
            if (meshObj.Faces != null)
            {
                foreach (var face in meshObj.Faces)
                {
                    data.Faces.Add(new List<int>(face));
                }
            }
        }
        else if (obj is Core.PointCloud pointCloud)
        {
            data.ObjectType = "PointCloud";
            data.Points = new List<PointData>();
            data.PointSize = pointCloud.PointSize;

            if (pointCloud.Points != null)
            {
                foreach (var pt in pointCloud.Points)
                {
                    data.Points.Add(new PointData { X = pt.X, Y = pt.Y, Z = pt.Z });
                }
            }
        }

        return data;
    }

    private static Core.VisualizationObject DataToObject(ObjectData data)
    {
        switch (data.ObjectType)
        {
            case "LinePlot":
                var linePlot = new Core.LinePlot
                {
                    Id = data.Id ?? "",
                    Color = data.Color ?? "#0000FF",
                    LineWidth = data.LineWidth,
                    IsDashed = data.IsDashed
                };

                if (data.Points != null)
                {
                    linePlot.Points = new List<System.Numerics.Vector2>();
                    foreach (var pt in data.Points)
                    {
                        linePlot.Points.Add(new System.Numerics.Vector2((float)pt.X, (float)pt.Y));
                    }
                }

                return linePlot;

            case "SurfacePlot":
                var surfacePlot = new Core.SurfacePlot
                {
                    Id = data.Id ?? "",
                    FillColor = data.FillColor ?? "#4488CC",
                    Opacity = data.Opacity
                };

                if (data.Cells != null)
                {
                    surfacePlot.Cells = new List<List<System.Numerics.Vector3>>();
                    foreach (var cell in data.Cells)
                    {
                        var cellPoints = new List<System.Numerics.Vector3>();
                        if (cell != null)
                        {
                            foreach (var pt in cell)
                            {
                                cellPoints.Add(new System.Numerics.Vector3((float)pt.X, (float)pt.Y, (float)pt.Z));
                            }
                        }
                        surfacePlot.Cells.Add(cellPoints);
                    }
                }

                return surfacePlot;

            case "MeshObject":
                var meshObj = new Core.MeshObject
                {
                    Id = data.Id ?? "",
                    WireframeColor = data.WireframeColor ?? "#333333",
                    FillColor = data.FillColor ?? "#CCCCCC"
                };

                if (data.Vertices != null)
                {
                    meshObj.Vertices = new List<System.Numerics.Vector3>();
                    foreach (var v in data.Vertices)
                    {
                        meshObj.Vertices.Add(new System.Numerics.Vector3((float)v.X, (float)v.Y, (float)v.Z));
                    }
                }

                if (data.Faces != null)
                {
                    meshObj.Faces = new List<int[]>();
                    foreach (var face in data.Faces)
                    {
                        meshObj.Faces.Add(face.ToArray());
                    }
                }

                return meshObj;

            case "PointCloud":
                var pointCloud = new Core.PointCloud
                {
                    Id = data.Id ?? "",
                    Color = data.Color ?? "#FF0000",
                    PointSize = data.PointSize
                };

                if (data.Points != null)
                {
                    pointCloud.Points = new List<System.Numerics.Vector3>();
                    foreach (var pt in data.Points)
                    {
                        pointCloud.Points.Add(new System.Numerics.Vector3((float)pt.X, (float)pt.Y, (float)pt.Z));
                    }
                }

                return pointCloud;

            default:
                return new Core.VisualizationObject
                {
                    Id = data.Id ?? "",
                    Color = data.Color ?? "#000000"
                };
        }
    }

    private sealed class SceneData
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public List<ObjectData>? Objects { get; set; }
    }

    private sealed class ObjectData
    {
        public string? Type { get; set; }
        public string? ObjectType { get; set; }
        public string? Id { get; set; }
        public string? Color { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public List<PointData>? Points { get; set; }
        public List<List<PointData>>? Cells { get; set; }
        public List<PointData>? Vertices { get; set; }
        public List<List<int>>? Faces { get; set; }
        public double LineWidth { get; set; }
        public bool IsDashed { get; set; }
        public string? FillColor { get; set; }
        public string? WireframeColor { get; set; }
        public double Opacity { get; set; }
        public double PointSize { get; set; }
    }

    private sealed class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
