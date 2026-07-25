using BenchmarkDotNet.Attributes;
using MathVerse.Math.Parsing;
using MathVerse.Math.Parsing.Diagnostics;
using MathVerse.Math.Parsing.Lexer;
using MathVerse.Math.Parsing.Parser;
using MathVerse.Math.Parsing.Syntax;

namespace MathVerse.Performance.Tests;

/// <summary>
/// Benchmarks for the mathematical expression lexing pipeline.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class LexerBenchmarks
{
    private string _simple = null!;
    private string _medium = null!;
    private string _complex = null!;
    private string _large = null!;

    /// <summary>Sets up benchmark data.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _simple = "1 + 2";
        _medium = "sin(x)^2 + cos(y)^2";
        _complex = "∫_0^1 sin(x^2 + cos(x * 3.14)) dx";
        _large = string.Join(" + ", Enumerable.Range(0, 50).Select(i => $"sin(x_{i}) * cos(y_{i})"));
    }

    /// <summary>Benchmarks lexing a simple expression.</summary>
    [Benchmark(Baseline = true)]
    public Token[] Tokenize_Simple() => Lexer.Tokenize(_simple);

    /// <summary>Benchmarks lexing a medium expression.</summary>
    [Benchmark]
    public Token[] Tokenize_Medium() => Lexer.Tokenize(_medium);

    /// <summary>Benchmarks lexing a complex expression.</summary>
    [Benchmark]
    public Token[] Tokenize_Complex() => Lexer.Tokenize(_complex);

    /// <summary>Benchmarks lexing a large expression.</summary>
    [Benchmark]
    public Token[] Tokenize_Large() => Lexer.Tokenize(_large);
}

/// <summary>
/// Benchmarks for the mathematical expression parsing pipeline.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ParserBenchmarks
{
    private string _simple = null!;
    private string _medium = null!;
    private string _complex = null!;
    private string _large = null!;

    /// <summary>Sets up benchmark data.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _simple = "1 + 2";
        _medium = "sin(x)^2 + cos(y)^2";
        _complex = "∫_0^1 sin(x^2 + cos(x * 3.14)) dx";
        _large = string.Join(" + ", Enumerable.Range(0, 50).Select(i => $"sin(x_{i}) * cos(y_{i})"));
    }

    /// <summary>Benchmarks parsing a simple expression.</summary>
    [Benchmark(Baseline = true)]
    public ParserResult Parse_Simple() => Parser.Parse(_simple);

    /// <summary>Benchmarks parsing a medium expression.</summary>
    [Benchmark]
    public ParserResult Parse_Medium() => Parser.Parse(_medium);

    /// <summary>Benchmarks parsing a complex expression.</summary>
    [Benchmark]
    public ParserResult Parse_Complex() => Parser.Parse(_complex);

    /// <summary>Benchmarks parsing a large expression.</summary>
    [Benchmark]
    public ParserResult Parse_Large() => Parser.Parse(_large);
}

/// <summary>
/// Benchmarks for the full lex→parse→convert pipeline.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class FullPipelineBenchmarks
{
    private string _simple = null!;
    private string _medium = null!;
    private string _complex = null!;

    /// <summary>Sets up benchmark data.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _simple = "1 + 2";
        _medium = "sin(x)^2 + cos(y)^2";
        _complex = "∫_0^1 sin(x^2 + cos(x * 3.14)) dx";
    }

    /// <summary>Benchmarks full pipeline for a simple expression.</summary>
    [Benchmark(Baseline = true)]
    public MathVerse.Math.Expressions.Expression Pipeline_Simple() =>
        ParsingFacade.ParseExpression(_simple);

    /// <summary>Benchmarks full pipeline for a medium expression.</summary>
    [Benchmark]
    public MathVerse.Math.Expressions.Expression Pipeline_Medium() =>
        ParsingFacade.ParseExpression(_medium);

    /// <summary>Benchmarks full pipeline for a complex expression.</summary>
    [Benchmark]
    public MathVerse.Math.Expressions.Expression Pipeline_Complex() =>
        ParsingFacade.ParseExpression(_complex);
}

/// <summary>
/// Benchmarks for deep nesting and syntax tree traversal.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class SyntaxTreeBenchmarks
{
    private string _deepNesting = null!;
    private string _wideTree = null!;

    /// <summary>Sets up benchmark data.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _deepNesting = "(" + string.Join("", Enumerable.Range(0, 20).Select(_ => "(")) + "1" + string.Join("", Enumerable.Range(0, 20).Select(_ => ")"));
        _wideTree = string.Join(" + ", Enumerable.Range(0, 100).Select(i => i.ToString()));
    }

    /// <summary>Benchmarks parsing deeply nested expressions.</summary>
    [Benchmark(Baseline = true)]
    public SyntaxTree Parse_DeepNesting() => ParsingFacade.ParseSyntaxTree(_deepNesting);

    /// <summary>Benchmarks parsing wide/flat expressions.</summary>
    [Benchmark]
    public SyntaxTree Parse_WideTree() => ParsingFacade.ParseSyntaxTree(_wideTree);
}

/// <summary>
/// Benchmarks for diagnostic creation overhead.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class DiagnosticBenchmarks
{
    private DiagnosticBag _bag = null!;

    /// <summary>Sets up benchmark data.</summary>
    [GlobalSetup]
    public void Setup() => _bag = new DiagnosticBag();

    /// <summary>Benchmarks adding a single diagnostic.</summary>
    [Benchmark(Baseline = true)]
    public void AddSingle_Diagnostic()
    {
        _bag.Clear();
        _bag.AddError("MV0001", "Test error", 1, 1, 5);
    }

    /// <summary>Benchmarks adding 100 diagnostics.</summary>
    [Benchmark]
    public void Add100_Diagnostics()
    {
        _bag.Clear();
        for (var i = 0; i < 100; i++)
            _bag.AddError("MV0001", $"Error {i}", i, 1, 5);
    }

    /// <summary>Benchmarks retrieving all diagnostics.</summary>
    [Benchmark]
    public Diagnostic[] GetAll_Diagnostics()
    {
        _bag.Clear();
        for (var i = 0; i < 50; i++)
            _bag.AddError("MV0001", $"Error {i}", i, 1, 5);
        return _bag.GetAll();
    }
}
