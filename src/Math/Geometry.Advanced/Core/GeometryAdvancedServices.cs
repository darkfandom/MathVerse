using System;

namespace MathVerse.Math.Geometry.Advanced;

/// <summary>
/// Provides global initialization, shutdown, and configuration management for the advanced
/// geometry processing subsystem. Manages the lifecycle of pooled resources and exposes
/// the currently active configuration.
/// </summary>
public static class GeometryAdvancedServices
{
    private static volatile bool _initialized;

    /// <summary>
    /// Gets the currently active configuration for the advanced geometry engine.
    /// Defaults to <see cref="GeometryAdvancedConfiguration.Default"/> if not explicitly configured.
    /// </summary>
    public static GeometryAdvancedConfiguration CurrentConfiguration { get; private set; } = GeometryAdvancedConfiguration.Default;

    /// <summary>
    /// Initializes the advanced geometry subsystem, preparing object pools and caches
    /// for efficient geometry processing. Safe to call multiple times; subsequent calls
    /// are no-ops if initialization has already occurred.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
    }

    /// <summary>
    /// Shuts down the advanced geometry subsystem and releases all pooled resources.
    /// After shutdown, pools will be lazily re-initialized on the next call to <see cref="Initialize"/>.
    /// </summary>
    public static void Shutdown()
    {
        if (!_initialized) return;

        if (CurrentConfiguration.Options.UseObjectPooling)
            Performance.GeometryPerformancePool.ClearPools();

        _initialized = false;
    }

    /// <summary>
    /// Sets the active configuration for the advanced geometry engine, affecting all
    /// subsequent geometry operations. Can be called at any time to reconfigure the engine.
    /// </summary>
    /// <param name="config">The new configuration to apply. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public static void Configure(GeometryAdvancedConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        CurrentConfiguration = config;
    }
}
