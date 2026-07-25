namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using MathVerse.Math.Compiler.IR;

public sealed record CompilationResult
{
    public IRModule? IR { get; init; }
    public string GeneratedCode { get; init; } = string.Empty;
    public CompilationMetadata Metadata { get; init; } = CompilationMetadata.Empty;
    public TimeSpan CompilationTime { get; init; }
    public bool Success { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static CompilationResult SuccessResult(IRModule ir, string code, CompilationMetadata metadata, TimeSpan time)
        => new()
        {
            IR = ir,
            GeneratedCode = code,
            Metadata = metadata,
            CompilationTime = time,
            Success = true,
            Warnings = Array.Empty<string>()
        };

    public static CompilationResult FailureResult(string error, TimeSpan time)
        => new()
        {
            CompilationTime = time,
            Success = false,
            Errors = new[] { error }
        };
}

public sealed record CompilationMetadata
{
    public CompilationTarget Target { get; init; }
    public int InstructionCount { get; init; }
    public int BlockCount { get; init; }
    public int FunctionCount { get; init; }
    public int OptimizationsApplied { get; init; }
    public bool Vectorized { get; init; }
    public string SourceHash { get; init; } = string.Empty;

    public static CompilationMetadata Empty { get; } = new();

    public CompilationMetadata WithIR(IRModule module)
        => this with
        {
            InstructionCount = module.TotalInstructionCount(),
            BlockCount = module.TotalBlockCount(),
            FunctionCount = module.Functions.Count
        };
}
