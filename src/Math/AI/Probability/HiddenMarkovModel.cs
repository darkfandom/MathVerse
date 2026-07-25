namespace MathVerse.Math.AI.Probability;

using System;
using System.Collections.Generic;

/// <summary>Hidden Markov Model with forward algorithm, Viterbi decoding, and Baum-Welch training.</summary>
public sealed class HiddenMarkovModel
{
    private readonly int _numStates;
    private readonly int _numObservations;
    private readonly double[] _initialProbs;
    private readonly double[][] _transitionMatrix;
    private readonly double[][] _emissionMatrix;

    /// <summary>Initializes a new Hidden Markov Model.</summary>
    /// <param name="initialProbs">Initial state probability distribution.</param>
    /// <param name="transitionMatrix">State transition matrix (row-stochastic).</param>
    /// <param name="emissionMatrix">Observation emission matrix (row-stochastic).</param>
    public HiddenMarkovModel(double[] initialProbs, double[][] transitionMatrix, double[][] emissionMatrix)
    {
        if (initialProbs == null)
            throw new ArgumentNullException(nameof(initialProbs));
        if (transitionMatrix == null)
            throw new ArgumentNullException(nameof(transitionMatrix));
        if (emissionMatrix == null)
            throw new ArgumentNullException(nameof(emissionMatrix));

        _numStates = initialProbs.Length;
        _numObservations = emissionMatrix[0].Length;

        _initialProbs = new double[_numStates];
        double initSum = 0.0;
        for (int i = 0; i < _numStates; i++)
        {
            _initialProbs[i] = initialProbs[i];
            initSum += initialProbs[i];
        }
        if (System.Math.Abs(initSum - 1.0) > 1e-6)
            throw new ArgumentException("Initial probabilities must sum to 1.0.");

        _transitionMatrix = new double[_numStates][];
        for (int i = 0; i < _numStates; i++)
        {
            if (transitionMatrix[i].Length != _numStates)
                throw new ArgumentException($"Transition matrix row {i} must have {_numStates} elements.");
            _transitionMatrix[i] = new double[_numStates];
            double rowSum = 0.0;
            for (int j = 0; j < _numStates; j++)
            {
                _transitionMatrix[i][j] = transitionMatrix[i][j];
                rowSum += transitionMatrix[i][j];
            }
            if (System.Math.Abs(rowSum - 1.0) > 1e-6)
                throw new ArgumentException($"Transition matrix row {i} must sum to 1.0.");
        }

        _emissionMatrix = new double[_numStates][];
        for (int i = 0; i < _numStates; i++)
        {
            if (emissionMatrix[i].Length != _numObservations)
                throw new ArgumentException($"Emission matrix row {i} must have {_numObservations} elements.");
            _emissionMatrix[i] = new double[_numObservations];
            double rowSum = 0.0;
            for (int j = 0; j < _numObservations; j++)
            {
                _emissionMatrix[i][j] = emissionMatrix[i][j];
                rowSum += emissionMatrix[i][j];
            }
            if (System.Math.Abs(rowSum - 1.0) > 1e-6)
                throw new ArgumentException($"Emission matrix row {i} must sum to 1.0.");
        }
    }

    /// <summary>Gets the number of hidden states.</summary>
    public int NumStates => _numStates;

    /// <summary>Gets the number of possible observations.</summary>
    public int NumObservations => _numObservations;

    /// <summary>Computes the probability of an observation sequence using the forward algorithm.</summary>
    /// <param name="observations">Sequence of observation indices.</param>
    /// <returns>Log probability of the observation sequence (to avoid underflow).</returns>
    public double Forward(double[] observations)
    {
        if (observations == null)
            throw new ArgumentNullException(nameof(observations));

        int T = observations.Length;
        double[][] alpha = new double[T][];

        for (int t = 0; t < T; t++)
            alpha[t] = new double[_numStates];

        for (int i = 0; i < _numStates; i++)
            alpha[0][i] = _initialProbs[i] * _emissionMatrix[i][(int)observations[0]];

        for (int t = 1; t < T; t++)
        {
            for (int j = 0; j < _numStates; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < _numStates; i++)
                    sum += alpha[t - 1][i] * _transitionMatrix[i][j];
                alpha[t][j] = sum * _emissionMatrix[j][(int)observations[t]];
            }
        }

        double prob = 0.0;
        for (int i = 0; i < _numStates; i++)
            prob += alpha[T - 1][i];

        return prob;
    }

    /// <summary>Runs the forward algorithm and returns the full alpha table.</summary>
    /// <param name="observations">Sequence of observation indices.</param>
    /// <returns>Forward probability table alpha[t][state].</returns>
    public double[][] ForwardCompute(double[] observations)
    {
        if (observations == null)
            throw new ArgumentNullException(nameof(observations));

        int T = observations.Length;
        double[][] alpha = new double[T][];

        for (int t = 0; t < T; t++)
            alpha[t] = new double[_numStates];

        for (int i = 0; i < _numStates; i++)
            alpha[0][i] = _initialProbs[i] * _emissionMatrix[i][(int)observations[0]];

        for (int t = 1; t < T; t++)
        {
            for (int j = 0; j < _numStates; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < _numStates; i++)
                    sum += alpha[t - 1][i] * _transitionMatrix[i][j];
                alpha[t][j] = sum * _emissionMatrix[j][(int)observations[t]];
            }
        }

        return alpha;
    }

    /// <summary>Finds the most likely state sequence using the Viterbi algorithm.</summary>
    /// <param name="observations">Sequence of observation indices.</param>
    /// <returns>Most likely sequence of hidden state indices.</returns>
    public int[] Viterbi(double[] observations)
    {
        if (observations == null)
            throw new ArgumentNullException(nameof(observations));

        int T = observations.Length;
        double[][] delta = new double[T][];
        int[][] psi = new int[T][];

        for (int t = 0; t < T; t++)
        {
            delta[t] = new double[_numStates];
            psi[t] = new int[_numStates];
        }

        for (int i = 0; i < _numStates; i++)
        {
            delta[0][i] = _initialProbs[i] * _emissionMatrix[i][(int)observations[0]];
            psi[0][i] = 0;
        }

        for (int t = 1; t < T; t++)
        {
            for (int j = 0; j < _numStates; j++)
            {
                double maxProb = 0.0;
                int maxState = 0;

                for (int i = 0; i < _numStates; i++)
                {
                    double prob = delta[t - 1][i] * _transitionMatrix[i][j];
                    if (prob > maxProb)
                    {
                        maxProb = prob;
                        maxState = i;
                    }
                }

                delta[t][j] = maxProb * _emissionMatrix[j][(int)observations[t]];
                psi[t][j] = maxState;
            }
        }

        double bestFinalProb = 0.0;
        int bestFinalState = 0;
        for (int i = 0; i < _numStates; i++)
        {
            if (delta[T - 1][i] > bestFinalProb)
            {
                bestFinalProb = delta[T - 1][i];
                bestFinalState = i;
            }
        }

        int[] path = new int[T];
        path[T - 1] = bestFinalState;

        for (int t = T - 2; t >= 0; t--)
            path[t] = psi[t + 1][path[t + 1]];

        return path;
    }

    /// <summary>Runs the Baum-Welch (EM) algorithm to train the HMM parameters.</summary>
    /// <param name="observationSequences">Multiple observation sequences for training.</param>
    /// <param name="maxIterations">Maximum number of EM iterations.</param>
    /// <param name="tolerance">Convergence tolerance for log-likelihood change.</param>
    /// <returns>Final log-likelihood after training.</returns>
    public double BaumWelch(double[][] observationSequences, int maxIterations = 100, double tolerance = 1e-6)
    {
        if (observationSequences == null)
            throw new ArgumentNullException(nameof(observationSequences));

        double[] pi = new double[_numStates];
        double[][] A = new double[_numStates][];
        double[][] B = new double[_numStates][];

        Array.Copy(_initialProbs, pi, _numStates);
        for (int i = 0; i < _numStates; i++)
        {
            A[i] = new double[_numStates];
            Array.Copy(_transitionMatrix[i], A[i], _numStates);
            B[i] = new double[_numObservations];
            Array.Copy(_emissionMatrix[i], B[i], _numObservations);
        }

        double prevLogLikelihood = double.MinValue;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] piNum = new double[_numStates];
            double[][] ANum = new double[_numStates][];
            double[][] BNum = new double[_numStates][];

            for (int i = 0; i < _numStates; i++)
            {
                ANum[i] = new double[_numStates];
                BNum[i] = new double[_numObservations];
            }

            double logLikelihood = 0.0;

            foreach (double[] obs in observationSequences)
            {
                int T = obs.Length;
                double[][] alpha = ForwardInternal(pi, A, B, obs);
                double[][] beta = BackwardInternal(pi, A, B, obs);

                double[][] gamma = new double[T][];
                double[][][] xi = new double[T - 1][][];

                for (int t = 0; t < T; t++)
                    gamma[t] = new double[_numStates];

                for (int t = 0; t < T - 1; t++)
                {
                    xi[t] = new double[_numStates][];
                    for (int i = 0; i < _numStates; i++)
                        xi[t][i] = new double[_numStates];
                }

                for (int t = 0; t < T; t++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < _numStates; i++)
                    {
                        gamma[t][i] = alpha[t][i] * beta[t][i];
                        sum += gamma[t][i];
                    }
                    if (sum > 0.0)
                    {
                        for (int i = 0; i < _numStates; i++)
                            gamma[t][i] /= sum;
                    }
                }

                for (int t = 0; t < T - 1; t++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < _numStates; i++)
                    {
                        for (int j = 0; j < _numStates; j++)
                        {
                            xi[t][i][j] = alpha[t][i] * A[i][j] * B[j][(int)obs[t + 1]] * beta[t + 1][j];
                            sum += xi[t][i][j];
                        }
                    }
                    if (sum > 0.0)
                    {
                        for (int i = 0; i < _numStates; i++)
                            for (int j = 0; j < _numStates; j++)
                                xi[t][i][j] /= sum;
                    }
                }

                for (int i = 0; i < _numStates; i++)
                {
                    piNum[i] += gamma[0][i];
                    for (int j = 0; j < _numStates; j++)
                    {
                        double xiSum = 0.0;
                        for (int t = 0; t < T - 1; t++)
                            xiSum += xi[t][i][j];
                        ANum[i][j] += xiSum;
                    }

                    for (int k = 0; k < _numObservations; k++)
                    {
                        double gammaSum = 0.0;
                        for (int t = 0; t < T; t++)
                        {
                            if ((int)obs[t] == k)
                                gammaSum += gamma[t][i];
                        }
                        BNum[i][k] += gammaSum;
                    }
                }

                for (int i = 0; i < _numStates; i++)
                    logLikelihood += System.Math.Log(alpha[T - 1][i] + 1e-300);
            }

            int N = observationSequences.Length;
            double piSum = 0.0;
            for (int i = 0; i < _numStates; i++)
            {
                pi[i] = piNum[i] / N;
                piSum += pi[i];
            }
            if (piSum > 0.0)
            {
                for (int i = 0; i < _numStates; i++)
                    pi[i] /= piSum;
            }

            for (int i = 0; i < _numStates; i++)
            {
                double gammaSumAll = 0.0;
                for (int t = 0; t < _numStates; t++)
                    gammaSumAll += ANum[i][t];

                if (gammaSumAll > 1e-300)
                {
                    for (int j = 0; j < _numStates; j++)
                        A[i][j] = ANum[i][j] / gammaSumAll;
                }

                double bSum = BNum[i][0];
                for (int k = 1; k < _numObservations; k++)
                    bSum += BNum[i][k];

                if (bSum > 1e-300)
                {
                    for (int k = 0; k < _numObservations; k++)
                        B[i][k] = BNum[i][k] / bSum;
                }
            }

            for (int i = 0; i < _numStates; i++)
            {
                Array.Copy(A[i], _transitionMatrix[i], _numStates);
                Array.Copy(B[i], _emissionMatrix[i], _numObservations);
            }
            Array.Copy(pi, _initialProbs, _numStates);

            if (System.Math.Abs(logLikelihood - prevLogLikelihood) < tolerance)
                break;

            prevLogLikelihood = logLikelihood;
        }

        return prevLogLikelihood;
    }

    /// <summary>Predicts the most likely state sequence for given observations.</summary>
    /// <param name="observations">Sequence of observation indices.</param>
    /// <returns>Most likely hidden state sequence.</returns>
    public int[] Predict(double[] observations) => Viterbi(observations);

    private double[][] ForwardInternal(double[] pi, double[][] A, double[][] B, double[] obs)
    {
        int T = obs.Length;
        double[][] alpha = new double[T][];
        for (int t = 0; t < T; t++)
            alpha[t] = new double[_numStates];

        for (int i = 0; i < _numStates; i++)
            alpha[0][i] = pi[i] * B[i][(int)obs[0]];

        for (int t = 1; t < T; t++)
        {
            for (int j = 0; j < _numStates; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < _numStates; i++)
                    sum += alpha[t - 1][i] * A[i][j];
                alpha[t][j] = sum * B[j][(int)obs[t]];
            }
        }

        return alpha;
    }

    private double[][] BackwardInternal(double[] pi, double[][] A, double[][] B, double[] obs)
    {
        int T = obs.Length;
        double[][] beta = new double[T][];
        for (int t = 0; t < T; t++)
            beta[t] = new double[_numStates];

        for (int i = 0; i < _numStates; i++)
            beta[T - 1][i] = 1.0;

        for (int t = T - 2; t >= 0; t--)
        {
            for (int i = 0; i < _numStates; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < _numStates; j++)
                    sum += A[i][j] * B[j][(int)obs[t + 1]] * beta[t + 1][j];
                beta[t][i] = sum;
            }
        }

        return beta;
    }
}
