namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel adaptive Simpson integration.
    /// </summary>
    public sealed class ParallelIntegration
    {
        /// <summary>
        /// Integrates a function using adaptive Simpson's rule with parallel subdivision.
        /// </summary>
        /// <param name="func">Function to integrate.</param>
        /// <param name="a">Lower bound.</param>
        /// <param name="b">Upper bound.</param>
        /// <param name="tolerance">Error tolerance.</param>
        /// <returns>Approximate integral value.</returns>
        public double Integrate(Func<double, double> func, double a, double b, double tolerance = 1e-8)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            double result = AdaptiveSimpson(func, a, b, tolerance, 0, 20);
            return result;
        }

        private double AdaptiveSimpson(Func<double, double> func, double a, double b, double tol, int depth, int maxDepth)
        {
            double c = (a + b) / 2;
            double fa = func(a);
            double fb = func(b);
            double fc = func(c);

            double whole = SimpsonRule(fa, fc, fb, a, b);

            double d = (a + c) / 2;
            double e = (c + b) / 2;
            double fd = func(d);
            double fe = func(e);

            double left = SimpsonRule(fa, fd, fc, a, c);
            double right = SimpsonRule(fc, fe, fb, c, b);
            double half = left + right;

            double error = System.Math.Abs(half - whole) / 15;

            if (error < tol || depth >= maxDepth)
                return half + (half - whole) / 15;

            if (depth < 3)
            {
                var leftTask = Task.Run(() => AdaptiveSimpson(func, a, c, tol / 2, depth + 1, maxDepth));
                var rightTask = Task.Run(() => AdaptiveSimpson(func, c, b, tol / 2, depth + 1, maxDepth));
                Task.WaitAll(leftTask, rightTask);
                return leftTask.Result + rightTask.Result;
            }

            return AdaptiveSimpson(func, a, c, tol / 2, depth + 1, maxDepth) +
                   AdaptiveSimpson(func, c, b, tol / 2, depth + 1, maxDepth);
        }

        private static double SimpsonRule(double fa, double fm, double fb, double a, double b)
        {
            double h = (b - a) / 6;
            return h * (fa + 4 * fm + fb);
        }
    }
}
