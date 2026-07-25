namespace MathVerse.Math.DataScience.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Registry for data connectors and transformers using factory functions.
/// </summary>
public sealed class DataRegistry
{
    private readonly Dictionary<string, Func<object>> _factories = new();

    /// <summary>
    /// Gets the number of registered factories.
    /// </summary>
    public int Count => _factories.Count;

    /// <summary>
    /// Registers a factory function for creating instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object the factory creates.</typeparam>
    /// <param name="name">The registered name for the factory.</param>
    /// <param name="factory">The factory function.</param>
    public void Register<T>(string name, Func<T> factory)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        _factories[name] = () => factory()!;
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the registered factory.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="name">The registered name of the factory.</param>
    /// <returns>A new instance created by the factory.</returns>
    public T Create<T>(string name)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        if (!_factories.TryGetValue(name, out var factory))
        {
            throw new KeyNotFoundException($"Factory '{name}' is not registered.");
        }
        return (T)factory();
    }

    /// <summary>
    /// Determines whether a factory with the specified name is registered.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>true if a factory with the specified name exists; otherwise, false.</returns>
    public bool Contains(string name)
    {
        return _factories.ContainsKey(name);
    }
}