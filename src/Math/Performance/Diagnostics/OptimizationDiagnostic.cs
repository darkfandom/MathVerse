namespace MathVerse.Math.Performance.Diagnostics;

/// <summary>
/// Records the outcome of a single optimization pass for diagnostic reporting.
/// </summary>
/// <param name="PassName">The name of the optimization pass.</param>
/// <param name="NodesRemoved">The number of expression nodes removed.</param>
/// <param name="NodesSimplified">The number of expression nodes simplified.</param>
/// <param name="Duration">How long the pass took.</param>
/// <param name="HasChanges">Whether the pass modified the expression.</param>
public sealed record OptimizationDiagnostic(
    string PassName,
    int NodesRemoved,
    int NodesSimplified,
    TimeSpan Duration,
    bool HasChanges)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"[{PassName}] Removed={NodesRemoved}, Simplified={NodesSimplified}, " +
        $"Duration={Duration.TotalMilliseconds:F2}ms, Changed={HasChanges}";
}
