using System;
using System.Collections.Immutable;
using MathVerse.Math.Calculus;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.CAS.Simplification;
using MathVerse.Math.Parsing;

namespace CasValidation;

class Program
{
    static int _pass = 0, _fail = 0;

    static void Main()
    {
        Console.WriteLine("=== MathVerse CAS Validation Suite ===\n");

        TestEvalNumeric("1 + 1", 2);
        TestEvalNumeric("2 * 3 + 5", 11);
        TestEvalNumeric("sin(pi / 2)", 1);
        TestEvalNumeric("cos(0)", 1);
        TestEvalSymbolic("(x+1)^2", "(x+1)^2");

        TestSimplify("sin(x)^2 + cos(x)^2", "1");
        TestSimplify("x + 0", "x");
        TestSimplify("x * 1", "x");

        TestDiffNumeric("sin(x)", "cos(x)", 0, 1);       // d/dx sin(0) = cos(0) = 1
        TestDiffNumeric("x^2", null, 0, 0);               // d/dx x^2 at 0 = 0
        TestDiffNumeric("x^2", null, 3, 6);               // d/dx x^2 at 3 = 6
        TestDiffNumeric("x^3", null, 2, 12);              // d/dx x^3 at 2 = 12
        TestDiffNumeric("cos(x)", "(-sin(x))", 0, 0);     // d/dx cos(0) = -sin(0) = 0
        TestDiffNumeric("cos(x)", null, Math.PI/2, -1);    // d/dx cos(pi/2) = -1
        TestDiffNumeric("e^x", null, 0, 1);               // d/dx e^x at 0 = 1
        TestDiffNumeric("e^x", null, 1, Math.E);           // d/dx e^x at 1 = e
        TestDiffNumeric("ln(x)", null, 1, 1);             // d/dx ln(1) = 1

        TestIntegNumeric("2 * x", 0, 2, 4);                  // ∫2x dx from 0 to 2 = 4
        TestIntegNumeric("sin(x)", 0, Math.PI, 2);        // ∫sin(x) from 0 to pi = 2
        TestIntegNumeric("cos(x)", 0, Math.PI/2, 1);      // ∫cos(x) from 0 to pi/2 = 1
        TestIntegNumeric("x^3", 0, 2, 4);                 // ∫x^3 from 0 to 2 = 4

        TestPlot("sin(x)", "sin wave", -1.0, 1.0);
        TestPlot("x^2", "parabola", 0.0, 100.0);
        TestPlot("cos(x)", "cosine", -1.0, 1.0);

        Console.WriteLine($"\n=== RESULTS: {_pass} passed, {_fail} failed ===");
    }

    static void TestEvalNumeric(string input, double expected)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var result = Evaluator.Instance.Evaluate(expr);
            var actual = result.Result is MathVerse.Math.Expressions.LiteralExpression lit ? lit.Value : double.NaN;
            bool ok = Math.Abs(actual - expected) < 1e-10;
            if (ok) { Console.WriteLine($"  PASS EVAL: {input} = {actual}"); _pass++; }
            else { Console.WriteLine($"  FAIL EVAL: {input} = {actual} (expected {expected})"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR EVAL: {input} -> {ex.Message}"); _fail++; }
    }

    static void TestEvalSymbolic(string input, string expected)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var result = Evaluator.Instance.Evaluate(expr);
            var resultStr = result.Result.ToString();
            bool ok = resultStr.Replace(" ", "").Contains(expected.Replace(" ", ""));
            if (ok) { Console.WriteLine($"  PASS EVAL: {input} = {resultStr}"); _pass++; }
            else { Console.WriteLine($"  FAIL EVAL: {input} = {resultStr} (expected containing {expected})"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR EVAL: {input} -> {ex.Message}"); _fail++; }
    }

    static void TestSimplify(string input, string expected)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var simplified = Simplifier.Instance.SimplifyInPlace(expr);
            var resultStr = simplified.ToString();
            bool ok = resultStr.Replace(" ", "") == expected.Replace(" ", "");
            if (ok) { Console.WriteLine($"  PASS SIMPLIFY: {input} = {resultStr}"); _pass++; }
            else { Console.WriteLine($"  FAIL SIMPLIFY: {input} = {resultStr} (expected {expected})"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR SIMPLIFY: {input} -> {ex.Message}"); _fail++; }
    }

    static void TestDiffNumeric(string input, string? expectedSym, double atX, double expectedY)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var diff = new Differentiator().Differentiate(expr, "x");
            var resultStr = diff.ToString();
            double numericResult = Evaluator.Instance.EvaluateToDouble(diff,
                ImmutableDictionary<string, double>.Empty.Add("x", atX));
            bool ok = Math.Abs(numericResult - expectedY) < 1e-6;
            string symCheck = expectedSym is not null ? $" (sym: {resultStr})" : "";
            if (ok) { Console.WriteLine($"  PASS DIFF: d/dx({input}) at x={atX} = {numericResult}{symCheck}"); _pass++; }
            else { Console.WriteLine($"  FAIL DIFF: d/dx({input}) at x={atX} = {numericResult} (expected {expectedY}){symCheck}"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR DIFF: {input} -> {ex.Message}"); _fail++; }
    }

    static void TestIntegNumeric(string input, double lower, double upper, double expected)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var integ = new Integrator().IndefiniteIntegrate(expr, "x");
            var upperVal = Evaluator.Instance.EvaluateToDouble(integ,
                ImmutableDictionary<string, double>.Empty.Add("x", upper));
            var lowerVal = Evaluator.Instance.EvaluateToDouble(integ,
                ImmutableDictionary<string, double>.Empty.Add("x", lower));
            double numericResult = upperVal - lowerVal;
            bool ok = Math.Abs(numericResult - expected) < 1e-6;
            if (ok) { Console.WriteLine($"  PASS INTEG: ∫({input}) from {lower} to {upper} = {numericResult}"); _pass++; }
            else { Console.WriteLine($"  FAIL INTEG: ∫({input}) from {lower} to {upper} = {numericResult} (expected {expected})"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR INTEG: {input} -> {ex.Message}"); _fail++; }
    }

    static void TestPlot(string input, string desc, double expectMin, double expectMax)
    {
        try
        {
            var expr = ParsingFacade.ParseExpression(input);
            var evaluator = Evaluator.Instance;
            var points = new System.Collections.Generic.List<(double x, double y)>();
            double xMin = -10, xMax = 10;
            int samples = 500;
            double step = (xMax - xMin) / samples;
            for (int i = 0; i <= samples; i++)
            {
                double x = xMin + i * step;
                try
                {
                    double y = evaluator.EvaluateToDouble(expr, ImmutableDictionary<string, double>.Empty.Add("x", x));
                    if (!double.IsInfinity(y) && !double.IsNaN(y))
                        points.Add((x, y));
                }
                catch { }
            }
            double actualMin = points.Count > 0 ? points.Min(p => p.y) : double.NaN;
            double actualMax = points.Count > 0 ? points.Max(p => p.y) : double.NaN;
            bool ok = points.Count > 100 &&
                      Math.Abs(actualMin - expectMin) < 0.01 &&
                      Math.Abs(actualMax - expectMax) < 0.01;
            if (ok) { Console.WriteLine($"  PASS PLOT: {input} ({desc}) -> {points.Count} points, y=[{actualMin:F3}, {actualMax:F3}]"); _pass++; }
            else { Console.WriteLine($"  FAIL PLOT: {input} ({desc}) -> {points.Count} points, y=[{actualMin:F3}, {actualMax:F3}] (expected [{expectMin}, {expectMax}])"); _fail++; }
        }
        catch (Exception ex) { Console.WriteLine($"  ERROR PLOT: {input} -> {ex.Message}"); _fail++; }
    }
}
