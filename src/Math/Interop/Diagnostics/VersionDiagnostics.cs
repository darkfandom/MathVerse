namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Tracks version compatibility diagnostics.
/// </summary>
public sealed class VersionDiagnostics
{
    private readonly List<VersionMismatch> _mismatches = new();

    /// <summary>
    /// Gets the list of version mismatches.
    /// </summary>
    public IReadOnlyList<VersionMismatch> Mismatches => _mismatches;

    /// <summary>
    /// Gets a value indicating whether all versions are compatible.
    /// </summary>
    public bool AllVersionsCompatible => _mismatches.Count == 0;

    /// <summary>
    /// Records a version mismatch.
    /// </summary>
    /// <param name="mismatch">The version mismatch.</param>
    public void RecordMismatch(VersionMismatch mismatch)
    {
        _ = mismatch ?? throw new ArgumentNullException(nameof(mismatch));
        _mismatches.Add(mismatch);
    }

    /// <summary>
    /// Checks version compatibility between source and target.
    /// </summary>
    /// <param name="sourceVersion">The source version string.</param>
    /// <param name="targetVersion">The target version string.</param>
    /// <param name="component">The component name.</param>
    /// <returns>True if compatible.</returns>
    public bool CheckCompatibility(string sourceVersion, string targetVersion, string component)
    {
        _ = sourceVersion ?? throw new ArgumentNullException(nameof(sourceVersion));
        _ = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));
        _ = component ?? throw new ArgumentNullException(nameof(component));

        if (Version.TryParse(sourceVersion, out var sv) && Version.TryParse(targetVersion, out var tv))
        {
            if (sv.Major != tv.Major)
            {
                _mismatches.Add(new VersionMismatch
                {
                    Component = component,
                    SourceVersion = sourceVersion,
                    TargetVersion = targetVersion,
                    Severity = MismatchSeverity.Major
                });
                return false;
            }
            return true;
        }
        return true;
    }
}

/// <summary>
/// Represents a version mismatch between source and target.
/// </summary>
public sealed class VersionMismatch
{
    /// <summary>Gets or sets the component name.</summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>Gets or sets the source version.</summary>
    public string SourceVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the target version.</summary>
    public string TargetVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the mismatch severity.</summary>
    public MismatchSeverity Severity { get; set; }
}

/// <summary>
/// Severity of a version mismatch.
/// </summary>
public enum MismatchSeverity
{
    /// <summary>Minor version difference.</summary>
    Minor,

    /// <summary>Major version difference.</summary>
    Major,

    /// <summary>Incompatible versions.</summary>
    Incompatible
}
