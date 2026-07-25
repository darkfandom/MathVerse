namespace MathVerse.Math.Distributed.ExpressionExecution
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel expression evaluator over datasets.
    /// </summary>
    public sealed class ParallelExpressionEvaluator
    {
        /// <summary>
        /// Evaluates multiple expressions in parallel over variable sets.
        /// </summary>
        /// <param name="expressions">Array of expressions to evaluate.</param>
        /// <param name="variableSets">Array of variable sets for each expression.</param>
        /// <returns>2D array of results [expression][variableSet].</returns>
        public double[][] EvaluateBatch(string[] expressions, double[][] variableSets)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));
            if (variableSets == null)
                throw new ArgumentNullException(nameof(variableSets));

            var results = new double[expressions.Length][];

            Parallel.For(0, expressions.Length, i =>
            {
                results[i] = EvaluateSingle(expressions[i], variableSets);
            });

            return results;
        }

        private static double[] EvaluateSingle(string expression, double[][] variableSets)
        {
            var results = new double[variableSets.Length];
            for (int j = 0; j < variableSets.Length; j++)
            {
                results[j] = EvaluateExpression(expression, variableSets[j]);
            }
            return results;
        }

        private static double EvaluateExpression(string expression, double[] variables)
        {
            if (variables.Length == 0)
                return 0;

            double result = 0;
            double currentNumber = 0;
            bool hasNumber = false;
            char lastOp = '+';

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber = currentNumber * 10 + (c - '0');
                    hasNumber = true;
                }
                else if (c == 'x' && i + 1 < expression.Length && char.IsDigit(expression[i + 1]))
                {
                    int varIndex = expression[i + 1] - '0';
                    if (varIndex < variables.Length)
                    {
                        currentNumber = variables[varIndex];
                        hasNumber = true;
                        i++;
                    }
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    if (hasNumber)
                    {
                        result = ApplyOperation(result, lastOp, currentNumber);
                        currentNumber = 0;
                        hasNumber = false;
                    }
                    lastOp = c;
                }
            }

            if (hasNumber)
                result = ApplyOperation(result, lastOp, currentNumber);

            return result;
        }

        private static double ApplyOperation(double left, char op, double right)
        {
            return op switch
            {
                '+' => left + right,
                '-' => left - right,
                '*' => left * right,
                '/' => right != 0 ? left / right : double.NaN,
                _ => right
            };
        }
    }
}
