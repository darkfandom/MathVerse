namespace MathVerse.Math.Interop.ScriptingAbstraction;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core;

/// <summary>
/// Defines the interface for scripting language adapters.
/// </summary>
public interface IScriptingAdapter
{
    /// <summary>
    /// Gets the language identifier (e.g., "python", "julia", "r", "matlab").
    /// </summary>
    string LanguageId { get; }

    /// <summary>
    /// Gets the display name of the language.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the supported script file extensions.
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Transpiles a MathVerse expression to target language script.
    /// </summary>
    /// <param name="expression">The expression object to transpile.</param>
    /// <param name="expressionType">An optional hint for the expression type.</param>
    /// <returns>The transpiled script text.</returns>
    string Transpile(object expression, string? expressionType = null);

    /// <summary>
    /// Validates whether the adapter can handle the given expression.
    /// </summary>
    /// <param name="expression">The expression to test.</param>
    /// <returns>True if the adapter supports this expression type.</returns>
    bool CanTranspile(object expression);
}
