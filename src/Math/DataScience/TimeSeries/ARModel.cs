namespace MathVerse.Math.DataScience.TimeSeries
{
    using System;

    /// <summary>
    /// Provides autoregressive (AR) model fitting and prediction using the Yule-Walker equations
    /// solved via the Levinson-Durbin recursion.
    /// </summary>
    public sealed class ARModel
    {
        /// <summary>
        /// Fits an AR(p) model to the data using the Yule-Walker equations and the Levinson-Durbin algorithm.
        /// </summary>
        /// <param name="data">The input time series data.</param>
        /// <param name="order">The order of the autoregressive model (must be positive and less than data length).</param>
        /// <returns>An array of AR coefficients [phi_1, phi_2, ..., phi_p].</returns>
        public static double[] Fit(double[] data, int order)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (order <= 0) throw new ArgumentException("Order must be positive.");
            if (order >= data.Length)
                throw new ArgumentException("Order must be less than data length.");

            int n = data.Length;

            double mean = 0.0;
            for (int i = 0; i < n; i++)
            {
                mean += data[i];
            }
            mean /= n;

            double[] gamma = new double[order + 1];
            for (int k = 0; k <= order; k++)
            {
                double sum = 0.0;
                for (int t = 0; t < n - k; t++)
                {
                    double a = data[t] - mean;
                    double b = data[t + k] - mean;
                    sum += a * b;
                }
                gamma[k] = sum / n;
            }

            double[] phi = new double[order + 1];
            phi[0] = 1.0;
            double error = gamma[0];

            for (int m = 1; m <= order; m++)
            {
                double lambda = gamma[m];
                for (int j = 1; j < m; j++)
                {
                    lambda += phi[j] * gamma[m - j];
                }

                double k = -lambda / error;

                double[] newPhi = new double[order + 1];
                newPhi[0] = 1.0;
                newPhi[m] = k;
                for (int j = 1; j < m; j++)
                {
                    newPhi[j] = phi[j] + k * phi[m - j];
                }
                phi = newPhi;

                error *= (1.0 - k * k);
            }

            double[] coefficients = new double[order];
            for (int i = 0; i < order; i++)
            {
                coefficients[i] = phi[i + 1];
            }
            return coefficients;
        }

        /// <summary>
        /// Forecasts future values from an AR(p) model using iterated one-step predictions.
        /// </summary>
        /// <param name="data">The historical time series data.</param>
        /// <param name="order">The order of the autoregressive model.</param>
        /// <param name="steps">The number of future steps to forecast.</param>
        /// <returns>An array of forecasted values of length <paramref name="steps"/>.</returns>
        public static double[] Predict(double[] data, int order, int steps)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (order <= 0) throw new ArgumentException("Order must be positive.");
            if (steps <= 0) throw new ArgumentException("Steps must be positive.");
            if (order >= data.Length)
                throw new ArgumentException("Order must be less than data length.");

            double[] coefficients = Fit(data, order);

            int n = data.Length;
            double[] extended = new double[n + steps];
            for (int i = 0; i < n; i++)
            {
                extended[i] = data[i];
            }

            for (int i = 0; i < steps; i++)
            {
                double pred = 0.0;
                for (int j = 0; j < order; j++)
                {
                    pred += coefficients[j] * extended[n + i - 1 - j];
                }
                extended[n + i] = pred;
            }

            double[] result = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                result[i] = extended[n + i];
            }
            return result;
        }
    }
}
