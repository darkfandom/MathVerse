namespace MathVerse.Math.AI.NeuralNetwork;
using System;
using System.Collections.Generic;

/// <summary>A sequential stack of neural network layers.</summary>
public sealed class SequentialNetwork
{
    private readonly List<ILayer> _layers = [];

    /// <summary>Gets the read-only list of layers in this network.</summary>
    public IReadOnlyList<ILayer> Layers => _layers;

    /// <summary>Gets the number of layers in this network.</summary>
    public int LayerCount => _layers.Count;

    /// <summary>Adds a layer to the end of the network.</summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(ILayer layer)
    {
        _layers.Add(layer);
    }

    /// <summary>Performs a forward pass through all layers in order.</summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="training">Whether the network is in training mode.</param>
    /// <returns>The output tensor after passing through all layers.</returns>
    public Tensor Forward(Tensor input, bool training = true)
    {
        Tensor current = input;
        for (int i = 0; i < _layers.Count; i++)
        {
            current = _layers[i].Forward(current, training);
        }
        return current;
    }

    /// <summary>Performs a backward pass through all layers in reverse order.</summary>
    /// <param name="lossGradient">The gradient of the loss with respect to the network output.</param>
    /// <param name="learningRate">The learning rate for parameter updates.</param>
    public void Backward(Tensor lossGradient, double learningRate)
    {
        Tensor currentGradient = lossGradient;
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            currentGradient = _layers[i].Backward(currentGradient, learningRate);
        }
    }

    /// <summary>Updates the parameters of all layers using their stored gradients.</summary>
    /// <param name="learningRate">The learning rate for the parameter update.</param>
    public void UpdateParameters(double learningRate)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            _layers[i].UpdateParameters(learningRate);
        }
    }
}
