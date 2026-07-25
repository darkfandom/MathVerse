namespace MathVerse.Math.DataScience.CASIntegration;

using System;
using System.Collections.Generic;
using MathVerse.Math.DataScience.Core;

/// <summary>
/// Provides a simple expression parser and evaluator for mathematical formulas.
/// Supports +, -, *, /, ^ (power), sin, cos, tan, log, exp, sqrt, abs, and parentheses.
/// </summary>
public sealed class FormulaEvaluator
{
    /// <summary>
    /// Evaluates a mathematical expression with the given variable bindings.
    /// </summary>
    /// <param name="expression">The expression string (e.g., "2*x + sin(y)").</param>
    /// <param name="variables">Dictionary mapping variable names to values.</param>
    /// <returns>The evaluated result.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is invalid.</exception>
    public double Evaluate(string expression, Dictionary<string, double> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
        if (variables is null) throw new ArgumentNullException(nameof(variables));

        string[] tokens = FormulaTokenizer.Tokenize(expression);
        int pos = 0;
        double result = ParseExpression(tokens, ref pos, variables);
        return result;
    }

    /// <summary>
    /// Evaluates an expression with no variables.
    /// </summary>
    /// <param name="expression">The expression string.</param>
    /// <returns>The evaluated result.</returns>
    public double EvaluateConstant(string expression)
    {
        return Evaluate(expression, new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase));
    }

    private double ParseExpression(string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        double left = ParseTerm(tokens, ref pos, variables);

        while (pos < tokens.Length && (tokens[pos] == "+" || tokens[pos] == "-"))
        {
            string op = tokens[pos];
            pos++;
            double right = ParseTerm(tokens, ref pos, variables);
            left = op == "+" ? left + right : left - right;
        }

        return left;
    }

    private double ParseTerm(string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        double left = ParsePower(tokens, ref pos, variables);

        while (pos < tokens.Length && (tokens[pos] == "*" || tokens[pos] == "/"))
        {
            string op = tokens[pos];
            pos++;
            double right = ParsePower(tokens, ref pos, variables);
            left = op == "*" ? left * right : left / right;
        }

        return left;
    }

    private double ParsePower(string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        double baseVal = ParseUnary(tokens, ref pos, variables);

        if (pos < tokens.Length && tokens[pos] == "^")
        {
            pos++;
            double exp = ParsePower(tokens, ref pos, variables);
            return System.Math.Pow(baseVal, exp);
        }

        return baseVal;
    }

    private double ParseUnary(string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        if (pos < tokens.Length && tokens[pos] == "-")
        {
            pos++;
            double val = ParseAtom(tokens, ref pos, variables);
            return -val;
        }

        if (pos < tokens.Length && tokens[pos] == "+")
        {
            pos++;
            return ParseAtom(tokens, ref pos, variables);
        }

        return ParseAtom(tokens, ref pos, variables);
    }

    private double ParseAtom(string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        if (pos >= tokens.Length)
            throw new ArgumentException("Unexpected end of expression.");

        string token = tokens[pos];

        // Parenthesized expression
        if (token == "(")
        {
            pos++;
            double result = ParseExpression(tokens, ref pos, variables);
            if (pos < tokens.Length && tokens[pos] == ")")
                pos++;
            return result;
        }

        // Number literal
        if (double.TryParse(token, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double numVal))
        {
            pos++;
            return numVal;
        }

        // Function call or variable
        string name = token.ToLowerInvariant();
        if (IsFunction(name))
        {
            pos++;
            return EvaluateFunction(name, tokens, ref pos, variables);
        }

        // Variable lookup
        if (variables.TryGetValue(token, out double varVal))
        {
            pos++;
            return varVal;
        }

        throw new ArgumentException($"Unknown token: '{token}'.");
    }

    private double EvaluateFunction(string name, string[] tokens, ref int pos, Dictionary<string, double> variables)
    {
        if (pos >= tokens.Length || tokens[pos] != "(")
            throw new ArgumentException($"Expected '(' after function '{name}'.");

        pos++;
        double arg = ParseExpression(tokens, ref pos, variables);

        if (pos < tokens.Length && tokens[pos] == ")")
            pos++;

        return name switch
        {
            "sin" => System.Math.Sin(arg),
            "cos" => System.Math.Cos(arg),
            "tan" => System.Math.Tan(arg),
            "asin" => System.Math.Asin(arg),
            "acos" => System.Math.Acos(arg),
            "atan" => System.Math.Atan(arg),
            "sinh" => System.Math.Sinh(arg),
            "cosh" => System.Math.Cosh(arg),
            "tanh" => System.Math.Tanh(arg),
            "log" => System.Math.Log10(arg),
            "ln" => System.Math.Log(arg),
            "log2" => System.Math.Log2(arg),
            "exp" => System.Math.Exp(arg),
            "sqrt" => System.Math.Sqrt(arg),
            "abs" => System.Math.Abs(arg),
            "ceil" => System.Math.Ceiling(arg),
            "floor" => System.Math.Floor(arg),
            "round" => System.Math.Round(arg),
            _ => throw new ArgumentException($"Unknown function: '{name}'.")
        };
    }

    private bool IsFunction(string name)
    {
        return name is "sin" or "cos" or "tan" or "asin" or "acos" or "atan"
            or "sinh" or "cosh" or "tanh"
            or "log" or "ln" or "log2" or "exp" or "sqrt" or "abs"
            or "ceil" or "floor" or "round";
    }
}
