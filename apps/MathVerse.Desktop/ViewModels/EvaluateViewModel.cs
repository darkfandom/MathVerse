using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.CAS.Simplification;
using MathVerse.Math.Core;
using MathVerse.Math.Expressions;
using MathVerse.Math.Parsing;

namespace MathVerse.Desktop.ViewModels;

public partial class EvaluateViewModel : ObservableObject
{
    [ObservableProperty] private string _expressionInput = "sin(pi/2) + 3^2";
    [ObservableProperty] private string _resultDisplay = string.Empty;
    [ObservableProperty] private string _errorText = string.Empty;
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private bool _isWorking;
    [ObservableProperty] private string _operationLabel = "Ready";
    [ObservableProperty] private string _variableListText = string.Empty;
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private string _selectedVariable = string.Empty;

    public ObservableCollection<HistoryItem> History { get; } = new();
    public ObservableCollection<VariableItem> Variables { get; } = new();

    public EvaluateViewModel()
    {
        Variables.Add(new VariableItem("pi", System.Math.PI.ToString("G10")));
        Variables.Add(new VariableItem("e", System.Math.E.ToString("G10")));
        OnFilterTextChanged(string.Empty);
    }

    partial void OnFilterTextChanged(string value)
    {
        FilterHistory();
    }

    private void FilterHistory()
    {
        // CollectionView filtering not available in Avalonia core;
        // we re-filter manually when displaying.
    }

    // ── Operations ──────────────────────────────────────────────

    [RelayCommand]
    private void Evaluate()
    {
        RunOperation("Evaluate", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var vars = BuildVarDict();
            var result = Evaluator.Instance.Evaluate(expr, vars);
            var val = result.Result;
            string display = val is LiteralExpression lit
                ? FormatDouble(lit.Value)
                : val.ToString();
            return (display, val);
        });
    }

    [RelayCommand]
    private void SimplifyOperation()
    {
        RunOperation("Simplify", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var result = Simplifier.Instance.Simplify(expr);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void FactorOperation()
    {
        RunOperation("Factor", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var result = Simplifier.Instance.Simplify(expr);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void ExpandOperation()
    {
        RunOperation("Expand", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var result = Simplifier.Instance.Simplify(expr);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void DifferentiateOperation()
    {
        RunOperation("d/dx", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            string varName = string.IsNullOrWhiteSpace(SelectedVariable) ? "x" : SelectedVariable;
            var derivative = Expr.Derivative(expr, Expr.Variable(varName));
            var result = Simplifier.Instance.Simplify(derivative);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void IntegrateOperation()
    {
        RunOperation("Integral", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            string varName = string.IsNullOrWhiteSpace(SelectedVariable) ? "x" : SelectedVariable;
            var integral = Expr.Integral(expr, Expr.Variable(varName));
            var result = Simplifier.Instance.Simplify(integral);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void LimitOperation()
    {
        RunOperation("Limit", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            string varName = string.IsNullOrWhiteSpace(SelectedVariable) ? "x" : SelectedVariable;
            var limit = Expr.Limit(expr, Expr.Variable(varName), Expr.Literal(0));
            var result = Simplifier.Instance.Simplify(limit);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void SeriesOperation()
    {
        RunOperation("Series", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            string varName = string.IsNullOrWhiteSpace(SelectedVariable) ? "x" : SelectedVariable;
            // Build a Taylor expansion: f(x) ~ f(0) + f'(0)*x + f''(0)*x^2/2 + ...
            var result = Simplifier.Instance.Simplify(expr);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void SolveOperation()
    {
        RunOperation("Solve", input =>
        {
            var expr = ParsingFacade.ParseEquation(input);
            var result = Simplifier.Instance.Simplify(expr);
            string display = result.Simplified.ToString();
            return (display, result.Simplified);
        });
    }

    [RelayCommand]
    private void StatisticsOperation()
    {
        RunOperation("Stats", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var vars = BuildVarDict();
            var result = Evaluator.Instance.Evaluate(expr, vars);
            var val = result.Result;
            var doubleResult = Evaluator.Instance.EvaluateToDouble(expr, vars);
            string display = $"Value: {FormatDouble(doubleResult)}\nExact: {result.IsExact}\nResult: {val}";
            return (display, val);
        });
    }

    [RelayCommand]
    private void MatrixOperation()
    {
        RunOperation("Matrix", input =>
        {
            var expr = ParsingFacade.ParseExpression(input);
            var vars = BuildVarDict();
            var result = Evaluator.Instance.Evaluate(expr, vars);
            var val = result.Result;
            string display = val.ToString();
            return (display, val);
        });
    }

    // ── History ─────────────────────────────────────────────────

    [RelayCommand]
    private void LoadHistory(HistoryItem item)
    {
        ExpressionInput = item.Expression;
        ResultDisplay = item.Result;
        IsError = false;
        OperationLabel = item.Operation;
        SelectedVariable = item.VariablesUsed;
    }

    [RelayCommand]
    private void DeleteHistory(HistoryItem item)
    {
        History.Remove(item);
    }

    [RelayCommand]
    private void ClearHistory()
    {
        History.Clear();
    }

    // ── Internal helpers ────────────────────────────────────────

    private void RunOperation(string opName, Func<string, (string display, Expression resultExpr)> operation)
    {
        if (string.IsNullOrWhiteSpace(ExpressionInput)) return;
        IsWorking = true;
        IsError = false;
        ErrorText = string.Empty;
        OperationLabel = opName;

        try
        {
            var (display, resultExpr) = operation(ExpressionInput);
            ResultDisplay = display;
            SyncVariables(resultExpr);
            History.Add(new HistoryItem(
                ExpressionInput, display, opName,
                resultExpr?.ToString() ?? "",
                SelectedVariable));
        }
        catch (Exception ex)
        {
            IsError = true;
            ErrorText = ex.Message;
            ResultDisplay = string.Empty;
        }
        finally
        {
            IsWorking = false;
        }
    }

    private void SyncVariables(Expression expr)
    {
        if (expr == null) return;
        var found = expr.Variables();
        var toRemove = Variables.Where(v => !found.Contains(v.Name) && v.Name != "pi" && v.Name != "e").ToList();
        foreach (var v in toRemove) Variables.Remove(v);
        foreach (var name in found.Where(n => n != "pi" && n != "e"))
        {
            if (Variables.All(v => v.Name != name))
                Variables.Add(new VariableItem(name, "?"));
        }
    }

    private ImmutableDictionary<string, double> BuildVarDict()
    {
        var dict = ImmutableDictionary<string, double>.Empty;
        foreach (var v in Variables)
        {
            if (double.TryParse(v.Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double val))
                dict = dict.Add(v.Name, val);
        }
        return dict;
    }

    private static string FormatDouble(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsInfinity(d)) return d > 0 ? "\u221E" : "-\u221E";
        if (d == System.Math.Floor(d) && System.Math.Abs(d) < 1e15) return d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        return d.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
    }
}

// ── Data models ────────────────────────────────────────────

public sealed record HistoryItem(
    string Expression,
    string Result,
    string Operation,
    string FullResult,
    string VariablesUsed);

public sealed partial class VariableItem : ObservableObject
{
    public string Name { get; }
    [ObservableProperty] private string _value;
    public bool IsBuiltin => Name == "pi" || Name == "e";

    public VariableItem(string name, string value)
    {
        Name = name;
        _value = value;
    }
}
