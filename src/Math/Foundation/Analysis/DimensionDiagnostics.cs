namespace MathVerse.Math.Foundation.Analysis;

public sealed class DimensionDiagnostics
{
    private readonly List<DimensionDiagnostic> _diagnostics = new();

    public IReadOnlyList<DimensionDiagnostic> Diagnostics => _diagnostics.AsReadOnly();

    public bool HasErrors => _diagnostics.Count > 0;

    public bool HasWarnings => _diagnostics.Any(d =>
        d.ExpectedDimension is not null && d.ActualDimension is not null);

    public void Add(DimensionDiagnostic diagnostic)
    {
        if (diagnostic is null) throw new ArgumentNullException(nameof(diagnostic));
        _diagnostics.Add(diagnostic);
    }

    public void Clear()
    {
        _diagnostics.Clear();
    }
}
