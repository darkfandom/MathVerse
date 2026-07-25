namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class ExecutionProfiler
{
    private readonly ConcurrentDictionary<string, ProfileEntry> _entries = new();

    public ProfileResult Profile(Action action, string name)
    {
        var sw = Stopwatch.StartNew();
        var memBefore = GC.GetTotalMemory(true);
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);

        action();

        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);

        var entry = new ProfileEntry
        {
            Name = name,
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
            MemoryAllocated = memAfter - memBefore,
            Gen0Collections = GC.CollectionCount(0) - gen0Before,
            Gen1Collections = GC.CollectionCount(1) - gen1Before,
            Gen2Collections = GC.CollectionCount(2) - gen2Before,
            Timestamp = DateTime.UtcNow
        };

        _entries[name] = entry;
        return new ProfileResult(entry);
    }

    public T Profile<T>(Func<T> func, string name, out ProfileResult result)
    {
        var sw = Stopwatch.StartNew();
        var memBefore = GC.GetTotalMemory(true);
        var value = func();
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);

        var entry = new ProfileEntry
        {
            Name = name,
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
            MemoryAllocated = memAfter - memBefore,
            Timestamp = DateTime.UtcNow
        };

        _entries[name] = entry;
        result = new ProfileResult(entry);
        return value;
    }

    public ProfileResult? GetLastProfile(string name)
        => _entries.TryGetValue(name, out var entry) ? new ProfileResult(entry) : null;

    public IReadOnlyDictionary<string, ProfileEntry> GetAllEntries()
        => new Dictionary<string, ProfileEntry>(_entries);

    public void Clear() => _entries.Clear();
}

public sealed record ProfileEntry
{
    public string Name { get; init; } = string.Empty;
    public double ElapsedMs { get; init; }
    public long MemoryAllocated { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public DateTime Timestamp { get; init; }
}

public sealed class ProfileResult
{
    public ProfileEntry Entry { get; }
    public double ElapsedMs => Entry.ElapsedMs;
    public long MemoryAllocated => Entry.MemoryAllocated;

    public ProfileResult(ProfileEntry entry)
    {
        Entry = entry;
    }
}
