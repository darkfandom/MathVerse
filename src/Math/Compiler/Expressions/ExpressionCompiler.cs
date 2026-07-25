namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>Compiles string expressions into AST (ExpressionNode) and optionally into IRModule.</summary>
public sealed class ExpressionCompiler
{
    private static readonly HashSet<string> KnownFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "asin", "acos", "atan",
        "log", "ln", "exp", "sqrt", "abs", "ceil", "floor",
    };

    private string _input = string.Empty;
    private int _pos;
    private readonly List<ExpressionToken> _tokens = [];
    private int _tokenIndex;

    /// <summary>Compiles a mathematical expression string into an AST.</summary>
    public ExpressionNode Compile(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression must not be null or empty.", nameof(expression));

        _input = expression;
        _pos = 0;
        _tokens.Clear();
        _tokenIndex = 0;

        Tokenize();
        var ast = ParseExpression();

        if (_tokenIndex < _tokens.Count)
            throw new FormatException($"Unexpected token '{_tokens[_tokenIndex].Value}' at position {_tokens[_tokenIndex].Position}.");

        return ast;
    }

    /// <summary>Compiles a mathematical expression string into an IRModule.</summary>
    public IR.IRModule CompileToIR(string expression)
    {
        var ast = Compile(expression);
        var lowering = new ExpressionLowering();
        return lowering.Lower(ast);
    }

    /// <summary>Compiles and optimizes a mathematical expression string into an IRModule.</summary>
    public IR.IRModule CompileOptimized(string expression)
    {
        var ast = Compile(expression);
        var optimizer = new ExpressionOptimizer();
        var optimized = optimizer.Optimize(ast);
        var lowering = new ExpressionLowering();
        return lowering.Lower(optimized);
    }

    private void Tokenize()
    {
        while (_pos < _input.Length)
        {
            char c = _input[_pos];

            if (char.IsWhiteSpace(c))
            {
                _pos++;
                continue;
            }

            if (char.IsDigit(c) || (c == '.' && _pos + 1 < _input.Length && char.IsDigit(_input[_pos + 1])))
            {
                TokenizeNumber();
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                TokenizeIdentifier();
                continue;
            }

            switch (c)
            {
                case '+':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.Plus, "+", _pos));
                    _pos++;
                    break;
                case '-':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.Minus, "-", _pos));
                    _pos++;
                    break;
                case '*':
                    if (_pos + 1 < _input.Length && _input[_pos + 1] == '*')
                    {
                        _tokens.Add(new ExpressionToken(ExpressionTokenKind.Power, "^", _pos));
                        _pos += 2;
                    }
                    else
                    {
                        _tokens.Add(new ExpressionToken(ExpressionTokenKind.Star, "*", _pos));
                        _pos++;
                    }
                    break;
                case '/':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.Slash, "/", _pos));
                    _pos++;
                    break;
                case '^':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.Power, "^", _pos));
                    _pos++;
                    break;
                case '(':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.LeftParen, "(", _pos));
                    _pos++;
                    break;
                case ')':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.RightParen, ")", _pos));
                    _pos++;
                    break;
                case ',':
                    _tokens.Add(new ExpressionToken(ExpressionTokenKind.Comma, ",", _pos));
                    _pos++;
                    break;
                default:
                    throw new FormatException($"Unexpected character '{c}' at position {_pos}.");
            }
        }

        _tokens.Add(new ExpressionToken(ExpressionTokenKind.EndOfInput, "", _pos));
    }

    private void TokenizeNumber()
    {
        int start = _pos;
        while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '.'))
            _pos++;

        if (_pos < _input.Length && (_input[_pos] == 'e' || _input[_pos] == 'E'))
        {
            _pos++;
            if (_pos < _input.Length && (_input[_pos] == '+' || _input[_pos] == '-'))
                _pos++;
            while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                _pos++;
        }

        string numStr = _input[start.._pos];
        if (!double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            throw new FormatException($"Invalid number '{numStr}' at position {start}.");

        _tokens.Add(new ExpressionToken(ExpressionTokenKind.Number, numStr, start));
    }

    private void TokenizeIdentifier()
    {
        int start = _pos;
        while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
            _pos++;

        string name = _input[start.._pos];

        if (KnownFunctions.Contains(name))
            _tokens.Add(new ExpressionToken(ExpressionTokenKind.Function, name, start));
        else
            _tokens.Add(new ExpressionToken(ExpressionTokenKind.Identifier, name, start));
    }

    private ExpressionNode ParseExpression() => ParseTerm();

    private ExpressionNode ParseTerm()
    {
        var left = ParseFactor();

        while (_tokenIndex < _tokens.Count)
        {
            var token = _tokens[_tokenIndex];
            if (token.Kind is ExpressionTokenKind.Plus or ExpressionTokenKind.Minus)
            {
                _tokenIndex++;
                var right = ParseFactor();
                var op = token.Kind == ExpressionTokenKind.Plus ? BinaryOperator.Add : BinaryOperator.Subtract;
                left = new BinaryOpNode(left, op, right);
            }
            else
                break;
        }

        return left;
    }

    private ExpressionNode ParseFactor()
    {
        var left = ParseUnary();

        while (_tokenIndex < _tokens.Count)
        {
            var token = _tokens[_tokenIndex];
            if (token.Kind is ExpressionTokenKind.Star or ExpressionTokenKind.Slash)
            {
                _tokenIndex++;
                var right = ParseUnary();
                var op = token.Kind == ExpressionTokenKind.Star ? BinaryOperator.Multiply : BinaryOperator.Divide;
                left = new BinaryOpNode(left, op, right);
            }
            else
                break;
        }

        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (_tokenIndex < _tokens.Count)
        {
            var token = _tokens[_tokenIndex];
            if (token.Kind == ExpressionTokenKind.Minus)
            {
                _tokenIndex++;
                var operand = ParsePower();
                return new UnaryOpNode(UnaryOperator.Negate, operand);
            }
            if (token.Kind == ExpressionTokenKind.Plus)
            {
                _tokenIndex++;
                var operand = ParsePower();
                return new UnaryOpNode(UnaryOperator.Positive, operand);
            }
        }

        return ParsePower();
    }

    private ExpressionNode ParsePower()
    {
        var baseExpr = ParsePrimary();

        if (_tokenIndex < _tokens.Count && _tokens[_tokenIndex].Kind == ExpressionTokenKind.Power)
        {
            _tokenIndex++;
            var exponent = ParseUnary();
            return new BinaryOpNode(baseExpr, BinaryOperator.Power, exponent);
        }

        return baseExpr;
    }

    private ExpressionNode ParsePrimary()
    {
        if (_tokenIndex >= _tokens.Count)
            throw new FormatException("Unexpected end of expression.");

        var token = _tokens[_tokenIndex];

        switch (token.Kind)
        {
            case ExpressionTokenKind.Number:
                _tokenIndex++;
                double value = double.Parse(token.Value, CultureInfo.InvariantCulture);
                return new NumberNode(value);

            case ExpressionTokenKind.Identifier:
                _tokenIndex++;
                return new VariableNode(token.Value);

            case ExpressionTokenKind.Function:
                return ParseFunctionCall();

            case ExpressionTokenKind.LeftParen:
                _tokenIndex++;
                var expr = ParseExpression();
                if (_tokenIndex < _tokens.Count && _tokens[_tokenIndex].Kind == ExpressionTokenKind.RightParen)
                    _tokenIndex++;
                else
                    throw new FormatException($"Expected ')' at position {_pos}.");
                return expr;

            default:
                throw new FormatException($"Unexpected token '{token.Value}' at position {token.Position}.");
        }
    }

    private ExpressionNode ParseFunctionCall()
    {
        var funcToken = _tokens[_tokenIndex];
        string funcName = funcToken.Value;
        _tokenIndex++;

        if (_tokenIndex >= _tokens.Count || _tokens[_tokenIndex].Kind != ExpressionTokenKind.LeftParen)
            throw new FormatException($"Expected '(' after function name '{funcName}' at position {funcToken.Position}.");

        _tokenIndex++;
        var args = new List<ExpressionNode>();

        if (_tokenIndex < _tokens.Count && _tokens[_tokenIndex].Kind != ExpressionTokenKind.RightParen)
        {
            args.Add(ParseExpression());

            while (_tokenIndex < _tokens.Count && _tokens[_tokenIndex].Kind == ExpressionTokenKind.Comma)
            {
                _tokenIndex++;
                args.Add(ParseExpression());
            }
        }

        if (_tokenIndex < _tokens.Count && _tokens[_tokenIndex].Kind == ExpressionTokenKind.RightParen)
            _tokenIndex++;
        else
            throw new FormatException($"Expected ')' after function arguments at position {_pos}.");

        return new FunctionNode(funcName, args);
    }
}

/// <summary>Represents a lexical token from an expression string.</summary>
internal sealed record ExpressionToken(ExpressionTokenKind Kind, string Value, int Position);

/// <summary>Enumerates the kinds of lexical tokens in mathematical expressions.</summary>
internal enum ExpressionTokenKind
{
    Number,
    Identifier,
    Function,
    Plus,
    Minus,
    Star,
    Slash,
    Power,
    LeftParen,
    RightParen,
    Comma,
    EndOfInput,
}
