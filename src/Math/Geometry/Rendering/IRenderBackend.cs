namespace MathVerse.Math.Geometry.Rendering;

using Meshes;
using SceneGraph;

/// <summary>Backend-independent rendering interface.</summary>
public interface IRenderBackend
{
    /// <summary>Backend name.</summary>
    string Name { get; }
    
    /// <summary>Initializes the rendering backend.</summary>
    void Initialize(int width, int height);
    
    /// <summary>Resizes the render target.</summary>
    void Resize(int width, int height);
    
    /// <summary>Begins a new frame.</summary>
    void BeginFrame();
    
    /// <summary>Ends the current frame.</summary>
    void EndFrame();
    
    /// <summary>Submits a mesh for rendering.</summary>
    void SubmitMesh(TriangleMesh mesh, Transformations.Transform3D transform, string materialName);
    
    /// <summary>Renders the current scene.</summary>
    void Render(Scene scene);
    
    /// <summary>Shuts down the backend.</summary>
    void Shutdown();
}
