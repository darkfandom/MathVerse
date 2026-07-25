namespace MathVerse.Math.AI.Core;

using System.Collections.Concurrent;

/// <summary>Service locator for AI subsystems, providing lazy-initialized access to all engines.</summary>
public sealed class AIServices
{
    private readonly AIConfiguration _configuration;
    private readonly ConcurrentDictionary<Type, object> _services = new();
    private readonly Lazy<AIRegistry> _registry;
    private readonly Lazy<AIContext> _context;

    /// <summary>Initialises the service locator.</summary>
    /// <param name="configuration">Optional configuration; uses <see cref="AIConfiguration.Default"/> when <c>null</c>.</param>
    public AIServices(AIConfiguration? configuration = null)
    {
        _configuration = configuration ?? AIConfiguration.Default;
        _registry = new Lazy<AIRegistry>(() => AIRegistry.CreateDefault(), LazyThreadSafetyMode.ExecutionAndPublication);
        _context = new Lazy<AIContext>(() => new AIContext(_configuration), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>Gets the global model/algorithm registry.</summary>
    public AIRegistry Registry => _registry.Value;

    /// <summary>Gets the shared execution context.</summary>
    public AIContext Context => _context.Value;

    /// <summary>Gets or creates a lazily-initialised service of the specified type.</summary>
    /// <typeparam name="T">Service type. Must have a parameterless constructor.</typeparam>
    /// <returns>The singleton service instance.</returns>
    public T GetService<T>() where T : class, new()
    {
        return (T)_services.GetOrAdd(typeof(T), _ => new T());
    }

    /// <summary>Gets or creates a lazily-initialised service using a factory delegate.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <param name="factory">Factory delegate invoked at most once.</param>
    /// <returns>The singleton service instance.</returns>
    public T GetService<T>(Func<T> factory) where T : class
    {
        return (T)_services.GetOrAdd(typeof(T), _ => factory());
    }

    /// <summary>Registers a pre-built service instance.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <param name="instance">Instance to register.</param>
    public void RegisterService<T>(T instance) where T : class
    {
        _services[typeof(T)] = instance;
    }

    /// <summary>Returns <c>true</c> when a service of the specified type has been registered.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns><c>true</c> if registered.</returns>
    public bool HasService<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>Removes a previously registered service.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns><c>true</c> if the service was present and removed.</returns>
    public bool RemoveService<T>() where T : class
    {
        return _services.TryRemove(typeof(T), out _);
    }

    /// <summary>Returns the configuration used by this service locator.</summary>
    public AIConfiguration Configuration => _configuration;
}
