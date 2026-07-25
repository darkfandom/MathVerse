namespace MathVerse.Math.Interop.PluginInfrastructure;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages plugin loading and unloading using a pre-registered factory map pattern.
/// No runtime reflection is used — plugins must be registered via factory delegates.
/// </summary>
public sealed class PluginLoader
{
    private readonly ConcurrentDictionary<string, PluginDescriptor> _loadedPlugins = new();
    private readonly ConcurrentDictionary<string, Func<PluginDescriptor, object>> _factories = new();

    /// <summary>
    /// Registers a factory delegate for creating plugin instances.
    /// This must be called before loading a plugin with the specified ID.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="factory">The factory function to create the plugin instance.</param>
    public void RegisterFactory(string pluginId, Func<PluginDescriptor, object> factory)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        ArgumentNullException.ThrowIfNull(factory);
        _factories[pluginId] = factory;
    }

    /// <summary>
    /// Loads a plugin by its identifier using the pre-registered factory.
    /// </summary>
    /// <param name="pluginId">The plugin identifier to load.</param>
    /// <param name="descriptor">The descriptor of the plugin to load.</param>
    public void LoadPlugin(string pluginId, PluginDescriptor descriptor)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        ArgumentNullException.ThrowIfNull(descriptor);

        _loadedPlugins[pluginId] = descriptor;
    }

    /// <summary>
    /// Determines whether a plugin with the specified ID is loaded.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>True if the plugin is loaded.</returns>
    public bool IsLoaded(string pluginId)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        return _loadedPlugins.ContainsKey(pluginId);
    }

    /// <summary>
    /// Unloads a plugin by its identifier.
    /// </summary>
    /// <param name="pluginId">The plugin identifier to unload.</param>
    public void UnloadPlugin(string pluginId)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginId);
        _loadedPlugins.TryRemove(pluginId, out _);
    }

    /// <summary>
    /// Gets the identifiers of all currently loaded plugins.
    /// </summary>
    /// <returns>A read-only list of loaded plugin IDs.</returns>
    public IReadOnlyList<string> GetLoadedPluginIds()
    {
        return _loadedPlugins.Keys.ToArray();
    }
}
