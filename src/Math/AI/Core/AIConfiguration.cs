namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;

/// <summary>Full AI system configuration including model defaults and module settings.</summary>
public sealed class AIConfiguration
{
    /// <summary>General engine options.</summary>
    public AIOptions Options { get; init; } = new();

    /// <summary>Default key-value pairs applied to every newly created model.</summary>
    public ImmutableDictionary<string, string> ModelDefaults { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>Name of the optimizer used when none is explicitly specified.</summary>
    public string DefaultOptimizer { get; init; } = "Adam";

    /// <summary>Name of the loss function used when none is explicitly specified.</summary>
    public string DefaultLossFunction { get; init; } = "MSE";

    /// <summary>Default hidden-layer width for neural networks.</summary>
    public int DefaultHiddenSize { get; init; } = 128;

    /// <summary>Default dropout rate applied during training.</summary>
    public double DefaultDropoutRate { get; init; } = 0.1;

    /// <summary>Whether GPU-accelerated kernels should be preferred when available.</summary>
    public bool EnableGPUAcceleration { get; init; } = false;

    /// <summary>Returns a default <see cref="AIConfiguration"/> instance.</summary>
    public static AIConfiguration Default => new();

    /// <summary>Creates a shallow copy with overridden values.</summary>
    /// <param name="options">New options, or <c>null</c> to keep current.</param>
    /// <param name="defaultOptimizer">New default optimizer name, or <c>null</c> to keep current.</param>
    /// <param name="defaultLossFunction">New default loss function name, or <c>null</c> to keep current.</param>
    /// <param name="defaultHiddenSize">New hidden size, or <c>null</c> to keep current.</param>
    /// <param name="defaultDropoutRate">New dropout rate, or <c>null</c> to keep current.</param>
    /// <returns>A new <see cref="AIConfiguration"/> instance with the specified overrides.</returns>
    public AIConfiguration WithOverrides(
        AIOptions? options = null,
        string? defaultOptimizer = null,
        string? defaultLossFunction = null,
        int? defaultHiddenSize = null,
        double? defaultDropoutRate = null) =>
        new()
        {
            Options = options ?? Options,
            ModelDefaults = ModelDefaults,
            DefaultOptimizer = defaultOptimizer ?? DefaultOptimizer,
            DefaultLossFunction = defaultLossFunction ?? DefaultLossFunction,
            DefaultHiddenSize = defaultHiddenSize ?? DefaultHiddenSize,
            DefaultDropoutRate = defaultDropoutRate ?? DefaultDropoutRate,
            EnableGPUAcceleration = EnableGPUAcceleration,
        };
}
