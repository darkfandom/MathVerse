namespace MathVerse.Math.Types.Checking;

/// <summary>Result of type checking.</summary>
public sealed class TypeCheckResult
{
    /// <summary>The checked (possibly refined) type.</summary>
    public MathType Type { get; }

    /// <summary>Whether checking was successful.</summary>
    public bool IsSuccess { get; }

    /// <summary>Diagnostics produced during checking.</summary>
    public IReadOnlyList<TypeCheckDiagnostic> Diagnostics { get; }

    /// <summary>Creates a type check result.</summary>
    public TypeCheckResult(MathType type, bool isSuccess, IReadOnlyList<TypeCheckDiagnostic> diagnostics)
    {
        Type = type;
        IsSuccess = isSuccess;
        Diagnostics = diagnostics;
    }
}

/// <summary>A diagnostic produced during type checking.</summary>
public sealed class TypeCheckDiagnostic : IEquatable<TypeCheckDiagnostic>
{
    /// <summary>The severity.</summary>
    public TypeCheckSeverity Severity { get; }

    /// <summary>The diagnostic code.</summary>
    public TypeDiagnosticCode Code { get; }

    /// <summary>The message.</summary>
    public string Message { get; }

    /// <summary>Optional expression that caused the diagnostic.</summary>
    public string? Expression { get; }

    /// <summary>Creates a type check diagnostic.</summary>
    public TypeCheckDiagnostic(TypeCheckSeverity severity, TypeDiagnosticCode code,
        string message, string? expression = null)
    {
        Severity = severity;
        Code = code;
        Message = message;
        Expression = expression;
    }

    /// <inheritdoc/>
    public bool Equals(TypeCheckDiagnostic? other) =>
        other is not null && other.Code == Code && other.Message == Message;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TypeCheckDiagnostic);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Code, Message);

    /// <inheritdoc/>
    public override string ToString() => $"[{Severity}] [{Code}] {Message}";
}

/// <summary>Diagnostic severity.</summary>
public enum TypeCheckSeverity
{
    /// <summary>Informational.</summary>
    Info,
    /// <summary>A warning.</summary>
    Warning,
    /// <summary>An error.</summary>
    Error,
}
