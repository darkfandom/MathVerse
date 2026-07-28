using System.Numerics;

namespace MathVerse.Desktop.Models;

public readonly record struct BoundingBox(Vector3 Min, Vector3 Max);

public interface IWorkspaceObject
{
    Guid Id { get; }
    string Name { get; set; }
    string Icon { get; set; }
    string TypeTag { get; }
    bool IsVisible { get; set; }
    bool IsLocked { get; set; }
    bool IsPinned { get; set; }
    bool IsSelected { get; set; }
    bool IsExpanded { get; set; }
    List<string> Tags { get; }
    string Category { get; set; }
    Guid? ParentId { get; set; }
    List<Guid> Children { get; }
    Matrix4x4 Transform { get; set; }
    Dictionary<string, object> Metadata { get; }
    BoundingBox? BoundingBox { get; set; }
    int Layer { get; set; }
    Guid? Owner { get; set; }
    DateTime CreatedAt { get; }
    DateTime ModifiedAt { get; set; }

    IWorkspaceObject Clone();
    byte[] Serialize();
    void Destroy();
    IWorkspaceObject Duplicate();
    void Select();
    void Deselect();
}
