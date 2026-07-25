namespace MathVerse.Math.CAS.PatternMatching;

using MathVerse.Math.Expressions;
using System.Collections.Immutable;

public static class PatternCompiler
{
    public static Pattern Compile(string patternString)
    {
        if (string.IsNullOrWhiteSpace(patternString))
            throw new ArgumentException("Pattern string cannot be empty", nameof(patternString));

        return ParsePattern(patternString);
    }

    private static Pattern ParsePattern(string pattern)
    {
        pattern = pattern.Trim();

        if (pattern.StartsWith("_"))
            return ParseVariablePattern(pattern[1..]);

        if (pattern.StartsWith("?"))
            return Pattern.Wildcard;

        if (pattern.StartsWith("(") && pattern.EndsWith(")"))
            return ParseSequencePattern(pattern[1..^1]);

        return Pattern.Structural(ParseExpression(pattern));
    }

    private static Pattern ParseVariablePattern(string name)
    {
        if (name.Contains(':'))
        {
            var parts = name.Split(':', 2);
            var varName = parts[0];
            var typeName = parts[1];

            Type? type = typeName switch
            {
                "num" or "number" or "double" => typeof(LiteralExpression),
                "var" or "variable" => typeof(VariableExpression),
                "func" or "function" => typeof(FunctionCallExpression),
                "expr" or "expression" => typeof(Expression),
                _ => null
            };

            return Pattern.Variable(varName, type);
        }

        return Pattern.Variable(name);
    }

    private static Pattern ParseSequencePattern(string pattern)
    {
        var elements = SplitTopLevel(pattern, ',').Select(ParsePattern).ToArray();
        return Pattern.Sequence(elements);
    }

    private static Expression ParseExpression(string expr)
    {
        expr = expr.Trim();

        if (double.TryParse(expr, out var val))
            return Expr.Literal(val);

        if (expr.StartsWith("_") || expr.StartsWith("?"))
            return new VariableExpression(expr);

        var parts = SplitTopLevel(expr, '+', '-', '*', '/', '^');
        if (parts.Length > 1)
            return BuildBinaryExpression(parts);

        if (expr.Contains('(') && expr.EndsWith(")"))
        {
            var funcName = expr[..expr.IndexOf('(')];
            var argsStr = expr[(expr.IndexOf('(') + 1)..^1];
            var args = SplitTopLevel(argsStr, ',').Select(ParseExpression).ToArray();
            return Expr.Call(funcName, args);
        }

        return new VariableExpression(expr);
    }

    private static string[] SplitTopLevel(string str, params char[] separators)
    {
        var result = new System.Collections.Generic.List<string>();
        var current = new System.Text.StringBuilder();
        int depth = 0;

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c == '(' || c == '[' || c == '{')
                depth++;
            else if (c == ')' || c == ']' || c == '}')
                depth--;

            if (depth == 0 && separators.Contains(c))
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private static Expression BuildBinaryExpression(string[] parts)
    {
        var left = ParseExpression(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            var op = DetermineOperator(parts[i - 1]);
            var right = ParseExpression(parts[i]);
            left = new BinaryExpression(op, left, right);
        }
        return left;
    }

    private static MathOperator DetermineOperator(string prevPart)
    {
        if (prevPart.EndsWith("+")) return MathOperator.Add;
        if (prevPart.EndsWith("-")) return MathOperator.Subtract;
        if (prevPart.EndsWith("*")) return MathOperator.Multiply;
        if (prevPart.EndsWith("/")) return MathOperator.Divide;
        if (prevPart.EndsWith("^")) return MathOperator.Power;
        return MathOperator.Add;
    }
}