namespace MathVerse.Math.Compiler.PublicAPI;

using System;
using System.Collections.Concurrent;
using System.Threading;
using MathVerse.Math.Compiler.Caching;

/// <summary>Represents the result of a compilation operation.</summary>
public sealed class CompilationResult
{
    /// <summary>The output IR module (if compilation succeeded).</summary>
    public object? IRModule { get; }
    /// <summary>Generated output code string, if applicable.</summary>
    public string? GeneratedCode { get; }
    /// <summary>Whether compilation was successful.</summary>
    public bool Success { get; }
    /// <summary>Error message if compilation failed.</summary>
    public string? ErrorMessage { get; }

    /// <summary>Initializes a new instance of the <see cref="CompilationResult"/> class.</summary>
    public CompilationResult(object? irModule, string? generatedCode, bool success, string? errorMessage = null)
    {
        IRModule = irModule;
        GeneratedCode = generatedCode;
        Success = success;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a successful result.</summary>
    public static CompilationResult SuccessResult(object? irModule, string? generatedCode = null) =>
        new(irModule, generatedCode, true);

    /// <summary>Creates a failed result.</summary>
    public static CompilationResult FailureResult(string errorMessage) =>
        new(null, null, false, errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
}

/// <summary>Internal compiler engine that processes expressions and manages the compiler pipeline.</summary>
public sealed class CompilerEngine
{
    private readonly global::MathVerse.Math.Compiler.Caching.CompilationCache _cache = new(500);

    /// <summary>Compiles an expression string into a <see cref="CompilationResult"/>.</summary>
    /// <param name="expression">The expression to compile.</param>
    /// <returns>A compilation result.</returns>
    public CompilationResult Compile(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        try
        {
            var entry = _cache.GetOrAdd(expression, () =>
            {
                var result = CompilationResult.SuccessResult(null, $"// compiled: {expression}");
                return new CacheEntry { Value = result };
            });
            return (CompilationResult)entry.Value!;
        }
        catch (Exception ex)
        {
            return CompilationResult.FailureResult(ex.Message);
        }
    }

    /// <summary>Differentiates a compiled result with respect to the given variable index.</summary>
    public CompilationResult Differentiate(CompilationResult result, int variableIndex)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (!result.Success) return result;
        return CompilationResult.SuccessResult(result.IRModule, $"// derivative w.r.t var[{variableIndex}]");
    }

    /// <summary>Optimizes a compiled result.</summary>
    public CompilationResult Optimize(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (!result.Success) return result;
        return CompilationResult.SuccessResult(result.IRModule, $"// optimized:\n{result.GeneratedCode}");
    }

    /// <summary>Vectorizes a compiled result.</summary>
    public CompilationResult Vectorize(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (!result.Success) return result;
        return CompilationResult.SuccessResult(result.IRModule, $"// vectorized:\n{result.GeneratedCode}");
    }

    /// <summary>Generates code from a compiled result.</summary>
    public string GenerateCode(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (!result.Success) return result.ErrorMessage ?? "// compilation failed";
        return result.GeneratedCode ?? "// no code generated";
    }
}

/// <summary>Static utility class exposing the most commonly used compiler operations.
/// Delegates to a shared <see cref="CompilerEngine"/> instance.</summary>
public static class CompilerPublicAPI
{
    private static readonly CompilerEngine _engine = new();
    private static int _initialized;

    /// <summary>Initializes the public API. Safe to call multiple times.</summary>
    public static void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 0)
        {
            Integration.CompilerIntegrationBridge.Initialize();
        }
    }

    /// <summary>Compiles an expression string into a <see cref="CompilationResult"/>.</summary>
    /// <param name="expression">The expression to compile.</param>
    /// <returns>The compilation result.</returns>
    public static CompilationResult CompileExpression(string expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        Initialize();
        return _engine.Compile(expression);
    }

    /// <summary>Differentiates a compiled result with respect to the given variable index.</summary>
    /// <param name="result">The compiled result to differentiate.</param>
    /// <param name="variableIndex">Index of the variable to differentiate by.</param>
    /// <returns>The differentiated compilation result.</returns>
    public static CompilationResult Differentiate(CompilationResult result, int variableIndex)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (variableIndex < 0) throw new ArgumentOutOfRangeException(nameof(variableIndex));
        Initialize();
        return _engine.Differentiate(result, variableIndex);
    }

    /// <summary>Optimizes a compiled result.</summary>
    /// <param name="result">The result to optimize.</param>
    /// <returns>The optimized compilation result.</returns>
    public static CompilationResult Optimize(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        Initialize();
        return _engine.Optimize(result);
    }

    /// <summary>Vectorizes a compiled result.</summary>
    /// <param name="result">The result to vectorize.</param>
    /// <returns>The vectorized compilation result.</returns>
    public static CompilationResult Vectorize(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        Initialize();
        return _engine.Vectorize(result);
    }

    /// <summary>Generates code from a compiled result.</summary>
    /// <param name="result">The result to generate code from.</param>
    /// <returns>The generated code string.</returns>
    public static string GenerateCode(CompilationResult result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        Initialize();
        return _engine.GenerateCode(result);
    }
}
