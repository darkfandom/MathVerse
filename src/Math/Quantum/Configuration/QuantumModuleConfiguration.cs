namespace MathVerse.Math.Quantum.Configuration;

/// <summary>
/// Module-level configuration that wraps global settings and provides module-specific overrides.
/// </summary>
public sealed class QuantumModuleConfiguration
{
    private readonly Dictionary<string, object> _moduleSettings;

    /// <summary>
    /// Gets the global quantum configuration.
    /// </summary>
    public Core.QuantumConfiguration Global { get; }

    /// <summary>
    /// Gets the module-specific settings dictionary.
    /// </summary>
    public Dictionary<string, object> ModuleSettings => _moduleSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumModuleConfiguration"/> class.
    /// </summary>
    /// <param name="global">The global quantum configuration.</param>
    public QuantumModuleConfiguration(Core.QuantumConfiguration global)
    {
        Global = global ?? throw new ArgumentNullException(nameof(global));
        _moduleSettings = new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets a module setting cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected setting type.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or <c>default</c> if not found or not of type <typeparamref name="T"/>.</returns>
    public T? GetSetting<T>(string key)
    {
        if (_moduleSettings.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out object? value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Sets a module setting.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    public void SetSetting(string key, object value)
    {
        _moduleSettings[key ?? throw new ArgumentNullException(nameof(key))] = value ?? throw new ArgumentNullException(nameof(value));
    }
}
