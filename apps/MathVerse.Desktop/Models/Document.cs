namespace MathVerse.Desktop.Models;

public enum DocumentType
{
    Notebook,
    Graph,
    Simulation,
    Geometry,
    Scene
}

public sealed class Document
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public Scene Scene { get; } = new();
    public Dictionary<string, object> Metadata { get; } = [];
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Scene
{
    public List<Guid> Objects { get; } = [];
    public CameraState Camera { get; set; } = new();
    public List<LightState> Lights { get; } = [];
}

public sealed class CameraState
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; } = -10;
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float TargetZ { get; set; }
    public float Fov { get; set; } = 45;
    public float NearClip { get; set; } = 0.1f;
    public float FarClip { get; set; } = 1000f;
}

public sealed class LightState
{
    public string Type { get; set; } = "Directional";
    public float R { get; set; } = 1;
    public float G { get; set; } = 1;
    public float B { get; set; } = 1;
    public float Intensity { get; set; } = 1;
    public float DirectionX { get; set; }
    public float DirectionY { get; set; } = -1;
    public float DirectionZ { get; set; }
}
