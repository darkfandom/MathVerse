namespace MathVerse.Math.AI.Integration;

using System.Collections.Immutable;

/// <summary>Registry mapping AI capabilities to MathVerse subsystem integrations.</summary>
public sealed class AIIntegrationRegistry
{
    private readonly Dictionary<string, Func<string, string>> _integrationHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _descriptions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _registeredNames = [];

    /// <summary>Gets the number of registered integrations.</summary>
    public int Count => _registeredNames.Count;

    /// <summary>Gets the names of all registered integrations.</summary>
    public IReadOnlyList<string> RegisteredNames => _registeredNames.AsReadOnly();

    /// <summary>Registers a new integration handler with a descriptive name.</summary>
    /// <param name="integrationName">The unique name for this integration.</param>
    /// <param name="handler">The function that processes input and returns output.</param>
    /// <param name="description">Optional description of what this integration does.</param>
    /// <exception cref="ArgumentException">Thrown when the integration name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void Register(string integrationName, Func<string, string> handler, string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(integrationName);
        ArgumentNullException.ThrowIfNull(handler);

        _integrationHandlers[integrationName] = handler;
        _descriptions[integrationName] = description ?? "";

        if (!_registeredNames.Contains(integrationName))
        {
            _registeredNames.Add(integrationName);
        }
    }

    /// <summary>Executes the named integration handler with the given input.</summary>
    /// <param name="integrationName">The name of the integration to execute.</param>
    /// <param name="input">The input string to process.</param>
    /// <returns>The output from the handler, or null if the integration was not found.</returns>
    public string? Execute(string integrationName, string input)
    {
        if (_integrationHandlers.TryGetValue(integrationName, out var handler))
        {
            return handler(input);
        }

        return null;
    }

    /// <summary>Determines whether an integration with the given name is registered.</summary>
    /// <param name="name">The integration name to check.</param>
    /// <returns>true if the integration exists; otherwise, false.</returns>
    public bool HasIntegration(string name)
    {
        return _integrationHandlers.ContainsKey(name);
    }

    /// <summary>Gets the description of a registered integration.</summary>
    /// <param name="integrationName">The integration name.</param>
    /// <returns>The description string, or an empty string if not found.</returns>
    public string GetDescription(string integrationName)
    {
        if (_descriptions.TryGetValue(integrationName, out var desc))
        {
            return desc;
        }

        return "";
    }

    /// <summary>Removes a registered integration by name.</summary>
    /// <param name="integrationName">The name of the integration to remove.</param>
    /// <returns>true if the integration was removed; otherwise, false.</returns>
    public bool Unregister(string integrationName)
    {
        bool removed = _integrationHandlers.Remove(integrationName);
        _descriptions.Remove(integrationName);
        _registeredNames.Remove(integrationName);
        return removed;
    }

    /// <summary>Removes all registered integrations.</summary>
    public void Clear()
    {
        _integrationHandlers.Clear();
        _descriptions.Clear();
        _registeredNames.Clear();
    }
}
