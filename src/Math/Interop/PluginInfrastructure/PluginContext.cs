namespace MathVerse.Math.Interop.PluginInfrastructure;

using System;
using System.Collections.Concurrent;

/// <summary>
/// Provides context to a plugin including service access and metadata.
/// </summary>
public sealed class PluginContext
{
    private readonly ConcurrentDictionary<Type, object> _services = new();

    /// <summary>
    /// Gets the plugin identifier this context is associated with.
    /// </summary>
    public string PluginId { get; }

    /// <summary>
    /// Gets the timestamp when this plugin was loaded.
    /// </summary>
    public DateTimeOffset LoadedAt { get; }

    /// <summary>
    /// Gets the services dictionary for this plugin context.
    /// </summary>
    public ConcurrentDictionary<string, object> Services { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginContext"/> class.
    /// </summary>
    /// <param name="pluginId">The identifier of the owning plugin.</param>
    public PluginContext(string pluginId)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        PluginId = pluginId;
        LoadedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets a registered service of the specified type.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance, or null if not registered.</returns>
    public T? GetService<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        return null;
    }

    /// <summary>
    /// Registers a service instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="service">The service instance to register.</param>
    public void RegisterService<T>(T service) where T : class
    {
        ArgumentNullException.ThrowIfNull(service);
        _services[typeof(T)] = service;
    }
}
