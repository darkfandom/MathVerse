namespace MathVerse.Math.Distributed.ExpressionExecution
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Computer Algebra System (CAS) expression simplifier.
    /// </summary>
    public sealed class ParallelCASRewriter
    {
        /// <summary>
        /// Simplifies multiple expressions in parallel.
        /// </summary>
        /// <param name="expressions">Expressions to simplify.</param>
        /// <param name="maxIterations">Maximum simplification iterations.</param>
        /// <returns>Simplified expressions.</returns>
        public string[] SimplifyBatch(string[] expressions, int maxIterations = 10)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));
            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");

            var results = new string[expressions.Length];

            Parallel.For(0, expressions.Length, i =>
            {
                results[i] = SimplifyExpression(expressions[i], maxIterations);
            });

            return results;
        }

        private static string SimplifyExpression(string expression, int maxIterations)
        {
            string current = expression;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                string previous = current;
                current = RemoveDoubleNegatives(current);
                current = SimplifyZeroOperations(current);
                current = SimplifyOneOperations(current);
                current = SimplifyConstantFolding(current);
                current = RemoveRedundantParentheses(current);

                if (current == previous)
                    break;
            }
            return current;
        }

        private static string RemoveDoubleNegatives(string expr)
        {
            return expr.Replace("--", "+").Replace("+-", "-").Replace("-+", "-");
        }

        private static string SimplifyZeroOperations(string expr)
        {
            expr = Regex.Replace(expr, @"(\d+\.?\d*)\s*\*\s*0", "0");
            expr = Regex.Replace(expr, @"0\s*\*\s*(\d+\.?\d*)", "0");
            expr = Regex.Replace(expr, @"(\d+\.?\d*)\s*\+\s*0", "$1");
            expr = Regex.Replace(expr, @"0\s*\+\s*(\d+\.?\d*)", "$1");
            expr = Regex.Replace(expr, @"(\d+\.?\d*)\s*\-\s*0", "$1");
            return expr;
        }

        private static string SimplifyOneOperations(string expr)
        {
            expr = Regex.Replace(expr, @"(\d+\.?\d*)\s*\*\s*1", "$1");
            expr = Regex.Replace(expr, @"1\s*\*\s*(\d+\.?\d*)", "$1");
            return expr;
        }

        private static string SimplifyConstantFolding(string expr)
        {
            var match = Regex.Match(expr, @"(\d+\.?\d*)\s*([\+\-\*\/])\s*(\d+\.?\d*)");
            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double left) &&
                    double.TryParse(match.Groups[3].Value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double right))
                {
                    double result = match.Groups[2].Value switch
                    {
                        "+" => left + right,
                        "-" => left - right,
                        "*" => left * right,
                        "/" => right != 0 ? left / right : double.NaN,
                        _ => double.NaN
                    };

                    if (!double.IsNaN(result) && !double.IsInfinity(result))
                    {
                        expr = expr.Substring(0, match.Index) +
                               result.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                               expr.Substring(match.Index + match.Length);
                    }
                }
            }
            return expr;
        }

        private static string RemoveRedundantParentheses(string expr)
        {
            return expr.Replace("( ", "(").Replace(" )", ")");
        }
    }
}
