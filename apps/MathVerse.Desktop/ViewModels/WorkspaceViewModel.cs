using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathVerse.Math.CAS.Evaluation;
using MathVerse.Math.Core;
using MathVerse.Math.Expressions;
using MathVerse.Math.Parsing;

namespace MathVerse.Desktop.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    public GraphViewModel Graph { get; }
    public ObservableCollection<ConsoleEntry> Console { get; } = new();
    public ObservableCollection<string> ExpressionHistory { get; } = new();

    [ObservableProperty] private string _expressionInput = string.Empty;
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private int _consoleLineCount;

    public WorkspaceViewModel(GraphViewModel graph)
    {
        Graph = graph;
    }

    [RelayCommand]
    private void QuickEvaluate()
    {
        var expr = ExpressionInput?.Trim();
        if (string.IsNullOrEmpty(expr)) return;

        ExpressionHistory.Insert(0, expr);
        if (ExpressionHistory.Count > 50) ExpressionHistory.RemoveAt(ExpressionHistory.Count - 1);

        try
        {
            var parsed = ParsingFacade.ParseExpression(expr);
            var vars = ImmutableDictionary<string, double>.Empty;
            foreach (var v in parsed.Variables())
            {
                if (v == "pi") vars = vars.Add(v, System.Math.PI);
                else if (v == "e") vars = vars.Add(v, System.Math.E);
            }
            var result = Evaluator.Instance.Evaluate(parsed, vars);
            var display = result.Result.ToString();
            Console.Add(new ConsoleEntry(expr, display, false));
            StatusText = display;
        }
        catch (Exception ex)
        {
            Console.Add(new ConsoleEntry(expr, ex.Message, true));
            StatusText = "Error";
        }

        ConsoleLineCount = Console.Count;
    }

    [RelayCommand]
    private void AddGraphFromExpression()
    {
        var expr = ExpressionInput?.Trim();
        if (string.IsNullOrEmpty(expr)) return;

        Graph.NewExpression = expr;
        Graph.SelectedGraphTypeIndex = 0;
        Graph.AddGraphCommand.Execute(null);
        ExpressionInput = string.Empty;
        Console.Add(new ConsoleEntry("graph", $"Added: {expr}", false));
        ConsoleLineCount = Console.Count;
    }

    [RelayCommand]
    private void ClearConsole()
    {
        Console.Clear();
        ConsoleLineCount = 0;
    }

    [RelayCommand]
    private void ClearAll()
    {
        Graph.ClearGraphsCommand.Execute(null);
        Console.Clear();
        ExpressionHistory.Clear();
        ConsoleLineCount = 0;
        ExpressionInput = string.Empty;
        StatusText = "Cleared";
    }
}

public sealed record ConsoleEntry(string Input, string Output, bool IsError);
