namespace MathVerse.Math.Compiler.Configuration;

public sealed record CompilerOptions
{
    public string MethodName { get; init; } = "main";

    public string? ReturnType { get; init; }

    public IReadOnlyDictionary<string, string>? Parameters { get; init; }

    public bool StrictMode { get; init; }

    public bool EnableTrace { get; init; }

    public int PrecisionDigits { get; init; } = 15;

    public CompilationTargetType Target { get; init; } = CompilationTargetType.Generic;

    public static CompilerOptions Default { get; } = new();
}
