namespace MathVerse.Math.AI.Configuration;

using System.Collections.Concurrent;
using System.Collections.Immutable;

/// <summary>Per-module configuration manager for enabling, disabling, and configuring individual AI modules.</summary>
public sealed class AIModuleConfiguration
{
    private readonly ConcurrentDictionary<string, ModuleSettings> _moduleSettings = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> _moduleProperties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the number of configured modules.</summary>
    public int ModuleCount => _moduleSettings.Count;

    /// <summary>Configures a module with the specified settings.</summary>
    /// <param name="moduleName">The name of the module to configure.</param>
    /// <param name="settings">Key-value pairs of configuration settings for the module.</param>
    public void Configure(string moduleName, ImmutableDictionary<string, string> settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        var moduleSettings = new ModuleSettings
        {
            Name = moduleName,
            IsEnabled = true,
            ConfiguredAt = DateTime.UtcNow
        };

        _moduleSettings[moduleName] = moduleSettings;
        _moduleProperties[moduleName] = settings ?? ImmutableDictionary<string, string>.Empty;
    }

    /// <summary>Configures a module with enabled/disabled state and settings.</summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="isEnabled">Whether the module is enabled.</param>
    /// <param name="settings">Optional key-value configuration settings.</param>
    public void Configure(string moduleName, bool isEnabled, ImmutableDictionary<string, string>? settings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        var moduleSettings = new ModuleSettings
        {
            Name = moduleName,
            IsEnabled = isEnabled,
            ConfiguredAt = DateTime.UtcNow
        };

        _moduleSettings[moduleName] = moduleSettings;

        if (settings != null)
        {
            _moduleProperties[moduleName] = settings;
        }
    }

    /// <summary>Checks whether a module is enabled.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <returns>true if the module is enabled; false if disabled or not configured.</returns>
    public bool IsEnabled(string moduleName)
    {
        if (_moduleSettings.TryGetValue(moduleName, out var settings))
        {
            return settings.IsEnabled;
        }

        return false;
    }

    /// <summary>Gets the configuration settings for a specific module.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <returns>The settings dictionary, or an empty dictionary if the module is not configured.</returns>
    public ImmutableDictionary<string, string> GetSettings(string moduleName)
    {
        if (_moduleProperties.TryGetValue(moduleName, out var settings))
        {
            return settings;
        }

        return ImmutableDictionary<string, string>.Empty;
    }

    /// <summary>Gets a specific setting value for a module.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value if the setting is not found.</param>
    /// <returns>The setting value, or the default.</returns>
    public string GetSetting(string moduleName, string key, string defaultValue = "")
    {
        if (_moduleProperties.TryGetValue(moduleName, out var settings) && settings.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>Gets a setting parsed as an integer.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value if not found or unparseable.</param>
    /// <returns>The parsed integer value, or the default.</returns>
    public int GetSettingInt(string moduleName, string key, int defaultValue = 0)
    {
        string raw = GetSetting(moduleName, key, "");
        if (int.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>Gets a setting parsed as a double.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value if not found or unparseable.</param>
    /// <returns>The parsed double value, or the default.</returns>
    public double GetSettingDouble(string moduleName, string key, double defaultValue = 0.0)
    {
        string raw = GetSetting(moduleName, key, "");
        if (double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>Gets a setting parsed as a boolean.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value if not found or unparseable.</param>
    /// <returns>The parsed boolean value, or the default.</returns>
    public bool GetSettingBool(string moduleName, string key, bool defaultValue = false)
    {
        string raw = GetSetting(moduleName, key, "");
        if (bool.TryParse(raw, out bool result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>Disables a module.</summary>
    /// <param name="moduleName">The module name to disable.</param>
    /// <returns>true if the module was found and disabled; false if not found.</returns>
    public bool Disable(string moduleName)
    {
        if (_moduleSettings.TryGetValue(moduleName, out _))
        {
            _moduleSettings[moduleName] = new ModuleSettings
            {
                Name = moduleName,
                IsEnabled = false,
                ConfiguredAt = DateTime.UtcNow
            };
            return true;
        }

        return false;
    }

    /// <summary>Enables a module.</summary>
    /// <param name="moduleName">The module name to enable.</param>
    /// <returns>true if the module was found and enabled; false if not found.</returns>
    public bool Enable(string moduleName)
    {
        if (_moduleSettings.TryGetValue(moduleName, out _))
        {
            _moduleSettings[moduleName] = new ModuleSettings
            {
                Name = moduleName,
                IsEnabled = true,
                ConfiguredAt = DateTime.UtcNow
            };
            return true;
        }

        return false;
    }

    /// <summary>Gets the names of all configured modules.</summary>
    /// <returns>An array of configured module names.</returns>
    public string[] GetConfiguredModules()
    {
        var keys = new string[_moduleSettings.Count];
        int i = 0;
        foreach (var kvp in _moduleSettings)
        {
            keys[i++] = kvp.Key;
        }
        return keys;
    }

    /// <summary>Gets the names of all enabled modules.</summary>
    /// <returns>An array of enabled module names.</returns>
    public string[] GetEnabledModules()
    {
        var enabled = new List<string>();
        foreach (var kvp in _moduleSettings)
        {
            if (kvp.Value.IsEnabled)
            {
                enabled.Add(kvp.Key);
            }
        }
        return enabled.ToArray();
    }

    /// <summary>Removes all configuration for a module.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <returns>true if the module was removed; false if not found.</returns>
    public bool Remove(string moduleName)
    {
        bool removed = _moduleSettings.TryRemove(moduleName, out _);
        _moduleProperties.TryRemove(moduleName, out _);
        return removed;
    }

    /// <summary>Clears all module configurations.</summary>
    public void Clear()
    {
        _moduleSettings.Clear();
        _moduleProperties.Clear();
    }
}

/// <summary>Metadata about a configured module.</summary>
internal sealed class ModuleSettings
{
    /// <summary>Gets the module name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets whether the module is enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets the UTC timestamp when the module was configured.</summary>
    public DateTime ConfiguredAt { get; init; }
}
