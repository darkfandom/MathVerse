namespace MathVerse.Math.AI.NeuralNetwork;

/// <summary>Interface for neural network optimizers.</summary>
public interface INNOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    string Name { get; }

    /// <summary>Updates the parameters of a layer using the optimizer's algorithm.</summary>
    /// <param name="layer">The layer whose parameters to update.</param>
    /// <param name="learningRate">The base learning rate.</param>
    void UpdateLayer(ILayer layer, double learningRate);
}
