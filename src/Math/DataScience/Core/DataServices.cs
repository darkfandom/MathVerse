namespace MathVerse.Math.DataScience.Core;

using System;

/// <summary>
/// Lazy service locator for data science services.
/// </summary>
public sealed class DataServices
{
    private static readonly Lazy<DataServices> _instance = new(() => new DataServices());

    private readonly DataRegistry _registry = new();

    /// <summary>
    /// Gets the singleton instance of <see cref="DataServices"/>.
    /// </summary>
    public static DataServices Instance => _instance.Value;

    /// <summary>
    /// Gets the underlying data registry.
    /// </summary>
    public DataRegistry Registry => _registry;

    private DataServices()
    {
    }

    /// <summary>
    /// Registers a factory function for creating instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object the factory creates.</typeparam>
    /// <param name="name">The registered name for the factory.</param>
    /// <param name="factory">The factory function.</param>
    public void Register<T>(string name, Func<T> factory)
    {
        _registry.Register(name, factory);
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using the registered factory.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="name">The registered name of the factory.</param>
    /// <returns>A new instance created by the factory.</returns>
    public T Resolve<T>(string name)
    {
        return _registry.Create<T>(name);
    }
}