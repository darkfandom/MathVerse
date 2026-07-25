using System.Collections.Concurrent;

namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// A thread-safe registry for quantum service factories, enabling service locator patterns.
/// </summary>
public sealed class QuantumRegistry
{
    private readonly ConcurrentDictionary<string, Func<object>> _factories;

    /// <summary>
    /// Gets the number of registered services.
    /// </summary>
    public int Count => _factories.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumRegistry"/> class.
    /// </summary>
    public QuantumRegistry()
    {
        _factories = new ConcurrentDictionary<string, Func<object>>();
    }

    /// <summary>
    /// Registers a service factory under the specified name.
    /// </summary>
    /// <typeparam name="T">The type of service to register.</typeparam>
    /// <param name="name">The name to register the service under.</param>
    /// <param name="factory">A factory function that creates instances of the service.</param>
    public void Register<T>(string name, Func<T> factory) where T : class
    {
        _factories[name ?? throw new ArgumentNullException(nameof(name))] = () => factory();
    }

    /// <summary>
    /// Resolves a registered service by name and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected service type.</typeparam>
    /// <param name="name">The registered service name.</param>
    /// <returns>The resolved service, or <c>default</c> if not found or cast fails.</returns>
    public T? Resolve<T>(string name) where T : class
    {
        if (_factories.TryGetValue(name ?? throw new ArgumentNullException(nameof(name)), out Func<object>? factory))
        {
            return factory() as T;
        }
        return null;
    }

    /// <summary>
    /// Determines whether a service is registered under the specified name.
    /// </summary>
    /// <param name="name">The service name to check.</param>
    /// <returns><c>true</c> if the service is registered; otherwise, <c>false</c>.</returns>
    public bool IsRegistered(string name)
    {
        return _factories.ContainsKey(name ?? throw new ArgumentNullException(nameof(name)));
    }
}
