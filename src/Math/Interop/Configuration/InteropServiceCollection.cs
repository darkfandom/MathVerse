namespace MathVerse.Math.Interop.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Simple service collection for interop dependencies. AOT-compatible replacement for DI containers.
/// </summary>
public sealed class InteropServiceCollection
{
    private readonly ConcurrentDictionary<Type, Func<object>> _factories = new();
    private readonly ConcurrentDictionary<Type, object> _singletons = new();

    /// <summary>
    /// Registers a singleton service instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="instance">The service instance.</param>
    public void RegisterSingleton<T>(T instance) where T : class
    {
        _ = instance ?? throw new ArgumentNullException(nameof(instance));
        _singletons[typeof(T)] = instance;
    }

    /// <summary>
    /// Registers a transient service factory.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="factory">The factory function.</param>
    public void RegisterTransient<T>(Func<T> factory) where T : class
    {
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        _factories[typeof(T)] = () => factory();
    }

    /// <summary>
    /// Resolves a registered service.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance, or null if not registered.</returns>
    public T? Resolve<T>() where T : class
    {
        if (_singletons.TryGetValue(typeof(T), out var singleton))
        {
            return (T)singleton;
        }
        if (_factories.TryGetValue(typeof(T), out var factory))
        {
            return (T)factory();
        }
        return null;
    }

    /// <summary>
    /// Determines whether a service type is registered.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>True if registered.</returns>
    public bool IsRegistered<T>() where T : class
    {
        return _singletons.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
    }
}
