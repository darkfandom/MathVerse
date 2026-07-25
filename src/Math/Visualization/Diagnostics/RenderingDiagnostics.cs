namespace MathVerse.Math.Visualization.Diagnostics;

/// <summary>Collects and tracks rendering performance diagnostics across multiple frames.</summary>
public sealed class RenderingDiagnostics
{
    private double _totalRenderTimeMs;
    private int _totalDrawCalls;
    private int _totalTriangles;
    private int _frameCount;

    /// <summary>Gets the total number of frames recorded.</summary>
    public int FrameCount => _frameCount;

    /// <summary>Gets the average render time per frame in milliseconds.</summary>
    public double AverageRenderTimeMs => _frameCount > 0 ? _totalRenderTimeMs / _frameCount : 0.0;

    /// <summary>Gets the average draw calls per frame.</summary>
    public double AverageDrawCalls => _frameCount > 0 ? (double)_totalDrawCalls / _frameCount : 0.0;

    /// <summary>Gets the average triangles rendered per frame.</summary>
    public double AverageTriangles => _frameCount > 0 ? (double)_totalTriangles / _frameCount : 0.0;

    /// <summary>Records the statistics for a single rendered frame.</summary>
    /// <param name="renderTimeMs">The time taken to render the frame in milliseconds.</param>
    /// <param name="drawCalls">The number of draw calls issued.</param>
    /// <param name="triangles">The number of triangles rendered.</param>
    public void RecordFrame(double renderTimeMs, int drawCalls, int triangles)
    {
        _totalRenderTimeMs += renderTimeMs;
        _totalDrawCalls += drawCalls;
        _totalTriangles += triangles;
        _frameCount++;
    }

    /// <summary>Resets all accumulated diagnostics data.</summary>
    public void Reset()
    {
        _totalRenderTimeMs = 0.0;
        _totalDrawCalls = 0;
        _totalTriangles = 0;
        _frameCount = 0;
    }
}
