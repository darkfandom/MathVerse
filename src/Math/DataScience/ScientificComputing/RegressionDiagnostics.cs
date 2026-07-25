namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Provides regression diagnostics including R^2, adjusted R^2, RMSE, MAE, MAPE,
    /// Durbin-Watson statistic, and residual analysis.
    /// </summary>
    public sealed class RegressionDiagnostics
    {
        /// <summary>
        /// Computes comprehensive regression diagnostics comparing actual and predicted values.
        /// </summary>
        /// <param name="actual">The observed (actual) values.</param>
        /// <param name="predicted">The model-predicted values.</param>
        /// <returns>A <see cref="RegressionDiagnosticsResult"/> with all computed diagnostics.</returns>
        public static RegressionDiagnosticsResult Compute(double[] actual, double[] predicted)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));
            if (predicted == null) throw new ArgumentNullException(nameof(predicted));
            if (actual.Length != predicted.Length)
                throw new ArgumentException("Actual and predicted arrays must have the same length.");
            if (actual.Length < 2)
                throw new ArgumentException("At least 2 data points are required.");

            int n = actual.Length;

            double meanActual = 0.0;
            for (int i = 0; i < n; i++)
            {
                meanActual += actual[i];
            }
            meanActual /= n;

            double[] residuals = new double[n];
            double ssRes = 0.0, ssTot = 0.0;
            double sumAbsError = 0.0;
            double sumAbsPercentError = 0.0;
            int mapeCount = 0;

            for (int i = 0; i < n; i++)
            {
                residuals[i] = actual[i] - predicted[i];
                ssRes += residuals[i] * residuals[i];
                ssTot += (actual[i] - meanActual) * (actual[i] - meanActual);
                sumAbsError += System.Math.Abs(residuals[i]);

                if (actual[i] != 0.0)
                {
                    sumAbsPercentError += System.Math.Abs(residuals[i] / actual[i]);
                    mapeCount++;
                }
            }

            double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
            double rmse = System.Math.Sqrt(ssRes / n);
            double mae = sumAbsError / n;
            double mape = mapeCount > 0 ? (sumAbsPercentError / mapeCount) * 100.0 : 0.0;

            double adjustedR2 = 1.0 - (1.0 - r2) * (n - 1.0) / (n - 2.0);

            double dwNumerator = 0.0;
            double dwDenominator = 0.0;
            for (int i = 0; i < n; i++)
            {
                dwDenominator += residuals[i] * residuals[i];
                if (i > 0)
                {
                    double diff = residuals[i] - residuals[i - 1];
                    dwNumerator += diff * diff;
                }
            }

            double durbinWatson = dwDenominator > 0 ? dwNumerator / dwDenominator : 0.0;

            double sumSqResid = 0.0;
            for (int i = 0; i < n; i++)
            {
                sumSqResid += residuals[i] * residuals[i];
            }
            double residualStdErr = System.Math.Sqrt(sumSqResid / (n - 2));

            double[] standardizedResiduals = new double[n];
            for (int i = 0; i < n; i++)
            {
                standardizedResiduals[i] = residualStdErr > 0
                    ? residuals[i] / residualStdErr
                    : 0.0;
            }

            return new RegressionDiagnosticsResult(
                r2, adjustedR2, rmse, mae, mape, durbinWatson,
                residuals, standardizedResiduals);
        }
    }
}
