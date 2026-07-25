namespace MathVerse.Math.Distributed.ExpressionExecution
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Chunked expression evaluator for processing large variable sets in parallel.
    /// </summary>
    public sealed class ChunkedExpressionEvaluator
    {
        /// <summary>
        /// Evaluates a single expression over many variable sets in chunks.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="variableSets">Array of variable sets.</param>
        /// <param name="chunkSize">Size of each parallel chunk.</param>
        /// <returns>Array of evaluation results.</returns>
        public double[] Evaluate(string expression, double[][] variableSets, int chunkSize = 1000)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
            if (variableSets == null)
                throw new ArgumentNullException(nameof(variableSets));
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

            var results = new double[variableSets.Length];
            int chunkCount = (variableSets.Length + chunkSize - 1) / chunkSize;

            Parallel.For(0, chunkCount, chunkIndex =>
            {
                int start = chunkIndex * chunkSize;
                int end = System.Math.Min(start + chunkSize, variableSets.Length);

                for (int i = start; i < end; i++)
                {
                    results[i] = EvaluateExpression(expression, variableSets[i]);
                }
            });

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
