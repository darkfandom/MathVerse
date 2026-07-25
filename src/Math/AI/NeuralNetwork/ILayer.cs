namespace MathVerse.Math.AI.NeuralNetwork;

/// <summary>Interface for neural network layers.</summary>
public interface ILayer
{
    /// <summary>Gets the name of this layer.</summary>
    string Name { get; }

    /// <summary>Gets the expected input shape of this layer.</summary>
    TensorShape InputShape { get; }

    /// <summary>Gets the output shape of this layer.</summary>
    TensorShape OutputShape { get; }

    /// <summary>Performs the forward pass of this layer.</summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="training">Whether the layer is in training mode.</param>
    /// <returns>The output tensor.</returns>
    Tensor Forward(Tensor input, bool training = true);

    /// <summary>Performs the backward pass, computing gradients.</summary>
    /// <param name="outputGradient">The gradient of the loss with respect to the output.</param>
    /// <param name="learningRate">The learning rate for parameter updates.</param>
    /// <returns>The gradient of the loss with respect to the input.</returns>
    Tensor Backward(Tensor outputGradient, double learningRate);

    /// <summary>Updates the learnable parameters of this layer using stored gradients.</summary>
    /// <param name="learningRate">The learning rate for the update.</param>
    void UpdateParameters(double learningRate);
}
