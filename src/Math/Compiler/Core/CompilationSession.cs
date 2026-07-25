namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

public sealed class CompilationSession
{
    public string SessionId { get; }
    public DateTime StartTime { get; }
    public CompilationTarget Target { get; }
    public string Source { get; }
    private readonly Stopwatch _stopwatch;
    private readonly List<string> _log = new();

    public CompilationSession(string source, CompilationTarget target)
    {
        SessionId = Guid.NewGuid().ToString("N")[..12];
        Source = source;
        Target = target;
        StartTime = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Log(string message)
    {
        _log.Add($"[{_stopwatch.ElapsedMilliseconds}ms] {message}");
    }

    public IReadOnlyList<string> GetLog() => _log.AsReadOnly();

    public string ComputeSourceHash()
    {
        var bytes = Encoding.UTF8.GetBytes(Source);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash)[..16];
    }

    public void Stop() => _stopwatch.Stop();
}
