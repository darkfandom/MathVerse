namespace MathVerse.Math.AI.NeuralNetwork;
using System;

/// <summary>Activation functions for neural network layers.</summary>
public enum ActivationType
{
    /// <summary>No activation (identity).</summary>
    None,
    /// <summary>Rectified Linear Unit: max(0, x).</summary>
    ReLU,
    /// <summary>Gaussian Error Linear Unit.</summary>
    GELU,
    /// <summary>Sigmoid Linear Unit (Swish): x * sigmoid(x).</summary>
    SiLU,
    /// <summary>Sigmoid function: 1 / (1 + exp(-x)).</summary>
    Sigmoid,
    /// <summary>Hyperbolic tangent activation.</summary>
    Tanh,
    /// <summary>Softmax for multi-class probability distribution.</summary>
    Softmax,
    /// <summary>Exponential Linear Unit.</summary>
    ELU,
    /// <summary>Leaky Rectified Linear Unit.</summary>
    LeakyReLU
}

/// <summary>Provides activation functions and their derivatives.</summary>
public static class Activations
{
    private const double LeakyReluAlpha = 0.01;
    private const double GeluCoeff = 0.044715;

    /// <summary>Applies the specified activation function to a scalar value.</summary>
    /// <param name="x">The input value.</param>
    /// <param name="type">The activation function type.</param>
    /// <returns>The activated value.</returns>
    public static double Activate(double x, ActivationType type)
    {
        return type switch
        {
            ActivationType.ReLU => x > 0.0 ? x : 0.0,
            ActivationType.GELU =>
                0.5 * x * (1.0 + System.Math.Tanh(
                    System.Math.Sqrt(2.0 / System.Math.PI) * (x + GeluCoeff * x * x * x))),
            ActivationType.SiLU => x / (1.0 + System.Math.Exp(-x)),
            ActivationType.Sigmoid => 1.0 / (1.0 + System.Math.Exp(-x)),
            ActivationType.Tanh => System.Math.Tanh(x),
            ActivationType.ELU => x >= 0.0 ? x : System.Math.Exp(x) - 1.0,
            ActivationType.LeakyReLU => x > 0.0 ? x : LeakyReluAlpha * x,
            _ => x
        };
    }

    /// <summary>Computes the derivative of the specified activation function at a scalar value.</summary>
    /// <param name="x">The input value (pre-activation).</param>
    /// <param name="type">The activation function type.</param>
    /// <returns>The derivative value.</returns>
    public static double Derivative(double x, ActivationType type)
    {
        return type switch
        {
            ActivationType.ReLU => x > 0.0 ? 1.0 : 0.0,
            ActivationType.GELU => GeluDerivative(x),
            ActivationType.SiLU => SiluDerivative(x),
            ActivationType.Sigmoid => SigmoidDerivative(x),
            ActivationType.Tanh => TanhDerivative(x),
            ActivationType.ELU => x >= 0.0 ? 1.0 : System.Math.Exp(x),
            ActivationType.LeakyReLU => x > 0.0 ? 1.0 : LeakyReluAlpha,
            _ => 1.0
        };
    }

    private static double GeluDerivative(double x)
    {
        double inner = System.Math.Sqrt(2.0 / System.Math.PI) * (x + GeluCoeff * x * x * x);
        double tanhVal = System.Math.Tanh(inner);
        double sech2 = 1.0 - tanhVal * tanhVal;
        double dInner = System.Math.Sqrt(2.0 / System.Math.PI) * (1.0 + 3.0 * GeluCoeff * x * x);
        return 0.5 * (1.0 + tanhVal) + 0.5 * x * sech2 * dInner;
    }

    private static double SiluDerivative(double x)
    {
        double sigmoid = 1.0 / (1.0 + System.Math.Exp(-x));
        return sigmoid + x * sigmoid * (1.0 - sigmoid);
    }

    private static double SigmoidDerivative(double x)
    {
        double s = 1.0 / (1.0 + System.Math.Exp(-x));
        return s * (1.0 - s);
    }

    private static double TanhDerivative(double x)
    {
        double t = System.Math.Tanh(x);
        return 1.0 - t * t;
    }

    /// <summary>Applies the specified activation function element-wise to a tensor.</summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="type">The activation function type.</param>
    /// <returns>A new tensor with the activation applied to each element.</returns>
    public static Tensor Activate(Tensor input, ActivationType type)
    {
        if (type == ActivationType.Softmax)
        {
            return SoftmaxTensor(input);
        }
        double[] result = new double[input.TotalSize];
        double[] data = input.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Activate(data[i], type);
        }
        return new Tensor(input.Shape, result);
    }

    /// <summary>Computes the derivative of the specified activation function element-wise for a tensor.</summary>
    /// <param name="input">The input tensor (pre-activation values).</param>
    /// <param name="type">The activation function type.</param>
    /// <returns>A new tensor with the derivative computed at each element.</returns>
    public static Tensor Derivative(Tensor input, ActivationType type)
    {
        if (type == ActivationType.Softmax)
        {
            return SoftmaxDerivative(input);
        }
        double[] result = new double[input.TotalSize];
        double[] data = input.Data;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Derivative(data[i], type);
        }
        return new Tensor(input.Shape, result);
    }

    /// <summary>Applies softmax along the last axis of a tensor.</summary>
    /// <param name="input">The input tensor.</param>
    /// <returns>A new tensor with softmax applied.</returns>
    private static Tensor SoftmaxTensor(Tensor input)
    {
        if (input.Rank == 1)
        {
            return Softmax1D(input);
        }
        if (input.Rank == 2)
        {
            return Softmax2D(input);
        }
        throw new ArgumentException($"Softmax is only supported for tensors of rank 1 or 2. Got rank {input.Rank}.");
    }

    /// <summary>Computes the Jacobian-based derivative for softmax when used in backpropagation.</summary>
    /// <param name="input">The input tensor (after softmax has been applied).</param>
    /// <returns>A tensor representing the diagonal Jacobian approximation.</returns>
    private static Tensor SoftmaxDerivative(Tensor input)
    {
        if (input.Rank == 1)
        {
            double[] result = new double[input.TotalSize];
            double[] data = input.Data;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = data[i] * (1.0 - data[i]);
            }
            return new Tensor(input.Shape, result);
        }
        if (input.Rank == 2)
        {
            int rows = input.Shape[0];
            int cols = input.Shape[1];
            double[] result = new double[rows * cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double val = input.Data[r * cols + c];
                    result[r * cols + c] = val * (1.0 - val);
                }
            }
            return new Tensor(input.Shape, result);
        }
        throw new ArgumentException($"Softmax derivative is only supported for tensors of rank 1 or 2.");
    }

    /// <summary>Applies softmax to a 1D tensor.</summary>
    /// <param name="input">The input tensor.</param>
    /// <returns>A new tensor with softmax applied.</returns>
    private static Tensor Softmax1D(Tensor input)
    {
        int n = input.TotalSize;
        double[] result = new double[n];
        double maxVal = input.Max();
        double expSum = 0.0;
        for (int i = 0; i < n; i++)
        {
            result[i] = System.Math.Exp(input.Data[i] - maxVal);
            expSum += result[i];
        }
        for (int i = 0; i < n; i++)
        {
            result[i] /= expSum;
        }
        return new Tensor([n], result);
    }

    /// <summary>Applies softmax along each row of a 2D tensor.</summary>
    /// <param name="input">The input tensor of shape [batch, classes].</param>
    /// <returns>A new tensor with softmax applied per row.</returns>
    private static Tensor Softmax2D(Tensor input)
    {
        int rows = input.Shape[0];
        int cols = input.Shape[1];
        double[] result = new double[rows * cols];
        for (int r = 0; r < rows; r++)
        {
            double maxVal = input.Data[r * cols];
            for (int c = 1; c < cols; c++)
            {
                double v = input.Data[r * cols + c];
                if (v > maxVal) maxVal = v;
            }
            double expSum = 0.0;
            for (int c = 0; c < cols; c++)
            {
                result[r * cols + c] = System.Math.Exp(input.Data[r * cols + c] - maxVal);
                expSum += result[r * cols + c];
            }
            for (int c = 0; c < cols; c++)
            {
                result[r * cols + c] /= expSum;
            }
        }
        return new Tensor([rows, cols], result);
    }
}
