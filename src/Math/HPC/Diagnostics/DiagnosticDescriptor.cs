namespace MathVerse.Math.HPC.Diagnostics;

public readonly record struct SourceLocation(int Line, int Column, int Length, string FilePath)
{
    public static SourceLocation None => new(0, 0, 0, string.Empty);
    public bool IsValid => Line > 0 || Column > 0 || Length > 0 || !string.IsNullOrEmpty(FilePath);
    public override string ToString() => IsValid ? $"{FilePath}({Line},{Column})" : "Unknown";
}

public readonly record struct DiagnosticDescriptor(string Id, string Title, string MessageFormat, DiagnosticSeverity DefaultSeverity, string Category, bool IsEnabledByDefault = true)
{
    public Diagnostic Create(SourceLocation location, params object?[] args) =>
        new(this, location, args);
}