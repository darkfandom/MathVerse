namespace MathVerse.Math.Distributed.MessagePassing;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/// <summary>
/// Registry of named remote procedures that can be invoked by other nodes.
/// Each procedure is identified by a unique name and backed by an async handler.
/// </summary>
public sealed class RemoteProcedure
{
    private readonly ConcurrentDictionary<string, Func<byte[], ValueTask<byte[]>>> _handlers = new();

    /// <summary>
    /// Registers a remote procedure with the given name and handler.
    /// </summary>
    /// <param name="name">Unique name identifying the procedure.</param>
    /// <param name="handler">Async handler that receives serialized arguments and returns serialized results.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="handler"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a procedure with the same name is already registered.</exception>
    public void Register(string name, Func<byte[], ValueTask<byte[]>> handler)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        if (!_handlers.TryAdd(name, handler))
            throw new InvalidOperationException($"Procedure '{name}' is already registered.");
    }

    /// <summary>
    /// Removes a previously registered remote procedure.
    /// </summary>
    /// <param name="name">The name of the procedure to unregister.</param>
    /// <returns>True if the procedure was found and removed; otherwise, false.</returns>
    public bool Unregister(string name)
    {
        return _handlers.TryRemove(name, out _);
    }

    /// <summary>
    /// Invokes a registered remote procedure by name.
    /// </summary>
    /// <param name="name">The name of the procedure to invoke.</param>
    /// <param name="args">Serialized arguments to pass to the handler.</param>
    /// <returns>The serialized result from the handler.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no procedure with the given name is registered.</exception>
    public async ValueTask<byte[]> Invoke(string name, byte[] args)
    {
        if (!_handlers.TryGetValue(name, out var handler))
            throw new KeyNotFoundException($"Procedure '{name}' is not registered.");

        return await handler(args).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks whether a procedure with the given name is registered.
    /// </summary>
    /// <param name="name">The procedure name to check.</param>
    /// <returns>True if the procedure is registered; otherwise, false.</returns>
    public bool IsRegistered(string name)
    {
        return _handlers.ContainsKey(name);
    }
}
