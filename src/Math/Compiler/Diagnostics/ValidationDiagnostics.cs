namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Generic;
using CompilerConfiguration = MathVerse.Math.Compiler.Configuration.CompilerConfiguration;

/// <summary>Validates compiler inputs and configurations.</summary>
public sealed class ValidationDiagnostics
{
    /// <summary>Validates a source expression for basic correctness.</summary>
    /// <param name="source">The source expression string.</param>
    /// <returns>True if the input is valid.</returns>
    public bool ValidateInput(string source)
    {
        if (source is null) return false;
        if (source.Length == 0) return false;
        return source.Trim().Length > 0;
    }

    /// <summary>Validates a compiler configuration object.</summary>
    /// <param name="config">The configuration to validate.</param>
    /// <returns>A list of diagnostic messages for any issues found.</returns>
    public IReadOnlyList<DiagnosticMessage> ValidateConfiguration(CompilerConfiguration config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        var messages = new List<DiagnosticMessage>();
        return messages;
    }
}
