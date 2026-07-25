namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Frustum culling engine that filters objects based on camera visibility.</summary>
public sealed class FrustumCuller
{
    /// <summary>Filters a list of visualization objects, returning only those that are partially or fully inside the view frustum.</summary>
    /// <param name="objects">The list of visualization objects to test.</param>
    /// <param name="frustum">The view frustum to test against.</param>
    /// <returns>A list containing only the visible objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="objects"/> or <paramref name="frustum"/> is <c>null</c>.</exception>
    public List<Core.VisualizationObject> Cull(List<Core.VisualizationObject> objects, Frustum frustum)
    {
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(frustum);

        List<Core.VisualizationObject> visible = [];
        for (int i = 0; i < objects.Count; i++)
        {
            Core.VisualizationObject obj = objects[i];
            BoundingBox bounds = obj.Bounds;

            if (frustum.ContainsBox(bounds))
            {
                visible.Add(obj);
            }
        }

        return visible;
    }

    /// <summary>Performs frustum culling on a list of bounding boxes, returning only those that pass the test.</summary>
    /// <param name="bounds">The list of bounding boxes to test.</param>
    /// <param name="frustum">The view frustum to test against.</param>
    /// <returns>A list of indices for bounding boxes that are visible.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bounds"/> or <paramref name="frustum"/> is <c>null</c>.</exception>
    public List<int> CullBounds(List<BoundingBox> bounds, Frustum frustum)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        ArgumentNullException.ThrowIfNull(frustum);

        List<int> visibleIndices = [];
        for (int i = 0; i < bounds.Count; i++)
        {
            if (frustum.ContainsBox(bounds[i]))
            {
                visibleIndices.Add(i);
            }
        }

        return visibleIndices;
    }
}

/// <summary>Level-of-detail culling engine that selects appropriate detail levels based on distance to camera.</summary>
public sealed class LODCuller
{
    private readonly LOD.LODManager _lodManager = new();

    /// <summary>Selects the appropriate level of detail index for an object based on its distance from the camera.</summary>
    /// <param name="objectCenter">The world-space center of the object.</param>
    /// <param name="cameraPosition">The world-space position of the camera.</param>
    /// <returns>The selected LOD index, where 0 is the highest detail.</returns>
    public int SelectLOD(Vector3 objectCenter, Vector3 cameraPosition) =>
        _lodManager.SelectLOD(objectCenter, cameraPosition);

    /// <summary>Selects the appropriate LOD for an object using a distance-based heuristic.</summary>
    /// <param name="objectCenter">The world-space center of the object.</param>
    /// <param name="cameraPosition">The world-space position of the camera.</param>
    /// <param name="screenHeight">The viewport height in pixels.</param>
    /// <param name="objectWorldRadius">The approximate world-space radius of the object.</param>
    /// <param name="lodCount">The total number of available LOD levels.</param>
    /// <param name="pixelThreshold">The minimum pixel size to trigger a higher LOD. Defaults to 64.</param>
    /// <returns>The selected LOD index, where 0 is the highest detail.</returns>
    public static int SelectLOD(
        Vector3 objectCenter,
        Vector3 cameraPosition,
        float screenHeight,
        float objectWorldRadius,
        int lodCount,
        float pixelThreshold = 64.0f)
    {
        if (lodCount <= 0)
            return 0;

        float distance = Vector3.Distance(cameraPosition, objectCenter);
        if (distance < 1e-6f)
            return 0;

        float fovRad = 60.0f * System.MathF.PI / 180.0f;
        float projectedSize = (objectWorldRadius / distance) * (screenHeight / (2.0f * System.MathF.Tan(fovRad * 0.5f)));
        float lodRatio = projectedSize / pixelThreshold;

        int lod = System.Math.Clamp(
            (int)System.Math.Floor(System.Math.Log(System.Math.Max(lodRatio, 1e-6f), 2.0f)),
            0,
            lodCount - 1);

        return lod;
    }
}

/// <summary>Defines the output target for render operations, specifying resolution and format.</summary>
public sealed class RenderTarget
{
    /// <summary>Gets the width of the render target in pixels.</summary>
    public int Width { get; init; }

    /// <summary>Gets the height of the render target in pixels.</summary>
    public int Height { get; init; }

    /// <summary>Gets the number of MSAA samples for this render target. Set to 1 for no multisampling.</summary>
    public int Samples { get; init; } = 1;

    /// <summary>Gets the pixel format string (e.g., "RGBA8", "RGBA16F", "RGBA32F").</summary>
    public string PixelFormat { get; init; } = "RGBA8";

    /// <summary>Gets the aspect ratio of the render target.</summary>
    public float AspectRatio => Width > 0 && Height > 0 ? (float)Width / Height : 1.0f;
}

/// <summary>Contains aggregated statistics from a single frame rendered through the pipeline.</summary>
public sealed class RenderFrameResult
{
    /// <summary>Gets the total number of draw calls issued during the frame.</summary>
    public int DrawCalls { get; init; }

    /// <summary>Gets the total number of triangles rendered during the frame.</summary>
    public int TrianglesRendered { get; init; }

    /// <summary>Gets the total number of vertices processed during the frame.</summary>
    public int VerticesProcessed { get; init; }

    /// <summary>Gets the total GPU render time for the frame in milliseconds.</summary>
    public double RenderTimeMs { get; init; }

    /// <summary>Gets the number of render passes executed during the frame.</summary>
    public int PassCount { get; init; }
}
