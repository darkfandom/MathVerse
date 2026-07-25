namespace MathVerse.Math.Quantum.MachineLearning;

using System;
using System.Numerics;
using Circuits;
using Gates;
using LinearAlgebra;

/// <summary>
/// Parameterized quantum neural network (QNN) for hybrid quantum-classical machine learning.
/// Implements a hardware-efficient ansatz with configurable depth.
/// </summary>
public sealed class QuantumNeuralNetwork
{
    private readonly int _inputQubits;
    private readonly int _hiddenLayers;
    private readonly int _outputQubits;
    private readonly Random _rng;

    /// <summary>Gets the number of input qubits.</summary>
    public int InputQubits => _inputQubits;

    /// <summary>Gets the number of hidden variational layers.</summary>
    public int HiddenLayers => _hiddenLayers;

    /// <summary>Gets the number of output qubits.</summary>
    public int OutputQubits => _outputQubits;

    /// <summary>Gets the total number of trainable parameters.</summary>
    public int ParameterCount => (_inputQubits + _outputQubits) * _hiddenLayers * 3;

    /// <summary>Creates a parameterized quantum neural network.</summary>
    /// <param name="inputQubits">The number of input qubits.</param>
    /// <param name="hiddenLayers">The number of hidden variational layers.</param>
    /// <param name="outputQubits">The number of output qubits.</param>
    public QuantumNeuralNetwork(int inputQubits, int hiddenLayers, int outputQubits)
    {
        if (inputQubits < 1) throw new ArgumentOutOfRangeException(nameof(inputQubits));
        if (hiddenLayers < 1) throw new ArgumentOutOfRangeException(nameof(hiddenLayers));
        if (outputQubits < 1) throw new ArgumentOutOfRangeException(nameof(outputQubits));
        _inputQubits = inputQubits;
        _hiddenLayers = hiddenLayers;
        _outputQubits = outputQubits;
        _rng = new Random(42);
    }

    /// <summary>Builds the parameterized quantum circuit with the given parameters.</summary>
    /// <param name="parameters">The trainable parameter values.</param>
    /// <returns>A <see cref="QuantumCircuit"/> implementing the QNN.</returns>
    public QuantumCircuit BuildCircuit(double[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (parameters.Length < ParameterCount)
            throw new ArgumentException($"Expected at least {ParameterCount} parameters, got {parameters.Length}.");

        int totalQubits = _inputQubits + _outputQubits;
        var circuit = new QuantumCircuit(totalQubits);
        int paramIdx = 0;

        for (int q = 0; q < totalQubits; q++)
            circuit.AddGate(SingleQubitGates.Hadamard, q);

        for (int layer = 0; layer < _hiddenLayers; layer++)
        {
            for (int q = 0; q < totalQubits; q++)
            {
                circuit.AddGate(RotationGates.RX(parameters[paramIdx++]), q);
                circuit.AddGate(RotationGates.RY(parameters[paramIdx++]), q);
                circuit.AddGate(RotationGates.RZ(parameters[paramIdx++]), q);
            }
            for (int q = 0; q < totalQubits - 1; q++)
                circuit.AddGate(MultiQubitGates.CX, q, q + 1);
        }

        return circuit;
    }

    /// <summary>Runs the QNN forward pass and measures the output qubits.</summary>
    /// <param name="input">Input feature vector.</param>
    /// <param name="parameters">Trainable circuit parameters.</param>
    /// <returns>An array of measurement probabilities for each output qubit.</returns>
    public double[] Forward(double[] input, double[] parameters)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        int totalQubits = _inputQubits + _outputQubits;
        int dim = 1 << totalQubits;
        var state = new Complex[dim];
        state[0] = Complex.One;

        for (int q = 0; q < System.Math.Min(input.Length, _inputQubits); q++)
        {
            int mask = 1 << q;
            for (int i = 0; i < dim; i++)
            {
                if ((i & mask) == 0)
                {
                    int j = i | mask;
                    double cos = System.Math.Cos(input[q] / 2.0);
                    double sin = System.Math.Sin(input[q] / 2.0);
                    Complex a = state[i];
                    Complex b = state[j];
                    state[i] = new Complex(cos, 0.0) * a + new Complex(-sin, 0.0) * b;
                    state[j] = new Complex(sin, 0.0) * a + new Complex(cos, 0.0) * b;
                }
            }
        }

        int paramIdx = 0;
        for (int layer = 0; layer < _hiddenLayers; layer++)
        {
            for (int q = 0; q < totalQubits; q++)
            {
                if (paramIdx + 2 < parameters.Length)
                {
                    ApplyRx(state, q, parameters[paramIdx++]);
                    ApplyRy(state, q, parameters[paramIdx++]);
                    ApplyRz(state, q, parameters[paramIdx++]);
                }
            }
            for (int q = 0; q < totalQubits - 1; q++)
                ApplyCNOT(state, totalQubits, q, q + 1);
        }

        var output = new double[_outputQubits];
        for (int q = 0; q < _outputQubits; q++)
        {
            int qubitIdx = _inputQubits + q;
            int mask = 1 << qubitIdx;
            double probOne = 0.0;
            for (int i = 0; i < dim; i++)
            {
                if ((i & mask) != 0)
                    probOne += state[i].Magnitude * state[i].Magnitude;
            }
            output[q] = probOne;
        }
        return output;
    }

    private static void ApplyRx(Complex[] state, int qubit, double angle)
    {
        int n = state.Length;
        int mask = 1 << qubit;
        double cos = System.Math.Cos(angle / 2.0);
        double sin = System.Math.Sin(angle / 2.0);
        for (int i = 0; i < n; i++)
        {
            if ((i & mask) == 0)
            {
                int j = i | mask;
                Complex a = state[i];
                Complex b = state[j];
                state[i] = new Complex(cos, 0.0) * a + new Complex(0, -sin) * b;
                state[j] = new Complex(0, -sin) * a + new Complex(cos, 0.0) * b;
            }
        }
    }

    private static void ApplyRy(Complex[] state, int qubit, double angle)
    {
        int n = state.Length;
        int mask = 1 << qubit;
        double cos = System.Math.Cos(angle / 2.0);
        double sin = System.Math.Sin(angle / 2.0);
        for (int i = 0; i < n; i++)
        {
            if ((i & mask) == 0)
            {
                int j = i | mask;
                Complex a = state[i];
                Complex b = state[j];
                state[i] = new Complex(cos, 0.0) * a + new Complex(-sin, 0.0) * b;
                state[j] = new Complex(sin, 0.0) * a + new Complex(cos, 0.0) * b;
            }
        }
    }

    private static void ApplyRz(Complex[] state, int qubit, double angle)
    {
        int n = state.Length;
        int mask = 1 << qubit;
        for (int i = 0; i < n; i++)
        {
            if ((i & mask) != 0)
                state[i] *= Complex.FromPolarCoordinates(1.0, -angle / 2.0);
            else
                state[i] *= Complex.FromPolarCoordinates(1.0, angle / 2.0);
        }
    }

    private static void ApplyCNOT(Complex[] state, int totalQubits, int control, int target)
    {
        int n = state.Length;
        int controlMask = 1 << control;
        int targetMask = 1 << target;
        for (int i = 0; i < n; i++)
        {
            if ((i & controlMask) != 0 && (i & targetMask) == 0)
            {
                int j = i | targetMask;
                (state[i], state[j]) = (state[j], state[i]);
            }
        }
    }
}
