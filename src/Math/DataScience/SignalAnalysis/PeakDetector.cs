namespace MathVerse.Math.DataScience.SignalAnalysis
{
    using System;

    /// <summary>
    /// Provides peak detection in signals with configurable height, distance, and threshold constraints.
    /// </summary>
    public sealed class PeakDetector
    {
        /// <summary>
        /// Detects peaks (local maxima) in the signal that satisfy the given constraints.
        /// A peak is defined as a sample greater than its neighbors and meeting minimum height,
        /// minimum distance, and threshold criteria.
        /// </summary>
        /// <param name="signal">The input signal.</param>
        /// <param name="minHeight">The minimum amplitude a peak must have. Default is 0.</param>
        /// <param name="minDistance">The minimum number of samples between adjacent peaks. Default is 1.</param>
        /// <param name="threshold">The minimum vertical distance a peak must have relative to its neighbors. Default is 0.</param>
        /// <returns>An array of indices where peaks occur in the signal.</returns>
        public static int[] Detect(double[] signal, double minHeight = 0.0, int minDistance = 1, double threshold = 0.0)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length == 0) throw new ArgumentException("Signal must not be empty.");
            if (minDistance < 1) throw new ArgumentException("Minimum distance must be at least 1.");

            int n = signal.Length;
            bool[] isPeak = new bool[n];

            for (int i = 1; i < n - 1; i++)
            {
                if (signal[i] > signal[i - 1] && signal[i] > signal[i + 1])
                {
                    if (signal[i] >= minHeight)
                    {
                        double leftDiff = signal[i] - signal[i - 1];
                        double rightDiff = signal[i] - signal[i + 1];

                        if (leftDiff >= threshold && rightDiff >= threshold)
                        {
                            isPeak[i] = true;
                        }
                    }
                }
            }

            int count = 0;
            for (int i = 0; i < n; i++)
            {
                if (isPeak[i]) count++;
            }

            int[] candidates = new int[count];
            int idx = 0;
            for (int i = 0; i < n; i++)
            {
                if (isPeak[i])
                {
                    candidates[idx] = i;
                    idx++;
                }
            }

            if (candidates.Length == 0)
                return Array.Empty<int>();

            if (minDistance <= 1)
                return candidates;

            int[] selected = new int[candidates.Length];
            int selectedCount = 0;
            int lastSelected = -minDistance;

            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] - lastSelected >= minDistance)
                {
                    selected[selectedCount] = candidates[i];
                    lastSelected = candidates[i];
                    selectedCount++;
                }
                else
                {
                    if (signal[candidates[i]] > signal[selected[selectedCount - 1]])
                    {
                        selected[selectedCount - 1] = candidates[i];
                        lastSelected = candidates[i];
                    }
                }
            }

            int[] result = new int[selectedCount];
            for (int i = 0; i < selectedCount; i++)
            {
                result[i] = selected[i];
            }
            return result;
        }
    }
}
