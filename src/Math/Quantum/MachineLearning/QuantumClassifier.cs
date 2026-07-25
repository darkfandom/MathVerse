namespace MathVerse.Math.Quantum.MachineLearning;

using System;
using System.Numerics;
using LinearAlgebra;

/// <summary>
/// Variational quantum classifier for multi-class classification using parameterized quantum circuits
/// with classical optimization of circuit parameters.
/// </summary>
public sealed class QuantumClassifier
{
    private readonly int _numQubits;
    private readonly int _numClasses;
    private readonly int _layers;
    private readonly Random _rng;

    /// <summary>Gets the number of qubits in the classifier circuit.</summary>
    public int NumQubits => _numQubits;

    /// <summary>Gets the number of output classes.</summary>
    public int NumClasses => _numClasses;

    /// <summary>Gets the number of variational layers.</summary>
    public int Layers => _layers;

    /// <summary>Creates a variational quantum classifier.</summary>
    /// <param name="numQubits">The number of qubits.</param>
    /// <param name="numClasses">The number of classification categories.</param>
    /// <param name="layers">The number of variational layers.</param>
    public QuantumClassifier(int numQubits, int numClasses, int layers)
    {
        if (numQubits < 1) throw new ArgumentOutOfRangeException(nameof(numQubits));
        if (numClasses < 2) throw new ArgumentOutOfRangeException(nameof(numClasses));
        if (layers < 1) throw new ArgumentOutOfRangeException(nameof(layers));
        _numQubits = numQubits;
        _numClasses = numClasses;
        _layers = layers;
        _rng = new Random(42);
    }

    /// <summary>Predicts the class label for a given feature vector and parameters.</summary>
    /// <param name="features">Input features.</param>
    /// <param name="parameters">Circuit parameters.</param>
    /// <returns>The predicted class index (0-based).</returns>
    public int Classify(double[] features, double[] parameters)
    {
        if (features == null) throw new ArgumentNullException(nameof(features));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        double[] probabilities = ComputeProbabilities(features, parameters);
        int maxIdx = 0;
        double maxProb = probabilities[0];
        for (int i = 1; i < probabilities.Length; i++)
        {
            if (probabilities[i] > maxProb)
            {
                maxProb = probabilities[i];
                maxIdx = i;
            }
        }
        return maxIdx;
    }

    /// <summary>Computes the cross-entropy loss over a dataset.</summary>
    /// <param name="parameters">Current circuit parameters.</param>
    /// <param name="features">Training feature vectors.</param>
    /// <param name="labels">Training labels.</param>
    /// <returns>The average cross-entropy loss.</returns>
    public double Loss(double[] parameters, ComplexVector[] features, int[] labels)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (features == null) throw new ArgumentNullException(nameof(features));
        if (labels == null) throw new ArgumentNullException(nameof(labels));
        if (features.Length != labels.Length) throw new ArgumentException("Features and labels must have the same length.");

        double totalLoss = 0.0;
        for (int i = 0; i < features.Length; i++)
        {
            double[] probs = ComputeProbabilitiesFromVector(features[i], parameters);
            int label = labels[i];
            if (label >= 0 && label < _numClasses && probs[label] > 1e-15)
                totalLoss += -System.Math.Log(probs[label]);
        }
        return totalLoss / features.Length;
    }

    /// <summary>Trains the classifier using gradient-free optimization (parameter search).</summary>
    /// <param name="features">Training feature vectors.</param>
    /// <param name="labels">Training labels.</param>
    /// <param name="maxIterations">Maximum optimization iterations.</param>
    /// <returns>The optimized parameter vector.</returns>
    public double[] Train(ComplexVector[] features, int[] labels, int maxIterations)
    {
        if (features == null) throw new ArgumentNullException(nameof(features));
        if (labels == null) throw new ArgumentNullException(nameof(labels));
        if (maxIterations < 1) throw new ArgumentOutOfRangeException(nameof(maxIterations));

        int paramCount = _numQubits * _layers * 3;
        var bestParams = new double[paramCount];
        for (int i = 0; i < paramCount; i++)
            bestParams[i] = _rng.NextDouble() * 2.0 * System.Math.PI;

        double bestLoss = Loss(bestParams, features, labels);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var trialParams = new double[paramCount];
            for (int i = 0; i < paramCount; i++)
                trialParams[i] = bestParams[i] + (_rng.NextDouble() - 0.5) * 0.1;

            double trialLoss = Loss(trialParams, features, labels);
            if (trialLoss < bestLoss)
            {
                bestLoss = trialLoss;
                Array.Copy(trialParams, bestParams, paramCount);
            }
        }
        return bestParams;
    }

    private double[] ComputeProbabilities(double[] features, double[] parameters)
    {
        int dim = 1 << _numQubits;
        var state = new Complex[dim];
        state[0] = Complex.One;

        for (int q = 0; q < System.Math.Min(features.Length, _numQubits); q++)
        {
            ApplyRz(state, q, features[q]);
        }

        int paramIdx = 0;
        for (int layer = 0; layer < _layers; layer++)
        {
            for (int q = 0; q < _numQubits; q++)
            {
                if (paramIdx + 2 < parameters.Length)
                {
                    ApplyRx(state, q, parameters[paramIdx++]);
                    ApplyRy(state, q, parameters[paramIdx++]);
                    ApplyRz(state, q, parameters[paramIdx++]);
                }
            }
            for (int q = 0; q < _numQubits - 1; q++)
            {
                ApplyCNOT(state, _numQubits, q, q + 1);
            }
        }

        var probs = new double[_numClasses];
        for (int i = 0; i < dim && i < _numClasses; i++)
            probs[i] = state[i].Magnitude * state[i].Magnitude;

        double total = 0.0;
        for (int i = 0; i < _numClasses; i++) total += probs[i];
        if (total > 1e-15)
            for (int i = 0; i < _numClasses; i++) probs[i] /= total;

        return probs;
    }

    private double[] ComputeProbabilitiesFromVector(ComplexVector features, double[] parameters)
    {
        var featureArray = new double[features.Dimension];
        for (int i = 0; i < features.Dimension; i++)
            featureArray[i] = features[i].Real;
        return ComputeProbabilities(featureArray, parameters);
    }

    private static void ApplyRz(Complex[] state, int qubit, double angle)
    {
        int n = state.Length;
        int mask = 1 << qubit;
        for (int i = 0; i < n; i++)
        {
            if ((i & mask) != 0)
            {
                double phase = ((i & mask) != 0) ? -angle : angle;
                state[i] *= Complex.FromPolarCoordinates(1.0, phase / 2.0);
            }
        }
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
