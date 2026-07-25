namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Predicts mathematical formulas from data points using symbolic regression via genetic programming.</summary>
public sealed class FormulaPredictor
{
    private readonly Random _rng;

    /// <summary>Initializes a new formula predictor.</summary>
    /// <param name="seed">Random seed for reproducibility. Use -1 for non-deterministic.</param>
    public FormulaPredictor(int seed = -1)
    {
        _rng = seed >= 0 ? new Random(seed) : new Random();
    }

    /// <summary>Predicts a mathematical formula that fits the given data points.</summary>
    /// <param name="points">Data points where each row is [x1, x2, ..., y] (last column is target).</param>
    /// <param name="maxTerms">Maximum number of terms in the formula.</param>
    /// <returns>The best formula string found.</returns>
    public string PredictFormula(double[][] points, int maxTerms = 5)
    {
        List<FormulaCandidate> ranked = RankedFormulas(points, 1, maxTerms);
        return ranked.Count > 0 ? ranked[0].Formula : "0";
    }

    /// <summary>Returns the top-k formula candidates ranked by fitness.</summary>
    /// <param name="points">Data points where each row is [x1, x2, ..., y] (last column is target).</param>
    /// <param name="topK">Number of top candidates to return.</param>
    /// <param name="maxTerms">Maximum number of terms in formulas.</param>
    /// <returns>Ranked list of formula candidates.</returns>
    public List<FormulaCandidate> RankedFormulas(double[][] points, int topK = 10, int maxTerms = 5)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException("Points cannot be null or empty.", nameof(points));
        if (topK <= 0)
            throw new ArgumentException("Top K must be positive.", nameof(topK));

        int populationSize = System.Math.Max(topK * 10, 100);
        int numGenerations = 50;
        double mutationRate = 0.15;
        double crossoverRate = 0.7;

        List<string> population = InitializePopulation(populationSize, maxTerms);

        for (int gen = 0; gen < numGenerations; gen++)
        {
            List<FormulaCandidate> evaluated = new();
            foreach (string formula in population)
            {
                double fitness = EvaluateFitness(formula, points);
                double complexity = ComputeComplexity(formula);
                evaluated.Add(new FormulaCandidate
                {
                    Formula = formula,
                    Fitness = fitness,
                    Complexity = complexity
                });
            }

            evaluated.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

            List<string> newPopulation = new();
            int eliteCount = System.Math.Max(2, populationSize / 10);
            for (int i = 0; i < eliteCount && i < evaluated.Count; i++)
                newPopulation.Add(evaluated[i].Formula);

            while (newPopulation.Count < populationSize)
            {
                if (_rng.NextDouble() < crossoverRate)
                {
                    string parent1 = TournamentSelect(evaluated);
                    string parent2 = TournamentSelect(evaluated);
                    string child = Crossover(parent1, parent2, maxTerms);
                    newPopulation.Add(child);
                }
                else
                {
                    string parent = TournamentSelect(evaluated);
                    newPopulation.Add(Mutate(parent, maxTerms));
                }
            }

            for (int i = 0; i < newPopulation.Count; i++)
            {
                if (_rng.NextDouble() < mutationRate)
                    newPopulation[i] = Mutate(newPopulation[i], maxTerms);
            }

            population = newPopulation;
        }

        List<FormulaCandidate> finalEvaluated = new();
        HashSet<string> seen = new();
        foreach (string formula in population)
        {
            if (seen.Contains(formula))
                continue;
            seen.Add(formula);

            double fitness = EvaluateFitness(formula, points);
            double complexity = ComputeComplexity(formula);
            finalEvaluated.Add(new FormulaCandidate
            {
                Formula = formula,
                Fitness = fitness,
                Complexity = complexity
            });
        }

        finalEvaluated.Sort((a, b) =>
        {
            double scoreA = a.Fitness - 0.01 * a.Complexity;
            double scoreB = b.Fitness - 0.01 * b.Complexity;
            return scoreB.CompareTo(scoreA);
        });

        return finalEvaluated.Take(topK).ToList();
    }

    /// <summary>Evaluates a formula at given variable values.</summary>
    /// <param name="formula">The formula string to evaluate.</param>
    /// <param name="variables">Variable values (in order x, y, z, ...).</param>
    /// <returns>Evaluated result.</returns>
    public double EvaluateFormula(string formula, double[] variables)
    {
        if (string.IsNullOrEmpty(formula))
            throw new ArgumentException("Formula cannot be null or empty.", nameof(formula));
        if (variables == null)
            throw new ArgumentNullException(nameof(variables));

        return EvalExpression(formula, variables);
    }

    private double EvaluateFitness(string formula, double[][] points)
    {
        double totalError = 0.0;
        int numVars = points[0].Length - 1;

        foreach (double[] point in points)
        {
            double[] vars = new double[numVars];
            Array.Copy(point, vars, numVars);
            double target = point[numVars];

            double predicted;
            try
            {
                predicted = EvalExpression(formula, vars);
            }
            catch
            {
                return 0.0;
            }

            if (double.IsNaN(predicted) || double.IsInfinity(predicted))
                return 0.0;

            double error = predicted - target;
            totalError += error * error;
        }

        double mse = totalError / points.Length;
        double rmse = System.Math.Sqrt(mse);
        double maxAbs = 0.0;
        foreach (double[] point in points)
        {
            double abs = System.Math.Abs(point[numVars]);
            if (abs > maxAbs) maxAbs = abs;
        }

        if (maxAbs < 1e-10)
            maxAbs = 1.0;

        return System.Math.Exp(-rmse / maxAbs);
    }

    private static double ComputeComplexity(string formula)
    {
        int ops = 0;
        foreach (char c in formula)
        {
            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                ops++;
            else if (char.IsLetter(c))
            {
                if (formula.Contains("sin(") || formula.Contains("cos(") || formula.Contains("exp(") || formula.Contains("log("))
                    ops += 3;
            }
        }
        return ops + formula.Length * 0.1;
    }

    private string TournamentSelect(List<FormulaCandidate> population)
    {
        int tournamentSize = System.Math.Min(5, population.Count);
        FormulaCandidate best = population[_rng.Next(population.Count)];
        for (int i = 1; i < tournamentSize; i++)
        {
            FormulaCandidate contender = population[_rng.Next(population.Count)];
            if (contender.Fitness > best.Fitness)
                best = contender;
        }
        return best.Formula;
    }

    private string Crossover(string parent1, string parent2, int maxTerms)
    {
        string[] terms1 = SplitTerms(parent1);
        string[] terms2 = SplitTerms(parent2);

        List<string> childTerms = new();
        int numTerms = System.Math.Min(maxTerms, System.Math.Max(terms1.Length, terms2.Length));

        for (int i = 0; i < numTerms; i++)
        {
            if (_rng.NextDouble() < 0.5 && i < terms1.Length)
                childTerms.Add(terms1[i]);
            else if (i < terms2.Length)
                childTerms.Add(terms2[i]);
        }

        if (childTerms.Count == 0)
            childTerms.Add("x");

        return CombineTerms(childTerms);
    }

    private string Mutate(string formula, int maxTerms)
    {
        int mutationType = _rng.Next(4);

        return mutationType switch
        {
            0 => MutateOperator(formula),
            1 => MutateConstant(formula),
            2 => AddTerm(formula, maxTerms),
            3 => RemoveTerm(formula),
            _ => formula
        };
    }

    private string MutateOperator(string formula)
    {
        char[] chars = formula.ToCharArray();
        string ops = "+-*/^";
        for (int i = 0; i < chars.Length; i++)
        {
            if (ops.Contains(chars[i]))
            {
                chars[i] = ops[_rng.Next(ops.Length)];
                break;
            }
        }
        return new string(chars);
    }

    private string MutateConstant(string formula)
    {
        string[] parts = formula.Split(new[] { '+', '-', '*', '/', '^', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return formula;

        string target = parts[_rng.Next(parts.Length)];
        if (double.TryParse(target, out _))
        {
            double newVal = _rng.NextDouble() * 10.0 - 5.0;
            string newStr = newVal.ToString("F2");
            return formula.Replace(target, newStr, StringComparison.Ordinal);
        }
        return formula;
    }

    private string AddTerm(string formula, int maxTerms)
    {
        string[] terms = SplitTerms(formula);
        if (terms.Length >= maxTerms)
            return formula;

        string[] newTerms = ["x", "x^2", "x^3", "sin(x)", "cos(x)", "exp(x)", "log(x+1)"];
        string newTerm = newTerms[_rng.Next(newTerms.Length)];
        double coeff = _rng.NextDouble() * 4.0 - 2.0;

        return formula + (coeff >= 0 ? " + " : " - ") + System.Math.Abs(coeff).ToString("F2") + "*" + newTerm;
    }

    private string RemoveTerm(string formula)
    {
        string[] terms = SplitTerms(formula);
        if (terms.Length <= 1)
            return formula;

        int removeIdx = _rng.Next(terms.Length);
        List<string> remaining = new();
        for (int i = 0; i < terms.Length; i++)
        {
            if (i != removeIdx)
                remaining.Add(terms[i]);
        }

        return CombineTerms(remaining);
    }

    private static string[] SplitTerms(string formula)
    {
        List<string> terms = new();
        string current = "";
        foreach (char c in formula)
        {
            if ((c == '+' || c == '-') && current.Length > 0)
            {
                terms.Add(current.Trim());
                current = c.ToString();
            }
            else
            {
                current += c;
            }
        }
        if (current.Length > 0)
            terms.Add(current.Trim());
        return terms.Where(t => t.Length > 0).ToArray();
    }

    private static string CombineTerms(List<string> terms)
    {
        if (terms.Count == 0)
            return "0";

        string result = terms[0];
        for (int i = 1; i < terms.Count; i++)
        {
            string term = terms[i].TrimStart();
            if (term.StartsWith('-'))
                result += " " + term;
            else
                result += " + " + term;
        }
        return result;
    }

    private static double EvalExpression(string expr, double[] vars)
    {
        string normalized = expr.Replace(" ", "");
        return ParseAddSub(normalized, ref normalized, vars);
    }

    private static double ParseAddSub(string expr, ref string remaining, double[] vars)
    {
        double result = ParseMulDiv(expr, ref remaining, vars);

        while (remaining.Length > 0 && (remaining[0] == '+' || remaining[0] == '-'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            double right = ParseMulDiv(expr, ref remaining, vars);
            result = op == '+' ? result + right : result - right;
        }

        return result;
    }

    private static double ParseMulDiv(string expr, ref string remaining, double[] vars)
    {
        double result = ParseUnary(expr, ref remaining, vars);

        while (remaining.Length > 0 && (remaining[0] == '*' || remaining[0] == '/'))
        {
            char op = remaining[0];
            remaining = remaining[1..];
            double right = ParseUnary(expr, ref remaining, vars);
            result = op == '*' ? result * right : result / right;
        }

        return result;
    }

    private static double ParseUnary(string expr, ref string remaining, double[] vars)
    {
        if (remaining.Length > 0 && remaining[0] == '-')
        {
            remaining = remaining[1..];
            return -ParsePower(expr, ref remaining, vars);
        }
        if (remaining.Length > 0 && remaining[0] == '+')
        {
            remaining = remaining[1..];
        }
        return ParsePower(expr, ref remaining, vars);
    }

    private static double ParsePower(string expr, ref string remaining, double[] vars)
    {
        double result = ParseAtom(expr, ref remaining, vars);

        if (remaining.Length > 0 && remaining[0] == '^')
        {
            remaining = remaining[1..];
            double exp = ParseUnary(expr, ref remaining, vars);
            result = System.Math.Pow(result, exp);
        }

        return result;
    }

    private static double ParseAtom(string expr, ref string remaining, double[] vars)
    {
        if (remaining.Length == 0)
            return 0.0;

        if (remaining[0] == '(')
        {
            remaining = remaining[1..];
            double result = ParseAddSub(expr, ref remaining, vars);
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return result;
        }

        if (remaining.StartsWith("sin("))
        {
            remaining = remaining[4..];
            double arg = ParseAddSub(expr, ref remaining, vars);
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return System.Math.Sin(arg);
        }

        if (remaining.StartsWith("cos("))
        {
            remaining = remaining[4..];
            double arg = ParseAddSub(expr, ref remaining, vars);
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return System.Math.Cos(arg);
        }

        if (remaining.StartsWith("exp("))
        {
            remaining = remaining[4..];
            double arg = ParseAddSub(expr, ref remaining, vars);
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return System.Math.Exp(arg);
        }

        if (remaining.StartsWith("log("))
        {
            remaining = remaining[4..];
            double arg = ParseAddSub(expr, ref remaining, vars);
            if (remaining.Length > 0 && remaining[0] == ')')
                remaining = remaining[1..];
            return System.Math.Log(arg);
        }

        if (remaining[0] >= '0' && remaining[0] <= '9' || remaining[0] == '.')
        {
            int start = 0;
            while (remaining.Length > start && (remaining[start] >= '0' && remaining[start] <= '9' || remaining[start] == '.'))
                start++;
            double val = double.Parse(remaining[..start], System.Globalization.CultureInfo.InvariantCulture);
            remaining = remaining[start..];
            return val;
        }

        if (remaining[0] == 'x' || remaining[0] == 'y' || remaining[0] == 'z')
        {
            int varIdx = remaining[0] switch
            {
                'x' => 0,
                'y' => 1,
                'z' => 2,
                _ => 0
            };
            remaining = remaining[1..];
            return varIdx < vars.Length ? vars[varIdx] : 0.0;
        }

        remaining = remaining[1..];
        return 0.0;
    }

    private List<string> InitializePopulation(int size, int maxTerms)
    {
        string[] bases = ["x", "x^2", "x^3", "sin(x)", "cos(x)", "exp(x)", "log(x+1)", "x*x", "1"];
        List<string> population = new();

        for (int i = 0; i < size; i++)
        {
            int numTerms = _rng.Next(1, maxTerms + 1);
            List<string> terms = new();
            for (int t = 0; t < numTerms; t++)
            {
                double coeff = _rng.NextDouble() * 6.0 - 3.0;
                string basis = bases[_rng.Next(bases.Length)];
                string term = coeff.ToString("F2") + "*" + basis;
                terms.Add(term);
            }
            population.Add(CombineTerms(terms));
        }

        return population;
    }
}

/// <summary>Represents a candidate formula with fitness and complexity metrics.</summary>
public sealed class FormulaCandidate
{
    /// <summary>Gets the formula string.</summary>
    public string Formula { get; init; } = "";

    /// <summary>Gets the fitness score (higher is better).</summary>
    public double Fitness { get; init; }

    /// <summary>Gets the complexity score (lower is simpler).</summary>
    public double Complexity { get; init; }
}
