namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class CompilerRegistry
{
    private readonly ConcurrentDictionary<string, Func<object>> _factories = new();
    private readonly ConcurrentDictionary<string, object> _singletons = new();

    public void Register<T>(string key, Func<T> factory) where T : class
    {
        _factories[key] = () => factory();
    }

    public void RegisterSingleton<T>(string key, Func<T> factory) where T : class
    {
        _singletons[key] = factory();
    }

    public T? Resolve<T>(string key) where T : class
    {
        if (_singletons.TryGetValue(key, out var singleton) && singleton is T typedSingleton)
            return typedSingleton;

        if (_factories.TryGetValue(key, out var factory) && factory() is T typed)
            return typed;

        return null;
    }

    public bool IsRegistered(string key)
        => _factories.ContainsKey(key) || _singletons.ContainsKey(key);

    public void Unregister(string key)
    {
        _factories.TryRemove(key, out _);
        _singletons.TryRemove(key, out _);
    }

    public IReadOnlyCollection<string> GetRegisteredKeys()
    {
        var keys = new HashSet<string>(_factories.Keys);
        keys.UnionWith(_singletons.Keys);
        return keys;
    }

    public void Clear()
    {
        _factories.Clear();
        _singletons.Clear();
    }
}
