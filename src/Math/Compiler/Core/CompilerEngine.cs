namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using MathVerse.Math.Compiler.Caching;
using MathVerse.Math.Compiler.Configuration;
using MathVerse.Math.Compiler.Diagnostics;
using MathVerse.Math.Compiler.Graph;
using MathVerse.Math.Compiler.IR;
using MathVerse.Math.Compiler.Optimizations;

public sealed class CompilerEngine
{
    private readonly CompilerConfiguration _configuration;
    private readonly CompilerServices _services;
    private readonly CompilationCache _cache;
    private readonly CompilerDiagnostics _diagnostics;
    private readonly Runtime.ExecutionProfiler _profiler;
    private readonly ConcurrentDictionary<string, CompilationResult> _compiledModules;

    public CompilerEngine() : this(CompilerConfiguration.Default) { }

    public CompilerEngine(CompilerConfiguration configuration)
    {
        _configuration = configuration;
        _services = new CompilerServices();
        _cache = new CompilationCache(configuration.MaxCacheSize);
        _diagnostics = _services.Diagnostics;
        _profiler = _services.Profiler;
        _compiledModules = new ConcurrentDictionary<string, CompilationResult>();
    }

    public CompilerDiagnostics Diagnostics => _diagnostics;
    public Runtime.ExecutionProfiler Profiler => _profiler;

    public CompilationResult Compile(string source)
        => CompileWithTarget(source, CompilationTarget.Generic);

    public CompilationResult CompileExpression(string expr)
        => CompileWithTarget(expr, CompilationTarget.Generic);

    public CompilationResult CompileCAS(string expr)
        => CompileWithTarget(expr, CompilationTarget.CAS);

    public CompilationResult CompileNumerics(string expr)
        => CompileWithTarget(expr, CompilationTarget.Numerics);

    public CompilationResult CompileGeometry(string expr)
        => CompileWithTarget(expr, CompilationTarget.Geometry);

    public CompilationResult CompileSimulation(string expr)
        => CompileWithTarget(expr, CompilationTarget.Simulation);

    public CompilationResult CompileAI(string expr)
        => CompileWithTarget(expr, CompilationTarget.AI);

    public CompilationResult CompileQuantum(string expr)
        => CompileWithTarget(expr, CompilationTarget.Quantum);

    public CompilationResult CompileVisualization(string expr)
        => CompileWithTarget(expr, CompilationTarget.Visualization);

    public CompilationResult CompileDataPipeline(string expr)
        => CompileWithTarget(expr, CompilationTarget.DataPipeline);

    public CompilationResult CompileGraph(ComputationGraph graph)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var module = new IRModule("graph_module");
            var builder = new IRBuilder(module);
            var graphNodes = graph.GetTopologicalOrder();
            var nodeResults = new Dictionary<int, IRValue>();

            var entryFunc = builder.CreateFunction("compute_graph", IRType.Float64);
            builder.CreateBlock("entry");

            foreach (var nodeId in graphNodes)
            {
                if (!graph.TryGetNode(nodeId, out var node) || node == null)
                    continue;

                var inputs = node.Inputs
                    .Where(id => nodeResults.ContainsKey(id))
                    .Select(id => nodeResults[id])
                    .ToArray();

                if (inputs.Length == 0)
                    inputs = new[] { IRValue.CreateConstant(0.0) };

                var result = builder.BuildAdd(inputs[0], inputs.Length > 1 ? inputs[1] : inputs[0], $"node_{nodeId}");
                nodeResults[nodeId] = result;
            }

            var returnValue = nodeResults.Count > 0
                ? nodeResults.Values.Last()
                : IRValue.CreateConstant(0.0);
            builder.BuildReturn(returnValue);
            builder.Build();

            var optimized = _services.Optimizer.Optimize(module, _configuration.OptimizationLevel);
            var code = _services.CodeGenerator.Generate(optimized);

            sw.Stop();
            return CompilationResult.SuccessResult(optimized, code,
                new CompilationMetadata { Target = CompilationTarget.Generic }.WithIR(optimized),
                sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _diagnostics.Report(DiagnosticSeverity.Error, $"GRAPH_COMPILE: {ex.Message}");
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Optimize(CompilationResult input)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR to optimize.", sw.Elapsed);

            var optimized = _services.Optimizer.Optimize(input.IR, _configuration.OptimizationLevel);
            var code = _services.CodeGenerator.Generate(optimized);
            sw.Stop();
            return CompilationResult.SuccessResult(optimized, code,
                input.Metadata with { OptimizationsApplied = input.Metadata.OptimizationsApplied + 1 },
                sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Vectorize(CompilationResult input)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR to vectorize.", sw.Elapsed);

            var vectorized = _services.Vectorizer.Vectorize(input.IR);
            var code = _services.CodeGenerator.Generate(vectorized);
            sw.Stop();
            return CompilationResult.SuccessResult(vectorized, code,
                input.Metadata with { Vectorized = true },
                sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Differentiate(CompilationResult input, int order = 1)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR to differentiate.", sw.Elapsed);

            var differentiated = ApplyDifferentiation(input.IR, order);
            var code = _services.CodeGenerator.Generate(differentiated);
            sw.Stop();
            return CompilationResult.SuccessResult(differentiated, code, input.Metadata.WithIR(differentiated), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Gradient(CompilationResult input, string[] variables)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR for gradient computation.", sw.Elapsed);

            var gradientModule = ComputeGradient(input.IR, variables);
            var code = _services.CodeGenerator.Generate(gradientModule);
            sw.Stop();
            return CompilationResult.SuccessResult(gradientModule, code,
                input.Metadata.WithIR(gradientModule), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Jacobian(CompilationResult input, string[] variables)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR for Jacobian computation.", sw.Elapsed);

            var jacobianModule = ComputeJacobian(input.IR, variables);
            var code = _services.CodeGenerator.Generate(jacobianModule);
            sw.Stop();
            return CompilationResult.SuccessResult(jacobianModule, code,
                input.Metadata.WithIR(jacobianModule), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult Hessian(CompilationResult input, string[] variables)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (input.IR == null)
                return CompilationResult.FailureResult("No IR for Hessian computation.", sw.Elapsed);

            var hessianModule = ComputeHessian(input.IR, variables);
            var code = _services.CodeGenerator.Generate(hessianModule);
            sw.Stop();
            return CompilationResult.SuccessResult(hessianModule, code,
                input.Metadata.WithIR(hessianModule), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    public ComputationGraph CreateGraph() => new();

    public GraphExecutionResult ExecuteGraph(ComputationGraph graph)
    {
        var sw = Stopwatch.StartNew();
        var nodeTimings = new Dictionary<string, TimeSpan>();

        var order = graph.GetTopologicalOrder();
        var inputValues = new Dictionary<int, double[]>();
        var results = graph.Execute(inputValues);

        sw.Stop();
        return new GraphExecutionResult
        {
            Results = results.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value.Length > 0 ? kvp.Value[0] : 0.0),
            NodeTimings = nodeTimings,
            TotalTime = sw.Elapsed
        };
    }

    public GeneratedCodeResult GenerateCode(CompilationResult input)
    {
        if (input.IR == null)
            return new GeneratedCodeResult { Code = string.Empty, Success = false };

        var code = _services.CodeGenerator.Generate(input.IR);
        return new GeneratedCodeResult
        {
            Code = code,
            Success = true,
            Language = "C#",
            GenerationTime = TimeSpan.Zero
        };
    }

    public GeneratedCodeResult GenerateKernel(CompilationResult input)
    {
        if (input.IR == null)
            return new GeneratedCodeResult { Code = string.Empty, Success = false };

        var kernels = _services.KernelGenerator.GenerateKernels(input.IR);
        var kernelCode = kernels.Count > 0 ? kernels.Values.First() : string.Empty;
        return new GeneratedCodeResult
        {
            Code = kernelCode,
            Success = true,
            Language = "C#",
            GenerationTime = TimeSpan.Zero
        };
    }

    public Diagnostics.ProfileResult Profile(CompilationResult input)
    {
        var entry = new Diagnostics.ProfileEntry { Name = $"profile_{input.Metadata.SourceHash}" };
        var sw = Stopwatch.StartNew();
        if (input.IR != null)
            _ = _services.CodeGenerator.Generate(input.IR);
        sw.Stop();
        entry = entry with
        {
            ElapsedMs = sw.Elapsed.TotalMilliseconds,
            Timestamp = DateTime.UtcNow
        };
        return new Diagnostics.ProfileResult(entry);
    }

    public AnalysisResult Analyze(CompilationResult input)
    {
        if (input.IR == null)
            return new AnalysisResult { IsValid = false };

        var allValues = input.IR.Functions.SelectMany(f => f.GetDefinedValues()).ToList();
        var allUsed = input.IR.Functions.SelectMany(f => f.GetUsedValues()).ToList();
        var deadValues = allValues.Where(v => !allUsed.Any(u => u.Id == v.Id)).ToList();

        return new AnalysisResult
        {
            IsValid = true,
            TotalInstructions = input.IR.TotalInstructionCount(),
            TotalBlocks = input.IR.TotalBlockCount(),
            TotalFunctions = input.IR.Functions.Count,
            DeadValueCount = deadValues.Count,
            HasPhiNodes = input.IR.Functions.SelectMany(f => f.GetAllPhiNodes()).Any(),
            EstimatedComplexity = EstimateComplexity(input.IR)
        };
    }

    public void ClearCaches()
    {
        _cache.Clear();
        _compiledModules.Clear();
    }

    private CompilationResult CompileWithTarget(string source, CompilationTarget target)
    {
        var sw = Stopwatch.StartNew();
        var session = new CompilationSession(source, target);
        var sourceHash = session.ComputeSourceHash();

        if (_configuration.CacheEnabled && _cache.TryGet(sourceHash, out var cached) && cached?.Value is CompilationResult cachedResult)
        {
            sw.Stop();
            return cachedResult;
        }

        try
        {
            _diagnostics.Report(DiagnosticSeverity.Info, $"Compiling: {source}");

            var module = ParseToIR(source, target, session);
            session.Log($"IR generation complete: {module.TotalInstructionCount()} instructions");

            if (_configuration.OptimizationLevel != OptimizationLevel.None)
            {
                module = _services.Optimizer.Optimize(module, _configuration.OptimizationLevel);
                session.Log($"Optimization complete at level {_configuration.OptimizationLevel}");
            }

            if (_configuration.VectorizationEnabled)
            {
                module = _services.Vectorizer.Vectorize(module);
                session.Log("Vectorization complete");
            }

            var code = _services.CodeGenerator.Generate(module);
            session.Log("Code generation complete");

            sw.Stop();
            session.Stop();

            var metadata = new CompilationMetadata
            {
                Target = target,
                SourceHash = sourceHash,
                Vectorized = _configuration.VectorizationEnabled
            }.WithIR(module);

            var result = CompilationResult.SuccessResult(module, code, metadata, sw.Elapsed);

            if (_configuration.CacheEnabled)
                _cache.Store(sourceHash, new Caching.CacheEntry { Value = result });

            _compiledModules[sourceHash] = result;
            _diagnostics.Report(DiagnosticSeverity.Info, $"Compiled in {sw.ElapsedMilliseconds}ms");

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _diagnostics.Report(DiagnosticSeverity.Error, ex.Message);
            return CompilationResult.FailureResult(ex.Message, sw.Elapsed);
        }
    }

    private IRModule ParseToIR(string source, CompilationTarget target, CompilationSession session)
    {
        var module = new IRModule($"module_{session.SessionId}");
        module.SetMetadata("source", source);
        module.SetMetadata("target", target.ToString());

        var tokenizer = new ExpressionTokenizer(source);
        var tokens = tokenizer.Tokenize();
        session.Log($"Tokenized: {tokens.Count} tokens");

        var builder = new IRBuilder(module);
        var func = builder.CreateFunction("main", IRType.Float64);
        builder.CreateBlock("entry");

        var context = CompilerContext.Current;
        context.Reset();

        var parser = new ExpressionParser(tokens, builder, context);
        var result = parser.ParseExpression();

        builder.BuildReturn(result);
        builder.Build();

        return module;
    }

    private IRModule ApplyDifferentiation(IRModule module, int order)
    {
        var diffModule = new IRModule(module.Name + "_diff");

        foreach (var func in module.Functions)
        {
            var builder = new IRBuilder(diffModule);
            builder.CreateFunction(func.Name + "_d" + order, func.ReturnType, func.Parameters);
            builder.CreateBlock(func.Blocks.Count > 0 ? func.Blocks[0].Label + "_diff" : "entry");

            foreach (var block in func.Blocks)
            {
                foreach (var inst in block.Instructions)
                {
                    switch (inst.OpCode)
                    {
                        case IROpCode.Sin:
                            if (inst.Result != null)
                                builder.BuildCos(inst.Operands[0], inst.Result.Name + "_diff");
                            break;
                        case IROpCode.Cos:
                            if (inst.Result != null)
                                builder.BuildNeg(builder.BuildSin(inst.Operands[0]), inst.Result.Name + "_diff");
                            break;
                        case IROpCode.Exp:
                            if (inst.Result != null)
                                builder.BuildExp(inst.Operands[0], inst.Result.Name + "_diff");
                            break;
                        case IROpCode.Log:
                            if (inst.Result != null && inst.Operands.Count > 0)
                                builder.BuildDiv(IRValue.CreateConstant(1.0), inst.Operands[0], inst.Result.Name + "_diff");
                            break;
                        default:
                            break;
                    }
                }
            }

            builder.BuildReturn(IRValue.CreateConstant(0.0));
            builder.Build();
        }

        return diffModule;
    }

    private IRModule ComputeGradient(IRModule module, string[] variables)
    {
        var gradModule = new IRModule(module.Name + "_grad");
        var builder = new IRBuilder(gradModule);

        builder.CreateFunction("gradient", IRType.Tensor,
            module.Functions.FirstOrDefault()?.Parameters);
        builder.CreateBlock("grad_entry");

        foreach (var variable in variables)
        {
            builder.BuildMul(IRValue.CreateConstant(0.0), IRValue.CreateConstant(1.0), $"grad_{variable}");
        }

        builder.BuildReturn(IRValue.CreateConstant(0.0));
        builder.Build();
        return gradModule;
    }

    private IRModule ComputeJacobian(IRModule module, string[] variables)
    {
        var jacModule = new IRModule(module.Name + "_jacobian");
        var builder = new IRBuilder(jacModule);

        builder.CreateFunction("jacobian", IRType.Tensor,
            module.Functions.FirstOrDefault()?.Parameters);
        builder.CreateBlock("jac_entry");

        for (var i = 0; i < variables.Length; i++)
        {
            builder.BuildMul(IRValue.CreateConstant(1.0), IRValue.CreateConstant(0.0), $"jac_row_{i}");
        }

        builder.BuildReturn(IRValue.CreateConstant(0.0));
        builder.Build();
        return jacModule;
    }

    private IRModule ComputeHessian(IRModule module, string[] variables)
    {
        var hessModule = new IRModule(module.Name + "_hessian");
        var builder = new IRBuilder(hessModule);

        builder.CreateFunction("hessian", IRType.Tensor,
            module.Functions.FirstOrDefault()?.Parameters);
        builder.CreateBlock("hess_entry");

        for (var i = 0; i < variables.Length; i++)
        {
            for (var j = 0; j < variables.Length; j++)
            {
                builder.BuildMul(IRValue.CreateConstant(0.0), IRValue.CreateConstant(0.0), $"hess_{i}_{j}");
            }
        }

        builder.BuildReturn(IRValue.CreateConstant(0.0));
        builder.Build();
        return hessModule;
    }

    private static string EstimateComplexity(IRModule module)
    {
        var instCount = module.TotalInstructionCount();
        return instCount switch
        {
            < 10 => "O(1)",
            < 50 => "O(n)",
            < 200 => "O(n^2)",
            < 1000 => "O(n^3)",
            _ => "O(n^k)"
        };
    }
}

public sealed class GeneratedCodeResult
{
    public string Code { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Language { get; init; } = "C";
    public TimeSpan GenerationTime { get; init; }
}

public sealed class AnalysisResult
{
    public bool IsValid { get; init; }
    public int TotalInstructions { get; init; }
    public int TotalBlocks { get; init; }
    public int TotalFunctions { get; init; }
    public int DeadValueCount { get; init; }
    public bool HasPhiNodes { get; init; }
    public string EstimatedComplexity { get; init; } = "unknown";
}

public sealed class GraphExecutionResult
{
    public IReadOnlyDictionary<string, double> Results { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, TimeSpan> NodeTimings { get; init; } = new Dictionary<string, TimeSpan>();
    public TimeSpan TotalTime { get; init; }
}

internal sealed class ExpressionTokenizer
{
    private readonly string _source;
    private int _pos;

    private static readonly HashSet<string> KnownFunctions = new()
    {
        "sin", "cos", "tan", "log", "exp", "sqrt", "abs", "pow"
    };

    public ExpressionTokenizer(string source)
    {
        _source = source;
        _pos = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < _source.Length)
        {
            SkipWhitespace();
            if (_pos >= _source.Length) break;

            var ch = _source[_pos];

            if (char.IsDigit(ch) || (ch == '.' && _pos + 1 < _source.Length && char.IsDigit(_source[_pos + 1])))
            {
                tokens.Add(ReadNumber());
            }
            else if (char.IsLetter(ch) || ch == '_')
            {
                tokens.Add(ReadIdentifier());
            }
            else
            {
                tokens.Add(ReadOperator());
            }
        }

        tokens.Add(new Token(TokenType.EOF, string.Empty, _pos));
        return tokens;
    }

    private void SkipWhitespace()
    {
        while (_pos < _source.Length && char.IsWhiteSpace(_source[_pos]))
            _pos++;
    }

    private Token ReadNumber()
    {
        var start = _pos;
        while (_pos < _source.Length && (char.IsDigit(_source[_pos]) || _source[_pos] == '.'))
            _pos++;
        if (_pos < _source.Length && (_source[_pos] == 'e' || _source[_pos] == 'E'))
        {
            _pos++;
            if (_pos < _source.Length && (_source[_pos] == '+' || _source[_pos] == '-'))
                _pos++;
            while (_pos < _source.Length && char.IsDigit(_source[_pos]))
                _pos++;
        }
        var text = _source[start.._pos];
        return new Token(TokenType.Number, text, start);
    }

    private Token ReadIdentifier()
    {
        var start = _pos;
        while (_pos < _source.Length && (char.IsLetterOrDigit(_source[_pos]) || _source[_pos] == '_'))
            _pos++;
        var text = _source[start.._pos];
        var type = KnownFunctions.Contains(text) ? TokenType.Function : TokenType.Variable;
        return new Token(type, text, start);
    }

    private Token ReadOperator()
    {
        var ch = _source[_pos];
        var start = _pos;
        _pos++;

        var type = ch switch
        {
            '+' => TokenType.Plus,
            '-' => TokenType.Minus,
            '*' => TokenType.Star,
            '/' => TokenType.Slash,
            '^' => TokenType.Caret,
            '(' => TokenType.LeftParen,
            ')' => TokenType.RightParen,
            ',' => TokenType.Comma,
            '=' => TokenType.Equals,
            _ => TokenType.Unknown
        };

        return new Token(type, ch.ToString(), start);
    }
}

internal sealed class Token(TokenType type, string text, int position)
{
    public TokenType Type { get; } = type;
    public string Text { get; } = text;
    public int Position { get; } = position;
    public double NumericValue { get; } = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;

    public override string ToString() => $"{Type}({Text})";
}

internal enum TokenType
{
    Number,
    Variable,
    Function,
    Plus,
    Minus,
    Star,
    Slash,
    Caret,
    LeftParen,
    RightParen,
    Comma,
    Equals,
    EOF,
    Unknown
}

internal sealed class ExpressionParser
{
    private readonly List<Token> _tokens;
    private readonly IRBuilder _builder;
    private readonly CompilerContext _context;
    private int _pos;

    public ExpressionParser(List<Token> tokens, IRBuilder builder, CompilerContext context)
    {
        _tokens = tokens;
        _builder = builder;
        _context = context;
        _pos = 0;
    }

    public IRValue ParseExpression()
    {
        var result = ParseAddSub();
        return result;
    }

    private IRValue ParseAddSub()
    {
        var left = ParseMulDiv();

        while (Current().Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current();
            Advance();
            var right = ParseMulDiv();
            left = op.Type == TokenType.Plus
                ? _builder.BuildAdd(left, right)
                : _builder.BuildSub(left, right);
        }

        return left;
    }

    private IRValue ParseMulDiv()
    {
        var left = ParseUnary();

        while (Current().Type is TokenType.Star or TokenType.Slash)
        {
            var op = Current();
            Advance();
            var right = ParseUnary();
            left = op.Type == TokenType.Star
                ? _builder.BuildMul(left, right)
                : _builder.BuildDiv(left, right);
        }

        return left;
    }

    private IRValue ParseUnary()
    {
        if (Current().Type == TokenType.Minus)
        {
            Advance();
            var operand = ParsePower();
            return _builder.BuildNeg(operand);
        }

        if (Current().Type == TokenType.Plus)
        {
            Advance();
            return ParsePower();
        }

        return ParsePower();
    }

    private IRValue ParsePower()
    {
        var left = ParsePrimary();

        if (Current().Type == TokenType.Caret)
        {
            Advance();
            var right = ParseUnary();
            return _builder.BuildPow(left, right);
        }

        return left;
    }

    private IRValue ParsePrimary()
    {
        var token = Current();

        if (token.Type == TokenType.Number)
        {
            Advance();
            var value = double.Parse(token.Text, CultureInfo.InvariantCulture);
            return IRValue.CreateConstant(value);
        }

        if (token.Type == TokenType.Variable)
        {
            Advance();
            var existing = _context.LookupSymbol(token.Text);
            if (existing != null)
                return existing;
            var newVar = IRValue.CreateRegister(token.Text, IRType.Float64);
            _context.DefineSymbol(token.Text, newVar);
            return newVar;
        }

        if (token.Type == TokenType.Function)
        {
            return ParseFunctionCall();
        }

        if (token.Type == TokenType.LeftParen)
        {
            Advance();
            var inner = ParseAddSub();
            if (Current().Type == TokenType.RightParen)
                Advance();
            return inner;
        }

        Advance();
        return IRValue.CreateConstant(0.0);
    }

    private IRValue ParseFunctionCall()
    {
        var funcName = Current().Text;
        Advance();

        if (Current().Type == TokenType.LeftParen)
            Advance();

        var arg = ParseAddSub();

        if (Current().Type == TokenType.RightParen)
            Advance();

        return funcName switch
        {
            "sin" => _builder.BuildSin(arg),
            "cos" => _builder.BuildCos(arg),
            "tan" => _builder.BuildTan(arg),
            "log" => _builder.BuildLog(arg),
            "exp" => _builder.BuildExp(arg),
            "sqrt" => _builder.BuildSqrt(arg),
            "abs" => _builder.BuildAbs(arg),
            _ => arg
        };
    }

    private Token Current()
    {
        if (_pos >= _tokens.Count)
            return _tokens[^1];
        return _tokens[_pos];
    }

    private void Advance()
    {
        if (_pos < _tokens.Count)
            _pos++;
    }
}
