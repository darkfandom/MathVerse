namespace MathVerse.Math.AI.NeuralNetwork;

/// <summary>Wraps an activation function as a neural network layer.</summary>
public sealed class ActivationLayer : ILayer
{
    private readonly ActivationType _activationType;
    private Tensor? _lastOutput;

    /// <summary>Gets the name of this layer.</summary>
    public string Name => $"Activation_{_activationType}";

    /// <summary>Gets the expected input shape.</summary>
    public TensorShape InputShape { get; private set; }

    /// <summary>Gets the output shape (same as input).</summary>
    public TensorShape OutputShape { get; private set; }

    /// <summary>Initializes a new activation layer with the specified activation type.</summary>
    /// <param name="activationType">The activation function to apply.</param>
    public ActivationLayer(ActivationType activationType)
    {
        _activationType = activationType;
        InputShape = TensorShape.Vector(1);
        OutputShape = TensorShape.Vector(1);
    }

    /// <summary>Initializes a new activation layer with a known input shape.</summary>
    /// <param name="activationType">The activation function to apply.</param>
    /// <param name="inputShape">The shape of the input tensors.</param>
    public ActivationLayer(ActivationType activationType, TensorShape inputShape)
    {
        _activationType = activationType;
        InputShape = inputShape;
        OutputShape = inputShape;
    }

    /// <summary>Gets the activation function type used by this layer.</summary>
    public ActivationType ActivationFunction => _activationType;

    /// <summary>Applies the activation function to the input tensor.</summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="training">Whether the layer is in training mode.</param>
    /// <returns>A new tensor with the activation applied element-wise.</returns>
    public Tensor Forward(Tensor input, bool training = true)
    {
        InputShape = new TensorShape(input.Shape);
        OutputShape = new TensorShape(input.Shape);
        _lastOutput = Activations.Activate(input, _activationType);
        return _lastOutput;
    }

    /// <summary>Computes the gradient through the activation function.</summary>
    /// <param name="outputGradient">The gradient of the loss with respect to the output.</param>
    /// <param name="learningRate">The learning rate (unused for activation layers).</param>
    /// <returns>The gradient of the loss with respect to the input.</returns>
    public Tensor Backward(Tensor outputGradient, double learningRate)
    {
        if (_lastOutput == null)
        {
            throw new System.InvalidOperationException("Forward must be called before Backward.");
        }

        if (_activationType == ActivationType.Softmax)
        {
            // For softmax in backprop, we assume the loss gradient is already applied
            // and the derivative of softmax combined with cross-entropy simplifies nicely.
            // We apply the diagonal Jacobian approximation.
            Tensor deriv = Activations.Derivative(_lastOutput, _activationType);
            var ops = new TensorOperations();
            return ops.Multiply(outputGradient, deriv);
        }

        Tensor derivative = Activations.Derivative(_lastOutput, _activationType);
        var op = new TensorOperations();
        return op.Multiply(outputGradient, derivative);
    }

    /// <summary>No parameters to update in an activation layer.</summary>
    /// <param name="learningRate">The learning rate (unused).</param>
    public void UpdateParameters(double learningRate)
    {
        // No learnable parameters.
    }
}
