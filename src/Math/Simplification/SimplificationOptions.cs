namespace MathVerse.Math.Simplification;

/// <summary>
/// Configuration options for the expression simplification engine.
/// </summary>
public sealed record SimplificationOptions
{
    /// <summary>Gets whether arithmetic identity rules are enabled.</summary>
    public bool EnableArithmeticRules { get; init; } = true;

    /// <summary>Gets whether power exponent rules are enabled.</summary>
    public bool EnablePowerRules { get; init; } = true;

    /// <summary>Gets whether logarithm and exponential rules are enabled.</summary>
    public bool EnableLogRules { get; init; } = true;

    /// <summary>Gets whether trigonometric identity rules are enabled.</summary>
    public bool EnableTrigRules { get; init; } = true;

    /// <summary>Gets whether constant folding is enabled.</summary>
    public bool EnableConstantFolding { get; init; } = true;

    /// <summary>Gets the maximum number of simplification iterations before stopping.</summary>
    public int MaxIterations { get; init; } = 50;

    /// <summary>Gets a minimal configuration that enables only arithmetic and constant folding.</summary>
    public static SimplificationOptions Minimal { get; } = new()
    {
        EnableArithmeticRules = true,
        EnablePowerRules = false,
        EnableLogRules = false,
        EnableTrigRules = false,
        EnableConstantFolding = true,
        MaxIterations = 10,
    };

    /// <summary>Gets the default configuration with all rule categories enabled.</summary>
    public static SimplificationOptions Default { get; } = new();

    /// <summary>Gets a full configuration with all rules enabled and a high iteration limit.</summary>
    public static SimplificationOptions Full { get; } = new()
    {
        EnableArithmeticRules = true,
        EnablePowerRules = true,
        EnableLogRules = true,
        EnableTrigRules = true,
        EnableConstantFolding = true,
        MaxIterations = 100,
    };
}
