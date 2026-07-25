namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Policy configuration for creating and resetting pooled objects.
/// </summary>
/// <typeparam name="T">The type of pooled objects.</typeparam>
public sealed class PoolPolicy<T> where T : class
{
    /// <summary>
    /// Gets or sets the factory function used to create new instances.
    /// </summary>
    public Func<T> Factory { get; set; } = () => throw new InvalidOperationException("Factory not configured.");

    /// <summary>
    /// Gets or sets the action used to reset an instance before returning it to the pool.
    /// </summary>
    public Action<T> ResetAction { get; set; } = static _ => { };
}
