namespace MathVerse.Math.HPC.Diagnostics;

public sealed class Diagnostic
{
    public DiagnosticDescriptor Descriptor { get; }
    public SourceLocation Location { get; }
    public IReadOnlyList<object?> Arguments { get; }
    public DiagnosticSeverity Severity => Descriptor.DefaultSeverity;
    public string Id => Descriptor.Id;
    public string Message => string.Format(Descriptor.MessageFormat, Arguments);

    public Diagnostic(DiagnosticDescriptor descriptor, SourceLocation location, IReadOnlyList<object?> arguments)
    {
        Descriptor = descriptor;
        Location = location;
        Arguments = arguments;
    }

    public Diagnostic(DiagnosticDescriptor descriptor, SourceLocation location, params object?[] args)
        : this(descriptor, location, (IReadOnlyList<object?>)args) { }

    public override string ToString() => $"{Location}: {Severity} {Id}: {Message}";
}

public sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = new();

    public int Count => _diagnostics.Count;
    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    public bool HasWarnings => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning);
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public void Add(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);
    public void AddRange(IEnumerable<Diagnostic> diagnostics) => _diagnostics.AddRange(diagnostics);
    public void Clear() => _diagnostics.Clear();
    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}