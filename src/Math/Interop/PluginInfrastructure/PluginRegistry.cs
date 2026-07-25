namespace MathVerse.Math.Interop.PluginInfrastructure;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Thread-safe registry of available plugins using a concurrent dictionary.
/// </summary>
public sealed class PluginRegistry
{
    private readonly ConcurrentDictionary<string, PluginDescriptor> _plugins = new();
    private readonly ConcurrentDictionary<string, List<string>> _capabilityIndex = new();

    /// <summary>
    /// Gets the number of registered plugins.
    /// </summary>
    public int Count => _plugins.Count;

    /// <summary>
    /// Registers a plugin descriptor in the registry.
    /// </summary>
    /// <param name="descriptor">The plugin descriptor to register.</param>
    public void Register(PluginDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentException.ThrowIfNullOrEmpty(descriptor.Id);

        _plugins[descriptor.Id] = descriptor;

        if (descriptor.Metadata.TryGetValue("Capability", out var capability))
        {
            _capabilityIndex.AddOrUpdate(
                capability,
                _ => new List<string> { descriptor.Id },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        if (!existing.Contains(descriptor.Id))
                        {
                            existing.Add(descriptor.Id);
                        }
                    }
                    return existing;
                });
        }
    }

    /// <summary>
    /// Finds a plugin by its identifier.
    /// </summary>
    /// <param name="id">The plugin identifier.</param>
    /// <returns>The plugin descriptor, or null if not found.</returns>
    public PluginDescriptor? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        _plugins.TryGetValue(id, out var descriptor);
        return descriptor;
    }

    /// <summary>
    /// Gets all registered plugin descriptors.
    /// </summary>
    /// <returns>A read-only list of all registered plugins.</returns>
    public IReadOnlyList<PluginDescriptor> GetAll()
    {
        return _plugins.Values.ToArray();
    }

    /// <summary>
    /// Finds plugins by a specific capability tag stored in metadata.
    /// </summary>
    /// <param name="capability">The capability to search for.</param>
    /// <returns>A list of matching plugin descriptors.</returns>
    public IReadOnlyList<PluginDescriptor> FindByCapability(string capability)
    {
        ArgumentException.ThrowIfNullOrEmpty(capability);

        if (!_capabilityIndex.TryGetValue(capability, out var ids))
        {
            return Array.Empty<PluginDescriptor>();
        }

        var result = new List<PluginDescriptor>();
        lock (ids)
        {
            foreach (var id in ids)
            {
                if (_plugins.TryGetValue(id, out var desc))
                {
                    result.Add(desc);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Determines whether a plugin with the specified ID is registered.
    /// </summary>
    /// <param name="id">The plugin identifier.</param>
    /// <returns>True if the plugin is registered.</returns>
    public bool IsRegistered(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        return _plugins.ContainsKey(id);
    }
}
