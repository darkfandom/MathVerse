namespace MathVerse.Math.Simplification;

/// <summary>
/// Defines a single simplification rule that attempts to rewrite an expression.
/// Returns <c>null</c> when the rule does not apply to the given expression.
/// </summary>
/// <param name="Name">A human-readable name describing the rule.</param>
/// <param name="TryRewrite">The transformation function. Returns the simplified expression or <c>null</c> if not applicable.</param>
/// <param name="Priority">Execution priority. Rules with higher priority execute first.</param>
public sealed record SimplificationRule(
    string Name,
    Func<Expression, Expression?> TryRewrite,
    int Priority)
{
    /// <summary>Creates a new simplification rule.</summary>
    /// <param name="name">A human-readable name for the rule.</param>
    /// <param name="transform">The rewrite function applied when the rule matches.</param>
    /// <param name="priority">Execution priority. Higher values execute first.</param>
    /// <returns>A new <see cref="SimplificationRule"/>.</returns>
    public static SimplificationRule Create(string name, Func<Expression, Expression?> transform, int priority = 0) =>
        new(name, transform, priority);
}
