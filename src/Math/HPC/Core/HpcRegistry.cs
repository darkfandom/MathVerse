namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Thread-safe service registry for HPC subsystems.
/// </summary>
public sealed class HpcRegistry : IDisposable
{
    private readonly ConcurrentDictionary<string, Func<object>> _factories;
    private readonly ConcurrentDictionary<string, object> _instances;
    private readonly ConcurrentDictionary<string, Type> _types;
    private bool _disposed;

    public HpcRegistry()
    {
        _factories = new ConcurrentDictionary<string, Func<object>>();
        _instances = new ConcurrentDictionary<string, object>();
        _types = new ConcurrentDictionary<string, Type>();
    }

    /// <summary>
    /// Registers a service factory.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="factory">The factory function.</param>
    public void Register<T>(Func<T> factory)
        where T : class
    {
        var key = typeof(T).FullName!;
        _factories.TryAdd(key, () => factory()!);
        _types.TryAdd(key, typeof(T));
    }

    /// <summary>
    /// Registers a singleton instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="instance">The instance to register.</param>
    public void RegisterInstance<T>(T instance)
        where T : class
    {
        var key = typeof(T).FullName!;
        _instances.TryAdd(key, instance);
        _types.TryAdd(key, typeof(T));
    }

    /// <summary>
    /// Registers a service by type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="factory">The factory function.</param>
    public void Register(Type serviceType, Func<object> factory)
    {
        var key = serviceType.FullName!;
        _factories.TryAdd(key, factory);
        _types.TryAdd(key, serviceType);
    }

    /// <summary>
    /// Resolves a service instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    public T Resolve<T>()
        where T : class
    {
        var key = typeof(T).FullName!;
        if (_instances.TryGetValue(key, out var instance))
        {
            return (T)instance;
        }

        if (_factories.TryGetValue(key, out var factory))
        {
            var newInstance = (T)factory();
            _instances.TryAdd(key, newInstance);
            return newInstance;
        }

        throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
    }

    /// <summary>
    /// Tries to resolve a service instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="instance">The resolved instance.</param>
    /// <returns>True if resolved; otherwise, false.</returns>
    public bool TryResolve<T>(out T? instance)
        where T : class
    {
        var key = typeof(T).FullName!;
        if (_instances.TryGetValue(key, out var existing))
        {
            instance = (T)existing;
            return true;
        }

        if (_factories.TryGetValue(key, out var factory))
        {
            var newInstance = (T)factory();
            _instances.TryAdd(key, newInstance);
            instance = newInstance;
            return true;
        }

        instance = null;
        return false;
    }

    /// <summary>
    /// Checks if a service is registered.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>True if registered; otherwise, false.</returns>
    public bool IsRegistered<T>()
        where T : class
    {
        var key = typeof(T).FullName!;
        return _instances.ContainsKey(key) || _factories.ContainsKey(key);
    }

    /// <summary>
    /// Checks if a service is registered by type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <returns>True if registered; otherwise, false.</returns>
    public bool IsRegistered(Type serviceType)
    {
        var key = serviceType.FullName!;
        return _instances.ContainsKey(key) || _factories.ContainsKey(key);
    }

    /// <summary>
    /// Unregisters a service.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>True if unregistered; otherwise, false.</returns>
    public bool Unregister<T>()
        where T : class
    {
        var key = typeof(T).FullName!;
        _instances.TryRemove(key, out _);
        _factories.TryRemove(key, out _);
        _types.TryRemove(key, out _);
        return true;
    }

    /// <summary>
    /// Gets all registered service types.
    /// </summary>
    public IReadOnlyList<Type> GetRegisteredTypes()
    {
        return _types.Values.ToArray();
    }

    /// <summary>
    /// Clears all registrations.
    /// </summary>
    public void Clear()
    {
        _instances.Clear();
        _factories.Clear();
        _types.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var instance in _instances.Values)
        {
            if (instance is IDisposable disposable)
            {
                try { disposable.Dispose(); } catch { }
            }
        }

        _instances.Clear();
        _factories.Clear();
        _types.Clear();
    }
}
