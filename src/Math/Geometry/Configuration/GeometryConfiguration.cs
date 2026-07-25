namespace MathVerse.Math.Geometry.Configuration;

/// <summary>Fluent builder for geometry options.</summary>
public sealed class GeometryConfiguration
{
    private double _tolerance = 1e-10;
    private bool _enableCaching = true;
    private bool _enableParallelProcessing = true;
    private int _maxParallelism = Environment.ProcessorCount;
    private bool _enableDiagnostics = false;
    private bool _validateOnCreate = false;
    
    /// <summary>Sets the numerical tolerance.</summary>
    public GeometryConfiguration WithTolerance(double tolerance) { _tolerance = tolerance; return this; }
    
    /// <summary>Enables or disables caching.</summary>
    public GeometryConfiguration WithCaching(bool enable) { _enableCaching = enable; return this; }
    
    /// <summary>Enables or disables parallel processing.</summary>
    public GeometryConfiguration WithParallelProcessing(bool enable) { _enableParallelProcessing = enable; return this; }
    
    /// <summary>Sets maximum parallelism.</summary>
    public GeometryConfiguration WithMaxParallelism(int max) { _maxParallelism = max; return this; }
    
    /// <summary>Enables or disables diagnostics.</summary>
    public GeometryConfiguration WithDiagnostics(bool enable) { _enableDiagnostics = enable; return this; }
    
    /// <summary>Enables or disables validation on creation.</summary>
    public GeometryConfiguration WithValidation(bool enable) { _validateOnCreate = enable; return this; }
    
    /// <summary>Builds the geometry options.</summary>
    public GeometryOptions Build() => new()
    {
        Tolerance = _tolerance,
        EnableCaching = _enableCaching,
        EnableParallelProcessing = _enableParallelProcessing,
        MaxParallelism = _maxParallelism,
        EnableDiagnostics = _enableDiagnostics,
        ValidateOnCreate = _validateOnCreate
    };
}
