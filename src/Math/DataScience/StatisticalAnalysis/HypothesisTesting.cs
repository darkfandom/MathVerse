namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the type of t-test to perform.
    /// </summary>
    public enum TTestType
    {
        /// <summary>Two independent samples t-test.</summary>
        TwoSample,

        /// <summary>One sample t-test against a hypothesized mean.</summary>
        OneSample,

        /// <summary>Paired samples t-test.</summary>
        Paired
    }

    /// <summary>
    /// Performs various hypothesis tests for statistical inference.
    /// </summary>
    public sealed class HypothesisTesting
    {
        /// <summary>
        /// Performs a t-test to compare means.
        /// Uses Welch's approximation for unequal variances.
        /// </summary>
        /// <param name="sample1">The first sample data.</param>
        /// <param name="sample2">The second sample data (or hypothesized mean for one-sample test).</param>
        /// <param name="type">The type of t-test to perform.</param>
        /// <returns>A TTestResult with the test statistic, p-value, and other statistics.</returns>
        public static TTestResult TTest(double[] sample1, double[] sample2, TTestType type = TTestType.TwoSample)
        {
            if (sample1 is null) throw new ArgumentNullException(nameof(sample1));
            if (sample2 is null) throw new ArgumentNullException(nameof(sample2));

            double mean1 = ComputeMean(sample1);
            double var1 = ComputeVariance(sample1, mean1);
            int n1 = sample1.Length;

            TTestResult result = new();

            switch (type)
            {
                case TTestType.TwoSample:
                {
                    double mean2 = ComputeMean(sample2);
                    double var2 = ComputeVariance(sample2, mean2);
                    int n2 = sample2.Length;

                    double se = System.Math.Sqrt(var1 / n1 + var2 / n2);
                    result.TStatistic = se > 0.0 ? (mean1 - mean2) / se : 0.0;

                    double num = System.Math.Pow(var1 / n1 + var2 / n2, 2);
                    double denom1 = System.Math.Pow(var1 / n1, 2) / (n1 - 1);
                    double denom2 = System.Math.Pow(var2 / n2, 2) / (n2 - 1);
                    result.DegreesOfFreedom = num / (denom1 + denom2);
                    result.PValue = 2.0 * (1.0 - TCDF(System.Math.Abs(result.TStatistic), result.DegreesOfFreedom));

                    result.Mean1 = mean1;
                    result.Mean2 = mean2;
                    result.StdDev1 = System.Math.Sqrt(var1);
                    result.StdDev2 = System.Math.Sqrt(var2);
                    break;
                }

                case TTestType.OneSample:
                {
                    double hypothesizedMean = sample2.Length > 0 ? sample2[0] : 0.0;
                    double se = System.Math.Sqrt(var1 / n1);
                    result.TStatistic = se > 0.0 ? (mean1 - hypothesizedMean) / se : 0.0;
                    result.DegreesOfFreedom = n1 - 1;
                    result.PValue = 2.0 * (1.0 - TCDF(System.Math.Abs(result.TStatistic), result.DegreesOfFreedom));

                    result.Mean1 = mean1;
                    result.StdDev1 = System.Math.Sqrt(var1);
                    break;
                }

                case TTestType.Paired:
                {
                    if (sample1.Length != sample2.Length)
                        throw new ArgumentException("Paired samples must have equal length.");

                    int n = sample1.Length;
                    double[] diffs = new double[n];
                    for (int i = 0; i < n; i++)
                        diffs[i] = sample1[i] - sample2[i];

                    double diffMean = ComputeMean(diffs);
                    double diffVar = ComputeVariance(diffs, diffMean);
                    double se = System.Math.Sqrt(diffVar / n);

                    result.TStatistic = se > 0.0 ? diffMean / se : 0.0;
                    result.DegreesOfFreedom = n - 1;
                    result.PValue = 2.0 * (1.0 - TCDF(System.Math.Abs(result.TStatistic), result.DegreesOfFreedom));

                    result.Mean1 = mean1;
                    result.Mean2 = ComputeMean(sample2);
                    result.StdDev1 = System.Math.Sqrt(var1);
                    result.StdDev2 = System.Math.Sqrt(ComputeVariance(sample2, result.Mean2));
                    break;
                }
            }

            result.Significant = result.PValue < 0.05;
            return result;
        }

        /// <summary>
        /// Performs a chi-square goodness-of-fit test.
        /// Tests whether observed frequencies match expected frequencies.
        /// </summary>
        /// <param name="observed">The observed frequencies.</param>
        /// <param name="expected">The expected frequencies.</param>
        /// <returns>A ChiSquareResult with the test statistic, p-value, and degrees of freedom.</returns>
        public static ChiSquareResult ChiSquareTest(double[] observed, double[] expected)
        {
            if (observed is null) throw new ArgumentNullException(nameof(observed));
            if (expected is null) throw new ArgumentNullException(nameof(expected));
            if (observed.Length != expected.Length)
                throw new ArgumentException("Observed and expected arrays must have the same length.");

            ChiSquareResult result = new();
            double chiSquare = 0.0;

            for (int i = 0; i < observed.Length; i++)
            {
                if (expected[i] > 0.0)
                {
                    double diff = observed[i] - expected[i];
                    chiSquare += (diff * diff) / expected[i];
                }
            }

            result.ChiSquare = chiSquare;
            result.DegreesOfFreedom = observed.Length - 1;
            result.PValue = 1.0 - ChiSquareCDF(chiSquare, result.DegreesOfFreedom);
            result.Significant = result.PValue < 0.05;

            return result;
        }

        /// <summary>
        /// Performs a one-way ANOVA F-test to compare means across multiple groups.
        /// </summary>
        /// <param name="groups">An array of group data arrays.</param>
        /// <returns>An ANOVAResult with the F-statistic, p-value, and degrees of freedom.</returns>
        public static ANOVAResult ANOVA(double[][] groups)
        {
            if (groups is null) throw new ArgumentNullException(nameof(groups));
            if (groups.Length < 2) throw new ArgumentException("At least two groups are required.", nameof(groups));

            int k = groups.Length;
            int N = 0;
            double grandSum = 0.0;

            double[] groupMeans = new double[k];
            double[] groupSizes = new double[k];

            for (int i = 0; i < k; i++)
            {
                double sum = 0.0;
                foreach (double v in groups[i]) sum += v;
                groupSizes[i] = groups[i].Length;
                N += groups[i].Length;
                groupMeans[i] = sum / groupSizes[i];
                grandSum += sum;
            }

            double grandMean = grandSum / N;

            double ssb = 0.0;
            for (int i = 0; i < k; i++)
            {
                double diff = groupMeans[i] - grandMean;
                ssb += groupSizes[i] * diff * diff;
            }

            double ssw = 0.0;
            for (int i = 0; i < k; i++)
            {
                foreach (double v in groups[i])
                {
                    double diff = v - groupMeans[i];
                    ssw += diff * diff;
                }
            }

            double dfBetween = k - 1;
            double dfWithin = N - k;

            double msb = ssb / dfBetween;
            double msw = dfWithin > 0.0 ? ssw / dfWithin : 0.0;

            double fStatistic = msw > 0.0 ? msb / msw : 0.0;

            ANOVAResult result = new()
            {
                FStatistic = fStatistic,
                DegreesOfFreedomBetween = dfBetween,
                DegreesOfFreedomWithin = dfWithin,
                MSB = msb,
                MSW = msw,
                SSB = ssb,
                SSW = ssw,
                PValue = 1.0 - FCDF(fStatistic, dfBetween, dfWithin),
                Significant = false
            };
            result.Significant = result.PValue < 0.05;

            return result;
        }

        /// <summary>
        /// Performs the Mann-Whitney U test (Wilcoxon rank-sum test) for comparing two independent samples.
        /// Non-parametric alternative to the two-sample t-test.
        /// </summary>
        /// <param name="sample1">The first sample data.</param>
        /// <param name="sample2">The second sample data.</param>
        /// <returns>A TTestResult with the U statistic, p-value, and z-approximation.</returns>
        public static TTestResult MannWhitneyTest(double[] sample1, double[] sample2)
        {
            if (sample1 is null) throw new ArgumentNullException(nameof(sample1));
            if (sample2 is null) throw new ArgumentNullException(nameof(sample2));

            int n1 = sample1.Length;
            int n2 = sample2.Length;
            int N = n1 + n2;

            (double value, int group, int originalIndex)[] combined = new (double, int, int)[N];
            for (int i = 0; i < n1; i++)
                combined[i] = (sample1[i], 0, i);
            for (int i = 0; i < n2; i++)
                combined[n1 + i] = (sample2[i], 1, i);

            Array.Sort(combined, (a, b) => a.value.CompareTo(b.value));

            double[] ranks = new double[N];
            int idx = 0;
            while (idx < N)
            {
                int j = idx;
                while (j < N - 1 && combined[j + 1].value == combined[j].value)
                    j++;

                double avgRank = (idx + j) / 2.0 + 1.0;
                for (int k = idx; k <= j; k++)
                    ranks[k] = avgRank;
                idx = j + 1;
            }

            double rankSum1 = 0.0;
            for (int i = 0; i < N; i++)
            {
                if (combined[i].group == 0)
                    rankSum1 += ranks[i];
            }

            double U1 = rankSum1 - n1 * (n1 + 1) / 2.0;
            double U2 = n1 * n2 - U1;
            double U = System.Math.Min(U1, U2);

            double meanU = n1 * n2 / 2.0;
            double varU = n1 * n2 * (N + 1) / 12.0;
            double z = varU > 0.0 ? (U - meanU) / System.Math.Sqrt(varU) : 0.0;

            TTestResult result = new()
            {
                TStatistic = U,
                PValue = 2.0 * (1.0 - NormalCDF(System.Math.Abs(z))),
                DegreesOfFreedom = N - 2,
                Mean1 = ComputeMean(sample1),
                Mean2 = ComputeMean(sample2),
                StdDev1 = System.Math.Sqrt(ComputeVariance(sample1, ComputeMean(sample1))),
                StdDev2 = System.Math.Sqrt(ComputeVariance(sample2, ComputeMean(sample2))),
                Significant = false
            };
            result.Significant = result.PValue < 0.05;

            return result;
        }

        /// <summary>
        /// Performs the Kolmogorov-Smirnov two-sample test to compare distributions.
        /// </summary>
        /// <param name="sample1">The first sample data.</param>
        /// <param name="sample2">The second sample data.</param>
        /// <returns>A TTestResult with the KS statistic and p-value.</returns>
        public static TTestResult KolmogorovSmirnovTest(double[] sample1, double[] sample2)
        {
            if (sample1 is null) throw new ArgumentNullException(nameof(sample1));
            if (sample2 is null) throw new ArgumentNullException(nameof(sample2));

            int n1 = sample1.Length;
            int n2 = sample2.Length;

            double[] sorted1 = (double[])sample1.Clone();
            double[] sorted2 = (double[])sample2.Clone();
            Array.Sort(sorted1);
            Array.Sort(sorted2);

            double dMax = 0.0;
            int i = 0, j = 0;

            while (i < n1 && j < n2)
            {
                double ecdf1 = (double)(i + 1) / n1;
                double ecdf2 = (double)(j + 1) / n2;

                double d = System.Math.Abs(ecdf1 - ecdf2);
                if (d > dMax) dMax = d;

                if (sorted1[i] < sorted2[j])
                    i++;
                else if (sorted1[i] > sorted2[j])
                    j++;
                else
                {
                    i++;
                    j++;
                }
            }

            double lambda = System.Math.Sqrt(n1 * n2 / (double)(n1 + n2));
            double pValue = 0.0;
            double t = (lambda + 0.12 + 0.11 / lambda) * dMax;
            for (int k = 1; k <= 100; k++)
            {
                pValue += System.Math.Pow(-1.0, k - 1) * System.Math.Exp(-2.0 * t * t * k * k);
            }
            pValue = System.Math.Min(1.0, System.Math.Max(0.0, 2.0 * pValue));

            TTestResult result = new()
            {
                TStatistic = dMax,
                PValue = pValue,
                DegreesOfFreedom = n1 + n2 - 2,
                Mean1 = ComputeMean(sample1),
                Mean2 = ComputeMean(sample2),
                StdDev1 = System.Math.Sqrt(ComputeVariance(sample1, ComputeMean(sample1))),
                StdDev2 = System.Math.Sqrt(ComputeVariance(sample2, ComputeMean(sample2))),
                Significant = pValue < 0.05
            };

            return result;
        }

        private static double ComputeMean(double[] data)
        {
            double sum = 0.0;
            foreach (double v in data) sum += v;
            return sum / data.Length;
        }

        private static double ComputeVariance(double[] data, double mean)
        {
            double sum = 0.0;
            foreach (double v in data)
            {
                double diff = v - mean;
                sum += diff * diff;
            }
            return sum / (data.Length - 1);
        }

        private static double TCDF(double t, double df)
        {
            double x = df / (df + t * t);
            return 1.0 - 0.5 * IncompleteBeta(df / 2.0, 0.5, x);
        }

        private static double FCDF(double f, double d1, double d2)
        {
            double x = d1 * f / (d1 * f + d2);
            return IncompleteBeta(d1 / 2.0, d2 / 2.0, x);
        }

        private static double NormalCDF(double x)
        {
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            int sign = x < 0 ? -1 : 1;
            double absX = System.Math.Abs(x);
            double t = 1.0 / (1.0 + p * absX);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * System.Math.Exp(-absX * absX / 2.0);
            return 0.5 * (1.0 + sign * y);
        }

        private static double ChiSquareCDF(double x, int df)
        {
            if (x <= 0.0) return 0.0;
            return RegularizedGammaP(df / 2.0, x / 2.0);
        }

        private static double IncompleteBeta(double a, double b, double x)
        {
            if (x < 0.0 || x > 1.0) return 0.0;
            if (x == 0.0 || x == 1.0) return x;

            double bt = System.Math.Exp(
                LnGamma(a + b) - LnGamma(a) - LnGamma(b) +
                a * System.Math.Log(x) + b * System.Math.Log(1.0 - x));

            if (x < (a + 1.0) / (a + b + 2.0))
            {
                return bt * BetaCF(a, b, x) / a;
            }
            else
            {
                return 1.0 - bt * BetaCF(b, a, 1.0 - x) / b;
            }
        }

        private static double BetaCF(double a, double b, double x)
        {
            const int maxIter = 200;
            const double eps = 3.0e-12;

            double qab = a + b;
            double qap = a + 1.0;
            double qam = a - 1.0;

            double c = 1.0;
            double d = 1.0 - qab * x / qap;
            if (System.Math.Abs(d) < 1.0e-30) d = 1.0e-30;
            d = 1.0 / d;
            double h = d;

            for (int m = 1; m <= maxIter; m++)
            {
                int m2 = 2 * m;
                double aa = m * (b - m) * x / ((qam + m2) * (a + m2));
                d = 1.0 + aa * d;
                if (System.Math.Abs(d) < 1.0e-30) d = 1.0e-30;
                c = 1.0 + aa / c;
                if (System.Math.Abs(c) < 1.0e-30) c = 1.0e-30;
                d = 1.0 / d;
                h *= d * c;

                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
                d = 1.0 + aa * d;
                if (System.Math.Abs(d) < 1.0e-30) d = 1.0e-30;
                c = 1.0 + aa / c;
                if (System.Math.Abs(c) < 1.0e-30) c = 1.0e-30;
                d = 1.0 / d;
                double del = d * c;
                h *= del;

                if (System.Math.Abs(del - 1.0) < eps) break;
            }

            return h;
        }

        private static double LnGamma(double x)
        {
            double[] cof = new double[]
            {
                57.1562356658629235,
                -59.5979603554754912,
                14.1360979747416475,
                -0.491913816097620199,
                0.339946499848118887e-4,
                0.465236289270485756e-4,
                -0.983744753048795646e-4,
                0.158088703224912494e-3,
                -0.210264441741720009e-3,
                0.217439618115212643e-3,
                -0.164318106536763890e-3,
                0.844182239838527433e-4,
                -0.261908384015814087e-4,
                0.368991826595316234e-5
            };

            if (x <= 0.0) return double.NaN;

            double y = x;
            double tmp = x + 5.24218750000000000;
            tmp = (x + 0.5) * System.Math.Log(tmp) - tmp;
            double ser = 0.999999999999997092;

            for (int j = 0; j < 14; j++)
            {
                ser += cof[j] / ++y;
            }

            return tmp + System.Math.Log(2.50662827463100050 * ser / x);
        }

        private static double RegularizedGammaP(double a, double x)
        {
            if (x < 0.0 || a <= 0.0) return 0.0;
            if (x == 0.0) return 0.0;

            if (x < a + 1.0)
            {
                double sum = 1.0 / a;
                double ap = a;
                double delta = sum;
                for (int n = 1; n < 200; n++)
                {
                    ap += 1.0;
                    delta *= x / ap;
                    sum += delta;
                    if (System.Math.Abs(delta) < System.Math.Abs(sum) * 3.0e-12)
                        break;
                }
                return sum * System.Math.Exp(-x + a * System.Math.Log(x) - LnGamma(a));
            }
            else
            {
                double[] b = new double[] { 1.0, 1.0, 2.0, 6.0, 24.0, 120.0, 720.0, 5040.0, 40320.0, 362880.0 };
                double an = 0.0;
                double bn = b[0];
                double aVal = b[0];
                for (int n = 1; n < 200; n++)
                {
                    an = x + 1.0 - n;
                    bn = x + 1.0 + n;
                    aVal = an / bn * aVal;
                    double[] bNew = new double[b.Length];
                    for (int i = 0; i < b.Length - 1; i++)
                    {
                        bNew[i] = (i + 1) * b[i] + x * b[i + 1];
                    }
                    bNew[^1] = x * b[^1];
                    double sum = 0.0;
                    for (int i = 0; i < bNew.Length; i++) sum += bNew[i];
                    aVal = an / bn * aVal;
                    b = bNew;
                }
                return 1.0 - System.Math.Exp(-x + a * System.Math.Log(x) - LnGamma(a)) * aVal;
            }
        }
    }
}