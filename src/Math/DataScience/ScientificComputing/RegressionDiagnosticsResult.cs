namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Represents the result of regression diagnostics including goodness-of-fit measures
    /// and residual analysis.
    /// </summary>
    public sealed class RegressionDiagnosticsResult
    {
        /// <summary>
        /// Gets the coefficient of determination R^2.
        /// </summary>
        public double R2 { get; }

        /// <summary>
        /// Gets the adjusted R^2 accounting for the number of predictors.
        /// </summary>
        public double AdjustedR2 { get; }

        /// <summary>
        /// Gets the root mean square error.
        /// </summary>
        public double RMSE { get; }

        /// <summary>
        /// Gets the mean absolute error.
        /// </summary>
        public double MAE { get; }

        /// <summary>
        /// Gets the mean absolute percentage error (percentage).
        /// </summary>
        public double MAPE { get; }

        /// <summary>
        /// Gets the Durbin-Watson statistic for detecting autocorrelation in residuals.
        /// Values near 2 indicate no autocorrelation; values near 0 indicate positive autocorrelation;
        /// values near 4 indicate negative autocorrelation.
        /// </summary>
        public double DurbinWatson { get; }

        /// <summary>
        /// Gets the raw residual values (actual - predicted).
        /// </summary>
        public double[] Residuals { get; }

        /// <summary>
        /// Gets the standardized (studentized) residuals, computed by dividing each residual
        /// by the standard error of the regression.
        /// </summary>
        public double[] StandardizedResiduals { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionDiagnosticsResult"/> class.
        /// </summary>
        /// <param name="r2">The coefficient of determination.</param>
        /// <param name="adjustedR2">The adjusted coefficient of determination.</param>
        /// <param name="rmse">The root mean square error.</param>
        /// <param name="mae">The mean absolute error.</param>
        /// <param name="mape">The mean absolute percentage error.</param>
        /// <param name="durbinWatson">The Durbin-Watson statistic.</param>
        /// <param name="residuals">The residual array.</param>
        /// <param name="standardizedResiduals">The standardized residual array.</param>
        public RegressionDiagnosticsResult(
            double r2, double adjustedR2, double rmse, double mae, double mape,
            double durbinWatson, double[] residuals, double[] standardizedResiduals)
        {
            R2 = r2;
            AdjustedR2 = adjustedR2;
            RMSE = rmse;
            MAE = mae;
            MAPE = mape;
            DurbinWatson = durbinWatson;
            Residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            StandardizedResiduals = standardizedResiduals ?? throw new ArgumentNullException(nameof(standardizedResiduals));
        }
    }
}
