namespace MathVerse.Math.Visualization.Integration;
using System.Collections.Generic;

/// <summary>Represents a mathematical expression for visualization.</summary>
public sealed class MathExpression
{
    /// <summary>Gets the expression string.</summary>
    public string Expression { get; init; } = "";

    /// <summary>Gets the variable name.</summary>
    public string Variable { get; init; } = "x";

    /// <summary>Gets the expression parameters.</summary>
    public Dictionary<string, double> Parameters { get; init; } = new();
}

/// <summary>Represents evaluated points from a mathematical expression.</summary>
public sealed class EvaluatedPoints
{
    /// <summary>Gets the X values.</summary>
    public List<double> XValues { get; init; } = new();

    /// <summary>Gets the Y values.</summary>
    public List<double> YValues { get; init; } = new();

    /// <summary>Gets the Z values (for 3D plots).</summary>
    public List<double>? ZValues { get; set; }
}

/// <summary>Integrates with the MathVerse kernel for expression evaluation in visualization.</summary>
public sealed class KernelIntegration
{
    /// <summary>Evaluates a mathematical expression over a range of values.</summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="variable">The variable name.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="sampleCount">The number of sample points.</param>
    /// <returns>The evaluated points.</returns>
    public static EvaluatedPoints EvaluateExpression(MathExpression expression, string variable, double min, double max, int sampleCount = 200)
    {
        var result = new EvaluatedPoints();
        double step = (max - min) / System.Math.Max(1, sampleCount - 1);

        for (int i = 0; i < sampleCount; i++)
        {
            double x = min + i * step;
            double y = EvaluateMathExpression(expression.Expression, variable, x, expression.Parameters);

            result.XValues.Add(x);
            result.YValues.Add(y);
        }

        return result;
    }

    /// <summary>Evaluates a 2D expression over a grid for surface plotting.</summary>
    /// <param name="expression">The expression to evaluate (should use variables x and y).</param>
    /// <param name="xMin">The minimum X value.</param>
    /// <param name="xMax">The maximum X value.</param>
    /// <param name="yMin">The minimum Y value.</param>
    /// <param name="yMax">The maximum Y value.</param>
    /// <param name="xSamples">The number of X samples.</param>
    /// <param name="ySamples">The number of Y samples.</param>
    /// <returns>The evaluated points with X, Y, and Z values.</returns>
    public static EvaluatedPoints EvaluateExpression2D(
        MathExpression expression, double xMin, double xMax, double yMin, double yMax,
        int xSamples = 50, int ySamples = 50)
    {
        var result = new EvaluatedPoints
        {
            ZValues = new List<double>()
        };

        double xStep = (xMax - xMin) / System.Math.Max(1, xSamples - 1);
        double yStep = (yMax - yMin) / System.Math.Max(1, ySamples - 1);

        for (int j = 0; j < ySamples; j++)
        {
            for (int i = 0; i < xSamples; i++)
            {
                double x = xMin + i * xStep;
                double y = yMin + j * yStep;
                double z = EvaluateMathExpression2D(expression.Expression, x, y, expression.Parameters);

                result.XValues.Add(x);
                result.YValues.Add(y);
                result.ZValues.Add(z);
            }
        }

        return result;
    }

    /// <summary>Converts evaluated points to a visualization line plot.</summary>
    /// <param name="points">The evaluated points.</param>
    /// <param name="color">The line color hex string.</param>
    /// <param name="lineWidth">The line width.</param>
    /// <returns>A line plot visualization object.</returns>
    public static Core.LinePlot ToLinePlot(EvaluatedPoints points, string color = "#0000FF", double lineWidth = 2.0)
    {
        var linePlot = new Core.LinePlot
        {
            Id = "expr-line-" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
            Color = color,
            LineWidth = lineWidth,
            Points = new List<System.Numerics.Vector2>()
        };

        for (int i = 0; i < points.XValues.Count; i++)
        {
            float x = (float)points.XValues[i];
            float y = (float)points.YValues[i];

            linePlot.Points.Add(new System.Numerics.Vector2(x, y));
        }

        return linePlot;
    }

    /// <summary>Creates a parametric curve from separate expressions for x, y, and z.</summary>
    /// <param name="xExpression">Expression for x(t).</param>
    /// <param name="yExpression">Expression for y(t).</param>
    /// <param name="zExpression">Expression for z(t) (optional, null for 2D).</param>
    /// <param name="tMin">Minimum parameter value.</param>
    /// <param name="tMax">Maximum parameter value.</param>
    /// <param name="sampleCount">Number of samples.</param>
    /// <returns>Evaluated points for the parametric curve.</returns>
    public static EvaluatedPoints EvaluateParametric(
        string xExpression, string yExpression, string? zExpression,
        double tMin, double tMax, int sampleCount = 200)
    {
        var result = new EvaluatedPoints();

        if (zExpression != null)
            result.ZValues = new List<double>();

        double step = (tMax - tMin) / System.Math.Max(1, sampleCount - 1);

        for (int i = 0; i < sampleCount; i++)
        {
            double t = tMin + i * step;
            var tParams = new Dictionary<string, double> { ["t"] = t };

            double x = EvaluateMathExpression(xExpression, "t", t, tParams);
            double y = EvaluateMathExpression(yExpression, "t", t, tParams);

            result.XValues.Add(x);
            result.YValues.Add(y);

            if (zExpression != null && result.ZValues != null)
            {
                double z = EvaluateMathExpression(zExpression, "t", t, tParams);
                result.ZValues.Add(z);
            }
        }

        return result;
    }

    private static double EvaluateMathExpression(string expression, string variable, double value, Dictionary<string, double> parameters)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return 0;

        string processed = expression;
        processed = processed.Replace(variable, $"({value:F6})");

        foreach (var kvp in parameters)
        {
            processed = processed.Replace(kvp.Key, $"({kvp.Value:F6})");
        }

        processed = processed.Replace("pi", System.Math.PI.ToString("F6"));
        processed = processed.Replace("e", System.Math.E.ToString("F6"));

        return EvaluateSimpleExpression(processed);
    }

    private static double EvaluateMathExpression2D(string expression, double x, double y, Dictionary<string, double> parameters)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return 0;

        string processed = expression;
        processed = processed.Replace("x", $"({x:F6})");
        processed = processed.Replace("y", $"({y:F6})");

        foreach (var kvp in parameters)
        {
            processed = processed.Replace(kvp.Key, $"({kvp.Value:F6})");
        }

        processed = processed.Replace("pi", System.Math.PI.ToString("F6"));

        return EvaluateSimpleExpression(processed);
    }

    private static double EvaluateSimpleExpression(string expression)
    {
        expression = expression.Trim();

        if (double.TryParse(expression, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        expression = ProcessFunction(expression, "sin", x => System.Math.Sin(x));
        expression = ProcessFunction(expression, "cos", x => System.Math.Cos(x));
        expression = ProcessFunction(expression, "tan", x => System.Math.Tan(x));
        expression = ProcessFunction(expression, "asin", x => System.Math.Asin(x));
        expression = ProcessFunction(expression, "acos", x => System.Math.Acos(x));
        expression = ProcessFunction(expression, "atan", x => System.Math.Atan(x));
        expression = ProcessFunction(expression, "sinh", x => System.Math.Sinh(x));
        expression = ProcessFunction(expression, "cosh", x => System.Math.Cosh(x));
        expression = ProcessFunction(expression, "tanh", x => System.Math.Tanh(x));
        expression = ProcessFunction(expression, "sqrt", x => System.Math.Sqrt(x));
        expression = ProcessFunction(expression, "abs", x => System.Math.Abs(x));
        expression = ProcessFunction(expression, "log", x => System.Math.Log(x));
        expression = ProcessFunction(expression, "log10", x => System.Math.Log10(x));
        expression = ProcessFunction(expression, "exp", x => System.Math.Exp(x));
        expression = ProcessFunction(expression, "floor", x => System.Math.Floor(x));
        expression = ProcessFunction(expression, "ceil", x => System.Math.Ceiling(x));
        expression = ProcessFunction(expression, "round", x => System.Math.Round(x));

        expression = expression.Replace("^", "**");

        if (double.TryParse(expression, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return 0;
    }

    private static string ProcessFunction(string expression, string funcName, System.Func<double, double> func)
    {
        string search = funcName + "(";
        int startIndex;

        while ((startIndex = expression.IndexOf(search, System.StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            int parenDepth = 0;
            int endIndex = startIndex + search.Length;

            while (endIndex < expression.Length)
            {
                if (expression[endIndex] == '(')
                    parenDepth++;
                else if (expression[endIndex] == ')')
                {
                    if (parenDepth == 0)
                        break;
                    parenDepth--;
                }
                endIndex++;
            }

            if (endIndex >= expression.Length)
                break;

            string inner = expression.Substring(startIndex + search.Length, endIndex - startIndex - search.Length);

            if (double.TryParse(inner, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double innerValue))
            {
                double funcResult = func(innerValue);
                expression = expression.Substring(0, startIndex) +
                    funcResult.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) +
                    expression.Substring(endIndex + 1);
            }
            else
            {
                break;
            }
        }

        return expression;
    }
}
