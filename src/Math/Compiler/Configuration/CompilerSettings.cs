namespace MathVerse.Math.Compiler.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class CompilerSettings
{
    private readonly ConcurrentDictionary<string, object> _settings = new();

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }

    public void Set<T>(string key, T value)
    {
        _settings[key] = value!;
    }

    public bool Has(string key) => _settings.ContainsKey(key);

    public void Remove(string key) => _settings.TryRemove(key, out _);

    public IReadOnlyDictionary<string, object> GetAll()
        => new Dictionary<string, object>(_settings);

    public CompilerSettings Clone()
    {
        var clone = new CompilerSettings();
        foreach (var kvp in _settings)
            clone._settings[kvp.Key] = kvp.Value;
        return clone;
    }
}
